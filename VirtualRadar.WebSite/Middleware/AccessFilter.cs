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
using System.Net;
using System.Threading.Tasks;
using AWhewell.Owin.Utility;
using InterfaceFactory;
using VirtualRadar.Interface.Owin;

namespace VirtualRadar.WebSite.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Default implementation of <see cref="IAccessFilter"/>.
    /// </summary>
    class AccessFilter : IAccessFilter
    {
        /// <summary>
        /// The singleton <see cref="IAccessConfiguration"/>.
        /// </summary>
        private IAccessConfiguration _AccessConfiguration;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public AccessFilter()
        {
            _AccessConfiguration = Factory.ResolveSingleton<IAccessConfiguration>();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public AppFunc AppFuncBuilder(AppFunc next)
        {
            return async(IDictionary<string, object> environment) => {
                if(AllowAccess(environment)) {
                    await next(environment);
                }
            };
        }

        /// <summary>
        /// Returns true if the request passes the access configuration.
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        private bool AllowAccess(IDictionary<string, object> environment)
        {
            var context = OwinContext.Create(environment);
            var result = _AccessConfiguration.IsPathAccessible(context.RequestPathFlattened, context.ClientIpAddressParsed);

            if(!result) {
                context.ResponseHttpStatusCode = HttpStatusCode.Forbidden;
            }

            return result;
        }
    }
}
