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
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using AWhewell.Owin.Utility;
using Test.Framework;

namespace Test.VirtualRadar.Owin
{
    /// <summary>
    /// Exposes a dictionary of OWIN objects that can be used to test middleware.
    /// </summary>
    public class MockOwinEnvironment
    {
        /// <summary>
        /// Gets the environment. Note that this is a bog-standard dictionary, if you want the one
        /// that accepts a non-existent index then use Context.Environment.
        /// </summary>
        public IDictionary<string, object> Environment { get; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets an OwinContext that wraps the <see cref="Environment"/>.
        /// </summary>
        public OwinContext Context { get; }

        private CancellationToken _CallCancelled = CancellationToken.None;
        /// <summary>
        /// Gets or sets the cancellation token.
        /// </summary>
        public CancellationToken CallCancelled
        {
            get => _CallCancelled;
            set {
                _CallCancelled = value;
                Set(OwinConstants.CallCancelled, _CallCancelled);
            }
        }

        /// <summary>
        /// Gets or sets the request's Authorize header value.
        /// </summary>
        public string RequestAuthorizationHeader
        {
            get => Context.RequestHeadersDictionary.Authorization;
            set => Context.RequestHeadersDictionary["Authorization"] = value;
        }

        /// <summary>
        /// Gets or sets the request HTTP method.
        /// </summary>
        public string RequestMethod
        {
            get => Context.RequestMethod;
            set => Context.Environment[OwinConstants.RequestMethod] = value;
        }

        /// <summary>
        /// Gets the request headers.
        /// </summary>
        public RequestHeadersDictionary RequestHeaders => Context.RequestHeadersDictionary;

        /// <summary>
        /// Gets the response headers.
        /// </summary>
        public ResponseHeadersDictionary ResponseHeaders => Context.ResponseHeadersDictionary;

        /// <summary>
        /// Gets or sets the protocol used when making the request.
        /// </summary>
        public string RequestProtocol
        {
            get => Context.RequestProtocol;
            set => Context.Environment[OwinConstants.RequestProtocol] = value;
        }

        /// <summary>
        /// Gets or sets the scheme used when making the request.
        /// </summary>
        public string RequestScheme
        {
            get => Context.RequestScheme;
            set => Context.Environment[OwinConstants.RequestScheme] = value;
        }

        /// <summary>
        /// Gets or sets the host used when making the request.
        /// </summary>
        public string RequestHost
        {
            get => Context.RequestHost;
            set => Context.RequestHeadersDictionary["Host"] = value;
        }

        /// <summary>
        /// Gets or sets the request's path base.
        /// </summary>
        public string RequestPathBase
        {
            get => Context.RequestPathBase;
            set => Context.Environment[OwinConstants.RequestPathBase] = value;
        }

        /// <summary>
        /// Gets or sets the request's path and file.
        /// </summary>
        public string RequestPath
        {
            get => Context.RequestPath;
            set => Context.Environment[OwinConstants.RequestPath] = value;
        }

        /// <summary>
        /// Gets or sets the request's query string (without the leading ?).
        /// </summary>
        public string RequestQueryString
        {
            get => Context.RequestQueryString;
            set => Context.Environment[OwinConstants.RequestQueryString] = value;
        }

        /// <summary>
        /// Gets or sets the request body stream.
        /// </summary>
        public Stream RequestBody
        {
            get => Context.RequestBody;
            set => Context.Environment[OwinConstants.RequestBody] = value;
        }

        /// <summary>
        /// Gets or sets the user that the request is running as.
        /// </summary>
        public IPrincipal User
        {
            get => Context.RequestPrincipal;
            set => Context.Environment[CustomEnvironmentKey.Principal] = value;
        }

        /// <summary>
        /// Gets or sets the local IP address.
        /// </summary>
        public string ServerLocalIpAddress
        {
            get => Context.ServerLocalIpAddress;
            set => Context.Environment[OwinConstants.LocalIpAddress] = value;
        }

        /// <summary>
        /// Gets or sets the local port.
        /// </summary>
        public int ServerLocalPort
        {
            get => Context.ServerLocalPortNumber.GetValueOrDefault();
            set => Context.Environment[OwinConstants.LocalPort] = value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets or sets the remote IP address.
        /// </summary>
        public string ServerRemoteIpAddress
        {
            get => Context.ServerRemoteIpAddress;
            set => Context.Environment[OwinConstants.RemoteIpAddress] = value;
        }

        /// <summary>
        /// Gets or sets the remote port.
        /// </summary>
        public int ServerRemotePort
        {
            get => Context.ServerRemotePortNumber.GetValueOrDefault();
            set => Context.Environment[OwinConstants.RemotePort] = value.ToString(CultureInfo.InvariantCulture);
        }

        private MemoryStream _ResponseBodyStream = new MemoryStream();
        /// <summary>
        /// Gets a byte array that represents the content of the response body.
        /// </summary>
        public byte[] ResponseBodyBytes => _ResponseBodyStream.ToArray();

        /// <summary>
        /// Gets or the response status code.
        /// </summary>
        public int ResponseStatusCode
        {
            get => Context.ResponseStatusCode.GetValueOrDefault();
            set => Context.Environment[OwinConstants.ResponseStatusCode] = value;
        }

        /// <summary>
        /// Gets or sets the response body stream.
        /// </summary>
        public Stream ResponseBody
        {
            get => Context.ResponseBody;
            set => Context.Environment[OwinConstants.ResponseBody] = value;
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public MockOwinEnvironment()
        {
            Context = new OwinContext(Environment);
            AddRequiredFields();
        }

        /// <summary>
        /// Resets the environment.
        /// </summary>
        /// <param name="addRequiredFields"></param>
        public void Reset(bool addRequiredFields = true)
        {
            Environment.Clear();
            _ResponseBodyStream.Dispose();
            _ResponseBodyStream = new MemoryStream();

            if(addRequiredFields) {
                AddRequiredFields();
            }
        }

        /// <summary>
        /// Adds or overwrites the OWIN, request and response required fields.
        /// </summary>
        public void AddRequiredFields()
        {
            AddOwinEnvironment();
            AddRequestEnvironment();
            AddResponseEnvironment(body: _ResponseBodyStream);
        }

        /// <summary>
        /// Adds or overwrites the required OWIN environment fields.
        /// </summary>
        /// <param name="owinVersion"></param>
        public void AddOwinEnvironment(string owinVersion = "1.0.0")
        {
            Set(OwinConstants.OwinVersion, owinVersion);
            CallCancelled = CallCancelled;
        }

        /// <summary>
        /// Adds or overwrites required request fields to the environment.
        /// </summary>
        /// <param name="pathBase"></param>
        /// <param name="path"></param>
        /// <param name="protocol"></param>
        /// <param name="queryString"></param>
        /// <param name="requestScheme"></param>
        /// <param name="headers"></param>
        public void AddRequestEnvironment(
            string pathBase = "/VirtualRadar",
            string path = "/",
            string protocol = "HTTP/1.0",
            string queryString = "",
            string requestScheme = "HTTP",
            IDictionary<string, string[]> headers = null,
            Stream body = null
        )
        {
            Context.Environment[OwinConstants.RequestPathBase] =    pathBase;
            Context.Environment[OwinConstants.RequestPath] =        path;
            Context.Environment[OwinConstants.RequestProtocol] =    protocol;
            Context.Environment[OwinConstants.RequestQueryString] = queryString;
            Context.Environment[OwinConstants.RequestScheme] =      requestScheme;
            Context.Environment[OwinConstants.RequestHeaders] =     headers ?? new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            Context.Environment[OwinConstants.RequestBody] =        body ?? Stream.Null;
        }

        /// <summary>
        /// Adds or overwrites the required response fields.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="headers"></param>
        public void AddResponseEnvironment(Stream body = null, IDictionary<string, string[]> headers = null)
        {
            Context.Environment[OwinConstants.ResponseBody] =    body ?? Stream.Null;
            Context.Environment[OwinConstants.ResponseHeaders] = headers ?? new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Sets up the Authorize header for the username and password.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public void SetBasicCredentials(string userName, string password)
        {
            var encoded = EncodeBasicCredentials(userName, password);
            RequestAuthorizationHeader = $"Basic {encoded}";
        }

        /// <summary>
        /// Sets the request URL.
        /// </summary>
        /// <param name="unencodedPathAndFile"></param>
        /// <param name="unencodedQueryStrings">Pass unencoded keys and values. Keys with null values are added as '?value' whereas keys with empty strings are added as '?value='.</param>
        public void SetRequestUrl(string unencodedPathAndFile, string[,] unencodedQueryStrings = null)
        {
            unencodedPathAndFile = unencodedPathAndFile ?? "";
            if(unencodedPathAndFile == "" || unencodedPathAndFile[0] != '/') {
                unencodedPathAndFile = $"/{unencodedPathAndFile}";
            }

            RequestPath = unencodedPathAndFile;

            if(unencodedQueryStrings != null) {
                if(unencodedQueryStrings.GetLength(1) != 2) {
                    throw new ArgumentOutOfRangeException($"The unencoded query strings array must be 2D");
                }

                var buffer = new StringBuilder();
                for(var i = 0;i < unencodedQueryStrings.GetLength(0);++i) {
                    var key =   unencodedQueryStrings[i, 0];
                    var value = unencodedQueryStrings[i, 1];

                    if(i > 0) {
                        buffer.Append('&');
                    }
                    buffer.Append(HttpUtility.UrlEncode(key));

                    if(value != null) {
                        buffer.AppendFormat("={0}", HttpUtility.UrlEncode(value));
                    }
                }

                RequestQueryString = buffer.ToString();
            }
        }

        /// <summary>
        /// Converts the username and password into a MIME64 string of username:password.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string EncodeBasicCredentials(string userName, string password)
        {
            var encodeString = $"{userName}:{password}";
            var bytes = Encoding.UTF8.GetBytes(encodeString);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Sets an environment value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void Set(string key, object value) => Context.Environment[key] = value;
    }
}
