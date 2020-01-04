// Copyright © 2017 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.IO;
using System.Net;
using System.Text;
using AWhewell.Owin.Utility;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.WebServer;

namespace VirtualRadar.WebServer.HttpListener
{
    /// <summary>
    /// A wrapper around IOwinResponse.
    /// </summary>
    class Response : IResponse
    {
        /// <summary>
        /// The context that we are wrapping.
        /// </summary>
        private OwinContext _Context;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long ContentLength
        {
            get => _Context.ResponseHeadersDictionary.ContentLength.GetValueOrDefault();
            set => _Context.ResponseHeadersDictionary.ContentLength = value;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string MimeType
        {
            get => _Context.ResponseHeadersDictionary.ContentTypeValue.MediaType;
            set {
                var currentValues = _Context.ResponseHeadersDictionary.ContentTypeValue;
                _Context.ResponseHeadersDictionary.ContentTypeValue = new ContentTypeValue(
                    value,
                    currentValues.Charset,
                    currentValues.Boundary
                );
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public Stream OutputStream => _Context.ResponseBody;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get => _Context.ResponseHttpStatusCode.GetValueOrDefault();
            set => _Context.ResponseHttpStatusCode = value;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public Encoding ContentEncoding
        {
            get => _Context.ResponseHeadersDictionary.ContentTypeValue.Encoding;
            set {
                var currentValues = _Context.ResponseHeadersDictionary.ContentTypeValue;
                _Context.ResponseHeadersDictionary.ContentTypeValue = new ContentTypeValue(
                    currentValues.MediaType,
                    value.WebName,
                    currentValues.Boundary
                );
            }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="context"></param>
        public Response(OwinContext context)
        {
            _Context = context;
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
            _Context.ResponseHeadersDictionary[name] = value;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="url"></param>
        public void Redirect(string url)
        {
            _Context.Redirect(url);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="request"></param>
        public void EnableCompression(IRequest request)
        {
            // I'm leaving compression alone for now.
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
