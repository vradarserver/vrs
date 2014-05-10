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
using System.Drawing;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// A threadsafe class that retains instances of fonts for <see cref="ImagePage"/>.
    /// </summary>
    /// <remarks>
    /// The .NET code doesn't really need this, it originally just created and destroyed fonts as it needed
    /// them. However Mono does need it, it uses libcairo and that crashes all over the place when its asked
    /// to delete fonts repeatedly.
    /// </remarks>
    class FontCache : IDisposable
    {
        /// <summary>
        /// An immutable class describing an instance of a font.
        /// </summary>
        class FontDescriptor
        {
            public string FamilyName { get; private set; }
            public float EmSize { get; private set; }
            public FontStyle FontStyle { get; private set; }
            public GraphicsUnit GraphicsUnit { get; private set; }

            public FontDescriptor(string familyName, float emSize, FontStyle fontStyle, GraphicsUnit graphicsUnit)
            {
                FamilyName = familyName;
                EmSize = emSize;
                FontStyle = fontStyle;
                GraphicsUnit = graphicsUnit;
            }

            public override bool Equals(object obj)
            {
                bool result = object.ReferenceEquals(this, obj);
                if(!result) {
                    FontDescriptor other = obj as FontDescriptor;
                    result = EmSize == other.EmSize && FontStyle == other.FontStyle && GraphicsUnit == other.GraphicsUnit && FamilyName == other.FamilyName;
                }

                return result;
            }

            public override int GetHashCode()
            {
                return EmSize.GetHashCode();
            }

            public override string ToString()
            {
                return String.Format("{0} {1} {2} {3}", FamilyName, EmSize, GraphicsUnit, FontStyle);
            }
        }

        /// <summary>
        /// The cache of known fonts.
        /// </summary>
        private Dictionary<FontDescriptor, Font> _Fonts = new Dictionary<FontDescriptor,Font>();

        /// <summary>
        /// The lock used to allow access from multiple threads.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~FontCache()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes of the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                lock(_SyncLock) {
                    foreach(var font in _Fonts.Values) {
                        font.Dispose();
                    }
                    _Fonts.Clear();
                }
            }
        }

        /// <summary>
        /// Returns the font requested.
        /// </summary>
        /// <param name="familyName"></param>
        /// <param name="emSize"></param>
        /// <returns></returns>
        public Font BuildFont(string familyName, float emSize)
        {
            return BuildFont(familyName, emSize, FontStyle.Regular, GraphicsUnit.Point);
        }

        /// <summary>
        /// Returns the font requested.
        /// </summary>
        /// <param name="familyName"></param>
        /// <param name="emSize"></param>
        /// <param name="fontStyle"></param>
        /// <param name="graphicsUnit"></param>
        /// <returns></returns>
        public Font BuildFont(string familyName, float emSize, FontStyle fontStyle, GraphicsUnit graphicsUnit)
        {
            var descriptor = new FontDescriptor(familyName, emSize, fontStyle, graphicsUnit);
            Font result;

            lock(_SyncLock) {
                if(!_Fonts.TryGetValue(descriptor, out result)) {
                    result = new Font(familyName, emSize, fontStyle, graphicsUnit);
                    _Fonts.Add(descriptor, result);
                }
            }

            return result;
        }
    }
}
