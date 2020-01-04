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
using System.Linq;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.Owin.Middleware
{
    [TestClass]
    public class CorsHandlerTests
    {
        public TestContext TestContext { get; set; }
        private IClassFactory _Snapshot;

        private ICorsHandler _Handler;
        private MockOwinEnvironment _Environment;
        private MockOwinPipeline _Pipeline;
        private global::VirtualRadar.Interface.Settings.Configuration _Configuration;
        private Mock<ISharedConfiguration> _SharedConfiguration;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _Configuration = new global::VirtualRadar.Interface.Settings.Configuration();
            _SharedConfiguration = TestUtilities.CreateMockSingleton<ISharedConfiguration>();
            _SharedConfiguration.Setup(r => r.Get()).Returns(_Configuration);
            _SharedConfiguration.Setup(r => r.GetConfigurationChangedUtc()).Returns(DateTime.UtcNow);

            _Handler = Factory.Resolve<ICorsHandler>();
            _Environment = new MockOwinEnvironment();
            _Pipeline = new MockOwinPipeline();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        private void ConfigureCorsSupport(params string[] allowedDomains)
        {
            _Configuration.GoogleMapSettings.EnableCorsSupport = true;
            _Configuration.GoogleMapSettings.AllowCorsDomains = String.Join(";", allowedDomains);
        }

        private void ConfigurePreflightRequest(string origin, string requestPath = "/", string requestMethod = null, string[] requestHeaders = null)
        {
            _Environment.RequestMethod = "OPTIONS";
            _Environment.RequestPath = requestPath;
            _Environment.RequestHeaders["Origin"] = origin;

            if(requestMethod != "") {
                _Environment.RequestHeaders["Access-Control-Request-Method"] = requestMethod ?? "POST";
            }

            if(requestHeaders != null) {
                _Environment.RequestHeaders.SetValues("Access-Control-Request-Headers", requestHeaders);
            }
        }

        private void AssertPreflightAccepted(string allowedOrigin, string[] additionalMethods = null)
        {
            var allowOrigin = _Environment.ResponseHeaders["Access-Control-Allow-Origin"];
            Assert.AreEqual(allowedOrigin, allowOrigin);

            var allowMethods = _Environment.ResponseHeaders.GetCommaSeparatedValues("Access-Control-Allow-Methods");
            Assert.AreEqual(3 + (additionalMethods?.Length ?? 0), allowMethods.Count);
            foreach(var method in new string[] { "POST", "GET", "OPTIONS", }.Concat(additionalMethods ?? new string[0])) {
                Assert.AreNotEqual(-1, allowMethods.Contains(method, StringComparer.OrdinalIgnoreCase));
            }

            Assert.IsFalse(_Pipeline.NextMiddlewareCalled);
            Assert.AreEqual(200, _Environment.ResponseStatusCode);
        }

        private void AssertPreflightRejected()
        {
            Assert.IsFalse(_Pipeline.NextMiddlewareCalled);
            Assert.AreEqual(0, _Environment.ResponseHeaders.Count);
            Assert.AreEqual(403, _Environment.ResponseStatusCode);
        }

        private void ConfigureSimpleRequest(string origin, string path = "/", string method = null, string[] headers = null)
        {
            _Environment.RequestMethod = method ?? "GET";
            _Environment.RequestPath = path;
            _Environment.RequestHeaders["Origin"] = origin;
        }

        private void AssertSimpleAccepted(string allowedOrigin)
        {
            var allowOrigin = _Environment.ResponseHeaders["Access-Control-Allow-Origin"];
            Assert.AreEqual(allowedOrigin, allowOrigin);

            Assert.IsTrue(_Pipeline.NextMiddlewareCalled);
            Assert.IsTrue(
                   _Environment.Context.ResponseStatusCode == null  //  null and 200 are equivalent, if the status code remains
                || _Environment.Context.ResponseStatusCode == 200   //  at zero then eventually the runtime will set it to 200
            );
        }

        private void AssertSimpleRejected()
        {
            Assert.IsTrue(_Pipeline.NextMiddlewareCalled);
            Assert.AreEqual(0, _Environment.ResponseHeaders.Count);
            Assert.AreEqual(200, _Environment.ResponseStatusCode);
        }

        #region Preflight
        [TestMethod]
        public void CorsHandler_Preflight_Handles_OPTIONS_Request_From_Valid_Origin()
        {
            ConfigureCorsSupport("*");
            ConfigurePreflightRequest("http://www.allowed.com");

            _Pipeline.CallMiddleware(_Handler.HandleRequest, _Environment.Environment);

            AssertPreflightAccepted("http://www.allowed.com");
        }

        [TestMethod]
        public void CorsHandler_Preflight_Ignores_Requests_When_Disabled()
        {
            ConfigureCorsSupport("*");
            _Configuration.GoogleMapSettings.EnableCorsSupport = false;

            ConfigurePreflightRequest("http://www.allowed.com");

            _Pipeline.CallMiddleware(_Handler.HandleRequest, _Environment.Environment);

            AssertPreflightRejected();
        }

        [TestMethod]
        public void Corshandler_Preflight_Picks_Up_Configuration_Changes()
        {
            ConfigurePreflightRequest("http://www.allowed.com");
            ConfigureCorsSupport("*");

            _Configuration.GoogleMapSettings.EnableCorsSupport = false;
            _Pipeline.CallMiddleware(_Handler.HandleRequest, _Environment.Environment);
            AssertPreflightRejected();

            _SharedConfiguration.Setup(r => r.GetConfigurationChangedUtc()).Returns(DateTime.UtcNow.AddSeconds(1));
            _Configuration.GoogleMapSettings.EnableCorsSupport = true;
            _Pipeline.CallMiddleware(_Handler.HandleRequest, _Environment.Environment);
            AssertPreflightAccepted("http://www.allowed.com");
        }

        [TestMethod]
        public void CorsHandler_Preflight_Ignores_All_Methods_Except_Options()
        {
            foreach(var method in new string[] { "DELETE", "METHOD", "PATCH", "PUT", "TRACE" }) {
                TestCleanup();
                TestInitialise();

                ConfigureCorsSupport("*");
                ConfigurePreflightRequest("http://www.allowed.com");
                _Environment.RequestMethod = method;

                _Pipeline.CallMiddleware(_Handler.HandleRequest, _Environment.Environment);

                Assert.IsTrue(_Pipeline.NextMiddlewareCalled);
                var allowMethods = _Environment.ResponseHeaders["Access-Control-Allow-Methods"];
                Assert.IsNull(allowMethods, $"Failed for {method}");
            }
        }

        [TestMethod]
        public void CorsHandler_Preflight_Ignores_Unknown_Origins()
        {
            foreach(var origin in new string[] { "http://not-ok.domain.com", "http://ok.other.com", "https://ok.domain.com", }) {
                TestCleanup();
                TestInitialise();

                ConfigureCorsSupport("http://ok.domain.com", "https://good.other.com");
                ConfigurePreflightRequest(origin);

                _Pipeline.CallMiddleware(_Handler.HandleRequest, _Environment.Environment);

                AssertPreflightRejected();
            }
        }

        [TestMethod]
        public void CorsHandler_Preflight_Handles_Request_Method_Correctly()
        {
            foreach(var method in new string[] { "DELETE", "GET", "HEAD", "METHOD", "OPTIONS", "PATCH", "POST", "PUT", "TRACE", }) {
                TestCleanup();
                TestInitialise();

                ConfigureCorsSupport("*");
                ConfigurePreflightRequest("http://www.allowed.com", requestMethod: method);

                _Pipeline.CallMiddleware(_Handler.HandleRequest, _Environment.Environment);

                var headers = _Environment.ResponseHeaders.GetCommaSeparatedValues("Access-Control-Allow-Methods");
                Assert.AreEqual(1, headers.Count(r => String.Equals(r, method, StringComparison.OrdinalIgnoreCase)), $"Failed for method {method}");
            }
        }

        [TestMethod]
        public void CorsHandler_Preflight_Rejects_Requests_With_No_Method()
        {
            ConfigureCorsSupport("*");
            ConfigurePreflightRequest("http://www.allowed.com", requestMethod: "");

            _Pipeline.CallMiddleware(_Handler.HandleRequest, _Environment.Environment);

            AssertPreflightRejected();
        }

        [TestMethod]
        public void CorsHandler_Preflight_Accepts_All_Requested_Headers()
        {
            ConfigureCorsSupport("*");
            ConfigurePreflightRequest("http://www.allowed.com", requestHeaders: new string[] { "Header-1", "Header-2" });

            _Pipeline.CallMiddleware(_Handler.HandleRequest, _Environment.Environment);

            AssertPreflightAccepted("http://www.allowed.com");
            var headers = _Environment.ResponseHeaders.GetCommaSeparatedValues("Access-Control-Allow-Headers");
            Assert.AreEqual(1, headers.Count(r => r == "Header-1"));
            Assert.AreEqual(1, headers.Count(r => r == "Header-2"));
        }
        #endregion

        #region Simple request
        [TestMethod]
        public void CorsHandler_Simple_Requests_Are_Accepted()
        {
            ConfigureCorsSupport("*");
            ConfigureSimpleRequest("http://www.allowed.com");

            _Pipeline.CallMiddleware(_Handler.HandleRequest, _Environment.Environment);

            AssertSimpleAccepted("http://www.allowed.com");
        }

        [TestMethod]
        public void CorsHandler_Simple_Adds_Header_For_Safe_Methods()
        {
            foreach(var safeMethod in new string[] { "GET", "HEAD", "POST" }) {
                TestCleanup();
                TestInitialise();

                ConfigureCorsSupport("*");
                ConfigureSimpleRequest("http://www.allowed.com", method: safeMethod);

                _Pipeline.CallMiddleware(_Handler.HandleRequest, _Environment.Environment);

                AssertSimpleAccepted("http://www.allowed.com");
            }
        }
        #endregion
    }
}
