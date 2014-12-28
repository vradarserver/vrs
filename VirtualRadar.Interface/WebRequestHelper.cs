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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// A static class that helps with some common tasks when dealing with web requests.
    /// </summary>
    public static class WebRequestHelper
    {
        /// <summary>
        /// The period of time that requests will be repeated for before it gives up.
        /// </summary>
        public static readonly int TimeoutMilliseconds = 20000;

        /// <summary>
        /// A wrapper around GetResponse that automatically retries when the request fails because of
        /// common transitory errors.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="suppressRepeat"></param>
        /// <returns></returns>
        public static WebResponse GetResponse(WebRequest request, bool suppressRepeat = false)
        {
            return RepeatActionOnTemporaryErrorCondition(request, r => r.GetResponse(), suppressRepeat);
        }

        /// <summary>
        /// A wrapper around GetResponseStream that automatically retries when the request fails because
        /// of common transitory errors.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="suppressRepeat"></param>
        /// <returns></returns>
        public static Stream GetResponseStream(WebResponse response, bool suppressRepeat = false)
        {
            return RepeatActionOnTemporaryErrorCondition(response, r => r.GetResponseStream(), suppressRepeat);
        }

        /// <summary>
        /// Performs an action on a web request repeatedly until it either it works, it times out or a horrible error
        /// is caught.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="obj"></param>
        /// <param name="action"></param>
        /// <param name="suppressRepeat"></param>
        /// <returns></returns>
        private static TResponse RepeatActionOnTemporaryErrorCondition<T, TResponse>(T obj, Func<T, TResponse> action, bool suppressRepeat)
            where TResponse: class
        {
            TResponse result = null;
            var repeat = true;

            var started = DateTime.UtcNow;
            while(repeat) {
                repeat = false;
                try {
                    result = action(obj);
                } catch(WebException ex) {
                    if(!suppressRepeat) {
                        if((DateTime.UtcNow - started).TotalMilliseconds < TimeoutMilliseconds) {
                            if(ex.Status == WebExceptionStatus.NameResolutionFailure) {
                                repeat = true;
                                Thread.Sleep(100);
                            }
                        }
                    }
                    if(!repeat) throw;
                }
            }

            return result;
        }
    }
}
