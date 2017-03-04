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
using System.Collections.Specialized;

namespace VirtualRadar.Interface.WebServer
{
    /// <summary>
    /// The interface for objects that describe an incoming request.
    /// </summary>
    public interface IRequest
    {
        /// <summary>
        /// Gets the cookies on the request.
        /// </summary>
        CookieCollection Cookies { get; }

        /// <summary>
        /// Gets the values sent in the body of a POST request.
        /// </summary>
        NameValueCollection FormValues { get; }

        /// <summary>
        /// Gets a collection of the headers that were sent with the request.
        /// </summary>
        NameValueCollection Headers { get; }

        /// <summary>
        /// Gets the HTTP method used in the request.
        /// </summary>
        string HttpMethod { get; }

        /// <summary>
        /// Gets a stream that exposes the body of post requests.
        /// </summary>
        Stream InputStream { get; }

        /// <summary>
        /// Gets a value indicating that the request came from the local computer.
        /// </summary>
        bool IsLocal { get; }

        /// <summary>
        /// Gets or sets the maximum post body size.
        /// </summary>
        int MaximumPostBodySize { get; set; }

        /// <summary>
        /// Gets the URL from the root of the site.
        /// </summary>
        /// <remarks>
        /// For the request address 'http://127.0.0.1/MySite/MyPage.html", where MySite is the server root,
        /// this would return '/MySite/MyPage.html'.
        /// </remarks>
        string RawUrl { get; }

        /// <summary>
        /// Gets the address that the Internet request originated from.
        /// </summary>
        IPEndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Gets the address that the Internet request was sent to.
        /// </summary>
        IPEndPoint LocalEndPoint { get; }

        /// <summary>
        /// Gets the URL of the request.
        /// </summary>
        /// <remarks>
        /// This is the full address of the requested page, e.g. 'http://127.0.0.1/MySite/MyPage.html'.
        /// </remarks>
        Uri Url { get; }

        /// <summary>
        /// Gets the user-agent as reported by the client browser.
        /// </summary>
        string UserAgent { get; }

        /// <summary>
        /// Gets the user hostname, the hostname that the browser originally requested. For requests coming through
        /// port forwarding this will be the address that the browser requested and not necessarily the true address
        /// of the site.
        /// </summary>
        string UserHostName { get; }

        /// <summary>
        /// Gets or sets a value indicating that the web site has detected a valid CORS request and wants the response to
        /// include the correct headers if the request is successful.
        /// </summary>
        bool IsValidCorsRequest { get; set; }

        /// <summary>
        /// Gets the CORS origin.
        /// </summary>
        string CorsOrigin { get; }

        /// <summary>
        /// Returns the body as a single string.
        /// </summary>
        /// <param name="encoding"></param>
        /// <returns></returns>
        string ReadBodyAsString(Encoding encoding);
    }
}
