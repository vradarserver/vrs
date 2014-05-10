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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebSite;
using InterfaceFactory;

namespace Test.VirtualRadar.WebSite
{
    [TestClass]
    public class MinifierTests
    {
        #region TestContext, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private IClassFactory _OriginalClassFactory;
        private IMinifier _Minifier { get; set; }
        private Configuration _Configuration;
        private Mock<IConfigurationStorage> _ConfigurationStorage;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalClassFactory = Factory.TakeSnapshot();

            _Configuration = new Configuration();
            _ConfigurationStorage = TestUtilities.CreateMockSingleton<IConfigurationStorage>();
            _ConfigurationStorage.Setup(r => r.Load()).Returns(_Configuration);

            _Minifier = Factory.Singleton.Resolve<IMinifier>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalClassFactory);
            _Minifier.Dispose();
        }
        #endregion

        #region Dispose
        [TestMethod]
        public void Minifier_Dispose_Unhooks_IConfigurationStorage()
        {
            _Minifier.Initialise();
            _Minifier.Dispose();

            _Configuration.GoogleMapSettings.EnableMinifying = false;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            var javaScript = "// This is a comment\r\nvar x = 1;\r\n";
            Assert.AreNotEqual(javaScript, _Minifier.MinifyJavaScript(javaScript));
        }

        [TestMethod]
        public void Minifier_Dispose_Can_Be_Safely_Called_Before_Initialise()
        {
            _Minifier.Dispose();
        }

        [TestMethod]
        public void Minifier_Dispose_Can_Be_Safely_Called_Twice()
        {
            _Minifier.Initialise();
            _Minifier.Dispose();
            _Minifier.Dispose();
        }
        #endregion

        #region Initialise
        [TestMethod]
        public void Minifier_Initialise_Hooks_IConfigurationStorage()
        {
            _Configuration.GoogleMapSettings.EnableMinifying = false;
            _Minifier.Initialise();

            var javaScript = "// This is a comment\r\nvar x = 1;\r\n";
            _Configuration.GoogleMapSettings.EnableMinifying = true;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreNotEqual(javaScript, _Minifier.MinifyJavaScript(javaScript));
        }
        #endregion

        #region MinifyJavascript
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Minifier_MinifyJavaScript_Throws_If_Called_Before_Initialise()
        {
            _Minifier.MinifyJavaScript(null);
        }

        [TestMethod]
        public void Minifier_MinifyJavaScript_Returns_Null_If_Passed_Null()
        {
            _Minifier.Initialise();
            Assert.IsNull(_Minifier.MinifyJavaScript(null));
        }

        [TestMethod]
        public void Minifier_MinifyJavaScript_Returns_Empty_String_If_Passed_Empty_String()
        {
            _Minifier.Initialise();
            Assert.AreEqual("", _Minifier.MinifyJavaScript(""));
        }

        [TestMethod]
        public void Minifier_MinifyJavaScript_Compresses_Javascript()
        {
            // We don't need a big test for this at the moment, we're using a 3rd party minifier. Just need
            // to know that it's doing something along the right lines.

            var expanded = "// This is a comment - this should go\r\n" +
                           "function publicName(parameter) {\r\n" +
                           "  var internalVariable = parameter();\r\n" +
                           "  this.publicFunction = function() { return 'a'; }\r\n" +
                           "}\r\n";

            _Minifier.Initialise();
            var minified = _Minifier.MinifyJavaScript(expanded);

            Assert.AreNotEqual(expanded, minified);
            Assert.IsFalse(minified.Contains("This is a comment"));
            Assert.IsTrue(minified.Contains("publicName"));
            Assert.IsTrue(minified.Contains("publicFunction"));
            Assert.IsFalse(minified.Contains("parameter"));
            Assert.IsFalse(minified.Contains("internalVariable"));
        }

        [TestMethod]
        public void Minifier_MinifyJavaScript_Does_Not_Compress_If_Configuration_Prohibits_It()
        {
            _Configuration.GoogleMapSettings.EnableMinifying = false;

            _Minifier.Initialise();
            var javaScript = "// This is a comment\r\nvar x = 1;\r\n";
            var minified = _Minifier.MinifyJavaScript(javaScript);

            Assert.AreEqual(javaScript, minified);
        }
        #endregion

        #region MinifyCss
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Minifier_MinifyCss_Throws_If_Called_Before_Initialise()
        {
            _Minifier.MinifyCss(null);
        }

        [TestMethod]
        public void Minifier_MinifyCss_Returns_Null_If_Passed_Null()
        {
            _Minifier.Initialise();
            Assert.IsNull(_Minifier.MinifyCss(null));
        }

        [TestMethod]
        public void Minifier_MinifyCss_Returns_Empty_String_If_Passed_Empty_String()
        {
            _Minifier.Initialise();
            Assert.AreEqual("", _Minifier.MinifyCss(""));
        }

        [TestMethod]
        public void Minifier_MinifyCss_Compresses_Javascript()
        {
            var css = "/* This is a comment */\r\nHTML, BODY\r\n{\r\n    background: #000000; }\r\n";

            _Minifier.Initialise();
            var minified = _Minifier.MinifyCss(css);

            Assert.AreNotEqual(css, minified);
            Assert.IsFalse(minified.Contains("This is a comment"));
            Assert.IsFalse(minified.Contains("\r\n"));
            Assert.IsFalse(minified.Contains("  "));
        }

        [TestMethod]
        public void Minifier_MinifyCss_Does_Not_Compress_If_Configuration_Prohibits_It()
        {
            _Configuration.GoogleMapSettings.EnableMinifying = false;

            _Minifier.Initialise();
            var css = "/* A comment */\r\nHTML { color: #001122; }\r\n";
            var minified = _Minifier.MinifyCss(css);

            Assert.AreEqual(css, minified);
        }
        #endregion
    }
}
