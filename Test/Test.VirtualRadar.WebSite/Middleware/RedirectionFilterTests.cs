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
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.WebSite.Middleware
{
    [TestClass]
    public class RedirectionFilterTests
    {
        class Redirection
        {
            public string NormalPath { get; set; }
            public string MobilePath { get; set; }
        }

        public TestContext TestContext { get; set; }
        private IClassFactory _Snapshot;

        private IRedirectionFilter _Filter;
        private MockOwinEnvironment _Environment;
        private MockOwinPipeline _Pipeline;
        private Mock<ISharedConfiguration> _SharedConfiguration;
        private Configuration _Configuration;
        private Mock<IRedirectionConfiguration> _RedirectionConfiguration;
        private Dictionary<string, Redirection> _Redirections;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _SharedConfiguration = TestUtilities.CreateMockSingleton<ISharedConfiguration>();
            _Configuration = new global::VirtualRadar.Interface.Settings.Configuration();
            _SharedConfiguration.Setup(r => r.Get()).Returns(_Configuration);

            _RedirectionConfiguration = TestUtilities.CreateMockSingleton<IRedirectionConfiguration>();
            _Redirections = new Dictionary<string, Middleware.RedirectionFilterTests.Redirection>(StringComparer.OrdinalIgnoreCase);
            _RedirectionConfiguration.Setup(r => r.RedirectToPathFromRoot(It.IsAny<string>(), It.IsAny<RedirectionRequestContext>())).Returns(
                (string path, RedirectionRequestContext context) => {
                    string redirectTo = null;
                    Redirection redirection;
                    if(_Redirections.TryGetValue(path, out redirection)) {
                        if(context.IsMobile) {
                            redirectTo = redirection.MobilePath ?? redirection.NormalPath;
                        } else {
                            redirectTo = redirection.NormalPath;
                        }
                    }
                    return redirectTo;
                }
            );

            _Filter = Factory.Resolve<IRedirectionFilter>();
            _Environment = new MockOwinEnvironment();
            _Pipeline = new MockOwinPipeline();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        private void ConfigureEnvironment(
            string scheme = "http",
            string host = "127.0.0.1",
            int? port = 80,
            string pathBase = "/VirtualRadar",
            string path = "/",
            string queryString = ""
        ) {
            if(port != null && port != 0) {
                host = $"{host}:{port}";
            }

            _Environment.RequestScheme =       scheme;
            _Environment.RequestHost =         host;
            _Environment.ServerLocalPort =     port.GetValueOrDefault();
            _Environment.RequestPathBase =     pathBase;
            _Environment.RequestPath =         path;
            _Environment.RequestQueryString =  queryString;
        }

        private void ConfigureRedirection(string fromPath, string toPath, bool isMobile = false)
        {
            Redirection redirection;
            if(!_Redirections.TryGetValue(fromPath, out redirection)) {
                redirection = new Redirection();
                _Redirections.Add(fromPath, redirection);
            }

            if(isMobile) {
                redirection.MobilePath = toPath;
            } else {
                redirection.NormalPath = toPath;
            }
        }

        private void AssertPassedThrough()
        {
            Assert.IsTrue(_Pipeline.NextMiddlewareCalled);
            Assert.IsTrue(
                   _Environment.Context.ResponseStatusCode == null  //  null and 200 are equivalent, if the status code remains
                || _Environment.Context.ResponseStatusCode == 200   //  at zero then eventually the runtime will set it to 200
            );
        }

        private void AssertRedirectedTo(string fullPath, int status = 302)
        {
            Assert.IsFalse(_Pipeline.NextMiddlewareCalled);

            var location = _Environment.ResponseHeaders["Location"];
            Assert.IsNotNull(location);
            Assert.AreEqual(fullPath, location);

            Assert.AreEqual(status, _Environment.ResponseStatusCode);
        }

        [TestMethod]
        public void RedirectionFilter_Passes_Request_Through_If_Path_Not_Known()
        {
            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment.Environment);
            AssertPassedThrough();
        }

        [TestMethod]
        public void RedirectionFilter_Redirects_Request_If_Path_Known()
        {
            ConfigureRedirection("/", "/index.html");
            ConfigureEnvironment(scheme: "http", host: "127.0.0.1", port: 8080, pathBase: "/VirtualRadar", path: "/", queryString: "arg=value");

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment.Environment);

            AssertRedirectedTo("http://127.0.0.1:8080/VirtualRadar/index.html?arg=value");
        }

        [TestMethod]
        public void RedirectionFilter_Treats_Empty_Path_As_Root()
        {
            ConfigureRedirection("/", "/index.html");
            ConfigureEnvironment(scheme: "http", host: "127.0.0.1", port: 8080, pathBase: "/VirtualRadar", path: "", queryString: "arg=value");

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment.Environment);

            AssertRedirectedTo("http://127.0.0.1:8080/VirtualRadar/index.html?arg=value");
        }

        [TestMethod]
        public void RedirectionFilter_Removes_Reference_To_Port_80_For_Http_Requests()
        {
            ConfigureRedirection("/", "/index.html");
            ConfigureEnvironment(scheme: "http", host: "127.0.0.1", port: 80, pathBase: "/VirtualRadar", path: "/", queryString: "arg=value");

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment.Environment);

            AssertRedirectedTo("http://127.0.0.1/VirtualRadar/index.html?arg=value");
        }

        [TestMethod]
        public void RedirectionFilter_Does_Not_Remove_Reference_To_Port_80_For_Https_Requests()
        {
            ConfigureRedirection("/", "/index.html");
            ConfigureEnvironment(scheme: "https", host: "127.0.0.1", port: 80, pathBase: "/VirtualRadar", path: "/", queryString: "arg=value");

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment.Environment);

            AssertRedirectedTo("https://127.0.0.1:80/VirtualRadar/index.html?arg=value");
        }

        [TestMethod]
        public void RedirectionFilter_Removes_Reference_To_Port_443_For_Https_Requests()
        {
            ConfigureRedirection("/", "/index.html");
            ConfigureEnvironment(scheme: "https", host: "127.0.0.1", port: 443, pathBase: "/VirtualRadar", path: "/", queryString: "arg=value");

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment.Environment);

            AssertRedirectedTo("https://127.0.0.1/VirtualRadar/index.html?arg=value");
        }

        [TestMethod]
        public void RedirectionFilter_Does_Not_Remove_Reference_To_Port_443_For_Http_Requests()
        {
            ConfigureRedirection("/", "/index.html");
            ConfigureEnvironment(scheme: "http", host: "127.0.0.1", port: 443, pathBase: "/VirtualRadar", path: "/", queryString: "arg=value");

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment.Environment);

            AssertRedirectedTo("http://127.0.0.1:443/VirtualRadar/index.html?arg=value");
        }

        [TestMethod]
        public void RedirectionFilter_Uses_UserAgent_As_Best_Guess_For_Mobile_Device()
        {
            TestMobileUserAgent(@"Mozilla/5.0 (Linux; Android 4.0.4; Galaxy Nexus Build/IMM76B) AppleWebKit/535.19 (KHTML, like Gecko) Chrome/18.0.1025.133 Mobile Safari/535.19");
            TestMobileUserAgent(@"Mozilla/5.0 (iPhone; CPU iPhone OS 10_3 like Mac OS X) AppleWebKit/602.1.50 (KHTML, like Gecko) CriOS/56.0.2924.75 Mobile/14E5239e Safari/602.1");
            TestMobileUserAgent(@"Mozilla/5.0 (iPad; CPU OS 7_0 like Mac OS X) AppleWebKit/537.51.1 (KHTML, like Gecko) CriOS/30.0.1599.12 Mobile/11A465 Safari/8536.25 (3B92C18B-D9DE-4CB7-A02A-22FD2AF17C8F)");
            TestMobileUserAgent(@"Mozilla/5.0 (iPhone; CPU iPhone OS 7_0_3 like Mac OS X) AppleWebKit/537.51.1 (KHTML, like Gecko) CriOS/30.0.1599.16 Mobile/11B511 Safari/8536.25 (07EADACC-003D-4A04-850D-7D680B48921E)");
            TestMobileUserAgent(@"Mozilla/5.0 (compatible; MSIE 10.0; Windows Phone 8.0; Trident/6.0; IEMobile/10.0; ARM; Touch; NOKIA; Lumia 920)");
        }

        private void TestMobileUserAgent(string userAgent)
        {
            TestCleanup();
            TestInitialise();

            ConfigureRedirection("/", "/not-mobile.html", isMobile: false);
            ConfigureRedirection("/", "/mobile.html", isMobile: true);
            _Environment.RequestHeaders["User-Agent"] = userAgent;

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment.Environment);

            var location = _Environment.ResponseHeaders["Location"];
            Assert.IsTrue(location.EndsWith("/mobile.html"), $"{userAgent} not recognised as mobile user agent");
        }

        [TestMethod]
        public void RedirectionFilter_Uses_UserAgent_As_Best_Guess_For_NonMobile_Device()
        {
            TestNonMobileUserAgent(null);
            TestNonMobileUserAgent(@"");
            TestNonMobileUserAgent(@"Mozilla/5.0 (Windows NT x.y; WOW64; rv:10.0) Gecko/20100101 Firefox/10.0");
            TestNonMobileUserAgent(@"Mozilla/5.0 (Macintosh; PPC Mac OS X x.y; rv:10.0) Gecko/20100101 Firefox/10.0");
            TestNonMobileUserAgent(@"Mozilla/5.0 (X11; Linux i686 on x86_64; rv:10.0) Gecko/20100101 Firefox/10.0");
            TestNonMobileUserAgent(@"Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36");
        }

        private void TestNonMobileUserAgent(string userAgent)
        {
            TestCleanup();
            TestInitialise();

            ConfigureRedirection("/", "/not-mobile.html", isMobile: false);
            ConfigureRedirection("/", "/mobile.html", isMobile: true);
            _Environment.RequestHeaders["User-Agent"] = userAgent;

            _Pipeline.BuildAndCallMiddleware(_Filter.AppFuncBuilder, _Environment.Environment);

            var location = _Environment.ResponseHeaders["Location"];
            Assert.IsTrue(location.EndsWith("/not-mobile.html"), $"{userAgent} not recognised as desktop user agent");
        }
    }
}
