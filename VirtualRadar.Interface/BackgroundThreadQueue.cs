// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using InterfaceFactory;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// A class that wraps a queue of objects that one thread pushes onto while a background thread pops and processes them.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>This can be told to force everything onto a single thread by unit tests via the <see cref="IRuntimeEnvironment"/> interface.</remarks>
    public class BackgroundThreadQueue<T> : IDisposable
        where T: class
    {
        /// <summary>
        /// The object used to synchronise access to the queue.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// The queue of objects that we'll be maintaining.
        /// </summary>
        private Queue<T> _Queue = new Queue<T>();

        // <summary>
        // The signal that will be set when new entries are added to the queue.
        // </summary>
        // This is what I originally used to coordinate between the two threads - items would be pushed
        // onto the queue and then this signal was set from one thread while the background thread blocked
        // on it. However in profiling I found that the Set call is *VERY* expensive, when doing a torture
        // test of 25 hours of network packets all sent consecutively the Set call accounted for a third
        // of the packet processing time. So now the foreground thread queues the items and the background
        // thread spins, polling the queue and immediately surrendering its timeslice when the queue is
        // empty. In testing this had no discernable affect on CPU usage and improved the throughput of
        // messages enormously, previously it would take ~1 minute 40 seconds to process 113Mb of network
        // packets, after switching to the spinning & polling mechanism this was reduced to ~45 seconds.
        // 
        // However in some situations, particularly for queues where little work is done, it makes more
        // sense to use the signal rather than leaving the thread spinning.
        private EventWaitHandle _Signal = new EventWaitHandle(false, EventResetMode.AutoReset);

        /// <summary>
        /// The name that will be used in diagnostic messages.
        /// </summary>
        private readonly string _QueueName;

        /// <summary>
        /// The thread that will be dedicated to popping items from the queue and processing them.
        /// </summary>
        private Thread _BackgroundThread;

        /// <summary>
        /// The delegate called on the background thread with the object that has been popped from the queue.
        /// </summary>
        private Action<T> _ProcessObject;

        /// <summary>
        /// The delegate that is called on the background thread when it catches an exception.
        /// </summary>
        private Action<Exception> _ProcessException;

        /// <summary>
        /// If true then all multi-threading is disabled and items are dequeued as soon as they are queued.
        /// </summary>
        private bool _ForceOntoSingleThread;

        /// <summary>
        /// If true then the thread surrenders its time slice when there is no work to do. If false then it
        /// waits on _Signal, which the push methods set.
        /// </summary>
        private bool _SurrenderTimeSlice;

        /// <summary>
        /// If true then something has called Stop(). Once a background thread queue has been stopped it cannot
        /// be restarted. While it is stopped calls to enqueue items silently fail, and the background thread
        /// worker will stop processing items after it has finished processing the current item.
        /// </summary>
        private bool _Stopped;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="queueName"></param>
        public BackgroundThreadQueue(string queueName) : this(queueName, surrenderTimeSliceOnEmptyQueue: true)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="surrenderTimeSliceOnEmptyQueue"></param>
        public BackgroundThreadQueue(string queueName, bool surrenderTimeSliceOnEmptyQueue)
        {
            var runtimeEnvironment = Factory.Singleton.Resolve<IRuntimeEnvironment>().Singleton;
            if(runtimeEnvironment.IsMono) surrenderTimeSliceOnEmptyQueue = false;

            _QueueName = queueName;
            _ForceOntoSingleThread = Factory.Singleton.Resolve<IRuntimeEnvironment>().Singleton.IsTest;
            _SurrenderTimeSlice = surrenderTimeSliceOnEmptyQueue;
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~BackgroundThreadQueue()
        {
            Dispose(false);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                if(_BackgroundThread != null) {
                    _BackgroundThread.Abort();
                    _BackgroundThread = null;
                }
            }
        }

        /// <summary>
        /// Creates and starts the background thread that will pop objects from the queue and process them.
        /// </summary>
        /// <param name="processObject">A delegate that will be called on a background thread with each object popped from the queue.</param>
        /// <param name="processException">A delegate called on a background thread with the details of an exception that was caught during processObject's execution.</param>
        public void StartBackgroundThread(Action<T> processObject, Action<Exception> processException)
        {
            if(processObject == null) throw new ArgumentNullException("processObject");
            if(processException == null) throw new ArgumentNullException("processException");
            if(_ProcessObject != null) throw new InvalidOperationException("StartBackgroundThread cannot be called twice");
            if(String.IsNullOrEmpty(_QueueName)) throw new InvalidOperationException("A valid queue name must be supplied to the constructor");

            _ProcessObject = processObject;
            _ProcessException = processException;

            if(!_ForceOntoSingleThread) {
                _BackgroundThread = new Thread(BackgroundThreadMethod) { Name = _QueueName };
                _BackgroundThread.Start();
            }
        }

        /// <summary>
        /// Stops processing items. Once the queue has stopped it cannot be restarted.
        /// </summary>
        public void Stop()
        {
            _Stopped = true;
            Clear();
            _BackgroundThread = null;
        }

        /// <summary>
        /// Called on the background thread.
        /// </summary>
        private void BackgroundThreadMethod()
        {
            var applicationInformation = Factory.Singleton.Resolve<IApplicationInformation>();
            if(applicationInformation.CultureInfo != null) {
                Thread.CurrentThread.CurrentCulture = applicationInformation.CultureInfo;
                Thread.CurrentThread.CurrentUICulture = applicationInformation.CultureInfo;
            }

            while(!_Stopped) {
                try {
                    T itemFromQueue;
                    lock(_SyncLock) {
                        itemFromQueue = _Queue.Count == 0 ? null : _Queue.Dequeue();
                    }

                    if(itemFromQueue != null) _ProcessObject(itemFromQueue);
                    else {
                        if(_SurrenderTimeSlice) Thread.Sleep(1);
                        else                    _Signal.WaitOne();
                    }
                } catch(Exception ex) {
                    if(!(ex is ThreadAbortException)) {
                        _ProcessException(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a new item to the queue and signals the background thread that there is something to process.
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(T item)
        {
            if(!_Stopped) {
                if(_ForceOntoSingleThread) ProcessItemInSingleThreadMode(item);
                else {
                    if(_BackgroundThread != null) {
                        lock(_SyncLock) {
                            _Queue.Enqueue(item);
                        }
                        if(!_SurrenderTimeSlice) _Signal.Set();
                    }
                }
            }
        }

        /// <summary>
        /// Adds many items to the queue simultaneously.
        /// </summary>
        /// <param name="items"></param>
        public void EnqueueRange(IEnumerable<T> items)
        {
            if(!_Stopped) {
                if(_ForceOntoSingleThread) {
                    foreach(var item in items) {
                        ProcessItemInSingleThreadMode(item);
                    }
                } else if(_BackgroundThread != null) {
                    lock(_SyncLock) {
                        foreach(var item in items) {
                            _Queue.Enqueue(item);
                        }
                    }

                    if(!_SurrenderTimeSlice) _Signal.Set();
                }
            }
        }

        /// <summary>
        /// Removes all entries from the queue in a single operation.
        /// </summary>
        public void Clear()
        {
            if(_BackgroundThread != null) {
                lock(_SyncLock) {
                    _Queue.Clear();
                }
            }
        }

        /// <summary>
        /// Returns the number of entries in the queue.
        /// </summary>
        /// <returns></returns>
        public int GetQueueLength()
        {
            var result = 0;
            if(_BackgroundThread != null) {
                lock(_SyncLock) {
                    result = _Queue.Count;
                }
            }

            return result;
        }

        /// <summary>
        /// Processes queued items when running in single-threading mode.
        /// </summary>
        /// <param name="item"></param>
        private void ProcessItemInSingleThreadMode(T item)
        {
            try {
                if(item != null) _ProcessObject(item);
            } catch(Exception ex) {
                if(!(ex is ThreadAbortException)) {
                    _ProcessException(ex);
                }
            }
        }
    }
}
