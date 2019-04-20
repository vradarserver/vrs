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
using VrsDrawing = VirtualRadar.Interface.Drawing;

namespace VirtualRadar.Library.Drawing
{
    /// <summary>
    /// The base class for <see cref="VrsDrawing.IImage"/> image wrappers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    abstract class CommonImageWrapper<T> : VrsDrawing.IImage
    {
        /// <summary>
        /// Gets the native image that's being wrapped by this object.
        /// </summary>
        internal T NativeImage { get; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public VrsDrawing.Size Size { get; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int Width => Size.Width;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int Height => Size.Height;

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public abstract VrsDrawing.IImage Clone();

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="nativeImage"></param>
        public CommonImageWrapper(T nativeImage)
        {
            NativeImage = nativeImage;
            Size = ExtractSizeFromNativeImage();
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~CommonImageWrapper()
        {
            Dispose(false);
        }

        /// <summary>
        /// See interface docs.
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
                if(NativeImage is IDisposable disposable) {
                    disposable.Dispose();
                }
            }
        }

        /// <summary>
        /// Returns the size of <see cref="NativeImage"/>.
        /// </summary>
        /// <returns></returns>
        protected abstract VrsDrawing.Size ExtractSizeFromNativeImage();

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="imageFormat"></param>
        /// <returns></returns>
        public abstract byte[] GetImageBytes(VrsDrawing.ImageFormat imageFormat);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mode"></param>
        /// <param name="zoomBackground"></param>
        /// <param name="preferSpeedOverQuality"></param>
        /// <returns></returns>
        public abstract VrsDrawing.IImage Resize(int width, int height, VrsDrawing.ResizeMode mode = VrsDrawing.ResizeMode.Normal, VrsDrawing.IBrush zoomBackground = null, bool preferSpeedOverQuality = true);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public abstract VrsDrawing.IImage Rotate(float degrees);
    }
}
