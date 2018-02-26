// Copyright © 2014 onwards, Andrew Whewell
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
using System.Text;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;
using System.Net;

namespace Test.VirtualRadar.WebSite
{
    [TestClass]
    public class WebSiteExtenderTests
    {
        #region Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private IClassFactory _OriginalFactory;

        private IWebSiteExtender _Extender;
        private Mock<IWebSite> _WebSite;
        private SiteRoot _AddSiteRoot;
        private SiteRoot _RemoveSiteRoot;
        private List<HtmlContentInjector> _AddHtmlContentInjectors;
        private List<HtmlContentInjector> _RemoveHtmlContentInjectors;
        private PluginStartupParameters _PluginStartupParameters;
        private Mock<IAutoConfigWebServer> _AutoConfigWebServer;
        private Mock<IWebServer> _WebServer;
        private Mock<IRequest> _Request;
        private Mock<IResponse> _Response;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalFactory = Factory.TakeSnapshot();

            _Extender = Factory.Resolve<IWebSiteExtender>();

            _WebSite = TestUtilities.CreateMockInstance<IWebSite>();
            _PluginStartupParameters = new PluginStartupParameters(null, null, _WebSite.Object, "PluginFolder");

            _AddSiteRoot = null;
            _RemoveSiteRoot = null;
            _WebSite.Setup(r => r.AddSiteRoot(It.IsAny<SiteRoot>())).Callback((SiteRoot siteRoot) => _AddSiteRoot = siteRoot);
            _WebSite.Setup(r => r.RemoveSiteRoot(It.IsAny<SiteRoot>())).Callback((SiteRoot siteRoot) => _RemoveSiteRoot = siteRoot);

            _AddHtmlContentInjectors = new List<HtmlContentInjector>();
            _RemoveHtmlContentInjectors = new List<HtmlContentInjector>();
            _WebSite.Setup(r => r.AddHtmlContentInjector(It.IsAny<HtmlContentInjector>())).Callback((HtmlContentInjector injector) => _AddHtmlContentInjectors.Add(injector));
            _WebSite.Setup(r => r.RemoveHtmlContentInjector(It.IsAny<HtmlContentInjector>())).Callback((HtmlContentInjector injector) => _RemoveHtmlContentInjectors.Add(injector));

