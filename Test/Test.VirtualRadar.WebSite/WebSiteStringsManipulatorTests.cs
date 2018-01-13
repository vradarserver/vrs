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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface;
using VirtualRadar.WebSite;

namespace Test.VirtualRadar.WebSite
{
    [TestClass]
    [DeploymentItem("de-DE", "de-DE")]
    [DeploymentItem("fr-FR", "fr-FR")]
    [DeploymentItem("pt-BR", "pt-BR")]
    [DeploymentItem("ru-RU", "ru-RU")]
    [DeploymentItem("tr-TR", "tr-TR")]
    [DeploymentItem("zh-CN", "zh-CN")]
    public class WebSiteStringsManipulatorTests
    {
        public TestContext TestContext { get; set; }

        private WebSiteStringsManipulator _Manipulator;
        private Dictionary<string, object> _Environment;
        private TextContent _TextContent;
        private string _I18NPath;

        private const string _JavascriptWithMarkers = @"
    // [[ MARKER START SIMPLE STRINGS ]]
    // [[ MARKER END SIMPLE STRINGS ]]
        ";

        [TestInitialize]
        public void TestInitialise()
        {
            _Manipulator = new WebSiteStringsManipulator();

            _Environment = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            _TextContent = new TextContent();

            _I18NPath = Path.Combine(TestContext.DeploymentDirectory, @"Web\script\i18n");
        }

        private void ConfigureRequestPath(string path)
        {
            _Environment.Add("owin.RequestPath", path);
        }

        private IEnumerable<string> WebSiteStringFileNames()
        {
            return Directory.GetFiles(_I18NPath, "strings.*").Select(r => Path.GetFileName(r));
        }

        private string GetWebsiteStringLanguage(string fileName)
        {
            return Regex.Match(fileName, @"strings\.(?<lang>.*)\.js", RegexOptions.IgnoreCase).Groups["lang"].Value?.ToLower();
        }

        [TestMethod]
        public void WebSiteStringsManipulator_Injects_Web_Site_Strings_Into_Javascript()
        {
            foreach(var stringsFileName in WebSiteStringFileNames()) {
                TestInitialise();
                ConfigureRequestPath($"/script/i18n/{stringsFileName}");
                _TextContent.Content = _JavascriptWithMarkers;

                _Manipulator.ManipulateTextResponse(_Environment, _TextContent);

                string expected = null;
                var stringsLanguage = GetWebsiteStringLanguage(stringsFileName);
                if(!stringsLanguage.Contains('-')) {
                    expected = $@"VRS.$$.LanguageCode = '{stringsLanguage}-";
                } else {
                    expected = $@"VRS.$$.LanguageCode = '{stringsLanguage}";
                }
                Assert.AreNotEqual(_TextContent.Content, _JavascriptWithMarkers, $"No substitution took place for {stringsFileName}");
                Assert.IsTrue(_TextContent.Content.IndexOf(expected, StringComparison.OrdinalIgnoreCase) != -1, $"Did not find expected language string for {stringsFileName}");
            }
        }

        [TestMethod]
        public void WebSiteStringsManipulator_Does_Not_Inject_Web_Site_Strings_Into_Other_Javascript()
        {
            ConfigureRequestPath($"/script/i18n/other-strings.en.js");
            _TextContent.Content = _JavascriptWithMarkers;

            _Manipulator.ManipulateTextResponse(_Environment, _TextContent);

            Assert.AreEqual(_JavascriptWithMarkers, _TextContent.Content);
        }

        [TestMethod]
        public void WebSiteStringsManipulator_Does_Not_Throw_Exception_If_Unknown_Language_Is_Passed()
        {
            ConfigureRequestPath($"/script/i18n/strings.zz.js");
            _TextContent.Content = _JavascriptWithMarkers;

            _Manipulator.ManipulateTextResponse(_Environment, _TextContent);

            Assert.AreEqual(_JavascriptWithMarkers, _TextContent.Content);
        }
    }
}
