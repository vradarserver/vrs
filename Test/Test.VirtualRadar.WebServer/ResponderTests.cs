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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface.WebServer;
using Moq;
using System.IO;
using Test.Framework;
using System.Reflection;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using InterfaceFactory;

namespace Test.VirtualRadar.WebServer
{
    [TestClass]
    public class ResponderTests
    {
        #region Private class - JsonExample
        [DataContract]
        class JsonExample
        {
            [DataMember]
            public int IntegerField { get; set; }

            [DataMember]
            public DateTime DateField { get; set; }
        }
        #endregion

        #region TestInitialise, TestCleanup, Fields etc.
        public TestContext TestContext { get; set; }

        private IResponder _Responder;

        private Mock<IRequest> _Request;
        private Mock<IResponse> _Response;
        private MemoryStream _OutputStream;
        private Bitmap _Image;

        [TestInitialize]
        public void TestInitialise()
        {
            _Responder = Factory.Singleton.Resolve<IResponder>();

            _Request = TestUtilities.CreateMockInstance<IRequest>();
            _Response = TestUtilities.CreateMockInstance<IResponse>();
            _OutputStream = new MemoryStream();
            _Response.Setup(m => m.OutputStream).Returns(_OutputStream);

            _Image = new Bitmap(10, 10);
            using(var graphics = Graphics.FromImage(_Image)) {
                graphics.FillRectangle(Brushes.White, 0, 0, _Image.Width, _Image.Height);
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if(_OutputStream != null) _OutputStream.Dispose();
            _OutputStream = null;

            if(_Image != null) _Image.Dispose();
            _Image = null;
        }
        #endregion

        #region Forbidden
        [TestMethod]
        public void Responder_Forbidden_Sets_Correct_Response()
        {
            _Responder.Forbidden(_Response.Object);

            Assert.AreEqual(HttpStatusCode.Forbidden, _Response.Object.StatusCode);
        }
        #endregion

        #region SendText
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Responder_SendText_Throws_If_Passed_Null_Request()
        {
            _Responder.SendText(null, _Response.Object, "Hello", Encoding.UTF8, MimeType.Css);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Responder_SendText_Throws_If_Passed_Null_Response()
        {
            _Responder.SendText(_Request.Object, null, "Hello", Encoding.UTF8, MimeType.Css);
        }

        [TestMethod]
        [DataSource("Data Source='WebServerTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "SendText$")]
        public void Responder_SendText_Fills_Response_Correctly()
        {
            ExcelWorksheetData worksheet = new ExcelWorksheetData(TestContext);

            string encodingName = worksheet.String("Encoding");
            var encoding = encodingName == null ? (Encoding)null : (Encoding)typeof(Encoding).GetProperty(encodingName, BindingFlags.Static | BindingFlags.Public).GetValue(null, null);

            _Responder.SendText(_Request.Object, _Response.Object, worksheet.EString("Text"), encoding, worksheet.EString("MimeType"));

            byte[] expectedStreamContent = worksheet.Bytes("ResponseContent");
            byte[] actualStreamContent = _OutputStream.ToArray();

            Assert.IsTrue(expectedStreamContent.SequenceEqual(actualStreamContent));
            Assert.AreEqual(expectedStreamContent.Length, _Response.Object.ContentLength);
            Assert.AreEqual(worksheet.String("ResponseMimeType"), _Response.Object.MimeType);
            Assert.AreEqual(HttpStatusCode.OK, _Response.Object.StatusCode);
        }

        [TestMethod]
        public void Responder_SendText_Compresses_Response()
        {
            _Responder.SendText(_Request.Object, _Response.Object, "Hello", Encoding.UTF8, MimeType.Text);
            _Response.Verify(r => r.EnableCompression(_Request.Object), Times.Once());
        }
        #endregion

        #region SendJson
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Responser_SendJson_Throws_If_Request_Is_Null()
        {
            _Responder.SendJson(null, _Response.Object, new JsonExample(), null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Responser_SendJson_Throws_If_Response_Is_Null()
        {
            _Responder.SendJson(_Request.Object, null, new JsonExample(), null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Responser_SendJson_Throws_If_Json_Is_Null()
        {
            _Responder.SendJson(_Request.Object, _Response.Object, null, null, null);
        }

        [TestMethod]
        public void Responder_SendJson_Sends_JSON_File_Correctly()
        {
            var original = new JsonExample() { IntegerField = 102, DateField = new DateTime(2011, 11, 3, 4, 5, 6, 789) };

            _Responder.SendJson(_Request.Object, _Response.Object, original, null, null);

            var text = Encoding.UTF8.GetString(_OutputStream.ToArray());
            Assert.AreNotEqual(0, text.Length);
            Assert.AreEqual(text.Length, _Response.Object.ContentLength);

            JsonExample copy;
            using(var stream = new MemoryStream(Encoding.UTF8.GetBytes(text))) {
                DataContractJsonSerializer serialiser = new DataContractJsonSerializer(typeof(JsonExample));
                copy = (JsonExample)serialiser.ReadObject(stream);
            }

            Assert.AreEqual(MimeType.Json, _Response.Object.MimeType);
            Assert.AreEqual(HttpStatusCode.OK, _Response.Object.StatusCode);

            Assert.AreEqual(102, copy.IntegerField);
            Assert.AreEqual(new DateTime(2011, 11, 3, 4, 5, 6, 789), copy.DateField);

            _Response.Verify(r => r.AddHeader("Cache-Control", "max-age=0, no-cache, no-store, must-revalidate"), Times.Once());
        }

        [TestMethod]
        public void Responder_SendJson_Uses_Mime_Type_If_Supplied()
        {
            var original = new JsonExample() { IntegerField = 102, DateField = new DateTime(2011, 11, 3, 4, 5, 6, 789) };

            _Responder.SendJson(_Request.Object, _Response.Object, original, null, MimeType.Text);

            Assert.AreEqual(MimeType.Text, _Response.Object.MimeType);
        }

        [TestMethod]
        public void Responder_SendJson_Sends_JSONP_File_Correctly()
        {
            var original = new JsonExample() { IntegerField = 102, DateField = new DateTime(2011, 11, 3, 4, 5, 6, 789) };

            _Responder.SendJson(_Request.Object, _Response.Object, original, "mycallback", null);

            var text = Encoding.UTF8.GetString(_OutputStream.ToArray());
            Assert.AreNotEqual(0, text.Length);
            Assert.AreEqual(text.Length, _Response.Object.ContentLength);

            Assert.IsTrue(text.StartsWith("mycallback("));
            Assert.IsTrue(text.EndsWith(")"));
            text = text.Substring(11, text.Length - 12);

            JsonExample copy;
            using(var stream = new MemoryStream(Encoding.UTF8.GetBytes(text))) {
                DataContractJsonSerializer serialiser = new DataContractJsonSerializer(typeof(JsonExample));
                copy = (JsonExample)serialiser.ReadObject(stream);
            }

            Assert.AreEqual(MimeType.Json, _Response.Object.MimeType);
            Assert.AreEqual(HttpStatusCode.OK, _Response.Object.StatusCode);

            Assert.AreEqual(102, copy.IntegerField);
            Assert.AreEqual(new DateTime(2011, 11, 3, 4, 5, 6, 789), copy.DateField);

            _Response.Verify(r => r.AddHeader("Cache-Control", "max-age=0, no-cache, no-store, must-revalidate"), Times.Once());
        }
        
        [TestMethod]
        public void Responder_SendJson_Compresses_Response()
        {
            _Responder.SendJson(_Request.Object, _Response.Object, new JsonExample(), null, MimeType.Json);
        }
        #endregion

        #region SendImage
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Responser_SendImage_Throws_If_Request_Is_Null()
        {
            _Responder.SendImage(null, _Response.Object, _Image, ImageFormat.Png);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Responser_SendImage_Throws_If_Response_Is_Null()
        {
            _Responder.SendImage(_Request.Object, null, _Image, ImageFormat.Png);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Responser_SendImage_Throws_If_Image_Is_Null()
        {
            _Responder.SendImage(_Request.Object, _Response.Object, null, ImageFormat.Png);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Responser_SendImage_Throws_If_ImageFormat_Is_Null()
        {
            _Responder.SendImage(_Request.Object, _Response.Object, _Image, null);
        }

        [TestMethod]
        public void Responder_SendImage_Throws_If_ImageFormat_Is_Unsupported()
        {
            foreach(var property in typeof(ImageFormat).GetProperties().Where(p => p.Name != "Guid")) {
                TestCleanup();
                TestInitialise();

                bool supported = false;
                switch(property.Name) {
                    case "Bmp":
                    case "Gif":
                    case "Png":
                        supported = true;
                        break;
                }

                bool seenException = false;
                try {
                    _Responder.SendImage(_Request.Object, _Response.Object, _Image, (ImageFormat)property.GetValue(null, null));
                } catch(NotSupportedException) {
                    seenException = true;
                } catch(ArgumentNullException) { // <-- can be thrown for some formats if the image save went awry because we hadn't set up the save properly (which, in turn, is because it's not supported)
                    ; 
                }

                Assert.AreEqual(!supported, seenException, property.Name);
            }
        }

        [TestMethod]
        public void Responder_SendImage_Fills_Response_Correctly_For_Png_Images()
        {
            _Responder.SendImage(_Request.Object, _Response.Object, _Image, ImageFormat.Png);
            AssertImageMatches(ImageFormat.Png, MimeType.PngImage);
        }

        [TestMethod]
        public void Responder_SendImage_Fills_Response_Correctly_For_Gif_Images()
        {
            _Responder.SendImage(_Request.Object, _Response.Object, _Image, ImageFormat.Gif);
            AssertImageMatches(ImageFormat.Gif, MimeType.GifImage);
        }

        [TestMethod]
        public void Responder_SendImage_Fills_Response_Correctly_For_Bmp_Images()
        {
            _Responder.SendImage(_Request.Object, _Response.Object, _Image, ImageFormat.Bmp);
            AssertImageMatches(ImageFormat.Bmp, MimeType.BitmapImage);
        }

        [TestMethod]
        public void Responder_SendImage_Does_Not_Compress_Response()
        {
            _Responder.SendImage(_Request.Object, _Response.Object, _Image, ImageFormat.Bmp);
            _Response.Verify(r => r.EnableCompression(_Request.Object), Times.Never());
        }

        private void AssertImageMatches(ImageFormat imageFormat, string mimeType)
        {
            Assert.AreEqual(HttpStatusCode.OK, _Response.Object.StatusCode);
            Assert.AreEqual(mimeType, _Response.Object.MimeType);
            Assert.AreEqual(_OutputStream.Length, _Response.Object.ContentLength);
            _Response.Verify(r => r.AddHeader("Cache-Control", "max-age=21600"), Times.Once());

            using(var image = (Bitmap)Image.FromStream(_OutputStream)) {
                Assert.AreEqual(imageFormat, image.RawFormat);
                Assert.AreEqual(_Image.Width, image.Width);
                Assert.AreEqual(_Image.Height, image.Height);
                for(int x = 0;x < _Image.Width;++x) {
                    for(int y = 0;y < _Image.Height;++y) {
                        Assert.AreEqual(_Image.GetPixel(x, y), image.GetPixel(x, y), "Pixel at {0}, {1}", x, y);
                    }
                }
            }
        }
        #endregion

        #region SendAudio
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Responder_SendAudio_Throws_If_Request_Is_Null()
        {
            _Responder.SendAudio(null, _Response.Object, new byte[] {}, MimeType.WaveAudio);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Responder_SendAudio_Throws_If_Response_Is_Null()
        {
            _Responder.SendAudio(_Request.Object, null, new byte[] {}, MimeType.WaveAudio);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Responder_SendAudio_Throws_If_Audio_Is_Null()
        {
            _Responder.SendAudio(_Request.Object, _Response.Object, null, MimeType.WaveAudio);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Responder_SendAudio_Throws_If_MimeType_Is_Null()
        {
            _Responder.SendAudio(_Request.Object, _Response.Object, new byte[] {}, null);
        }

        [TestMethod]
        public void Responder_SendAudio_Copies_Audio_To_Response_Correctly()
        {
            var sourceAudio = new byte[] { 0x01, 0x02, 0xff, 0xfe };
            var mimeType = "MIME is not a profession";

            _Responder.SendAudio(_Request.Object, _Response.Object, sourceAudio, mimeType);

            Assert.AreEqual(sourceAudio.Length, _Response.Object.ContentLength);
            Assert.AreEqual(mimeType, _Response.Object.MimeType);
            Assert.AreEqual(HttpStatusCode.OK, _Response.Object.StatusCode);
            Assert.AreEqual(4, _Response.Object.OutputStream.Length);

            var outputStream = _OutputStream.ToArray();
            Assert.IsTrue(outputStream.SequenceEqual(sourceAudio));
        }

        [TestMethod]
        public void Responser_SendAudio_Compresses_WAV_Files()
        {
            _Responder.SendAudio(_Request.Object, _Response.Object, new byte[] { 0x01 }, MimeType.WaveAudio);
            _Response.Verify(r => r.EnableCompression(_Request.Object), Times.Once());
        }

        [TestMethod]
        public void Responser_SendAudio_Does_Not_Compress_Non_WAV_Audio()
        {
            _Responder.SendAudio(_Request.Object, _Response.Object, new byte[] { 0x01 }, "audio/whatever");
            _Response.Verify(r => r.EnableCompression(_Request.Object), Times.Never());
        }
        #endregion

        #region SendBinary
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Responder_SendBinary_Throws_If_Request_Is_Null()
        {
            _Responder.SendBinary(null, _Response.Object, new byte[] {}, MimeType.GifImage, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Responder_SendBinary_Throws_If_Response_Is_Null()
        {
            _Responder.SendBinary(_Request.Object, null, new byte[] {}, MimeType.GifImage, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Responder_SendBinary_Throws_If_Binary_Is_Null()
        {
            _Responder.SendBinary(_Request.Object, _Response.Object, null, MimeType.GifImage, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Responder_SendBinary_Throws_If_MimeType_Is_Null()
        {
            _Responder.SendBinary(_Request.Object, _Response.Object, new byte[] {}, null, false);
        }

        [TestMethod]
        public void Responder_SendBinary_Copies_Binary_To_Response_Correctly()
        {
            var sourceBinary = new byte[] { 0x01, 0x02, 0xff, 0xfe };
            var mimeType = "MIME is not a profession";

            _Responder.SendBinary(_Request.Object, _Response.Object, sourceBinary, mimeType, false);

            Assert.AreEqual(sourceBinary.Length, _Response.Object.ContentLength);
            Assert.AreEqual(mimeType, _Response.Object.MimeType);
            Assert.AreEqual(HttpStatusCode.OK, _Response.Object.StatusCode);
            Assert.AreEqual(4, _Response.Object.OutputStream.Length);

            var outputStream = _OutputStream.ToArray();
            Assert.IsTrue(outputStream.SequenceEqual(sourceBinary));
        }

        [TestMethod]
        public void Responser_SendBinary_Enables_Compression_When_Requested()
        {
            foreach(var compressResponse in new bool[] { false, true }) {
                TestCleanup();
                TestInitialise();

                _Responder.SendBinary(_Request.Object, _Response.Object, new byte[] { 0x01 }, "MIME ME", compressResponse);

                _Response.Verify(r => r.EnableCompression(_Request.Object), compressResponse ? Times.Once() : Times.Never());
            }
        }
        #endregion
    }
}
