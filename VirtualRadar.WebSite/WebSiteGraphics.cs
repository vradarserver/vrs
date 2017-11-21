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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Resources;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// The default implementation of <see cref="IWebSiteGraphics"/>.
    /// </summary>
    class WebSiteGraphics : IWebSiteGraphics
    {
        #region Fields
        /// <summary>
        /// The cache of known fonts.
        /// </summary>
        private static FontCache _FontCache = new FontCache();

        /// <summary>
        /// True if we're running under Mono.
        /// </summary>
        private bool _IsMono;

        /// <summary>
        /// The object used to synchronise access to the Graphics object.
        /// </summary>
        private object _SyncLock = new object();
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public WebSiteGraphics()
        {
            _IsMono = Factory.Singleton.ResolveSingleton<IRuntimeEnvironment>().IsMono;
        }
        #endregion

        #region Basic image handling - RotateImage, WidenImage, HeightenImage, ResizeForHiDpi, ResizeBitmap
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public Image RotateImage(Image original, double degrees)
        {
            var result = new Bitmap(original.Width, original.Height, PixelFormat.Format32bppArgb);

            lock(_SyncLock) {
                using(Graphics graphics = Graphics.FromImage(result)) {
                    float centreX = ((float)original.Width) / 2f;
                    float centreY = ((float)original.Height) / 2f;
                    graphics.TranslateTransform(centreX, centreY);
                    graphics.RotateTransform((float)degrees);
                    graphics.DrawImage(original, -centreX, -centreY, original.Width, original.Height);
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="width"></param>
        /// <param name="centreHorizontally"></param>
        /// <returns></returns>
        public Image WidenImage(Image original, int width, bool centreHorizontally)
        {
            var result = new Bitmap(width, original.Height, PixelFormat.Format32bppArgb);

            lock(_SyncLock) {
                using(Graphics graphics = Graphics.FromImage(result)) {
                    int x = !centreHorizontally ? 0 : (width - original.Width) / 2;
                    graphics.DrawImage(original, x, 0, original.Width, original.Height);
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="height"></param>
        /// <param name="centreVertically"></param>
        /// <returns></returns>
        public Image HeightenImage(Image original, int height, bool centreVertically)
        {
            var result = new Bitmap(original.Width, height, PixelFormat.Format32bppArgb);

            lock(_SyncLock) {
                using(Graphics graphics = Graphics.FromImage(result)) {
                    int y = !centreVertically ? 0 : (height - original.Height) / 2;
                    graphics.DrawImage(original, 0, y, original.Width, original.Height);
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public Image ResizeForHiDpi(Image original)
        {
            var result = new Bitmap(original.Width * 2, original.Height * 2, PixelFormat.Format32bppArgb);

            lock(_SyncLock) {
                using(var graphics = Graphics.FromImage(result)) {
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.NearestNeighbor;     // <-- need this to preserve sharp edges on the doubled-up image
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.DrawImage(original, 0, 0, result.Width, result.Height);
                }
            }

            return result;
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
        public Bitmap ResizeBitmap(Bitmap original, int width, int height, ResizeImageMode mode, Brush zoomBackground, bool preferSpeedOverQuality)
        {
            Bitmap result = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            lock(_SyncLock) {
                using(Graphics graphics = Graphics.FromImage(result)) {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.InterpolationMode = preferSpeedOverQuality ? InterpolationMode.Bicubic : InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    int newWidth = width, newHeight = height, left = 0, top = 0;
                    if(mode != ResizeImageMode.Stretch) {
                        double widthPercent = (double)newWidth / (double)original.Width;
                        double heightPercent = (double)newHeight / (double)original.Height;

                        switch(mode) {
                            case ResizeImageMode.Zoom:
                                // Resize the longest side by the smallest percentage
                                graphics.FillRectangle(zoomBackground, 0, 0, result.Width, result.Height);
                                if(widthPercent > heightPercent)        newWidth = Math.Min(newWidth, (int)(((double)original.Width * heightPercent) + 0.5));
                                else if(heightPercent > widthPercent)   newHeight = Math.Min(newHeight, (int)(((double)original.Height * widthPercent) + 0.5));
                                break;
                            case ResizeImageMode.Normal:
                            case ResizeImageMode.Centre:
                                // Resize the smallest side by the largest percentage
                                if(widthPercent > heightPercent)        newHeight = Math.Max(newHeight, (int)(((double)original.Height * widthPercent) + 0.5));
                                else if(heightPercent > widthPercent)   newWidth = Math.Max(newWidth, (int)(((double)original.Width * heightPercent) + 0.5));
                                break;
                        }

                        if(mode != ResizeImageMode.Normal) {
                            left = (width - newWidth) / 2;
                            top = (height - newHeight) / 2;
                        }
                    }

                    graphics.DrawImage(original, left, top, newWidth, newHeight);
                }
            }

            return result;
        }
        #endregion

        #region High level graphics - CreateIPhoneSplash, AddAltitudeStalk, AddTextLines
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="webSiteAddress"></param>
        /// <param name="isIPad"></param>
        /// <param name="pathParts"></param>
        /// <returns></returns>
        public Image CreateIPhoneSplash(string webSiteAddress, bool isIPad, List<string> pathParts)
        {
            if(!isIPad && pathParts.Where(pp => pp.Equals("file-ipad", StringComparison.OrdinalIgnoreCase)).Any()) isIPad = true;

            Image result;
            float titleSize, addressSize, lineHeight;
            if(!isIPad) {
                result = Images.Clone_IPhoneSplash();
                titleSize = 24f;
                addressSize = 12f;
                lineHeight = 40f;
            } else {
                result = Images.Clone_IPadSplash();
                titleSize = 36f;
                addressSize = 14f;
                lineHeight = 50f;
            }

            lock(_SyncLock) {
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
                        var log = Factory.Singleton.ResolveSingleton<ILog>();
                        log.WriteLine("Swallowed exception while generating {0} splash: {1}", isIPad ? "iPad" : "iPhone", ex.Message);
                    }
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
        public Image AddAltitudeStalk(Image original, int height, int centreX)
        {
            var result = new Bitmap(original.Width, height, PixelFormat.Format32bppArgb);

            lock(_SyncLock) {
                using(Graphics graphics = Graphics.FromImage(result)) {
                    int startOfAltitudeLine = original.Height / 2;

                    // Draw the altitude line
                    graphics.DrawLine(Pens.Black, new Point(centreX, startOfAltitudeLine), new Point(centreX, height - 3));

                    // Draw the X at the bottom of the altitude line
                    graphics.DrawLine(Pens.Black, new Point(centreX - 2, height - 5), new Point(centreX + 3, height - 1));
                    graphics.DrawLine(Pens.Black, new Point(centreX - 3, height - 1), new Point(centreX + 2, height - 5));

                    // Draw the original on top of all of the lines
                    graphics.DrawImage(original, 0, 0, original.Width, original.Height);
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="textLines"></param>
        /// <param name="centreText"></param>
        /// <param name="isHighDpi"></param>
        /// <returns></returns>
        public Image AddTextLines(Image image, IEnumerable<string> textLines, bool centreText, bool isHighDpi)
        {
            var lines = textLines.Where(tl => tl != null).ToList();
            var lineHeight = isHighDpi ? 24f : 12f;
            var topOffset = 5f;
            var startPointSize = isHighDpi ? 20f : 10f;
            var haloMod = isHighDpi ? 8 : 4;
            var haloTrans = isHighDpi ? 0.125f : 0.25f;
            var left = 0f;
            var top = (image.Height - topOffset) - (lines.Count * lineHeight);

            Image result = new Bitmap(image);

            lock(_SyncLock) {
                using(Graphics graphics = Graphics.FromImage(result)) {
                    // Note that this gets completely ignored by Mono...
                    StringFormat stringFormat = new StringFormat() {
                        Alignment = centreText ? StringAlignment.Center : StringAlignment.Near,
                        LineAlignment = StringAlignment.Center,
                        FormatFlags = StringFormatFlags.NoWrap,
                    };

                    graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

                    using(GraphicsPath path = new GraphicsPath()) {
                        float lineTop = top;
                        foreach(string line in lines) {
                            var fontName = _IsMono ? "Droid Sans" : "MS Sans Serif";
                            Font font = GetFontForRectangle(fontName, FontStyle.Regular, startPointSize, graphics, (float)result.Width - left, lineHeight * 2f, line);
                            path.AddString(line, font.FontFamily, (int)font.Style, font.Size, new RectangleF(0, lineTop, result.Width, lineHeight), stringFormat);
                            lineTop += lineHeight;
                        }

                        int haloWidth = result.Width + (result.Width % haloMod);
                        int haloHeight = result.Height + (result.Height % haloMod);
                        using(Bitmap halo = new Bitmap(haloWidth / haloMod, haloHeight / haloMod)) {
                            using(Graphics haloGraphics = Graphics.FromImage(halo)) {
                                using(Matrix matrix = new Matrix(haloTrans, 0, 0, haloTrans, -haloTrans, -haloTrans)) {
                                    haloGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                                    haloGraphics.Transform = matrix;
                                    using(Pen pen = new Pen(Color.Gray, 1) { LineJoin = LineJoin.Round }) {
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
            }

            return result;
        }
        #endregion

        #region Utility methods - CreateBlankImage, GetFontForRectangle, UseImage
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
        #endregion
    }
}
