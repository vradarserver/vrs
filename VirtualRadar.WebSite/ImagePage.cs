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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Resources;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// Responds to requests for images.
    /// </summary>
    class ImagePage : Page
    {
        #region Private enum - StandardSize
        /// <summary>
        /// An enumeration of the standard sizes for certain images.
        /// </summary>
        enum StandardSize
        {
            /// <summary>
            /// Not applicable / not specified.
            /// </summary>
            None,

            /// <summary>
            /// The original size of the stockImage.
            /// </summary>
            Full,

            /// <summary>
            /// The size of a thumbnail of an aircraft picture in the aircraft list.
            /// </summary>
            PictureListThumbnail,

            /// <summary>
            /// The size of an aircraft picture in the aircraft detail panel.
            /// </summary>
            PictureDetail,

            /// <summary>
            /// The size of an aircraft picture in the iPhone detail panel.
            /// </summary>
            IPhoneDetail,

            /// <summary>
            /// The size of an aircraft picture in the iPad detail panel.
            /// </summary>
            IPadDetail,

            /// <summary>
            /// The size of an aircraft picture conforming to the 200 x 133 standard.
            /// </summary>
            /// <remarks>
            /// If the original is larger than 200 x 133 it is scaled to fit on the longest edge
            /// and then cropped to 200 x 133 from the centre, as per <see cref="PictureListThumbnail"/>.
            /// If the original is smaller than 200x133 then it is zoomed up so that the shortest
            /// side is eith 200 or 133 and then cropped from the centre.
            /// </remarks>
            BaseStation,
        }
        #endregion

        #region Private class - ImageRequest
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
            public StandardSize Size;

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
        }
        #endregion

        #region Private enum - ResizeMode
        /// <summary>
        /// An enumeration of the different ways that <see cref="ResizeImage"/> can position an stockImage that has
        /// a different aspect ratio to the new stockImage size.
        /// </summary>
        enum ResizeMode
        {
            /// <summary>
            /// Image is drawn at top-left and is just large enough to fill the new space in both axis. One axis
            /// may end up being clipped.
            /// </summary>
            Normal,

            /// <summary>
            /// As per <see cref="Normal"/> but if an axis is clipped then the top or left is offset so that the centre is
            /// still in the middle of the new stockImage.
            /// </summary>
            Centre,

            /// <summary>
            /// Image is stretched to exactly fill the width and height of the new stockImage.
            /// </summary>
            Stretch,

            /// <summary>
            /// Image is centred within new stockImage, keeping the same aspect ratio as the original. One axis will be
            /// the same as the new stockImage, the other may be smaller. Unused space is filled with a brush.
            /// </summary>
            Zoom,
        }
        #endregion

        #region Fields
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

        /// <summary>
        /// The cache of known fonts.
        /// </summary>
        private static FontCache _FontCache = new FontCache();

        /// <summary>
        /// When true this blocks multithreaded access to image generation.
        /// </summary>
        /// <remarks>
        /// When multiple requests are received on many threads you can get two threads generating images at the same time. This is not a problem for
        /// .NET but under some Mono environments the native image library they use (libcairo) does not appear to be thread-safe. I don't want to degrade
        /// performance for .NET environments, under .NET I still want to allow multithreaded image generation, so this flag gets set when VRS is running
        /// under Mono and then when it's set a Monitor is used to force single-threaded access to image manipulation calls.
        /// </remarks>
        private bool _ForceSingleThreadAccess;

        /// <summary>
        /// The object on which <see cref="_ForceSingleThreadAccess"/> environments will lock.
        /// </summary>
        private static object _ForceSingleThreadAccessLock = new object();
        #endregion

        #region Properties
        private IImageFileManager _ImageFileManager;
        /// <summary>
        /// A private property that ensures we have a single instance of IImageFileManager in use and that it isn't
        /// created unless it's needed.
        /// </summary>
        private IImageFileManager ImageFileManager
        {
            get
            {
                if(_ImageFileManager == null) _ImageFileManager = Factory.Singleton.Resolve<IImageFileManager>();
                return _ImageFileManager;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ImagePage(WebSite webSite) : base(webSite)
        {
            _PictureManager = Factory.Singleton.Resolve<IAircraftPictureManager>().Singleton;
            _PictureFolderCache = Factory.Singleton.Resolve<IAutoConfigPictureFolderCache>().Singleton.DirectoryCache;
            _ForceSingleThreadAccess = Factory.Singleton.Resolve<IRuntimeEnvironment>().Singleton.IsMono;
        }
        #endregion

        #region DoLoadConfiguration, DoHandleRequest, UseImage
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
                Image stockImage = null;
                Image tempImage = null;

                if(result) {
                    if(_ForceSingleThreadAccess) Monitor.Enter(_ForceSingleThreadAccessLock);
                    try {
                        result = BuildInitialImage(imageRequest, args, ref stockImage, ref tempImage);
                        if(result) {
                            if(imageRequest.IsHighDpi)            tempImage = UseImage(tempImage, ResizeForHiDpi(tempImage ?? stockImage));
                            var addTextLines = imageRequest.HasTextLines && (!args.IsInternetRequest || _InternetClientCanShowText);
                            if(imageRequest.RotateDegrees > 0.0)  tempImage = UseImage(tempImage, RotateImage(tempImage ?? stockImage, imageRequest.RotateDegrees.Value));
                            if(imageRequest.Width != null)        tempImage = UseImage(tempImage, WidenImage(tempImage ?? stockImage, imageRequest.Width.Value, imageRequest.CentreImageHorizontally));
                            if(imageRequest.ShowAltitudeStalk)    tempImage = UseImage(tempImage, AddAltitudeStalk(tempImage ?? stockImage, imageRequest.Height.GetValueOrDefault(), imageRequest.CentreX.GetValueOrDefault()));
                            else if(imageRequest.Height != null)  tempImage = UseImage(tempImage, HeightenImage(tempImage ?? stockImage, imageRequest.Height.Value, imageRequest.CentreImageVertically));
                            if(addTextLines)                      tempImage = UseImage(tempImage, AddTextLines(tempImage ?? stockImage, imageRequest.TextLines, centreText: true, isHighDpi: imageRequest.IsHighDpi));
                        }

                        if(result) {
                            Responder.SendImage(args.Request, args.Response, tempImage ?? stockImage, imageRequest.ImageFormat);
                            args.Classification = ContentClassification.Image;
                        }
                    } finally {
                        if(tempImage != null) tempImage.Dispose();
                        if(_ForceSingleThreadAccess) Monitor.Exit(_ForceSingleThreadAccessLock);
                    }
                }
            }

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
        private Image UseImage(Image tempImage, Image newImage)
        {
            if(tempImage != null && !Object.ReferenceEquals(tempImage, newImage)) tempImage.Dispose();
            return newImage;
        }
        #endregion

        #region ExtractImageRequest
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
                else if(caselessPart.StartsWith("PL")) {
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

            switch(result.ImageName) {
                case "AIRPLANE":
                case "AIRPLANESELECTED":
                case "GROUNDVEHICLE":
                case "GROUNDVEHICLESELECTED":
                case "TOWER":
                case "TOWERSELECTED":
                    result.CentreImageVertically = false; break;
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
        private StandardSize ParseStandardSize(string text)
        {
            switch(text.ToUpper()) {
                case "DETAIL":          return StandardSize.PictureDetail;
                case "FULL":            return StandardSize.Full;
                case "LIST":            return StandardSize.PictureListThumbnail;
                case "IPADDETAIL":      return StandardSize.IPadDetail;
                case "IPHONEDETAIL":    return StandardSize.IPhoneDetail;
                case "BASESTATION":     return StandardSize.BaseStation;
            }

            return StandardSize.None;
        }
        #endregion

        #region BuildInitialImage, CreateLogoImage, CreateBlankImage, CreateAirplanePicture
        /// <summary>
        /// Fills either of the stock image or temporary image parameters with the initial image to use (before any alterations are made).
        /// </summary>
        /// <param name="imageRequest"></param>
        /// <param name="args"></param>
        /// <param name="stockImage"></param>
        /// <param name="tempImage"></param>
        /// <returns></returns>
        private bool BuildInitialImage(ImageRequest imageRequest, RequestReceivedEventArgs args, ref Image stockImage, ref Image tempImage)
        {
            bool result = true;

            switch(imageRequest.ImageName) {
                case "AIRPLANE":                stockImage = Images.Clone_Marker_Airplane(); break;
                case "AIRPLANESELECTED":        stockImage = Images.Clone_Marker_AirplaneSelected(); break;
                case "BLANK":                   tempImage  = CreateBlankImage(imageRequest.Width.GetValueOrDefault(), imageRequest.Height.GetValueOrDefault()); break;
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
                case "IPHONESPLASH":            tempImage  = CreateIPhoneSplash(args.WebSite, args.IsIPad, args.PathParts); break;
                case "IPHONETOOLBAR":           stockImage = Images.Clone_IPhoneToolbar(); break;
                case "IPHONETOOLBUTTON":        stockImage = Images.Clone_IPhoneToolButton(); break;
                case "IPHONEWHITEBUTTON":       stockImage = Images.Clone_IPhoneWhiteButton(); break;
                case "MINUS":                   stockImage = Images.Clone_Collapse(); break;
                case "MOVINGMAPCHECKED":        stockImage = Images.Clone_MovingMapChecked(); break;
                case "MOVINGMAPUNCHECKED":      stockImage = Images.Clone_MovingMapUnchecked(); break;
                case "OPENSLIDER":              stockImage = Images.Clone_OpenSlider(); break;
                case "OPFLAG":                  tempImage  = CreateLogoImage(imageRequest.File, _OperatorFlagFolder); break;
                case "PICTURE":                 tempImage  = CreateAirplanePicture(imageRequest.File, imageRequest.Size, args.IsInternetRequest, imageRequest.Width, imageRequest.Height); imageRequest.Width = imageRequest.Height = null; break;
                case "PLUS":                    stockImage = Images.Clone_Expand(); break;
                case "ROWHEADER":               stockImage = Images.Clone_RowHeader(); break;
                case "ROWHEADERSELECTED":       stockImage = Images.Clone_RowHeaderSelected(); break;
                case "SHOWLIST":                stockImage = Images.Clone_ShowList(); break;
                case "TESTSQUARE":              stockImage = Images.Clone_TestSquare(); break;
                case "TOWER":                   stockImage = Images.Clone_Tower(); break;
                case "TOWERSELECTED":           stockImage = Images.Clone_Tower(); break;
                case "TRANSPARENT-25":          stockImage = Images.Clone_Transparent_25(); break;
                case "TRANSPARENT-50":          stockImage = Images.Clone_Transparent_50(); break;
                case "TYPE":                    tempImage  = CreateLogoImage(imageRequest.File, _SilhouetteFolder); break;
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

            if(result) result = stockImage != null || tempImage != null;

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
                        if(File.Exists(fullPath)) {
                            result = ImageFileManager.LoadFromFile(fullPath);

                            if(result.Width != width)   result = UseImage(result, WidenImage(result, width, true));
                            if(result.Height != height) result = UseImage(result, HeightenImage(result, height, true));

                            break;
                        }
                    }
                }
            }

            return result ?? CreateBlankImage(width, height);
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
        private Image CreateAirplanePicture(string airplaneId, StandardSize standardSize, bool isInternetClient, int? width, int? height)
        {
            Bitmap result = null;

            if(_InternetClientCanViewPictures || !isInternetClient && !String.IsNullOrEmpty(airplaneId)) {
                var lastSpacePosn = airplaneId.LastIndexOf(' ');
                var registration = lastSpacePosn == -1 ? null : airplaneId.Substring(0, lastSpacePosn);
                var icao = airplaneId.Substring(lastSpacePosn + 1);

                if(registration != null && registration.Length == 0) registration = null;
                if(icao != null && icao.Length == 0) icao = null;

                result = (Bitmap)_PictureManager.LoadPicture(_PictureFolderCache, icao, registration);
                if(result != null) {
                    int newWidth = -1, newHeight = -1, minWidth = -1;
                    ResizeMode resizeMode = ResizeMode.Stretch;
                    bool preferSpeed = false;
                    switch(standardSize) {
                        case StandardSize.IPadDetail:           newWidth = 680; break;
                        case StandardSize.IPhoneDetail:         newWidth = 260; break;
                        case StandardSize.PictureDetail:        newWidth = 350; minWidth = 350; break;
                        case StandardSize.PictureListThumbnail: newWidth = 60; newHeight = 40; resizeMode = ResizeMode.Centre; break;
                        case StandardSize.BaseStation:          newWidth = 200; newHeight = 133; break;
                    }
                    if(width != null) newWidth = width.Value;
                    if(height != null) newHeight = height.Value;

                    if((newWidth != -1 || newHeight != -1) && (minWidth == -1 || result.Width > newWidth)) {
                        if(newWidth == -1)          newWidth = (int)(((double)newHeight * ((double)result.Width / (double)result.Height)) + 0.5);
                        else if(newHeight == -1)    newHeight = (int)(((double)newWidth / ((double)result.Width / (double)result.Height)) + 0.5);
                        result = (Bitmap)UseImage(result, ResizeImage(result, newWidth, newHeight, resizeMode, Brushes.Transparent, preferSpeed));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a fully-transparent image of the size specified.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private Image CreateBlankImage(int width, int height)
        {
            return width > 0 && height > 0 ? new Bitmap(width, height, PixelFormat.Format32bppArgb) : null;
        }

        /// <summary>
        /// Creates an iPhone splash page image.
        /// </summary>
        /// <param name="webSiteAddress"></param>
        /// <param name="isIPad"></param>
        /// <param name="pathParts"></param>
        /// <returns></returns>
        private Image CreateIPhoneSplash(string webSiteAddress, bool isIPad, List<string> pathParts)
        {
            if(!isIPad && pathParts.Where(pp => pp.Equals("file-ipad", StringComparison.OrdinalIgnoreCase)).Any()) isIPad = true;

            Image result;
            float titleSize, addressSize, lineHeight;
            if(!isIPad) {
                result = (Image)Images.IPhoneSplash.Clone();
                titleSize = 24f;
                addressSize = 12f;
                lineHeight = 40f;
            } else {
                result = (Image)Images.IPadSplash.Clone();
                titleSize = 36f;
                addressSize = 14f;
                lineHeight = 50f;
            }

            using(Graphics graphics = Graphics.FromImage(result)) {
                graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                graphics.SmoothingMode = SmoothingMode.HighQuality;

                RectangleF titleBounds = new RectangleF(5, (result.Height - 5) - (lineHeight * 2f), result.Width - 10f, lineHeight);
                RectangleF addressBounds = new RectangleF(5, titleBounds.Bottom + 5, result.Width - 10f, lineHeight);

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
            }

            return result;
        }
        #endregion

        #region RotateImage, WidenImage, HeightenImage, ResizeImage, AddAltitudeStalk, AddTextLines
        /// <summary>
        /// Rotates the image passed across by a number of degrees, running clockwise with 0 being north.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="degrees"></param>
        /// <returns></returns>
        private Image RotateImage(Image original, double degrees)
        {
            var result = new Bitmap(original.Width, original.Height, PixelFormat.Format32bppArgb);

            using(Graphics graphics = Graphics.FromImage(result)) {
                float centreX = ((float)original.Width) / 2f;
                float centreY = ((float)original.Height) / 2f;
                graphics.TranslateTransform(centreX, centreY);
                graphics.RotateTransform((float)degrees);
                graphics.DrawImage(original, -centreX, -centreY, original.Width, original.Height);
            }

            return result;
        }

        /// <summary>
        /// Widens the image and centres the original image within it. The new pixels are transparent.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="width"></param>
        /// <param name="centreHorizontally"></param>
        /// <returns></returns>
        private Image WidenImage(Image original, int width, bool centreHorizontally)
        {
            var result = new Bitmap(width, original.Height, PixelFormat.Format32bppArgb);

            using(Graphics graphics = Graphics.FromImage(result)) {
                int x = !centreHorizontally ? 0 : (width - original.Width) / 2;
                graphics.DrawImage(original, x, 0, original.Width, original.Height);
            }

            return result;
        }

        /// <summary>
        /// Heightens the image and centres the original image within it. The new pixels are transparent.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="height"></param>
        /// <param name="centreVertically"></param>
        /// <returns></returns>
        private Image HeightenImage(Image original, int height, bool centreVertically)
        {
            var result = new Bitmap(original.Width, height, PixelFormat.Format32bppArgb);

            using(Graphics graphics = Graphics.FromImage(result)) {
                int y = !centreVertically ? 0 : (height - original.Height) / 2;
                graphics.DrawImage(original, 0, y, original.Width, original.Height);
            }

            return result;
        }

        /// <summary>
        /// Doubles the width and height of the image for use on high DPI displays.
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        private Image ResizeForHiDpi(Image original)
        {
            var result = new Bitmap(original.Width * 2, original.Height * 2, PixelFormat.Format32bppArgb);

            using(var graphics = Graphics.FromImage(result)) {
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;     // <-- need this to preserve sharp edges on the doubled-up image
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(original, 0, 0, result.Width, result.Height);
            }

            return result;
        }

        /// <summary>
        /// Returns a new bitmap with the dimensions specified. The entire bitmap is filled with the
        /// background and then the original bitmap is drawn into the centre. If the aspect ratio of
        /// the original image is different to that of the new image then the ResizeMode indicates how
        /// to deal with it.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mode"></param>
        /// <param name="zoomBackground"></param>
        /// <param name="preferSpeedOverQuality"></param>
        /// <returns></returns>
        private Bitmap ResizeImage(Bitmap original, int width, int height, ResizeMode mode, Brush zoomBackground, bool preferSpeedOverQuality)
        {
            Bitmap result = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            using(Graphics graphics = Graphics.FromImage(result)) {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = preferSpeedOverQuality ? InterpolationMode.Bicubic : InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                int newWidth = width, newHeight = height, left = 0, top = 0;
                if(mode != ResizeMode.Stretch) {
                    double widthPercent = (double)newWidth / (double)original.Width;
                    double heightPercent = (double)newHeight / (double)original.Height;

                    switch(mode) {
                        case ResizeMode.Zoom:
                            // Resize the longest side by the smallest percentage
                            graphics.FillRectangle(zoomBackground, 0, 0, result.Width, result.Height);
                            if(widthPercent > heightPercent)        newWidth = Math.Min(newWidth, (int)(((double)original.Width * heightPercent) + 0.5));
                            else if(heightPercent > widthPercent)   newHeight = Math.Min(newHeight, (int)(((double)original.Height * widthPercent) + 0.5));
                            break;
                        case ResizeMode.Normal:
                        case ResizeMode.Centre:
                            // Resize the smallest side by the largest percentage
                            if(widthPercent > heightPercent)        newHeight = Math.Max(newHeight, (int)(((double)original.Height * widthPercent) + 0.5));
                            else if(heightPercent > widthPercent)   newWidth = Math.Max(newWidth, (int)(((double)original.Width * heightPercent) + 0.5));
                            break;
                    }

                    if(mode != ResizeMode.Normal) {
                        left = (width - newWidth) / 2;
                        top = (height - newHeight) / 2;
                    }
                }

                graphics.DrawImage(original, left, top, newWidth, newHeight);
            }

            return result;
        }

        /// <summary>
        /// Creates a new image of the required height with an altitude stalk drawn centred on the X
        /// pixel passed across.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="height"></param>
        /// <param name="centreX"></param>
        /// <returns></returns>
        private Image AddAltitudeStalk(Image original, int height, int centreX)
        {
            var result = new Bitmap(original.Width, height, PixelFormat.Format32bppArgb);

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

            return result;
        }

        /// <summary>
        /// Overlays lines of text onto the image.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="textLines"></param>
        /// <param name="centreText"></param>
        /// <param name="isHighDpi"></param>
        /// <returns></returns>
        private Image AddTextLines(Image image, IEnumerable<string> textLines, bool centreText, bool isHighDpi)
        {
            var lines = textLines.Where(tl => tl != null).ToList();
            var lineHeight = isHighDpi ? 24f : 12f;
            var topOffset = 5f;
            var startPointSize = isHighDpi ? 20f : 10f;
            var haloMod = isHighDpi ? 8 : 4;
            var haloTrans = isHighDpi ? 0.125f : 0.25f;
            var left = 0f;
            var top = (image.Height - topOffset) - (lines.Count * lineHeight);

            Image result = (Image)image.Clone();

            using(Graphics graphics = Graphics.FromImage(result)) {
                StringFormat stringFormat = new StringFormat() {
                    Alignment = centreText ? StringAlignment.Center : StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoWrap,
                };

                graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

                using(GraphicsPath path = new GraphicsPath()) {
                    float lineTop = top;
                    foreach(string line in lines) {
                        Font font = GetFontForRectangle("MS Sans Serif", FontStyle.Regular, startPointSize, graphics, (float)result.Width - left, lineHeight * 2f, line);
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

            return result;
        }
        #endregion

        #region GetFontForRectangle
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
        #endregion
    }
}
