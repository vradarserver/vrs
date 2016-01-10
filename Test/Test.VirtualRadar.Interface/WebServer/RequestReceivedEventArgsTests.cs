// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.VirtualRadar;
using VirtualRadar.Interface.WebServer;
using Moq;
using Test.Framework;
using System.Net;
using System.Collections.Specialized;

namespace Test.VirtualRadar.Interface.WebServer
{
    [TestClass]
    public class RequestReceivedEventArgsTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void RequestReceivedEventArgs_Conforms_To_EventArgs_Spec()
        {
            Assert.IsTrue(typeof(EventArgs).IsAssignableFrom(typeof(RequestReceivedEventArgs)));
        }

        [TestMethod]
        public void RequestReceivedEventArgs_Constructor_Initialises_To_Known_State()
        {
            var request = new Mock<IRequest>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            var response = new Mock<IResponse>()  { DefaultValue = DefaultValue.Mock }.SetupAllProperties();

            var endPoint = new IPEndPoint(new IPAddress(0x0100007f), 80);
            request.Setup(r => r.RemoteEndPoint).Returns(endPoint);

            RequestReceivedEventArgs args = new RequestReceivedEventArgs(request.Object, response.Object, "/Root");
            Assert.AreSame(request.Object, args.Request);
            Assert.AreSame(response.Object, args.Response);
            Assert.AreEqual("/Root", args.Root);
            Assert.AreEqual(0, args.QueryString.Count);
            Assert.IsNull(args.ProxyAddress);
            Assert.AreEqual("127.0.0.1", args.ClientAddress);
            Assert.AreEqual(false, args.IsIPad);
            Assert.AreEqual(false, args.IsIPhone);
            Assert.AreEqual(false, args.IsAndroid);
            Assert.AreEqual(false, args.IsIPod);
            TestUtilities.TestProperty(args, "Handled", false);
            TestUtilities.TestProperty(args, "Classification", ContentClassification.Other, ContentClassification.Image);
        }

        [TestMethod]
        [DataSource("Data Source='WebServerTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "RequestReceivedEventArgs$")]
        public void RequestReceivedEventArgs_Constructor_Builds_Properties_From_Request_Correctly()
        {
            ExcelWorksheetData worksheet = new ExcelWorksheetData(TestContext);

            var request = new Mock<IRequest>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            var response = new Mock<IResponse>()  { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            var headers = new NameValueCollection();

            var endPoint = new IPEndPoint(IPAddress.Parse(worksheet.String("ResponseAddress")), 45678);

            request.Setup(r => r.RemoteEndPoint).Returns(endPoint);
            request.Setup(r => r.Url).Returns(new Uri(worksheet.EString("Url")));
            request.Setup(r => r.RawUrl).Returns(worksheet.EString("RawUrl"));
            request.Setup(r => r.Headers).Returns(headers);

            var xForwardedFor = worksheet.EString("XForwardedFor");
            if(xForwardedFor != null) headers.Add("X-Forwarded-For", xForwardedFor);

            var args = new RequestReceivedEventArgs(request.Object, response.Object, worksheet.EString("Root"));

            Assert.AreEqual(worksheet.EString("WebSite"), args.WebSite);
            Assert.AreEqual(worksheet.EString("PathAndFile"), args.PathAndFile);
            Assert.AreEqual(worksheet.EString("Path"), args.Path);
            Assert.AreEqual(worksheet.EString("File"), args.File);
            Assert.AreEqual(worksheet.EString("ProxyAddress"), args.ProxyAddress);
            Assert.AreEqual(worksheet.EString("ClientAddress"), args.ClientAddress);
            Assert.AreEqual(worksheet.Bool("IsInternetRequest"), args.IsInternetRequest);
            Assert.AreEqual(worksheet.Int("PathCount"), args.PathParts.Count);
            Assert.AreEqual(worksheet.Int("QueryCount"), args.QueryString.Count);

            for(var i = 0;i < args.PathParts.Count;++i) {
                Assert.AreEqual(worksheet.EString(String.Format("Path{0}", i)), args.PathParts[i]);
            }

            for(var i = 0;i < args.QueryString.Count;++i) {
                var expectedName = worksheet.String(String.Format("QueryName{0}", i));
                var expectedValue = worksheet.String(String.Format("QueryValue{0}", i));

                Assert.AreEqual(expectedValue, args.QueryString[expectedName]);
            }
        }

        [TestMethod]
        [DataSource("Data Source='WebServerTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "RequestReceivedUserAgent$")]
        public void RequestReceivedEventArgs_Can_Interpret_User_Agent_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var request = new Mock<IRequest>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            var response = new Mock<IResponse>()  { DefaultValue = DefaultValue.Mock }.SetupAllProperties();

            request.Setup(r => r.RemoteEndPoint).Returns(new IPEndPoint(IPAddress.Parse("1.2.3.4"), 80));
            request.Setup(r => r.Url).Returns(new Uri("http://127.0.0.1/Root/Whatever"));
            request.Setup(r => r.RawUrl).Returns("/Root/Whatever");
            request.Setup(r => r.UserAgent).Returns(worksheet.String("UserAgent"));

            var args = new RequestReceivedEventArgs(request.Object, response.Object, "/Root");

            Assert.AreEqual(worksheet.Bool("IsAndroid"), args.IsAndroid);
            Assert.AreEqual(worksheet.Bool("IsIpad"), args.IsIPad);
            Assert.AreEqual(worksheet.Bool("IsIPhone"), args.IsIPhone);
            Assert.AreEqual(worksheet.Bool("IsIPod"), args.IsIPod);
        }

        [TestMethod]
        public void RequestReceivedEventArgs_Is_Case_Insensitive_When_Looking_For_Reverse_Proxy_Headers()
        {
            var request = new Mock<IRequest>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            var response = new Mock<IResponse>()  { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            var headers = new NameValueCollection();

            request.Setup(r => r.RemoteEndPoint).Returns(new IPEndPoint(IPAddress.Parse("192.168.0.1"), 1234));
            request.Setup(r => r.Url).Returns(new Uri("http://192.168.0.2/Root/"));
            request.Setup(r => r.RawUrl).Returns("http://192.168.0.2/Root/");
            request.Setup(r => r.Headers).Returns(headers);
            headers.Add("x-forwarded-for", "1.2.3.4");

            var args = new RequestReceivedEventArgs(request.Object, response.Object, "/Root");

            Assert.AreEqual("192.168.0.1", args.ProxyAddress);
            Assert.AreEqual("1.2.3.4", args.ClientAddress);
            Assert.AreEqual(true, args.IsInternetRequest);
        }
    }
}