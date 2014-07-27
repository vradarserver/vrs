// Copyright © 2014 onwards, Andrew Whewell
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
using System.Runtime.InteropServices;
using System.Text;

namespace VirtualRadar.Interop
{
    /// <summary>
    /// See Microsoft's documentation for the HDITEM struct.
    /// </summary>
    /// <remarks>
    /// At the time of writing this was at
    /// http://msdn.microsoft.com/en-gb/library/windows/desktop/bb775247(v=vs.85).aspx
    /// but don't be surprised if Microsoft have moved it.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct HDITEM
    {
        /// <summary>
        /// See MSDN documentation.
        /// </summary>
        public const Int32 HDI_FORMAT =     0x0004;

        /// <summary>
        /// See MSDN documentation.
        /// </summary>
        public const Int32 HDF_LEFT =       0x0000;

        /// <summary>
        /// See MSDN documentation.
        /// </summary>
        public const Int32 HDF_STRING =     0x4000;

        /// <summary>
        /// See MSDN documentation.
        /// </summary>
        public const Int32 HDF_SORTUP =     0x0400;

        /// <summary>
        /// See MSDN documentation.
        /// </summary>
        public const Int32 HDF_SORTDOWN =   0x0200;

        /// <summary>
        /// See MSDN documentation.
        /// </summary>
        public const Int32 HDM_FIRST =      0x1200;

        /// <summary>
        /// See MSDN documentation.
        /// </summary>
        public const Int32 HDM_GETITEM =    HDM_FIRST + 11;

        /// <summary>
        /// See MSDN documentation.
        /// </summary>
        public const Int32 HDM_SETITEM =    HDM_FIRST + 12;

        /// <summary>
        /// See MSDN documentation.
        /// </summary>
        public Int32 mask;

        /// <summary>
        /// See MSDN documentation.
        /// </summary>
        public Int32 cxy;

        /// <summary>
        /// See MSDN documentation.
        /// </summary>
        [MarshalAs(UnmanagedType.LPTStr)]
        public String pszText;

        /// <summary>
        /// See MSDN documentation.
        /// </summary>
        public IntPtr hbm;

        /// <summary>
        /// See MSDN documentation.
        /// </summary>
        public Int32 cchTextMax;

        /// <summary>
        /// See MSDN documentation.
        /// </summary>
        public Int32 fmt;

        /// <summary>
        /// See MSDN documentation.
        /// </summary>
        public Int32 lParam;

        /// <summary>
        /// See MSDN documentation.
        /// </summary>
        public Int32 iImage;

        /// <summary>
        /// See MSDN documentation.
        /// </summary>
        public Int32 iOrder;
    };
}
