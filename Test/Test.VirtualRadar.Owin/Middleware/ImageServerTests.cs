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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface.Owin;

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
            _ServerConfiguration = TestUtilities.CreateMockSingleton<IImageServerConfiguration>();

            _Server = Factory.Singleton.Resolve<IImageServer>();

            _Environment = new MockOwinEnvironment();
            _Pipeline = new MockOwinPipeline();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
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
    }
}
