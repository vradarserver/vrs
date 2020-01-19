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
using AWhewell.Owin.Utility;
using HtmlAgilityPack;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.WebSite.StreamManipulator
{
    [TestClass]
    public class BundlerHtmlManipulatorTests
    {
        private const string BundleStart = "<!-- [[ JS BUNDLE START ]] -->";
        private const string BundleEnd =   "<!-- [[ BUNDLE END ]] -->";

        public TestContext TestContext { get; set; }

        private IClassFactory _Snapshot;
        private IBundlerHtmlManipulator _Manipulator;
        private MockOwinEnvironment _Environment;
        private Mock<IBundlerConfiguration> _BundlerConfiguration;
        private string _LastHtmlPath;
        private List<string> _AllHtmlPaths;
        private int _LastBundleIndex;
        private List<int> _AllBundleIndexes;
        private List<string> _LastJavascriptLinks;
        private List<List<string>> _AllJavascriptLinks;
        private global::VirtualRadar.Interface.Settings.Configuration _Configuration;
        private Mock<ISharedConfiguration> _SharedConfiguration;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _Environment = new MockOwinEnvironment();

            _Configuration = new global::VirtualRadar.Interface.Settings.Configuration();
            _Configuration.GoogleMapSettings.EnableBundling = true;
            _SharedConfiguration = TestUtilities.CreateMockSingleton<ISharedConfiguration>();
            _SharedConfiguration.Setup(r => r.Get()).Returns(() => _Configuration);

            _BundlerConfiguration = TestUtilities.CreateMockImplementation<IBundlerConfiguration>();
            _LastHtmlPath = null;
            _AllHtmlPaths = new List<string>();
            _LastBundleIndex = -1;
            _AllBundleIndexes = new List<int>();
            _LastJavascriptLinks = new List<string>();
            _AllJavascriptLinks = new List<List<string>>();

            _BundlerConfiguration.Setup(r => r.RegisterJavascriptBundle(It.IsAny<IDictionary<string, object>>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                .Returns((IDictionary<string, object> htmlEnvironment, int bundleIndex, IEnumerable<string> javascriptLinks) => {
                    var context = OwinContext.Create(htmlEnvironment);
                    _LastHtmlPath = context.RequestPath;
                    _AllHtmlPaths.Add(_LastHtmlPath);

                    _LastBundleIndex = bundleIndex;
                    _AllBundleIndexes.Add(bundleIndex);

                    _LastJavascriptLinks.Clear();
                    _LastJavascriptLinks.AddRange(javascriptLinks);
                    _AllJavascriptLinks.Add(new List<string>(javascriptLinks));

                    return FakeHtmlPathIndexLink(_LastHtmlPath, bundleIndex);
                }
            );

            _Manipulator = Factory.Resolve<IBundlerHtmlManipulator>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        private string FakeHtmlPathIndexLink(string htmlPath, int bundleIndex)
        {
            return $"{htmlPath}-{bundleIndex}";
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
            Assert.IsFalse(textContent.IsDirty);

            _BundlerConfiguration.Verify(r => r.RegisterJavascriptBundle(It.IsAny<IDictionary<string, object>>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()), Times.Never());
        }

        [TestMethod]
        public void BundlerHtmlManipulator_Replaces_JavaScript_Links_With_Bundle_Path()
        {
            var htmlPath = "/index.html";
            var textContent = FakeCall(htmlPath, $@"
                {BundleStart}
                <script src=""file1.js"" type=""text/javascript""></script>
                <script src=""file2.js""></script>
                {BundleEnd}"
            );

            _Manipulator.ManipulateTextResponse(_Environment.Environment, textContent);

            Assert.AreEqual(htmlPath, _LastHtmlPath);
            Assert.AreEqual(0, _LastBundleIndex);
            Assert.IsTrue(new string[] { "file1.js", "file2.js" }.SequenceEqual(_LastJavascriptLinks));

            AssertTextChanged(textContent, $@"<script src=""{FakeHtmlPathIndexLink(htmlPath, 0)}"" type=""text/javascript""></script>");
        }

        [TestMethod]
        public void BundlerHtmlManipulator_Replaces_Multiple_Bundles_On_Page()
        {
            var htmlPath = "/index.html";
            var textContent = FakeCall(htmlPath, $@"
                {BundleStart}
                <script src=""file1.js"" type=""text/javascript""></script>
                {BundleEnd}
                {BundleStart}
                <script src=""file2.js""></script>
                {BundleEnd}
            ");

            _Manipulator.ManipulateTextResponse(_Environment.Environment, textContent);

            Assert.AreEqual(htmlPath, _AllHtmlPaths[0]);
            Assert.AreEqual(0, _AllBundleIndexes[0]);
            Assert.IsTrue(new string[] { "file1.js", }.SequenceEqual(_AllJavascriptLinks[0]));

            Assert.AreEqual(htmlPath, _AllHtmlPaths[1]);
            Assert.AreEqual(1, _AllBundleIndexes[1]);
            Assert.IsTrue(new string[] { "file2.js", }.SequenceEqual(_AllJavascriptLinks[1]));

            AssertTextChanged(textContent, $@"
                <script src=""{FakeHtmlPathIndexLink(htmlPath, 0)}"" type=""text/javascript""></script>
                <script src=""{FakeHtmlPathIndexLink(htmlPath, 1)}"" type=""text/javascript""></script>
            ");
        }

        [TestMethod]
        public void BundlerHtmlManipulator_Returns_HTML_Unchanged_If_It_Does_Not_Contain_Bundle_Commands()
        {
            var html = $@"
                <HTML><HEAD>
                <SCRIPT SRC=""source-1.js"" TYPE=""text/javascript""></SCRIPT>
                <SCRIPT SRC=""source-2.js"" TYPE=""text/javascript""></SCRIPT>
                </HEAD></HTML>
            ";
            var textContent = FakeCall("/index.html", html);

            _Manipulator.ManipulateTextResponse(_Environment.Environment, textContent);

            AssertTextUnchanged(textContent, html);
        }

        [TestMethod]
        public void BundlerHtmlManipulator_Returns_HTML_Unchanged_If_Bundling_Has_Been_Disabled()
        {
            _Configuration.GoogleMapSettings.EnableBundling = false;

            var html = $@"
                {BundleStart}
                <script src=""file1.js"" type=""text/javascript""></script>
                <script src=""file2.js""></script>
                {BundleEnd}
            ";
            var textContent = FakeCall("/index.html", html);

            _Manipulator.ManipulateTextResponse(_Environment.Environment, textContent);

            AssertTextUnchanged(textContent, html);
        }

        [TestMethod]
        public void BundlerHtmlManipulator_Removes_Blank_Lines_And_Comments_In_Bundle()
        {
            var htmlPath = "/index.html";
            var textContent = FakeCall(htmlPath, $@"
                {BundleStart}
                <!-- comment -->

                <script src=""file1.js"" type=""text/javascript""></script>

                {BundleEnd}"
            );

            _Manipulator.ManipulateTextResponse(_Environment.Environment, textContent);

            AssertTextChanged(textContent, $@"<script src=""{FakeHtmlPathIndexLink(htmlPath, 0)}"" type=""text/javascript""></script>");
        }

        [TestMethod]
        public void BundlerHtmlManipulator_Only_Replaces_Javascript_Tags()
        {
            var htmlPath = "/index.html";
            var textContent = FakeCall(htmlPath, $@"
                {BundleStart}
                <!-- comment -->
                <SCRIPT SRC=""source-1.js"" TYPE=""text/javascript""></SCRIPT>
                <SCRIPT SRC=""source-2.js"" TYPE=""text/ecma""></SCRIPT>
                <SCRIPT SRC=""source-3.js""></SCRIPT>
                <SCRIPT SRC=""source-4.js"" TYPE=""TEXT/JAVASCRIPT""></SCRIPT>
                <LINK REL=""stylesheet"" HREF=""source-1.css"" TYPE=""text/css"" MEDIA=""screen"" />
                {BundleEnd}"
            );

            _Manipulator.ManipulateTextResponse(_Environment.Environment, textContent);

            Assert.IsTrue(new string[] { "source-1.js", "source-3.js", "source-4.js" }.SequenceEqual(_LastJavascriptLinks));

            AssertTextChanged(textContent, $@"
                <script src=""{FakeHtmlPathIndexLink(htmlPath, 0)}"" type=""text/javascript""></script>
                <SCRIPT SRC=""source-2.js"" TYPE=""text/ecma""></SCRIPT>
                <LINK REL=""stylesheet"" HREF=""source-1.css"" TYPE=""text/css"" MEDIA=""screen"" />
            ");
        }

        [TestMethod]
        public void BundlerHtmlManipulator_Stops_If_The_End_Tag_Is_Malformed()
        {
            var htmlPath = "/index.html";
            var textContent = FakeCall(htmlPath, $@"
                <HTML><HEAD>
                <!-- [[ JS BUNDLE START ]] -->
                <SCRIPT SRC=""source-1.js"" TYPE=""text/javascript""></SCRIPT>
                <SCRIPT SRC=""source-2.js"" TYPE=""text/javascript""></SCRIPT>
                <!--  [[ BUNDLE END ]] -->
                </HEAD></HTML>
            ");

            _Manipulator.ManipulateTextResponse(_Environment.Environment, textContent);

            AssertTextChanged(textContent, $@"
                <HTML><HEAD>
                <!-- [[ JS BUNDLE START ]] -->
                <SCRIPT SRC=""source-1.js"" TYPE=""text/javascript""></SCRIPT>
                <SCRIPT SRC=""source-2.js"" TYPE=""text/javascript""></SCRIPT>
                <!--  [[ BUNDLE END ]] -->
                </HEAD></HTML>
                <!-- BUNDLE PARSER ERROR -->
                <!-- Bundle start at line 3 has no end -->
            ");
        }

        [TestMethod]
        public void BundlerHtmlManipulator_Stops_If_End_Tag_Has_No_Start_Tag()
        {
            var htmlPath = "/index.html";
            var textContent = FakeCall(htmlPath, $@"
                <HTML><HEAD>
                <!--  [[ JS BUNDLE START ]] -->
                <SCRIPT SRC=""source-1.js"" TYPE=""text/javascript""></SCRIPT>
                <SCRIPT SRC=""source-2.js"" TYPE=""text/javascript""></SCRIPT>
                <!-- [[ BUNDLE END ]] -->
                </HEAD></HTML>
            ");

            _Manipulator.ManipulateTextResponse(_Environment.Environment, textContent);

            AssertTextChanged(textContent, $@"
                <HTML><HEAD>
                <!--  [[ JS BUNDLE START ]] -->
                <SCRIPT SRC=""source-1.js"" TYPE=""text/javascript""></SCRIPT>
                <SCRIPT SRC=""source-2.js"" TYPE=""text/javascript""></SCRIPT>
                <!-- [[ BUNDLE END ]] -->
                </HEAD></HTML>
                <!-- BUNDLE PARSER ERROR -->
                <!-- Bundle end at line 6 has no start -->
            ");
        }

        [TestMethod]
        public void BundlerHtmlManipulator_Stops_If_Tags_Are_Nested()
        {
            var htmlPath = "/index.html";
            var textContent = FakeCall(htmlPath, $@"
                <HTML><HEAD>
                <!-- [[ JS BUNDLE START ]] -->
                <SCRIPT SRC=""source-1.js"" TYPE=""text/javascript""></SCRIPT>
                <!-- [[ JS BUNDLE START ]] -->
                <SCRIPT SRC=""source-2.js"" TYPE=""text/javascript""></SCRIPT>
                <!-- [[ BUNDLE END ]] -->
                <!-- [[ BUNDLE END ]] -->
                </HEAD></HTML>
            ");

            _Manipulator.ManipulateTextResponse(_Environment.Environment, textContent);

            AssertTextChanged(textContent, $@"
                <HTML><HEAD>
                <!-- [[ JS BUNDLE START ]] -->
                <SCRIPT SRC=""source-1.js"" TYPE=""text/javascript""></SCRIPT>
                <!-- [[ JS BUNDLE START ]] -->
                <SCRIPT SRC=""source-2.js"" TYPE=""text/javascript""></SCRIPT>
                <!-- [[ BUNDLE END ]] -->
                <!-- [[ BUNDLE END ]] -->
                </HEAD></HTML>
                <!-- BUNDLE PARSER ERROR -->
                <!-- Bundle start at line 3 has another bundle start nested within it at line 5 -->
            ");
        }
    }
}
