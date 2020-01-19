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
using System.Net;
using System.Text;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.WebSite;

namespace Test.VirtualRadar.WebSite.MiddlewareConfiguration
{
    [TestClass]
    public class BundlerConfigurationTests
    {
        public TestContext TestContext { get; set; }

        private IClassFactory _Snapshot;
        private IBundlerConfiguration _Config;
        private MockOwinEnvironment _HtmlEnv;
        private MockOwinEnvironment _BndlEnv;
        private Mock<ILoopbackHost> _LoopbackHost;
        private Dictionary<string, string> _LoopbackContent;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _HtmlEnv = new MockOwinEnvironment();
            _BndlEnv = new MockOwinEnvironment();

            _LoopbackHost = TestUtilities.CreateMockImplementation<ILoopbackHost>();
            _LoopbackContent = new Dictionary<string, string>();
            _LoopbackHost.Setup(r => r.SendSimpleRequest(It.IsAny<string>(), _BndlEnv.Environment)).Returns((string path, IDictionary<string, object> env) => {
                var result = new SimpleContent();
                if(!_LoopbackContent.TryGetValue(path, out string content)) {
                    result.HttpStatusCode = HttpStatusCode.NotFound;
                } else {
                    result.Content = Encoding.UTF8.GetBytes(content);
                    result.HttpStatusCode = HttpStatusCode.OK;
                }

                return result;
            });

            _Config = Factory.ResolveNewInstance<IBundlerConfiguration>();
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
            _HtmlEnv.RequestPath = "/index.html";
            var path = _Config.RegisterJavascriptBundle(_HtmlEnv.Environment, 0, new List<string>() { "/bundled.js" });

            _BndlEnv.RequestPath = "/index-0-bundle.js";
            var bundled = _Config.GetJavascriptBundle(_BndlEnv.Environment);

            Assert.AreEqual("index-0-bundle.js", path);
            Assert.AreEqual("/* /bundled.js */\r\nabc\r\n", bundled);
        }

        [TestMethod]
        public void BundlerConfiguration_Replays_Two_Files_Joined_Together()
        {
            AddContent("/first.js", "abc");
            AddContent("/second.js", "xyz");
            _HtmlEnv.RequestPath = "/index.html";
            var path = _Config.RegisterJavascriptBundle(_HtmlEnv.Environment, 0, new List<string>() { "/first.js", "/second.js" });

            _BndlEnv.RequestPath = "/index-0-bundle.js";
            var bundled = _Config.GetJavascriptBundle(_BndlEnv.Environment);

            Assert.AreEqual("index-0-bundle.js", path);
            Assert.AreEqual("/* /first.js */\r\nabc\r\n;\r\n/* /second.js */\r\nxyz\r\n", bundled);
        }

        [TestMethod]
        public void BundlerConfiguration_Reports_Issues_With_Fetching_Source()
        {
            AddContent("/present.js", "abc");
            _HtmlEnv.RequestPath = "/index.html";
            var path = _Config.RegisterJavascriptBundle(_HtmlEnv.Environment, 0, new List<string>() { "/present.js", "/missing.js" });

            _BndlEnv.RequestPath = "/index-0-bundle.js";
            var bundled = _Config.GetJavascriptBundle(_BndlEnv.Environment);

            Assert.AreEqual("index-0-bundle.js", path);
            Assert.AreEqual("/* /present.js */\r\nabc\r\n;\r\n/* /missing.js */\r\n// Status 404 on /missing.js\r\n", bundled);
        }

        [TestMethod]
        public void BundlerConfiguration_Handles_Relative_Paths_On_JavaScript_Links()
        {
            AddContent("/folder/source.js", "abc");
            _HtmlEnv.RequestPath = "/folder/file.html";
            var path = _Config.RegisterJavascriptBundle(_HtmlEnv.Environment, 0, new List<string>() { "source.js" });

            _BndlEnv.RequestPath = "/folder/file-0-bundle.js";
            var bundled = _Config.GetJavascriptBundle(_BndlEnv.Environment);

            Assert.AreEqual("file-0-bundle.js", path);
            Assert.AreEqual("/* /folder/source.js */\r\nabc\r\n", bundled);
        }

