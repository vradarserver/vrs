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
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;

namespace Test.VirtualRadar.Owin.StreamManipulator
{
    [TestClass]
    public class JavascriptManipulatorTests : ManipulatorTests
    {
        private Mock<IJavascriptManipulatorConfiguration> _Config;
        private IJavascriptManipulator _Manipulator;
        private Mock<IMinifier> _Minifier;
        private List<ITextResponseManipulator> _Manipulators;

        protected override void ExtraInitialise()
        {
            _Config = TestUtilities.CreateMockImplementation<IJavascriptManipulatorConfiguration>();
            _Manipulators = new List<ITextResponseManipulator>();
            _Config.Setup(r => r.GetTextResponseManipulators()).Returns(() => _Manipulators);

            _Minifier = TestUtilities.CreateMockImplementation<IMinifier>();
            _Minifier.Setup(r => r.MinifyJavaScript(It.IsAny<string>())).Returns((string r) => r);

            _Manipulator = Factory.Singleton.Resolve<IJavascriptManipulator>();
        }

        [TestMethod]
        public void JavascriptManipulator_ManipulateResponseStream_Calls_Any_Registered_Manipulators()
        {
            var manipulator = new TextManipulator();
            _Manipulators.Add(manipulator);

            SetResponseContent(MimeType.Javascript, "a");
            _Manipulator.ManipulateResponseStream(_Environment.Environment);

            Assert.AreEqual(1, manipulator.CallCount);
            Assert.AreSame(_Environment.Environment, manipulator.Environment);
            Assert.AreEqual("a", manipulator.TextContent.Content);
        }

        [TestMethod]
        public void JavascriptManipulator_ManipulateResponseStream_Minifies_Javascript()
        {
            SetResponseContent(MimeType.Javascript, "a");
            _Manipulator.ManipulateResponseStream(_Environment.Environment);
            _Minifier.Verify(r => r.MinifyJavaScript("a"), Times.Once());
        }

        [TestMethod]
        public void JavascriptManipulator_ManipulateResponseStream_Does_Not_Minify_NonJavascript()
        {
            SetResponseContent(MimeType.Html, "a");
            _Manipulator.ManipulateResponseStream(_Environment.Environment);
            _Minifier.Verify(r => r.MinifyJavaScript(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void JavascriptManipulator_ManipulateResponseStream_Writes_Minified_Javascript_To_Stream()
        {
            SetResponseContent(MimeType.Javascript, "abc");
            _Minifier.Setup(r => r.MinifyJavaScript("abc")).Returns("z");

            _Manipulator.ManipulateResponseStream(_Environment.Environment);

            var textContent = GetResponseContent();
            Assert.AreEqual("z", textContent.Content);
            Assert.AreNotEqual(0, _Environment.Response.Body.Position);    // MemoryStream will throw an exception if you read this after it's been disposed
        }

        [TestMethod]
        public void JavascriptManipulator_ManipulateResponseStream_Ignores_Minifier_Output_If_Not_Shorter_Than_Original()
        {
            SetResponseContent(MimeType.Javascript, "abc");
            _Minifier.Setup(r => r.MinifyJavaScript("abc")).Returns("xyz");

            _Manipulator.ManipulateResponseStream(_Environment.Environment);

            Assert.AreEqual("abc", GetResponseContent().Content);
        }

        [TestMethod]
        public void JavaScriptManipulator_ManipulateResponseStream_Writes_Preamble_If_Original_Had_Preamble()
        {
            SetResponseContent(MimeType.Javascript, "abc", Encoding.Unicode, addPreamble: true);
            _Minifier.Setup(r => r.MinifyJavaScript("abc")).Returns("z");

            _Manipulator.ManipulateResponseStream(_Environment.Environment);

            var textContent = GetResponseContent();
            Assert.AreEqual("z", textContent.Content);
            Assert.AreEqual(Encoding.Unicode.EncodingName, textContent.Encoding.EncodingName);
            Assert.IsTrue(textContent.HadPreamble);
            Assert.AreEqual(Encoding.Unicode.GetPreamble().Length + 2, _Environment.Response.Body.Length);
        }
    }
}
