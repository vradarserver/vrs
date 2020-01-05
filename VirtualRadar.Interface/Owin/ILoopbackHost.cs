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
using System.Threading.Tasks;
using AWhewell.Owin.Interface;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.Interface.Owin
{
    /// <summary>
    /// Manages a host that is not attached to a web server, it just calls OWIN middleware.
    /// </summary>
    /// <remarks>
    /// This class can be used to call OWIN middleware nodes as if they were being called for a live
    /// HTTP request.
    /// </remarks>
    public interface ILoopbackHost
    {
        /// <summary>
        /// Gets or sets an action that is called by the loopback host to perform modifications
        /// to the environment before passing a request through the pipeline.
        /// </summary>
        Action<IDictionary<string, object>> ModifyEnvironmentAction { get; set; }

        /// <summary>
        /// Configures the web site pipeline.
        /// </summary>
        void ConfigureStandardPipeline();

        /// <summary>
        /// Configures the pipeline using the builder passed across.
        /// </summary>
        /// <param name="pipelineBuilder"></param>
        void ConfigureCustomPipeline(IPipelineBuilder pipelineBuilder);

        /// <summary>
        /// Sends an anonymous GET request from the local loopback:10000 to loopback:10001 through the pipeline.
        /// </summary>
        /// <param name="pathAndFile">The full path from root of the request.</param>
        /// <param name="environment">An optional environment. If passed then all request headers from the environment
        /// are copied to the loopback request.</param>
        /// <returns></returns>
        /// <remarks>
        /// This should correspond closely to the old RequestSimpleContent call on IWebSite. If no environment
        /// is passed then the user agent is set to FAKE REQUEST. Cookies are only sent if environment is supplied.
        /// </remarks>
        SimpleContent SendSimpleRequest(string pathAndFile, IDictionary<string, object> environment = null);
    }
}
