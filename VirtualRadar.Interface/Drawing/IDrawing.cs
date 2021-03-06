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

namespace VirtualRadar.Interface.Drawing
{
    /// <summary>
    /// The interface for an object that can perform drawing operations on an image.
    /// </summary>
    public interface IDrawing : IDisposable
    {
        /// <summary>
        /// Draws an image at the point passed across. The image is not resized.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void DrawImage(IImage image, int x, int y);

        /// <summary>
        /// Draws a line between two points using the supplied pen.
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="fromX"></param>
        /// <param name="fromY"></param>
        /// <param name="toX"></param>
        /// <param name="toY"></param>
        void DrawLine(IPen pen, int fromX, int fromY, int toX, int toY);

        /// <summary>
        /// Draws text onto the image.
        /// </summary>
        /// <param name="text">The text to draw.</param>
        /// <param name="font">The font to use.</param>
        /// <param name="fillBrush">The fill brush - pass null for no fill.</param>
        /// <param name="outlinePen">The outline pen - pass null for no outline.</param>
        /// <param name="x">Where to start drawing the text.</param>
        /// <param name="y">Where to start drawing the text.</param>
        /// <param name="alignment">How to align the text at the draw coordinate.</param>
        /// <param name="preferSpeedOverQuality">A hint to the library as to whether speed should be preferred over quality.</param>
        void DrawText(string text, IFont font, IBrush fillBrush, IPen outlinePen, float x, float y, HorizontalAlignment alignment, bool preferSpeedOverQuality = true);

        /// <summary>
        /// Fills a rectangle with a brush.
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        void FillRectangle(IBrush brush, int x, int y, int width, int height);
    }
}
