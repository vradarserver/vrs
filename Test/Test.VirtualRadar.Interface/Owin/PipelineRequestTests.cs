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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface.Owin;

namespace Test.VirtualRadar.Interface.Owin
{
    [TestClass]
    public class PipelineRequestTests
    {
        public TestContext TestContext { get; set; }

        private Dictionary<string, object> _Environment;
        private PipelineRequest _Request;

        [TestInitialize]
        public void TestInitialise()
        {
            _Environment = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            _Environment["owin.RequestHeaders"] = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            _Request = new PipelineRequest(_Environment);
        }

        [TestMethod]
        public void PipelineRequest_Constructor_Initialises_To_Known_State()
        {
            Assert.AreSame(_Environment, _Request.Environment);

            var defaultCtor = new PipelineRequest();
            Assert.IsNotNull(defaultCtor.Environment);
        }

        [TestMethod]
        public void PipelineRequest_PathNormalised_Returns_Root_If_Path_Is_Empty()
        {
            _Request.Path = new PathString("");
            Assert.AreEqual(new PathString("/"), _Request.PathNormalised);
        }

        [TestMethod]
        public void PipelineRequest_PathNormalised_Returns_Null_PathString_If_Path_Is_Null()
        {
            _Request.Path = new PathString(null);
            Assert.IsNull(_Request.PathNormalised.Value);
        }

        [TestMethod]
        public void PipelineRequest_PathNormalised_Returns_Path_If_Not_Empty()
        {
            _Request.Path = new PathString("/hello");
            Assert.AreEqual("/hello", _Request.PathNormalised.Value);
        }

        [TestMethod]
        public void PipelineRequest_PathNormalised_Picks_Up_Changes_To_Path()
        {
            _Request.Path = new PathString("");
            Assert.AreEqual("/", _Request.PathNormalised.Value);

            _Request.Path = new PathString("/hello");
            Assert.AreEqual("/hello", _Request.PathNormalised.Value);
        }

        [TestMethod]
        public void PipelineRequest_ClientIpAddressParsed_Returns_Parsed_Address()
        {
            _Request.RemoteIpAddress = "1.2.3.4";
            var parsed = _Request.ClientIpAddressParsed;
            Assert.AreEqual(AddressFamily.InterNetwork, parsed.AddressFamily);
            Assert.AreEqual("1.2.3.4", parsed.ToString());
        }

        [TestMethod]
        public void PipelineRequest_ClientIpAddressParsed_Actually_Uses_ClientIpAddress_Not_RemoteIpAddress()
        {
            _Request.RemoteIpAddress = "192.168.0.1";
            _Request.Headers.Add("X-Forwarded-For", new string[] { "1.2.3.4" });

            var parsed = _Request.ClientIpAddressParsed;
            Assert.AreEqual(AddressFamily.InterNetwork, parsed.AddressFamily);
            Assert.AreEqual("1.2.3.4", parsed.ToString());
        }

        [TestMethod]
        public void PipelineRequest_ClientIpAddressParsed_Returns_None_If_Address_Missing()
        {
            var parsed = _Request.ClientIpAddressParsed;
            Assert.AreEqual(IPAddress.None, parsed);
        }

        [TestMethod]
        public void PipelineRequest_ClientIpAddressParsed_Returns_None_If_Address_Empty()
        {
            _Request.RemoteIpAddress = "";
            var parsed = _Request.ClientIpAddressParsed;
            Assert.AreEqual(IPAddress.None, parsed);
        }

        [TestMethod]
        public void PipelineRequest_ClientIpAddressParsed_Returns_None_If_Address_Unparseable()
        {
            _Request.RemoteIpAddress = "this is not an address";
            var parsed = _Request.ClientIpAddressParsed;
            Assert.AreEqual(IPAddress.None, parsed);
        }

        [TestMethod]
        public void PipelineRequest_ClientIpAddressParsed_Picks_Up_Changes_To_Remote_Address()
        {
            _Request.RemoteIpAddress = "1.2.3.4";
            var parsed = _Request.ClientIpAddressParsed;

            _Request.RemoteIpAddress = "5.6.7.8";
            parsed = _Request.ClientIpAddressParsed;

            Assert.AreEqual("5.6.7.8", parsed.ToString());
        }

        [TestMethod]
        public void PipelineRequest_ClientIpEndPoint_Returns_Parsed_IPAddress_And_Port()
        {
            _Request.RemoteIpAddress = "1.2.3.4";
            _Request.RemotePort = 789;

            Assert.AreEqual(new IPEndPoint(IPAddress.Parse("1.2.3.4"), 789), _Request.ClientIpEndPoint);
        }

        [TestMethod]
        public void PipelineRequest_ClientIpEndPoint_Is_Influenced_By_XFF_Headers()
        {
            _Request.RemoteIpAddress = "10.1.2.3";
            _Request.Headers.Add("X-Forwarded-For", new string[] {  "1.2.3.4" });
            _Request.RemotePort = 789;

            Assert.AreEqual(new IPEndPoint(IPAddress.Parse("1.2.3.4"), 789), _Request.ClientIpEndPoint);
        }

        [TestMethod]
        public void PipelineRequest_ClientIpEndPoint_Handles_Case_Where_Remote_Address_Is_Unknown()
        {
            _Request.RemotePort = 789;

            Assert.AreEqual(new IPEndPoint(IPAddress.None, 789), _Request.ClientIpEndPoint);
        }

