// Copyright © 2013 onwards, Andrew Whewell
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

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// A class that describes a root folder to serve files from.
    /// </summary>
    public class SiteRoot
    {
        /// <summary>
        /// Gets or sets the folder to serve files from.
        /// </summary>
        public string Folder { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates the order in which site roots are searched for files to serve.
        /// Lower numbers are searched before higher numbers. The VRS default website has a priority of 0.
        /// </summary>
        /// <remarks>
        /// The default priority is 100. The relative order of two site roots with the same priority is not
        /// defined and may not be constant over the life of the site.
        /// </remarks>
        public int Priority { get; set; }

        /// <summary>
        /// Gets an optional collection of checksums for the files in the root folder.
        /// </summary>
        /// <remarks>
        /// If this is supplied then the files in the folder cannot be modified by the user.
        /// </remarks>
        public List<ChecksumFileEntry> Checksums { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public SiteRoot()
        {
            Checksums = new List<ChecksumFileEntry>();
            Priority = 100;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0}:{1} ({2} checksums)", Priority, Folder, Checksums.Count);
        }
    }
}
