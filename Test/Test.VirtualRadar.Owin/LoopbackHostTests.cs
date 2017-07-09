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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Owin;
using Test.Framework;
using VirtualRadar.Interface.Owin;

namespace Test.VirtualRadar.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    [TestClass]
    public class LoopbackHostTests
    {
        public TestContext TestContext { get; set; }
        private IClassFactory _Snapshot;

        private ILoopbackHost _LoopbackHost;
        private Mock<IStandardPipeline> _StandardPipeline;
        private IWebAppConfiguration _WebAppConfiguration;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _StandardPipeline = TestUtilities.CreateMockImplementation<IStandardPipeline>();
            _WebAppConfiguration = Factory.Singleton.Resolve<IWebAppConfiguration>();

            _LoopbackHost = Factory.Singleton.Resolve<ILoopbackHost>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        [TestMethod]
        public void LoopbackHost_ConfigureStandardPipeline_Adds_Standard_Middleware()
        {
            var mockWebAppConfig = TestUtilities.CreateMockImplementation<IWebAppConfiguration>();

            _LoopbackHost.ConfigureStandardPipeline();

            _StandardPipeline.Verify(r => r.Register(mockWebAppConfig.Object), Times.Once());
            mockWebAppConfig.Verify(r => r.Configure(It.IsAny<IAppBuilder>()), Times.Once());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LoopbackHost_ConfigureStandardPipeline_Throws_If_Already_Configured()
        {
            _LoopbackHost.ConfigureStandardPipeline();
            _LoopbackHost.ConfigureStandardPipeline();
        }

        [TestMethod]
        public void LoopbackHost_ConfigureCustomPipeline_Builds_IApp_From_Configuration()
        {
            var mockWebAppConfig = TestUtilities.CreateMockImplementation<IWebAppConfiguration>();

            _LoopbackHost.ConfigureCustomPipeline(mockWebAppConfig.Object);

            _StandardPipeline.Verify(r => r.Register(It.IsAny<IWebAppConfiguration>()), Times.Never());
            mockWebAppConfig.Verify(r => r.Configure(It.IsAny<IAppBuilder>()), Times.Once());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LoopbackHost_ConfigureCustomPipeline_Throws_If_Already_Configured()
        {
            _LoopbackHost.ConfigureCustomPipeline(_WebAppConfiguration);
            _LoopbackHost.ConfigureCustomPipeline(_WebAppConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LoopbackHost_SendSimpleRequest_Throws_If_Host_Not_Configured()
        {
            _LoopbackHost.SendSimpleRequest("/file.txt");
        }

        private void AddCallback(Func<IDictionary<string, object>, bool> callNextMiddleware)
        {
            _WebAppConfiguration.AddCallback((IAppBuilder app) => {
                var middleware = new Func<AppFunc, AppFunc>((AppFunc next) => {
                    AppFunc result = async(IDictionary<string, object> env) => {
                        if(callNextMiddleware(env)) {
                            await next.Invoke(env);
                        }
                    };
                    return result;
                });
                app.Use(middleware);
            }, 0);
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Uses_Owin_Compliant_Environment()
        {
            IDictionary<string, object> env = null;
            AddCallback((IDictionary<string, object> environment) => {
                env = environment;
                return true;
            });

            _LoopbackHost.ConfigureCustomPipeline(_WebAppConfiguration);
            _LoopbackHost.SendSimpleRequest("/file.txt?a=1&amp;b=2");

            Assert.IsNotNull(env);

            // Mandatory context elements
            Assert.AreEqual("1.0.0", env["owin.version"]);
            Assert.IsTrue(env["owin.callcancelled"] is CancellationToken);

            // Mandatory request elements
            Assert.AreSame(Stream.Null, env["owin.requestbody"]);
            Assert.IsTrue(env["owin.requestheaders"] is IDictionary<string, string[]>);
            Assert.AreSame(StringComparer.OrdinalIgnoreCase, ((Dictionary<string, string[]>)env["owin.requestheaders"]).Comparer);  // Hmm...
            Assert.AreEqual("GET", env["owin.requestmethod"]);
            Assert.AreEqual("HTTP/1.1", env["owin.requestprotocol"]);
            Assert.AreEqual("http", env["owin.requestscheme"]);
            Assert.AreEqual("/VirtualRadar", env["owin.requestpathbase"]);
            Assert.AreEqual("/file.txt", env["owin.requestpath"]);
            Assert.AreEqual("a=1&amp;b=2", env["owin.requestquerystring"]);

            // Mandatory response elements
            Assert.IsTrue(env["owin.responsebody"] is Stream);
            Assert.IsTrue(env["owin.responseheaders"] is IDictionary<string, string[]>);
            Assert.AreSame(StringComparer.OrdinalIgnoreCase, ((Dictionary<string, string[]>)env["owin.responseheaders"]).Comparer);  // Hmm again...
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Forces_Leading_Slash_On_PathAndFile()
        {
            throw new NotImplementedException();        // "file.txt" should have a path of "/file.txt", "" should have a path of "/" etc.
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Uses_Empty_String_When_Request_Has_No_Query_String()
        {
            IDictionary<string, object> env = null;
            AddCallback((IDictionary<string, object> environment) => {
                env = environment;
                return true;
            });
            _LoopbackHost.ConfigureCustomPipeline(_WebAppConfiguration);

            _LoopbackHost.SendSimpleRequest("/file.txt");
            Assert.AreEqual("/file.txt", env["owin.requestpath"]);
            Assert.AreEqual("", env["owin.requestquerystring"]);
        }
    }
}
