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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// A collection of assorted methods that help make dealing with Javascript a bit easier.
    /// </summary>
    public static class JavascriptHelper
    {
        /// <summary>
        /// The number of .NET ticks that represent midnight January 1st 1970.
        /// </summary>
        private const long DotNetTicksMidnight1stJanuary1970 = 621355968000000000L;

        /// <summary>
        /// The number of .NET ticks in a Javascript tick.
        /// </summary>
        private const long DotNetTicksPerMillisecond = 10000L;

        /// <summary>
        /// Returns the Javascript equivalent of the .NET ticks passed across.
        /// </summary>
        /// <param name="dotNetTicks"></param>
        /// <returns></returns>
        public static long ToJavascriptTicks(long dotNetTicks)
        {
            return (dotNetTicks - DotNetTicksMidnight1stJanuary1970) / DotNetTicksPerMillisecond;
        }

        /// <summary>
        /// Returns the Javascript ticks that represent the .NET date passed across.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static long ToJavascriptTicks(DateTime date)
        {
            return ToJavascriptTicks(date.Ticks);
        }

        /// <summary>
        /// Formats a string so that it can be used in a JavaScript string literal.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string FormatStringLiteral(string text)
        {
            return text == null ? "null" : String.Format("'{0}'", text.Replace("'", "\\'"));
        }
    }
}
