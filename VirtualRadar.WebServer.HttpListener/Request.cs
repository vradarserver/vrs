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
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using AWhewell.Owin.Utility;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.WebServer;

namespace VirtualRadar.WebServer.HttpListener
{
    /// <summary>
    /// A wrapper around an IOwinRequest.
    /// </summary>
    class Request : IRequest
    {
        /// <summary>
        /// The underlying context we're wrapping.
        /// </summary>
        private OwinContext _Context;

        private NameValueCollection _FormValues;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public NameValueCollection FormValues
        {
            get {
                if(_FormValues == null) {
                    _FormValues = new NameValueCollection();
                    ExtractFormValues();
                }
                return _FormValues;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int MaximumPostBodySize { get; set; }

        private NameValueCollection _Headers;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public NameValueCollection Headers
        {
            get {
                if(_Headers == null) {
                    var headers = new NameValueCollection();
                    var owinHeaders = _Context.RequestHeadersDictionary;
                    foreach(var headerKey in owinHeaders.Keys) {
                        headers.Add(headerKey, owinHeaders[headerKey]);
                    }
                    _Headers = headers;
                }

                return _Headers;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string HttpMethod => _Context.RequestMethod;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public Stream InputStream => _Context.RequestBody;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsLocal => _Context.ServerIsLocal.GetValueOrDefault();

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string RawUrl => _Context.RequestUrl;

        private IPEndPoint _RemoteEndPoint;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get {
                if(_RemoteEndPoint == null) {
                    var remoteEndPoint = new IPEndPoint(IPAddress.Parse(_Context.ServerRemoteIpAddress), _Context.ServerRemotePortNumber.GetValueOrDefault());
                    _RemoteEndPoint = remoteEndPoint;
                }

                return _RemoteEndPoint;
            }
        }

        private IPEndPoint _LocalEndPoint;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IPEndPoint LocalEndPoint
        {
            get {
                if(_LocalEndPoint == null) {
                    var localEndPoint = new IPEndPoint(IPAddress.Parse(_Context.ServerLocalIpAddress), _Context.ServerLocalPortNumber.GetValueOrDefault());
                    _LocalEndPoint = localEndPoint;
                }

                return _LocalEndPoint;
            }
        }

        private Uri _Url;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public Uri Url
        {
            get {
                if(_Url == null) {
                    _Url = new Uri(_Context.RequestUrl);
                }
                return _Url;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string UserAgent
        {
            get { return _Context.RequestHeadersDictionary.UserAgent; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string UserHostName
        {
            get { return _Context.RequestHost; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsValidCorsRequest { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string CorsOrigin
        {
            get { return (_Context.RequestHeadersDictionary["Origin"] ?? "").Trim(); }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="context"></param>
        public Request(OwinContext context)
        {
            _Context = context;
            MaximumPostBodySize = 4 * 1024 * 1024;
        }

        /// <summary>
        /// Extracts the form values from the body of an HTTP POST request.
        /// </summary>
        private void ExtractFormValues()
        {
            if(_Context.RequestHttpMethod == AWhewell.Owin.Utility.HttpMethod.Post) {
                if(_Context.RequestHeadersDictionary.ContentTypeValue.MediaTypeParsed == MediaType.UrlEncodedForm) {
                    var form = _Context.RequestBodyForm(caseSensitiveKeys: false);
                    foreach(var key in form.Keys) {
                        _FormValues[key] = form[key];
                    }
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public string ReadBodyAsString(Encoding encoding)
        {
            string result = null;

            using(var stream = InputStream) {
                using(var memory = new MemoryStream()) {
                    var bytesRead = 0;
                    var totalBytesRead = 0;
                    var buffer = new byte[128];
                    var failed = false;
                    do {
                        totalBytesRead += (bytesRead = stream.Read(buffer, 0, buffer.Length));
                        if(totalBytesRead > MaximumPostBodySize) failed = true;
                        else if(bytesRead != 0) memory.Write(buffer, 0, bytesRead);
                    } while(bytesRead != 0 && !failed);

                    if(!failed) {
                        var bytes = memory.ToArray();
                        result = encoding.GetString(bytes);
                    }
                }
            }

            return result;
        }
    }
}
