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
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Owin.Configuration
{
    /// <summary>
    /// The default implementation of <see cref="IAccessConfiguration"/>.
    /// </summary>
    class AccessConfiguration : IAccessConfiguration
    {
        /// <summary>
        /// Describes a restricted path.
        /// </summary>
        class RestrictedPath
        {
            /// <summary>
            /// The path that has an <see cref="Access"/> object recorded against it.
            /// </summary>
            public string Path;

            /// <summary>
            /// Path converted to lower case.
            /// </summary>
            public string NormalisedPath;

            /// <summary>
            /// The access filter built from the <see cref="Access"/> stored against the path.
            /// </summary>
            public VirtualRadar.Interface.IAccessFilter AccessFilter;

            /// <summary>
            /// Creates a new object.
            /// </summary>
            /// <param name="path"></param>
            /// <param name="access"></param>
            public RestrictedPath(string path, Access access)
            {
                Path = path;
                NormalisedPath = NormalisePath(path);
                AccessFilter = Factory.Resolve<VirtualRadar.Interface.IAccessFilter>();
                AccessFilter.Initialise(access);
            }

            /// <summary>
            /// Normalises a path for storage of / comparison to <see cref="NormalisedPath"/>.
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public static string NormalisePath(string path)
            {
                return (path ?? "").ToLower();
            }
        }

        /// <summary>
        /// The object that ensures that writes to the fields are thread safe.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// A list of all restricted path objects.
        /// </summary>
        private List<RestrictedPath> _RestrictedPaths = new List<RestrictedPath>();

        /// <summary>
        /// A dictionary of all restricted path objects.
        /// </summary>
        private Dictionary<string, RestrictedPath> _RestrictedPathsMap = new Dictionary<string, RestrictedPath>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, Access> GetRestrictedPathsMap()
        {
            var map = _RestrictedPathsMap;
            var result = new Dictionary<string, Access>(StringComparer.OrdinalIgnoreCase);
            foreach(var kvp in map) {
                result.Add(kvp.Key, kvp.Value.AccessFilter.Access);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="pathFromRoot"></param>
        /// <param name="access"></param>
        public void SetRestrictedPath(string pathFromRoot, Access access)
        {
            pathFromRoot = UriHelper.NormalisePathFromRoot(pathFromRoot);

            lock(_SyncLock) {
                var restrictedPaths = CollectionHelper.ShallowCopy(_RestrictedPaths);
                var restrictedPathsMap = CollectionHelper.ShallowCopy(_RestrictedPathsMap);

                RestrictedPath restrictedPath;
                var hasEntry = restrictedPathsMap.TryGetValue(pathFromRoot, out restrictedPath);
                var removeEntry = access == null || access.DefaultAccess == DefaultAccess.Unrestricted;

                if(removeEntry && hasEntry) {
                    restrictedPaths.Remove(restrictedPath);
                    restrictedPathsMap.Remove(pathFromRoot);
                } else if(!removeEntry) {
                    var newRestrictedPath = new RestrictedPath(pathFromRoot, access);

                    if(!hasEntry) {
                        restrictedPaths.Add(newRestrictedPath);
                        restrictedPathsMap.Add(pathFromRoot, newRestrictedPath);
                    } else {
                        restrictedPaths.Remove(restrictedPath);
                        restrictedPaths.Add(newRestrictedPath);
                        restrictedPathsMap[pathFromRoot] = newRestrictedPath;
                    }
                }

                _RestrictedPaths = restrictedPaths;
                _RestrictedPathsMap = restrictedPathsMap;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="pathAndFile"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public bool IsPathAccessible(string pathAndFile, string ipAddress)
        {
            IPAddress parsedAddress;
            if(String.IsNullOrEmpty(ipAddress) || !IPAddress.TryParse(ipAddress, out parsedAddress)) {
                parsedAddress = IPAddress.None;
            }

            return IsPathAccessible(pathAndFile, parsedAddress);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="pathAndFile"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public bool IsPathAccessible(string pathAndFile, IPAddress ipAddress)
        {
            var result = true;

            var normalisedPathAndFile = RestrictedPath.NormalisePath(pathAndFile);
            var restrictedPaths = _RestrictedPaths;

            for(var i = 0;i < restrictedPaths.Count;++i) {
                var restrictedPath = restrictedPaths[i];
                if(normalisedPathAndFile.StartsWith(restrictedPath.NormalisedPath)) {
                    result = restrictedPath.AccessFilter.Allow(ipAddress);
                    break;
                }
            }

            return result;
        }
    }
}
