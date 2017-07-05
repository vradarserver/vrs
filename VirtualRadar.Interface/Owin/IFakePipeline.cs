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
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.Interface.Owin
{
    /// <summary>
    /// Manages a fake OWIN middleware pipeline.
    /// </summary>
    /// <remarks>
    /// Fake pipelines can be used to call OWIN middleware as if they were being called for a live
    /// HTTP request.
    /// </remarks>
    public interface IFakePipeline
    {
        /// <summary>
        /// Configures the pipeline using the standard web site middleware.
        /// </summary>
        void ConfigureStandardPipeline();

        /// <summary>
        /// Configures the pipeline using the web app configuration passed across.
        /// </summary>
        /// <param name="webAppConfiguration"></param>
        void ConfigureCustomPipeline(IWebAppConfiguration webAppConfiguration);

        /// <summary>
        /// Sends an anonymous GET request from the local loopback:10000 to loopback:10001 through the pipeline.
        /// </summary>
        /// <param name="pathAndFile"></param>
        /// <returns></returns>
        /// <remarks>
        /// This should correspond closely to the old RequestSimpleContent call on IWebSite. The user agent is
        /// set to FAKE REQUEST, the user host name is FAKE.HOST.NAME. No cookies are sent.
        /// </remarks>
        SimpleContent SendSimpleRequest(string pathAndFile);
    }
}
