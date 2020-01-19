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
using System.Threading;
using AWhewell.Owin.Interface;
using AWhewell.Owin.Utility;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.WebSite;

namespace Test.VirtualRadar.WebSite
{
    [TestClass]
    public class LoopbackHostTests
    {
        public TestContext TestContext { get; set; }
        private IClassFactory _Snapshot;

        private ILoopbackHost _LoopbackHost;
        private Mock<IWebSitePipelineBuilder> _WebSitePipelineBuilder;
        private Mock<IPipelineBuilder> _StandardPipelineBuilder;
        private Mock<IPipelineBuilder> _CustomPipelineBuilder;
        private Mock<AWhewell.Owin.Interface.IPipeline> _Pipeline;
        private Mock<IPipelineBuilderEnvironment> _PipelineBuilderEnvironment;
        private IDictionary<string, object> _LoopbackEnvironment;
        private MockOwinEnvironment _Environment;
        private Action<IDictionary<string, object>> _ProcessRequestAction;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _Pipeline = TestUtilities.CreateMockImplementation<AWhewell.Owin.Interface.IPipeline>();
            _ProcessRequestAction = null;
            _Pipeline
                .Setup(r => r.ProcessRequest(It.IsAny<IDictionary<string, object>>()))
                .Callback((IDictionary<string, object> environment) => {
                    _LoopbackEnvironment = environment;
                    _ProcessRequestAction?.Invoke(environment);
                });

            _PipelineBuilderEnvironment = TestUtilities.CreateMockImplementation<IPipelineBuilderEnvironment>();

            // Standard pipeline builder
            _WebSitePipelineBuilder = TestUtilities.CreateMockImplementation<IWebSitePipelineBuilder>();
            _StandardPipelineBuilder = TestUtilities.CreateMockImplementation<IPipelineBuilder>();
            _WebSitePipelineBuilder.SetupGet(r => r.PipelineBuilder).Returns(_StandardPipelineBuilder.Object);

            // Custom pipeline builder
            _CustomPipelineBuilder = TestUtilities.CreateMockImplementation<IPipelineBuilder>();

            _StandardPipelineBuilder.Setup(r => r.CreatePipeline(It.IsAny<IPipelineBuilderEnvironment>())).Returns(() => _Pipeline.Object);
            _CustomPipelineBuilder.Setup  (r => r.CreatePipeline(It.IsAny<IPipelineBuilderEnvironment>())).Returns(() => _Pipeline.Object);

            _LoopbackEnvironment = null;
            _Environment = new MockOwinEnvironment();

