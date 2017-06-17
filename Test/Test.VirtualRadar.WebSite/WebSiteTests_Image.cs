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
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;
using System.Web;

namespace Test.VirtualRadar.WebSite
{
    // This partial class contains all of the specific tests on the images served by the website.

    // The GDI+ stuff can sometimes produce colours that are *almost* there but not quite. We need
    // to allow a small amount of variance between the actual color and the expected colour.
    // Moreover GDI+ always renders fully-transparent pixels as black whereas the source editor for
    // expected image might have used other colours, so if the expected pixel alpha is zero we need
    // to ignore everything else.
    //
    // Bicubic resizing can produce pixels where the colour is pretty much what we're expecting but
    // the alpha channel is very different to our expected result.
    //
    // High quality bicubic resizing, which currently gets used for large aircraft pictures, produces
    // pixels around the joining of two colours that are VERY different to those you would otherwise
    // expect and make it very difficult to test the results against an 'expected' image. Those methods
    // don't have image comparison tests, I couldn't get anything reliable going.

    public partial class WebSiteTests
    {
        /// <summary>
        /// Creates an image file consisting of a single colour in the test deployment folder and returns the
        /// full path to it.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private string CreateMonochromeImage(string fileName, int width, int height, Brush color)
        {
            string result = Path.Combine(TestContext.TestDeploymentDir, fileName);
            if(File.Exists(result)) File.Delete(result);

            using(var image = new Bitmap(width, height, PixelFormat.Format32bppArgb)) {
                using(var graphics = Graphics.FromImage(image)) {
                    graphics.FillRectangle(color, 0, 0, width, height);
                }
                image.Save(result);
            }

            return result;
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

            for(int y = 0;y < expectedImage.Height;++y) {
                for(int x = 0;x < expectedImage.Width;++x) {
                    Color expectedPixel = expectedImage.GetPixel(x, y);
                    Color actualPixel = actualImage.GetPixel(x, y);

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
            bool identical = expectedImage.Height == actualImage.Height && expectedImage.Width == actualImage.Width;
            if(identical) {
                for(int y = 0;identical && y < expectedImage.Height;++y) {
                    for(int x = 0;identical && x < expectedImage.Width;++x) {
                        Color expectedPixel = expectedImage.GetPixel(x, y);
                        Color actualPixel = actualImage.GetPixel(x, y);

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
                double delta = 5.0;
                Assert.AreEqual(expectedPixel.R, actualPixel.R, delta, "x = {0}, y = {1} {2}", x, y, message);
                Assert.AreEqual(expectedPixel.G, actualPixel.G, delta, "x = {0}, y = {1} {2}", x, y, message);
                Assert.AreEqual(expectedPixel.B, actualPixel.B, delta, "x = {0}, y = {1} {2}", x, y, message);
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

            for(int y = 0;y < image.Height;++y) {
                for(int x = 0;x < image.Width;++x) {
                    CompareColours(color, image.GetPixel(x, y), x, y);
                }
            }
        }

        /// <summary>
        /// Loads the expected image from the file specified (which is assumed to be in the test deployment folder) and compares
        /// it against the actual image.
        /// </summary>
        /// <param name="expectedImageFileName"></param>
        /// <param name="actualImage"></param>
        private void AssertImagesAreIdentical(string expectedImageFileName, Bitmap actualImage, string message = "")
        {
            using(var expectedImage = (Bitmap)Bitmap.FromFile(Path.Combine(TestContext.TestDeploymentDir, expectedImageFileName))) {
                AssertImagesAreIdentical(expectedImage, actualImage, message);
            }
        }

        /// <summary>
        /// Loads the expected image from the file specified and checks that it is different to the actual image.
        /// </summary>
        /// <param name="expectedImageFileName"></param>
        /// <param name="actualImage"></param>
        /// <param name="message"></param>
        private void AssertImagesAreNotIdentical(string expectedImageFileName, Bitmap actualImage, string message = "")
        {
            using(var expectedImage = (Bitmap)Bitmap.FromFile(Path.Combine(TestContext.TestDeploymentDir, expectedImageFileName))) {
                AssertImagesAreNotIdentical(expectedImage, actualImage, message);
            }
        }

        private void ConfigurePictureManagerForFile(string imageFullPath, string icao24 = null, string registration = null)
        {
            if(_Image != null) _Image.Dispose();
            _Image = null;

            _Image = Image.FromFile(imageFullPath);

            if(icao24 != null || registration != null) {
                _LoadPictureTestParams = true;
                _LoadPictureExpectedIcao = icao24;
                _LoadPictureExpectedReg = registration;
            }
        }

        private void ConfigurePictureManagerForPathAndFile(string path, string imageFileName, string icao24 = null, string registration = null)
        {
            ConfigurePictureManagerForFile(Path.Combine(path, imageFileName), icao24, registration);
        }

        [TestMethod]
        public void WebSite_Image_Does_Not_Compress_Images()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);
            var pathAndFile = "/Images/Hght-15/CenX-4/Alt-/TestSquare.png";
            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));

            _Response.Verify(r => r.EnableCompression(_Request.Object), Times.Never());
        }

//        [TestMethod]
//        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
//                    "BlankImage$")]
//        public void WebSite_Image_Can_Create_Blank_Images_Dynamically()
//        {
//            ExcelWorksheetData worksheet = new ExcelWorksheetData(TestContext);
//            _WebSite.AttachSiteToServer(_WebServer.Object);
//
//            var args = RequestReceivedEventArgsHelper.Create(_Request, _Response, worksheet.String("PathAndFile"), false);
//            _WebServer.Raise(m => m.RequestReceived += null, args);
//
//            if(worksheet.String("Width") == null) {
//                Assert.AreEqual(false, args.Handled);
//                Assert.AreEqual(0, _OutputStream.Length);
//                Assert.AreEqual(0, _Response.Object.ContentLength);
//            } else {
//                using(var siteImage = (Bitmap)Bitmap.FromStream(_OutputStream)) {
//                    AssertImageIsMonochrome(siteImage, _Transparent, worksheet.Int("Width"), worksheet.Int("Height"));
//                }
//            }
//        }

//        [TestMethod]
//        public void WebSite_Image_Can_Dynamically_Rotate_Images()
//        {
//            foreach(var rotateDegrees in new int[] { 0, 90, 180, 270 }) {
//                TestCleanup();
//                TestInitialise();
//                _WebSite.AttachSiteToServer(_WebServer.Object);
//
//                var pathAndFile = String.Format("/Images/Rotate-{0}/TestSquare.png", rotateDegrees);
//                _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));
//
//                using(var siteImage = (Bitmap)Bitmap.FromStream(_OutputStream)) {
//                    Assert.AreEqual(9, siteImage.Width);
//                    Assert.AreEqual(9, siteImage.Height);
//
//                    // Determine the colours we expect to see at the 12 o'clock, 3 o'clock, 6 o'clock and 9 o'clock positions
//                    Color p12 = _White, p3 = _White, p6 = _White, p9 = _White;
//                    switch(rotateDegrees) {
//                        case 0:     p12 = _Black;  p3 = _Green;   p6 = _Blue;    p9 = _Red;     break;
//                        case 90:    p12 = _Red;    p3 = _Black;   p6 = _Green;   p9 = _Blue;    break;
//                        case 180:   p12 = _Blue;   p3 = _Red;     p6 = _Black;   p9 = _Green;   break;
//                        case 270:   p12 = _Green;  p3 = _Blue;    p6 = _Red;     p9 = _Black;   break;
//                    }
//
//                    Assert.AreEqual(p12, siteImage.GetPixel(4, 1), rotateDegrees.ToString());
//                    Assert.AreEqual(p3,  siteImage.GetPixel(7, 4), rotateDegrees.ToString());
//                    Assert.AreEqual(p6,  siteImage.GetPixel(4, 7), rotateDegrees.ToString());
//                    Assert.AreEqual(p9,  siteImage.GetPixel(1, 4), rotateDegrees.ToString());
//                }
//            }
//        }

//        [TestMethod]
//        public void WebSite_Image_Can_Dynamically_Widen_Images()
//        {
//            _WebSite.AttachSiteToServer(_WebServer.Object);
//            var pathAndFile = "/Images/Wdth-11/TestSquare.png";
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));
//
//            using(var siteImage = (Bitmap)Bitmap.FromStream(_OutputStream)) {
//                Assert.AreEqual(11, siteImage.Width);
//                Assert.AreEqual(9, siteImage.Height);
//
//                foreach(var x in new int[] { 0, 10 }) {
//                    for(var y = 0;y < 9;++y) {
//                        Assert.AreEqual(_Transparent, siteImage.GetPixel(x, y), "x = {0}, y = {1}", x, y);
//                    }
//                }
//
//                Assert.AreEqual(_White, siteImage.GetPixel(1, 0));
//                Assert.AreEqual(_Black, siteImage.GetPixel(4, 0));
//                Assert.AreEqual(_Red, siteImage.GetPixel(1, 3));
//                Assert.AreEqual(_Green, siteImage.GetPixel(9, 3));
//                Assert.AreEqual(_Blue, siteImage.GetPixel(4, 8));
//            }
//        }

//        [TestMethod]
//        public void WebSite_Image_Can_Dynamically_Change_Height_Of_Images()
//        {
//            _WebSite.AttachSiteToServer(_WebServer.Object);
//            var pathAndFile = "/Images/Hght-11/TestSquare.png";
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));
//
//            using(var siteImage = (Bitmap)Bitmap.FromStream(_OutputStream)) {
//                Assert.AreEqual(9, siteImage.Width);
//                Assert.AreEqual(11, siteImage.Height);
//
//                for(var x = 0;x < 9;++x) {
//                    foreach(var y in new int[] { 0, 10 }) {
//                        Assert.AreEqual(_Transparent, siteImage.GetPixel(x, y), "x = {0}, y = {1}", x, y);
//                    }
//
//                    Assert.AreEqual(_White, siteImage.GetPixel(0, 1));
//                    Assert.AreEqual(_Black, siteImage.GetPixel(3, 1));
//                    Assert.AreEqual(_Red, siteImage.GetPixel(0, 4));
//                    Assert.AreEqual(_Green, siteImage.GetPixel(8, 4));
//                    Assert.AreEqual(_Blue, siteImage.GetPixel(3, 9));
//                }
//            }
//        }

