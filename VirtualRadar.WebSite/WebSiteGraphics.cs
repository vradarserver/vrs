// Copyright © 2015 onwards, Andrew Whewell
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
using InterfaceFactory;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Resources;
using VrsDrawing = VirtualRadar.Interface.Drawing;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// The default implementation of <see cref="IWebSiteGraphics"/>.
    /// </summary>
    class WebSiteGraphics : IWebSiteGraphics
    {
        private VrsDrawing.IImageFile       _ImageFile;
        private VrsDrawing.IPenFactory      _PenFactory;
        private VrsDrawing.IBrushFactory    _BrushFactory;
        private VrsDrawing.IFontFactory     _FontFactory;

        private VrsDrawing.FontStyle        _MarkerTextFontStyle = VrsDrawing.FontStyle.Bold;
        private VrsDrawing.IFontFamily      _MarkerTextFontFamily;
        private VrsDrawing.IPen             _MarkerTextOutlinePen;
        private VrsDrawing.IPen             _MarkerTextOutlinePenHiDpi;
        private VrsDrawing.IBrush           _MarkerTextFillBrush;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public WebSiteGraphics()
        {
            _ImageFile = Factory.ResolveSingleton<VrsDrawing.IImageFile>();
            _PenFactory = Factory.ResolveSingleton<VrsDrawing.IPenFactory>();
            _BrushFactory = Factory.ResolveSingleton<VrsDrawing.IBrushFactory>();
            _FontFactory = Factory.ResolveSingleton<VrsDrawing.IFontFactory>();

            _MarkerTextOutlinePen =      _PenFactory.CreatePen(0, 0, 0, 222, 4.0F);
            _MarkerTextOutlinePenHiDpi = _PenFactory.CreatePen(0, 0, 0, 222, 6.0F);
            _MarkerTextFillBrush =       _BrushFactory.CreateBrush(255, 255, 255, 255);

            _MarkerTextFontFamily = _FontFactory.GetFontFamilyOrFallback(
                _MarkerTextFontStyle,
                "Microsoft Sans Serif",
                "MS Reference Sans Serif",
                "Verdana",
                "Tahoma",
                "Roboto",
                "Droid Sans",
                "MS Sans Serif",
                "Helvetica",
                "Sans Serif",
                "Sans"
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public VrsDrawing.IImage RotateImage(VrsDrawing.IImage original, double degrees)
        {
            return _ImageFile.CloneAndDraw(original, drawing => {
                drawing.DrawImage(original, 0, 0);
                drawing.RotateAroundCentre((float)degrees);
            });
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="width"></param>
        /// <param name="centreHorizontally"></param>
        /// <returns></returns>
        public VrsDrawing.IImage WidenImage(VrsDrawing.IImage original, int width, bool centreHorizontally)
        {
            return _ImageFile.CloneAndDraw(
                _ImageFile.Create(width, original.Height),
                drawing => {
                    var x = !centreHorizontally ? 0 : (width - original.Width) / 2;
                    drawing.DrawImage(original, x, 0);
                }
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="height"></param>
        /// <param name="centreVertically"></param>
        /// <returns></returns>
        public VrsDrawing.IImage HeightenImage(VrsDrawing.IImage original, int height, bool centreVertically)
        {
            return _ImageFile.CloneAndDraw(
                _ImageFile.Create(original.Width, height),
                drawing => {
                    var y = !centreVertically ? 0 : (height - original.Height) / 2;
                    drawing.DrawImage(original, 0, y);
                }
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public VrsDrawing.IImage ResizeForHiDpi(VrsDrawing.IImage original)
        {
            return original.Resize(original.Width * 2, original.Height * 2, preferSpeedOverQuality: false);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mode"></param>
        /// <param name="zoomBackground"></param>
        /// <param name="preferSpeedOverQuality"></param>
        /// <returns></returns>
        public VrsDrawing.IImage ResizeBitmap(VrsDrawing.IImage original, int width, int height, VrsDrawing.ResizeMode mode, VrsDrawing.IBrush zoomBackground, bool preferSpeedOverQuality)
        {
            return original.Resize(width, height, mode, zoomBackground, preferSpeedOverQuality);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="webSiteAddress"></param>
        /// <param name="isIPad"></param>
        /// <param name="pathParts"></param>
        /// <returns></returns>
        public VrsDrawing.IImage CreateIPhoneSplash(string webSiteAddress, bool isIPad, List<string> pathParts)
        {
            if(!isIPad && pathParts.Where(pp => pp.Equals("file-ipad", StringComparison.OrdinalIgnoreCase)).Any()) {
                isIPad = true;
            }

            VrsDrawing.IImage result;
            float titleSize, addressSize, lineHeight;
            if(!isIPad) {
                result = _ImageFile.LoadFromByteArray(Images.IPhoneSplash);
                titleSize = 24f;
                addressSize = 12f;
                lineHeight = 40f;
            } else {
                result = _ImageFile.LoadFromByteArray(Images.IPadSplash);
                titleSize = 36f;
                addressSize = 14f;
                lineHeight = 50f;
            }

            using(Graphics graphics = Graphics.FromImage(result)) {
                graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                graphics.SmoothingMode = SmoothingMode.HighQuality;

                RectangleF titleBounds = new RectangleF(5, (result.Height - 5) - (lineHeight * 2f), result.Width - 10f, lineHeight);
                RectangleF addressBounds = new RectangleF(5, titleBounds.Bottom + 5, result.Width - 10f, lineHeight);

                // It looks like we can occasionally have an issue here under Mono when Tahoma isn't installed and Mono
                // throws an exception instead of falling back to a default font. We don't really care too much about the
                // text on the splash image so if we get exceptions here just swallow them
                try {
                    Font titleFont = _FontCache.BuildFont("Tahoma", titleSize);
                    graphics.DrawString("Virtual Radar Server", titleFont, Brushes.White, titleBounds, new StringFormat() {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Far,
                        FormatFlags = StringFormatFlags.NoWrap,
                    });

                    Font addressFont = GetFontForRectangle("Tahoma", FontStyle.Regular, addressSize, graphics, addressBounds.Width, addressBounds.Height, webSiteAddress);
                    graphics.DrawString(webSiteAddress, addressFont, Brushes.LightGray, addressBounds, new StringFormat() {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Near,
                        FormatFlags = StringFormatFlags.NoWrap,
                        Trimming = StringTrimming.EllipsisCharacter,
                    });
                } catch(Exception ex) {
                    var log = Factory.ResolveSingleton<ILog>();
                    log.WriteLine("Swallowed exception while generating {0} splash: {1}", isIPad ? "iPad" : "iPhone", ex.Message);
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="height"></param>
        /// <param name="centreX"></param>
        /// <returns></returns>
        public VrsDrawing.IImage AddAltitudeStalk(VrsDrawing.IImage original, int height, int centreX)
        {
            return _ImageFile.CloneAndDraw(
                _ImageFile.Create(original.Width, height),
                drawing => {
                    var startOfAltitudeLine = original.Height / 2;

                    // Draw the altitude line
                    drawing.DrawLine(
                        _PenFactory.Black,
                        centreX, startOfAltitudeLine,
                        centreX, height - 3
                    );

                    // Draw the X at the bottom of the altitude line
                    drawing.DrawLine(
                        _PenFactory.Black,
                        centreX - 2, height - 5,
                        centreX + 3, height - 1
                    );
                    drawing.DrawLine(
                        _PenFactory.Black,
                        centreX - 3, height - 1,
                        centreX + 2, height - 5
                    );

                    // Draw the original on top of all of the lines
                    drawing.DrawImage(original, 0, 0);
                }
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="textLines"></param>
        /// <param name="centreText"></param>
        /// <param name="isHighDpi"></param>
        /// <returns></returns>
        public VrsDrawing.IImage AddTextLines(VrsDrawing.IImage image, IEnumerable<string> textLines, bool centreText, bool isHighDpi)
        {
            var lines =          textLines.Where(tl => tl != null).ToList();
            var lineHeight =     isHighDpi ? 24f : 12f;
            var topOffset =      5f;
            var startPointSize = isHighDpi ? 20f : 10f;
            var outlinePen =     isHighDpi ? _MarkerTextOutlinePenHiDpi : _MarkerTextOutlinePen;
            var left =           centreText ? ((float)image.Width / 2.0F) : outlinePen.StrokeWidth / 2.0F;
            var top =            (image.Height - topOffset) - (lines.Count * lineHeight);
            var width =          Math.Max(0F, image.Width - outlinePen.StrokeWidth);

            return _ImageFile.CloneAndDraw(
                image,
                drawing => {
                    var lineTop = top;
                    foreach(var line in lines) {
                        var fontAndText = _FontFactory.GetFontForRectangle(_MarkerTextFontFamily, _MarkerTextFontStyle, startPointSize, 6.0F, width, lineHeight * 2F, line);

                        drawing.DrawText(
                            fontAndText.Text,
                            fontAndText.Font,
                            _MarkerTextFillBrush,
                            _MarkerTextOutlinePen,
                            left,
                            lineTop,
                            centreText ? VrsDrawing.HorizontalAlignment.Centre : VrsDrawing.HorizontalAlignment.Left,
                            preferSpeedOverQuality: false
                        );

                        lineTop += lineHeight;
                    }
                }
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public Image CreateBlankImage(int width, int height)
        {
            return width > 0 && height > 0 ? new Bitmap(width, height, PixelFormat.Format32bppArgb) : null;
        }

        /// <summary>
        /// Returns the largest font that will fit all of the text passed across into the rectangle specified.
        /// </summary>
        /// <param name="fontFamily"></param>
        /// <param name="fontStyle"></param>
        /// <param name="startSize"></param>
        /// <param name="graphics"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        private Font GetFontForRectangle(string fontFamily, FontStyle fontStyle, float startSize, Graphics graphics, float width, float height, string text)
        {
            Font result = null;

            float pointSize = startSize;
            bool fits = false;
            do {
                result = _FontCache.BuildFont(fontFamily, pointSize, fontStyle, GraphicsUnit.Point);
                SizeF size = graphics.MeasureString(text, result, new PointF(0, 0), StringFormat.GenericTypographic);
                fits = pointSize <= 4f || (size.Width <= width && size.Height <= height);
                if(!fits) pointSize -= 0.25f;
            } while(!fits);

            return result;
        }

        /// <summary>
        /// When passed the current temporary image and the image that will become to new temporary image this
        /// disposes of the old temporary image and returns the new one. It just helps with keeping the code
        /// free of checks to see whether tempImage is not null and therefore needs disposing of.
        /// </summary>
        /// <param name="tempImage"></param>
        /// <param name="newImage"></param>
        /// <returns></returns>
        public Image UseImage(Image tempImage, Image newImage)
        {
            if(tempImage != null && !Object.ReferenceEquals(tempImage, newImage)) tempImage.Dispose();
            return newImage;
        }
    }
}
