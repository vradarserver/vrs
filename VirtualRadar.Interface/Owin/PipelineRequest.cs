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
        /// As per the OwinRequest.Path exception an empty string path is returned as /.
        /// </summary>
        public PathString PathNormalised
        {
            get {
                return GetOrSet<PathString>(EnvironmentKey.RequestPathNormalised, () => {
                    var path = Path;
                    if(path.Value == "") {
                        path = new PathString("/");
                    }
                    return path;
                });
            }
            set {
                var path = value;
                if(path.Value == "") {
                    path = new PathString("/");
                }
                Set<PathString>(EnvironmentKey.RequestPathNormalised, path);
            }
        }

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
        /// Gets or sets an indicator that the user-agent indicates that the request MIGHT be from a mobile device.
        /// </summary>
        public bool IsMobileUserAgentString
        {
            get {
                return GetOrSet<bool>(EnvironmentKey.RequestIsMobileUserAgentString, () => {
                    var result = false;
                    var userAgent = UserAgent;
                    if(!String.IsNullOrEmpty(userAgent)) {
                        var tokens = userAgent.Split(' ', '/', '(', ')');
                        result = tokens.Any(r => 
                            String.Equals("mobile", r, StringComparison.OrdinalIgnoreCase) ||
                            String.Equals("iemobile", r, StringComparison.OrdinalIgnoreCase)
                        );
                    }
                    return result;
                });
            }
        }

        /// <summary>
        /// Gets or sets the user agent from the request.
        /// </summary>
        public string UserAgent
        {
            get { return Headers["User-Agent"]; }
            set { Headers.Set("User-Agent", value); }
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
