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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using AWhewell.Owin.Utility;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;

namespace Test.VirtualRadar.WebSite
{
    [TestClass]
    public partial class WebSiteTests
    {
        public TestContext TestContext { get; set; }

        private IClassFactory _OriginalContainer;

        // Mocks etc. actually used by the tests
        private IWebSite _WebSite;
        private Mock<IWebServer> _WebServer;
        private Mock<IRequest> _Request;
        private Mock<IResponse> _Response;
        private Mock<IInstallerSettingsStorage> _InstallerSettingsStorage;
        private InstallerSettings _InstallerSettings;
        private Mock<ISharedConfiguration> _SharedConfiguration;
        private Configuration _Configuration;
        private Mock<IBaseStationDatabase> _BaseStationDatabase;
        private Mock<IAutoConfigBaseStationDatabase> _AutoConfigBaseStationDatabase;
        private Mock<IStandingDataManager> _StandingDataManager;
        private MockOwinPipelineConfiguration _PipelineConfiguration;
        private Mock<ILoopbackHost> _LoopbackHost;
        private string _LoopbackPathAndFile;
        private IDictionary<string, object> _LoopbackEnvironment;
        private SimpleContent _LoopbackResponse;
        private Mock<IFileSystemServerConfiguration> _FileSystemServerConfiguration;
        private Mock<IPluginManager> _PluginManager;

        // Other mocks required to get IWebSite implementation running
        private Mock<IRuntimeEnvironment> _RuntimeEnvironment;
        private Mock<IUserManager> _UserManager;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalContainer = Factory.TakeSnapshot();

            _WebServer = new Mock<IWebServer>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _Request = new Mock<IRequest>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _Response = new Mock<IResponse>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();

            _InstallerSettingsStorage = TestUtilities.CreateMockImplementation<IInstallerSettingsStorage>();
            _InstallerSettings = new InstallerSettings();
            _InstallerSettingsStorage.Setup(m => m.Load()).Returns(_InstallerSettings);

            _SharedConfiguration = TestUtilities.CreateMockSingleton<ISharedConfiguration>();
            _Configuration = new Configuration();
            _Configuration.GoogleMapSettings.WebSiteReceiverId = 1;
            _Configuration.GoogleMapSettings.ClosestAircraftReceiverId = 1;
            _SharedConfiguration.Setup(r => r.Get()).Returns(_Configuration);

            _RuntimeEnvironment = TestUtilities.CreateMockSingleton<IRuntimeEnvironment>();
            _RuntimeEnvironment.Setup(r => r.IsMono).Returns(false);
            _RuntimeEnvironment.Setup(r => r.ExecutablePath).Returns(TestContext.TestDeploymentDir);

            _BaseStationDatabase = new Mock<IBaseStationDatabase>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _AutoConfigBaseStationDatabase = TestUtilities.CreateMockSingleton<IAutoConfigBaseStationDatabase>();
            _AutoConfigBaseStationDatabase.Setup(a => a.Database).Returns(_BaseStationDatabase.Object);

            _StandingDataManager = TestUtilities.CreateMockSingleton<IStandingDataManager>();
            _UserManager = TestUtilities.CreateMockSingleton<IUserManager>();

            _PipelineConfiguration = new MockOwinPipelineConfiguration();
            Factory.RegisterInstance<IPipelineConfiguration>(_PipelineConfiguration);

            _LoopbackHost = TestUtilities.CreateMockImplementation<ILoopbackHost>();
            _LoopbackEnvironment = null;
            _LoopbackPathAndFile = null;
            _LoopbackResponse = new SimpleContent() {
                Content = new byte[0],
                HttpStatusCode = HttpStatusCode.OK,
            };
            _LoopbackHost.Setup(r => r.SendSimpleRequest(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>())).Returns((string p, IDictionary<string, object> e) => {
                _LoopbackPathAndFile = p;
                _LoopbackEnvironment = e;
                return _LoopbackResponse;
            });

            _FileSystemServerConfiguration = TestUtilities.CreateMockSingleton<IFileSystemServerConfiguration>();
            _PluginManager = TestUtilities.CreateMockSingleton<IPluginManager>();

