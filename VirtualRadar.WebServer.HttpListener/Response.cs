// Copyright © 2017 onwards, Andrew Whewell
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
using VirtualRadar.Interface.WebServer;
using System.Net;
using System.IO;
using System.IO.Compression;
using Microsoft.Owin;

namespace VirtualRadar.WebServer.HttpListener
{
    /// <summary>
    /// A wrapper around IOwinResponse.
    /// </summary>
    class Response : IResponse
    {
        /// <summary>
        /// The response object that we are wrapping.
        /// </summary>
        private IOwinResponse _Response;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long ContentLength
        {
            get { return _Response.ContentLength.GetValueOrDefault(); }
            set { _Response.ContentLength = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public CookieCollection Cookies { get; set; } = new CookieCollection();

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="cookie"></param>
        public void SetCookie(Cookie cookie)
        {
            Cookies.Add(cookie);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string MimeType
        {
            get { return _Response.ContentType; }
            set { _Response.ContentType = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public Stream OutputStream
        {
            get { return _Response.Body; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get { return (HttpStatusCode)_Response.StatusCode; }
            set { _Response.StatusCode = (int)value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <remarks>
        /// The HttpListenerResponse is just a property...
        /// </remarks>
        public Encoding ContentEncoding { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="response"></param>
        public Response(IOwinResponse response)
        {
            _Response = response;
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~Response()
        {
            Dispose(false);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if(disposing) {
                ;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddHeader(string name, string value)
        {
            _Response.Headers.Add(name, new string[] { value });
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="url"></param>
        public void Redirect(string url)
        {
            _Response.Redirect(url);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="request"></param>
        public void EnableCompression(IRequest request)
        {
            // I'm leaving compression alone for now. These IRequest and IResponse implementations are only
            // here for the shim's sake, once everything has been moved over to OWIN I will be deleting them.
            // Any implementation of compression needs to be a pure OWIN implementation and not a part of the
            // shim.
            ;
        }

        /// <summary>
        /// Returns true if the browser appears to be one that does not deal well with compressed responses.
        /// </summary>
        /// <param name="userAgent"></param>
        /// <returns>True if compression could break the browser, false if the browser should be alright.</returns>
        /// <remarks><para>
        /// In testing compression has worked fine on everything except iOS Mobile Safari. On there I randomly get instances where the browser
        /// requests a file, I send the file back (compressed) and then the browser acts as if it hasn't received it and pauses for 75 seconds
        /// until it finally realises that it's there and then continues with the page load. If I disable compression then this effect is not
        /// seen. When I look on Wireshark the file has been sent correctly - it hangs on a random file, it's not always the same one. As far
        /// as I can see I'm sending the right headers. If I run it on the iOS simulator it's fine, it's just my iPad that's having problems.
        /// For now, just in case it's ALL iPads that get the problem, I'm disabling compression if the user agent declares itself to be iOS
        /// Safari. Mac OS/X Safari is fine. I've also added a configuration switch that can be used to switch compression off entirely.
        /// </para><para>
        /// The problem only affects Safari - if you use Chrome on the iPad it's fine, no hangs.
        /// </para></remarks>
        private bool IsBadBrowser(string userAgent)
        {
            // Chrome on iPad: Mozilla/5.0 (iPad; CPU OS 6_0_1 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) CriOS/31.0.1650.18 Mobile/10A523 Safari/8536.25
            // Safari on iPad: Mozilla/5.0 (iPad; CPU OS 6_0_1 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A523 Safari/8536.25
            return !String.IsNullOrEmpty(userAgent) && (userAgent.Contains("iPad;") || userAgent.Contains("iPhone;")) && userAgent.Contains("Safari/") && !userAgent.Contains("CriOS/");
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Close()
        {
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Abort()
        {
        }
    }
}
