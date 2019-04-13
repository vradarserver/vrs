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

namespace VirtualRadar.Library.Drawing
{
    /// <summary>
    /// The default ImageSharp implementation of <see cref="VrsDrawing.IFontFactory"/>.
    /// </summary>
    class FontFactory : VrsDrawing.IFontFactory
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="fontFamily"></param>
        /// <param name="pointSize"></param>
        /// <param name="fontStyle"></param>
        /// <returns></returns>
        public VrsDrawing.IFont CreateFont(VrsDrawing.IFontFamily fontFamily, float pointSize, VrsDrawing.FontStyle fontStyle)
        {
            return new FontWrapper(
                SystemFonts.CreateFont(fontFamily.Name, pointSize, Convert.ToImageSharpFontStyle(fontStyle))
            );
        }

        static FontFamilyWrapper[] _InstalledFonts;
        static object _InstalledFontsBuildSyncLock = new object();

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<VrsDrawing.IFontFamily> GetInstalledFonts()
        {
            var result = _InstalledFonts;
            if(result == null) {
                lock(_InstalledFontsBuildSyncLock) {
                    if(_InstalledFonts == null) {
                        _InstalledFonts = SystemFonts.Families.Select(r => new FontFamilyWrapper(r)).ToArray();
                    }
                }
                result = _InstalledFonts;
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="fontStyle"></param>
        /// <param name="fontFamilyNames"></param>
        /// <returns></returns>
        public VrsDrawing.IFontFamily GetFontFamilyOrFallback(VrsDrawing.FontStyle fontStyle, params string[] fontFamilyNames)
        {
            VrsDrawing.IFontFamily result = null;

            var installedFontFamilies = GetInstalledFonts();

            foreach(var familyName in fontFamilyNames) {
                result = installedFontFamilies.FirstOrDefault(r =>
                    String.Equals(familyName, r.Name, StringComparison.OrdinalIgnoreCase)
                    && r.AvailableStyles.Contains(fontStyle)
                );
                if(result != null) {
                    break;
                }
            }

            if(result == null) {
                result = installedFontFamilies.FirstOrDefault(r => r.AvailableStyles.Contains(fontStyle));
            }

            if(result == null) {
                result = installedFontFamilies.FirstOrDefault();
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="fontFamily"></param>
        /// <param name="fontStyle"></param>
        /// <param name="largestPointSize"></param>
        /// <param name="smallestPointSize"></param>
        /// <param name="constrainToWidth"></param>
        /// <param name="constrainToHeight"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public VrsDrawing.FontAndText GetFontForRectangle(VrsDrawing.IFontFamily fontFamily, VrsDrawing.FontStyle fontStyle, float largestPointSize, float smallestPointSize, float constrainToWidth, float constrainToHeight, string text)
        {
            var textBuffer = new StringBuilder(text);

            var pointSize = largestPointSize;
            bool trySmaller;
            Font font;
            do {
                font = SystemFonts.CreateFont(fontFamily.Name, pointSize, Convert.ToImageSharpFontStyle(fontStyle));
                var size = TextMeasurer.Measure(textBuffer.ToString(), new RendererOptions(font));

                trySmaller = size.Width > constrainToWidth || size.Height > constrainToHeight;
                if(trySmaller) {
                    if(pointSize > smallestPointSize) {
                        pointSize = Math.Max(smallestPointSize, pointSize - 0.25F);
                    } else {
                        // ImageSharp crashes if it's asked to truncate text so we need to do that for it. Note that GDI+
                        // always truncated from the end so I've duplicated that behaviour here.
                        if(textBuffer.Length == 0) {
                            trySmaller = false;
                        } else {
                            textBuffer.Remove(textBuffer.Length - 1, 1);
                        }
                    }
                }
            } while(trySmaller);

            return new VrsDrawing.FontAndText(new FontWrapper(font), text.ToString());
        }
    }
}
