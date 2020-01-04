// Copyright © 2017 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Collections.Generic;
using System.Net.Http;
using AWhewell.Owin.Interface.WebApi;
using AWhewell.Owin.Utility;
using InterfaceFactory;

namespace VirtualRadar.Interface.Owin
{
    /// <summary>
    /// The base controller for all non-static web API controllers in the application.
    /// </summary>
    public abstract class BaseApiController : IApiController
    {
        /// <summary>
        /// A responder that can be used to set up the <see cref="OwinEnvironment"/> with responses.
        /// </summary>
        protected static IWebApiResponder _WebApiResponder;

        /// <summary>
        /// Initialises static fields.
        /// </summary>
        static BaseApiController()
        {
            _WebApiResponder = Factory.Resolve<IWebApiResponder>();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IDictionary<string, object> OwinEnvironment { get; set; }

        private OwinContext _Context;
        /// <summary>
        /// Gets a <see cref="OwinContext"/> around the <see cref="OwinEnvironment"/>.
        /// </summary>
        protected OwinContext Context
        {
            get {
                if(_Context == null) {
                    _Context = OwinContext.Create(OwinEnvironment);
                }
                return _Context;
            }
        }

        private bool _ReadRoute;
        private Route _Route;
        /// <summary>
        /// Gets the <see cref="Route"/> representing the executing method.
        /// </summary>
        protected Route Route
        {
            get {
                if(!_ReadRoute) {
                    _ReadRoute = true;
                    _Route = OwinEnvironment[WebApiEnvironmentKey.Route] as Route;
                }
                return _Route;
            }
        }

        /// <summary>
        /// Gets the query string with case insensitive keys.
        /// </summary>
        protected QueryStringDictionary RequestQueryString => Context.RequestQueryStringDictionary(caseSensitiveKeys: false);
    }
}
