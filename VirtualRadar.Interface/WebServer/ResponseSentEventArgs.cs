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
using System.Text;

namespace VirtualRadar.Interface.WebServer
{
    /// <summary>
    /// The event args for <see cref="IWebServer"/> events that are raised when a response is
    /// sent to a web browser.
    /// </summary>
    public class ResponseSentEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the address of the page requested.
        /// </summary>
        public string UrlRequested { get; private set; }

        /// <summary>
        /// Gets the full address and port of the browser that requested the page.
        /// </summary>
        public string UserAddressAndPort { get; private set; }

        /// <summary>
        /// Gets the address without the port of the browser that requested the page.
        /// </summary>
        public string UserAddress { get; private set; }

        /// <summary>
        /// Gets the name of the authenticated user that made the request, if any.
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// Gets a value indicating the type of content served.
        /// </summary>
        public ContentClassification Classification { get; private set; }

        /// <summary>
        /// Gets the bytes sent in response to the request.
        /// </summary>
        public long BytesSent { get; private set; }

        /// <summary>
        /// Gets the object describing the original request.
        /// </summary>
        public IRequest Request { get; private set; }

        /// <summary>
        /// Gets the HTTP status code send to the browser.
        /// </summary>
        public int HttpStatus { get; private set; }

        /// <summary>
        /// Gets the number of milliseconds that elapsed between the RequestReceived event being raised and the response returned.
        /// </summary>
        public int Milliseconds { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="urlRequested"></param>
        /// <param name="userAddressAndPort"></param>
        /// <param name="userAddress"></param>
        /// <param name="bytesSent"></param>
        /// <param name="classification"></param>
        /// <param name="request"></param>
        /// <param name="httpStatus"></param>
        /// <param name="milliseconds"></param>
        /// <param name="userName"></param>
        public ResponseSentEventArgs(string urlRequested, string userAddressAndPort, string userAddress, long bytesSent, ContentClassification classification, IRequest request, int httpStatus, int milliseconds, string userName)
        {
            UrlRequested = urlRequested;
            UserAddressAndPort = userAddressAndPort;
            UserAddress = userAddress;
            BytesSent = bytesSent;
            Classification = classification;
            Request = request;
            HttpStatus = httpStatus;
            Milliseconds = milliseconds;
            UserName = userName;
        }
    }
}
