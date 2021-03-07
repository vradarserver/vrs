// Copyright © 2019 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Collections.Generic;
using VirtualRadar.Interface;
using VrsDrawing = VirtualRadar.Interface.Drawing;

namespace VirtualRadar.Library.Drawing
{
    abstract class CommonPenFactory : VrsDrawing.IPenFactory
    {
        /// <summary>
        /// The write lock on shared objects.
        /// </summary>
        private readonly object _SyncLock = new object();

        /// <summary>
        /// A cache of pens. Take a copy to read, lock and copy to write.
        /// </summary>
        private Dictionary<PenKey, VrsDrawing.IPen> _PenCache = new Dictionary<PenKey, VrsDrawing.IPen>();

        /// <summary>
        /// See interface docs.
        /// </summary>
        public VrsDrawing.IPen Black { get; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public VrsDrawing.IPen LightGray { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public CommonPenFactory()
        {
            Black =     CreatePenWrapper(0,   0,   0,   255, 1.0F, isCached: true);
            LightGray = CreatePenWrapper(211, 211, 211, 255, 1.0F, isCached: true);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <param name="alpha"></param>
        /// <param name="strokeWidth"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        public VrsDrawing.IPen CreatePen(int red, int green, int blue, int alpha, float strokeWidth, bool useCache)
        {
            if(!useCache) {
                return CreatePenWrapper(red, green, blue, alpha, strokeWidth, isCached: false);
            } else {
                return GetOrCreateCachedPen(red, green, blue, alpha, strokeWidth);
            }
        }

        /// <summary>
        /// Returns the cached pen corresponding to the parameters passed across or creates and returns a new
        /// cached pen.
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <param name="alpha"></param>
        /// <param name="strokeWidth"></param>
        /// <returns></returns>
        private VrsDrawing.IPen GetOrCreateCachedPen(int red, int green, int blue, int alpha, float strokeWidth)
        {
            var key = new PenKey(red, green, blue, alpha, strokeWidth);

            var map = _PenCache;
            if(!map.TryGetValue(key, out var pen)) {
                lock(_SyncLock) {
                    if(!_PenCache.TryGetValue(key, out pen)) {
                        pen = CreatePenWrapper(red, green, blue, alpha, strokeWidth, isCached: true);
                        map = CollectionHelper.ShallowCopy(_PenCache);
                        map.Add(key, pen);
                        _PenCache = map;
                    }
                }
            }

            return pen;
        }

        /// <summary>
        /// Creates a wrapped pen.
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <param name="alpha"></param>
        /// <param name="strokeWidth"></param>
        /// <param name="isCached"></param>
        /// <returns></returns>
        protected abstract VrsDrawing.IPen CreatePenWrapper(int red, int green, int blue, int alpha, float strokeWidth, bool isCached);
    }
}
