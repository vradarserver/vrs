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
using System.IO;
using System.Linq;
using System.Web;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Resources;
using VrsDrawing = VirtualRadar.Interface.Drawing;

namespace Test.VirtualRadar.WebSite.Middleware
{
    [TestClass]
    public class ImageServerTests
    {
        public TestContext TestContext { get; set; }
        private IClassFactory _Snapshot;

        private IImageServer _Server;
        private MockOwinEnvironment _Environment;
        private MockOwinPipeline _Pipeline;
        private Mock<IImageServerConfiguration> _ServerConfiguration;
        private MockFileSystemProvider _FileSystem;
        private Mock<IFileSystemServerConfiguration> _FileSystemServerConfiguration;
        private Configuration _ProgramConfiguration;
        private Mock<ISharedConfiguration> _SharedConfiguration;
        private Mock<IImageFileManager> _ImageFileManager;
        private Mock<IAircraftPictureManager> _AircraftPictureManager;
        private Mock<IAutoConfigPictureFolderCache> _AutoConfigurePictureCache;
        private Mock<IDirectoryCache> _AircraftPictureCache;

        private Color _Black = Color.FromArgb(0, 0, 0);
        private Color _White = Color.FromArgb(255, 255, 255);
        private Color _Red = Color.FromArgb(255, 0, 0);
        private Color _Green = Color.FromArgb(0, 255, 0);
        private Color _Blue = Color.FromArgb(0, 0, 255);
        private Color _Transparent = Color.FromArgb(0, 0, 0, 0);

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _ProgramConfiguration = new global::VirtualRadar.Interface.Settings.Configuration();
            _SharedConfiguration = TestUtilities.CreateMockSingleton<ISharedConfiguration>();
            _SharedConfiguration.Setup(r => r.Get()).Returns(_ProgramConfiguration);

            _FileSystem = new MockFileSystemProvider();
            Factory.RegisterInstance<IFileSystemProvider>(_FileSystem);

            _FileSystemServerConfiguration = TestUtilities.CreateMockSingleton<IFileSystemServerConfiguration>();
            _FileSystemServerConfiguration.Setup(r => r.GetSiteRootFolders()).Returns(() => new List<string>() {
                @"c:\web\",
            });
            _FileSystemServerConfiguration.Setup(r => r.IsFileUnmodified(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>())).Returns(true);

            _ImageFileManager = TestUtilities.CreateMockImplementation<IImageFileManager>();
            _AircraftPictureManager = TestUtilities.CreateMockSingleton<IAircraftPictureManager>();
            _AircraftPictureManager.Setup(r => r.FindPicture(It.IsAny<IDirectoryCache>(), It.IsAny<string>(), It.IsAny<string>())).Returns((PictureDetail)null);
            _AircraftPictureManager.Setup(r => r.LoadPicture(It.IsAny<IDirectoryCache>(), It.IsAny<string>(), It.IsAny<string>())).Returns((VrsDrawing.IImage)null);
            _AircraftPictureCache = TestUtilities.CreateMockImplementation<IDirectoryCache>();
            _AutoConfigurePictureCache = TestUtilities.CreateMockSingleton<IAutoConfigPictureFolderCache>();
            _AutoConfigurePictureCache.SetupGet(r => r.DirectoryCache).Returns(_AircraftPictureCache.Object);
            _ServerConfiguration = TestUtilities.CreateMockSingleton<IImageServerConfiguration>();
            _ServerConfiguration.SetupGet(r => r.ImageFileManager).Returns(_ImageFileManager.Object);

            _Server = Factory.Resolve<IImageServer>();

            _Environment = new MockOwinEnvironment();
            _Environment.ServerRemoteIpAddress = "127.0.0.1";
            _Pipeline = new MockOwinPipeline();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        private string CreateMonochromeImage(string fileName, int width, int height, Brush color, string folder = @"c:\web")
        {
            var fullPath = Path.Combine(folder, fileName);

            ImageFormat imageFormat = null;
            var extension = Path.GetExtension(fileName);
            if(extension.Equals(".bmp", StringComparison.InvariantCultureIgnoreCase)) {
                imageFormat = ImageFormat.Bmp;
            }
            else if (extension.Equals(".png", StringComparison.InvariantCultureIgnoreCase)) {
                imageFormat = ImageFormat.Png;
            }
            else {
                throw new NotImplementedException();
            }

            using(var stream = new MemoryStream()) {
                using(var image = new Bitmap(width, height, PixelFormat.Format32bppArgb)) {
                    using(var graphics = Graphics.FromImage(image)) {
                        graphics.FillRectangle(color, 0, 0, width, height);
                    }
                    AddFileSystemImageFile(image, fileName, imageFormat, folder);
                }
            }

            return fullPath;
        }

        private void AddFileSystemImageFile(Image image, string fileName, ImageFormat imageFormat, string path = @"c:\web")
        {
            using(var stream = new MemoryStream()) {
                using(var imageCopy = (Image)image.Clone()) {
                    imageCopy.Save(stream, imageFormat);
                    _FileSystem.AddFile(Path.Combine(path, fileName), stream.ToArray());
                }
            }
        }