            _LoopbackHost = Factory.Resolve<ILoopbackHost>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        [TestMethod]
        public void LoopbackHost_ConfigureStandardPipeline_Uses_Web_Site_Pipeline()
        {
            _LoopbackHost.ConfigureStandardPipeline();

            _StandardPipelineBuilder.Verify(r => r.CreatePipeline(It.IsAny<IPipelineBuilderEnvironment>()), Times.Once());
            _CustomPipelineBuilder.Verify(r => r.CreatePipeline(It.IsAny<IPipelineBuilderEnvironment>()), Times.Never());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LoopbackHost_ConfigureStandardPipeline_Throws_If_Already_Configured()
        {
            _LoopbackHost.ConfigureStandardPipeline();
            _LoopbackHost.ConfigureStandardPipeline();
        }

        public void LoopbackHost_ConfigureCustomPipeline_Builds_Pipeline_From_Builder_Supplied()
        {
            _LoopbackHost.ConfigureCustomPipeline(_CustomPipelineBuilder.Object);

            _CustomPipelineBuilder.Verify(r => r.CreatePipeline(It.IsAny<IPipelineBuilderEnvironment>()), Times.Once());
            _StandardPipelineBuilder.Verify(r => r.CreatePipeline(It.IsAny<IPipelineBuilderEnvironment>()), Times.Never());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LoopbackHost_ConfigureCustomPipeline_Throws_If_Already_Configured()
        {
            _LoopbackHost.ConfigureCustomPipeline(_CustomPipelineBuilder.Object);
            _LoopbackHost.ConfigureCustomPipeline(_CustomPipelineBuilder.Object);
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
            _LoopbackHost.ConfigureStandardPipeline();
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
            _LoopbackHost.ConfigureStandardPipeline();
            _LoopbackHost.ModifyEnvironmentAction += environment => {
                environment.Add("customKey", "customValue");
            };

            _LoopbackHost.SendSimpleRequest("/file.txt");

            Assert.AreEqual("customValue", _LoopbackEnvironment["customKey"]);
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Forces_Leading_Slash_On_PathAndFile()
        {
            _LoopbackHost.ConfigureStandardPipeline();

            _LoopbackHost.SendSimpleRequest("file.txt");
            Assert.AreEqual("/file.txt", _LoopbackEnvironment["owin.requestpath"]);
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Flattens_PathAndFile()
        {
            _LoopbackHost.ConfigureStandardPipeline();

            _LoopbackHost.SendSimpleRequest("/folder/subfolder/../othersub/minor/.././../file.txt");
            Assert.AreEqual("/folder/file.txt", _LoopbackEnvironment["owin.requestpath"]);
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Sets_Correct_PathAndFile_For_Empty_String()
        {
            _LoopbackHost.ConfigureStandardPipeline();

            _LoopbackHost.SendSimpleRequest("");
            Assert.AreEqual("/", _LoopbackEnvironment["owin.requestpath"]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LoopbackHost_SendSimpleRequest_Rejects_Requests_For_Null_PathAndFile()
        {
            _LoopbackHost.ConfigureStandardPipeline();
            _LoopbackHost.SendSimpleRequest(null);
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Uses_Empty_String_When_Request_Has_No_Query_String()
        {
            _LoopbackHost.ConfigureStandardPipeline();

            _LoopbackHost.SendSimpleRequest("/file.txt");
            Assert.AreEqual("/file.txt", _LoopbackEnvironment["owin.requestpath"]);
            Assert.AreEqual("", _LoopbackEnvironment["owin.requestquerystring"]);
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Returns_Status_To_Caller()
        {
            _ProcessRequestAction = (env) => {
                var context = OwinContext.Create(env);
                context.ResponseStatusCode = 123;
            };
            _LoopbackHost.ConfigureStandardPipeline();

            var result = _LoopbackHost.SendSimpleRequest("/test");

            Assert.AreEqual(123, (int)result.HttpStatusCode);
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Returns_Response_Stream_Content_To_Caller()
        {
            var bodyBytes = Encoding.UTF8.GetBytes("Up at the Lake");
            _ProcessRequestAction = (env) => {
                var context = OwinContext.Create(env);
                context.ResponseBody.Write(bodyBytes, 0, bodyBytes.Length);
            };
            _LoopbackHost.ConfigureStandardPipeline();

            var result = _LoopbackHost.SendSimpleRequest("/test");

            Assert.IsTrue(bodyBytes.SequenceEqual(result.Content));
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Returns_Empty_Content_Array_When_Response_Has_No_Body()
        {
            _LoopbackHost.ConfigureStandardPipeline();

            var result = _LoopbackHost.SendSimpleRequest("/test");

            Assert.AreEqual(0, result.Content.Length);
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_UserAgent_Defaults_As_Per_Docs()
        {
            _LoopbackHost.ConfigureStandardPipeline();

            _LoopbackHost.SendSimpleRequest("/");

            var context = OwinContext.Create(_LoopbackEnvironment);
            Assert.AreEqual("FAKE REQUEST", context.RequestHeadersDictionary.UserAgent);
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Copies_Headers_From_Environment()
        {
            _LoopbackHost.ConfigureStandardPipeline();

            _Environment.RequestHeaders["Authorization"] = "Basic Hello&=1;2";
            _Environment.RequestHeaders["Cookie"] = "abc=123";
            _Environment.RequestHeaders["Custom"] = "foo;bar";
            _LoopbackHost.SendSimpleRequest("/", _Environment.Environment);

            var context = OwinContext.Create(_LoopbackEnvironment);
            Assert.AreEqual("Basic Hello&=1;2", context.RequestHeadersDictionary["Authorization"]);
            Assert.AreEqual("abc=123", context.RequestHeadersDictionary["Cookie"]);
            Assert.AreEqual("foo;bar", context.RequestHeadersDictionary["Custom"]);
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Copes_When_Environment_Supplies_Headers_That_Are_Defaulted()
        {
            _LoopbackHost.ConfigureStandardPipeline();

            _Environment.RequestHeaders["User-Agent"] = "My User Agent";
            _LoopbackHost.SendSimpleRequest("/", _Environment.Environment);

            var context = OwinContext.Create(_LoopbackEnvironment);
            Assert.AreEqual("My User Agent", context.RequestHeadersDictionary.UserAgent);
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Copies_Client_IP_Address_From_Environment()
        {
            _LoopbackHost.ConfigureStandardPipeline();

            _Environment.ServerRemoteIpAddress = "1.2.3.4";
            _Environment.ServerRemotePort = 54321;
            _LoopbackHost.SendSimpleRequest("/", _Environment.Environment);

            var context = OwinContext.Create(_LoopbackEnvironment);
            Assert.AreEqual("1.2.3.4", context.ServerRemoteIpAddress);
            Assert.AreEqual(54321, context.ServerRemotePortNumber);
        }

        [TestMethod]
        public void LoopbackHost_SendSimpleRequest_Sets_IsLoopbackRequest_Environment_Key()
        {
            _LoopbackHost.ConfigureStandardPipeline();

            _LoopbackHost.SendSimpleRequest("/");

            Assert.IsTrue((bool)_LoopbackEnvironment[VrsEnvironmentKey.IsLoopbackRequest]);
        }
    }
}
