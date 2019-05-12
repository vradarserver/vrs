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
using VirtualRadar.Interface.Drawing;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Resources;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// The default implementation of <see cref="IWebSiteGraphics"/>.
    /// </summary>
    class WebSiteGraphics : IWebSiteGraphics
    {
        private IImageFile      _ImageFile;
        private IPenFactory     _PenFactory;
        private IBrushFactory   _BrushFactory;
        private IFontFactory    _FontFactory;

        private FontStyle       _MarkerTextFontStyle = FontStyle.Bold;
        private IFontFamily     _MarkerTextFontFamily;
        private IPen            _MarkerTextOutlinePen;
        private IPen            _MarkerTextOutlinePenHiDpi;
        private IBrush          _MarkerTextFillBrush;

        private FontStyle       _SplashFontStyle = FontStyle.Normal;
        private IFontFamily     _SplashFontFamily;
        private IBrush          _SplashFillBrush;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public WebSiteGraphics()
        {
            _ImageFile = Factory.ResolveSingleton<IImageFile>();
            _PenFactory = Factory.ResolveSingleton<IPenFactory>();
            _BrushFactory = Factory.ResolveSingleton<IBrushFactory>();
            _FontFactory = Factory.ResolveSingleton<IFontFactory>();

            _MarkerTextOutlinePen =      _PenFactory.CreatePen(0, 0, 0, 222, 4.0F, useCache: true);
            _MarkerTextOutlinePenHiDpi = _PenFactory.CreatePen(0, 0, 0, 222, 6.0F, useCache: true);
            _MarkerTextFillBrush =       _BrushFactory.CreateBrush(255, 255, 255, 255, useCache: true);
            _MarkerTextFontFamily =      _FontFactory.GetFontFamilyOrFallback(
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

            _SplashFillBrush =  _BrushFactory.CreateBrush(255, 255, 255, 255, useCache: true);
            _SplashFontFamily = _FontFactory.GetFontFamilyOrFallback(
                _SplashFontStyle,
                "Tahoma",
                "Microsoft Sans Serif",
                "MS Reference Sans Serif",
                "Roboto",
                "Droid Sans",
                "MS Sans Serif",
                "Verdana",
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
        public IImage RotateImage(IImage original, double degrees)
        {
            return original.Rotate((float)degrees);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="width"></param>
        /// <param name="centreHorizontally"></param>
        /// <returns></returns>
        public IImage WidenImage(IImage original, int width, bool centreHorizontally)
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
        public IImage HeightenImage(IImage original, int height, bool centreVertically)
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
        public IImage ResizeForHiDpi(IImage original)
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
        public IImage ResizeBitmap(IImage original, int width, int height, ResizeMode mode, IBrush zoomBackground, bool preferSpeedOverQuality)
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
        public IImage CreateIPhoneSplash(string webSiteAddress, bool isIPad, List<string> pathParts)
        {
            if(!isIPad && pathParts.Where(pp => pp.Equals("file-ipad", StringComparison.OrdinalIgnoreCase)).Any()) {
                isIPad = true;
            }

            IImage splashImage;
            float titleSize, addressSize, lineHeight;
            if(!isIPad) {
                splashImage = _ImageFile.LoadFromByteArray(Images.IPhoneSplash);
                titleSize = 24f;
                addressSize = 12f;
                lineHeight = 40f;
            } else {
                splashImage = _ImageFile.LoadFromByteArray(Images.IPadSplash);
                titleSize = 36f;
                addressSize = 14f;
                lineHeight = 50f;
            }

            var titleBounds =   new RectangleF(5, (splashImage.Height - 5) - (lineHeight * 2f), splashImage.Width - 10f, lineHeight);
            var addressBounds = new RectangleF(5, titleBounds.Bottom + 5, splashImage.Width - 10f, lineHeight);
            const string title = "Virtual Radar Server";

            return _ImageFile.CloneAndDraw(splashImage, drawing => {
                using(var fontAndText = _FontFactory.GetFontForRectangle(
                    drawing,
                    _SplashFontFamily,
                    _SplashFontStyle,
                    titleSize,
                    6.0F,
                    titleBounds.Width,
                    titleBounds.Height,
                    title,
                    useCache: true
                )) {
                    drawing.DrawText(
                        fontAndText.Text,
                        fontAndText.Font,
                        _SplashFillBrush,
                        null,
                        titleBounds.Left + (titleBounds.Width / 2.0F),
                        titleBounds.Top + lineHeight,
                        HorizontalAlignment.Centre,
                        preferSpeedOverQuality: false
                    );
                }

                using(var fontAndText = _FontFactory.GetFontForRectangle(
                    drawing,
                    _SplashFontFamily,
                    _SplashFontStyle,
                    addressSize,
                    6.0F,
                    addressBounds.Width,
                    addressBounds.Height,
                    webSiteAddress,
                    useCache: true
                )) {
                    drawing.DrawText(
                        fontAndText.Text,
                        fontAndText.Font,
                        _SplashFillBrush,
                        null,
                        addressBounds.Left + (addressBounds.Width / 2.0F),
                        addressBounds.Top + lineHeight,
                        HorizontalAlignment.Centre,
                        preferSpeedOverQuality: false
                    );
                }
            });
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="height"></param>
        /// <param name="centreX"></param>
        /// <returns></returns>
        public IImage AddAltitudeStalk(IImage original, int height, int centreX)
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
        public IImage AddTextLines(IImage image, IEnumerable<string> textLines, bool centreText, bool isHighDpi)
        {
            return _ImageFile.CloneAndDraw(
                image,
                drawing => {
                    var lines =          textLines.Where(tl => tl != null).ToList();
                    var lineHeight =     isHighDpi ? 24f : 12f;
                    var topOffset =      5f;
                    var startPointSize = isHighDpi ? 20f : 10f;
                    var outlinePen =     isHighDpi ? _MarkerTextOutlinePenHiDpi : _MarkerTextOutlinePen;
                    var left =           centreText ? ((float)image.Width / 2.0F) : outlinePen.StrokeWidth / 2.0F;
                    var top =            (image.Height - topOffset) - (lines.Count * lineHeight);
                    var width =          Math.Max(0F, image.Width - outlinePen.StrokeWidth);

                    var lineTop = top;
                    foreach(var line in lines) {
                        using(var fontAndText = _FontFactory.GetFontForRectangle(drawing, _MarkerTextFontFamily, _MarkerTextFontStyle, startPointSize, 6.0F, width, lineHeight * 2F, line, useCache: true)) {
                            drawing.DrawText(
                                fontAndText.Text,
                                fontAndText.Font,
                                _MarkerTextFillBrush,
                                _MarkerTextOutlinePen,
                                left,
                                lineTop,
                                centreText ? HorizontalAlignment.Centre : HorizontalAlignment.Left,
                                preferSpeedOverQuality: false
                            );
                        }

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
        public IImage CreateBlankImage(int width, int height)
        {
            return width > 0 && height > 0
                ? _ImageFile.Create(width, height)
                : null;
        }

        /// <summary>
        /// When passed the current temporary image and the image that will become to new temporary image this
        /// disposes of the old temporary image and returns the new one. It just helps with keeping the code
        /// free of checks to see whether tempImage is not null and therefore needs disposing of.
        /// </summary>
        /// <param name="tempImage"></param>
        /// <param name="newImage"></param>
        /// <returns></returns>
        public IImage UseImage(IImage tempImage, IImage newImage)
        {
            if(tempImage != null && !Object.ReferenceEquals(tempImage, newImage)) {
                tempImage.Dispose();
            }

            return newImage;
        }
    }
}
