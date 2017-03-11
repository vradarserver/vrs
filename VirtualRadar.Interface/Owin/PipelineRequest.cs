using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace VirtualRadar.Interface.Owin
{
    /// <summary>
    /// Extends an OwinRequest object.
    /// </summary>
    public class PipelineRequest : OwinRequest
    {
        /// <summary>
        /// Gets or sets the <see cref="RemoteIpAddress"/> parsed into a System.Net IPAddress.
        /// </summary>
        public IPAddress RemoteIpAddressParsed
        {
            get {
                return GetOrSet<IPAddress>(EnvironmentKey.RequestRemoteIpAddressParsed, () => {
                    var remoteIpAddress = RemoteIpAddress;
                    var parsed = IPAddress.None;
                    if(!String.IsNullOrEmpty(remoteIpAddress)) {
                        if(!IPAddress.TryParse(remoteIpAddress, out parsed)) {
                            parsed = IPAddress.None;
                        }
                    }
                    return parsed;
                });
            }
            set { Set(EnvironmentKey.RequestRemoteIpAddressParsed, value); }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PipelineRequest() : base()
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="environment"></param>
        public PipelineRequest(IDictionary<string, object> environment) : base(environment)
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
