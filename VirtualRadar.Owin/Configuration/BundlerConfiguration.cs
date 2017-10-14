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
        /// The object that protects changes to fields.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// The map of bundle paths to bundles.
        /// </summary>
        private Dictionary<string, string> _Bundles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The loopback host.
        /// </summary>
        private ILoopbackHost _LoopbackHost;

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="htmlPath"></param>
        /// <param name="pageBundleIndex"></param>
        /// <param name="javascriptLinkPaths"></param>
        /// <returns></returns>
        public string RegisterJavascriptBundle(string htmlPath, int pageBundleIndex, IEnumerable<string> javascriptLinkPaths)
        {
            if(htmlPath == null) {
                throw new ArgumentNullException(nameof(htmlPath));
            }

            string result = null;
            if(htmlPath.Length > 0 && htmlPath[0] == '/') {
                var bundlePath = BuildBundlePath(htmlPath, pageBundleIndex);

                var bundles = _Bundles;
                if(!bundles.ContainsKey(bundlePath)) {
                    lock(_SyncLock) {
                        var newBundles = CollectionHelper.ShallowCopy(_Bundles);
                        newBundles.Add(bundlePath, CreateBundle(htmlPath, javascriptLinkPaths));
                        _Bundles = newBundles;
                    }
                }

                result = bundlePath;
            }

            return result;
        }

        private string BuildBundlePath(string htmlPath, int pageBundleIndex)
        {
            var result = htmlPath.Substring(1).Replace('/', '-');
            var extensionIndex = result.LastIndexOf('.');
            result = result.Substring(0, extensionIndex);

            return $"/{result}-{pageBundleIndex}.js";
        }

        private string CreateBundle(string htmlPath, IEnumerable<string> javascriptLinkPaths)
        {
            var result = new StringBuilder();
            CreateLoopbackHost();

            foreach(var relativePath in javascriptLinkPaths) {
                var linkPath = UriHelper.RelativePathToFull(htmlPath, relativePath);
                var simpleContent = _LoopbackHost.SendSimpleRequest(linkPath);
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

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="bundlePath"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        public string GetJavascriptBundle(string bundlePath, IDictionary<string, object> environment)
        {
            var bundles = _Bundles;
            if(bundles.TryGetValue(bundlePath, out string result)) {
                var context = PipelineContext.GetOrCreate(environment);
                if(context.Get<bool>(EnvironmentKey.SuppressJavascriptBundles)) {
                    result = null;
                }
            }

            return result;
        }
    }
}
