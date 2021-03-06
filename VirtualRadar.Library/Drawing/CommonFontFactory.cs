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
using VirtualRadar.Interface;
using VrsDrawing = VirtualRadar.Interface.Drawing;

namespace VirtualRadar.Library.Drawing
{
    /// <summary>
    /// Base implementation of <see cref="VrsDrawing.IFontFactory"/>.
    /// </summary>
    abstract class CommonFontFactory : VrsDrawing.IFontFactory
    {
        /// <summary>
        /// The write lock on shared objects.
        /// </summary>
        private readonly object _SyncLock = new object();

        /// <summary>
        /// A cache of fonts indexed by key. Take a copy to read, lock and copy to write.
        /// </summary>
        private Dictionary<FontKey, VrsDrawing.IFont> _FontCache = new Dictionary<FontKey, VrsDrawing.IFont>();

        /// <summary>
        /// Gets a value indicating that the library will truncate strings if they are too long.
        /// </summary>
        protected abstract bool ImageLibraryDrawTextWillTruncate { get; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="fontFamily"></param>
        /// <param name="pointSize"></param>
        /// <param name="fontStyle"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        public VrsDrawing.IFont CreateFont(VrsDrawing.IFontFamily fontFamily, float pointSize, VrsDrawing.FontStyle fontStyle, bool useCache)
        {
            if(!useCache) {
                return CreateFontWrapper(fontFamily, pointSize, fontStyle, isCached: false);
            } else {
                return GetOrCreateCachedFont(fontFamily, pointSize, fontStyle);
            }
        }

        /// <summary>
        /// Returns a font out of the cache or, if no such font exists, creates and caches a new font for the
        /// properties passed across.
        /// </summary>
        /// <param name="fontFamily"></param>
        /// <param name="pointSize"></param>
        /// <param name="fontStyle"></param>
        /// <returns></returns>
        private VrsDrawing.IFont GetOrCreateCachedFont(VrsDrawing.IFontFamily fontFamily, float pointSize, VrsDrawing.FontStyle fontStyle)
        {
            var key = new FontKey(fontFamily.Name, pointSize, fontStyle);

            var map = _FontCache;
            if(!map.TryGetValue(key, out var font)) {
                lock(_SyncLock) {
                    if(!_FontCache.TryGetValue(key, out font)) {
                        font = CreateFontWrapper(fontFamily, pointSize, fontStyle, isCached: true);
                        map = CollectionHelper.ShallowCopy(_FontCache);
                        map.Add(key, font);
                        _FontCache = map;
                    }
                }
            }

            return font;
        }

        /// <summary>
        /// Creates a new instance of a font wrapper.
        /// </summary>
        /// <param name="fontFamily"></param>
        /// <param name="pointSize"></param>
        /// <param name="fontStyle"></param>
        /// <param name="isCached"></param>
        /// <returns></returns>
        protected abstract VrsDrawing.IFont CreateFontWrapper(VrsDrawing.IFontFamily fontFamily, float pointSize, VrsDrawing.FontStyle fontStyle, bool isCached);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="fontStyle"></param>
        /// <param name="fontFamilyNames"></param>
        /// <returns></returns>
        public VrsDrawing.IFontFamily GetFontFamilyOrFallback(VrsDrawing.FontStyle fontStyle, params string[] fontFamilyNames)
        {
            VrsDrawing.IFontFamily result = null;

            var installedFontFamilies = GetInstalledFonts().ToArray();
            try {
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
            } finally {
                foreach(var unusedFontFamily in installedFontFamilies.Where(r => !Object.ReferenceEquals(r, result))) {
                    unusedFontFamily.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="drawing"></param>
        /// <param name="fontFamily"></param>
        /// <param name="fontStyle"></param>
        /// <param name="largestPointSize"></param>
        /// <param name="smallestPointSize"></param>
        /// <param name="constrainToWidth"></param>
        /// <param name="constrainToHeight"></param>
        /// <param name="text"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        public VrsDrawing.FontAndText GetFontForRectangle(VrsDrawing.IDrawing drawing, VrsDrawing.IFontFamily fontFamily, VrsDrawing.FontStyle fontStyle, float largestPointSize, float smallestPointSize, float constrainToWidth, float constrainToHeight, string text, bool useCache)
        {
            var textBuffer = new StringBuilder(text);

            var pointSize = largestPointSize;
            bool trySmaller;
            VrsDrawing.IFont font;
            do {
                font = CreateFont(fontFamily, pointSize, fontStyle, useCache);
                MeasureText(drawing, font, textBuffer.ToString(), out var width, out var height);

                trySmaller = width > constrainToWidth || height > constrainToHeight;
                if(trySmaller) {
                    if(pointSize > smallestPointSize) {
                        pointSize = Math.Max(smallestPointSize, pointSize - 0.25F);
                    } else {
                        if(textBuffer.Length == 0 || ImageLibraryDrawTextWillTruncate) {
                            trySmaller = false;
                        } else {
                            textBuffer.Remove(textBuffer.Length - 1, 1);
                        }
                    }
                }
            } while(trySmaller);

            return new VrsDrawing.FontAndText(font, text.ToString());
        }

        /// <summary>
        /// Measures a string given a font and a drawing context.
        /// </summary>
        /// <param name="drawing"></param>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        protected abstract void MeasureText(VrsDrawing.IDrawing drawing, VrsDrawing.IFont font, string text, out float width, out float height);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<VrsDrawing.IFontFamily> GetInstalledFonts();
    }
}
