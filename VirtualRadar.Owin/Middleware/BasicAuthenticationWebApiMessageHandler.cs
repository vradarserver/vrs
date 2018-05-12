// Copyright © 2018 onwards, Andrew Whewell
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
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VirtualRadar.Interface.Owin;

namespace VirtualRadar.Owin.Middleware
{
    /// <summary>
    /// A web API message handler that picks up credentials sent via basic authentication and
    /// uses them to set up the principal on the request.
    /// </summary>
    /// <remarks>
    /// This lets you use Authorize attributes on web API actions. The drawback with web API
    /// message handlers is that they derive from a base class so we need to expose the class
    /// rather than an interface.
    /// </remarks>
    class BasicAuthenticationWebApiMessageHandler : System.Net.Http.DelegatingHandler, IBasicAuthenticationWebApiMessageHandler
    {
        /// <summary>
        /// The object that holds all of the common code for basic authentication.
        /// </summary>
        private BasicAuthenticationHelper _BasicAuthentication;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public BasicAuthenticationWebApiMessageHandler() : base()
        {
            _BasicAuthentication = new BasicAuthenticationHelper();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestContext = request.GetRequestContext();
            if(requestContext.Principal == null) {
                string userName = null;
                string password = null;
                if(ExtractCredentials(request, ref userName, ref password)) {
                    var cachedUser = _BasicAuthentication.GetCachedUser(userName);
                    var cachedUserTag = _BasicAuthentication.GetCachedUserTag(cachedUser);
                    var isPasswordValid = _BasicAuthentication.IsPasswordValid(cachedUser, cachedUserTag, password);

                    if(isPasswordValid) {
                        requestContext.Principal = _BasicAuthentication.CreatePrincipal(cachedUser, cachedUserTag);
                    }
                }
            }

            return base.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// Extracts the credentials from the request.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private bool ExtractCredentials(HttpRequestMessage request, ref string userName, ref string password)
        {
            return _BasicAuthentication.ExtractCredentials(request.Headers.Authorization?.ToString(), ref userName, ref password);
        }
    }
}
