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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using VirtualRadar.Interface;

namespace VirtualRadar.Interface.Owin
{
    /// <summary>
    /// Extends an OwinRequest object.
    /// </summary>
    public class PipelineRequest : OwinRequest
    {
        /// <summary>
        /// As per the OwinRequest.Path except an empty string path is returned as /.
        /// </summary>
        public PathString PathNormalised
        {
            get {
                var result = Path;
                if(result.Value == "") {
                    result = new PathString("/");
                }
                return result;
            }
        }

        /// <summary>
        /// As per OwinRequest.Path except a string is returned, directory traversal characters are parsed out and
        /// an empty or null path is returned as /.
        /// </summary>
        public string FlattenedPath
        {
            get {
                return FlattenPath(PathNormalised.Value ?? "");
            }
        }

        /// <summary>
        /// The filename portion of the request URL. If no filename has been specified then an empty string is returned.
        /// </summary>
        public string FileName
        {
            get {
                var path = FlattenedPath;
                var lastFolderIndex = path.LastIndexOf('/');
                return lastFolderIndex == -1 ? "" : path.Substring(lastFolderIndex + 1);
            }
        }

        /// <summary>
        /// All of the portions of the request URL that were separated by forward-slash. If the URL was not terminated with
        /// a forward-slash then the final portion (which is assumed to be the filename) is not included.
        /// </summary>
        /// <remarks>
        /// For example, if the URL is '/folder/filename' then this will return an array of one string containing 'folder'. However,
        /// if the URL is '/folder/subfolder/' then you will get two strings back - 'folder' and 'subfolder'.
        /// </remarks>
        public string[] PathParts
        {
            get {
                var result = new List<string>();
                var path = PathNormalised.Value;

                var startPart = -1;
                for(var i = 0;i < path.Length;++i) {
                    if(path[i] == '/') {
                        if(startPart > -1) {
                            result.Add(path.Substring(startPart, i - startPart));
                            startPart = -1;
                        }
                    } else if(startPart == -1) {
                        startPart = i;
                    }
                }

                return result.ToArray();
            }
        }

        /// <summary>
        /// Gets or sets an indicator that the user-agent indicates that the request MIGHT be from a mobile device.
        /// </summary>
        public bool IsMobileUserAgentString
        {
            get {
                return GetOrSetTranslation<string, bool>(
                    null,
                    EnvironmentKey.IsMobileUserAgentString,
                    UserAgent,
                    () => {
                        var result = false;
                        var userAgent = UserAgent;
                        if(!String.IsNullOrEmpty(userAgent)) {
                            var tokens = userAgent.Split(' ', '/', '(', ')', ';');
                            result = tokens.Any(r => 
                                String.Equals("mobile", r, StringComparison.OrdinalIgnoreCase) ||
                                String.Equals("iemobile", r, StringComparison.OrdinalIgnoreCase) ||
                                String.Equals("android", r, StringComparison.OrdinalIgnoreCase) ||
                                String.Equals("playstation", r, StringComparison.OrdinalIgnoreCase) ||
                                String.Equals("nintendo", r, StringComparison.OrdinalIgnoreCase) ||
                                r.StartsWith("appletv", StringComparison.OrdinalIgnoreCase)
                            );
                        }
                        return result;
                    }
                );
            }
        }

        /// <summary>
        /// Gets a value indicating that the request came from the LAN or was local.
        /// </summary>
        public bool IsLocalOrLan
        {
            get {
                return GetOrSetTranslation<string, bool>(
                    null,
                    EnvironmentKey.IsLocalOrLan,
                    ClientIpAddress,
                    () => {
                        return IPEndPointHelper.IsLocalOrLanAddress(ClientIpEndPoint);
                    }
                );
            }
        }

        /// <summary>
        /// Gets a value indicating that the request came from the Internet. Shorthand for !IsLocalOrLan.
        /// </summary>
        public bool IsInternet
        {
            get {
                return !IsLocalOrLan;
            }
        }

        /// <summary>
        /// Gets the IP address of the machine that made the request on the server. If the request
        /// came from a proxy server on the LAN then it is the address of the machine that accesssed
        /// the proxy server, as opposed to RequestIpAddress which would be the address of the proxy
        /// server.
        /// </summary>
        public string ClientIpAddress
        {
            get {
                DetermineClientAndProxyAddresses();
                return Get<string>(EnvironmentKey.ClientIpAddress);
            }
        }

        /// <summary>
        /// Gets the <see cref="ClientIpAddress"/> parsed into a System.Net IPAddress.
        /// </summary>
        public IPAddress ClientIpAddressParsed
        {
            get {
                return GetOrSetTranslation<string, IPAddress>(
                    null,
                    EnvironmentKey.ClientIpAddressParsed,
                    ClientIpAddress,
                    () => {
                        return ParseIpAddress(ClientIpAddress);
                    }
                );
            }
        }

