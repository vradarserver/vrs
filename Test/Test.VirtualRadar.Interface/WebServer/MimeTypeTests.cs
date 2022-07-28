// Copyright © 2013 onwards, Andrew Whewell
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
using Test.Framework;

namespace Test.VirtualRadar.Interface.WebServer
{
    [TestClass]
    public class MimeTypeTests
    {
        #region Test initialisation
        public TestContext TestContext { get; set; }
        #endregion

        #region Static Properties
        [TestMethod]
        public void MimeType_Static_Properties_Return_Correct_Strings()
        {
            Assert.AreEqual("image/bmp",                MimeType.BitmapImage);
            Assert.AreEqual("text/css",                 MimeType.Css);
            Assert.AreEqual("image/gif",                MimeType.GifImage);
            Assert.AreEqual("text/html",                MimeType.Html);
            Assert.AreEqual("image/x-icon",             MimeType.IconImage);
            Assert.AreEqual("application/javascript",   MimeType.Javascript);
            Assert.AreEqual("image/jpeg",               MimeType.JpegImage);
            Assert.AreEqual("application/json",         MimeType.Json);
            Assert.AreEqual("image/png",                MimeType.PngImage);
            Assert.AreEqual("text/plain",               MimeType.Text);
            Assert.AreEqual("image/tiff",               MimeType.TiffImage);
            Assert.AreEqual("audio/x-wav",              MimeType.WaveAudio);
        }
        #endregion

        #region GetForExtension
        [TestMethod]
        public void MimeType_GetForExtension_Returns_Null_When_Passed_Null()
        {
            Assert.AreEqual(null, MimeType.GetForExtension(null));
        }

        [TestMethod]
        public void MimeType_GetForExtension_Returns_Null_When_Passed_Empty_String()
        {
            Assert.AreEqual(null, MimeType.GetForExtension(""));
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "MimeTypes$")]
        public void MimeType_GetForExtension_Returns_Correct_MimeType_For_Extension()
        {
            var excelWorkSheet = new ExcelWorksheetData(TestContext);
            var mimeType = excelWorkSheet.String("MimeType");
            var extension = excelWorkSheet.String("Extension");

            Assert.AreEqual(mimeType, MimeType.GetForExtension(extension), extension);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "MimeTypes$")]
        public void MimeType_GetForExtension_Returns_Correct_MimeType_For_Extensions_With_Leading_Full_Stop()
        {
            var excelWorkSheet = new ExcelWorksheetData(TestContext);
            var mimeType = excelWorkSheet.String("MimeType");
            var extension = String.Format(".{0}", excelWorkSheet.String("Extension"));

            Assert.AreEqual(mimeType, MimeType.GetForExtension(extension), extension);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "MimeTypes$")]
        public void MimeType_GetForExtension_Is_Case_Insensitive()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            var mimeType = worksheet.String("MimeType");
            var extension = String.Format(".{0}", worksheet.String("Extension"));

            Assert.AreEqual(mimeType, MimeType.GetForExtension(extension.ToLowerInvariant()), extension.ToLowerInvariant());
            Assert.AreEqual(mimeType, MimeType.GetForExtension(extension.ToUpperInvariant()), extension.ToUpperInvariant());
        }

        [TestMethod]
        public void MimeType_GetForExtension_Returns_Octet_Mime_Type_For_Unknown_Extensions()
        {
            Assert.AreEqual("application/octet-stream", MimeType.GetForExtension("Well, this is just gibberish!"));
        }
        #endregion

        #region GetKnownExtensions
        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "MimeTypes$")]
        public void MimeType_GetKnownExtensions_Returns_Reasonable_List()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            var extension = worksheet.String("Extension");

            var extensions = MimeType.GetKnownExtensions();
            Assert.IsTrue(extensions.Contains(extension, StringComparer.InvariantCultureIgnoreCase));
        }

        [TestMethod]
        public void MimeType_GetKnownExtensions_Returns_Unique_List()
        {
            var extensions = MimeType.GetKnownExtensions();
            var distinctExtensions = extensions.Distinct().ToArray();
            Assert.AreEqual(extensions.Length, distinctExtensions.Length);
        }
        #endregion

        #region GetContentClassification
        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "Classification$")]
        public void MimeType_GetContentClassification_Returns_Correct_Content_Classification_For_Mime_Type()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            var mimeType = worksheet.EString("MimeType");
            var classification = worksheet.ParseEnum<ContentClassification>("Classification");

            Assert.AreEqual(classification, MimeType.GetContentClassification(mimeType));
        }
        #endregion
    }
}
