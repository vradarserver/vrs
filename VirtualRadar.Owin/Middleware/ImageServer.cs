// Copyright © 2017 onwards, Andrew Whewell
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
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Resources;

namespace VirtualRadar.Owin.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// The default implementation of <see cref="IImageServer"/>.
    /// </summary>
    class ImageServer : IImageServer
    {
        /// <summary>
        /// A private class that carries information gleaned from the URL used to request the stockImage.
        /// </summary>
        class ImageRequest
        {
            /// <summary>
            /// The basic image to send back to the browser.
            /// </summary>
            public string ImageName;

            /// <summary>
            /// The format requested by the browser.
            /// </summary>
            public ImageFormat ImageFormat;

            /// <summary>
            /// The file to read from the web site.
            /// </summary>
            public string WebSiteFileName;

            /// <summary>
            /// The angle of rotation to apply to the image.
            /// </summary>
            public double? RotateDegrees;

            /// <summary>
            /// The new width of the image.
            /// </summary>
            public int? Width;

            /// <summary>
            /// Centres the image horizontally if the width needs to be changed, otherwise leaves
            /// the image aligned on the left edge.
            /// </summary>
            public bool CentreImageHorizontally = true;

            /// <summary>
            /// The new height of the image.
            /// </summary>
            public int? Height;

            /// <summary>
            /// Centres the image vertically if the height needs to be changed, otherwise leaves
            /// the image aligned on the top edge.
            /// </summary>
            public bool CentreImageVertically = true;

            /// <summary>
            /// The centre pixel for those transformations that require one.
            /// </summary>
            public int? CentreX;

            /// <summary>
            /// A value indicating that the altitude stalk should be drawn.
            /// </summary>
            public bool ShowAltitudeStalk;

            /// <summary>
            /// The name of a file that should be used as the source for the image.
            /// </summary>
            public string File;

            /// <summary>
            /// The standard size at which to render particular images.
            /// </summary>
            public StandardWebSiteImageSize Size;

            /// <summary>
            /// A collection of text strings that need to be overlaid onto the image.
            /// </summary>
            public List<string> TextLines = new List<string>();

            /// <summary>
            /// Indicates that the browser is going to display the image in a space half the size of the img tag,
            /// or that the request is twice the size that it's expected to be rendered at.
            /// </summary>
            public bool IsHighDpi;

            /// <summary>
            /// Returns true if <see cref="TextLines"/> contains something.
            /// </summary>
            public bool HasTextLines
            {
                get { return TextLines.Count > 0 && TextLines.Any(r => r != null); }
            }

            /// <summary>
            /// True if the web image should always be fetched from disk rather than from the cache. This should always
            /// be disabled for Internet requests.
            /// </summary>
            public bool NoCache;
        }

        /// <summary>
        /// The shared image server configuration object.
        /// </summary>
        private IImageServerConfiguration _ImageServerConfiguration;

        /// <summary>
        /// The shared program configuration object.
        /// </summary>
        private ISharedConfiguration _SharedConfiguration;

        /// <summary>
        /// The shared graphics object.
        /// </summary>
        private IWebSiteGraphics _Graphics;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ImageServer()
        {
            _ImageServerConfiguration = Factory.Singleton.Resolve<IImageServerConfiguration>().Singleton;
            _SharedConfiguration = Factory.Singleton.Resolve<ISharedConfiguration>().Singleton;
            _Graphics = Factory.Singleton.Resolve<IWebSiteGraphics>().Singleton;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public AppFunc HandleRequest(AppFunc next)
        {
            AppFunc appFunc = async(IDictionary<string, object> environment) => {
                var handled = false;
                var context = PipelineContext.GetOrCreate(environment);

                if(context.Request.PathNormalised.Value.StartsWith("/images/", StringComparison.OrdinalIgnoreCase)) {
                    handled = ServeImage(context);
                }

                if(!handled) {
                    await next.Invoke(environment);
                }
            };

            return appFunc;
        }

        /// <summary>
        /// Handles the request for an image.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool ServeImage(PipelineContext context)
        {
            var handled = false;
            var imageRequest = ExtractImageRequest(context.Request);
            var result = imageRequest != null;

            if(result) {
                Image stockImage = null;
                Image tempImage = null;

                try {
                    result = BuildInitialImage(imageRequest, context.Request, ref stockImage, ref tempImage);

                    if(result) {
                        var configuration = _SharedConfiguration.Get();

                        var addTextLines = imageRequest.HasTextLines && (!context.Request.IsInternet || configuration.InternetClientSettings.CanShowPinText);
                        if(imageRequest.RotateDegrees > 0.0) {
                            tempImage = _Graphics.UseImage(tempImage, _Graphics.RotateImage(tempImage ?? stockImage, imageRequest.RotateDegrees.Value));
                        }
                        if(imageRequest.Width != null) {
                            tempImage = _Graphics.UseImage(tempImage, _Graphics.WidenImage(tempImage ?? stockImage, imageRequest.Width.Value, imageRequest.CentreImageHorizontally));
                        }
                        if(imageRequest.ShowAltitudeStalk) {
                            tempImage = _Graphics.UseImage(tempImage, _Graphics.AddAltitudeStalk(tempImage ?? stockImage, imageRequest.Height.GetValueOrDefault(), imageRequest.CentreX.GetValueOrDefault()));
                        } else if(imageRequest.Height != null) {
                            tempImage = _Graphics.UseImage(tempImage, _Graphics.HeightenImage(tempImage ?? stockImage, imageRequest.Height.Value, imageRequest.CentreImageVertically));
                        }
                        if(addTextLines) {
                            tempImage = _Graphics.UseImage(tempImage, _Graphics.AddTextLines(tempImage ?? stockImage, imageRequest.TextLines, centreText: true, isHighDpi: imageRequest.IsHighDpi));
                        }
                    }

                    if(result) {
                        var image = tempImage ?? stockImage;

                        if(image != null) {
                            using(var stream = new MemoryStream()) {
                                using(var copy = (Image)image.Clone()) {
                                    copy.Save(stream, imageRequest.ImageFormat);
                                    var bytes = stream.ToArray();

                                    context.Response.ContentLength = bytes.Length;
                                    context.Response.ContentType = ImageMimeType(imageRequest.ImageFormat);
                                    context.Response.Body.Write(bytes, 0, bytes.Length);

                                    handled = true;
                                }
                            }
                        }
                    }
                } finally {
                    if(stockImage != null)  stockImage.Dispose();        // clones are now made of all stock images
                    if(tempImage != null)   tempImage.Dispose();
                }
            }

            return handled;
        }

        /// <summary>
        /// Returns the correct MIME type for an image format.
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        private static string ImageMimeType(ImageFormat format)
        {
            if(format == ImageFormat.Png)       return MimeType.PngImage;
            else if(format == ImageFormat.Gif)  return MimeType.GifImage;
            else if(format == ImageFormat.Bmp)  return MimeType.BitmapImage;
            else                                return "";
        }

        /// <summary>
        /// Extracts information about the required image from the request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private ImageRequest ExtractImageRequest(PipelineRequest request)
        {
            var requestFileName = request.FileName;
            ImageRequest result = new ImageRequest() {
                ImageName = Path.GetFileNameWithoutExtension(requestFileName).ToUpper(),
            };
            switch(Path.GetExtension(requestFileName).ToUpper()) {
                case ".PNG":    result.ImageFormat = ImageFormat.Png; break;
                case ".GIF":    result.ImageFormat = ImageFormat.Gif; break;
                case ".BMP":    result.ImageFormat = ImageFormat.Bmp; break;
                default:        result = null; break;
            }

            if(result != null) {
                result = ParsePathParts(result, request);
            }

            return result;
        }

        private ImageRequest ParsePathParts(ImageRequest result, PipelineRequest request)
        {
            foreach(var pathPart in request.PathParts) {
                var caselessPart = pathPart.ToUpper();

                if(caselessPart.StartsWith("ALT-")) {
                    result.ShowAltitudeStalk = true;
                }

                if(caselessPart.StartsWith("ROTATE-")) {
                    result.RotateDegrees = ParseDouble(pathPart.Substring(7), 0.0, 359.99);
                } else if(caselessPart.StartsWith("HGHT-")) {
                    result.Height = ParseInt(pathPart.Substring(5), 0, 4096);
                } else if(caselessPart.StartsWith("WDTH-")) {
                    result.Width = ParseInt(pathPart.Substring(5), 0, 4096);
                } else if(caselessPart.StartsWith("CENX-")) {
                    result.CentreX = ParseInt(pathPart.Substring(5), 0, 4096);
                } else if(caselessPart.StartsWith("FILE-")) {
                    result.File = pathPart.Substring(5).Replace("\\", "");
                } else if(caselessPart.StartsWith("SIZE-")) {
                    result.Size = ParseStandardSize(pathPart.Substring(5));
                } else if(caselessPart == "HIDPI") {
                    result.IsHighDpi = true;
                } else if(caselessPart == "LEFT") {
                    result.CentreImageHorizontally = false;
                } else if(caselessPart == "TOP") {
                    result.CentreImageVertically = false;
                } else if(caselessPart == "NO-CACHE") {
                    result.NoCache = !request.IsInternet;
                } else if(caselessPart.StartsWith("WEB")) {
                    var pathAndFileName = new StringBuilder("/images/");
                    var hyphenPosn = pathPart.IndexOf('-');
                    if(hyphenPosn != -1) {
                        var folder = pathPart.Substring(hyphenPosn + 1).Replace("\\", "/").Trim();
                        if(folder.Length > 0) {
                            pathAndFileName.AppendFormat("{0}{1}", folder, folder[folder.Length - 1] == '/' ? "" : "/");
                        }
                    }
                    pathAndFileName.Append(Path.GetFileName(request.FileName));
                    result.WebSiteFileName = pathAndFileName.ToString();
                } else if(caselessPart.StartsWith("PL")) {
                    var hyphenPosn = caselessPart.IndexOf('-');
                    if(hyphenPosn >= 2) {
                        var rowText = caselessPart.Substring(2, hyphenPosn - 2);
                        if(int.TryParse(rowText, out int row) && row >= 1 && row <= 9) {
                            --row;
                            while(result.TextLines.Count <= row) result.TextLines.Add(null);
                            result.TextLines[row] = pathPart.Substring(hyphenPosn + 1);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Extracts a double value from a path part.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private double? ParseDouble(string value, double min = -4096.0, double max = 4096.0)
        {
            return double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out double result)
                ? Math.Min(4096.0, Math.Max(-4096.0, result))
                : (double?)null;
        }

        /// <summary>
        /// Extract an integer value from a path part.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private int? ParseInt(string value, int min = -4096, int max = 4096)
        {
            return int.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out int result)
                ? Math.Min(4096, Math.Max(-4096, result))
                : (int?)null;
        }

        /// <summary>
        /// Parses a string into a standard size enum value.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private StandardWebSiteImageSize ParseStandardSize(string text)
        {
            switch(text.ToUpper()) {
                case "DETAIL":          return StandardWebSiteImageSize.PictureDetail;
                case "FULL":            return StandardWebSiteImageSize.Full;
                case "LIST":            return StandardWebSiteImageSize.PictureListThumbnail;
                case "IPADDETAIL":      return StandardWebSiteImageSize.IPadDetail;
                case "IPHONEDETAIL":    return StandardWebSiteImageSize.IPhoneDetail;
                case "BASESTATION":     return StandardWebSiteImageSize.BaseStation;
            }

            return StandardWebSiteImageSize.None;
        }

        /// <summary>
        /// Fills either of the stock image or temporary image parameters with the initial image to use (before any alterations are made).
        /// </summary>
        /// <param name="imageRequest"></param>
        /// <param name="request"></param>
        /// <param name="stockImage"></param>
        /// <param name="tempImage"></param>
        /// <returns></returns>
        private bool BuildInitialImage(ImageRequest imageRequest, PipelineRequest request, ref Image stockImage, ref Image tempImage)
        {
            bool result = true;

            if(imageRequest.WebSiteFileName != null) {
            } else {
                switch(imageRequest.ImageName) {
                    case "AIRPLANE":                stockImage = Images.Clone_Marker_Airplane(); break;
                    case "BLANK":                   tempImage  = _Graphics.CreateBlankImage(imageRequest.Width.GetValueOrDefault(), imageRequest.Height.GetValueOrDefault()); break;
                    case "TESTSQUARE":              stockImage = Images.Clone_TestSquare(); break;
                    default:                        result = false; break;
                }
            }

            if(result) {
                result = stockImage != null || tempImage != null;
            }

            return result;
        }
    }
}
