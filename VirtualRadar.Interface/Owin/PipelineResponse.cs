using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace VirtualRadar.Interface.Owin
{
    /// <summary>
    /// Wraps an OwinResponse response object.
    /// </summary>
    public class PipelineResponse : OwinResponse
    {
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PipelineResponse() : base()
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="environment"></param>
        public PipelineResponse(IDictionary<string, object> environment) : base(environment)
        {
        }

        /// <summary>
        /// See <see cref="PipelineContext.GetOrSet{T}(IDictionary{string, object}, string, Func{T})"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="buildFunc"></param>
        /// <returns></returns>
        protected virtual T GetOrSet<T>(string key, Func<T> buildFunc)
        {
            return PipelineContext.GetOrSet<T>(Environment, key, buildFunc);
        }
    }
}
