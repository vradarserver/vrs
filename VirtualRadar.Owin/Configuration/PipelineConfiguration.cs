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
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;

namespace VirtualRadar.Owin.Configuration
{
    /// <summary>
    /// Default implementation of <see cref="IPipelineConfiguration"/>.
    /// </summary>
    class PipelineConfiguration : IPipelineConfiguration
    {
        /// <summary>
        /// The list of registered pipeline types. All types implement <see cref="IPipeline"/>.
        /// </summary>
        List<Type> _Pipelines = new List<Type>();

        /// <summary>
        /// The object that protects fields against multithreaded writes.
        /// </summary>
        object _SyncLock = new object();

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="pipelineType"></param>
        public void AddPipeline(Type pipelineType)
        {
            AssertPipelineType(pipelineType);

            lock(_SyncLock) {
                if(!_Pipelines.Contains(pipelineType)) {
                    var newList = CollectionHelper.ShallowCopy(_Pipelines);
                    newList.Add(pipelineType);
                    _Pipelines = newList;
                }
            }
        }

        private void AssertPipelineType(Type pipelineType)
        {
            if(pipelineType == null) {
                throw new ArgumentNullException(nameof(pipelineType));
            }
            if(!typeof(IPipeline).IsAssignableFrom(pipelineType)) {
                throw new InvalidOperationException($"{pipelineType.FullName} does not implement {nameof(IPipeline)}");
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddPipeline<T>()
            where T: IPipeline
        {
            AddPipeline(typeof(T));
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="pipelineType"></param>
        public void RemovePipeline(Type pipelineType)
        {
            AssertPipelineType(pipelineType);

            lock(_SyncLock) {
                var newList = CollectionHelper.ShallowCopy(_Pipelines);
                newList.Remove(pipelineType);
                _Pipelines = newList;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RemovePipeline<T>()
            where T: IPipeline
        {
            RemovePipeline(typeof(T));
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public Type[] GetPipelines()
        {
            var types = _Pipelines;
            return types.ToArray();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IPipeline[] CreatePipelines()
        {
            var result = new List<IPipeline>();

            var types = _Pipelines;
            foreach(var type in types) {
                result.Add((IPipeline)Activator.CreateInstance(type));
            }

            return result.ToArray();
        }
    }
}
