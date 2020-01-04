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
using System.Linq;
using System.Text;
using AWhewell.Owin.Utility;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.WebServer;

namespace Test.VirtualRadar.Owin.Middleware
{
    [TestClass]
    public class BundlerServerTests
    {
        public TestContext TestContext { get; set; }
        private IClassFactory _Snapshot;

        private IBundlerServer _Server;
        private Mock<IBundlerConfiguration> _Configuration;
        private MockOwinEnvironment _Environment;
        private MockOwinPipeline _Pipeline;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _Configuration = TestUtilities.CreateMockImplementation<IBundlerConfiguration>();
            _Server = Factory.Resolve<IBundlerServer>();

            _Environment = new MockOwinEnvironment();
            _Pipeline = new MockOwinPipeline();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        private void AssertBundleReturned(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);

            Assert.AreEqual(bytes.Length, _Environment.ResponseHeaders.ContentLength);
            Assert.AreEqual(MimeType.Javascript, _Environment.ResponseHeaders.ContentType);
            Assert.IsTrue(bytes.SequenceEqual(_Environment.ResponseBodyBytes));
            Assert.AreEqual(200, _Environment.ResponseStatusCode);
            Assert.IsFalse(_Pipeline.NextMiddlewareCalled);
        }

        private void AssertBundleNotReturned()
        {
            Assert.AreEqual(null, _Environment.ResponseHeaders.ContentLength);
            Assert.AreEqual(null, _Environment.ResponseHeaders.ContentType);
            Assert.AreEqual(0, _Environment.ResponseBodyBytes.Length);
            Assert.IsTrue(_Pipeline.NextMiddlewareCalled);
        }

        private void RegisterBundle(string pathFromRoot = "/bundle.js", string bundleContent = "hello")
        {
            _Configuration
                .Setup(r => r.GetJavascriptBundle(It.IsAny<IDictionary<string, object>>()))
                .Returns((IDictionary<string, object> env) => {
                    var context = OwinContext.Create(env);
                    return context.RequestPathFlattened == pathFromRoot ? bundleContent : null;
                }
            );
        }

        [TestMethod]
        public void BundlerServer_Responds_To_Requests_For_Bundles()
        {
            RegisterBundle();

            _Environment.RequestPath = "/bundle.js";
            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            AssertBundleReturned("hello");
        }

        [TestMethod]
        public void BundlerServer_Calls_Next_Middleware_If_Not_A_Bundle_Request()
        {
            RegisterBundle();

            _Environment.RequestPath = "/not-bundle.js";
            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            AssertBundleNotReturned();
        }

        [TestMethod]
        public void BundlerServer_Suppresses_Minification_Of_The_Bundle()
        {
            RegisterBundle();

            _Environment.RequestPath = "/bundle.js";
            _Pipeline.CallMiddleware(_Server.HandleRequest, _Environment.Environment);

            Assert.AreEqual(true, (bool)_Environment.Environment[VrsEnvironmentKey.SuppressJavascriptMinification]);
        }
    }
}
