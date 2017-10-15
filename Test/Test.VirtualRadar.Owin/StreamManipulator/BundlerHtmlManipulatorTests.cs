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

namespace Test.VirtualRadar.Owin.StreamManipulator
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

            _BundlerConfiguration.Setup(r => r.RegisterJavascriptBundle(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                .Returns((string htmlPath, int bundleIndex, IEnumerable<string> javascriptLinks) => {
                    _LastHtmlPath = htmlPath;
                    _AllHtmlPaths.Add(htmlPath);

                    _LastBundleIndex = bundleIndex;
                    _AllBundleIndexes.Add(bundleIndex);

                    _LastJavascriptLinks.Clear();
                    _LastJavascriptLinks.AddRange(javascriptLinks);
                    _AllJavascriptLinks.Add(new List<string>(javascriptLinks));

                    return FakeHtmlPathIndexLink(htmlPath, bundleIndex);
                }
            );

            _Manipulator = Factory.Singleton.Resolve<IBundlerHtmlManipulator>();
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

            _Environment.RequestPath = htmlPath;

            return result;
        }

        private string StripHtmlWhitespace(string html)
        {
            var stripped = (html ?? "").Replace("\t", " ").Replace("\r", " ").Replace("\n", " ");
            var len = stripped.Length;
            var newLen = len;
            do {
                len = newLen;
                stripped = stripped.Replace("  ", " ");
                newLen = stripped.Length;
            } while(len != newLen);

            return stripped.Trim();
        }

        private void AssertTextChanged(TextContent textContent, string expectedHtml)
        {
            expectedHtml = StripHtmlWhitespace(expectedHtml);
            var actualHtml = StripHtmlWhitespace(textContent.Content);

            Assert.AreEqual(expectedHtml, actualHtml);
            Assert.IsTrue(textContent.IsDirty);
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

            AssertTextChanged(textContent, $@"<script src=""{FakeHtmlPathIndexLink(htmlPath, 0)}"" type=""text/javascript""></script>");
        }
    }
}
