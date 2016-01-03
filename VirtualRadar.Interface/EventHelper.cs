// Copyright © 2015 onwards, Andrew Whewell
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
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Methods that help when handling events.
    /// </summary>
    public static class EventHelper
    {
        /// <summary>
        /// A class that can report on the number of cached delegates in the queue diagnostic display.
        /// </summary>
        class QueueReporter : IQueue
        {
            public string Name
            {
                get { return "EventHelper-CachedDelegates"; }
            }

            public int CountQueuedItems
            {
                get { return 0; }
            }

            public int PeakQueuedItems
            {
                get { lock(EventHelper._CachedMethodsLock) { return EventHelper._CachedMethods.Count; } }
            }
        }

        /// <summary>
        /// The object that shows the number of cached methods in the queues diagnostic display.
        /// </summary>
        private static QueueReporter _QueueReporter;

        /// <summary>
        /// A dictionary of pre-compiled delegates.
        /// </summary>
        private static Dictionary<MethodInfo, object> _CachedMethods = new Dictionary<MethodInfo,object>();

        /// <summary>
        /// A lock on the _CachedMethods dictionary.
        /// </summary>
        private static object _CachedMethodsLock = new object();

        /// <summary>
        /// Raises an event for an event handler. This is 6x to 9x faster than the Raise methods but, unlike those methods, if one of
        /// the event handlers throws an exception then no further handlers are called.
        /// </summary>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <param name="eventHandler"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public static void RaiseQuickly<TEventArgs>(EventHandler<TEventArgs> eventHandler, object sender, TEventArgs args)
            where TEventArgs: EventArgs
        {
            if(eventHandler != null) {
                eventHandler(sender, args);
            }
        }

        /// <summary>
        /// Raises an event for an event handler. This is 6x to 9x faster than the Raise methods but, unlike those methods, if one of
        /// the event handlers throws an exception then no further handlers are called.
        /// </summary>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <param name="eventHandler"></param>
        /// <param name="sender"></param>
        /// <param name="buildArgsCallback"></param>
        public static void RaiseQuickly<TEventArgs>(EventHandler<TEventArgs> eventHandler, object sender, Func<TEventArgs> buildArgsCallback)
            where TEventArgs: EventArgs
        {
            if(eventHandler != null) {
                eventHandler(sender, buildArgsCallback());
            }
        }

        /// <summary>
        /// Raises an event for an event handler.
        /// </summary>
        /// <param name="eventHandler"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <param name="exceptionCallback">
        /// Called whenever an exception is raised. If this is supplied then <paramref name="throwEventHelperException"/> has no effect,
        /// all handlers will be called and this function won't let exceptions thrown by the handlers bubble (unless the callback
        /// throws an exception, in which case an EventHelperException that wraps the exception(s) will be thrown).</param>
        /// <param name="throwEventHelperException">
        /// True if exceptions thrown by event handlers should be wrapped into a single EventHelperException, false if the first
        /// exception thrown by an event handler should be rethrown. If this is false then the first event handler that throws
        /// an exception will stop all other event handlers from being called.
        /// </param>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <remarks>
        /// Note that this is roughly 6x - 9x slower than a normal event handler call.
        /// </remarks>
        public static void Raise<TEventArgs>(Delegate eventHandler, object sender, TEventArgs args = null, Action<Exception> exceptionCallback = null, bool throwEventHelperException = false)
            where TEventArgs: EventArgs
        {
            Raise(eventHandler, sender, () => args, exceptionCallback, throwEventHelperException);
        }

        /// <summary>
        /// Raises an event for an event handler.
        /// </summary>
        /// <param name="eventHandler"></param>
        /// <param name="sender"></param>
        /// <param name="buildArgsCallback"></param>
        /// <param name="exceptionCallback">
        /// Called whenever an exception is raised. If this is supplied then <paramref name="throwEventHelperException"/> has no effect,
        /// all handlers will be called and this function won't let exceptions thrown by the handlers bubble (unless the callback
        /// throws an exception, in which case an EventHelperException that wraps the exception(s) will be thrown).</param>
        /// <param name="throwEventHelperException">
        /// True if exceptions thrown by event handlers should be wrapped into a single EventHelperException, false if the first
        /// exception thrown by an event handler should be rethrown. If this is false then the first event handler that throws
        /// an exception will stop all other event handlers from being called.
        /// </param>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <remarks>
        /// Note that this is roughly 6x - 9x slower than a normal event handler call.
        /// </remarks>
        public static void Raise<TEventArgs>(Delegate eventHandler, object sender, Func<TEventArgs> buildArgsCallback, Action<Exception> exceptionCallback = null, bool throwEventHelperException = false)
            where TEventArgs: EventArgs
        {
            AddCacheCountToQueuesDisplay();

            var handler = eventHandler as Delegate;
            if(handler != null) {
                var args = buildArgsCallback();
                object[] handlerParams = null;
                var exceptions = new List<Exception>();

                var invocationList = handler.GetInvocationList();
                for(var i = 0;i < invocationList.Length;++i) {
                    try {
                        var method = invocationList[i];

                        if(method.Target != null && method.Target is ISynchronizeInvoke && ((ISynchronizeInvoke)method.Target).InvokeRequired) {
                            if(handlerParams == null) handlerParams = new object[] { sender, args };
                            ((ISynchronizeInvoke)method.Target).Invoke(method, handlerParams);
                        } else {
                            Action<object, object, TEventArgs> cachedMethodDelegate;
                            lock(_CachedMethodsLock) {
                                object anonymousDelegate;
                                if(!_CachedMethods.TryGetValue(method.Method, out anonymousDelegate)) {
                                    anonymousDelegate = CompileDelegate<TEventArgs>(method.Method);
                                    _CachedMethods.Add(method.Method, anonymousDelegate);
                                }
                                cachedMethodDelegate = (Action<object, object, TEventArgs>)anonymousDelegate;
                            }

                            cachedMethodDelegate(method.Target, sender, args);
                        }
                    } catch(ThreadAbortException) {
                        ;
                    } catch(TargetInvocationException ex) {
                        Exception realException = ex.InnerException;

                        if(exceptionCallback == null) {
                            if(throwEventHelperException) {
                                exceptions.Add(realException);
                            } else {
                                Rethrow(realException);
                            }
                        } else {
                            try {
                                exceptionCallback(realException);
                            } catch(Exception callbackException) {
                                exceptions.Add(callbackException);
                            }
                        }
                    } catch(Exception ex) {
                        if(exceptionCallback == null) {
                            if(throwEventHelperException) {
                                exceptions.Add(ex);
                            } else {
                                throw;
                            }
                        } else {
                            try {
                                exceptionCallback(ex);
                            } catch(Exception callbackException) {
                                exceptions.Add(callbackException);
                            }
                        }
                    }
                }

                if(exceptions.Count > 0) {
                    throw new EventHelperException(
                        String.Format("{0} exception{1} thrown by event handler{2} for {3}: {4}",
                            exceptions.Count,
                            exceptions.Count == 1 ? "" : "s",
                            invocationList.Length == 1 ? "" : "s",
                            handler.Method == null ? "<no name>" : handler.Method.Name,
                            String.Join(", ", exceptions.Select(r => r.Message).Distinct().DefaultIfEmpty("").ToArray())
                        ), exceptions
                    );
                }
            }
        }

        /// <summary>
        /// Creates the object that displays the number of entries in the cached methods dictionary in the diagnostics
        /// queue display.
        /// </summary>
        private static void AddCacheCountToQueuesDisplay()
        {
            if(_QueueReporter == null) {
                lock(_CachedMethodsLock) {
                    if(_QueueReporter == null) {
                        _QueueReporter = new QueueReporter();
                        QueueRepository.AddQueue(_QueueReporter);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a compiled delegate for the MethodInfo passed across.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        /// <remarks>
        /// Mostly copied from
        /// http://stackoverflow.com/questions/7083618/alternative-for-using-slow-dynamicinvoke-on-muticast-delegate
        /// with two changes - the arguments parameter is now generic, added support for static event handlers.
        /// </remarks>
        private static Action<object, object, TEventArgs> CompileDelegate<TEventArgs>(MethodInfo methodInfo)
            where TEventArgs: EventArgs
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var sender = Expression.Parameter(typeof(object), "sender");
            var parameter = Expression.Parameter(typeof(TEventArgs), "parameter");
            
            var lambda = Expression.Lambda<Action<object, object, TEventArgs>>(
                Expression.Call(
                    methodInfo.IsStatic ? null : Expression.Convert(instance, methodInfo.DeclaringType),
                    methodInfo,
                    sender,
                    parameter
                ),
                instance,
                sender,
                parameter
            );

            return lambda.Compile();
        }

        /// <summary>
        /// Rethrows an exception without losing stack information.
        /// </summary>
        /// <param name="ex"></param>
        /// <remarks>
        /// Copied from http://stackoverflow.com/questions/4555599/how-to-rethrow-the-inner-exception-of-a-targetinvocationexception-without-losing.
        /// </remarks>
        public static void Rethrow(Exception ex)
        {
            // Note from the OP that this has been replaced with an official API, ExceptionDispatchInfo, in .NET 4.5
            typeof(Exception).GetMethod("PrepForRemoting", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(ex, new object[0]);
            throw ex;
        }
    }
}
