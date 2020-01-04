// Copyright © 2017 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Collections.Generic;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.WebServer;

namespace Test.VirtualRadar.Owin.StreamManipulator
{
    [TestClass]
    public class HtmlManipulatorTests : ManipulatorTests
    {
        private Mock<IHtmlManipulatorConfiguration> _Config;
        private IHtmlManipulator _Manipulator;
        private List<ITextResponseManipulator> _Manipulators;

        protected override void ExtraInitialise()
        {
            _Config = TestUtilities.CreateMockImplementation<IHtmlManipulatorConfiguration>();
            _Manipulators = new List<ITextResponseManipulator>();
            _Config.Setup(r => r.GetTextResponseManipulators()).Returns(() => _Manipulators);

            _Manipulator = Factory.Resolve<IHtmlManipulator>();
        }

        [TestMethod]
        public void HtmlManipulator_ManipulateResponseStream_Calls_Any_Registered_Manipulators()
        {
            var manipulator = new TextManipulator();
            _Manipulators.Add(manipulator);

            SetResponseContent(MimeType.Html, "a");
            _Manipulator.ManipulateResponseStream(_Environment.Environment);

            Assert.AreEqual(1, manipulator.CallCount);
            Assert.AreSame(_Environment.Environment, manipulator.Environment);
            Assert.AreEqual("a", manipulator.TextContent.Content);
        }

        [TestMethod]
        public void HtmlManipulator_Initialises_TextContent_IsDirty_To_False()
        {
            var manipulator = new TextManipulator();
            _Manipulators.Add(manipulator);

            SetResponseContent(MimeType.Html, "a");
            _Manipulator.ManipulateResponseStream(_Environment.Environment);

            Assert.IsFalse(manipulator.TextContent.IsDirty);
        }

        [TestMethod]
        public void HtmlManipulator_ManipulateResponseStream_Writes_Manipulated_Response()
        {
            var manipulator = new TextManipulator {
                Callback = (env, content) => content.Content = "b"
            };
            _Manipulators.Add(manipulator);

            SetResponseContent(MimeType.Html, "a");
            _Manipulator.ManipulateResponseStream(_Environment.Environment);

            var textContent = GetResponseContent();
            Assert.AreEqual("b", textContent.Content);
            Assert.AreNotEqual(0, _Environment.ResponseBody.Position);    // MemoryStream will throw an exception if you read this after it's been disposed
        }

        [TestMethod]
        public void HtmlManipulator_ManipulateResponseStream_Only_Writes_Dirty_TextContent()
        {
            var manipulator = new TextManipulator {
                Callback = (env, content) => {
                    content.Content = "b";
                    content.IsDirty = false;
                }
            };
            _Manipulators.Add(manipulator);

            SetResponseContent(MimeType.Html, "a");
            _Manipulator.ManipulateResponseStream(_Environment.Environment);

            var textContent = GetResponseContent();
            Assert.AreEqual("a", textContent.Content);
        }
    }
}
