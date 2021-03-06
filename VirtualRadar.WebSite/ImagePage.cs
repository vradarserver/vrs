// Copyright © 2010 onwards, Andrew Whewell
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Drawing;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Resources;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// Responds to requests for images.
    /// </summary>
    class ImagePage : Page
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
        /// The object that loads images from files for us.
        /// </summary>
        private IImageFile _ImageFile;

        /// <summary>
        /// The object that handles brushes for us.
        /// </summary>
        private IBrushFactory   _BrushFactory;


        /// <summary>
        /// The object that manages graphics library interactions for us.
        /// </summary>
        private IWebSiteGraphics _Graphics;

        /// <summary>
        /// The folder that holds the operator flag images.
        /// </summary>
        private string _OperatorFlagFolder;

        /// <summary>
        /// The folder that holds silhouette images.
        /// </summary>
        private string _SilhouetteFolder;

        /// <summary>
        /// The object that can lookup aircraft pictures for us.
        /// </summary>
        private IAircraftPictureManager _PictureManager;

        /// <summary>
        /// The object that is keeping track of filenames in the picture folder.
        /// </summary>
        private IDirectoryCache _PictureFolderCache;

        /// <summary>
        /// A value indicating whether browsers coming to us from the Internet are allowed to view aircraft pictures.
        /// </summary>
        private bool _InternetClientCanViewPictures;

        /// <summary>
        /// A value indicating whether browsers coming from the Internet are allowed to add text to images, which could be computationally expensive.
        /// </summary>
        private bool _InternetClientCanShowText;

        private IImageFileManager _ImageFileManager;
        /// <summary>
        /// A private property that ensures we have a single instance of IImageFileManager in use and that it isn't
        /// created unless it's needed.
        /// </summary>
        private IImageFileManager ImageFileManager
        {
            get
            {
                if(_ImageFileManager == null) _ImageFileManager = Factory.Resolve<IImageFileManager>();
                return _ImageFileManager;
            }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ImagePage(WebSite webSite) : base(webSite)
        {
            _ImageFile = Factory.ResolveSingleton<IImageFile>();
            _BrushFactory = Factory.ResolveSingleton<IBrushFactory>();
            _Graphics = Factory.ResolveSingleton<IWebSiteGraphics>();
            _PictureManager = Factory.ResolveSingleton<IAircraftPictureManager>();
            _PictureFolderCache = Factory.ResolveSingleton<IAutoConfigPictureFolderCache>().DirectoryCache;
        }

        /// <summary>
        /// See base class.
        /// </summary>
        /// <param name="configuration"></param>
        protected override void DoLoadConfiguration(Configuration configuration)
        {
            _OperatorFlagFolder = ValidatedConfigurationFolder(configuration.BaseStationSettings.OperatorFlagsFolder);
            _SilhouetteFolder = ValidatedConfigurationFolder(configuration.BaseStationSettings.SilhouettesFolder);
            _InternetClientCanViewPictures = configuration.InternetClientSettings.CanShowPictures;
            _InternetClientCanShowText = configuration.InternetClientSettings.CanShowPinText;
        }

        private string ValidatedConfigurationFolder(string folder)
        {
            return String.IsNullOrEmpty(folder) || !Directory.Exists(folder) ? null : folder;
        }

        /// <summary>
        /// See base class.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected override bool DoHandleRequest(IWebServer server, RequestReceivedEventArgs args)
        {
            bool result = false;

            if(args.PathAndFile.StartsWith("/Images/", StringComparison.OrdinalIgnoreCase)) {
                var imageRequest = ExtractImageRequest(args);
                result = imageRequest != null;

                // Stock image is an image from the resource file. We don't need to dispose of it, it can be reused
                // Temp image is an image we have either built from scratch or built by modifying a stock image. It is only
                // good for this request and needs to be disposed of when we're done with it.
                IImage stockImage = null;
                IImage tempImage = null;

                if(result) {
                    try {
                        result = BuildInitialImage(imageRequest, args, ref stockImage, ref tempImage);
                        if(result) {
                            if(imageRequest.IsHighDpi)            tempImage = _Graphics.UseImage(tempImage, _Graphics.ResizeForHiDpi(tempImage ?? stockImage));
                            var addTextLines = imageRequest.HasTextLines && (!args.IsInternetRequest || _InternetClientCanShowText);
                            if(imageRequest.RotateDegrees > 0.0)  tempImage = _Graphics.UseImage(tempImage, _Graphics.RotateImage(tempImage ?? stockImage, imageRequest.RotateDegrees.Value));
                            if(imageRequest.Width != null)        tempImage = _Graphics.UseImage(tempImage, _Graphics.WidenImage(tempImage ?? stockImage, imageRequest.Width.Value, imageRequest.CentreImageHorizontally));
                            if(imageRequest.ShowAltitudeStalk)    tempImage = _Graphics.UseImage(tempImage, _Graphics.AddAltitudeStalk(tempImage ?? stockImage, imageRequest.Height.GetValueOrDefault(), imageRequest.CentreX.GetValueOrDefault()));
                            else if(imageRequest.Height != null)  tempImage = _Graphics.UseImage(tempImage, _Graphics.HeightenImage(tempImage ?? stockImage, imageRequest.Height.Value, imageRequest.CentreImageVertically));
                            if(addTextLines)                      tempImage = _Graphics.UseImage(tempImage, _Graphics.AddTextLines(tempImage ?? stockImage, imageRequest.TextLines, centreText: true, isHighDpi: imageRequest.IsHighDpi));
                        }

                        if(result) {
                            Responder.SendImage(args.Request, args.Response, tempImage ?? stockImage, imageRequest.ImageFormat);
                            args.Classification = ContentClassification.Image;
                        }
                    } finally {
                        if(stockImage != null) stockImage.Dispose();        // clones are now made of all stock images
                        if(tempImage != null) tempImage.Dispose();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Extracts information about the required image from the URL.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private ImageRequest ExtractImageRequest(RequestReceivedEventArgs args)
        {
            bool isValid = true;

            ImageRequest result = new ImageRequest() {
                ImageName = Path.GetFileNameWithoutExtension(args.File).ToUpper(),
            };

            switch(Path.GetExtension(args.File).ToUpper()) {
                case ".PNG":    result.ImageFormat = ImageFormat.Png; break;
                case ".GIF":    result.ImageFormat = ImageFormat.Gif; break;
                case ".BMP":    result.ImageFormat = ImageFormat.Bmp; break;
                default:        isValid = false; break;
            }

            foreach(var pathPart in args.PathParts) {
                var caselessPart = pathPart.ToUpper();
                if(caselessPart.StartsWith("ALT-"))         result.ShowAltitudeStalk = true;
                if(caselessPart.StartsWith("ROTATE-"))      result.RotateDegrees = ParseDouble(pathPart.Substring(7), 0.0, 359.99);
                else if(caselessPart.StartsWith("HGHT-"))   result.Height = ParseInt(pathPart.Substring(5), 0, 4096);
                else if(caselessPart.StartsWith("WDTH-"))   result.Width = ParseInt(pathPart.Substring(5), 0, 4096);
                else if(caselessPart.StartsWith("CENX-"))   result.CentreX = ParseInt(pathPart.Substring(5), 0, 4096);
                else if(caselessPart.StartsWith("FILE-"))   result.File = pathPart.Substring(5).Replace("\\", "");
                else if(caselessPart.StartsWith("SIZE-"))   result.Size = ParseStandardSize(pathPart.Substring(5));
                else if(caselessPart == "HIDPI")            result.IsHighDpi = true;
                else if(caselessPart == "LEFT")             result.CentreImageHorizontally = false;
                else if(caselessPart == "TOP")              result.CentreImageVertically = false;
                else if(caselessPart == "NO-CACHE")         result.NoCache = !args.IsInternetRequest;
                else if(caselessPart.StartsWith("WEB") && isValid) {
                    var pathAndFileName = new StringBuilder("/images/");
                    var hyphenPosn = pathPart.IndexOf('-');
                    if(hyphenPosn != -1) {
                        var folder = pathPart.Substring(hyphenPosn + 1).Replace("\\", "/").Trim();
                        if(folder.Length > 0) {
                            pathAndFileName.AppendFormat("{0}{1}", folder, folder[folder.Length - 1] == '/' ? "" : "/");
                        }
                    }
                    pathAndFileName.Append(Path.GetFileName(args.File));
                    result.WebSiteFileName = pathAndFileName.ToString();
                } else if(caselessPart.StartsWith("PL")) {
                    var hyphenPosn = caselessPart.IndexOf('-');
                    if(hyphenPosn >= 2) {
                        var rowText = caselessPart.Substring(2, hyphenPosn - 2);
                        int row;
                        if(int.TryParse(rowText, out row) && row >= 1 && row <= 9) {
                            --row;
                            while(result.TextLines.Count <= row) result.TextLines.Add(null);
                            result.TextLines[row] = pathPart.Substring(hyphenPosn + 1);
                        }
                    }
                }
            }

            return isValid ? result : null;
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
            double result;
            return double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result) ? Math.Min(4096.0, Math.Max(-4096.0, result)) : (double?)null;
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
            int result;
            return int.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result) ? Math.Min(4096, Math.Max(-4096, result)) : (int?)null;
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
        /// <param name="args"></param>
        /// <param name="stockImage"></param>
        /// <param name="tempImage"></param>
        /// <returns></returns>
        private bool BuildInitialImage(ImageRequest imageRequest, RequestReceivedEventArgs args, ref IImage stockImage, ref IImage tempImage)
        {
            bool result = true;

            if(imageRequest.WebSiteFileName != null) {
                // This will return a clone, we don't have to clone it. We are responsible for destroying the image.
                stockImage = ImageFileManager.LoadFromWebSite(_WebSite, imageRequest.WebSiteFileName, !imageRequest.NoCache);
            } else {
                switch(imageRequest.ImageName) {
                    case "AIRPLANE":                stockImage = _ImageFile.LoadFromByteArray(Images.Marker_Airplane); break;
                    case "AIRPLANESELECTED":        stockImage = _ImageFile.LoadFromByteArray(Images.Marker_AirplaneSelected); break;
                    case "BLANK":                   tempImage  = _Graphics.CreateBlankImage(imageRequest.Width.GetValueOrDefault(), imageRequest.Height.GetValueOrDefault()); break;
                    case "CHEVRONBLUECIRCLE":       stockImage = _ImageFile.LoadFromByteArray(Images.ChevronBlueCircle); break;
                    case "CHEVRONGREENCIRCLE":      stockImage = _ImageFile.LoadFromByteArray(Images.ChevronGreenCircle); break;
                    case "CHEVRONREDCIRCLE":        stockImage = _ImageFile.LoadFromByteArray(Images.ChevronRedCircle); break;
                    case "CLOSESLIDER":             stockImage = _ImageFile.LoadFromByteArray(Images.CloseSlider); break;
                    case "COMPASS":                 stockImage = _ImageFile.LoadFromByteArray(Images.Compass); break;
                    case "CORNER-TL":               stockImage = _ImageFile.LoadFromByteArray(Images.Corner_TopLeft); break;
                    case "CORNER-TR":               stockImage = _ImageFile.LoadFromByteArray(Images.Corner_TopRight); break;
                    case "CORNER-BL":               stockImage = _ImageFile.LoadFromByteArray(Images.Corner_BottomLeft); break;
                    case "CORNER-BR":               stockImage = _ImageFile.LoadFromByteArray(Images.Corner_BottomRight); break;
                    case "CROSSHAIR":               stockImage = _ImageFile.LoadFromByteArray(Images.Crosshair); break;
                    case "GOTOCURRENTLOCATION":     stockImage = _ImageFile.LoadFromByteArray(Images.GotoCurrentLocation); break;
                    case "GROUNDVEHICLE":           stockImage = _ImageFile.LoadFromByteArray(Images.FollowMe); break;
                    case "GROUNDVEHICLESELECTED":   stockImage = _ImageFile.LoadFromByteArray(Images.FollowMe); break;
                    case "HEADING":                 stockImage = _ImageFile.LoadFromByteArray(Images.SmallPlaneNorth); break;
                    case "HIDELIST":                stockImage = _ImageFile.LoadFromByteArray(Images.HideList); break;
                    case "IPHONEBACKBUTTON":        stockImage = _ImageFile.LoadFromByteArray(Images.IPhoneBackButton); break;
                    case "IPHONEBLUEBUTTON":        stockImage = _ImageFile.LoadFromByteArray(Images.IPhoneBlueButton); break;
                    case "IPHONECHEVRON":           stockImage = _ImageFile.LoadFromByteArray(Images.IPhoneChevron); break;
                    case "IPHONEGRAYBUTTON":        stockImage = _ImageFile.LoadFromByteArray(Images.IPhoneGrayButton); break;
                    case "IPHONEICON":              stockImage = _ImageFile.LoadFromByteArray(Images.IPhoneIcon); break;
                    case "IPHONELISTGROUP":         stockImage = _ImageFile.LoadFromByteArray(Images.IPhoneListGroup); break;
                    case "IPHONEONOFF":             stockImage = _ImageFile.LoadFromByteArray(Images.IPhoneOnOff); break;
                    case "IPHONEPINSTRIPES":        stockImage = _ImageFile.LoadFromByteArray(Images.IPhonePinstripes); break;
                    case "IPHONESELECTEDTICK":      stockImage = _ImageFile.LoadFromByteArray(Images.IPhoneSelectedTick); break;
                    case "IPHONESELECTION":         stockImage = _ImageFile.LoadFromByteArray(Images.IPhoneSelection); break;
                    case "IPHONESPLASH":            tempImage  = _Graphics.CreateIPhoneSplash(args.WebSite, args.IsIPad, args.PathParts); break;
                    case "IPHONETOOLBAR":           stockImage = _ImageFile.LoadFromByteArray(Images.IPhoneToolbar); break;
                    case "IPHONETOOLBUTTON":        stockImage = _ImageFile.LoadFromByteArray(Images.IPhoneToolButton); break;
                    case "IPHONEWHITEBUTTON":       stockImage = _ImageFile.LoadFromByteArray(Images.IPhoneWhiteButton); break;
                    case "MINUS":                   stockImage = _ImageFile.LoadFromByteArray(Images.Collapse); break;
                    case "MOVINGMAPCHECKED":        stockImage = _ImageFile.LoadFromByteArray(Images.MovingMapChecked); break;
                    case "MOVINGMAPUNCHECKED":      stockImage = _ImageFile.LoadFromByteArray(Images.MovingMapUnchecked); break;
                    case "OPENSLIDER":              stockImage = _ImageFile.LoadFromByteArray(Images.OpenSlider); break;
                    case "OPFLAG":                  tempImage  = CreateLogoImage(imageRequest.File, _OperatorFlagFolder); break;
                    case "PICTURE":                 tempImage  = CreateAirplanePicture(imageRequest.File, imageRequest.Size, args.IsInternetRequest, imageRequest.Width, imageRequest.Height); imageRequest.Width = imageRequest.Height = null; break;
                    case "PLUS":                    stockImage = _ImageFile.LoadFromByteArray(Images.Expand); break;
                    case "ROWHEADER":               stockImage = _ImageFile.LoadFromByteArray(Images.RowHeader); break;
                    case "ROWHEADERSELECTED":       stockImage = _ImageFile.LoadFromByteArray(Images.RowHeaderSelected); break;
                    case "SHOWLIST":                stockImage = _ImageFile.LoadFromByteArray(Images.ShowList); break;
                    case "TESTSQUARE":              stockImage = _ImageFile.LoadFromByteArray(Images.TestSquare); break;
                    case "TOWER":                   stockImage = _ImageFile.LoadFromByteArray(Images.Tower); break;
                    case "TOWERSELECTED":           stockImage = _ImageFile.LoadFromByteArray(Images.Tower); break;
                    case "TRANSPARENT-25":          stockImage = _ImageFile.LoadFromByteArray(Images.Transparent_25); break;
                    case "TRANSPARENT-50":          stockImage = _ImageFile.LoadFromByteArray(Images.Transparent_50); break;
                    case "TYPE":                    tempImage  = CreateLogoImage(imageRequest.File, _SilhouetteFolder); break;
                    case "VOLUME0":                 stockImage = _ImageFile.LoadFromByteArray(Images.Volume0); break;
                    case "VOLUME25":                stockImage = _ImageFile.LoadFromByteArray(Images.Volume25); break;
                    case "VOLUME50":                stockImage = _ImageFile.LoadFromByteArray(Images.Volume50); break;
                    case "VOLUME75":                stockImage = _ImageFile.LoadFromByteArray(Images.Volume75); break;
                    case "VOLUME100":               stockImage = _ImageFile.LoadFromByteArray(Images.Volume100); break;
                    case "VOLUMEDOWN":              stockImage = _ImageFile.LoadFromByteArray(Images.VolumeDown); break;
                    case "VOLUMEMUTE":              stockImage = _ImageFile.LoadFromByteArray(Images.VolumeMute); break;
                    case "VOLUMEUP":                stockImage = _ImageFile.LoadFromByteArray(Images.VolumeUp); break;
                    case "YOUAREHERE":              stockImage = _ImageFile.LoadFromByteArray(Images.BlueBall); break;
                    default:                        result = false; break;
                }
            }

            if(result) result = stockImage != null || tempImage != null;

            return result;
        }

        /// <summary>
        /// Creates an image corresponding to the operator ICAO code or silhoutte passed across.
        /// </summary>
        /// <param name="logo"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        private IImage CreateLogoImage(string logo, string folder)
        {
            IImage result = null;

            const int width = 85;
            const int height = 20;

            if(!String.IsNullOrEmpty(folder)) {
                foreach(var chunk in logo.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries)) {
                    if(chunk.IndexOfAny(Path.GetInvalidFileNameChars()) == -1 && chunk.IndexOfAny(Path.GetInvalidPathChars()) == -1) {
                        string fullPath = Path.Combine(folder, String.Format("{0}.bmp", chunk));
                        if(File.Exists(fullPath)) {
                            result = ImageFileManager.LoadFromFile(fullPath);

                            if(result != null) {
                                if(result.Width != width)   result = _Graphics.UseImage(result, _Graphics.WidenImage(result, width, true));
                                if(result.Height != height) result = _Graphics.UseImage(result, _Graphics.HeightenImage(result, height, true));
                            }

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
        private IImage CreateAirplanePicture(string airplaneId, StandardWebSiteImageSize standardSize, bool isInternetClient, int? width, int? height)
        {
            IImage result = null;

            if(_InternetClientCanViewPictures || !isInternetClient && !String.IsNullOrEmpty(airplaneId)) {
                var lastSpacePosn = airplaneId.LastIndexOf(' ');
                var registration = lastSpacePosn == -1 ? null : airplaneId.Substring(0, lastSpacePosn);
                var icao = airplaneId.Substring(lastSpacePosn + 1);

                if(registration != null && registration.Length == 0) registration = null;
                if(icao != null && icao.Length == 0) icao = null;

                result = _PictureManager.LoadPicture(_PictureFolderCache, icao, registration);
                if(result != null) {
                    int newWidth = -1, newHeight = -1, minWidth = -1;
                    var resizeMode = ResizeMode.Stretch;
                    bool preferSpeed = false;
                    switch(standardSize) {
                        case StandardWebSiteImageSize.IPadDetail:           newWidth = 680; break;
                        case StandardWebSiteImageSize.IPhoneDetail:         newWidth = 260; break;
                        case StandardWebSiteImageSize.PictureDetail:        newWidth = 350; minWidth = 350; break;
                        case StandardWebSiteImageSize.PictureListThumbnail: newWidth = 60; newHeight = 40; resizeMode = ResizeMode.Centre; break;
                        case StandardWebSiteImageSize.BaseStation:          newWidth = 200; newHeight = 133; break;
                    }
                    if(width != null) newWidth = width.Value;
                    if(height != null) newHeight = height.Value;

                    if((newWidth != -1 || newHeight != -1) && (minWidth == -1 || result.Width > newWidth)) {
                        if(newWidth == -1)          newWidth = (int)(((double)newHeight * ((double)result.Width / (double)result.Height)) + 0.5);
                        else if(newHeight == -1)    newHeight = (int)(((double)newWidth / ((double)result.Width / (double)result.Height)) + 0.5);
                        result = _Graphics.UseImage(result, _Graphics.ResizeBitmap(result, newWidth, newHeight, resizeMode, _BrushFactory.Transparent, preferSpeed));
                    }
                }
            }

            return result;
        }
    }
}
