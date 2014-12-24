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
using System.Net;
using System.IO;

namespace VirtualRadar.Interface.WebServer
{
    /// <summary>
    /// The interface for objects that describe the response to a request.
    /// </summary>
    public interface IResponse : IDisposable
    {
        /// <summary>
        /// Gets or sets the length of the content to send back to the user.
        /// </summary>
        long ContentLength { get; set; }

        /// <summary>
        /// Gets the cookies to set on the response.
        /// </summary>
        CookieCollection Cookies { get; set; }

        /// <summary>
        /// Gets or sets the mime type to associate with the content.
        /// </summary>
        string MimeType { get; set; }

        /// <summary>
        /// Gets the stream that can be used to send data to the browser.
        /// </summary>
        Stream OutputStream { get; }

        /// <summary>
        /// Gets or sets the HTTP status code to send back to the user.
        /// </summary>
        HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the encoding used in the response.
        /// </summary>
        Encoding ContentEncoding { get; set; }

        /// <summary>
        /// Adds a header to the response. If the header already exists then its value is overwritten.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void AddHeader(string name, string value);

        /// <summary>
        /// Switches compression of the response, but only if the request indicates that the server supports a
        /// compression encoding that we can support.
        /// </summary>
        /// <param name="request"></param>
        /// <remarks>If this is called after the first request for <see cref="OutputStream"/> then an exception
        /// is thrown. If this is called then ResponseComplete must be called, after which it is
        /// illegal to write anything else to the OutputStream.</remarks>
        void EnableCompression(IRequest request);

        /// <summary>
        /// Closes the response, sending the content to the browser.
        /// </summary>
        void Close();

        /// <summary>
        /// Aborts the response without sending the content to the browser.
        /// </summary>
        void Abort();

        /// <summary>
        /// Instructs the web server to redirect the browser to another URL. This sets <see cref="StatusCode"/>
        /// to an appropriate value but does not close <see cref="OutputStream"/>.
        /// </summary>
        /// <param name="url"></param>
        void Redirect(string url);

        /// <summary>
        /// Sets or updates a cookie in the collection of cookies to return to the caller.
        /// </summary>
        /// <param name="cookie"></param>
        void SetCookie(Cookie cookie);
    }
}
