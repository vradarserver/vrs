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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VrsDrawing = VirtualRadar.Interface.Drawing;

namespace VirtualRadar.Library.Drawing.SystemDrawing
{
    /// <summary>
    /// System.Drawing implementation of <see cref="VrsDrawing.IDrawing"/>.
    /// </summary>
    class Drawing : CommonDrawing<Graphics>
    {
        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="nativeDrawingContext"></param>
        public Drawing(Graphics nativeDrawingContext) : base(nativeDrawingContext)
        {
            ;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public override void DrawImage(VrsDrawing.IImage image, int x, int y)
        {
            if(image is ImageWrapper imageWrapper) {
                GdiPlusLock.EnforceSingleThread(() => {
                    NativeDrawingContext.DrawImage(
                        imageWrapper.NativeImage,
                        x, y
                    );
                });
            }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="fromX"></param>
        /// <param name="fromY"></param>
        /// <param name="toX"></param>
        /// <param name="toY"></param>
        public override void DrawLine(VrsDrawing.IPen pen, int fromX, int fromY, int toX, int toY)
        {
            if(pen is PenWrapper penWrapper) {
                GdiPlusLock.EnforceSingleThread(() => {
                    NativeDrawingContext.DrawLine(
                        penWrapper.NativePen,
                        fromX, fromY,
                        toX,   toY
                    );
                });
            }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="fillBrush"></param>
        /// <param name="outlinePen"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="alignment"></param>
        /// <param name="preferSpeedOverQuality"></param>
        public override void DrawText(string text, VrsDrawing.IFont font, VrsDrawing.IBrush fillBrush, VrsDrawing.IPen outlinePen, float x, float y, VrsDrawing.HorizontalAlignment alignment, bool preferSpeedOverQuality = true)
        {
            if(font is FontWrapper fontWrapper) {
                GdiPlusLock.EnforceSingleThread(() => {
                    var location = new PointF(x, y);

                    // Mono ignores these flags...
                    var stringFormat = new StringFormat() {
                        Alignment =     Convert.ToSystemDrawingStringAlignment(alignment),
                        LineAlignment = Convert.ToSystemDrawingStringAlignment(alignment),
                        FormatFlags =   StringFormatFlags.NoWrap,
                    };

                    using(var graphicsPath = new GraphicsPath()) {
                        graphicsPath.AddString(
                            text,
                            fontWrapper.NativeFont.FontFamily,
                            (int)fontWrapper.NativeFont.Style,
                            fontWrapper.NativeFont.Size,
                            location,
                            stringFormat
                        );

                        NativeDrawingContext.SmoothingMode =     SmoothingMode.AntiAlias;
                        NativeDrawingContext.InterpolationMode = InterpolationMode.HighQualityBicubic;

                        if(outlinePen is PenWrapper outlinePenWrapper) {
                            NativeDrawingContext.DrawPath(outlinePenWrapper.NativePen, graphicsPath);
                        }
                        if(fillBrush is BrushWrapper fillBrushWrapper) {
                            NativeDrawingContext.FillPath(fillBrushWrapper.NativeBrush, graphicsPath);
                        }
                    }
                });
            }
        }
    }
}