//        [TestMethod]
//        public void WebSite_Image_Can_Dynamically_Add_Altitude_Stalk()
//        {
//            _WebSite.AttachSiteToServer(_WebServer.Object);
//            var pathAndFile = "/Images/Hght-15/CenX-4/Alt-/TestSquare.png";
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));
//
//            using(var siteImage = (Bitmap)Bitmap.FromStream(_OutputStream)) {
//                AssertImagesAreIdentical("AltitudeImageTest-01.png", siteImage);
//            }
//        }

//        [TestMethod]
//        public void WebSite_Image_Can_Dynamically_Add_Text()
//        {
//            _Configuration.BaseStationSettings.PicturesFolder = TestContext.TestDeploymentDir;
//            _WebSite.AttachSiteToServer(_WebServer.Object);
//
//            // This is tricky to test as we can't really reliably compare images that have had text drawn on them. I think the images will be
//            // slightly different between systems. Instead we just check that something changes about the image.
//            for(var lineNumber = 0;lineNumber < 12;++lineNumber) {
//                ConfigurePictureManagerForPathAndFile(TestContext.TestDeploymentDir, "TestSquare.png");
//                var expectDifference = lineNumber >= 1 && lineNumber <= 9;
//                string message = lineNumber.ToString();
//
//                CreateMonochromeImage("TextImageTest.png", 50, 50, Brushes.Black);
//                _OutputStream.SetLength(0);
//
//                var pathAndFile = String.Format("/Images/PL{0}-!/File-TestSquare/Size-Full/Picture.png", lineNumber);
//                _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));
//
//                using(var siteImage = (Bitmap)Bitmap.FromStream(_OutputStream)) {
//                    Assert.AreEqual(9, siteImage.Width, message);
//                    Assert.AreEqual(9, siteImage.Height, message);
//
//                    if(expectDifference) AssertImagesAreNotIdentical("TestSquare.png", siteImage, message);
//                    else                 AssertImagesAreIdentical("TestSquare.png", siteImage, message);
//                }
//            }
//        }

