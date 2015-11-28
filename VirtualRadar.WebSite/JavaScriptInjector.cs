// Copyright © 2015 onwards, Andrew Whewell
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

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// Base class for objects that can inject content into JavaScript files.
    /// </summary>
    abstract class JavaScriptInjector
    {
        /// <summary>
        /// Injects content into the JavaScript file passed across.
        /// </summary>
        /// <param name="pathAndFile"></param>
        /// <param name="content"></param>
        public void InjectIntoContent(string pathAndFile, TextContent content)
        {
            DoInjectIntoContent(pathAndFile, content);
        }

        /// <summary>
        /// When overridden by the derivee this optionally injects content into the JavaScript file.
        /// </summary>
        /// <param name="pathAndFile"></param>
        /// <param name="content"></param>
        protected abstract void DoInjectIntoContent(string pathAndFile, TextContent content);

        /// <summary>
        /// Formats a string so that it can be used in a JavaScript string literal.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        protected string FormatJavaScriptStringLiteral(string text)
        {
            return text == null ? "null" : String.Format("'{0}'", text.Replace("'", "\\'"));
        }
    }
}
