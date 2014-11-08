// Copyright © 2014 onwards, Andrew Whewell
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
    /// Performs an action on the background thread and waits for it to finish. If it
    /// does not finish within a given amount of time then the background thread is
    /// aborted.
    /// </summary>
    public class BackgroundThreadTimeout
    {
        /// <summary>
        /// Gets the number of milliseconds to wait before aborting the action.
        /// </summary>
        public int Timeout { get; private set; }

        /// <summary>
        /// Gets the action to perform on the background thread.
        /// </summary>
        public Action Action { get; private set; }

        /// <summary>
        /// Gets the exception that was thrown on the background thread.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Gets a value indicating that the action was aborted after the timeout expired.
        /// </summary>
        public bool TimedOut { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating that a timeout should be dealt with as an
        /// TimeoutException rather than setting <see cref="TimedOut"/>.
        /// </summary>
        /// <remarks>
        /// Defaults to true.
        /// </remarks>
        public bool TreatTimeoutAsException { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that an exception on the background thread should
        /// be re-thrown on the main thread instead of being recorded in <see cref="Exception"/>.
        /// </summary>
        /// <remarks>
        /// Defaults to true.
        /// </remarks>
        public bool ThrowExceptions { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="action"></param>
        public BackgroundThreadTimeout(int timeout, Action action)
        {
            Timeout = timeout;
            Action = action;
            TreatTimeoutAsException = true;
            ThrowExceptions = true;
        }

        /// <summary>
        /// Runs the <see cref="Action"/> on a background thread.
        /// </summary>
        /// <returns>True if the action completed within the timeout period, false if it did not or if an exception was thrown.</returns>
        public bool PerformAction()
        {
            var result = false;
            var thread = new Thread(RunAction);
            thread.Start();

            try {
                result = thread.Join(Timeout);
                TimedOut = !result;
                if(TimedOut) {
                    try {
                        thread.Abort();
                    } catch {
                        ;
                    }

                    if(TreatTimeoutAsException) {
                        throw new TimeoutException(String.Format("Action timed out after {0:N0}ms", Timeout));
                    }
                }
                if(Exception != null && ThrowExceptions) {
                    throw Exception;
                }
            } catch(Exception ex) {
                if(ThrowExceptions) throw;
                Exception = ex;
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Runs the action on the background thread.
        /// </summary>
        protected virtual void RunAction()
        {
            try {
                Action();
            } catch(Exception ex) {
                if(!(ex is ThreadAbortException)) {
                    Exception = ex;
                }
            }
        }
    }
}
