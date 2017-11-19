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
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;

namespace VirtualRadar.Owin.Configuration
{
    /// <summary>
    /// Default implementation of the singleton <see cref="IBundlerConfiguration"/>.
    /// </summary>
    class BundlerConfiguration : IBundlerConfiguration
    {
        /// <summary>
        /// Describes the paths to a bundle.
        /// </summary>
        class BundlePath
        {
            public string RelativeToHtml;
            public string FromRoot;
        }

        /// <summary>
        /// The object that protects changes to fields.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// The map of bundle paths to fully-pathed JavaScript addresses.
        /// </summary>
        private Dictionary<string, IEnumerable<string>> _Bundles = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The loopback host.
        /// </summary>
        private ILoopbackHost _LoopbackHost;

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="htmlRequestEnvironment"></param>
        /// <param name="pageBundleIndex"></param>
        /// <param name="javascriptLinkPaths"></param>
        /// <returns></returns>
        public string RegisterJavascriptBundle(IDictionary<string, object> htmlRequestEnvironment, int pageBundleIndex, IEnumerable<string> javascriptLinkPaths)
        {
            if(htmlRequestEnvironment == null) {
                throw new ArgumentNullException(nameof(htmlRequestEnvironment));
            }

            var context = PipelineContext.GetOrCreate(htmlRequestEnvironment);
            var htmlPath = context.Request.FlattenedPath;

            string result = null;
            if(htmlPath.Length > 0 && htmlPath[0] == '/') {
                var bundlePath = BuildPaths(htmlPath, pageBundleIndex);
                if(bundlePath != null) {
                    var bundles = _Bundles;
                    if(!bundles.ContainsKey(bundlePath.FromRoot)) {
                        lock(_SyncLock) {
                            var newBundles = CollectionHelper.ShallowCopy(_Bundles);
                            newBundles.Add(bundlePath.FromRoot, javascriptLinkPaths.Select(relativePath => {
                                var linkPath = UriHelper.RelativePathToFull(htmlPath, relativePath);
                                return UriHelper.FlattenPath(linkPath);
                            }));
                            _Bundles = newBundles;
                        }
                    }

                    result = bundlePath.RelativeToHtml;
                }
            }

            return result;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="bundleRequestEnvironment"></param>
        /// <returns></returns>
        public string GetJavascriptBundle(IDictionary<string, object> bundleRequestEnvironment)
        {
            var bundles = _Bundles;

            var context = PipelineContext.GetOrCreate(bundleRequestEnvironment);
            if(bundles.TryGetValue(context.Request.FlattenedPath, out IEnumerable<string> javascriptLinkPaths)) {
                if(context.Get<bool>(EnvironmentKey.SuppressJavascriptBundles)) {
                    javascriptLinkPaths = null;
                }
            }

            var result = javascriptLinkPaths == null ? null : CreateBundle(javascriptLinkPaths, bundleRequestEnvironment);

            return result;
        }

        private BundlePath BuildPaths(string htmlPath, int pageBundleIndex)
        {
            BundlePath result = null;

            var fromRoot = new StringBuilder();
            var extensionIndex = htmlPath.LastIndexOf('.');
            if(extensionIndex > 0) {
                fromRoot.Append(htmlPath.Substring(0, extensionIndex));
                fromRoot.AppendFormat("-{0}-bundle", pageBundleIndex);
                fromRoot.Append(".js");

                result = new BundlePath() {
                    FromRoot = fromRoot.ToString(),
                };

                var lastFolderIndex = result.FromRoot.LastIndexOf('/');
                if(lastFolderIndex == -1) {
                    result = null;
                } else {
                    result.RelativeToHtml = result.FromRoot.Substring(lastFolderIndex + 1);
                }
            }

            return result;
        }

        private string CreateBundle(IEnumerable<string> javascriptFullyPathedLinks, IDictionary<string, object> requestEnvironment)
        {
            var result = new StringBuilder();
            CreateLoopbackHost();

            foreach(var linkPath in javascriptFullyPathedLinks) {
                var simpleContent = _LoopbackHost.SendSimpleRequest(linkPath, requestEnvironment);
                var content = simpleContent.HttpStatusCode == HttpStatusCode.OK && simpleContent.Content != null
                    ? Encoding.UTF8.GetString(simpleContent.Content)
                    : $"// Status {(int)simpleContent.HttpStatusCode} on {linkPath}";

                if(result.Length > 0) {
                    result.AppendLine(";");
                }
                result.AppendLine($"/* {linkPath} */");
                result.AppendLine(content);
            }

            return result.ToString();
        }

        private void CreateLoopbackHost()
        {
            if(_LoopbackHost == null) {
                var loopbackHost = Factory.Singleton.Resolve<ILoopbackHost>();
                loopbackHost.ConfigureStandardPipeline();
                loopbackHost.ModifyEnvironmentAction = r => {
                    if(r.ContainsKey(EnvironmentKey.SuppressJavascriptBundles)) {
                        r[EnvironmentKey.SuppressJavascriptBundles] = true;
                    } else {
                        r.Add(EnvironmentKey.SuppressJavascriptBundles, true);
                    }
                };

                lock(_SyncLock) {
                    if(_LoopbackHost == null) {
                        _LoopbackHost = loopbackHost;
                    }
                }
            }
        }
    }
}
