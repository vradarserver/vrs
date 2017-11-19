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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// A class that carries the content of a text file and its encoding.
    /// </summary>
    public class TextContent
    {
        private string _Content = "";
        /// <summary>
        /// Gets or sets the text content of a file without the encoding preamble.
        /// </summary>
        /// <remarks>
        /// Setting this property always sets  </remarks>
        public string Content
        {
            get { return _Content; }
            set {
                _Content = value;
                IsDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets a flag indicating that the <see cref="Content"/> may have changed.
        /// </summary>
        /// <remarks>
        /// Always set to true by the setter for <see cref="Content"/>.
        /// </remarks>
        public bool IsDirty { get; set; }

        /// <summary>
        /// Gets or sets the best guess at the encoding of the original file.
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// Gets or sets a value indicating that the original file had a BOM preamble.
        /// </summary>
        public bool HadPreamble { get; set; }

        /// <summary>
        /// Returns the <see cref="Content"/> encoded as an array of bytes with an optional
        /// preamble attached (but only if the original file had the preamble).
        /// </summary>
        /// <param name="includePreamble">True if the preamble is to be included. Ignored if HadPreamble is false.</param>
        /// <returns></returns>
        public byte[] GetBytes(bool includePreamble = true)
        {
            var preamble = includePreamble && HadPreamble ? Encoding.GetPreamble() : new byte[]{};
            var result = preamble.Concat(Encoding.GetBytes(Content));

            return result.ToArray();
        }

        /// <summary>
        /// Builds a <see cref="TextContent"/> from a positionable stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="leaveOpen"></param>
        /// <returns></returns>
        public static TextContent Load(Stream stream, bool leaveOpen = false)
        {
            var result = new TextContent();

            using(var streamReader = new StreamReader(stream, Encoding.UTF8, true, 1024, leaveOpen)) {
                var currentStreamPosition = stream.Position;

                result.Content = streamReader.ReadToEnd();
                result.IsDirty = false;
                result.Encoding = streamReader.CurrentEncoding;

                stream.Position = currentStreamPosition;
                var preamble = streamReader.CurrentEncoding.GetPreamble();
                if(preamble.Length > 0 && stream.Length >= preamble.Length) {
                    var filePreamble = new byte[preamble.Length];
                    var bytesRead = stream.Read(filePreamble, 0, filePreamble.Length);
                    result.HadPreamble = bytesRead == preamble.Length && preamble.SequenceEqual(filePreamble);
                }
            }

            return result;
        }

        /// <summary>
        /// Builds a <see cref="TextContent"/> from bytes that might start with a BOM preamble, i.e. bytes read
        /// from a file.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static TextContent Load(byte[] bytes)
        {
            using(var stream = new MemoryStream(bytes)) {
                return Load(stream);
            }
        }
    }
}
