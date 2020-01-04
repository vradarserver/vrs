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
using AWhewell.Owin.Utility;

namespace VirtualRadar.Interface.Owin
{
    /// <summary>
    /// Exposes the <see cref="PipelineRequest"/> and <see cref="PipelineResponse"/> objects.
    /// </summary>
    [Obsolete("Use AWhewell.Owin.Utility.OwinContext")]
    public class PipelineContext : OwinContext
    {
        private PipelineRequest _PipelineRequest;
        /// <summary>
        /// Exposes the request as a <see cref="PipelineRequest"/>.
        /// </summary>
        public PipelineRequest Request
        {
            get { return _PipelineRequest; }
        }

        private PipelineResponse _PipelineResponse;
        /// <summary>
        /// Exposes the response as a <see cref="PipelineResponse"/>.
        /// </summary>
        public PipelineResponse Response
        {
            get { return _PipelineResponse; }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PipelineContext() : base()
        {
            BuildRequestAndResponse();
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="environment"></param>
        public PipelineContext(IDictionary<string, object> environment) : base(environment)
        {
            BuildRequestAndResponse();
        }

        /// <summary>
        /// Creates the custom request and response
        /// </summary>
        private void BuildRequestAndResponse()
        {
            _PipelineRequest = new PipelineRequest(Environment);
            _PipelineResponse = new PipelineResponse(Environment); 
        }

        /// <summary>
        /// Gets the Pipeline context stored in the environment. If a context cannot be found then
        /// one is created and stored within the environment.
        /// </summary>
        /// <param name="environment"></param>
        public static PipelineContext GetOrCreate(IDictionary<string, object> environment)
        {
            if(!environment.TryGetValue(VrsEnvironmentKey.PipelineContext, out var objResult)) {
                objResult = new PipelineContext(environment);
                environment[VrsEnvironmentKey.PipelineContext] = objResult;
            }

            return (PipelineContext)objResult;
        }

        /// <summary>
        /// Constructs a URL from the components commonly found in OWIN environment dictionaries.
        /// </summary>
        /// <param name="scheme"></param>
        /// <param name="host"></param>
        /// <param name="pathBase"></param>
        /// <param name="path"></param>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public static string ConstructUrl(string scheme, string host, string pathBase, string path, string queryString)
        {
            return OwinPath.ConstructUrl(
                scheme,
                host,
                pathBase,
                path,
                queryString
            );
        }
    }
}