//        [TestMethod]
//        public void WebSite_Image_Will_Not_Dynamically_Add_Text_If_Configuration_Prohibits_It()
//        {
//            _WebSite.AttachSiteToServer(_WebServer.Object);
//
//            // Get a blank image to start with
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, "/Images/Hght-200/Wdth-60/Airplane.png", false));
//            byte[] blankImage = _OutputStream.ToArray();
//
//            // Ask for the same image from the Internet but with a line of text 
//            _OutputStream.SetLength(0);
//            _Configuration.InternetClientSettings.CanShowPinText = true;
//            _ConfigurationStorage.Raise(c => c.ConfigurationChanged += null, EventArgs.Empty);
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, "/Images/PL1-X/Hght-200/Wdth-60/Airplane.png", true));
//            byte[] internetWithText = _OutputStream.ToArray();
//            Assert.IsFalse(blankImage.SequenceEqual(internetWithText));
//
//            // Ask for same image with text from the Internet when the configuration prohibits it
//            _OutputStream.SetLength(0);
//            _Configuration.InternetClientSettings.CanShowPinText = false;
//            _ConfigurationStorage.Raise(c => c.ConfigurationChanged += null, EventArgs.Empty);
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, "/Images/PL1-X/Hght-200/Wdth-60/Airplane.png", true));
//            byte[] internetWithoutText = _OutputStream.ToArray();
//            Assert.IsTrue(blankImage.SequenceEqual(internetWithoutText));
//
//            // Ask for the same image from the LAN but with a line of text 
//            _OutputStream.SetLength(0);
//            _Configuration.InternetClientSettings.CanShowPinText = true;
//            _ConfigurationStorage.Raise(c => c.ConfigurationChanged += null, EventArgs.Empty);
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, "/Images/PL1-X/Hght-200/Wdth-60/Airplane.png", false));
//            byte[] lanWithText = _OutputStream.ToArray();
//            Assert.IsFalse(blankImage.SequenceEqual(lanWithText));
//
//            // Ask for same image with text from the LAN when the configuration prohibits it
//            _OutputStream.SetLength(0);
//            _Configuration.InternetClientSettings.CanShowPinText = false;
//            _ConfigurationStorage.Raise(c => c.ConfigurationChanged += null, EventArgs.Empty);
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, "/Images/PL1-X/Hght-200/Wdth-60/Airplane.png", false));
//            byte[] lanWithoutText = _OutputStream.ToArray();
//            Assert.IsFalse(blankImage.SequenceEqual(lanWithoutText));
//        }

//        [TestMethod]
//        public void WebSite_Image_Can_Serve_Operator_Logos()    { Do_Image_Can_Serve_Logo(true); }

