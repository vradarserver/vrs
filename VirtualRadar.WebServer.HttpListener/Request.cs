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
using System.Collections.Specialized;
using System.Web;
using Microsoft.Owin;

namespace VirtualRadar.WebServer.HttpListener
{
    /// <summary>
    /// A wrapper around an IOwinRequest.
    /// </summary>
    class Request : IRequest
    {
        /// <summary>
        /// The underlying OWIN environment object that we're wrapping.
        /// </summary>
        private IOwinRequest _Request;

        private CookieCollection _Cookies;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public CookieCollection Cookies
        {
            get {
                if(_Cookies == null) {
                    var cookies = new CookieCollection();
                    foreach(var cookie in _Request.Cookies) {
                        cookies.Add(new Cookie(cookie.Key, cookie.Value));
                    }
                    _Cookies = cookies;
                }

                return _Cookies;
            }
        }

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
                    foreach(var headerKey in _Request.Headers.Keys) {
                        headers.Add(headerKey, _Request.Headers[headerKey]);
                    }
                    _Headers = headers;
                }

                return _Headers;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string HttpMethod
        {
            get { return _Request.Method; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public Stream InputStream
        {
            get { return _Request.Body; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsLocal
        {
            get {
                // Ported from HttpRequest source
                var remoteAddress = _Request.RemoteIpAddress;

                if(String.IsNullOrEmpty(remoteAddress)) {
                    return false;
                }

                if(remoteAddress == "127.0.0.1" || remoteAddress == "::1") {
                    return true;
                }

                if(remoteAddress == _Request.LocalIpAddress) {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string RawUrl
        {
            get {
                // Working off the "URI reconstruction algorithm" from OWIN.ORG (http://owin.org/html/owin.html#53-paths):
                var requestPathBase = (string)_Request.Environment["owin.RequestPathBase"];
                var requestPath =     (string)_Request.Environment["owin.RequestPath"];
                var queryString =     (string)_Request.Environment["owin.RequestQueryString"];

                var result = new StringBuilder();
                result.Append(requestPathBase);
                result.Append(requestPath);
                if(!String.IsNullOrEmpty(queryString)) {
                    result.Append('?');
                    result.Append(queryString);
                }

                return result.ToString();
            }
        }

        private IPEndPoint _RemoteEndPoint;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get {
                if(_RemoteEndPoint == null) {
                    var remoteEndPoint = new IPEndPoint(IPAddress.Parse(_Request.RemoteIpAddress), _Request.RemotePort.GetValueOrDefault());
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
                    var localEndPoint = new IPEndPoint(IPAddress.Parse(_Request.LocalIpAddress), _Request.LocalPort.GetValueOrDefault());
                    _LocalEndPoint = localEndPoint;
                }

                return _LocalEndPoint;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public Uri Url
        {
            get { return _Request.Uri; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string UserAgent
        {
            get { return _Request.Headers["User-Agent"]; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string UserHostName
        {
            get { return _Request.Host.Value; }
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
            get { return (_Request.Headers["Origin"] ?? "").Trim(); }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="owinRequest"></param>
        public Request(IOwinRequest owinRequest)
        {
            _Request = owinRequest;
            MaximumPostBodySize = 4 * 1024 * 1024;
        }

        /// <summary>
        /// Extracts the form values from the body of an HTTP POST request.
        /// </summary>
        private void ExtractFormValues()
        {
            var contentType = _Request.ContentType ?? "";
            var parameterIndex = contentType.IndexOf(';');
            var parameter = parameterIndex == -1 ? "" : contentType.Substring(parameterIndex + 1).Trim();
            if(parameterIndex != -1) contentType = contentType.Substring(0, parameterIndex).Trim();

            if(HttpMethod == "POST" && contentType == "application/x-www-form-urlencoded") {
                var charset = Encoding.UTF8;
                if(parameter.StartsWith("charset=")) charset = ParseEncoding(parameter.Substring(8).Trim());

                string content = ReadBodyAsString(charset);
                if(!String.IsNullOrEmpty(content)) {
                    for(int start = 0, end = 0;start < content.Length;start = end + 1) {
                        end = content.IndexOf('&', start);
                        if(end == -1) end = content.Length;
                        var nameValue = content.Substring(start, end - start);

                        var separator = nameValue.IndexOf('=');
                        var name = separator == -1 ? nameValue : nameValue.Substring(0, separator).Trim();
                        var value = separator == -1 ? "" : nameValue.Substring(separator + 1);
                        value = HttpUtility.UrlDecode(value);
                        if(!String.IsNullOrEmpty(name)) {
                            _FormValues[name] = value;
                        }
                    }
                }
            }
        }

        private Encoding ParseEncoding(string charsetName)
        {
            var result = Encoding.UTF8;
            switch(charsetName.ToUpper()) {
                case "US-ASCII":    result = Encoding.ASCII; break;
                case "UTF-7":       result = Encoding.UTF7; break;
                case "UTF-16BE":    result = Encoding.BigEndianUnicode; break;
                case "UTF-16LE":    result = Encoding.Unicode; break;
                case "UTF-32LE":    result = Encoding.UTF32; break;
                default:            break;
            }

            return result;
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
