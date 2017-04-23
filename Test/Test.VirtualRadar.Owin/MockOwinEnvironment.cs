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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Test.Framework;
using VirtualRadar.Interface.Owin;

namespace Test.VirtualRadar.Owin
{
    /// <summary>
    /// Exposes a dictionary of OWIN objects that can be used to test middleware.
    /// </summary>
    public class MockOwinEnvironment
    {
        /// <summary>
        /// Gets the environment.
        /// </summary>
        public IDictionary<string, object> Environment { get; private set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets a request object that has been mapped to the environment.
        /// </summary>
        public PipelineRequest Request { get; private set; }

        /// <summary>
        /// Gets a response object that has been mapped to the environment.
        /// </summary>
        public PipelineResponse Response { get; private set; }

        private CancellationToken _CallCancelled = CancellationToken.None;
        /// <summary>
        /// Gets or sets the cancellation token.
        /// </summary>
        public CancellationToken CallCancelled
        {
            get { return _CallCancelled; }
            set
            {
                _CallCancelled = value;
                Set(OwinConstants.CallCancelled, _CallCancelled);
            }
        }

        /// <summary>
        /// Gets or sets the request's Authorize header value.
        /// </summary>
        public string RequestAuthorizationHeader
        {
            get { return Request.Headers["Authorization"]; }
            set { Request.Headers["Authorization"] = value; }
        }

        /// <summary>
        /// Gets or sets the request's path and file.
        /// </summary>
        public string RequestPath
        {
            get { return Request.Path.Value; }
            set { Request.Path = new PathString(value); }
        }

        private MemoryStream _ResponseBodyStream = new MemoryStream();
        /// <summary>
        /// Gets a byte array that represents the content of the response body.
        /// </summary>
        public byte[] ResponseBodyBytes
        {
            get { return _ResponseBodyStream.ToArray(); }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public MockOwinEnvironment()
        {
            Request = new PipelineRequest(Environment);
            Response = new PipelineResponse(Environment);

            AddRequiredFields();
        }

        /// <summary>
        /// Resets the environment.
        /// </summary>
        /// <param name="addRequiredFields"></param>
        public void Reset(bool addRequiredFields = true)
        {
            Environment.Clear();

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
            Request.PathBase = new PathString(pathBase);
            Request.Path =     new PathString(path);
            Request.Protocol = protocol;
            Request.QueryString = new QueryString(queryString);
            Request.Scheme = requestScheme;

            if(headers == null) {
                headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            }
            Set(OwinConstants.RequestHeaders, headers);

            if(body == null) {
                body = Stream.Null;
            }
            Request.Body = body;
        }

        /// <summary>
        /// Adds or overwrites the required response fields.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="headers"></param>
        public void AddResponseEnvironment(Stream body = null, IDictionary<string, string[]> headers = null)
        {
            if(body == null) {
                body = Stream.Null;
            }
            Response.Body = body;

            if(headers == null) {
                headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            }
            Set(OwinConstants.ResponseHeaders, headers);
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
        /// Constructs a URL from the parts passed across.
        /// </summary>
        /// <param name="scheme"></param>
        /// <param name="hostHeaders"></param>
        /// <param name="pathBase"></param>
        /// <param name="path"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static string ConstructUrl(string scheme, string[] hostHeaders, PathString pathBase, PathString path, QueryString query)
        {
            var result = new StringBuilder();

            return result.ToString();
        }

        /// <summary>
        /// Sets an environment value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void Set(string key, object value)
        {
            if(Environment.ContainsKey(key)) {
                Environment[key] = value;
            } else {
                Environment.Add(key, value);
            }
        }
    }
}
