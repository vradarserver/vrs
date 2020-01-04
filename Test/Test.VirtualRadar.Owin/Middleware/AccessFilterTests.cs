// Copyright © 2017 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Net;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface.Owin;

namespace Test.VirtualRadar.Owin.Middleware
{
    [TestClass]
    public class AccessFilterTests
    {
        public TestContext TestContext { get; set; }
        private IClassFactory _Snapshot;

        private IAccessFilter _Filter;
        private MockOwinEnvironment _Environment;
        private MockOwinPipeline _Pipeline;
        private Mock<IAccessConfiguration> _AccessConfiguration;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();
            _AccessConfiguration = TestUtilities.CreateMockSingleton<IAccessConfiguration>();

            _Filter = Factory.Resolve<IAccessFilter>();
            _Environment = new MockOwinEnvironment();
            _Pipeline = new MockOwinPipeline();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        private void Configure_Acceptable_Request()
        {
            _AccessConfiguration.Setup(r => r.IsPathAccessible(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _AccessConfiguration.Setup(r => r.IsPathAccessible(It.IsAny<string>(), It.IsAny<IPAddress>())).Returns(true);
        }

        private void Configure_Unacceptable_Request()
        {
            _AccessConfiguration.Setup(r => r.IsPathAccessible(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
            _AccessConfiguration.Setup(r => r.IsPathAccessible(It.IsAny<string>(), It.IsAny<IPAddress>())).Returns(false);
        }

        private void Configure_Access(string path, string address, bool canAccess)
        {
            var ipAddress = IPAddress.Parse(address);
            _AccessConfiguration.Setup(r => r.IsPathAccessible(path, address)).Returns((string p, string a) => canAccess);
            _AccessConfiguration.Setup(r => r.IsPathAccessible(path, ipAddress)).Returns((string p, IPAddress a) => canAccess);
        }

        [TestMethod]
        public void AccessFilter_Calls_Next_Middleware_When_AccessConfiguration_Allows_Request()
        {
            Configure_Acceptable_Request();

            _Pipeline.CallMiddleware(_Filter.FilterRequest, _Environment.Environment);

            Assert.IsTrue(_Pipeline.NextMiddlewareCalled);
        }

        [TestMethod]
        public void AccessFilter_Does_Not_Call_Next_Middleware_When_AccessConfiguration_Disallows_Request()
        {
            Configure_Unacceptable_Request();

            _Pipeline.CallMiddleware(_Filter.FilterRequest, _Environment.Environment);

            Assert.IsFalse(_Pipeline.NextMiddlewareCalled);
        }

        [TestMethod]
        public void AccessFilter_Passes_Correct_Environment_Values_To_AccessConfiguration()
        {
            Configure_Acceptable_Request();
            Configure_Access("/file.txt", "192.168.0.1", canAccess: false);

            _Environment.RequestPath = "/file.txt";
            _Environment.ServerRemoteIpAddress = "192.168.0.1";

            _Pipeline.CallMiddleware(_Filter.FilterRequest, _Environment.Environment);

            Assert.IsFalse(_Pipeline.NextMiddlewareCalled);
        }

        [TestMethod]
        public void AccessFilter_Passes_Correct_Environment_Values_To_AccessConfiguration_When_Called_Via_Proxy()
        {
            Configure_Acceptable_Request();
            Configure_Access("/file.txt", "1.2.3.4", canAccess: false);

            _Environment.RequestPath = "/file.txt";
            _Environment.ServerRemoteIpAddress = "192.168.0.1";
            _Environment.RequestHeaders["X-Forwarded-For"] = "1.2.3.4";        // This should put 1.2.3.4 into the client IP address on the request

            _Pipeline.CallMiddleware(_Filter.FilterRequest, _Environment.Environment);

            Assert.IsFalse(_Pipeline.NextMiddlewareCalled);
        }

        [TestMethod]
        public void AccessFilter_Sets_Forbidden_Status_When_Access_Blocked()
        {
            Configure_Unacceptable_Request();

            _Pipeline.CallMiddleware(_Filter.FilterRequest, _Environment.Environment);

            Assert.AreEqual((int)HttpStatusCode.Forbidden, _Environment.Context.ResponseStatusCode);
        }

        [TestMethod]
        public void AccessFilter_Does_Not_Set_Forbidden_Status_When_Access_Allowed()
        {
            Configure_Acceptable_Request();

            _Pipeline.CallMiddleware(_Filter.FilterRequest, _Environment.Environment);

            Assert.AreNotEqual((int)HttpStatusCode.Forbidden, _Environment.Context.ResponseStatusCode);
        }

        [TestMethod]
        public void AccessFilter_Considers_Empty_Path_To_Be_Root()
        {
            Configure_Acceptable_Request();
            Configure_Access("/", "192.168.0.1", canAccess: false);

            _Environment.RequestPath = "";
            _Environment.ServerRemoteIpAddress = "192.168.0.1";

            _Pipeline.CallMiddleware(_Filter.FilterRequest, _Environment.Environment);

            Assert.IsFalse(_Pipeline.NextMiddlewareCalled);
        }

        [TestMethod]
        public void AccessFilter_Ignores_CurrentFolder_Path_Part()
        {
            Configure_Acceptable_Request();
            Configure_Access("/protected", "192.168.0.1", canAccess: false);

            _Environment.RequestPath = "/./protected";
            _Environment.ServerRemoteIpAddress = "192.168.0.1";

            _Pipeline.CallMiddleware(_Filter.FilterRequest, _Environment.Environment);

            Assert.IsFalse(_Pipeline.NextMiddlewareCalled);
        }

        [TestMethod]
        public void AccessFilter_Flattens_Directory_Traversal_Path_Parts()
        {
            Configure_Acceptable_Request();
            Configure_Access("/allowed", "192.168.0.1", canAccess: true);
            Configure_Access("/protected", "192.168.0.1", canAccess: false);

            _Environment.RequestPath = "/allowed/../protected";
            _Environment.ServerRemoteIpAddress = "192.168.0.1";

            _Pipeline.CallMiddleware(_Filter.FilterRequest, _Environment.Environment);

            Assert.IsFalse(_Pipeline.NextMiddlewareCalled);
        }
    }
}
