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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.WebSite;

namespace Test.VirtualRadar.Owin.Configuration
{
    [TestClass]
    public class BundlerConfigurationTests
    {
        public TestContext TestContext { get; set; }

        private IClassFactory _Snapshot;
        private IBundlerConfiguration _Config;
        private MockOwinEnvironment _Environment;
        private Mock<ILoopbackHost> _LoopbackHost;
        private Dictionary<string, string> _LoopbackContent;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _Environment = new MockOwinEnvironment();
            _LoopbackHost = TestUtilities.CreateMockImplementation<ILoopbackHost>();
            _LoopbackContent = new Dictionary<string, string>();
            _LoopbackHost.Setup(r => r.SendSimpleRequest(It.IsAny<string>())).Returns((string path) => {
                var result = new SimpleContent();
                if(!_LoopbackContent.TryGetValue(path, out string content)) {
                    result.HttpStatusCode = HttpStatusCode.NotFound;
                } else {
                    result.Content = Encoding.UTF8.GetBytes(content);
                    result.HttpStatusCode = HttpStatusCode.OK;
                }

                return result;
            });

            _Config = Factory.Singleton.ResolveNewInstance<IBundlerConfiguration>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        private void AddContent(string path, string content)
        {
            if(_LoopbackContent.ContainsKey(path)) {
                _LoopbackContent[path] = content;
            } else {
                _LoopbackContent.Add(path, content);
            }
        }

        [TestMethod]
        public void BundlerConfiguration_Replays_Simple_Configuration()
        {
            AddContent("/bundled.js", "abc");
            var path = _Config.RegisterJavascriptBundle("/index.html", 0, new List<string>() { "/bundled.js" });

            _Environment.RequestPath = path;
            var bundled = _Config.GetJavascriptBundle(path, _Environment.Environment);

            Assert.AreEqual("/* /bundled.js */\r\nabc\r\n", bundled);
        }

        [TestMethod]
        public void BundlerConfiguration_Replays_Two_Files_Joined_Together()
        {
            AddContent("/first.js", "abc");
            AddContent("/second.js", "xyz");
            var path = _Config.RegisterJavascriptBundle("/index.html", 0, new List<string>() { "/first.js", "/second.js" });

            _Environment.RequestPath = path;
            var bundled = _Config.GetJavascriptBundle(path, _Environment.Environment);

            Assert.AreEqual("/* /first.js */\r\nabc\r\n;\r\n/* /second.js */\r\nxyz\r\n", bundled);
        }

        [TestMethod]
        public void BundlerConfiguration_Reports_Issues_With_Fetching_Source()
        {
            AddContent("/present.js", "abc");
            var path = _Config.RegisterJavascriptBundle("/index.html", 0, new List<string>() { "/present.js", "/missing.js" });

            _Environment.RequestPath = path;
            var bundled = _Config.GetJavascriptBundle(path, _Environment.Environment);

            Assert.AreEqual("/* /present.js */\r\nabc\r\n;\r\n/* /missing.js */\r\n// Status 404 on /missing.js\r\n", bundled);
        }

        [TestMethod]
        public void BundlerConfiguration_Handles_Relative_Paths_On_JavaScript_Links()
        {
            AddContent("/folder/source.js", "abc");
            var path = _Config.RegisterJavascriptBundle("/folder/file.html", 0, new List<string>() { "source.js" });

            _Environment.RequestPath = path;
            var bundled = _Config.GetJavascriptBundle(path, _Environment.Environment);

            Assert.AreEqual("/* /folder/source.js */\r\nabc\r\n", bundled);
        }

        [TestMethod]
        public void BundlerConfiguration_Returns_Different_Paths_For_Different_Indexes()
        {
            var path1 = _Config.RegisterJavascriptBundle("/index.html", 0, new List<string>() { "1", "2" });
            var path2 = _Config.RegisterJavascriptBundle("/index.html", 1, new List<string>() { "3", "4" });

            Assert.AreNotEqual(path1, path2);
        }