            _WebSite = Factory.Resolve<IWebSite>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalContainer);
        }

        private void ConfigureRequestObject(string root, string urlFromRoot, string queryString = null, string protocol = "http", string dns = "mysite.co.uk", string remoteEndPoint = "1.2.3.4", int remotePort = 54321, int localPort = 80)
        {
            queryString = String.IsNullOrEmpty(queryString) ? "" : $"?{queryString}";
            _Request.Setup(r => r.RawUrl).Returns($"{root}{urlFromRoot}{queryString}");
            _Request.Setup(r => r.Url).Returns(new Uri($"{protocol}://{dns}:{localPort}{_Request.Object.RawUrl}"));
            _Request.Setup(r => r.RemoteEndPoint).Returns(new IPEndPoint(IPAddress.Parse(remoteEndPoint), remotePort));
        }

        private Stream ConfigureResponseObject(Stream stream = null)
        {
            if(stream == null) {
                stream = new MemoryStream();
            }

            _Response.Setup(r => r.OutputStream).Returns(stream);

            return stream;
        }

        #region Constructors and Properties
        [TestMethod]
        public void WebSite_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            _WebSite = Factory.Resolve<IWebSite>();
            Assert.IsNull(_WebSite.WebServer);
            TestUtilities.TestProperty(_WebSite, "BaseStationDatabase", null, _BaseStationDatabase.Object);
            TestUtilities.TestProperty(_WebSite, "StandingDataManager", null, _StandingDataManager.Object);
        }
        #endregion

        #region AttachSiteToServer
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WebSite_AttachSiteToServer_Throws_If_Passed_Null()
        {
            _WebSite.AttachSiteToServer(null);
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Sets_Server_Property()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);
            Assert.AreSame(_WebServer.Object, _WebSite.WebServer);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void WebSite_AttachSiteToServer_Can_Only_Be_Called_Once()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);
            _WebSite.AttachSiteToServer(new Mock<IWebServer>().Object);
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Registers_Standard_OWIN_Pipeline()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);
            Assert.AreEqual(1, _PipelineConfiguration.AddPipelineCallCount);
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Registers_Plugin_Pipelines()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);
            _PluginManager.Verify(r => r.RegisterOwinMiddleware(), Times.Once());
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Copes_If_Attached_To_The_Same_Server_Twice()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);
            _WebSite.AttachSiteToServer(_WebServer.Object);
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Sets_Server_Root()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);
            Assert.AreEqual("/VirtualRadar", _WebServer.Object.Root);
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Sets_Server_Port()
        {
            _InstallerSettings.WebServerPort = 9876;
            _WebSite.AttachSiteToServer(_WebServer.Object);
            Assert.AreEqual(9876, _WebServer.Object.Port);
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Allows_Default_File_System_Site_Pages_To_Be_Served()
        {
            SiteRoot siteRoot = null;
            _FileSystemServerConfiguration.Setup(r => r.AddSiteRoot(It.IsAny<SiteRoot>())).Callback((SiteRoot s) => {
                siteRoot = s;
            });

            _WebSite.AttachSiteToServer(_WebServer.Object);

            var runtime = Factory.ResolveSingleton<IRuntimeEnvironment>();
            var expectedFolder = String.Format("{0}{1}", Path.Combine(runtime.ExecutablePath, "Web"), Path.DirectorySeparatorChar);

            Assert.IsNotNull(siteRoot);
            Assert.AreEqual(0, siteRoot.Priority);
            Assert.AreEqual(expectedFolder, siteRoot.Folder);
            Assert.IsTrue(siteRoot.Checksums.Count > 0);
        }
        #endregion

        #region RequestContent
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WebSite_RequestContent_Throws_If_Passed_Null()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);
            _WebSite.RequestContent(null);
        }

        [TestMethod]
        public void WebSite_RequestContent_Calls_LoopbackHost()
        {
            var root = "/root";
            _WebSite.AttachSiteToServer(_WebServer.Object);

            ConfigureRequestObject(root, "/index.html");
            using(ConfigureResponseObject()) {
                _WebSite.RequestContent(new RequestReceivedEventArgs(_Request.Object, _Response.Object, root));
                _LoopbackHost.Verify(r => r.SendSimpleRequest("/root/index.html", It.IsAny<IDictionary<string, object>>()), Times.Once());
            }
        }

        [TestMethod]
        public void WebSite_RequestContent_Uses_Case_Insensitive_Dictionary()
        {
            var root = "/root";
            _WebSite.AttachSiteToServer(_WebServer.Object);

            ConfigureRequestObject(root, "/index.html");
            using(ConfigureResponseObject()) {
                _WebSite.RequestContent(new RequestReceivedEventArgs(_Request.Object, _Response.Object, root));

                _LoopbackEnvironment.Add("my-test", "123");
                Assert.AreEqual(_LoopbackEnvironment["MY-TEST"], "123");
            }
        }

        [TestMethod]
        public void WebSite_RequestContent_Fills_All_Required_OWIN_Entries()
        {
            var root = "/root";
            _WebSite.AttachSiteToServer(_WebServer.Object);

            ConfigureRequestObject(root, "/index.html", queryString: "abc=123%20456&xyz");
            using(ConfigureResponseObject()) {
                _WebSite.RequestContent(new RequestReceivedEventArgs(_Request.Object, _Response.Object, root));

                Assert.AreSame(Stream.Null, _LoopbackEnvironment["owin.RequestBody"]);
                Assert.IsNotNull(_LoopbackEnvironment["owin.RequestHeaders"] as IDictionary<string, string[]>);
                Assert.AreEqual("GET", _LoopbackEnvironment["owin.RequestMethod"] as string, ignoreCase: true);
                Assert.AreEqual("/index.html", _LoopbackEnvironment["owin.RequestPath"]);
                Assert.AreEqual(root, _LoopbackEnvironment["owin.RequestPathBase"]);
                Assert.AreEqual("HTTP/1.1", _LoopbackEnvironment["owin.RequestProtocol"]);
                Assert.AreEqual("abc=123%20456&xyz", _LoopbackEnvironment["owin.RequestQueryString"]);
                Assert.AreEqual("http", _LoopbackEnvironment["owin.RequestScheme"]);

                Assert.IsNotNull(_LoopbackEnvironment["owin.ResponseBody"] as Stream);
                Assert.AreNotSame(Stream.Null, _LoopbackEnvironment["owin.ResponseBody"]);
                Assert.IsNotNull(_LoopbackEnvironment["owin.ResponseHeaders"]  as IDictionary<string, string[]>);

                Assert.IsFalse(((CancellationToken)_LoopbackEnvironment["owin.CallCancelled"]).IsCancellationRequested);
                Assert.AreEqual("1.0.0", _LoopbackEnvironment["owin.Version"]);
            }
        }

        [TestMethod]
        public void WebSite_RequestContent_Fills_Host_Request_Header()
        {
            var root = "/root";
            _WebSite.AttachSiteToServer(_WebServer.Object);

            ConfigureRequestObject(root, "/index.html", dns: "example.com", localPort: 1234);
            using(ConfigureResponseObject()) {
                _WebSite.RequestContent(new RequestReceivedEventArgs(_Request.Object, _Response.Object, root));
                var context = OwinContext.Create(_LoopbackEnvironment);
                Assert.AreEqual("example.com:1234", context.RequestHost);
            }
        }

        [TestMethod]
        public void WebStite_RequestContent_Handles_No_QueryString_Case()
        {
            var root = "/root";
            _WebSite.AttachSiteToServer(_WebServer.Object);

            ConfigureRequestObject(root, "/index.html");
            using(ConfigureResponseObject()) {
                _WebSite.RequestContent(new RequestReceivedEventArgs(_Request.Object, _Response.Object, root));
                Assert.AreEqual("", _LoopbackEnvironment["owin.RequestQueryString"]);
            }
        }

        [TestMethod]
        public void WebSite_RequestContent_Request_Headers_Are_Case_Insensitive()
        {
            var root = "/root";
            _WebSite.AttachSiteToServer(_WebServer.Object);

            ConfigureRequestObject(root, "/index.html");
            using(ConfigureResponseObject()) {
                _WebSite.RequestContent(new RequestReceivedEventArgs(_Request.Object, _Response.Object, root));
                var headers = (IDictionary<string, string[]>)_LoopbackEnvironment["owin.RequestHeaders"];
                headers.Add("my-test", new string[] { "123" });
                Assert.AreEqual("123", headers["MY-TEST"][0]);
            }
        }

        [TestMethod]
        public void WebSite_RequestContent_Response_Headers_Are_Case_Insensitive()
        {
            var root = "/root";
            _WebSite.AttachSiteToServer(_WebServer.Object);

            ConfigureRequestObject(root, "/index.html");
            using(ConfigureResponseObject()) {
                _WebSite.RequestContent(new RequestReceivedEventArgs(_Request.Object, _Response.Object, root));
                var headers = (IDictionary<string, string[]>)_LoopbackEnvironment["owin.ResponseHeaders"];
                headers.Add("my-test", new string[] { "123" });
                Assert.AreEqual("123", headers["MY-TEST"][0]);
            }
        }

        [TestMethod]
        public void WebSite_RequestContent_Fills_In_Remote_Endpoint()
        {
            var root = "/root";
            _WebSite.AttachSiteToServer(_WebServer.Object);

            ConfigureRequestObject(root, "/index.html", remoteEndPoint: "1.2.3.4", remotePort: 54321);
            using(ConfigureResponseObject()) {
                _WebSite.RequestContent(new RequestReceivedEventArgs(_Request.Object, _Response.Object, root));
                Assert.AreEqual("1.2.3.4", _LoopbackEnvironment["server.RemoteIpAddress"]);
                Assert.AreEqual("54321", _LoopbackEnvironment["server.RemotePort"]);
            }
        }

        [TestMethod]
        public void WebSite_RequestContent_Returns_SimpleContent_HttpStatus()
        {
            var root = "/root";
            _WebSite.AttachSiteToServer(_WebServer.Object);

            ConfigureRequestObject(root, "/index.html");
            using(ConfigureResponseObject()) {
                _LoopbackResponse.HttpStatusCode = HttpStatusCode.Conflict;
                _WebSite.RequestContent(new RequestReceivedEventArgs(_Request.Object, _Response.Object, root));

                Assert.AreEqual(HttpStatusCode.Conflict, _Response.Object.StatusCode);
            }
        }

        [TestMethod]
        public void WebSite_RequestContent_Fills_Response_Stream_With_SimpleContent_Content()
        {
            var root = "/root";
            _WebSite.AttachSiteToServer(_WebServer.Object);

            ConfigureRequestObject(root, "/index.html");
            using(var stream = ConfigureResponseObject()) {
                _LoopbackResponse.Content = new byte[] { 1, 2, 3, 4 };
                _WebSite.RequestContent(new RequestReceivedEventArgs(_Request.Object, _Response.Object, root));

                stream.Position = 0;
                Assert.AreEqual(4, stream.Length);
                var streamContent = new byte[4];
                stream.Read(streamContent, 0, streamContent.Length);

                Assert.IsTrue(new byte[] { 1, 2, 3, 4 }.SequenceEqual(streamContent));
            }
        }

        [TestMethod]
        public void WebSite_RequestContent_Sets_ContentLength()
        {
            var root = "/root";
            _WebSite.AttachSiteToServer(_WebServer.Object);

            ConfigureRequestObject(root, "/index.html");
            using(ConfigureResponseObject()) {
                _LoopbackResponse.Content = new byte[] { 1, 2, 3, 4 };
                _WebSite.RequestContent(new RequestReceivedEventArgs(_Request.Object, _Response.Object, root));

                Assert.AreEqual(4, _Response.Object.ContentLength);
            }
        }
        #endregion

        #region RequestSimpleContent
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WebSite_RequestSimpleContent_Throws_If_Passed_Null()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);
            _WebSite.RequestSimpleContent(null);
        }
        #endregion

        #region Configuration Changes
        [TestMethod]
        public void WebSite_Configuration_Change_To_Authentication_Scheme_Cycles_WebServer()
        {
            _Configuration.WebServerSettings.AuthenticationScheme = AuthenticationSchemes.Anonymous;
            _WebSite.AttachSiteToServer(_WebServer.Object);

            _WebServer.Object.Online = true;

            _Configuration.WebServerSettings.AuthenticationScheme = AuthenticationSchemes.Basic;
            _SharedConfiguration.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _WebServer.VerifySet(r => r.Online = false, Times.AtLeastOnce());
            _WebServer.VerifySet(r => r.Online = true, Times.AtLeast(2));
            Assert.IsTrue(_WebServer.Object.Online);
        }

        [TestMethod]
        public void WebSite_Configuration_Change_To_Authentication_Scheme_Does_Not_Cycle_Server_If_Already_Offline()
        {
            _Configuration.WebServerSettings.AuthenticationScheme = AuthenticationSchemes.Anonymous;
            _WebSite.AttachSiteToServer(_WebServer.Object);

            _WebServer.Object.Online = false;

            _Configuration.WebServerSettings.AuthenticationScheme = AuthenticationSchemes.Basic;
            _SharedConfiguration.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _WebServer.VerifySet(r => r.Online = true, Times.Never());
            Assert.IsFalse(_WebServer.Object.Online);
        }
        #endregion

        #region HtmlLoadedFromFile event
        [TestMethod]
        public void WebSite_HtmlLoadedFromFile_Raised_When_OWIN_FileSystemConfiguration_Raises_TextLoadedFromFile()
        {
            var listener = new EventRecorder<TextContentEventArgs>();
            _WebSite.HtmlLoadedFromFile += listener.Handler;
            _WebSite.AttachSiteToServer(_WebServer.Object);

            var args = new TextContentEventArgs("/index.html", "The Content", Encoding.Unicode, MimeType.Html);
            _FileSystemServerConfiguration.Raise(r => r.TextLoadedFromFile += null, args);

            Assert.AreEqual(1, listener.CallCount);
            Assert.AreSame(_WebSite, listener.Sender);
            Assert.AreSame(args, listener.Args);
        }

        [TestMethod]
        public void WebSite_HtmlLoadedFromFile_Not_Raised_For_Non_HTML_Mime_Types()
        {
            var listener = new EventRecorder<TextContentEventArgs>();
            _WebSite.HtmlLoadedFromFile += listener.Handler;
            _WebSite.AttachSiteToServer(_WebServer.Object);

            var args = new TextContentEventArgs("/index.html", "The Content", Encoding.Unicode, MimeType.Css);
            _FileSystemServerConfiguration.Raise(r => r.TextLoadedFromFile += null, args);

            Assert.AreEqual(0, listener.CallCount);
        }
        #endregion
    }
}
