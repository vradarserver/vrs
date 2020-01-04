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
using AWhewell.Owin.Utility;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.WebServer;

namespace VirtualRadar.WebServer.HttpListener
{
    /// <summary>
    /// A wrapper around OWIN request and response objects.
    /// </summary>
    class Context : IContext
    {
        /// <summary>
        /// The context that this object is wrapping.
        /// </summary>
        private OwinContext _Context;

        private Request _Request;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IRequest Request
        {
            get { return _Request; }
        }

        private Response _Response;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IResponse Response
        {
            get { return _Response; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string BasicUserName
        {
            get { return _Context.RequestPrincipal?.Identity?.Name; }
        }

        /// <summary>
        /// See interface docs... not sure where this is used?
        /// </summary>
        public string BasicPassword { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="environment"></param>
        public Context(IDictionary<string, object> environment)
        {
            _Context = OwinContext.Create(environment);
            _Request = new Request(_Context);
            _Response = new Response(_Context);
        }
    }
}
