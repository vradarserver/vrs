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
    public class BackgroundThreadQueue<T> : IQueue, IDisposable
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
        /// In ThreadPool mode this can be called in parallel, in all other modes it's called in series.
        /// </summary>
        private Action<T> _ProcessObject;

        /// <summary>
        /// The delegate that is called on the background thread when it catches an exception. In ThreadPool
        /// mode this can be called in parallel, in all other modes it is called in series.
        /// </summary>
        private Action<Exception> _ProcessException;

        /// <summary>
        /// The mechanism that the queue uses to process items.
        /// </summary>
        private BackgroundThreadQueueMechanism _Mechanism;

        /// <summary>
        /// If true then something has called Stop(). Once a background thread queue has been stopped it cannot
        /// be restarted. While it is stopped calls to enqueue items silently fail, and the background thread
        /// worker will stop processing items after it has finished processing the current item.
        /// </summary>
        private bool _Stopped;

        /// <summary>
        /// The semaphore used by the thread pool mechanism to control how many parallel tasks to queue up simultaneously.
        /// </summary>
        private Semaphore _ParallelThreadSemaphore;

        /// <summary>
        /// The maximum number of parallel threads to allow. This needs to be established before <see cref="StartBackgroundThread"/>
        /// is called because it controls the creation of <see cref="_ParallelThreadSemaphore"/>.
        /// </summary>
        private int _MaximumParallelThreads = -1;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Name { get { return _QueueName; } }

        /// <summary>
        /// Gets or sets the maximum number of parallel threads used when the mechanism is <see cref="BackgroundThreadQueueMechanism.ThreadPool"/>.
        /// Has no effect with other mechanisms, or if the queue has already been started.
        /// </summary>
        public int MaximumParallelThreads
        {
            get { return _MaximumParallelThreads; }
            set {
                if(_BackgroundThread == null) {
                    _MaximumParallelThreads = value;
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int CountQueuedItems
        {
            get {
                var result = 0;
                if(_Mechanism != BackgroundThreadQueueMechanism.SingleThread && _BackgroundThread != null) {
                    lock(_SyncLock) {
                        result = _Queue.Count;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int PeakQueuedItems { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long CountDroppedItems { get; private set; }

        /// <summary>
        /// The maximum number of items allowed in the queue.
        /// </summary>
        public int MaxQueuedItems { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="queueName"></param>
        public BackgroundThreadQueue(string queueName, int maxQueuedItems = int.MaxValue) : this(queueName, BackgroundThreadQueueMechanism.Queue, maxQueuedItems)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="mechanism"></param>
        /// <param name="maxQueuedItems"></param>
        public BackgroundThreadQueue(string queueName, BackgroundThreadQueueMechanism mechanism, int maxQueuedItems = int.MaxValue)
        {
            var runtimeEnvironment = Factory.Singleton.Resolve<IRuntimeEnvironment>().Singleton;

            if(mechanism == BackgroundThreadQueueMechanism.QueueWithNoBlock) {
                if(runtimeEnvironment.IsMono) mechanism = BackgroundThreadQueueMechanism.Queue;
            }
            if(runtimeEnvironment.IsTest) {
                mechanism = BackgroundThreadQueueMechanism.SingleThread;
            }

            _QueueName = queueName;
            _Mechanism = mechanism;
            MaxQueuedItems = maxQueuedItems;

            QueueRepository.AddQueue(this);
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
                QueueRepository.RemoveQueue(this);

                if(_BackgroundThread != null) {
                    _BackgroundThread.Abort();
                    _BackgroundThread = null;
                }
            }
        }

        /// <summary>
        /// Creates and starts the background thread that will pop objects from the queue and process them.
        /// </summary>
        /// <param name="processObject">
        /// A delegate that will be called on a background thread with each object popped from the queue. In ThreadPool mode this can be
        /// called in parallel, in all other modes it is called in series.
        /// </param>
        /// <param name="processException">
        /// A delegate called on a background thread with the details of an exception that was caught during processObject's execution.
        /// In ThreadPool mode this can be called in parallel, in all other modes it is called in series.
        /// </param>
        public void StartBackgroundThread(Action<T> processObject, Action<Exception> processException)
        {
            if(processObject == null) throw new ArgumentNullException("processObject");
            if(processException == null) throw new ArgumentNullException("processException");
            if(_ProcessObject != null) throw new InvalidOperationException("StartBackgroundThread cannot be called twice");
            if(String.IsNullOrEmpty(_QueueName)) throw new InvalidOperationException("A valid queue name must be supplied to the constructor");

            _ProcessObject = processObject;
            _ProcessException = processException;

            switch(_Mechanism) {
                case BackgroundThreadQueueMechanism.Queue:
                case BackgroundThreadQueueMechanism.QueueWithNoBlock:
                    _BackgroundThread = new Thread(BackgroundThreadMethod) {
                        Name = _QueueName,
                        IsBackground = true,
                    };
                    _BackgroundThread.Start();
                    break;
                case BackgroundThreadQueueMechanism.SingleThread:
                    break;
                case BackgroundThreadQueueMechanism.ThreadPool:
                    if(_MaximumParallelThreads > 0) {
                        _ParallelThreadSemaphore = new Semaphore(_MaximumParallelThreads, _MaximumParallelThreads);
                    }
                    goto case BackgroundThreadQueueMechanism.Queue;
                default:
                    throw new NotImplementedException();
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
            try {
                var applicationInformation = Factory.Singleton.Resolve<IApplicationInformation>();
                if(applicationInformation.CultureInfo != null) {
                    Thread.CurrentThread.CurrentCulture = applicationInformation.CultureInfo;
                    Thread.CurrentThread.CurrentUICulture = applicationInformation.CultureInfo;
                }

                while(!_Stopped) {
                    try {
                        T itemFromQueue = null;
                        lock(_SyncLock) {
                            if(_Queue.Count > 0) {
                                itemFromQueue = _Queue.Dequeue();
                            }
                        }

                        if(itemFromQueue != null) {
                            switch(_Mechanism) {
                                case BackgroundThreadQueueMechanism.Queue:
                                case BackgroundThreadQueueMechanism.QueueWithNoBlock:
                                    _ProcessObject(itemFromQueue);
                                    break;
                                case BackgroundThreadQueueMechanism.ThreadPool:
                                    EnqueueCallToProcessObjectToThreadPool(itemFromQueue);
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                        } else {
                            switch(_Mechanism) {
                                case BackgroundThreadQueueMechanism.Queue:              _Signal.WaitOne(); break;
                                case BackgroundThreadQueueMechanism.QueueWithNoBlock:   Thread.Sleep(1); break;
                                case BackgroundThreadQueueMechanism.ThreadPool:         _Signal.WaitOne(); break;
                                default:                                                throw new NotImplementedException();
                            }
                        }
                    } catch(Exception ex) {
                        if(!(ex is ThreadAbortException)) {
                            _ProcessException(ex);
                        }
                    }
                }
            } catch(ThreadAbortException) {
            } catch(Exception ex) {
                try {
                    var log = Factory.Singleton.Resolve<ILog>().Singleton;
                    log.WriteLine("Caught exception in BackgroundThreadMethod for queue {0}: {1}", _QueueName, ex);
                } catch {
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
                switch(_Mechanism) {
                    case BackgroundThreadQueueMechanism.Queue:
                    case BackgroundThreadQueueMechanism.QueueWithNoBlock:
                    case BackgroundThreadQueueMechanism.ThreadPool:
                        if(_BackgroundThread != null) {
                            lock(_SyncLock) {
                                _Queue.Enqueue(item);
                                if(MaxQueuedItems < int.MaxValue) DropExcessItems();
                                if(_Queue.Count > PeakQueuedItems) PeakQueuedItems = _Queue.Count;
                            }

                            if(_Mechanism != BackgroundThreadQueueMechanism.QueueWithNoBlock) {
                                _Signal.Set();
                            }
                        }
                        break;
                    case BackgroundThreadQueueMechanism.SingleThread:
                        ProcessItemInSingleThreadMode(item);
                        break;
                    default:
                        throw new NotImplementedException();
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
                switch(_Mechanism) {
                    case BackgroundThreadQueueMechanism.Queue:
                    case BackgroundThreadQueueMechanism.QueueWithNoBlock:
                    case BackgroundThreadQueueMechanism.ThreadPool:
                        lock(_SyncLock) {
                            foreach(var item in items) {
                                _Queue.Enqueue(item);
                            }
                            if(MaxQueuedItems < int.MaxValue) DropExcessItems();
                            if(_Queue.Count > PeakQueuedItems) PeakQueuedItems = _Queue.Count;
                        }

                        if(_Mechanism != BackgroundThreadQueueMechanism.QueueWithNoBlock) {
                            _Signal.Set();
                        }
                        break;
                    case BackgroundThreadQueueMechanism.SingleThread:
                        foreach(var item in items) {
                            ProcessItemInSingleThreadMode(item);
                        }
                        break;
                    default:
                        throw new NotImplementedException();
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
        /// Drops items from the head of the queue until the number of queued items is in line
        /// with the maximum allowed.
        /// </summary>
        private void DropExcessItems()
        {
            if(_Queue.Count > MaxQueuedItems) {
                var removeCount = _Queue.Count - MaxQueuedItems;
                for(var i = 0;i < removeCount;++i) {
                    _Queue.Dequeue();
                }
                CountDroppedItems += removeCount;
            }
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

        /// <summary>
        /// Queues up a call to _ProcessObject via the ThreadPool.
        /// </summary>
        /// <param name="itemFromQueue"></param>
        private void EnqueueCallToProcessObjectToThreadPool(T itemFromQueue)
        {
            if(_ParallelThreadSemaphore != null) {
                _ParallelThreadSemaphore.WaitOne();
            }
            ThreadPool.QueueUserWorkItem(CallProcessObjectFromThreadPool, itemFromQueue);
        }

        /// <summary>
        /// Calls _ProcessObject from a ThreadPool thread.
        /// </summary>
        /// <param name="state"></param>
        private void CallProcessObjectFromThreadPool(object state)
        {
            try {
                try {
                    if(!_Stopped) {
                        _ProcessObject(state as T);
                    }
                } catch(ThreadAbortException) {
                } catch(Exception ex) {
                    _ProcessException(ex);
                } finally {
                    if(_ParallelThreadSemaphore != null) {
                        _ParallelThreadSemaphore.Release();
                    }
                }
            } catch(ThreadAbortException) {
            } catch(Exception ex) {
                try {
                    var log = Factory.Singleton.Resolve<ILog>().Singleton;
                    log.WriteLine("Caught exception in CallProcessObjectFromThreadPool for queue {0}: {1}", _QueueName, ex);
                } catch {
                }
            }
        }
    }
}
