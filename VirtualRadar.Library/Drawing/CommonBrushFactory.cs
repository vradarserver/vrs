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
using VirtualRadar.Interface;
using VrsDrawing = VirtualRadar.Interface.Drawing;

namespace VirtualRadar.Library.Drawing
{
    /// <summary>
    /// Base implementation of <see cref="VrsDrawing.IBrushFactory"/>.
    /// </summary>
    abstract class CommonBrushFactory : VrsDrawing.IBrushFactory
    {
        /// <summary>
        /// The write lock on shared objects.
        /// </summary>
        private readonly object _SyncLock = new object();

        /// <summary>
        /// A cache of solid brushes indexed by a 32 bit color. Take a copy to read, lock and copy to write.
        /// </summary>
        private Dictionary<UInt32, VrsDrawing.IBrush> _SolidBrushCache = new Dictionary<uint, VrsDrawing.IBrush>();

        /// <summary>
        /// See interface docs.
        /// </summary>
        public VrsDrawing.IBrush Transparent { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public CommonBrushFactory()
        {
            Transparent = CreateBrush(255, 255, 255, 0, useCache: true);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <param name="alpha"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        public VrsDrawing.IBrush CreateBrush(int red, int green, int blue, int alpha, bool useCache)
        {
            if(!useCache) {
                return CreateBrushWrapper(red, green, blue, alpha, isCached: false);
            } else {
                return GetOrCreateCachedBrush(red, green, blue, alpha);
            }
        }

        /// <summary>
        /// Returns the cached brush corresponding to the parameters passed across. If no such brush
        /// has been cached then a new cached brush is created and returned.
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        private VrsDrawing.IBrush GetOrCreateCachedBrush(int red, int green, int blue, int alpha)
        {
            var key = ColourHelper.PackColour(red, green, blue, alpha);

            var map = _SolidBrushCache;
            if(!map.TryGetValue(key, out var brush)) {
                lock(_SyncLock) {
                    if(!_SolidBrushCache.TryGetValue(key, out brush)) {
                        brush = CreateBrushWrapper(red, green, blue, alpha, isCached: true);
                        map = CollectionHelper.ShallowCopy(_SolidBrushCache);
                        map.Add(key, brush);
                        _SolidBrushCache = map;
                    }
                }
            }

            return brush;
        }

        /// <summary>
        /// Creates a wrapped brush.
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <param name="alpha"></param>
        /// <param name="isCached"></param>
        /// <returns></returns>
        protected abstract VrsDrawing.IBrush CreateBrushWrapper(int red, int green, int blue, int alpha, bool isCached);
    }
}
