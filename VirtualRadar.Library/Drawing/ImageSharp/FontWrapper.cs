// Copyright © 2019 onwards, Andrew Whewell
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
using SixLabors.Fonts;
using VrsDrawing = VirtualRadar.Interface.Drawing;

namespace VirtualRadar.Library.Drawing.ImageSharp
{
    /// <summary>
    /// ImageSharp implementation of <see cref="VrsDrawing.IFont"/>.
    /// </summary>
    class FontWrapper : CommonFontWrapper<Font>
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        public override string FontFamilyName { get; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public override float PointSize => NativeFont.Size;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public override VrsDrawing.FontStyle FontStyle { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="font"></param>
        /// <param name="isCached"></param>
        public FontWrapper(Font font, bool isCached) : base(font, isCached)
        {
            FontFamilyName = font.Family.Name;
            FontStyle = font.Bold   ? VrsDrawing.FontStyle.Bold
                      : font.Italic ? VrsDrawing.FontStyle.Italic
                      :               VrsDrawing.FontStyle.Normal;
        }
    }
}