        [TestMethod]
        public void BundlerConfiguration_Returns_Bundle_Path_With_JavaScript_Extension()
        {
            var path = _Config.RegisterJavascriptBundle("/folder/file.html", 0, new List<string>() { "source.js" });
            Assert.AreEqual(".js", Path.GetExtension(path));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BundlerConfiguration_Rejects_Null_HtmlPath()
        {
            _Config.RegisterJavascriptBundle(null, 0, new string[0]);
        }

        [TestMethod]
        public void BundlerConfiguration_Ignores_Relative_HtmlPaths()
        {
            AddContent("1", "a");
            AddContent("/1", "a");
            Assert.IsNull(_Config.RegisterJavascriptBundle("index.html", 0, new List<string>() { "1" }));
        }

        [TestMethod]
        public void BundlerConfiguration_Ignores_Double_Registrations()
        {
            AddContent("/1", "a");
            AddContent("/2", "b");

            var path1 = _Config.RegisterJavascriptBundle("/index.html", 0, new List<string>() { "1" });
            var path2 = _Config.RegisterJavascriptBundle("/index.html", 0, new List<string>() { "2" });

            _Environment.RequestPath = path1;
            var bundled = _Config.GetJavascriptBundle(path1, _Environment.Environment);

            Assert.AreEqual(path1, path2);

            Assert.AreEqual("/* /1 */\r\na\r\n", bundled);
        }

        [TestMethod]
        public void BundlerConfiguration_Ignores_Case()
        {
            AddContent("/1", "a");

            var path = _Config.RegisterJavascriptBundle("/index.html", 0, new List<string>() { "1" });

            _Environment.RequestPath = path;
            var bundled1 = _Config.GetJavascriptBundle(path.ToUpper(), _Environment.Environment);
            var bundled2 = _Config.GetJavascriptBundle(path.ToLower(), _Environment.Environment);

            Assert.AreEqual(bundled1, bundled2);
        }

        [TestMethod]
        public void BundlerConfiguration_Returns_Null_When_Bundle_Path_Is_Unknown()
        {
            _Environment.RequestPath = "/unknown.js";
            Assert.IsNull(_Config.GetJavascriptBundle("/unknown.js", _Environment.Environment));
        }

        [TestMethod]
        public void BundlerConfiguration_Will_Not_Return_Bundled_Content_When_Environment_Suppresses_It()
        {
            AddContent("/1", "a");
            var path = _Config.RegisterJavascriptBundle("/index.html", 0, new List<string>() { "1" });

            _Environment.RequestPath = path;
            _Environment.Environment.Add(EnvironmentKey.SuppressJavascriptBundles, true);

            Assert.IsNull(_Config.GetJavascriptBundle(path, _Environment.Environment));
        }

        [TestMethod]
        public void BundlerConfiguration_Returns_Bundled_Content_When_Suppress_Flag_Is_False()
        {
            AddContent("/1", "a");
            var path = _Config.RegisterJavascriptBundle("/index.html", 0, new List<string>() { "1" });

            _Environment.RequestPath = path;
            _Environment.Environment.Add(EnvironmentKey.SuppressJavascriptBundles, false);

            Assert.IsNotNull(_Config.GetJavascriptBundle(path, _Environment.Environment));
        }

        [TestMethod]
        public void BundlerConfiguration_Suppresses_Recursive_Bundling()
        {
            _LoopbackHost.Setup(r => r.SendSimpleRequest(It.IsAny<string>())).Returns((string r) => {
                _LoopbackHost.Object.ModifyEnvironmentAction(_Environment.Environment);
                Assert.AreEqual(true, (bool)_Environment.Environment[EnvironmentKey.SuppressJavascriptBundles]);

                return new SimpleContent() { Content = Encoding.UTF8.GetBytes("a"), HttpStatusCode = HttpStatusCode.OK, };
            });
            var path = _Config.RegisterJavascriptBundle("/index.html", 0, new List<string>() { "1" });
        }
    }
}