        private void ConfigurePictureManagerForFile(string imageFullPath, int width, int height, string icao24 = null, string registration = null, DateTime? lastModifiedTime = null)
        {
            if(icao24 != null || registration != null) {
                _AircraftPictureManager.Setup(r => r.FindPicture(_AircraftPictureCache.Object, icao24, registration))
                .Returns((IDirectoryCache cache, string i1, string r1) => {
                    var result = GetFakePictureDetail(imageFullPath, width, height, lastModifiedTime);
                    return result;
                });

                _AircraftPictureManager.Setup(r => r.LoadPicture(_AircraftPictureCache.Object, icao24, registration))
                .Returns((IDirectoryCache cache, string i1, string r1) => {
                    var result = GetFakePicture(imageFullPath);
                    return result;
                });
            }
        }

        private PictureDetail GetFakePictureDetail(string imageFullPath, int width, int height, DateTime? lastModifiedTime)
        {
            PictureDetail result = null;
            var bytes = _FileSystem.FileExists(imageFullPath) ? _FileSystem.FileReadAllBytes(imageFullPath) : null;
            if(bytes != null) {
                result = new PictureDetail() {
                    FileName =          imageFullPath,
                    Height =            height,
                    Width =             width,
                    Length =            bytes.Length,
                    LastModifiedTime =  lastModifiedTime == null ? DateTime.UtcNow : lastModifiedTime.Value,
                };
            }

            return result;
        }

        private VrsDrawing.IImage GetFakePicture(string imageFullPath)
        {
            VrsDrawing.IImage result = null;
            var bytes = _FileSystem.FileExists(imageFullPath) ? _FileSystem.FileReadAllBytes(imageFullPath) : null;
            if(bytes != null) {
                var imageFile = Factory.ResolveSingleton<VrsDrawing.IImageFile>();
                result = imageFile.LoadFromByteArray(bytes);
            }

            return result;
        }

        /// <summary>
        /// Checks that the image is a swathe of a single colour.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="color"></param>
        /// <param name="expectedHeight"></param>
        /// <param name="expectedWidth"></param>
        private void AssertImageIsMonochrome(Bitmap image, Color color, int expectedWidth = -1, int expectedHeight = -1)
        {
            if(expectedWidth > -1) Assert.AreEqual(expectedWidth, image.Width);
            if(expectedHeight > -1) Assert.AreEqual(expectedHeight, image.Height);

            for(var y = 0;y < image.Height;++y) {
                for(var x = 0;x < image.Width;++x) {
                    CompareColours(color, image.GetPixel(x, y), x, y);
                }
            }
        }

        /// <summary>
        /// Checks that two images are identical.
        /// </summary>
        /// <param name="expectedImage"></param>
        /// <param name="actualImage"></param>
        private void AssertImagesAreIdentical(Bitmap expectedImage, Bitmap actualImage, string message = "")
        {
            Assert.AreEqual(expectedImage.Height, actualImage.Height, message);
            Assert.AreEqual(expectedImage.Width, actualImage.Width, message);

            for(var y = 0;y < expectedImage.Height;++y) {
                for(var x = 0;x < expectedImage.Width;++x) {
                    var expectedPixel = expectedImage.GetPixel(x, y);
                    var actualPixel = actualImage.GetPixel(x, y);

                    CompareColours(expectedPixel, actualPixel, x, y, message);
                }
            }
        }

        /// <summary>
        /// Checks that two images are not identical.
        /// </summary>
        /// <param name="expectedImage"></param>
        /// <param name="actualImage"></param>
        private void AssertImagesAreNotIdentical(Bitmap expectedImage, Bitmap actualImage, string message = "")
        {
            var identical = expectedImage.Height == actualImage.Height && expectedImage.Width == actualImage.Width;
            if(identical) {
                for(var y = 0;identical && y < expectedImage.Height;++y) {
                    for(var x = 0;identical && x < expectedImage.Width;++x) {
                        var expectedPixel = expectedImage.GetPixel(x, y);
                        var actualPixel = actualImage.GetPixel(x, y);

                        identical = expectedPixel == actualPixel;
                    }
                }
            }

            Assert.IsFalse(identical, message);
        }

        private void CompareColours(Color expectedPixel, Color actualPixel, int x, int y, string message = "")
        {
            if(expectedPixel.A == 0)    Assert.IsTrue(actualPixel.A < 250, "x = {0}, y = {1} {2}", x, y, message);
            else                        Assert.IsTrue(actualPixel.A > 0, "x = {0}, y = {1} {2}", x, y, message);

            if(expectedPixel.A > 0) {
                var colourDelta = 5.0;
                Assert.AreEqual(expectedPixel.R, actualPixel.R, colourDelta, "x = {0}, y = {1} {2}", x, y, message);
                Assert.AreEqual(expectedPixel.G, actualPixel.G, colourDelta, "x = {0}, y = {1} {2}", x, y, message);
                Assert.AreEqual(expectedPixel.B, actualPixel.B, colourDelta, "x = {0}, y = {1} {2}", x, y, message);
            }
        }

