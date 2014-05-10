// Copyright © 2010 onwards, Andrew Whewell
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

namespace VirtualRadar.WebServer
{
    /// <summary>
    /// A wrapper around HttpListenerContext objects.
    /// </summary>
    class Context : IContext
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IRequest Request { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IResponse Response { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string BasicUserName { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string BasicPassword { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="allowCompression"></param>
        public Context(HttpListenerContext context, bool allowCompression)
        {
            Request = new Request(context.Request);
            Response = new Response(context.Response, allowCompression);

            var basicIdentity = context.User == null ? null : context.User.Identity as HttpListenerBasicIdentity;
            if(basicIdentity != null) {
                BasicUserName = basicIdentity.Name;
                BasicPassword = basicIdentity.Password;
            }
        }
    }
}