        [TestMethod]
        public void PipelineRequest_ClientIpEndPoint_Handles_Case_Where_Remote_Port_Is_Unknown()
        {
            _Request.RemoteIpAddress = "1.2.3.4";
            _Request.RemotePort = null;

            Assert.AreEqual(new IPEndPoint(IPAddress.Parse("1.2.3.4"), 0), _Request.ClientIpEndPoint);
        }

        [TestMethod]
        public void PipelineRequest_ClientIpEndPoint_Picks_Up_Changes_To_Remote_IPAddress_Or_Port()
        {
            _Request.RemoteIpAddress = "1.2.3.4";
            _Request.RemotePort = 789;
            Assert.AreEqual(new IPEndPoint(IPAddress.Parse("1.2.3.4"), 789), _Request.ClientIpEndPoint);

            _Request.RemoteIpAddress = "5.6.7.8";
            Assert.AreEqual(new IPEndPoint(IPAddress.Parse("5.6.7.8"), 789), _Request.ClientIpEndPoint);

            _Request.RemotePort = 123;
            Assert.AreEqual(new IPEndPoint(IPAddress.Parse("5.6.7.8"), 123), _Request.ClientIpEndPoint);
        }

        [TestMethod]
        [DataSource("Data Source='OwinTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "MobileUserAgent$")]
        public void PipelineRequest_IsMobileUserAgentString_Decodes_Common_Mobile_UserAgent_Strings()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            var userAgent = worksheet.EString("UserAgent");
            var device =    worksheet.String("Device");
            var isMobile = worksheet.Bool("IsMobile");

            _Request.UserAgent = userAgent;

            Assert.AreEqual(isMobile, _Request.IsMobileUserAgentString, "{0}: {1}", device, userAgent ?? "<null>");
        }

        [TestMethod]
        public void PipelineRequest_IsMobileUserAgentString_Copes_If_UserAgent_Changes()
        {
            _Request.UserAgent = "Mozilla/5.0 (Linux; Android 6.0.1; SM-G920V Build/MMB29K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.98 Mobile Safari/537.36";   // Samsung S6
            Assert.AreEqual(true, _Request.IsMobileUserAgentString);

            _Request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246"; // Windows 10 Edge
            Assert.AreEqual(false, _Request.IsMobileUserAgentString);
        }

        [TestMethod]
        public void PipelineRequest_IsMobileUserAgentString_Returns_False_If_There_Is_No_User_Agent()
        {
            Assert.AreEqual(false, _Request.IsMobileUserAgentString);
        }

        [TestMethod]
        public void PipelineRequest_UserAgent_Returns_UserAgent_Header()
        {
            _Request.Headers.Add("User-Agent",  new string[] { "agent" });

            Assert.AreEqual("agent", _Request.UserAgent);
        }

        [TestMethod]
        public void PipelineRequest_UserAgent_Sets_UserAgent_Header()
        {
            _Request.UserAgent = "agent";

            Assert.AreEqual("agent", _Request.Headers["User-Agent"]);
        }

        [TestMethod]
        public void PipelineRequest_IsLocalOrLan_Calls_Down_To_IPEndpointHelper_IsLocalOrLan()
        {
            // Hard to test this one without reproducing all of the tests for IsLocalOrLan...
            _Request.RemoteIpAddress = "127.0.0.1";
            Assert.IsTrue(_Request.IsLocalOrLan);

            _Request.RemoteIpAddress = "1.2.3.4";
            Assert.IsFalse(_Request.IsLocalOrLan);

            _Request.RemoteIpAddress = "192.168.0.1";
            Assert.IsTrue(_Request.IsLocalOrLan);

            _Request.RemoteIpAddress = "unparseable";
            Assert.IsFalse(_Request.IsLocalOrLan);

            _Request.RemoteIpAddress = "";
            Assert.IsFalse(_Request.IsLocalOrLan);

            _Request.RemoteIpAddress = null;
            Assert.IsFalse(_Request.IsLocalOrLan);
        }

        [TestMethod]
        public void PipelineRequest_IsLocalOrLan_Takes_Proxy_Servers_Into_Account()
        {
            _Request.RemoteIpAddress = "192.168.0.1";
            _Request.Headers.Add("X-Forwarded-For", new string[] { "1.2.3.4" });

            Assert.IsFalse(_Request.IsLocalOrLan);
        }

        [TestMethod]
        [DataSource("Data Source='OwinTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "IsInternet$")]
        public void PipelineRequest_IsInternet_And_ProxyAddress_Are_Accurately_Detected_During_Construction()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            var remoteIpAddress =       worksheet.EString("RemoteIpAddress");
            var xff =                   worksheet.EString("XFF");
            var proxyIpAddress =        worksheet.EString("ProxyIpAddress");
            var clientIpAddress =       worksheet.EString("ClientIpAddress");
            var isInternet =            worksheet.Bool("IsInternet");
            var comments =              worksheet.EString("Comments");

            _Request.RemoteIpAddress = remoteIpAddress;
            if(xff != null) {
                _Request.Headers.Add("X-Forwarded-For", new string[] { xff });
            }

            Assert.AreEqual(proxyIpAddress, _Request.ProxyIpAddress, comments);
            Assert.AreEqual(clientIpAddress, _Request.ClientIpAddress, comments);
            Assert.AreEqual(isInternet, _Request.IsInternet, comments);
        }
    }
}
