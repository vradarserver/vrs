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
        private IDictionary<string, object> _LoopbackEnvironment;
        private MockOwinEnvironment _Environment;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _StandardPipeline = TestUtilities.CreateMockImplementation<IStandardPipeline>();
            _WebAppConfiguration = Factory.Singleton.Resolve<IWebAppConfiguration>();
            _LoopbackEnvironment = null;
            _Environment = new MockOwinEnvironment();

            _LoopbackHost = Factory.Singleton.Resolve<ILoopbackHost>();
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

        private void RecordEnvironment()
        {
            AddCallback((IDictionary<string, object> environment) => {
                _LoopbackEnvironment = environment;
                return true;
            });
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

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Uses_Owin_Compliant_Environment()
        {
            RecordEnvironment();

            _LoopbackHost.ConfigureCustomPipeline(_WebAppConfiguration);
            _LoopbackHost.SendSimpleRequest("/file.txt?a=1&amp;b=2");

            Assert.IsNotNull(_LoopbackEnvironment);
            var env = _LoopbackEnvironment;

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

            // Other request elements
            Assert.AreEqual("127.0.0.1", env["server.RemoteIpAddress"]);

            // Mandatory response elements
            Assert.IsTrue(env["owin.responsebody"] is Stream);
            Assert.IsTrue(env["owin.responseheaders"] is IDictionary<string, string[]>);
            Assert.AreSame(StringComparer.OrdinalIgnoreCase, ((Dictionary<string, string[]>)env["owin.responseheaders"]).Comparer);  // Hmm again...
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Calls_ModifyEnvironmentAction_Before_Sending_Request()
        {
            RecordEnvironment();
            _LoopbackHost.ConfigureCustomPipeline(_WebAppConfiguration);
            _LoopbackHost.ModifyEnvironmentAction += environment => {
                environment.Add("customKey", "customValue");
            };

            _LoopbackHost.SendSimpleRequest("/file.txt");

            Assert.AreEqual("customValue", _LoopbackEnvironment["customKey"]);
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Forces_Leading_Slash_On_PathAndFile()
        {
            RecordEnvironment();
            _LoopbackHost.ConfigureCustomPipeline(_WebAppConfiguration);

            _LoopbackHost.SendSimpleRequest("file.txt");
            Assert.AreEqual("/file.txt", _LoopbackEnvironment["owin.requestpath"]);
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Flattens_PathAndFile()
        {
            RecordEnvironment();
            _LoopbackHost.ConfigureCustomPipeline(_WebAppConfiguration);

            _LoopbackHost.SendSimpleRequest("/folder/subfolder/../othersub/minor/.././../file.txt");
            Assert.AreEqual("/folder/file.txt", _LoopbackEnvironment["owin.requestpath"]);
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Sets_Correct_PathAndFile_For_Empty_String()
        {
            RecordEnvironment();
            _LoopbackHost.ConfigureCustomPipeline(_WebAppConfiguration);

            _LoopbackHost.SendSimpleRequest("");
            Assert.AreEqual("/", _LoopbackEnvironment["owin.requestpath"]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LoopbackHost_SendSimpleRequest_Rejects_Requests_For_Null_PathAndFile()
        {
            _LoopbackHost.ConfigureCustomPipeline(_WebAppConfiguration);
            _LoopbackHost.SendSimpleRequest(null);
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Uses_Empty_String_When_Request_Has_No_Query_String()
        {
            RecordEnvironment();
            _LoopbackHost.ConfigureCustomPipeline(_WebAppConfiguration);

            _LoopbackHost.SendSimpleRequest("/file.txt");
            Assert.AreEqual("/file.txt", _LoopbackEnvironment["owin.requestpath"]);
            Assert.AreEqual("", _LoopbackEnvironment["owin.requestquerystring"]);
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Returns_Status_To_Caller()
        {
            AddCallback((IDictionary<string, object> environment) => {
                var context = PipelineContext.GetOrCreate(environment);
                context.Response.StatusCode = 123;
                return false;
            });
            _LoopbackHost.ConfigureCustomPipeline(_WebAppConfiguration);

            var result = _LoopbackHost.SendSimpleRequest("/test");

            Assert.AreEqual(123, (int)result.HttpStatusCode);
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Returns_Response_Stream_Content_To_Caller()
        {
            var bodyBytes = Encoding.UTF8.GetBytes("Up at the Lake");
            AddCallback((IDictionary<string, object> environment) => {
                var context = PipelineContext.GetOrCreate(environment);
                context.Response.Body.Write(bodyBytes, 0, bodyBytes.Length);
                return false;
            });
            _LoopbackHost.ConfigureCustomPipeline(_WebAppConfiguration);

            var result = _LoopbackHost.SendSimpleRequest("/test");

            Assert.IsTrue(bodyBytes.SequenceEqual(result.Content));
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Returns_Empty_Content_Array_When_Response_Has_No_Body()
        {
            AddCallback((IDictionary<string, object> environment) => {
                return false;
            });
            _LoopbackHost.ConfigureCustomPipeline(_WebAppConfiguration);

            var result = _LoopbackHost.SendSimpleRequest("/test");

            Assert.AreEqual(0, result.Content.Length);
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_UserAgent_Defaults_As_Per_Docs()
        {
            RecordEnvironment();
            _LoopbackHost.ConfigureCustomPipeline(_WebAppConfiguration);

            _LoopbackHost.SendSimpleRequest("/");

            var context = PipelineContext.GetOrCreate(_LoopbackEnvironment);
            Assert.AreEqual("FAKE REQUEST", context.Request.UserAgent);
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Cookies_Defaulted_As_Per_Docs()
        {
            RecordEnvironment();
            _LoopbackHost.ConfigureCustomPipeline(_WebAppConfiguration);

            _LoopbackHost.SendSimpleRequest("/");

            var context = PipelineContext.GetOrCreate(_LoopbackEnvironment);
            Assert.AreEqual(0, context.Request.Cookies.Count());
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Copies_Headers_From_Environment()
        {
            RecordEnvironment();
            _LoopbackHost.ConfigureCustomPipeline(_WebAppConfiguration);

            _Environment.Request.Headers["Authorization"] = "Basic Hello&=1;2";
            _Environment.Request.Headers["Cookie"] = "abc=123";
            _Environment.Request.Headers["Custom"] = "foo;bar";
            _LoopbackHost.SendSimpleRequest("/", _Environment.Environment);

            var context = PipelineContext.GetOrCreate(_LoopbackEnvironment);
            Assert.AreEqual("Basic Hello&=1;2", context.Request.Headers["Authorization"]);
            Assert.AreEqual("abc=123", context.Request.Headers["Cookie"]);
            Assert.AreEqual("foo;bar", context.Request.Headers["Custom"]);
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Copes_When_Environment_Supplies_Headers_That_Are_Defaulted()
        {
            RecordEnvironment();
            _LoopbackHost.ConfigureCustomPipeline(_WebAppConfiguration);

            _Environment.Request.UserAgent = "My User Agent";
            _LoopbackHost.SendSimpleRequest("/", _Environment.Environment);

            var context = PipelineContext.GetOrCreate(_LoopbackEnvironment);
            Assert.AreEqual("My User Agent", context.Request.UserAgent);
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Copies_Client_IP_Address_From_Environment()
        {
            RecordEnvironment();
            _LoopbackHost.ConfigureCustomPipeline(_WebAppConfiguration);

            _Environment.Request.RemoteIpAddress = "1.2.3.4";
            _Environment.Request.RemotePort = 54321;
            _LoopbackHost.SendSimpleRequest("/", _Environment.Environment);

            var context = PipelineContext.GetOrCreate(_LoopbackEnvironment);
            Assert.AreEqual("1.2.3.4", context.Request.RemoteIpAddress);
            Assert.AreEqual(54321, context.Request.RemotePort);
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Sets_IsLoopbackRequest_Environment_Key()
        {
            RecordEnvironment();
            _LoopbackHost.ConfigureCustomPipeline(_WebAppConfiguration);

            _LoopbackHost.SendSimpleRequest("/");

            Assert.IsTrue((bool)_LoopbackEnvironment[EnvironmentKey.IsLoopbackRequest]);
        }
    }
}