//        [TestMethod]
//        public void WebSite_Image_Can_Serve_Silhouettes()       { Do_Image_Can_Serve_Logo(false); }

//        private void Do_Image_Can_Serve_Logo(bool isOpFlag)
//        {
//            if(isOpFlag) _Configuration.BaseStationSettings.OperatorFlagsFolder = TestContext.TestDeploymentDir;
//            else         _Configuration.BaseStationSettings.SilhouettesFolder = TestContext.TestDeploymentDir;
//            _WebSite.AttachSiteToServer(_WebServer.Object);
//
//            var pathAndFile = String.Format("/Images/File-DLH/{0}.png", isOpFlag ? "OpFlag" : "Type");
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));
//
//            using(var siteImage = (Bitmap)Bitmap.FromStream(_OutputStream)) {
//                AssertImagesAreIdentical("DLH.bmp", siteImage);
//            }
//        }

//        [TestMethod]
//        public void WebSite_Image_Does_Not_Throw_Exception_When_OperatorFlagCode_Contains_Invalid_Characters()
//        {
//            _Configuration.BaseStationSettings.OperatorFlagsFolder = TestContext.TestDeploymentDir;
//            _WebSite.AttachSiteToServer(_WebServer.Object);
//
//            foreach(var badChar in Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars())) {
//                var fileName = "BA" + badChar;
//                var pathAndFile = String.Format("/Images/File-{0}/OpFlag.png", HttpUtility.UrlEncode(fileName));
//                _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));
//            }
//        }

//        [TestMethod]
//        public void WebSite_Image_Does_Not_Throw_Exception_When_Model_Icao_Contains_Invalid_Characters()
//        {
//            _Configuration.BaseStationSettings.SilhouettesFolder = TestContext.TestDeploymentDir;
//            _WebSite.AttachSiteToServer(_WebServer.Object);
//
//            foreach(var badChar in Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars())) {
//                var fileName = "BA" + badChar;
//                var pathAndFile = String.Format("/Images/File-{0}/Type.png", HttpUtility.UrlEncode(fileName));
//                _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));
//            }
//        }

//        [TestMethod]
//        public void WebSite_Image_Can_Serve_Alternative_Operator_Logos()    { Do_Image_Can_Serve_Alternative_Logo(true); }

//        [TestMethod]
//        public void WebSite_Image_Can_Serve_Alternative_Silhouettes()       { Do_Image_Can_Serve_Alternative_Logo(false); }

//        private void Do_Image_Can_Serve_Alternative_Logo(bool isOpFlag)
//        {
//            if(isOpFlag) _Configuration.BaseStationSettings.OperatorFlagsFolder = TestContext.TestDeploymentDir;
//            else         _Configuration.BaseStationSettings.SilhouettesFolder = TestContext.TestDeploymentDir;
//            _WebSite.AttachSiteToServer(_WebServer.Object);
//
//            var pathAndFile = String.Format("/Images/File-DOESNOTEXIST|DLH/{0}.png", isOpFlag ? "OpFlag" : "Type");
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));
//
//            using(var siteImage = (Bitmap)Bitmap.FromStream(_OutputStream)) {
//                AssertImagesAreIdentical("DLH.bmp", siteImage);
//            }
//        }

//        [TestMethod]
//        public void WebSite_Image_Resizes_Small_Operator_Logos_To_Fit_Standard_Size()   { Do_Image_Resizes_Small_Logos_To_Fit_Standard_Size(true); }

//        [TestMethod]
//        public void WebSite_Image_Resizes_Small_Silhouettes_To_Fit_Standard_Size()      { Do_Image_Resizes_Small_Logos_To_Fit_Standard_Size(false); }

//        private void Do_Image_Resizes_Small_Logos_To_Fit_Standard_Size(bool isOpFlag)
//        {
//            if(isOpFlag) _Configuration.BaseStationSettings.OperatorFlagsFolder = TestContext.TestDeploymentDir;
//            else         _Configuration.BaseStationSettings.SilhouettesFolder = TestContext.TestDeploymentDir;
//            _WebSite.AttachSiteToServer(_WebServer.Object);
//
//            var pathAndFile = String.Format("/Images/File-TestSquare/{0}.png", isOpFlag ? "OpFlag" : "Type");
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));
//
//            using(var siteImage = (Bitmap)Bitmap.FromStream(_OutputStream)) {
//                Assert.AreEqual(85, siteImage.Width);
//                Assert.AreEqual(20, siteImage.Height);
//
//                // Should have placed the small image (TestSquare is 9x9) in the centre as per rules for WDTH and HGHT
//                Assert.AreEqual(_Transparent, siteImage.GetPixel(0, 0));
//                Assert.AreEqual(_White, siteImage.GetPixel(39, 7));
//                Assert.AreEqual(_Black, siteImage.GetPixel(42, 7));
//            }
//        }

//        [TestMethod]
//        public void WebSite_Image_Resizes_Large_Operator_Logos_To_Fit_Standard_Size()   { Do_Image_Resizes_Large_Logos_To_Fit_Standard_Size(true); }

