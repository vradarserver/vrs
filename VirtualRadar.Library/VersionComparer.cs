// Copyright © 2010 onwards, Andrew Whewell
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

namespace VirtualRadar.Library
{
    /// <summary>
    /// Holds methods that can be used to compare a text version number against a 
    /// </summary>
    static class VersionComparer
    {
        /// <summary>
        /// Returns a -ve number if version &lt; applicationVersion, 0 if they are the same and a +ve number if version &gt; applicationVersion.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="applicationVersion"></param>
        /// <returns></returns>
        /// <remarks>
        /// Only the first three parts of the version are compared. Throws an exception if version is supplied with four parts.
        /// </remarks>
        public static int Compare(string version, Version applicationVersion)
        {
            string[] chunks = version.Split('.');
            if(chunks.Length != 3) throw new InvalidOperationException(String.Format("{0} is not a valid a.b.c version number", version));

            int major = int.Parse(chunks[0]);
            int minor = int.Parse(chunks[1]);
            int build = int.Parse(chunks[2]);

            int result = major - applicationVersion.Major;
            if(result == 0) result = minor - applicationVersion.Minor;
            if(result == 0) result = build - applicationVersion.Build;

            return result;
        }
    }
}
