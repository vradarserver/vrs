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
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;

namespace VirtualRadar.Interface.Owin
{
    /// <summary>
    /// The interface for a singleton that holds collections of objects to instantiate and call to
    /// create the application's OWIN pipeline.
    /// </summary>
    [Singleton]
    [Obsolete("IPipelineConfiguration is going away, use IPipelineBuilder")]
    public interface IPipelineConfiguration
    {
        /// <summary>
        /// Adds a pipeline to the application's standard pipeline.
        /// </summary>
        /// <param name="pipelineType">A type that implements <see cref="IPipeline"/>.</param>
        void AddPipeline(Type pipelineType);

        /// <summary>
        /// Adds a pipeline to the application's standard pipeline.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void AddPipeline<T>() where T: IPipeline;

        /// <summary>
        /// Removes a previously registered pipeline type.
        /// </summary>
        /// <param name="pipelineType">A type that implements <see cref="IPipeline"/>.</param>
        void RemovePipeline(Type pipelineType);

        /// <summary>
        /// Removes a previously registered pipeline type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void RemovePipeline<T>() where T: IPipeline;

        /// <summary>
        /// Returns an array of pipeline types that have been registered.
        /// </summary>
        /// <returns></returns>
        Type[] GetPipelines();

        /// <summary>
        /// Returns a collection of newly instantiated pipelines using the types currently registered
        /// against the configuration.
        /// </summary>
        /// <returns></returns>
        IPipeline[] CreatePipelines();
    }
}
