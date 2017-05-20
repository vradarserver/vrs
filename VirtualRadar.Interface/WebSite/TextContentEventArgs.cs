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
using System.Text;
using VirtualRadar.Interface.WebServer;

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// The event args that are raised by the web site for text content events.
    /// </summary>
    public class TextContentEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the path and file of the request from root.
        /// </summary>
        /// <remarks>
        /// If the server root is /Root then for the RawUrl of '/Root' this would return '/', for the RawUrl of
        /// '/Root/' it would also return '/', for '/Root/Page.htm' it would return '/Page.htm', '/Root/Folder/' it
        /// would return '/Folder/' and so on. Query strings are stripped off and escaped characters are unescaped.
        /// </remarks>
        public string PathAndFile { get; private set; }

        /// <summary>
        /// Gets or sets the text content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the encoding of the content.
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="pathAndFile"></param>
        /// <param name="content"></param>
        /// <param name="encoding"></param>
        public TextContentEventArgs(string pathAndFile, string content, Encoding encoding)
        {
            PathAndFile = pathAndFile;
            Content = content;
            Encoding = encoding;
        }
    }
}
