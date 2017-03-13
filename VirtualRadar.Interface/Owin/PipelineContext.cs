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
