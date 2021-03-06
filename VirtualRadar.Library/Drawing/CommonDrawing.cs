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
using VrsDrawing = VirtualRadar.Interface.Drawing;

namespace VirtualRadar.Library.Drawing
{
    /// <summary>
    /// The base class for <see cref="VrsDrawing.IDrawing"/> implementations.
    /// </summary>
    abstract class CommonDrawing<T> : VrsDrawing.IDrawing
    {
        /// <summary>
        /// Gets the drawing context that this is wrapping.
        /// </summary>
        internal T NativeDrawingContext { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="nativeDrawingContext"></param>
        public CommonDrawing(T nativeDrawingContext)
        {
            NativeDrawingContext = nativeDrawingContext;
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~CommonDrawing()
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
                if(NativeDrawingContext is IDisposable disposable) {
                    disposable.Dispose();
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public abstract void DrawImage(VrsDrawing.IImage image, int x, int y);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="fromX"></param>
        /// <param name="fromY"></param>
        /// <param name="toX"></param>
        /// <param name="toY"></param>
        public abstract void DrawLine(VrsDrawing.IPen pen, int fromX, int fromY, int toX, int toY);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="fillBrush"></param>
        /// <param name="outlinePen"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="alignment"></param>
        /// <param name="preferSpeedOverQuality"></param>
        public abstract void DrawText(string text, VrsDrawing.IFont font, VrsDrawing.IBrush fillBrush, VrsDrawing.IPen outlinePen, float x, float y, VrsDrawing.HorizontalAlignment alignment, bool preferSpeedOverQuality = true);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public abstract void FillRectangle(VrsDrawing.IBrush brush, int x, int y, int width, int height);
    }
}
