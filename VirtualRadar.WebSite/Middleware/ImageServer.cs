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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AWhewell.Owin.Utility;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Resources;

namespace VirtualRadar.WebSite.Middleware
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
        /// The shared aircraft picture manager object.
        /// </summary>
        private IAircraftPictureManager _AircraftPictureManager;

        /// <summary>
        /// The cache used to hold aircraft pictures.
        /// </summary>
        private IAutoConfigPictureFolderCache _AutoConfigPictureFolderCache;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ImageServer()
        {
            _ImageServerConfiguration = Factory.ResolveSingleton<IImageServerConfiguration>();
            _SharedConfiguration = Factory.ResolveSingleton<ISharedConfiguration>();
            _Graphics = Factory.ResolveSingleton<IWebSiteGraphics>();
            _AircraftPictureManager = Factory.ResolveSingleton<IAircraftPictureManager>();
            _AutoConfigPictureFolderCache = Factory.ResolveSingleton<IAutoConfigPictureFolderCache>();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public AppFunc AppFuncBuilder(AppFunc next)
        {
            return async(IDictionary<string, object> environment) => {
                var handled = false;
                var context = OwinContext.Create(environment);

                if(context.RequestPathNormalised.StartsWith("/images/", StringComparison.OrdinalIgnoreCase)) {
                    handled = ServeImage(context);
                }

                if(!handled) {
                    await next(environment);
                }
            };
        }

        /// <summary>
        /// Handles the request for an image.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool ServeImage(OwinContext context)
        {
            var handled = false;
            var imageRequest = ExtractImageRequest(context);
            var result = imageRequest != null;

            if(result) {
                Image stockImage = null;
                Image tempImage = null;

                try {
                    result = BuildInitialImage(context, imageRequest, ref stockImage, ref tempImage);

                    if(result) {
                        var configuration = _SharedConfiguration.Get();

                        if(imageRequest.IsHighDpi) {
                            tempImage = _Graphics.UseImage(tempImage, _Graphics.ResizeForHiDpi(tempImage ?? stockImage));
                        }
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

                        var addTextLines = imageRequest.HasTextLines && (!context.IsInternet || configuration.InternetClientSettings.CanShowPinText);
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

                                    context.ResponseHttpStatusCode = HttpStatusCode.OK;
                                    context.ReturnBytes(
                                        ImageMimeType(imageRequest.ImageFormat),
                                        bytes
                                    );

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
        /// <param name="context"></param>
        /// <returns></returns>
        private ImageRequest ExtractImageRequest(OwinContext context)
        {
            var requestFileName = context.RequestPathFileName;
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
                result = ParsePathParts(result, context);
            }

            return result;
        }

        private ImageRequest ParsePathParts(ImageRequest result, OwinContext context)
        {
            foreach(var pathPart in context.RequestPathParts) {
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
                    result.NoCache = !context.IsInternet;
                } else if(caselessPart.StartsWith("WEB")) {
                    var pathAndFileName = new StringBuilder("/images/");
                    var hyphenPosn = pathPart.IndexOf('-');
                    if(hyphenPosn != -1) {
                        var folder = pathPart.Substring(hyphenPosn + 1).Replace("\\", "/").Trim();
                        if(folder.Length > 0) {
                            pathAndFileName.AppendFormat("{0}{1}", folder, folder[folder.Length - 1] == '/' ? "" : "/");
                        }
                    }
                    pathAndFileName.Append(Path.GetFileName(context.RequestPathFileName));
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
        /// <param name="context"></param>
        /// <param name="imageRequest"></param>
        /// <param name="stockImage"></param>
        /// <param name="tempImage"></param>
        /// <returns></returns>
        private bool BuildInitialImage(OwinContext context, ImageRequest imageRequest, ref Image stockImage, ref Image tempImage)
        {
            var result = true;

            if(imageRequest.WebSiteFileName != null) {
                stockImage = _ImageServerConfiguration.ImageFileManager.LoadFromStandardPipeline(imageRequest.WebSiteFileName, !imageRequest.NoCache, context.Environment);
            } else {
                switch(imageRequest.ImageName) {
                    case "AIRPLANE":                stockImage = Images.Clone_Marker_Airplane(); break;
                    case "AIRPLANESELECTED":        stockImage = Images.Clone_Marker_AirplaneSelected(); break;
                    case "BLANK":                   tempImage  = _Graphics.CreateBlankImage(imageRequest.Width.GetValueOrDefault(), imageRequest.Height.GetValueOrDefault()); break;
                    case "CHEVRONBLUECIRCLE":       stockImage = Images.Clone_ChevronBlueCircle(); break;
                    case "CHEVRONGREENCIRCLE":      stockImage = Images.Clone_ChevronGreenCircle(); break;
                    case "CHEVRONREDCIRCLE":        stockImage = Images.Clone_ChevronRedCircle(); break;
                    case "CLOSESLIDER":             stockImage = Images.Clone_CloseSlider(); break;
                    case "COMPASS":                 stockImage = Images.Clone_Compass(); break;
                    case "CORNER-TL":               stockImage = Images.Clone_Corner_TopLeft(); break;
                    case "CORNER-TR":               stockImage = Images.Clone_Corner_TopRight(); break;
                    case "CORNER-BL":               stockImage = Images.Clone_Corner_BottomLeft(); break;
                    case "CORNER-BR":               stockImage = Images.Clone_Corner_BottomRight(); break;
                    case "CROSSHAIR":               stockImage = Images.Clone_Crosshair(); break;
                    case "GOTOCURRENTLOCATION":     stockImage = Images.Clone_GotoCurrentLocation(); break;
                    case "GROUNDVEHICLE":           stockImage = Images.Clone_FollowMe(); break;
                    case "GROUNDVEHICLESELECTED":   stockImage = Images.Clone_FollowMe(); break;
                    case "HEADING":                 stockImage = Images.Clone_SmallPlaneNorth(); break;
                    case "HIDELIST":                stockImage = Images.Clone_HideList(); break;
                    case "IPHONEBACKBUTTON":        stockImage = Images.Clone_IPhoneBackButton(); break;
                    case "IPHONEBLUEBUTTON":        stockImage = Images.Clone_IPhoneBlueButton(); break;
                    case "IPHONECHEVRON":           stockImage = Images.Clone_IPhoneChevron(); break;
                    case "IPHONEGRAYBUTTON":        stockImage = Images.Clone_IPhoneGrayButton(); break;
                    case "IPHONEICON":              stockImage = Images.Clone_IPhoneIcon(); break;
                    case "IPHONELISTGROUP":         stockImage = Images.Clone_IPhoneListGroup(); break;
                    case "IPHONEONOFF":             stockImage = Images.Clone_IPhoneOnOff(); break;
                    case "IPHONEPINSTRIPES":        stockImage = Images.Clone_IPhonePinstripes(); break;
                    case "IPHONESELECTEDTICK":      stockImage = Images.Clone_IPhoneSelectedTick(); break;
                    case "IPHONESELECTION":         stockImage = Images.Clone_IPhoneSelection(); break;
                    case "IPHONESPLASH":
                        var webSiteAddress = new StringBuilder();
                        webSiteAddress.Append(String.IsNullOrEmpty(context.RequestScheme) ? "http" : context.RequestScheme);
                        webSiteAddress.Append("://");
                        webSiteAddress.Append(String.IsNullOrEmpty(context.RequestHost) ? "127.0.0.1" : context.RequestHost);
                        webSiteAddress.Append(context.RequestPathBase);
                        tempImage  = _Graphics.CreateIPhoneSplash(
                            webSiteAddress.ToString(),
                            context.RequestHeadersDictionary.UserAgentValue.IsTabletUserAgentString,
                            new List<string>(context.RequestPathParts)
                        );
                        break;
                    case "IPHONETOOLBAR":           stockImage = Images.Clone_IPhoneToolbar(); break;
                    case "IPHONETOOLBUTTON":        stockImage = Images.Clone_IPhoneToolButton(); break;
                    case "IPHONEWHITEBUTTON":       stockImage = Images.Clone_IPhoneWhiteButton(); break;
                    case "MINUS":                   stockImage = Images.Clone_Collapse(); break;
                    case "MOVINGMAPCHECKED":        stockImage = Images.Clone_MovingMapChecked(); break;
                    case "MOVINGMAPUNCHECKED":      stockImage = Images.Clone_MovingMapUnchecked(); break;
                    case "OPENSLIDER":              stockImage = Images.Clone_OpenSlider(); break;
                    case "OPFLAG":                  tempImage  = CreateLogoImage(imageRequest.File, _ImageServerConfiguration.OperatorFolder); break;
                    case "PICTURE":
                        tempImage  = CreateAirplanePicture(
                            imageRequest.File,
                            imageRequest.Size,
                            context.IsInternet,
                            imageRequest.Width,
                            imageRequest.Height
                        );
                        imageRequest.Width = imageRequest.Height = null;
                        break;
                    case "PLUS":                    stockImage = Images.Clone_Expand(); break;
                    case "ROWHEADER":               stockImage = Images.Clone_RowHeader(); break;
                    case "ROWHEADERSELECTED":       stockImage = Images.Clone_RowHeaderSelected(); break;
                    case "SHOWLIST":                stockImage = Images.Clone_ShowList(); break;
                    case "TESTSQUARE":              stockImage = Images.Clone_TestSquare(); break;
                    case "TOWER":                   stockImage = Images.Clone_Tower(); break;
                    case "TOWERSELECTED":           stockImage = Images.Clone_Tower(); break;
                    case "TRANSPARENT-25":          stockImage = Images.Clone_Transparent_25(); break;
                    case "TRANSPARENT-50":          stockImage = Images.Clone_Transparent_50(); break;
                    case "TYPE":                    tempImage  = CreateLogoImage(imageRequest.File, _ImageServerConfiguration.SilhouettesFolder); break;
                    case "VOLUME0":                 stockImage = Images.Clone_Volume0(); break;
                    case "VOLUME25":                stockImage = Images.Clone_Volume25(); break;
                    case "VOLUME50":                stockImage = Images.Clone_Volume50(); break;
                    case "VOLUME75":                stockImage = Images.Clone_Volume75(); break;
                    case "VOLUME100":               stockImage = Images.Clone_Volume100(); break;
                    case "VOLUMEDOWN":              stockImage = Images.Clone_VolumeDown(); break;
                    case "VOLUMEMUTE":              stockImage = Images.Clone_VolumeMute(); break;
                    case "VOLUMEUP":                stockImage = Images.Clone_VolumeUp(); break;
                    case "YOUAREHERE":              stockImage = Images.Clone_BlueBall(); break;
                    default:                        result = false; break;
                }
            }

            if(result) {
                result = stockImage != null || tempImage != null;
            }

            return result;
        }

        /// <summary>
        /// Creates an image corresponding to the operator ICAO code or silhoutte passed across.
        /// </summary>
        /// <param name="logo"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        private Image CreateLogoImage(string logo, string folder)
        {
            Image result = null;

            const int width = 85;
            const int height = 20;

            if(!String.IsNullOrEmpty(folder)) {
                foreach(var chunk in logo.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries)) {
                    if(chunk.IndexOfAny(Path.GetInvalidFileNameChars()) == -1 && chunk.IndexOfAny(Path.GetInvalidPathChars()) == -1) {
                        string fullPath = Path.Combine(folder, String.Format("{0}.bmp", chunk));
                        var fileSystem = Factory.Resolve<IFileSystemProvider>();
                        if(fileSystem.FileExists(fullPath)) {
                            result = _ImageServerConfiguration.ImageFileManager.LoadFromFile(fullPath);

                            if(result.Width != width)   result = _Graphics.UseImage(result, _Graphics.WidenImage(result, width, true));
                            if(result.Height != height) result = _Graphics.UseImage(result, _Graphics.HeightenImage(result, height, true));

                            break;
                        }
                    }
                }
            }

            return result ?? _Graphics.CreateBlankImage(width, height);
        }

        /// <summary>
        /// Loads an aircraft image and sizes it according to the size passed across.
        /// </summary>
        /// <param name="airplaneId"></param>
        /// <param name="standardSize"></param>
        /// <param name="isInternetClient"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private Image CreateAirplanePicture(string airplaneId, StandardWebSiteImageSize standardSize, bool isInternetClient, int? width, int? height)
        {
            Bitmap result = null;

            var settings = _SharedConfiguration.Get();

            if((settings.InternetClientSettings.CanShowPictures || !isInternetClient) && !String.IsNullOrEmpty(airplaneId)) {
                var lastSpacePosn = airplaneId.LastIndexOf(' ');
                var registration = lastSpacePosn == -1 ? null : airplaneId.Substring(0, lastSpacePosn);
                var icao = airplaneId.Substring(lastSpacePosn + 1);

                if(registration != null && registration.Length == 0) registration = null;
                if(icao != null && icao.Length == 0) icao = null;

                result = (Bitmap)_AircraftPictureManager.LoadPicture(_AutoConfigPictureFolderCache.DirectoryCache, icao, registration);
                if(result != null) {
                    int newWidth = -1, newHeight = -1, minWidth = -1;
                    var resizeMode = ResizeImageMode.Stretch;
                    var preferSpeed = false;
                    switch(standardSize) {
                        case StandardWebSiteImageSize.IPadDetail:           newWidth = 680; break;
                        case StandardWebSiteImageSize.IPhoneDetail:         newWidth = 260; break;
                        case StandardWebSiteImageSize.PictureDetail:        newWidth = 350; minWidth = 350; break;
                        case StandardWebSiteImageSize.PictureListThumbnail: newWidth = 60; newHeight = 40; resizeMode = ResizeImageMode.Centre; break;
                        case StandardWebSiteImageSize.BaseStation:          newWidth = 200; newHeight = 133; break;
                    }
                    if(width != null) newWidth = width.Value;
                    if(height != null) newHeight = height.Value;

                    if((newWidth != -1 || newHeight != -1) && (minWidth == -1 || result.Width > newWidth)) {
                        if(newWidth == -1)          newWidth = (int)(((double)newHeight * ((double)result.Width / (double)result.Height)) + 0.5);
                        else if(newHeight == -1)    newHeight = (int)(((double)newWidth / ((double)result.Width / (double)result.Height)) + 0.5);
                        result = (Bitmap)_Graphics.UseImage(result, _Graphics.ResizeBitmap(result, newWidth, newHeight, resizeMode, Brushes.Transparent, preferSpeed));
                    }
                }
            }

            return result;
        }
    }
}
