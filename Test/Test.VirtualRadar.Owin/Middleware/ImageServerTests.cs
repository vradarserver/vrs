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
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Resources;

namespace Test.VirtualRadar.Owin.Middleware
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
        private global::VirtualRadar.Interface.Settings.Configuration _ProgramConfiguration;
        private Mock<ISharedConfiguration> _SharedConfiguration;
        private Mock<IImageFileManager> _ImageFileManager;

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
            Factory.Singleton.RegisterInstance<IFileSystemProvider>(_FileSystem);

            _FileSystemServerConfiguration = TestUtilities.CreateMockSingleton<IFileSystemServerConfiguration>();
            _FileSystemServerConfiguration.Setup(r => r.GetSiteRootFolders()).Returns(() => new List<string>() {
                @"c:\web\",
            });
            _FileSystemServerConfiguration.Setup(r => r.IsFileUnmodified(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>())).Returns(true);

            _ImageFileManager = TestUtilities.CreateMockImplementation<IImageFileManager>();
            _ServerConfiguration = TestUtilities.CreateMockSingleton<IImageServerConfiguration>();
            _ServerConfiguration.SetupGet(r => r.ImageFileManager).Returns(_ImageFileManager.Object);

            _Server = Factory.Singleton.Resolve<IImageServer>();

            _Environment = new MockOwinEnvironment();
            _Environment.Request.RemoteIpAddress = "127.0.0.1";
            _Pipeline = new MockOwinPipeline();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
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
            else                        Assert.IsTrue(actualPixel.A > 10, "x = {0}, y = {1} {2}", x, y, message);

            if(expectedPixel.A > 0) {
                var delta = 5.0;
                Assert.AreEqual(expectedPixel.R, actualPixel.R, delta, "x = {0}, y = {1} {2}", x, y, message);
                Assert.AreEqual(expectedPixel.G, actualPixel.G, delta, "x = {0}, y = {1} {2}", x, y, message);
                Assert.AreEqual(expectedPixel.B, actualPixel.B, delta, "x = {0}, y = {1} {2}", x, y, message);
            }
        }

        private Mock<IWebSiteGraphics> ReplaceWebSiteGraphics()
        {
            var result = TestUtilities.CreateMockSingleton<IWebSiteGraphics>();
            result.Setup(r => r.UseImage(It.IsAny<Image>(), It.IsAny<Image>()))
                           .Returns((Image tempImage, Image newImage) => {
                                return newImage;
                           });

            return result;
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "BlankImage$")]
        public void ImageServer_Can_Create_Blank_Images_Dynamically()
        {
            ExcelWorksheetData worksheet = new ExcelWorksheetData(TestContext);

            _Environment.RequestPath = worksheet.String("PathAndFile");
            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

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
        public void ImageServer_Can_Dynamically_Rotate_Images()
        {
            foreach(var rotateDegrees in new int[] { 0, 90, 180, 270 }) {
                TestCleanup();
                TestInitialise();

                _Environment.RequestPath = $"/Images/Rotate-{rotateDegrees}/TestSquare.png";
                _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

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
            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

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
            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

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
        public void ImageServer_Can_Dynamically_Add_Altitude_Stalk()
        {
            _Environment.RequestPath = "/Images/Hght-15/CenX-4/Alt-/TestSquare.png";
            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    AssertImagesAreIdentical(TestImages.AltitudeImageTest_01_png, siteImage);
                }
            }
        }

        [TestMethod]
        public void ImageServer_Can_Dynamically_Add_Text()
        {
            var textLines = new List<string>();
            var webSiteGraphics = ReplaceWebSiteGraphics();
            webSiteGraphics.Setup(r => r.AddTextLines(It.IsAny<Image>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>()))
                           .Returns((Image image, IEnumerable<string> lines, bool unused2, bool unused3) => {
                                textLines.AddRange(lines);
                                return (Image)image.Clone();
                           });
            _Server = Factory.Singleton.Resolve<IImageServer>();

            _Environment.RequestPath = "/Images/PL1-Hello/PL2-There/TestSquare.png";
            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            webSiteGraphics.Verify(r => r.AddTextLines(It.IsAny<Image>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
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
            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);
            byte[] imageWithoutText = _Environment.ResponseBodyBytes;

            // Ask for the same image from the Internet but with a line of text 
            _Environment.Reset();
            _ProgramConfiguration.InternetClientSettings.CanShowPinText = true;
            _Environment.RequestPath = "/Images/PL1-X/Hght-200/Wdth-60/Airplane.png";
            _Environment.Request.RemoteIpAddress = "1.2.3.4";
            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);
            byte[] internetWithText = _Environment.ResponseBodyBytes;
            Assert.IsFalse(imageWithoutText.SequenceEqual(internetWithText));

            // Ask for same image with text from the Internet when the configuration prohibits it
            _Environment.Reset();
            _ProgramConfiguration.InternetClientSettings.CanShowPinText = false;
            _Environment.RequestPath = "/Images/PL1-X/Hght-200/Wdth-60/Airplane.png";
            _Environment.Request.RemoteIpAddress = "1.2.3.4";
            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);
            byte[] internetWithoutText = _Environment.ResponseBodyBytes;
            Assert.IsTrue(imageWithoutText.SequenceEqual(internetWithoutText));

            // Ask for the same image from the LAN but with a line of text 
            _Environment.Reset();
            _ProgramConfiguration.InternetClientSettings.CanShowPinText = true;
            _Environment.RequestPath = "/Images/PL1-X/Hght-200/Wdth-60/Airplane.png";
            _Environment.Request.RemoteIpAddress = "192.168.2.3";
            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);
            byte[] lanWithText = _Environment.ResponseBodyBytes;
            Assert.IsFalse(imageWithoutText.SequenceEqual(lanWithText));

            // Ask for same image with text from the LAN when the configuration prohibits it
            _Environment.Reset();
            _ProgramConfiguration.InternetClientSettings.CanShowPinText = false;
            _Environment.RequestPath = "/Images/PL1-X/Hght-200/Wdth-60/Airplane.png";
            _Environment.Request.RemoteIpAddress = "192.168.2.3";
            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);
            byte[] lanWithoutText = _Environment.ResponseBodyBytes;
            Assert.IsFalse(imageWithoutText.SequenceEqual(lanWithoutText));
        }

        [TestMethod]
        public void ImageServer_Can_Serve_Operator_Logos()
        {
            _ServerConfiguration.SetupGet(r => r.OperatorFolder).Returns(@"c:\flags");
            AddFileSystemImageFile(TestImages.DLH_bmp, "DLH.bmp", ImageFormat.Bmp, @"c:\flags");
            _ImageFileManager.Setup(r => r.LoadFromFile(@"c:\flags\DLH.bmp")).Returns(TestImages.DLH_bmp);

            _Environment.RequestPath = "/Images/File-DLH/OpFlag.png";
            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    AssertImagesAreIdentical(TestImages.DLH_bmp, siteImage);
                }
            }
        }

        [TestMethod]
        public void ImageServer_Can_Serve_Silhouettes()
        {
            _ServerConfiguration.SetupGet(r => r.SilhouettesFolder).Returns(@"c:\types");
            AddFileSystemImageFile(TestImages.DLH_bmp, "DLH.bmp", ImageFormat.Bmp, @"c:\types");
            _ImageFileManager.Setup(r => r.LoadFromFile(@"c:\types\DLH.bmp")).Returns(TestImages.DLH_bmp);

            _Environment.RequestPath = "/Images/File-DLH/Type.png";
            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            using(var stream = new MemoryStream(_Environment.ResponseBodyBytes)) {
                using(var siteImage = (Bitmap)Bitmap.FromStream(stream)) {
                    AssertImagesAreIdentical(TestImages.DLH_bmp, siteImage);
                }
            }
        }
    }
}