//        [TestMethod]
//        public void WebSite_Image_Resizes_Large_Silhouettes_To_Fit_Standard_Size()      { Do_Image_Resizes_Large_Logos_To_Fit_Standard_Size(false); }

//        private void Do_Image_Resizes_Large_Logos_To_Fit_Standard_Size(bool isOpFlag)
//        {
//            if(isOpFlag) _Configuration.BaseStationSettings.OperatorFlagsFolder = TestContext.TestDeploymentDir;
//            else         _Configuration.BaseStationSettings.SilhouettesFolder = TestContext.TestDeploymentDir;
//            _WebSite.AttachSiteToServer(_WebServer.Object);
//
//            var pathAndFile = String.Format("/Images/File-OversizedLogo/{0}.png", isOpFlag ? "OpFlag" : "Type");
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));
//
//            using(var siteImage = (Bitmap)Bitmap.FromStream(_OutputStream)) {
//                Assert.AreEqual(85, siteImage.Width);
//                Assert.AreEqual(20, siteImage.Height);
//
//                // Should have placed the large image in the centre - the image is 87x22 whereas the standard size is 85x20 so by centreing
//                // it we should have cropped a 1 pixel border off the image, which leaves a black pixel in each corner
//                Assert.AreEqual(_Black, siteImage.GetPixel(0, 0));
//                Assert.AreEqual(_White, siteImage.GetPixel(1, 0));
//                Assert.AreEqual(_Black, siteImage.GetPixel(84, 0));
//                Assert.AreEqual(_Black, siteImage.GetPixel(0, 19));
//                Assert.AreEqual(_Black, siteImage.GetPixel(84, 19));
//            }
//        }

//        [TestMethod]
//        public void WebSite_Image_Returns_Blank_Image_If_OperatorFlagsFolder_Not_Configured()   { Do_Image_Returns_Blank_Image_If_Logo_Not_Configured(true); }

//        [TestMethod]
//        public void WebSite_Image_Returns_Blank_Image_If_SilhouettesFolder_Not_Configured()     { Do_Image_Returns_Blank_Image_If_Logo_Not_Configured(false); }

//        private void Do_Image_Returns_Blank_Image_If_Logo_Not_Configured(bool isOpFlag)
//        {
//            if(isOpFlag) _Configuration.BaseStationSettings.OperatorFlagsFolder = null;
//            else         _Configuration.BaseStationSettings.SilhouettesFolder = null;
//            _WebSite.AttachSiteToServer(_WebServer.Object);
//
//            var pathAndFile = String.Format("/Images/File-DLH/{0}.png", isOpFlag ? "OpFlag" : "Type");
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));
//
//            using(var siteImage = (Bitmap)Bitmap.FromStream(_OutputStream)) {
//                Assert.AreEqual(85, siteImage.Width);
//                Assert.AreEqual(20, siteImage.Height);
//                AssertImageIsMonochrome(siteImage, _Transparent);
//            }
//        }

//        [TestMethod]
//        public void WebSite_Image_Returns_Blank_Image_If_Operator_Flag_File_Not_Found() { Do_Image_Returns_Blank_Image_If_Logo_File_Not_Found(true); }

//        [TestMethod]
//        public void WebSite_Image_Returns_Blank_Image_If_Silhouette_File_Not_Found()    { Do_Image_Returns_Blank_Image_If_Logo_File_Not_Found(false); }

//        private void Do_Image_Returns_Blank_Image_If_Logo_File_Not_Found(bool isOpFlag)
//        {
//            if(isOpFlag) _Configuration.BaseStationSettings.OperatorFlagsFolder = TestContext.TestDeploymentDir;
//            else         _Configuration.BaseStationSettings.SilhouettesFolder = TestContext.TestDeploymentDir;
//            _WebSite.AttachSiteToServer(_WebServer.Object);
//
//            var pathAndFile = String.Format("/Images/File-DoesNotExist/{0}.png", isOpFlag ? "OpFlag" : "Type");
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));
//
//            using(var siteImage = (Bitmap)Bitmap.FromStream(_OutputStream)) {
//                Assert.AreEqual(85, siteImage.Width);
//                Assert.AreEqual(20, siteImage.Height);
//                AssertImageIsMonochrome(siteImage, _Transparent);
//            }
//        }

//        [TestMethod]
//        public void WebSite_Image_Render_Operator_Flag_Picks_Up_Changes_In_Configuration_Folder()   { Do_Image_Render_Logo_Picks_Up_Changes_In_Configuration_Folder(true); }

//        [TestMethod]
//        public void WebSite_Image_Render_Silhouette_Picks_Up_Changes_In_Configuration_Folder()      { Do_Image_Render_Logo_Picks_Up_Changes_In_Configuration_Folder(false); }