        /// <summary>
        /// Gets the <see cref="ClientIpAddress"/> and <see cref="RemotePort"/> parsed and joined together into an end point.
        /// </summary>
        public IPEndPoint ClientIpEndPoint
        {
            get {
                return GetOrSetTranslation<string, IPEndPoint>(
                    null,
                    EnvironmentKey.ClientIpEndPoint,
                    String.Format("{0}:{1}", ClientIpAddress, RemotePort),
                    () => {
                        return new IPEndPoint(ClientIpAddressParsed, RemotePort.GetValueOrDefault());
                    }
                );
            }
        }

        /// <summary>
        /// Gets the address of the reverse proxy that the request came through, if known.
        /// </summary>
        public string ProxyIpAddress
        {
            get {
                DetermineClientAndProxyAddresses();
                return Get<string>(EnvironmentKey.ProxyIpAddress);
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

        /// <summary>
        /// See <see cref="PipelineContext.GetOrSetTranslation{TOriginal, TTranslation}(IDictionary{string, object}, string, string, TOriginal, Func{TTranslation})"/>.
        /// </summary>
        /// <typeparam name="TOriginal"></typeparam>
        /// <typeparam name="TTranslation"></typeparam>
        /// <param name="originalKey"></param>
        /// <param name="translationKey"></param>
        /// <param name="currentValue"></param>
        /// <param name="buildTranslation"></param>
        /// <returns></returns>
        protected virtual TTranslation GetOrSetTranslation<TOriginal, TTranslation>(string originalKey, string translationKey, TOriginal currentValue, Func<TTranslation> buildTranslation)
        {
            return PipelineContext.GetOrSetTranslation<TOriginal, TTranslation>(Environment, originalKey, translationKey, currentValue, buildTranslation);
        }

        /// <summary>
        /// Parses an IP address. If the address is unparseable then IPAddress.None is returned.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        private IPAddress ParseIpAddress(string ipAddress)
        {
            var result = IPAddress.None;

            if(!String.IsNullOrEmpty(ipAddress)) {
                if(!IPAddress.TryParse(ipAddress, out result)) {
                    result = IPAddress.None;
                }
            }

            return result;
        }

        /// <summary>
        /// Tries to determine the true IP address of the client and the address of the proxy, if any. If the
        /// values have already been calculated and aren't going to change then this does nothing.
        /// </summary>
        private void DetermineClientAndProxyAddresses()
        {
            var translationBasis = String.Format("{0}-{1}", RemoteIpAddress, Headers["X-Forwarded-For"]);
            if(!String.Equals(translationBasis, Get<string>(EnvironmentKey.ClientIpAddressBasis))) {
                Set<string>(EnvironmentKey.ClientIpAddressBasis, translationBasis);

                var localOrLanRequest = IPEndPointHelper.IsLocalOrLanAddress(new IPEndPoint(ParseIpAddress(RemoteIpAddress), RemotePort.GetValueOrDefault()));
                var xff = localOrLanRequest ? Headers["X-Forwarded-For"] : null;

                if(!String.IsNullOrEmpty(xff)) {
                    xff = xff.Split(',').Last().Trim();
                    IPAddress ipAddress;
                    if(!IPAddress.TryParse(xff, out ipAddress)) {
                        xff = null;
                    }
                }

                if(String.IsNullOrEmpty(xff)) {
                    Set<string>(EnvironmentKey.ClientIpAddress, RemoteIpAddress);
                    Set<string>(EnvironmentKey.ProxyIpAddress, null);
                } else {
                    Set<string>(EnvironmentKey.ClientIpAddress, xff);
                    Set<string>(EnvironmentKey.ProxyIpAddress, RemoteIpAddress);
                }
            }
        }

        /// <summary>
        /// Accepts a request path and returns the same path after processing directory traversal parts.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string FlattenPath(string value)
        {
            var result = new StringBuilder();

            var pathParts = (value ?? "").Split(new char[] { '/' });
            for(var i = 0;i < pathParts.Length;++i) {
                var pathPart = pathParts[i];
                switch(pathPart) {
                    case ".":
                        TerminatePathWithSlash(result);
                        break;
                    case "..":
                        var lastFolderIdx = FindLastFolderIndex(result);
                        if(lastFolderIdx != -1) {
                            ++lastFolderIdx;
                            result.Remove(lastFolderIdx, result.Length - lastFolderIdx);
                        }
                        TerminatePathWithSlash(result);
                        break;
                    default:
                        TerminatePathWithSlash(result);
                        result.Append(pathPart);
                        break;
                }
            }

            return result.ToString();
        }

        private void TerminatePathWithSlash(StringBuilder buffer)
        {
            if(buffer.Length == 0 || buffer[buffer.Length - 1] != '/') {
                buffer.Append('/');
            }
        }

        private int FindLastFolderIndex(StringBuilder buffer)
        {
            var startIndex = buffer.Length > 0 && buffer[buffer.Length - 1] == '/' ? buffer.Length - 2 : buffer.Length - 1;
            return buffer.LastIndexOf('/', startIndex);
        }
    }
}