        [TestMethod]
        public void BundlerConfiguration_Returns_Different_Paths_For_Different_Indexes()
        {
            _HtmlEnv.RequestPath = "/index.html";
            var path1 = _Config.RegisterJavascriptBundle(_HtmlEnv.Environment, 0, new List<string>() { "1", "2" });
            var path2 = _Config.RegisterJavascriptBundle(_HtmlEnv.Environment, 1, new List<string>() { "3", "4" });

            Assert.AreEqual("index-0-bundle.js", path1);
            Assert.AreEqual("index-1-bundle.js", path2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BundlerConfiguration_Rejects_Null_HtmlEnvironment()
        {
            _Config.RegisterJavascriptBundle(null, 0, new string[0]);
        }

        [TestMethod]
        public void BundlerConfiguration_Rejects_Paths_With_No_Extension()
        {
            AddContent("1", "a");
            _HtmlEnv.RequestPath = "/noext";
            Assert.IsNull(_Config.RegisterJavascriptBundle(_HtmlEnv.Environment, 0, new string[] { "1" }));
        }

        [TestMethod]
        public void BundlerConfiguration_Ignores_Environments_With_No_Request_Path()
        {
            AddContent("1", "a");
            AddContent("/1", "a");
            Assert.IsNull(_Config.RegisterJavascriptBundle(_HtmlEnv.Environment, 0, new List<string>() { "1" }));
        }

        [TestMethod]
        public void BundlerConfiguration_Ignores_Double_Registrations()
        {
            AddContent("/1", "a");
            AddContent("/2", "b");

            _HtmlEnv.RequestPath = "/index.html";
            var path1 = _Config.RegisterJavascriptBundle(_HtmlEnv.Environment, 0, new List<string>() { "1" });
            var path2 = _Config.RegisterJavascriptBundle(_HtmlEnv.Environment, 0, new List<string>() { "2" });

            _BndlEnv.RequestPath = "/index-0-bundle.js";
            var bundled = _Config.GetJavascriptBundle(_BndlEnv.Environment);

            Assert.AreEqual(path1, path2);
            Assert.AreEqual("index-0-bundle.js", path1);
            Assert.AreEqual("/* /1 */\r\na\r\n", bundled);
        }

        [TestMethod]
        public void BundlerConfiguration_Ignores_Case()
        {
            AddContent("/1", "a");

            _HtmlEnv.RequestPath = "/index.html";
            _Config.RegisterJavascriptBundle(_HtmlEnv.Environment, 0, new List<string>() { "1" });

            _BndlEnv.RequestPath = "/index-0-bundle.js".ToUpper();
            var bundled1 = _Config.GetJavascriptBundle(_BndlEnv.Environment);
            _BndlEnv.RequestPath = "/index-0-bundle.js".ToLower();
            var bundled2 = _Config.GetJavascriptBundle(_BndlEnv.Environment);

            Assert.IsNotNull(bundled1);
            Assert.AreEqual(bundled1, bundled2);
        }

        [TestMethod]
        public void BundlerConfiguration_Returns_Null_When_Bundle_Path_Is_Unknown()
        {
            _HtmlEnv.RequestPath = "/unknown.js";
            Assert.IsNull(_Config.GetJavascriptBundle(_BndlEnv.Environment));
        }

        [TestMethod]
        public void BundlerConfiguration_Will_Not_Return_Bundled_Content_When_Environment_Suppresses_It()
        {
            AddContent("/1", "a");
            _HtmlEnv.RequestPath = "/index.html";
            _Config.RegisterJavascriptBundle(_HtmlEnv.Environment, 0, new List<string>() { "1" });

            _BndlEnv.RequestPath = "/index-0-bundle.js";
            _BndlEnv.Environment.Add(VrsEnvironmentKey.SuppressJavascriptBundles, true);

            Assert.IsNull(_Config.GetJavascriptBundle(_BndlEnv.Environment));
        }

        [TestMethod]
        public void BundlerConfiguration_Returns_Bundled_Content_When_Suppress_Flag_Is_False()
        {
            AddContent("/1", "a");
            _HtmlEnv.RequestPath = "/index.html";
            _Config.RegisterJavascriptBundle(_HtmlEnv.Environment, 0, new List<string>() { "1" });

            _BndlEnv.RequestPath = "/index-0-bundle.js";
            _BndlEnv.Environment.Add(VrsEnvironmentKey.SuppressJavascriptBundles, false);

            Assert.IsNotNull(_Config.GetJavascriptBundle(_BndlEnv.Environment));
        }

        [TestMethod]
        public void BundlerConfiguration_Suppresses_Recursive_Bundling()
        {
            _LoopbackHost.Setup(r => r.SendSimpleRequest(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>())).Returns((string r, IDictionary<string, object> env) => {
                Assert.AreSame(_BndlEnv.Environment, env);
                _LoopbackHost.Object.ModifyEnvironmentAction(_BndlEnv.Environment);
                Assert.AreEqual(true, (bool)_BndlEnv.Environment[VrsEnvironmentKey.SuppressJavascriptBundles]);

                return new SimpleContent() { Content = Encoding.UTF8.GetBytes("a"), HttpStatusCode = HttpStatusCode.OK, };
            });

            _HtmlEnv.RequestPath = "/index.html";
            _Config.RegisterJavascriptBundle(_HtmlEnv.Environment, 0, new List<string>() { "1" });

            _BndlEnv.RequestPath = "/index-0-bundle.js";
            _Config.GetJavascriptBundle(_BndlEnv.Environment);
        }

        [TestMethod]
        public void BundlerConfiguration_Loopback_Fetches_Are_Flattened_Before_Dispatch()
        {
            AddContent("/script/x.js", "Hello");

            _HtmlEnv.RequestPath = "/Folder/SubFolder/index.html";
            _Config.RegisterJavascriptBundle(_HtmlEnv.Environment, 0, new List<string>() { "../../script/x.js" });
            _BndlEnv.RequestPath = "/Folder/SubFolder/index-0-bundle.js";
            var bundle = _Config.GetJavascriptBundle(_BndlEnv.Environment);

            Assert.AreEqual("/* /script/x.js */\r\nHello\r\n", bundle);
        }

        [TestMethod]
        public void BundlerConfiguration_Loopback_Fetches_Do_Not_Suppress_Minification()
        {
            _LoopbackHost.Setup(r => r.SendSimpleRequest(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>())).Returns((string r, IDictionary<string, object> env) => {
                Assert.AreSame(_BndlEnv.Environment, env);
                _LoopbackHost.Object.ModifyEnvironmentAction(_BndlEnv.Environment);
                Assert.IsFalse(_BndlEnv.Environment.ContainsKey(VrsEnvironmentKey.SuppressJavascriptMinification));

                return new SimpleContent() { Content = Encoding.UTF8.GetBytes("a"), HttpStatusCode = HttpStatusCode.OK, };
            });
            _HtmlEnv.RequestPath = "/index.html";
            _Config.RegisterJavascriptBundle(_HtmlEnv.Environment, 0, new List<string>() { "1" });

            _BndlEnv.RequestPath = "/index-0-bundle.js";
            _Config.GetJavascriptBundle(_BndlEnv.Environment);
        }
    }
}