            _AutoConfigWebServer = TestUtilities.CreateMockSingleton<IAutoConfigWebServer>();
            _WebServer = TestUtilities.CreateMockInstance<IWebServer>();
            _AutoConfigWebServer.Setup(r => r.WebServer).Returns(_WebServer.Object);
            _Request = TestUtilities.CreateMockInstance<IRequest>();
            _Response = TestUtilities.CreateMockInstance<IResponse>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalFactory);
        }
        #endregion

        #region Helpers
        private RequestReceivedEventArgs CreateRequestReceivedEventArgs(string address)
        {
            _Request.Setup(r => r.RemoteEndPoint).Returns(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 54321));
            _Request.Setup(r => r.RawUrl).Returns("/VirtualRadar" + address);
            _Request.Setup(r => r.Url).Returns(new Uri("http://127.0.0.1" + _Request.Object.RawUrl));

            return new RequestReceivedEventArgs(_Request.Object, _Response.Object, "/VirtualRadar");
        }
        #endregion

        #region Constructor and Properties
        [TestMethod]
        public void WebSiteExtender_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            Assert.AreEqual(0, _Extender.InjectPages.Count);
            Assert.AreEqual(0, _Extender.PageHandlers.Count);

            TestUtilities.TestProperty(_Extender, r => r.Enabled, false);
            TestUtilities.TestProperty(_Extender, r => r.InjectContent, null, "Content");
            TestUtilities.TestProperty(_Extender, r => r.Priority, 100, -100);
            TestUtilities.TestProperty(_Extender, r => r.WebRootSubFolder, null, "Web");
        }
        #endregion

        #region Initialise
        #region SiteRoot Tests
        [TestMethod]
        public void WebSiteExtender_Initialise_Adds_Site_Root()
        {
            _Extender.Enabled = true;
            _Extender.WebRootSubFolder = "Site";

            _Extender.Initialise(_PluginStartupParameters);

            Assert.IsNotNull(_AddSiteRoot);
            Assert.IsNull(_RemoveSiteRoot);

            Assert.AreEqual(0, _AddSiteRoot.Checksums.Count);
            Assert.AreEqual("PluginFolder\\Site", _AddSiteRoot.Folder);
        }

        [TestMethod]
        public void WebSiteExtender_Initialise_Adds_Site_Root_With_Correct_Priority()
        {
            _Extender.Enabled = true;
            _Extender.WebRootSubFolder = "Site";
            _Extender.Priority = -102;

            _Extender.Initialise(_PluginStartupParameters);

            Assert.AreEqual(-102, _AddSiteRoot.Priority);
        }

        [TestMethod]
        public void WebSiteExtender_Initialise_Does_Not_Add_Site_Root_If_WebRootSubFolder_Is_Null()
        {
            _Extender.Enabled = true;
            _Extender.WebRootSubFolder = null;

            _Extender.Initialise(_PluginStartupParameters);

            Assert.IsNull(_AddSiteRoot);
        }

        [TestMethod]
        public void WebSiteExtender_Initialise_Does_Not_Add_Site_Root_If_WebRootSubFolder_Is_Empty()
        {
            _Extender.Enabled = true;
            _Extender.WebRootSubFolder = "";

            _Extender.Initialise(_PluginStartupParameters);

            Assert.IsNull(_AddSiteRoot);
        }

        [TestMethod]
        public void WebSiteExtender_Initialise_Does_Not_Add_Site_Root_If_Enabled_Is_False()
        {
            _Extender.Enabled = false;
            _Extender.WebRootSubFolder = "Site";

            _Extender.Initialise(_PluginStartupParameters);

            Assert.IsNull(_AddSiteRoot);
        }
        #endregion

        #region HtmlContentInjector Tests
        [TestMethod]
        public void WebSiteExtender_Initialise_Adds_HtmlContentInjector()
        {
            _Extender.Enabled = true;
            _Extender.InjectContent = "Some content";
            _Extender.InjectPages.Add("/desktop.html");

            _Extender.Initialise(_PluginStartupParameters);

            Assert.AreEqual(1, _AddHtmlContentInjectors.Count);
            Assert.AreEqual(0, _RemoveHtmlContentInjectors.Count);

            var injector = _AddHtmlContentInjectors[0];
            Assert.AreEqual(false, injector.AtStart);
            Assert.AreEqual("Some content", injector.Content());
            Assert.AreEqual("HEAD", injector.Element, ignoreCase: true);
            Assert.AreEqual("/desktop.html", injector.PathAndFile);
        }

        [TestMethod]
        public void WebSiteExtender_Initialise_Adds_HtmlContentInjector_With_Correct_Priority()
        {
            _Extender.Enabled = true;
            _Extender.InjectContent = "Some content";
            _Extender.InjectPages.Add("/desktop.html");
            _Extender.Priority = -1023;

            _Extender.Initialise(_PluginStartupParameters);

            var injector = _AddHtmlContentInjectors[0];
            Assert.AreEqual(-1023, injector.Priority);
        }

        [TestMethod]
        public void WebSiteExtender_Initialise_Adds_HtmlContentInjector_For_All_Pages_If_InjectPages_Is_Empty()
        {
            _Extender.Enabled = true;
            _Extender.InjectContent = "Some content";

            _Extender.Initialise(_PluginStartupParameters);

            Assert.AreEqual(1, _AddHtmlContentInjectors.Count);

            var injector = _AddHtmlContentInjectors[0];
            Assert.AreEqual(false, injector.AtStart);
            Assert.AreEqual("Some content", injector.Content());
            Assert.AreEqual("HEAD", injector.Element, ignoreCase: true);
            Assert.AreEqual(null, injector.PathAndFile);
        }

        [TestMethod]
        public void WebSiteExtender_Initialise_Adds_HtmlContentInjector_With_Correct_Priority_When_Injecting_Into_All_Pages()
        {
            _Extender.Enabled = true;
            _Extender.InjectContent = "Some content";
            _Extender.Priority = -1023;

            _Extender.Initialise(_PluginStartupParameters);

            var injector = _AddHtmlContentInjectors[0];
            Assert.AreEqual(-1023, injector.Priority);
        }

        [TestMethod]
        public void WebSiteExtender_Initialise_Adds_HtmlContentInjector_That_Ignores_Changes_To_InjectContent_Property()
        {
            _Extender.Enabled = true;
            _Extender.InjectContent = "Original";
            _Extender.InjectPages.Add("/desktop.html");
            _Extender.Initialise(_PluginStartupParameters);

            _Extender.InjectContent = "New";

            var injector = _AddHtmlContentInjectors[0];
            Assert.AreEqual("Original", injector.Content());
        }

        [TestMethod]
        public void WebSiteExtender_Initialise_Adds_HtmlContentInjector_For_Every_Page_In_InjectPages()
        {
            _Extender.Enabled = true;
            _Extender.InjectContent = "Some content";
            _Extender.InjectPages.Add("/desktop.html");
            _Extender.InjectPages.Add("/mobile.html");

            _Extender.Initialise(_PluginStartupParameters);

            Assert.AreEqual(2, _AddHtmlContentInjectors.Count);
            Assert.AreEqual(0, _RemoveHtmlContentInjectors.Count);

            Assert.IsTrue(_AddHtmlContentInjectors.Any(r => r.PathAndFile == "/desktop.html"));
            Assert.IsTrue(_AddHtmlContentInjectors.Any(r => r.PathAndFile == "/mobile.html"));
        }

        [TestMethod]
        public void WebSiteExtender_Initialise_Does_Not_Add_HtmlContentInjector_If_InjectContent_Is_Null()
        {
            _Extender.Enabled = true;
            _Extender.InjectContent = null;
            _Extender.InjectPages.Add("/desktop.html");

            _Extender.Initialise(_PluginStartupParameters);

            Assert.AreEqual(0, _AddHtmlContentInjectors.Count);
        }

        [TestMethod]
        public void WebSiteExtender_Initialise_Does_Not_Add_HtmlContentInjector_If_InjectContent_Is_Empty_String()
        {
            _Extender.Enabled = true;
            _Extender.InjectContent = "";
            _Extender.InjectPages.Add("/desktop.html");

            _Extender.Initialise(_PluginStartupParameters);

            Assert.AreEqual(0, _AddHtmlContentInjectors.Count);
        }

        [TestMethod]
        public void WebSiteExtender_Initialise_Does_Not_Add_HtmlContentInjector_If_Enabled_Is_False()
        {
            _Extender.Enabled = false;
            _Extender.InjectContent = "Hello";
            _Extender.InjectPages.Add("/desktop.html");

            _Extender.Initialise(_PluginStartupParameters);

            Assert.AreEqual(0, _AddHtmlContentInjectors.Count);
        }
        #endregion

        #region PageHandler Tests
        [TestMethod]
        public void WebExtender_Initialise_Adds_Page_Handlers()
        {
            _Extender.Enabled = true;
            RequestReceivedEventArgs eventArgs = null;
            _Extender.PageHandlers.Add("/plugin/data.json", (RequestReceivedEventArgs args) => eventArgs = args);
            _Extender.Initialise(_PluginStartupParameters);

            var raiseArgs = CreateRequestReceivedEventArgs("/plugin/data.json");
            _WebServer.Raise(r => r.BeforeRequestReceived += null, raiseArgs);

            Assert.AreSame(raiseArgs, eventArgs);
        }

        [TestMethod]
        public void WebExtender_Initialise_Adds_All_Page_Handlers()
        {
            _Extender.Enabled = true;
            RequestReceivedEventArgs page1EventArgs = null;
            RequestReceivedEventArgs page2EventArgs = null;
            _Extender.PageHandlers.Add("/plugin/page1.json", (RequestReceivedEventArgs args) => page1EventArgs = args);
            _Extender.PageHandlers.Add("/plugin/page2.json", (RequestReceivedEventArgs args) => page2EventArgs = args);
            _Extender.Initialise(_PluginStartupParameters);

            _WebServer.Raise(r => r.BeforeRequestReceived += null, CreateRequestReceivedEventArgs("/plugin/page1.json"));
            Assert.IsNotNull(page1EventArgs);
            Assert.IsNull(page2EventArgs);

            page1EventArgs = null;
            _WebServer.Raise(r => r.BeforeRequestReceived += null, CreateRequestReceivedEventArgs("/plugin/page2.json"));
            Assert.IsNull(page1EventArgs);
            Assert.IsNotNull(page2EventArgs);
        }

        [TestMethod]
        public void WebExtender_Initialise_Adds_Page_Handlers_That_Are_Case_Insensitive()
        {
            _Extender.Enabled = true;
            RequestReceivedEventArgs eventArgs = null;
            _Extender.PageHandlers.Add("/Plugin/Data.json", (RequestReceivedEventArgs args) => eventArgs = args);
            _Extender.Initialise(_PluginStartupParameters);

            var raiseArgs = CreateRequestReceivedEventArgs("/plugin/data.json");
            _WebServer.Raise(r => r.BeforeRequestReceived += null, raiseArgs);

            Assert.AreSame(raiseArgs, eventArgs);
        }

        [TestMethod]
        public void WebExtender_Initialise_Adds_Page_Handlers_That_Ignore_Query_String_Parameters()
        {
            _Extender.Enabled = true;
            RequestReceivedEventArgs eventArgs = null;
            _Extender.PageHandlers.Add("/Plugin/Data.json", (RequestReceivedEventArgs args) => eventArgs = args);
            _Extender.Initialise(_PluginStartupParameters);

            var raiseArgs = CreateRequestReceivedEventArgs("/plugin/data.json?param1=x&param2=y");
            _WebServer.Raise(r => r.BeforeRequestReceived += null, raiseArgs);

            Assert.AreSame(raiseArgs, eventArgs);
        }

        [TestMethod]
        public void WebExtender_Initialise_Ignores_Future_Changes_To_PageHandlers()
        {
            _Extender.Enabled = true;
            RequestReceivedEventArgs eventArgs = null;
            _Extender.PageHandlers.Add("/plugin/data.json", (RequestReceivedEventArgs args) => eventArgs = args);
            _Extender.Initialise(_PluginStartupParameters);

            _Extender.PageHandlers.Clear();

            var raiseArgs = CreateRequestReceivedEventArgs("/plugin/data.json");
            _WebServer.Raise(r => r.BeforeRequestReceived += null, raiseArgs);

            Assert.AreSame(raiseArgs, eventArgs);
        }

        [TestMethod]
        public void WebExtender_Initialise_Does_Not_Add_Page_Handlers_If_Enabled_Is_False()
        {
            _Extender.Enabled = false;
            RequestReceivedEventArgs eventArgs = null;
            _Extender.PageHandlers.Add("/plugin/data.json", (RequestReceivedEventArgs args) => eventArgs = args);
            _Extender.Initialise(_PluginStartupParameters);

            var raiseArgs = CreateRequestReceivedEventArgs("/plugin/data.json");
            _WebServer.Raise(r => r.BeforeRequestReceived += null, raiseArgs);

            Assert.IsNull(eventArgs);
        }
        #endregion
        #endregion

        #region Enabled Property
        #region SiteRoot
        [TestMethod]
        public void WebSiteExtender_Enabled_False_Removes_Site_Root()
        {
            _Extender.Enabled = true;
            _Extender.WebRootSubFolder = "Site";
            _Extender.Initialise(_PluginStartupParameters);

            _Extender.Enabled = false;

            Assert.IsNotNull(_RemoveSiteRoot);
            Assert.AreSame(_AddSiteRoot, _RemoveSiteRoot);
        }

        [TestMethod]
        public void WebSiteExtender_Enabled_False_Does_Not_Make_Redundant_Remove_Of_SiteRoot()
        {
            _Extender.Enabled = true;
            _Extender.WebRootSubFolder = "Site";
            _Extender.Initialise(_PluginStartupParameters);

            _Extender.Enabled = false;
            _Extender.Enabled = false;

            _WebSite.Verify(r => r.RemoveSiteRoot(It.IsAny<SiteRoot>()), Times.Once());
        }

        [TestMethod]
        public void WebSiteExtender_Enabled_False_Does_Not_Remove_Site_Root_If_No_Root_Supplied()
        {
            _Extender.Enabled = true;
            _Extender.WebRootSubFolder = null;
            _Extender.Initialise(_PluginStartupParameters);

            _Extender.Enabled = false;

            _WebSite.Verify(r => r.RemoveSiteRoot(It.IsAny<SiteRoot>()), Times.Never());
        }

        [TestMethod]
        public void WebSiteExtender_Enabled_True_Adds_Site_Root()
        {
            _Extender.Enabled = false;
            _Extender.WebRootSubFolder = "Site";
            _Extender.Initialise(_PluginStartupParameters);

            _Extender.Enabled = true;

            Assert.IsNotNull(_AddSiteRoot);
        }

        [TestMethod]
        public void WebSiteExtender_Enabled_True_Does_Not_Make_Redundant_Add_Of_SiteRoot()
        {
            _Extender.Enabled = true;
            _Extender.WebRootSubFolder = "Site";
            _Extender.Initialise(_PluginStartupParameters);

            _Extender.Enabled = true;

            _WebSite.Verify(r => r.AddSiteRoot(It.IsAny<SiteRoot>()), Times.Once());
        }

        [TestMethod]
        public void WebSiteExtender_Enabled_True_Does_Not_Add_Site_Root_If_No_Root_Supplied()
        {
            _Extender.Enabled = false;
            _Extender.WebRootSubFolder = null;
            _Extender.Initialise(_PluginStartupParameters);

            _Extender.Enabled = true;

            _WebSite.Verify(r => r.AddSiteRoot(It.IsAny<SiteRoot>()), Times.Never());
        }
        #endregion

        #region HtmlContentInjector
        [TestMethod]
        public void WebSiteExtender_Enabled_False_Removes_HtmlContentInjector()
        {
            _Extender.Enabled = true;
            _Extender.InjectContent = "Some content";
            _Extender.InjectPages.Add("/desktop.html");
            _Extender.Initialise(_PluginStartupParameters);

            _Extender.Enabled = false;

            Assert.AreEqual(1, _RemoveHtmlContentInjectors.Count);
            Assert.AreSame(_AddHtmlContentInjectors[0], _RemoveHtmlContentInjectors[0]);
        }

        [TestMethod]
        public void WebSiteExtender_Enabled_False_Does_Not_Make_Redundant_Remove_Of_HtmlContentInjector()
        {
            _Extender.Enabled = true;
            _Extender.InjectContent = "Some content";
            _Extender.InjectPages.Add("/desktop.html");
            _Extender.Initialise(_PluginStartupParameters);

            _Extender.Enabled = false;
            _Extender.Enabled = false;

            _WebSite.Verify(r => r.RemoveHtmlContentInjector(It.IsAny<HtmlContentInjector>()), Times.Once());
        }

        [TestMethod]
        public void WebSiteExtender_Enabled_False_Removes_All_HtmlContentInjectors()
        {
            _Extender.Enabled = true;
            _Extender.InjectContent = "Some content";
            _Extender.InjectPages.Add("/desktop.html");
            _Extender.InjectPages.Add("/mobile.html");
            _Extender.Initialise(_PluginStartupParameters);

            _Extender.Enabled = false;

            Assert.AreEqual(2, _RemoveHtmlContentInjectors.Count);
        }

        [TestMethod]
        public void WebSiteExtender_Enabled_False_Does_Not_Remove_HtmlContentInjectors_If_InjectContent_Missing()
        {
            _Extender.Enabled = true;
            _Extender.InjectContent = null;
            _Extender.InjectPages.Add("/desktop.html");
            _Extender.Initialise(_PluginStartupParameters);

            _Extender.Enabled = false;

            _WebSite.Verify(r => r.RemoveHtmlContentInjector(It.IsAny<HtmlContentInjector>()), Times.Never());
        }

        [TestMethod]
        public void WebSiteExtender_Enabled_True_Adds_HtmlContentInjectors()
        {
            _Extender.Enabled = false;
            _Extender.InjectContent = "Some content";
            _Extender.InjectPages.Add("/desktop.html");
            _Extender.InjectPages.Add("/mobile.html");
            _Extender.Initialise(_PluginStartupParameters);

            _Extender.Enabled = true;

            Assert.AreEqual(2, _AddHtmlContentInjectors.Count);
        }

        [TestMethod]
        public void WebSiteExtender_Enabled_True_Does_Not_Make_Redundant_Add_Of_HtmlContentInjector()
        {
            _Extender.Enabled = true;
            _Extender.InjectContent = "Some content";
            _Extender.InjectPages.Add("/desktop.html");
            _Extender.Initialise(_PluginStartupParameters);

            _Extender.Enabled = true;

            _WebSite.Verify(r => r.AddHtmlContentInjector(It.IsAny<HtmlContentInjector>()), Times.Once());
        }

        [TestMethod]
        public void WebSiteExtender_Enabled_True_Does_Not_Add_HtmlContentInjectors_If_InjectContent_Missing()
        {
            _Extender.Enabled = false;
            _Extender.InjectContent = null;
            _Extender.InjectPages.Add("/desktop.html");
            _Extender.Initialise(_PluginStartupParameters);

            _Extender.Enabled = true;

            _WebSite.Verify(r => r.AddHtmlContentInjector(It.IsAny<HtmlContentInjector>()), Times.Never());
        }
        #endregion

        #region PageHandlers
        [TestMethod]
        public void WebExtender_Enabled_False_Disables_PageHandlers()
        {
            _Extender.Enabled = true;
            RequestReceivedEventArgs eventArgs = null;
            _Extender.PageHandlers.Add("/plugin/data.json", (RequestReceivedEventArgs args) => eventArgs = args);
            _Extender.Initialise(_PluginStartupParameters);

            _Extender.Enabled = false;

            _WebServer.Raise(r => r.BeforeRequestReceived += null, CreateRequestReceivedEventArgs("/plugin/data.json"));

            Assert.IsNull(eventArgs);
        }

        [TestMethod]
        public void WebExtender_Enabled_True_Enables_PageHandlers()
        {
            _Extender.Enabled = false;
            RequestReceivedEventArgs eventArgs = null;
            _Extender.PageHandlers.Add("/plugin/data.json", (RequestReceivedEventArgs args) => eventArgs = args);
            _Extender.Initialise(_PluginStartupParameters);

            _Extender.Enabled = true;

            _WebServer.Raise(r => r.BeforeRequestReceived += null, CreateRequestReceivedEventArgs("/plugin/data.json"));

            Assert.IsNotNull(eventArgs);
        }
        #endregion
        #endregion

        #region Dispose
        [TestMethod]
        public void WebSiteExtender_Dispose_Removes_Site_Root()
        {
            _Extender.Enabled = true;
            _Extender.WebRootSubFolder = "Site";
            _Extender.Initialise(_PluginStartupParameters);

            _Extender.Dispose();

            Assert.IsNotNull(_RemoveSiteRoot);
            Assert.AreSame(_AddSiteRoot, _RemoveSiteRoot);
        }

        [TestMethod]
        public void WebSiteExtender_Dispose_Removes_HtmlContentInjector()
        {
            _Extender.Enabled = true;
            _Extender.InjectContent = "Some content";
            _Extender.InjectPages.Add("/desktop.html");
            _Extender.Initialise(_PluginStartupParameters);

            _Extender.Dispose();

            Assert.AreEqual(1, _RemoveHtmlContentInjectors.Count);
            Assert.AreSame(_AddHtmlContentInjectors[0], _RemoveHtmlContentInjectors[0]);
        }

        [TestMethod]
        public void WebExtender_Dispose_Releases_WebServer_Event()
        {
            _Extender.Enabled = true;
            RequestReceivedEventArgs eventArgs = null;
            _Extender.PageHandlers.Add("/plugin/data.json", (RequestReceivedEventArgs args) => eventArgs = args);
            _Extender.Initialise(_PluginStartupParameters);

            _Extender.Dispose();

            _WebServer.Raise(r => r.BeforeRequestReceived += null, CreateRequestReceivedEventArgs("/plugin/data.json"));

            Assert.IsNull(eventArgs);
        }
        #endregion

        #region InjectMapPages
        [TestMethod]
        public void WebSiteExtender_InjectMapPages_Adds_Desktop_Map_Page()
        {
            _Extender.InjectMapPages();
            Assert.IsTrue(_Extender.InjectPages.Contains("/desktop.html"));
        }

        [TestMethod]
        public void WebSiteExtender_InjectMapPages_Adds_Mobile_Map_Page()
        {
            _Extender.InjectMapPages();
            Assert.IsTrue(_Extender.InjectPages.Contains("/mobile.html"));
        }

        [TestMethod]
        public void WebSiteExtender_InjectMapPages_Returns_This()
        {
            Assert.AreSame(_Extender, _Extender.InjectMapPages());
        }
        #endregion

        #region InjectReportPages
        [TestMethod]
        public void WebSiteExtender_InjectReportPages_Adds_Desktop_Report_Page()
        {
            _Extender.InjectReportPages();
            Assert.IsTrue(_Extender.InjectPages.Contains("/desktopReport.html"));
        }

        [TestMethod]
        public void WebSiteExtender_InjectReportPages_Adds_Mobile_Report_Page()
        {
            _Extender.InjectReportPages();
            Assert.IsTrue(_Extender.InjectPages.Contains("/mobileReport.html"));
        }

        [TestMethod]
        public void WebSiteExtender_InjectReportPages_Returns_This()
        {
            Assert.AreSame(_Extender, _Extender.InjectReportPages());
        }
        #endregion
    }
}
