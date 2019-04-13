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
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using VrsDrawing = VirtualRadar.Interface.Drawing;

namespace VirtualRadar.Library.Drawing
{
    class Drawing : VrsDrawing.IDrawing
    {
        /// <summary>
        /// Gets the ImageSharp processing context that is wrapped by this object.
        /// </summary>
        public IImageProcessingContext<Rgba32> NativeContext { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="context"></param>
        public Drawing(IImageProcessingContext<Rgba32> context)
        {
            NativeContext = context;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawImage(VrsDrawing.IImage image, int x, int y)
        {
            if(image is ImageWrapper imageWrapper) {
                NativeContext.DrawImage(
                    imageWrapper.NativeImage,
                    new Point(x, y),
                    1.0F
                );
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="fromX"></param>
        /// <param name="fromY"></param>
        /// <param name="toX"></param>
        /// <param name="toY"></param>
        public void DrawLine(VrsDrawing.IPen pen, int fromX, int fromY, int toX, int toY)
        {
            if(pen is PenWrapper penWrapper) {
                NativeContext.DrawLines(
                    penWrapper.NativePen,
                    new PointF(fromX, fromY),
                    new PointF(toX, toY)
                );
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="degrees"></param>
        public void RotateAroundCentre(float degrees)
        {
            NativeContext.Rotate(degrees);
        }

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
        public void DrawText(string text, VrsDrawing.IFont font, VrsDrawing.IBrush fillBrush, VrsDrawing.IPen outlinePen, float x, float y, VrsDrawing.HorizontalAlignment alignment, bool preferSpeedOverQuality = true)
        {
            if(font is FontWrapper fontWrapper) {
                var options = new TextGraphicsOptions(enableAntialiasing: !preferSpeedOverQuality) {
                    HorizontalAlignment =   Convert.ToImageSharpHorizontalAlignment(alignment),
                    ApplyKerning =          !preferSpeedOverQuality,
                };
                var location = new PointF(x, y);

                if(outlinePen is PenWrapper outlinePenWrapper) {
                    NativeContext.DrawText(options, text, fontWrapper.NativeFont, outlinePenWrapper.NativePen, location);
                }
                if(fillBrush is BrushWrapper fillBrushWrapper) {
                    NativeContext.DrawText(options, text, fontWrapper.NativeFont, fillBrushWrapper.NativeBrush, location);
                }
            }
        }
    }
}
