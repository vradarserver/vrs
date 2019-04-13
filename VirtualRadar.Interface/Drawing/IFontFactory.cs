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

namespace VirtualRadar.Interface.Drawing
{
    /// <summary>
    /// Font handling methods.
    /// </summary>
    public interface IFontFactory
    {
        /// <summary>
        /// Returns a collection of every installed font.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IFontFamily> GetInstalledFonts();

        /// <summary>
        /// Returns the first font that matches the font style and family names passed across, with earlier
        /// names preferred over later. If nothing matches then a font family is chosen at random.
        /// </summary>
        /// <param name="fontStyle"></param>
        /// <param name="fontFamilyNames"></param>
        /// <returns></returns>
        IFontFamily GetFontFamilyOrFallback(FontStyle fontStyle, params string[] fontFamilyNames);

        /// <summary>
        /// Creates and returns a font matching the parameters passed across.
        /// </summary>
        /// <param name="fontFamily"></param>
        /// <param name="pointSize"></param>
        /// <param name="fontStyle"></param>
        /// <returns></returns>
        IFont CreateFont(IFontFamily fontFamily, float pointSize, FontStyle fontStyle);

        /// <summary>
        /// Returns a font that is at most <paramref name="largestPointSize"/> points in size but has been sized to fit within
        /// the boundaries passed across. If the text will not fit at the smallest allowable point size then it is truncated.
        /// </summary>
        /// <param name="fontFamily"></param>]
        /// <param name="fontStyle"></param>
        /// <param name="largestPointSize"></param>
        /// <param name="smallestPointSize"></param>
        /// <param name="constrainToWidth"></param>
        /// <param name="constrainToHeight"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        FontAndText GetFontForRectangle(IFontFamily fontFamily, FontStyle fontStyle, float largestPointSize, float smallestPointSize, float constrainToWidth, float constrainToHeight, string text);
    }
}