        private Mock<IWebSiteGraphics> ReplaceWebSiteGraphics()
        {
            var result = TestUtilities.CreateMockSingleton<IWebSiteGraphics>();
            result.Setup(r => r.UseImage(It.IsAny<VrsDrawing.IImage>(), It.IsAny<VrsDrawing.IImage>()))
                           .Returns((VrsDrawing.IImage tempImage, VrsDrawing.IImage newImage) => {
                                return newImage;
                           });

            return result;
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "StockResourceImages$")]
        public void ImageServer_Serves_All_Stock_Resource_Images()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            var requestImageName = worksheet.String("RequestImageName");
            var stockImagePropertyName = worksheet.String("StockImage");

            var getImageBytesMethod = typeof(Images).GetMethod($"{stockImagePropertyName}", new Type[0]);
            var expectedImageBytes = (byte[])getImageBytesMethod.Invoke(null, new object[0]);
            Bitmap expectedImage;
            using(var memoryStream = new MemoryStream(expectedImageBytes)) {
                expectedImage = (Bitmap)Bitmap.FromStream(memoryStream);
            }
            try {
                _Environment.RequestPath = $"/Images/{requestImageName}.png";
                _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

                using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                    using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                        AssertImagesAreIdentical(expectedImage, siteImage);
                    }
                }
            } finally {
                expectedImage.Dispose();
            }
        }

        [TestMethod]
        public void ImageServer_Returns_Correct_Status_For_Resource_Images()
        {
            _Environment.ResponseStatusCode = 404;
            _Environment.RequestPath = $"/Images/TestSquare.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            Assert.AreEqual(200, _Environment.ResponseStatusCode);
            Assert.IsFalse(_Pipeline.NextMiddlewareCalled);
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "BlankImage$")]
        public void ImageServer_Can_Create_Blank_Images_Dynamically()
        {
            ExcelWorksheetData worksheet = new ExcelWorksheetData(TestContext);

            _Environment.RequestPath = worksheet.String("PathAndFile");
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            if(worksheet.String("Width") == null) {
                Assert.AreEqual(0, _Environment.ResponseBodyBytes.Length);
                Assert.IsTrue(_Pipeline.NextMiddlewareCalled);
            } else {
                Assert.IsFalse(_Pipeline.NextMiddlewareCalled);
                using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                    using(var image = (Bitmap)Bitmap.FromStream(stream)) {
                        AssertImageIsMonochrome(image, _Transparent, worksheet.Int("Width"), worksheet.Int("Height"));
                    }
                }
            }
        }

        [TestMethod]
        public void ImageServer_Returns_Correct_Status_For_Dynamic_Images()
        {
            _Environment.ResponseStatusCode = 404;
            _Environment.RequestPath = $"/Images/Wdth-700/Hght-250/Blank.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            Assert.AreEqual(200, _Environment.ResponseStatusCode);
            Assert.IsFalse(_Pipeline.NextMiddlewareCalled);
        }

        [TestMethod]
        public void ImageServer_Can_Dynamically_Rotate_Images()
        {
            foreach(var rotateDegrees in new int[] { 0, 90, 180, 270 }) {
                TestCleanup();
                TestInitialise();

                _Environment.RequestPath = $"/Images/Rotate-{rotateDegrees}/TestSquare.png";
                _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

                using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                    using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                        Assert.AreEqual(9, siteImage.Width);
                        Assert.AreEqual(9, siteImage.Height);

                        // Determine the colours we expect to see at the 12 o'clock, 3 o'clock, 6 o'clock and 9 o'clock positions
                        Color p12 = _White, p3 = _White, p6 = _White, p9 = _White;
                        switch(rotateDegrees) {
                            case 0: p12 = _Black; p3 = _Green; p6 = _Blue; p9 = _Red; break;
                            case 90: p12 = _Red; p3 = _Black; p6 = _Green; p9 = _Blue; break;
                            case 180: p12 = _Blue; p3 = _Red; p6 = _Black; p9 = _Green; break;
                            case 270: p12 = _Green; p3 = _Blue; p6 = _Red; p9 = _Black; break;
                        }

                        Assert.AreEqual(p12, siteImage.GetPixel(4, 1), rotateDegrees.ToString());
                        Assert.AreEqual(p3, siteImage.GetPixel(7, 4), rotateDegrees.ToString());
                        Assert.AreEqual(p6, siteImage.GetPixel(4, 7), rotateDegrees.ToString());
                        Assert.AreEqual(p9, siteImage.GetPixel(1, 4), rotateDegrees.ToString());
                    }
                }
            }
        }

        [TestMethod]
        public void ImageServer_Can_Dynamically_Widen_Images()
        {
            _Environment.RequestPath = "/Images/Wdth-11/TestSquare.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    Assert.AreEqual(11, siteImage.Width);
                    Assert.AreEqual(9, siteImage.Height);

                    foreach(var x in new int[] { 0, 10 }) {
                        for(var y = 0;y < 9;++y) {
                            Assert.AreEqual(_Transparent, siteImage.GetPixel(x, y), "x = {0}, y = {1}", x, y);
                        }
                    }

                    Assert.AreEqual(_White, siteImage.GetPixel(1, 0));
                    Assert.AreEqual(_Black, siteImage.GetPixel(4, 0));
                    Assert.AreEqual(_Red, siteImage.GetPixel(1, 3));
                    Assert.AreEqual(_Green, siteImage.GetPixel(9, 3));
                    Assert.AreEqual(_Blue, siteImage.GetPixel(4, 8));
                }
            }
        }

        [TestMethod]
        public void ImageServer_Can_Dynamically_Change_Height_Of_Images()
        {
            _Environment.RequestPath = "/Images/Hght-11/TestSquare.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    Assert.AreEqual(9, siteImage.Width);
                    Assert.AreEqual(11, siteImage.Height);

                    for(var x = 0;x < 9;++x) {
                        foreach(var y in new int[] { 0, 10 }) {
                            Assert.AreEqual(_Transparent, siteImage.GetPixel(x, y), "x = {0}, y = {1}", x, y);
                        }

                        Assert.AreEqual(_White, siteImage.GetPixel(0, 1));
                        Assert.AreEqual(_Black, siteImage.GetPixel(3, 1));
                        Assert.AreEqual(_Red, siteImage.GetPixel(0, 4));
                        Assert.AreEqual(_Green, siteImage.GetPixel(8, 4));
                        Assert.AreEqual(_Blue, siteImage.GetPixel(3, 9));
                    }
                }
            }
        }

        [TestMethod]
        public void ImageServer_Dynamically_Resizes_For_HighDpi_Devices()
        {
            _Environment.RequestPath = "/Images/hiDpi/TestSquare.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    Assert.AreEqual(18, siteImage.Width);
                    Assert.AreEqual(18, siteImage.Height);
                }
            }
        }

        [TestMethod]
        public void ImageServer_Can_Dynamically_Add_Altitude_Stalk()
        {
            _Environment.RequestPath = "/Images/Hght-15/CenX-4/Alt-/TestSquare.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    AssertImagesAreIdentical(TestImages.AltitudeImageTest_01_png_Bitmap, siteImage);
                }
            }
        }

        [TestMethod]
        public void ImageServer_Can_Dynamically_Add_Text()
        {
            var textLines = new List<string>();
            var webSiteGraphics = ReplaceWebSiteGraphics();
            webSiteGraphics.Setup(r => r.AddTextLines(It.IsAny<VrsDrawing.IImage>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>()))
                           .Returns((VrsDrawing.IImage image, IEnumerable<string> lines, bool unused2, bool unused3) => {
                                textLines.AddRange(lines);
                                return image.Clone();
                           });
            _Server = Factory.Resolve<IImageServer>();

            _Environment.RequestPath = "/Images/PL1-Hello/PL2-There/TestSquare.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            webSiteGraphics.Verify(r => r.AddTextLines(It.IsAny<VrsDrawing.IImage>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
            Assert.AreEqual(2, textLines.Count);
            Assert.AreEqual("Hello", textLines[0]);
            Assert.AreEqual("There", textLines[1]);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    // Note that in real life IWebSiteGraphics would have done something with the image, but here all
                    // we can reasonably do is make sure that something that looks like an image was returned...
                }
            }
        }

        [TestMethod]
        public void ImageServer_Will_Not_Dynamically_Add_Text_If_Configuration_Prohibits_It()
        {
            // Get an image without text to start with
            _Environment.RequestPath = "/Images/Hght-200/Wdth-60/Airplane.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);
            byte[] imageWithoutText = _Environment.ResponseBodyBytes;

            // Ask for the same image from the Internet but with a line of text 
            _Environment.Reset();
            _ProgramConfiguration.InternetClientSettings.CanShowPinText = true;
            _Environment.RequestPath = "/Images/PL1-X/Hght-200/Wdth-60/Airplane.png";
            _Environment.ServerRemoteIpAddress = "1.2.3.4";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);
            byte[] internetWithText = _Environment.ResponseBodyBytes;
            Assert.IsFalse(imageWithoutText.SequenceEqual(internetWithText));

            // Ask for same image with text from the Internet when the configuration prohibits it
            _Environment.Reset();
            _ProgramConfiguration.InternetClientSettings.CanShowPinText = false;
            _Environment.RequestPath = "/Images/PL1-X/Hght-200/Wdth-60/Airplane.png";
            _Environment.ServerRemoteIpAddress = "1.2.3.4";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);
            byte[] internetWithoutText = _Environment.ResponseBodyBytes;
            Assert.IsTrue(imageWithoutText.SequenceEqual(internetWithoutText));

            // Ask for the same image from the LAN but with a line of text 
            _Environment.Reset();
            _ProgramConfiguration.InternetClientSettings.CanShowPinText = true;
            _Environment.RequestPath = "/Images/PL1-X/Hght-200/Wdth-60/Airplane.png";
            _Environment.ServerRemoteIpAddress = "192.168.2.3";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);
            byte[] lanWithText = _Environment.ResponseBodyBytes;
            Assert.IsFalse(imageWithoutText.SequenceEqual(lanWithText));

            // Ask for same image with text from the LAN when the configuration prohibits it
            _Environment.Reset();
            _ProgramConfiguration.InternetClientSettings.CanShowPinText = false;
            _Environment.RequestPath = "/Images/PL1-X/Hght-200/Wdth-60/Airplane.png";
            _Environment.ServerRemoteIpAddress = "192.168.2.3";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);
            byte[] lanWithoutText = _Environment.ResponseBodyBytes;
            Assert.IsFalse(imageWithoutText.SequenceEqual(lanWithoutText));
        }

        [TestMethod]
        public void ImageServer_Can_Serve_Operator_Logos()
        {
            _ServerConfiguration.SetupGet(r => r.OperatorFolder).Returns(@"c:\flags");
            AddFileSystemImageFile(TestImages.DLH_bmp_Bitmap, "DLH.bmp", ImageFormat.Bmp, @"c:\flags");
            _ImageFileManager.Setup(r => r.LoadFromFile(@"c:\flags\DLH.bmp")).Returns(TestImages.DLH_bmp_IImage);

            _Environment.RequestPath = "/Images/File-DLH/OpFlag.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    AssertImagesAreIdentical(TestImages.DLH_bmp_Bitmap, siteImage);
                }
            }
        }

        [TestMethod]
        public void ImageServer_Can_Serve_Silhouettes()
        {
            _ServerConfiguration.SetupGet(r => r.SilhouettesFolder).Returns(@"c:\types");
            AddFileSystemImageFile(TestImages.DLH_bmp_Bitmap, "DLH.bmp", ImageFormat.Bmp, @"c:\types");
            _ImageFileManager.Setup(r => r.LoadFromFile(@"c:\types\DLH.bmp")).Returns(TestImages.DLH_bmp_IImage);

            _Environment.RequestPath = "/Images/File-DLH/Type.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    AssertImagesAreIdentical(TestImages.DLH_bmp_Bitmap, siteImage);
                }
            }
        }

        [TestMethod]
        public void ImageServer_Can_Serve_Alternative_Operator_Logos()
        {
            _ServerConfiguration.SetupGet(r => r.OperatorFolder).Returns(@"c:\flags");
            AddFileSystemImageFile(TestImages.DLH_bmp_Bitmap, "DLH.bmp", ImageFormat.Bmp, @"c:\flags");
            _ImageFileManager.Setup(r => r.LoadFromFile(@"c:\flags\DLH.bmp")).Returns(TestImages.DLH_bmp_IImage);

            _Environment.RequestPath = "/Images/File-DOESNOTEXIST|DLH/OpFlag.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    AssertImagesAreIdentical(TestImages.DLH_bmp_Bitmap, siteImage);
                }
            }
        }

        [TestMethod]
        public void ImageServer_Can_Serve_Alternative_Silhouettes()
        {
            _ServerConfiguration.SetupGet(r => r.SilhouettesFolder).Returns(@"c:\types");
            AddFileSystemImageFile(TestImages.DLH_bmp_Bitmap, "DLH.bmp", ImageFormat.Bmp, @"c:\types");
            _ImageFileManager.Setup(r => r.LoadFromFile(@"c:\types\DLH.bmp")).Returns(TestImages.DLH_bmp_IImage);

            _Environment.RequestPath = "/Images/File-DOESNOTEXIST|DLH/Type.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    AssertImagesAreIdentical(TestImages.DLH_bmp_Bitmap, siteImage);
                }
            }
        }

        [TestMethod]
        public void ImageServer_Resizes_Small_Operator_Logos_To_Fit_Standard_Size()
        {
            _ServerConfiguration.SetupGet(r => r.OperatorFolder).Returns(@"c:\flags");
            AddFileSystemImageFile(TestImages.TestSquare_bmp_Bitmap, "TestSquare.bmp", ImageFormat.Bmp, @"c:\flags");
            _ImageFileManager.Setup(r => r.LoadFromFile(@"c:\flags\TestSquare.bmp")).Returns(TestImages.TestSquare_bmp_IImage);

            _Environment.RequestPath = "/Images/File-TestSquare/OpFlag.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    Assert.AreEqual(85, siteImage.Width);
                    Assert.AreEqual(20, siteImage.Height);

                    // Should have placed the small image (TestSquare is 9x9) in the centre as per rules for WDTH and HGHT
                    Assert.AreEqual(_Transparent, siteImage.GetPixel(0, 0));
                    Assert.AreEqual(_White, siteImage.GetPixel(39, 7));
                    Assert.AreEqual(_Black, siteImage.GetPixel(42, 7));
                }
            }
        }

        [TestMethod]
        public void ImageServer_Resizes_Small_Silhouettes_To_Fit_Standard_Size()
        {
            _ServerConfiguration.SetupGet(r => r.SilhouettesFolder).Returns(@"c:\types");
            AddFileSystemImageFile(TestImages.TestSquare_bmp_Bitmap, "TestSquare.bmp", ImageFormat.Bmp, @"c:\types");
            _ImageFileManager.Setup(r => r.LoadFromFile(@"c:\types\TestSquare.bmp")).Returns(TestImages.TestSquare_bmp_IImage);

            _Environment.RequestPath = "/Images/File-TestSquare/Type.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    Assert.AreEqual(85, siteImage.Width);
                    Assert.AreEqual(20, siteImage.Height);

                    // Should have placed the small image (TestSquare is 9x9) in the centre as per rules for WDTH and HGHT
                    Assert.AreEqual(_Transparent, siteImage.GetPixel(0, 0));
                    Assert.AreEqual(_White, siteImage.GetPixel(39, 7));
                    Assert.AreEqual(_Black, siteImage.GetPixel(42, 7));
                }
            }
        }

        [TestMethod]
        public void ImageServer_Resizes_Large_Operator_Logos_To_Fit_Standard_Size()
        {
            _ServerConfiguration.SetupGet(r => r.OperatorFolder).Returns(@"c:\flags");
            AddFileSystemImageFile(TestImages.OversizedLogo_bmp_Bitmap, "OversizeLogo.bmp", ImageFormat.Bmp, @"c:\flags");
            _ImageFileManager.Setup(r => r.LoadFromFile(@"c:\flags\OversizeLogo.bmp")).Returns(TestImages.OversizedLogo_bmp_IImage);

            _Environment.RequestPath = "/Images/File-OversizeLogo/OpFlag.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    Assert.AreEqual(85, siteImage.Width);
                    Assert.AreEqual(20, siteImage.Height);

                    // Should have placed the large image in the centre - the image is 87x22 whereas the standard size is 85x20 so by centreing
                    // it we should have cropped a 1 pixel border off the image, which leaves a black pixel in each corner
                    Assert.AreEqual(_Black, siteImage.GetPixel(0, 0));
                    Assert.AreEqual(_White, siteImage.GetPixel(1, 0));
                    Assert.AreEqual(_Black, siteImage.GetPixel(84, 0));
                    Assert.AreEqual(_Black, siteImage.GetPixel(0, 19));
                    Assert.AreEqual(_Black, siteImage.GetPixel(84, 19));
                }
            }
        }

        [TestMethod]
        public void ImageServer_Resizes_Large_Silhouettes_To_Fit_Standard_Size()
        {
            _ServerConfiguration.SetupGet(r => r.SilhouettesFolder).Returns(@"c:\types");
            AddFileSystemImageFile(TestImages.OversizedLogo_bmp_Bitmap, "OversizeLogo.bmp", ImageFormat.Bmp, @"c:\types");
            _ImageFileManager.Setup(r => r.LoadFromFile(@"c:\types\OversizeLogo.bmp")).Returns(TestImages.OversizedLogo_bmp_IImage);

            _Environment.RequestPath = "/Images/File-OversizeLogo/Type.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    Assert.AreEqual(85, siteImage.Width);
                    Assert.AreEqual(20, siteImage.Height);

                    // Should have placed the large image in the centre - the image is 87x22 whereas the standard size is 85x20 so by centreing
                    // it we should have cropped a 1 pixel border off the image, which leaves a black pixel in each corner
                    Assert.AreEqual(_Black, siteImage.GetPixel(0, 0));
                    Assert.AreEqual(_White, siteImage.GetPixel(1, 0));
                    Assert.AreEqual(_Black, siteImage.GetPixel(84, 0));
                    Assert.AreEqual(_Black, siteImage.GetPixel(0, 19));
                    Assert.AreEqual(_Black, siteImage.GetPixel(84, 19));
                }
            }
        }

        [TestMethod]
        public void ImageServer_Returns_Blank_When_OperatorFolder_Not_Configured()
        {
            _ServerConfiguration.SetupGet(r => r.OperatorFolder).Returns((string)null);
            _Environment.RequestPath = "/Images/File-BA/OpFlag.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    AssertImageIsMonochrome(siteImage, Color.Transparent, 85, 20);
                }
            }
        }

        [TestMethod]
        public void ImageServer_Returns_Blank_When_SilhouettesFolder_Not_Configured()
        {
            _ServerConfiguration.SetupGet(r => r.SilhouettesFolder).Returns((string)null);
            _Environment.RequestPath = "/Images/File-BA/Type.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    AssertImageIsMonochrome(siteImage, Color.Transparent, 85, 20);
                }
            }
        }

        [TestMethod]
        public void ImageServer_Returns_Blank_When_OperatorFlag_Not_Found()
        {
            _ServerConfiguration.SetupGet(r => r.OperatorFolder).Returns(@"c:\flags");
            _Environment.RequestPath = "/Images/File-DLH/OpFlag.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    AssertImageIsMonochrome(siteImage, Color.Transparent, 85, 20);
                }
            }
        }

        [TestMethod]
        public void ImageServer_Returns_Blank_When_Silhouette_Not_Found()
        {
            _ServerConfiguration.SetupGet(r => r.SilhouettesFolder).Returns(@"c:\types");
            _Environment.RequestPath = "/Images/File-DLH/Type.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    AssertImageIsMonochrome(siteImage, Color.Transparent, 85, 20);
                }
            }
        }

        [TestMethod]
        public void ImageServer_Returns_Blank_When_OperatorFlagCode_Contains_Invalid_Characters()
        {
            _ServerConfiguration.SetupGet(r => r.OperatorFolder).Returns(@"c:\flags");

            foreach(var badChar in Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars())) {
                var fileName = "BA" + badChar;
                _Environment.Reset();
                _Environment.RequestPath = String.Format("/Images/File-{0}/OpFlag.png", HttpUtility.UrlEncode(fileName));
                _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

                using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                    using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                        AssertImageIsMonochrome(siteImage, Color.Transparent, 85, 20);
                    }
                }
            }
        }

        [TestMethod]
        public void ImageServer_Returns_Blank_When_Silhouette_Contains_Invalid_Characters()
        {
            _ServerConfiguration.SetupGet(r => r.SilhouettesFolder).Returns(@"c:\types");

            foreach(var badChar in Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars())) {
                var fileName = "A380" + badChar;
                _Environment.Reset();
                _Environment.RequestPath = String.Format("/Images/File-{0}/Type.png", HttpUtility.UrlEncode(fileName));
                _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

                using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                    using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                        AssertImageIsMonochrome(siteImage, Color.Transparent, 85, 20);
                    }
                }
            }
        }

        [TestMethod]
        public void ImageServer_Returns_Blank_When_OperatorFlag_Includes_Directory_Traversal()
        {
            _ServerConfiguration.SetupGet(r => r.OperatorFolder).Returns(@"c:\flags\subfolder");
            _FileSystem.AddFolder(@"c:\flags\subfolder");
            AddFileSystemImageFile(TestImages.DLH_bmp_Bitmap, "DLH.bmp", ImageFormat.Bmp, @"c:\flags");
            _ImageFileManager.Setup(r => r.LoadFromFile(@"c:\flags\DLH.bmp")).Returns(TestImages.DLH_bmp_IImage);

            _Environment.RequestPath = "/Images/File-..\\DLH/OpFlag.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    AssertImageIsMonochrome(siteImage, Color.Transparent, 85, 20);
                }
            }
        }

        [TestMethod]
        public void ImageServer_Returns_Blank_When_Silhouette_Includes_Directory_Traversal()
        {
            _ServerConfiguration.SetupGet(r => r.SilhouettesFolder).Returns(@"c:\types\subfolder");
            _FileSystem.AddFolder(@"c:\types\subfolder");
            AddFileSystemImageFile(TestImages.DLH_bmp_Bitmap, "DLH.bmp", ImageFormat.Bmp, @"c:\types");
            _ImageFileManager.Setup(r => r.LoadFromFile(@"c:\types\DLH.bmp")).Returns(TestImages.DLH_bmp_IImage);

            _Environment.RequestPath = "/Images/File-..\\DLH/Type.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    AssertImageIsMonochrome(siteImage, Color.Transparent, 85, 20);
                }
            }
        }

        [TestMethod]
        public void ImageServer_Can_Return_IPhone_Splash_Screen()
        {
            // The content of this is built dynamically and while we could compare checksums I suspect that over different machines you'd get
            // slightly different results. So this test just checks that if you ask for a splash screen then you get something back that's the
            // right size and has what looks to be the correct colour background.
            _Environment.RequestPath = "/Images/IPhoneSplash.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    Assert.AreEqual(320, siteImage.Width);
                    Assert.AreEqual(460, siteImage.Height);
                    Assert.AreEqual(_Black, siteImage.GetPixel(10, 10));
                }
            }
        }

        [TestMethod]
        public void ImageServer_Can_Return_IPad_Splash_Screen_Via_UserAgent_String()
        {
            // See notes on iPhone version
            _Environment.RequestHeaders["User-Agent"] = "(iPad;something)";
            _Environment.RequestPath = "/Images/IPhoneSplash.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    Assert.AreEqual(768, siteImage.Width);
                    Assert.AreEqual(1004, siteImage.Height);
                    Assert.AreEqual(_Black, siteImage.GetPixel(10, 10));
                }
            }
        }

        [TestMethod]
        public void ImageServer_Can_Return_IPad_Splash_Screen_Via_Explicit_Instruction()
        {
            // See notes on iPhone version

            _Environment.RequestPath = "/Images/File-IPad/IPhoneSplash.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    Assert.AreEqual(768, siteImage.Width);
                    Assert.AreEqual(1004, siteImage.Height);
                    Assert.AreEqual(_Black, siteImage.GetPixel(10, 10));
                }
            }
        }

        [TestMethod]
        public void ImageServer_Can_Display_Aircraft_Picture_Correctly()
        {
            CreateMonochromeImage("AnAircraftPicture.png", 10, 10, Brushes.White, folder: @"c:\pictures");
            ConfigurePictureManagerForFile(@"c:\pictures\AnAircraftPicture.png", 10, 10, "112233", "G-ABCD");

            _Environment.RequestPath = "/Images/Size-Full/File-G-ABCD 112233/Picture.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    AssertImageIsMonochrome(siteImage, _White, 10, 10);
                }
            }
        }

        [TestMethod]
        public void ImageServer_Calls_Aircraft_Manager_Correctly_When_Registration_Is_Missing()
        {
            _Environment.RequestPath = "/Images/Size-Full/File- 112233/Picture.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            _AircraftPictureManager.Verify(m => m.LoadPicture(_AircraftPictureCache.Object, "112233", null), Times.Once());
        }

        [TestMethod]
        public void ImageServer_Calls_Aircraft_Manager_Correctly_When_Icao_Is_Missing()
        {
            _Environment.RequestPath = "/Images/Size-Full/File-G-ABCD /Picture.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            _AircraftPictureManager.Verify(m => m.LoadPicture(_AircraftPictureCache.Object, null, "G-ABCD"), Times.Once());
        }

        [TestMethod]
        public void ImageServer_Copes_If_Aircraft_Picture_Does_Not_Exist()
        {
            _Environment.RequestPath = "/Images/Size-Full/File-G-ABCD 112233/Picture.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            Assert.IsTrue(_Pipeline.NextMiddlewareCalled);
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "PictureResize$")]
        public void ImageServer_Renders_Pictures_At_Correct_Size()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            var originalWidth = worksheet.Int("OriginalWidth");
            var originalHeight = worksheet.Int("OriginalHeight");
            var newWidth = worksheet.Int("NewWidth");
            var newHeight = worksheet.Int("NewHeight");
            var size = worksheet.String("Size");

            CreateMonochromeImage("ImageRenderSize.png", originalWidth, originalHeight, Brushes.Red);
            ConfigurePictureManagerForFile(@"c:\web\ImageRenderSize.png", originalWidth, originalHeight, "ICAO", "REG");

            _Environment.RequestPath = $"/Images/Size-{size}/File-REG ICAO/Picture.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    AssertImageIsMonochrome(siteImage, _Red, newWidth, newHeight);
                }
            }
        }

        [TestMethod]
        public void ImageServer_Can_Render_Aircraft_Full_Sized_Picture()
        {
            _ProgramConfiguration.BaseStationSettings.PicturesFolder = @"c:\pictures";
            AddFileSystemImageFile(TestImages.Picture_700x400_png_Bitmap, "Picture-700x400.png", ImageFormat.Png, path: @"c:\pictures");
            ConfigurePictureManagerForFile(@"c:\pictures\Picture-700x400.png", 700, 400, icao24: "Picture-700x400", registration: null);

            _Environment.RequestPath = "/Images/Size-Full/File-Picture-700x400/Picture.png";
            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    AssertImagesAreIdentical(TestImages.Picture_700x400_png_Bitmap, siteImage);
                }
            }
        }

        [TestMethod]
        public void ImageServer_Ignores_Requests_For_Pictures_From_Internet_When_Prohibited()
        {
            _ProgramConfiguration.BaseStationSettings.PicturesFolder = @"c:\web";
            CreateMonochromeImage("Test.png", 10, 10, Brushes.Blue);
            ConfigurePictureManagerForFile(@"c:\web\Test.png", 10, 10, "ICAO", "REG");

            _ProgramConfiguration.InternetClientSettings.CanShowPictures = false;

            foreach(var size in new string[] { "DETAIL", "FULL", "LIST", "IPADDETAIL", "IPHONEDETAIL" }) {
                _Environment.Reset();

                _Environment.RequestPath = $"/Images/Size-{size}/File-REG ICAO/Picture.png";
                _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

                Assert.AreEqual(0, _Environment.ResponseBodyBytes.Length);
                Assert.IsTrue(_Pipeline.NextMiddlewareCalled);
            }
        }

        [TestMethod]
        public void ImageServer_Sends_Requests_For_Image_Files_Through_ImageFileManager()
        {
            _Environment.RequestPath = $"/Images/Web/File.bmp";
            _ImageFileManager.Setup(r => r.LoadFromStandardPipeline("/images/File.bmp", true, _Environment.Environment)).Returns(TestImages.DLH_bmp_IImage.Clone());

            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            _ImageFileManager.Verify(r => r.LoadFromStandardPipeline("/images/File.bmp", true, _Environment.Environment), Times.Once());
        }

        [TestMethod]
        public void ImageServer_Web_Request_SubFolders_Corrected_Extracted()
        {
            _Environment.RequestPath = $"/Images/Web-SubFolder\\ChildFolder/File.bmp";
            _ImageFileManager.Setup(r => r.LoadFromStandardPipeline("/images/SubFolder/ChildFolder/File.bmp", true, _Environment.Environment)).Returns(TestImages.DLH_bmp_IImage.Clone());

            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            _ImageFileManager.Verify(r => r.LoadFromStandardPipeline("/images/SubFolder/ChildFolder/File.bmp", true, _Environment.Environment), Times.Once());
        }

        [TestMethod]
        public void ImageServer_Can_Request_Uncached_Images()
        {
            _Environment.RequestPath = $"/Images/Web/no-cache/File.bmp";
            _ImageFileManager.Setup(r => r.LoadFromStandardPipeline("/images/File.bmp", false, _Environment.Environment)).Returns(TestImages.DLH_bmp_IImage.Clone());

            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            _ImageFileManager.Verify(r => r.LoadFromStandardPipeline("/images/File.bmp", false, _Environment.Environment), Times.Once());
        }

        [TestMethod]
        public void ImageServer_Internet_Cannot_Request_Uncached_Images()
        {
            _Environment.ServerRemoteIpAddress = "1.2.3.4";
            _Environment.RequestPath = $"/Images/Web/no-cache/File.bmp";
            _ImageFileManager.Setup(r => r.LoadFromStandardPipeline("/images/File.bmp", true, _Environment.Environment)).Returns(TestImages.DLH_bmp_IImage.Clone());

            _Pipeline.BuildAndCallMiddleware(_Server.AppFuncBuilder, _Environment.Environment);

            _ImageFileManager.Verify(r => r.LoadFromStandardPipeline("/images/File.bmp", true, _Environment.Environment), Times.Once());
        }
    }
}