//        private void Do_Image_Render_Logo_Picks_Up_Changes_In_Configuration_Folder(bool isOpFlag)
//        {
//            if(isOpFlag) _Configuration.BaseStationSettings.OperatorFlagsFolder = "c:\\Whatever, this don't exist, whatever";
//            else         _Configuration.BaseStationSettings.SilhouettesFolder = "c:\\Whatever, this don't exist, whatever";
//            _WebSite.AttachSiteToServer(_WebServer.Object);
//
//            var pathAndFile = String.Format("/Images/File-DLH/{0}.png", isOpFlag ? "OpFlag" : "Type");
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));
//
//            using(var siteImage = (Bitmap)Bitmap.FromStream(_OutputStream)) {
//                AssertImageIsMonochrome(siteImage, _Transparent);
//            }
//
//            _OutputStream.SetLength(0);
//
//            if(isOpFlag) _Configuration.BaseStationSettings.OperatorFlagsFolder = TestContext.TestDeploymentDir;
//            else         _Configuration.BaseStationSettings.SilhouettesFolder = TestContext.TestDeploymentDir;
//            _ConfigurationStorage.Raise(m => m.ConfigurationChanged += null, EventArgs.Empty);
//
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));
//            using(var siteImage = (Bitmap)Bitmap.FromStream(_OutputStream)) {
//                AssertImagesAreIdentical("DLH.bmp", siteImage);
//            }
//        }

//        [TestMethod]
//        public void WebSite_Image_Returns_Blank_Image_If_Attempt_Made_To_Move_Out_Of_OperatorFlagsFolder()  { Do_Image_Returns_Blank_Image_If_Attempt_Made_To_Move_Out_Of_Logo_Folder(true); }

//        [TestMethod]
//        public void WebSite_Image_Returns_Blank_Image_If_Attempt_Made_To_Move_Out_Of_SilhouettesFolder()    { Do_Image_Returns_Blank_Image_If_Attempt_Made_To_Move_Out_Of_Logo_Folder(false); }

//        private void Do_Image_Returns_Blank_Image_If_Attempt_Made_To_Move_Out_Of_Logo_Folder(bool isOpFlag)
//        {
//            if(isOpFlag) _Configuration.BaseStationSettings.OperatorFlagsFolder = Path.Combine(TestContext.TestDeploymentDir, "SubFolder");
//            else         _Configuration.BaseStationSettings.SilhouettesFolder = Path.Combine(TestContext.TestDeploymentDir, "SubFolder");
//            _WebSite.AttachSiteToServer(_WebServer.Object);
//
//            var pathAndFile = String.Format("/Images/File-VIR/{0}.png", isOpFlag ? "OpFlag" : "Type");
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));
//            using(var siteImage = (Bitmap)Bitmap.FromStream(_OutputStream)) {
//                AssertImagesAreIdentical("SubFolder\\Vir.bmp", siteImage);
//            }
//
//            _OutputStream.SetLength(0);
//            pathAndFile = String.Format("/Images/File-..\\DLH/{0}.png", isOpFlag ? "OpFlag" : "Type");
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));
//            using(var siteImage = (Bitmap)Bitmap.FromStream(_OutputStream)) {
//                Assert.AreNotEqual(0, siteImage.Width);
//                AssertImageIsMonochrome(siteImage, _Transparent);
//            }
//
//            _OutputStream.SetLength(0);
//            pathAndFile = String.Format("/Images/File-..%5CDLH/{0}.png", isOpFlag ? "OpFlag" : "Type");
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));
//            using(var siteImage = (Bitmap)Bitmap.FromStream(_OutputStream)) {
//                Assert.AreNotEqual(0, siteImage.Width);
//                AssertImageIsMonochrome(siteImage, _Transparent);
//            }
//
//            if(isOpFlag) _Configuration.BaseStationSettings.OperatorFlagsFolder = TestContext.TestDeploymentDir;
//            else         _Configuration.BaseStationSettings.SilhouettesFolder = TestContext.TestDeploymentDir;
//            _ConfigurationStorage.Raise(m => m.ConfigurationChanged += null, EventArgs.Empty);
//
//            _OutputStream.SetLength(0);
//            pathAndFile = String.Format("/Images/File-SubFolder\\VIR/{0}.png", isOpFlag ? "OpFlag" : "Type");
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));
//            using(var siteImage = (Bitmap)Bitmap.FromStream(_OutputStream)) {
//                Assert.AreNotEqual(0, siteImage.Width);
//                AssertImageIsMonochrome(siteImage, _Transparent);
//            }
//
//            _OutputStream.SetLength(0);
//            pathAndFile = String.Format("/Images/File-SubFolder%5CVIR/{0}.png", isOpFlag ? "OpFlag" : "Type");
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));
//            using(var siteImage = (Bitmap)Bitmap.FromStream(_OutputStream)) {
//                Assert.AreNotEqual(0, siteImage.Width);
//                AssertImageIsMonochrome(siteImage, _Transparent);
//            }
//        }

