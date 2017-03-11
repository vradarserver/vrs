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
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Interface.Owin
{
    /// <summary>
    /// The interface for a singleton object that supplies configuration details to the middleware
    /// that allows or disallows access based on the IP address of the requestor.
    /// </summary>
    public interface IAccessConfiguration : ISingleton<IAccessConfiguration>
    {
        /// <summary>
        /// Returns a map of restricted paths to the <see cref="Access"/> describing which IPAddresses can access the path.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Implementations must ensure that the dictionary that is returned uses the ordinal case-insensitive
        /// string comparer.
        /// </remarks>
        IDictionary<string, Access> GetRestrictedPathsMap();

        /// <summary>
        /// Sets access on a restricted path. If <paramref name="access"/> is null then the restrictions are removed
        /// from the path.
        /// </summary>
        /// <param name="pathFromRoot"></param>
        /// <param name="access"></param>
        void SetRestrictedPath(string pathFromRoot, Access access);

        /// <summary>
        /// Returns true if the IP address passed across is allowed to access the supplied path and file.
        /// </summary>
        /// <param name="pathAndFile"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        bool IsPathAccessible(string pathAndFile, string ipAddress);

        /// <summary>
        /// Returns true if the IP address passed across is allowed to access the supplied path and file.
        /// </summary>
        /// <param name="pathAndFile"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        bool IsPathAccessible(string pathAndFile, IPAddress ipAddress);
    }
}
