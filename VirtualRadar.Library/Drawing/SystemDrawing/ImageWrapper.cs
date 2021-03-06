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
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using InterfaceFactory;
using VrsDrawing = VirtualRadar.Interface.Drawing;

namespace VirtualRadar.Library.Drawing.SystemDrawing
{
    /// <summary>
    /// The System.Drawing implementation of <see cref="VrsDrawing.IImage"/>.
    /// </summary>
    class ImageWrapper : CommonImageWrapper<Image>
    {
        /// <summary>
        /// The font factory in use.
        /// </summary>
        private static VrsDrawing.IFontFactory _FontFactory;

        /// <summary>
        /// The font that is used to draw text.
        /// </summary>
        private static VrsDrawing.IFontFamily _PinTextFontFamily;

        /// <summary>
        /// The font style used on pin text.
        /// </summary>
        private const VrsDrawing.FontStyle _PinTextFontStyle = VrsDrawing.FontStyle.Normal;

        /// <summary>
        /// Static initialisation.
        /// </summary>
        static ImageWrapper()
        {
            _FontFactory = Factory.ResolveSingleton<VrsDrawing.IFontFactory>();
            _PinTextFontFamily = _FontFactory.GetFontFamilyOrFallback(
                _PinTextFontStyle,
                "Droid Sans",
                "MS Sans Serif",
                "Microsoft Sans Serif",
                "MS Reference Sans Serif",
                "Verdana",
                "Tahoma",
                "Roboto",
                "Helvetica",
                "Sans Serif",
                "Sans"
            );
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="nativeImage"></param>
        public ImageWrapper(Image nativeImage) : base(nativeImage)
        {
            ;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        protected override VrsDrawing.Size ExtractSizeFromNativeImage()
        {
            return new VrsDrawing.Size(NativeImage.Width, NativeImage.Height);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override VrsDrawing.IImage Clone()
        {
            VrsDrawing.IImage result = null;

            GdiPlusLock.EnforceSingleThread(() => {
                // GDI+ version of clone is a bit dodgy. We need to create a brand-new
                // copy of the image instead.
                if(NativeImage is Bitmap bitmap) {
                    result = new ImageWrapper(new Bitmap(bitmap));
                } else {
                    throw new InvalidOperationException($"Cannot clone {NativeImage.GetType().FullName} image objects");
                }
            });

            return result;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="imageFormat"></param>
        /// <returns></returns>
        public override byte[] GetImageBytes(VrsDrawing.ImageFormat imageFormat)
        {
            using(var memoryStream = new MemoryStream()) {
                GdiPlusLock.EnforceSingleThread(() => {
                    NativeImage.Save(
                        memoryStream,
                        Convert.ToSystemDrawingImageFormat(imageFormat)
                    );
                });

                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mode"></param>
        /// <param name="zoomBackground"></param>
        /// <param name="preferSpeedOverQuality"></param>
        /// <returns></returns>
        public override VrsDrawing.IImage Resize(int width, int height, VrsDrawing.ResizeMode mode = VrsDrawing.ResizeMode.Normal, VrsDrawing.IBrush zoomBackground = null, bool preferSpeedOverQuality = true)
        {
            var result = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            var zoomBackgroundBrushWrapper = zoomBackground as BrushWrapper;

            GdiPlusLock.EnforceSingleThread(() => {
                using(var graphics = Graphics.FromImage(result)) {
                    graphics.SmoothingMode =     SmoothingMode.AntiAlias;
                    graphics.InterpolationMode = preferSpeedOverQuality ? InterpolationMode.Bicubic : InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode =   PixelOffsetMode.HighQuality;

                    var newWidth = width;
                    var newHeight = height;
                    var left = 0;
                    var top = 0;

                    if(mode != VrsDrawing.ResizeMode.Stretch) {
                        var widthPercent = (double)newWidth / (double)NativeImage.Width;
                        var heightPercent = (double)newHeight / (double)NativeImage.Height;

                        switch(mode) {
                            case VrsDrawing.ResizeMode.Zoom:
                                // Resize the longest side by the smallest percentage
                                graphics.FillRectangle(zoomBackgroundBrushWrapper.NativeBrush, 0, 0, result.Width, result.Height);
                                if(widthPercent > heightPercent) {
                                    newWidth = Math.Min(newWidth, (int)(((double)NativeImage.Width * heightPercent) + 0.5));
                                } else if(heightPercent > widthPercent) {
                                    newHeight = Math.Min(newHeight, (int)(((double)NativeImage.Height * widthPercent) + 0.5));
                                }
                                break;
                            case VrsDrawing.ResizeMode.Normal:
                            case VrsDrawing.ResizeMode.Centre:
                                // Resize the smallest side by the largest percentage
                                if(widthPercent > heightPercent) {
                                    newHeight = Math.Max(newHeight, (int)(((double)NativeImage.Height * widthPercent) + 0.5));
                                } else if(heightPercent > widthPercent) {
                                    newWidth = Math.Max(newWidth, (int)(((double)NativeImage.Width * heightPercent) + 0.5));
                                }
                                break;
                        }

                        if(mode != VrsDrawing.ResizeMode.Normal) {
                            left = (width - newWidth) / 2;
                            top = (height - newHeight) / 2;
                        }
                    }

                    graphics.DrawImage(NativeImage, left, top, newWidth, newHeight);
                }
            });

            return new ImageWrapper(result);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public override VrsDrawing.IImage Rotate(float degrees)
        {
            var result = new Bitmap(NativeImage.Width, NativeImage.Height, PixelFormat.Format32bppArgb);

            GdiPlusLock.EnforceSingleThread(() => {
                using(var graphics = Graphics.FromImage(result)) {
                    var centreX = ((float)NativeImage.Width) / 2F;
                    var centreY = ((float)NativeImage.Height) / 2F;
                    graphics.TranslateTransform(centreX, centreY);
                    graphics.RotateTransform(degrees);
                    graphics.DrawImage(
                        NativeImage,
                        -centreX,
                        -centreY,
                        NativeImage.Width,
                        NativeImage.Height
                    );
                }
            });

            return new ImageWrapper(result);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="textLines"></param>
        /// <param name="centreText"></param>
        /// <param name="isHighDpi"></param>
        /// <returns></returns>
        public override VrsDrawing.IImage AddTextLines(IEnumerable<string> textLines, bool centreText, bool isHighDpi)
        {
            var lines = textLines.Where(tl => tl != null).ToList();
            var lineHeight = isHighDpi ? 24f : 12f;
            var topOffset = 5f;
            var startPointSize = isHighDpi ? 20f : 10f;
            var haloMod = isHighDpi ? 8 : 4;
            var haloTrans = isHighDpi ? 0.125f : 0.25f;
            var left = 0f;
            var top = (NativeImage.Height - topOffset) - (lines.Count * lineHeight);

            Image result = new Bitmap(NativeImage);

            GdiPlusLock.EnforceSingleThread(() => {
                using(var drawing = new Drawing(Graphics.FromImage(result))) {
                    var graphics = drawing.NativeDrawingContext;

                    var stringFormat = new StringFormat() {
                        Alignment = centreText ? StringAlignment.Center : StringAlignment.Near,
                        LineAlignment = StringAlignment.Center,
                        FormatFlags = StringFormatFlags.NoWrap,
                    };

                    graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

                    using(var path = new GraphicsPath()) {
                        var lineTop = top;
                        foreach(var line in lines) {
                            var fontAndText = _FontFactory.GetFontForRectangle(
                                drawing,
                                _PinTextFontFamily,
                                _PinTextFontStyle,
                                startPointSize,
                                4F,
                                (float)result.Width - left,
                                lineHeight * 2f,
                                line,
                                useCache: true
                            );

                            if(fontAndText.Font is FontWrapper fontWrapper) {
                                path.AddString(
                                    fontAndText.Text,
                                    fontWrapper.NativeFont.FontFamily,
                                    (int)fontWrapper.NativeFont.Style,
                                    fontWrapper.NativeFont.Size,
                                    new System.Drawing.RectangleF(0, lineTop, result.Width, lineHeight),
                                    stringFormat
                                );
                            }

                            lineTop += lineHeight;
                        }

                        var haloWidth = result.Width + (result.Width % haloMod);
                        var haloHeight = result.Height + (result.Height % haloMod);
                        using(var halo = new Bitmap(haloWidth / haloMod, haloHeight / haloMod)) {
                            using(var haloGraphics = Graphics.FromImage(halo)) {
                                using(var matrix = new Matrix(haloTrans, 0, 0, haloTrans, -haloTrans, -haloTrans)) {
                                    haloGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                                    haloGraphics.Transform = matrix;
                                    using(var pen = new Pen(Color.Gray, 1) { LineJoin = LineJoin.Round }) {
                                        haloGraphics.DrawPath(pen, path);
                                        haloGraphics.FillPath(Brushes.Black, path);
                                    }
                                }
                            }

                            graphics.SmoothingMode = SmoothingMode.AntiAlias;
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.DrawImage(halo, 1, 1, haloWidth, haloHeight);
                            graphics.DrawPath(Pens.LightGray, path);
                            graphics.FillPath(Brushes.White, path);
                        }
                    }
                }
            });

            return new ImageWrapper(result);
        }
    }
}
