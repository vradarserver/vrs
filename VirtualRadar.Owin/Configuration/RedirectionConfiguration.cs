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
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;

namespace VirtualRadar.Owin.Configuration
{
    /// <summary>
    /// Default implementation of <see cref="IRedirectionConfiguration"/>.
    /// </summary>
    class RedirectionConfiguration : IRedirectionConfiguration
    {
        /// <summary>
        /// Collects together all of the information we need about a redirection.
        /// </summary>
        class Redirection
        {
            /// <summary>
            /// The path from root to redirect to.
            /// </summary>
            public string ToPath;

            /// <summary>
            /// The circumstances under which the redirection takes place.
            /// </summary>
            public RedirectionContext RedirectionContext;

            /// <summary>
            /// Gets a value indicating that the <see cref="RedirectionContext"/> applies to mobiles only.
            /// </summary>
            public bool IsMobile
            {
                get { return (RedirectionContext & RedirectionContext.Mobile) != 0; }
            }

            /// <summary>
            /// Creates a new object.
            /// </summary>
            /// <param name="toPath"></param>
            /// <param name="context"></param>
            public Redirection(string toPath, RedirectionContext context)
            {
                ToPath = toPath;
                RedirectionContext = context;
            }
        }

        /// <summary>
        /// Protects fields from multi-threaded writes.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// The map of redirections.
        /// </summary>
        private Dictionary<string, List<Redirection>> _Redirections = new Dictionary<string, List<Redirection>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="pathFromRoot"></param>
        /// <param name="redirectToPathFromRoot"></param>
        /// <param name="redirectionContext"></param>
        public void AddRedirection(string pathFromRoot, string redirectToPathFromRoot, RedirectionContext redirectionContext)
        {
            if(String.IsNullOrEmpty(pathFromRoot)) {
                throw new ArgumentException(nameof(pathFromRoot));
            }
            if(String.IsNullOrEmpty(redirectToPathFromRoot)) {
                throw new ArgumentException(nameof(redirectToPathFromRoot));
            }

            lock(_SyncLock) {
                List<Redirection> liveRedirectList;
                List<Redirection> redirectList;

                var redirections = CollectionHelper.ShallowCopy(_Redirections);
                if(redirections.TryGetValue(pathFromRoot, out liveRedirectList)) {
                    redirectList = CollectionHelper.ShallowCopy(liveRedirectList);
                    redirections[pathFromRoot] = redirectList;
                } else {
                    redirectList = new List<Redirection>();
                    redirections.Add(pathFromRoot, redirectList);
                }

                var redirection = new Redirection(redirectToPathFromRoot, redirectionContext);
                var existingIndex = redirectList.FindIndex(r => r.RedirectionContext == redirectionContext);
                if(existingIndex > -1) {
                    redirectList[existingIndex] = redirection;
                } else {
                    redirectList.Add(redirection);
                }

                _Redirections = redirections;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="pathFromRoot"></param>
        /// <param name="redirectionContext"></param>
        public void RemoveRedirection(string pathFromRoot, RedirectionContext redirectionContext)
        {
            if(String.IsNullOrEmpty(pathFromRoot)) {
                throw new ArgumentException(nameof(pathFromRoot));
            }

            lock(_SyncLock) {
                List<Redirection> liveRedirectList;

                var redirections = CollectionHelper.ShallowCopy(_Redirections);
                if(redirections.TryGetValue(pathFromRoot, out liveRedirectList)) {
                    var redirectList = CollectionHelper.ShallowCopy(liveRedirectList);
                    redirections[pathFromRoot] = redirectList;

                    var existingIndex = redirectList.FindIndex(r => r.RedirectionContext == redirectionContext);
                    if(existingIndex > -1) {
                        redirectList.RemoveAt(existingIndex);
                        if(redirectList.Count == 0) {
                            redirections.Remove(pathFromRoot);
                        }
                    }
                }

                _Redirections = redirections;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="pathFromRoot"></param>
        /// <param name="requestContext"></param>
        /// <returns></returns>
        public string RedirectToPathFromRoot(string pathFromRoot, RedirectionRequestContext requestContext)
        {
            string result = null;

            var redirections = _Redirections;
            List<Redirection> redirectList;
            if(redirections.TryGetValue(pathFromRoot, out redirectList)) {
                for(var i = 0;i < redirectList.Count;++i) {
                    var redirection = redirectList[i];
                    var useThisRedirection = result == null || requestContext.IsMobile == redirection.IsMobile;

                    if(useThisRedirection) {
                        result = redirection.ToPath;
                    }
                }
            }

            return result;
        }
    }
}
