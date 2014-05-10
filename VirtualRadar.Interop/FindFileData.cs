// Copyright © 2012 onwards, Andrew Whewell
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
using System.Runtime.InteropServices;

namespace VirtualRadar.Interop
{
    /// <summary>
    /// A wrapper around the WIN32_FIND_DATA struct.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
    [BestFitMapping(false)]
    public struct FindFileData
    {
        /// <summary>
        /// The underlying file attributes.
        /// </summary>
        internal uint dwFileAttributes;

        /// <summary>
        /// The time the file was created.
        /// </summary>
        internal uint  ftCreationTime_dwLowDateTime;
        internal uint  ftCreationTime_dwHighDateTime; 

        /// <summary>
        /// The time the file was last accessed.
        /// </summary>
        internal uint  ftLastAccessTime_dwLowDateTime;
        internal uint  ftLastAccessTime_dwHighDateTime;

        /// <summary>
        /// The time the file was last written.
        /// </summary>
        internal uint  ftLastWriteTime_dwLowDateTime;
        internal uint  ftLastWriteTime_dwHighDateTime;

        /// <summary>
        /// The high DWORD of the file size.
        /// </summary>
        internal uint nFileSizeHigh;

        /// <summary>
        /// The low DWORD of the file size.
        /// </summary>
        internal uint nFileSizeLow;

        /// <summary>
        /// Unused.
        /// </summary>
        internal uint dwReserved0;

        /// <summary>
        /// Unused.
        /// </summary>
        internal uint dwReserved1;

        /// <summary>
        /// The filename.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)]
        internal string cFileName;

        /// <summary>
        /// The short filename.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=14)]
        internal string cAlternateFileName;

        /// <summary>
        /// Gets the filename.
        /// </summary>
        public string Name { get { return cFileName; } }

        /// <summary>
        /// Gets the time that the file was last modified in UTC.
        /// </summary>
        public DateTime LastWriteTimeUtc
        {
            get
            {
                var fileTime = ((long)ftLastWriteTime_dwHighDateTime << 32) | ftLastWriteTime_dwLowDateTime;
                return DateTime.FromFileTimeUtc(fileTime);
            }
        }
    }
}
