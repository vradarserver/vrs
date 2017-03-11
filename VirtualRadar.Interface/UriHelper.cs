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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Utility methods that can help when dealing with URLs.
    /// </summary>
    public static class UriHelper
    {
        /// <summary>
        /// Adds slashes to the start and end of the path (if required). A null or empty input
        /// results in an output of '/'.
        /// </summary>
        /// <param name="pathFromRoot"></param>
        /// <param name="convertToLowerCase"></param>
        /// <returns></returns>
        public static string NormalisePathFromRoot(string pathFromRoot, bool convertToLowerCase = false)
        {
            pathFromRoot = (pathFromRoot ?? "").Trim();
            if(convertToLowerCase) {
                pathFromRoot = pathFromRoot.ToLower();
            }

            if(!pathFromRoot.StartsWith("/")) pathFromRoot = String.Format("/{0}", pathFromRoot);
            if(!pathFromRoot.EndsWith("/"))   pathFromRoot = String.Format("{0}/", pathFromRoot);

            return pathFromRoot;
        }
    }
}