//        [TestMethod]
//        public void WebSite_Image_Can_Return_IPhone_Splash_Screen()
//        {
//            // The content of this is built dynamically and while we could compare checksums I suspect that over different machines you'd get
//            // slightly different results. So this test just checks that if you ask for a splash screen then you get something back that's the
//            // right size and has what looks to be the correct colour background.
//            _Configuration.BaseStationSettings.PicturesFolder = TestContext.TestDeploymentDir;
//            _WebSite.AttachSiteToServer(_WebServer.Object);
//
//            string pathAndFile = "/Images/IPhoneSplash.png";
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));
//
//            using(var siteImage = (Bitmap)Bitmap.FromStream(_OutputStream)) {
//                Assert.AreEqual(320, siteImage.Width);
//                Assert.AreEqual(460, siteImage.Height);
//                Assert.AreEqual(_Black, siteImage.GetPixel(10, 10));
//            }
//        }

//        [TestMethod]
//        public void WebSite_Image_Can_Return_IPad_Splash_Screen_Via_UserAgent_String()
//        {
//            // See notes on iPhone version
//
//            _Configuration.BaseStationSettings.PicturesFolder = TestContext.TestDeploymentDir;
//            _WebSite.AttachSiteToServer(_WebServer.Object);
//
//            string pathAndFile = "/Images/IPhoneSplash.png";
//            var args = RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false);
//            RequestReceivedEventArgsHelper.SetIPadUserAgent(_Request);
//            _WebServer.Raise(m => m.RequestReceived += null, args);
//
//            using(var siteImage = (Bitmap)Bitmap.FromStream(_OutputStream)) {
//                Assert.AreEqual(768, siteImage.Width);
//                Assert.AreEqual(1004, siteImage.Height);
//                Assert.AreEqual(_Black, siteImage.GetPixel(10, 10));
//            }
//        }

//        [TestMethod]
//        public void WebSite_Image_Can_Return_IPad_Splash_Screen_Via_Explicit_Instruction()
//        {
//            // See notes on iPhone version
//
//            _Configuration.BaseStationSettings.PicturesFolder = TestContext.TestDeploymentDir;
//            _WebSite.AttachSiteToServer(_WebServer.Object);
//
//            string pathAndFile = "/Images/File-IPad/IPhoneSplash.png";
//            var args = RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false);
//            _WebServer.Raise(m => m.RequestReceived += null, args);
//
//            using(var siteImage = (Bitmap)Bitmap.FromStream(_OutputStream)) {
//                Assert.AreEqual(768, siteImage.Width);
//                Assert.AreEqual(1004, siteImage.Height);
//                Assert.AreEqual(_Black, siteImage.GetPixel(10, 10));
//            }
//        }

//        [TestMethod]
//        public void WebSite_Image_Can_Display_Aircraft_Picture_Correctly()
//        {
//            _WebSite.AttachSiteToServer(_WebServer.Object);
//
//            CreateMonochromeImage("AnAircraftPicture.png", 10, 10, Brushes.White);
//            ConfigurePictureManagerForPathAndFile(TestContext.TestDeploymentDir, "AnAircraftPicture.png", "112233", "G-ABCD");
//
//            string pathAndFile = "/Images/Size-Full/File-G-ABCD 112233/Picture.png";
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));
//
//            using(var siteImage = (Bitmap)Bitmap.FromStream(_OutputStream)) {
//                AssertImageIsMonochrome(siteImage, _White, 10, 10);
//            }
//        }

//        [TestMethod]
//        public void WebSite_Image_Calls_Aircraft_Manager_Correctly_When_Registration_Is_Missing()
//        {
//            _WebSite.AttachSiteToServer(_WebServer.Object);
//
//            string pathAndFile = "/Images/Size-Full/File- 112233/Picture.png";
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));
//
//            _AircraftPictureManager.Verify(m => m.LoadPicture(_DirectoryCache.Object, "112233", null), Times.Once());
//        }

//        [TestMethod]
//        public void WebSite_Image_Calls_Aircraft_Manager_Correctly_When_Icao_Is_Missing()
//        {
//            _WebSite.AttachSiteToServer(_WebServer.Object);
//
//            string pathAndFile = "/Images/Size-Full/File-G-ABCD /Picture.png";
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));
//
//            _AircraftPictureManager.Verify(m => m.LoadPicture(_DirectoryCache.Object, null, "G-ABCD"), Times.Once());
//        }

//        [TestMethod]
//        public void WebSite_Image_Copes_If_Aircraft_Picture_Does_Not_Exist()
//        {
//            _Configuration.BaseStationSettings.PicturesFolder = TestContext.TestDeploymentDir;
//            _WebSite.AttachSiteToServer(_WebServer.Object);
//
//            _AircraftPictureManager.Setup(p => p.LoadPicture(_DirectoryCache.Object, It.IsAny<string>(), It.IsAny<string>())).Returns((Image)null);
//
//            string pathAndFile = "/Images/Size-Full/File-G-ABCD 112233/Picture.png";
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));
//
//            Assert.AreEqual((HttpStatusCode)0, _Response.Object.StatusCode);
//            Assert.AreEqual(0, _Response.Object.ContentLength);
//            Assert.AreEqual(0, _Response.Object.OutputStream.Length);
//        }

