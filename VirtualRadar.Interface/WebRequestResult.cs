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
using System.Net;
using System.Text;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Describes the request of a request to an Internet-based API.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WebRequestResult<T>
        where T: class
    {
        /// <summary>
        /// Gets or sets the status code returned by the request.
        /// </summary>
        public HttpStatusCode HttpStatusCode { get; set; }

        /// <summary>
        /// Gets or sets the result of the request.
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public WebRequestResult() : this(HttpStatusCode.NoContent, null)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="httpStatusCode"></param>
        public WebRequestResult(HttpStatusCode httpStatusCode) : this(httpStatusCode, null)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="httpStatusCode"></param>
        /// <param name="result"></param>
        public WebRequestResult(HttpStatusCode httpStatusCode, T result)
        {
            HttpStatusCode = httpStatusCode;
            Result = result;
        }
    }
}
