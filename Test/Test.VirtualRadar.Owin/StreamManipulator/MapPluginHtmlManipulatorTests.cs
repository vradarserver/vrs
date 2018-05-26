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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.Owin.StreamManipulator
{
    [TestClass]
    public class MapPluginHtmlManipulatorTests
    {
        private const string MapStylesheetMarker = "<!-- [[ MAP STYLESHEET ]] -->";
        private const string MapPluginMarker = "<!-- [[ MAP PLUGIN ]] -->";
        private const string ExpectedGoogleJavaScript = @"<script src=""script/jquiplugin/jquery.vrs.map-google-maps.js"" type=""text/javascript""></script>";
        private const string ExpectedOpenStreetMapStylesheet = @"<link rel=""stylesheet"" href=""css/leaflet/leaflet.css"" type=""text/css"" media=""screen"" />";
        private const string ExpectedOpenStreetMapJavaScript = @"<script src=""script/leaflet-src.js"" type=""text/javascript""></script><script src=""script/jquiplugin/jquery.vrs.map-openstreetmap.js"" type=""text/javascript""></script>";

        public TestContext TestContext { get; set; }

        private IClassFactory _Snapshot;
        private IMapPluginHtmlManipulator _Manipulator;
        private MockOwinEnvironment _Environment;
        private global::VirtualRadar.Interface.Settings.Configuration _Configuration;
        private Mock<ISharedConfiguration> _SharedConfiguration;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _Environment = new MockOwinEnvironment();

            _Configuration = new global::VirtualRadar.Interface.Settings.Configuration();
            _SharedConfiguration = TestUtilities.CreateMockSingleton<ISharedConfiguration>();
            _SharedConfiguration.Setup(r => r.Get()).Returns(() => _Configuration);

            _Manipulator = Factory.Resolve<IMapPluginHtmlManipulator>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        private TextContent FakeCall(string htmlPath, string html)
        {
            var result = new TextContent() {
                Content = html,
                Encoding = Encoding.UTF8,
                HadPreamble = false
            };
            result.IsDirty = false;

            _Environment.RequestPath = htmlPath;

            return result;
        }

        private string StripHtmlWhitespace(string html)
        {
            var trimmedLines = String.Join("",
                html
                    .Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(r => r.Trim())
                    .Where(r => r != "")
            );

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(trimmedLines);

            var buffer = new StringBuilder();
            using(var writer = new StringWriter(buffer)) {
                htmlDocument.Save(writer);
            }

            return buffer.ToString();
        }

        private void AssertTextChanged(TextContent textContent, string expectedHtml)
        {
            expectedHtml = StripHtmlWhitespace(expectedHtml);
            var actualHtml = StripHtmlWhitespace(textContent.Content);

            Assert.AreEqual(expectedHtml, actualHtml, ignoreCase: true);
            Assert.IsTrue(textContent.IsDirty);
        }

        private void AssertTextUnchanged(TextContent textContent, string expectedHtml)
        {
            expectedHtml = StripHtmlWhitespace(expectedHtml);
            var actualHtml = StripHtmlWhitespace(textContent.Content);

            Assert.AreEqual(expectedHtml, actualHtml, ignoreCase: true);
        }

        [TestMethod]
        public void MapPluginHtmlManipulator_Replaces_Marker_With_Google_Maps_Reference()
        {
            var htmlPath = "/index.html";
            var textContent = FakeCall(htmlPath, $"{MapPluginMarker}");

            _Configuration.GoogleMapSettings.MapProvider = MapProvider.GoogleMaps;
            _Manipulator.ManipulateTextResponse(_Environment.Environment, textContent);

            AssertTextChanged(textContent, $@"<script src=""script/jquiplugin/jquery.vrs.map-google-maps.js"" type=""text/javascript""></script>");
        }

        [TestMethod]
        public void MapPluginHtmlManipulator_Does_Not_Replace_Stylesheet_With_Google_Maps_Reference()
        {
            var htmlPath = "/index.html";
            var textContent = FakeCall(htmlPath, $"{MapStylesheetMarker}");

            _Configuration.GoogleMapSettings.MapProvider = MapProvider.GoogleMaps;
            _Manipulator.ManipulateTextResponse(_Environment.Environment, textContent);

            AssertTextUnchanged(textContent, MapStylesheetMarker);
        }

        [TestMethod]
        public void MapPluginHtmlManipulator_Replaces_Marker_With_OpenStreetMap_Reference()
        {
            var htmlPath = "/index.html";
            var textContent = FakeCall(htmlPath, $"{MapPluginMarker}");

            _Configuration.GoogleMapSettings.MapProvider = MapProvider.OpenStreetMap;
            _Manipulator.ManipulateTextResponse(_Environment.Environment, textContent);

            AssertTextChanged(textContent, ExpectedOpenStreetMapJavaScript);
        }

        [TestMethod]
        public void MapPluginHtmlManipulator_Replaces_Stylesheet_With_OpenStreetMap_Reference()
        {
            var htmlPath = "/index.html";
            var textContent = FakeCall(htmlPath, $"{MapStylesheetMarker}");

            _Configuration.GoogleMapSettings.MapProvider = MapProvider.OpenStreetMap;
            _Manipulator.ManipulateTextResponse(_Environment.Environment, textContent);

            AssertTextChanged(textContent, ExpectedOpenStreetMapStylesheet);
        }

        [TestMethod]
        public void MapPluginHtmlManipulator_Only_Replaces_Marker_And_Stylesheet()
        {
            var htmlPath = "/index.html";
            var textContent = FakeCall(htmlPath, $"Start-{MapStylesheetMarker}-Middle-{MapPluginMarker}-End");

            _Configuration.GoogleMapSettings.MapProvider = MapProvider.OpenStreetMap;
            _Manipulator.ManipulateTextResponse(_Environment.Environment, textContent);

            AssertTextChanged(textContent, $"Start-{ExpectedOpenStreetMapStylesheet}-Middle-{ExpectedOpenStreetMapJavaScript}-End");
        }
    }
}