//        [TestMethod]
//        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
//                    "PictureResize$")]
//        public void WebSite_Image_Renders_Pictures_At_Correct_Size()
//        {
//            var worksheet = new ExcelWorksheetData(TestContext);
//
//            CreateMonochromeImage("ImageRenderSize.png", worksheet.Int("OriginalWidth"), worksheet.Int("OriginalHeight"), Brushes.Red);
//            ConfigurePictureManagerForPathAndFile(TestContext.TestDeploymentDir, "ImageRenderSize.png");
//
//            _Configuration.BaseStationSettings.PicturesFolder = TestContext.TestDeploymentDir;
//            _WebSite.AttachSiteToServer(_WebServer.Object);
//
//            string pathAndFile = String.Format("/Images/Size-{0}/File-ImageRenderSize/Picture.png", worksheet.String("Size"));
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));
//
//            using(var siteImage = (Bitmap)Bitmap.FromStream(_OutputStream)) {
//                AssertImageIsMonochrome(siteImage, _Red, worksheet.Int("NewWidth"), worksheet.Int("NewHeight"));
//            }
//        }

//        [TestMethod]
//        public void WebSite_Image_Can_Render_Aircraft_Full_Sized_Picture()
//        {
//            _Configuration.BaseStationSettings.PicturesFolder = TestContext.TestDeploymentDir;
//            ConfigurePictureManagerForPathAndFile(TestContext.TestDeploymentDir, "Picture-700x400.png");
//
//            _WebSite.AttachSiteToServer(_WebServer.Object);
//
//            string pathAndFile = "/Images/Size-Full/File-Picture-700x400/Picture.png";
//            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false));
//
//            using(var siteImage = (Bitmap)Bitmap.FromStream(_OutputStream)) {
//                AssertImagesAreIdentical("Picture-700x400.png", siteImage);
//            }
//        }

//        [TestMethod]
//        public void WebSite_Image_Ignores_Requests_For_Pictures_From_Internet_When_Prohibited()
//        {
//            _Configuration.BaseStationSettings.PicturesFolder = TestContext.TestDeploymentDir;
//            ConfigurePictureManagerForPathAndFile(TestContext.TestDeploymentDir, "TestSquare.png");
//
//            _WebSite.AttachSiteToServer(_WebServer.Object);
//
//            foreach(var size in new string[] { "DETAIL", "FULL", "LIST", "IPADDETAIL", "IPHONEDETAIL" }) {
//                string pathAndFile = String.Format("/Images/Size-{0}/File-TestSquare/Picture.png", size);
//
//                _Configuration.InternetClientSettings.CanShowPictures = false;
//
//                _OutputStream.SetLength(0);
//                _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, true));
//
//                Assert.AreEqual((HttpStatusCode)0, _Response.Object.StatusCode, size);   // WebServer should eventually send 403 but we're not responsible for doing that
//                Assert.AreEqual(0, _Response.Object.ContentLength, size);
//                Assert.AreEqual(0, _OutputStream.Length, size);
//            }
//        }

        [TestMethod]
        public void WebSite_Image_Picks_Up_Configuration_Changes_For_Internet_Viewing_Of_Pictures()
        {
            _Configuration.BaseStationSettings.PicturesFolder = TestContext.TestDeploymentDir;
            ConfigurePictureManagerForPathAndFile(TestContext.TestDeploymentDir, "TestSquare.png");

            _WebSite.AttachSiteToServer(_WebServer.Object);

            foreach(var size in new string[] { "DETAIL", "FULL", "LIST", "IPADDETAIL", "IPHONEDETAIL" }) {
                ConfigurePictureManagerForPathAndFile(TestContext.TestDeploymentDir, "TestSquare.png");
                string pathAndFile = String.Format("/Images/Size-{0}/File-TestSquare/Picture.png", size);

                _OutputStream.SetLength(0);
                _Response = new Mock<IResponse>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
                _Response.Setup(m => m.OutputStream).Returns(_OutputStream);

                _Configuration.InternetClientSettings.CanShowPictures = true;
                _ConfigurationStorage.Raise(m => m.ConfigurationChanged += null, EventArgs.Empty);

                _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, true));

                Assert.AreEqual(HttpStatusCode.OK, _Response.Object.StatusCode, size);
                using(var siteImage = (Bitmap)Bitmap.FromStream(_OutputStream)) {
                    Assert.AreNotEqual(0, siteImage.Width, size);
                }

                _Configuration.InternetClientSettings.CanShowPictures = false;
                _ConfigurationStorage.Raise(m => m.ConfigurationChanged += null, EventArgs.Empty);

                _OutputStream.SetLength(0);
                _Response = new Mock<IResponse>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
                _Response.Setup(m => m.OutputStream).Returns(_OutputStream);

                _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, true));

                Assert.AreEqual((HttpStatusCode)0, _Response.Object.StatusCode, size);   // WebServer should eventually send 403 but we're not responsible for doing that
                Assert.AreEqual(0, _Response.Object.ContentLength, size);
                Assert.AreEqual(0, _OutputStream.Length, size);
            }
        }
    }
}
