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

namespace VirtualRadar.WebSite.MiddlewareConfiguration
{
    /// <summary>
    /// The default implementation of <see cref="IAuthenticationConfiguration"/>.
    /// </summary>
    class AuthenticationConfiguration : IAuthenticationConfiguration
    {
        /// <summary>
        /// The write lock on the administrator paths list.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// The list of paths that only administrators can access.
        /// </summary>
        private List<string> _AdministratorPaths = new List<string>();

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="pathFromRoot"></param>
        public void AddAdministratorPath(string pathFromRoot)
        {
            pathFromRoot = UriHelper.NormalisePathFromRoot(pathFromRoot, convertToLowerCase: true);

            var administratorPaths = _AdministratorPaths;
            if(!administratorPaths.Contains(pathFromRoot)) {
                lock(_SyncLock) {
                    if(!_AdministratorPaths.Contains(pathFromRoot)) {
                        administratorPaths = CollectionHelper.ShallowCopy(_AdministratorPaths);
                        administratorPaths.Add(pathFromRoot);
                        _AdministratorPaths = administratorPaths;
                    }
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="pathFromRoot"></param>
        public void RemoveAdministratorPath(string pathFromRoot)
        {
            pathFromRoot = UriHelper.NormalisePathFromRoot(pathFromRoot, convertToLowerCase: true);

            var administratorPaths = _AdministratorPaths;
            if(administratorPaths.Contains(pathFromRoot)) {
                lock(_SyncLock) {
                    if(_AdministratorPaths.Contains(pathFromRoot)) {
                        administratorPaths = CollectionHelper.ShallowCopy(_AdministratorPaths);
                        administratorPaths.Remove(pathFromRoot);
                        _AdministratorPaths = administratorPaths;
                    }
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public string[] GetAdministratorPaths()
        {
            var result = _AdministratorPaths;
            return result.ToArray();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="pathAndFile"></param>
        /// <returns></returns>
        public bool IsAdministratorPath(string pathAndFile)
        {
            var result = false;

            pathAndFile = (pathAndFile ?? "").ToLower();

            var administratorPaths = _AdministratorPaths;
            for(var i = 0;i < administratorPaths.Count;++i) {
                if(pathAndFile.StartsWith(administratorPaths[i])) {
                    result = true;
                    break;
                }
            }

            return result;
        }
    }
}
