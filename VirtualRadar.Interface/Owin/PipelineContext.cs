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
using Microsoft.Owin;

namespace VirtualRadar.Interface.Owin
{
    /// <summary>
    /// Exposes the <see cref="PipelineRequest"/> and <see cref="PipelineResponse"/> objects.
    /// </summary>
    public class PipelineContext : OwinContext
    {
        private PipelineRequest _PipelineRequest;
        /// <summary>
        /// Exposes the request as a <see cref="PipelineRequest"/>.
        /// </summary>
        public new PipelineRequest Request
        {
            get { return _PipelineRequest; }
        }

        private PipelineResponse _PipelineResponse;
        /// <summary>
        /// Exposes the response as a <see cref="PipelineResponse"/>.
        /// </summary>
        public new PipelineResponse Response
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
            return GetOrSet(environment, EnvironmentKey.PipelineContext, () => new PipelineContext(environment));
        }

        /// <summary>
        /// If the key is present in the environment then the associated value is returned, otherwise
        /// the build function is called, the result stored against the key and the result returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="environment"></param>
        /// <param name="key"></param>
        /// <param name="buildFunc"></param>
        /// <returns></returns>
        public static T GetOrSet<T>(IDictionary<string, object> environment, string key, Func<T> buildFunc)
        {
            object result;
            if(!environment.TryGetValue(key, out result)) {
                result = buildFunc();
                environment[key] = result;
            }

            return (T)result;
        }

        /// <summary>
        /// Handles the storage of a translation of a raw value where the translation can be expensive to compute, so we
        /// only want to do it once.
        /// </summary>
        /// <typeparam name="TOriginal">The type of the value that is being translated.</typeparam>
        /// <typeparam name="TTranslation">The type of the translation.</typeparam>
        /// <param name="environment"></param>
        /// <param name="originalKey">The key used to store the value that was translated.</param>
        /// <param name="translationKey">The key used to store the translated value.</param>
        /// <param name="currentValue">The current value to translate.</param>
        /// <param name="buildTranslation">The build function to call if no translation exists or if the value used to create the
        /// stored translation does not equal the current value.</param>
        /// <returns>The stored translation if the current value equals the value used to build the translation, or the new translation.</returns>
        public static TTranslation GetOrSetTranslation<TOriginal, TTranslation>(IDictionary<string, object> environment, string originalKey, string translationKey, TOriginal currentValue, Func<TTranslation> buildTranslation)
        {
            if(String.IsNullOrEmpty(translationKey)) {
                throw new ArgumentNullException(nameof(translationKey));
            }
            if(originalKey == null) {
                originalKey = $"original.{translationKey}";
            }
            if(String.Equals(translationKey, originalKey, StringComparison.OrdinalIgnoreCase)) {
                throw new InvalidOperationException("The translation and original keys must be different");
            }

            object originalValue;
            var exists = environment.TryGetValue(originalKey, out originalValue);
            var hasChanged = !exists || !Object.Equals(originalValue, currentValue);

            TTranslation result;
            if(!hasChanged) {
                result = (TTranslation)environment[translationKey];
            } else {
                result = buildTranslation();
                if(!exists) {
                    environment.Add(originalKey, currentValue);
                    environment.Add(translationKey, result);
                } else {
                    environment[originalKey] = currentValue;
                    environment[translationKey] = result;
                }
            }

            return result;
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
            var result = new StringBuilder();

            result.Append(String.IsNullOrEmpty(scheme) ? "http" : scheme);
            result.Append("://");
            result.Append(String.IsNullOrEmpty(host) ? "127.0.0.1" : host);     // note that OWIN host headers can include the port
            result.Append(pathBase);
            result.Append(path);

            if(!String.IsNullOrEmpty(queryString)) {
                result.Append($"?{queryString}");       // note that OWIN presents query strings in their percent encoded form
            }

            return result.ToString();
        }
    }
}
