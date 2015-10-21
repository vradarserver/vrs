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
using System.Linq;
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
        /// Raises an event safely and guarantees that every handler will be called even if one or more handlers throws
        /// an exception.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventHandler"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <param name="exceptionCallback">Optional callback that is passed an exception thrown by one of the event handlers. ThreadAbort
        /// exceptions are never passed across.</param>
        public static void Raise<T>(EventHandler<T> eventHandler, object sender, T args, Action<Exception> exceptionCallback = null)
            where T: EventArgs
        {
            Raise(eventHandler, sender, () => args, exceptionCallback);
        }

        /// <summary>
        /// Raises an event safely and guarantees that every handler will be called even if one or more handlers throws an exception.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventHandler"></param>
        /// <param name="sender"></param>
        /// <param name="buildArgsCallback">Only called if there are event handlers to call.</param>
        /// <param name="exceptionCallback"></param>
        public static void Raise<T>(EventHandler<T> eventHandler, object sender, Func<T> buildArgsCallback, Action<Exception> exceptionCallback = null)
            where T: EventArgs
        {
            if(eventHandler != null) {
                var args = buildArgsCallback();
                var invocationList = eventHandler.GetInvocationList();
                for(var i = 0;i < invocationList.Length;++i) {
                    try {
                        var eventHandlerMethod = (EventHandler<T>)invocationList[i];
                        eventHandlerMethod(sender, args);
                    } catch(ThreadAbortException) {
                        ;
                    } catch(Exception ex) {
                        try {
                            if(exceptionCallback != null) {
                                exceptionCallback(ex);
                            }
                        } catch {
                            ; // Don't let exceptions in the callback stop the event handlers being called
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Raises an event for a plain event args. If args is null then EventArgs.Empty is passed instead.
        /// </summary>
        /// <param name="eventHandler"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <param name="exceptionCallback"></param>
        public static void Raise(EventHandler eventHandler, object sender, EventArgs args = null, Action<Exception> exceptionCallback = null)
        {
            Raise(eventHandler, sender, () => args, exceptionCallback);
        }

        /// <summary>
        /// Raises an event for a plain EventArgs. If <paramref name="buildArgsCallback"/> returns null then EventArgs.Empty is passed to the event handlers.
        /// </summary>
        /// <param name="eventHandler"></param>
        /// <param name="sender"></param>
        /// <param name="buildArgsCallback"></param>
        /// <param name="exceptionCallback"></param>
        public static void Raise(EventHandler eventHandler, object sender, Func<EventArgs> buildArgsCallback, Action<Exception> exceptionCallback = null)
        {
            if(eventHandler != null) {
                var args = buildArgsCallback();
                if(args == null) args = EventArgs.Empty;

                var invocationList = eventHandler.GetInvocationList();
                for(var i = 0;i < invocationList.Length;++i) {
                    try {
                        var eventHandlerMethod = (EventHandler)invocationList[i];
                        eventHandlerMethod(sender, args);
                    } catch(ThreadAbortException) {
                        ;
                    } catch(Exception ex) {
                        try {
                            if(exceptionCallback != null) {
                                exceptionCallback(ex);
                            }
                        } catch {
                            ; // Don't let exceptions in the callback stop the event handlers being called
                        }
                    }
                }
            }
        }
    }
}
