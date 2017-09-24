// Copyright © 2013 onwards, Andrew Whewell
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
using VirtualRadar.Interface.WebSite;
using InterfaceFactory;
using Test.Framework;
using VirtualRadar.Interface.WebServer;
using Moq;
using HtmlAgilityPack;
using System.IO;
using VirtualRadar.Interface.Settings;
using System.Net;

namespace Test.VirtualRadar.WebSite
{
    [TestClass]
    public class BundlerTests
    {
        #region TestContext, fields, initialise and cleanup
        public TestContext TestContext { get; set; }

        private IClassFactory _OriginalClassFactory;
        private IBundler _Bundler;
        private Mock<IWebServer> _WebServer;
        private Mock<IWebSite> _WebSite;
        private Mock<IRequest> _Request;
        private Mock<IResponse> _Response;
        private Mock<IMinifier> _Minifier;
        private MemoryStream _OutputStream;
        private Configuration _Configuration;
        private Mock<IConfigurationStorage> _ConfigurationStorage;
        private Dictionary<string, SimpleContent> _PathAndFileToContent;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalClassFactory = Factory.TakeSnapshot();

            _Configuration = new Configuration();
            _ConfigurationStorage = TestUtilities.CreateMockSingleton<IConfigurationStorage>();
            _ConfigurationStorage.Setup(r => r.Load()).Returns(_Configuration);

            _Bundler = Factory.Singleton.Resolve<IBundler>();

            _Minifier = TestUtilities.CreateMockImplementation<IMinifier>();
            _Minifier.Setup(r => r.MinifyJavaScript(It.IsAny<string>())).Returns((string js) => js);

            _WebServer = TestUtilities.CreateMockImplementation<IWebServer>();
            _WebServer.Setup(r => r.Root).Returns("/Root/");
            _WebSite = TestUtilities.CreateMockImplementation<IWebSite>();
            _WebSite.Setup(r => r.WebServer).Returns(_WebServer.Object);

            _Request = new Mock<IRequest>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _Response = new Mock<IResponse>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _OutputStream = new MemoryStream();
            _Response.Setup(m => m.OutputStream).Returns(_OutputStream);

            _PathAndFileToContent = new Dictionary<string, SimpleContent>();
            _WebSite.Setup(r => r.RequestSimpleContent(It.IsAny<string>())).Returns((string pathAndFile) => {
                SimpleContent simpleContent;
                if(!_PathAndFileToContent.TryGetValue(pathAndFile, out simpleContent)) {
                    simpleContent = new SimpleContent() { Content = new byte[] { }, HttpStatusCode = HttpStatusCode.NotFound };
                }
                return simpleContent;
            });
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalClassFactory);

            _Bundler.Dispose();
            if(_OutputStream != null) _OutputStream.Dispose();
            _OutputStream = null;
        }
        #endregion

        #region Helper methods
        private bool ContainsScriptTag(string html, string source)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);
            return document.DocumentNode.Descendants("script").Any(r => r.GetAttributeValue("src", "") == source);
        }

        private bool ContainsCssTag(string html, string href)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);
            return document.DocumentNode.Descendants("link").Any(r => r.GetAttributeValue("href", "") == href);
        }

        private void AddPathAndFileContent(string pathAndFile, HttpStatusCode statusCode)
        {
            _PathAndFileToContent.Add(pathAndFile, new SimpleContent() { Content = new byte[] { }, HttpStatusCode = statusCode });
        }

        private void AddPathAndFileContent(string pathAndFile, string content, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _PathAndFileToContent.Add(pathAndFile, new SimpleContent() { Content = Encoding.UTF8.GetBytes(content), HttpStatusCode = statusCode });
        }

        private RequestReceivedEventArgs SendRequest(string pathAndFile)
        {
            return SendRequest(pathAndFile, false);
        }

        private RequestReceivedEventArgs SendRequest(string pathAndFile, bool isInternetClient)
        {
            var args = RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, isInternetClient);
            _WebServer.Raise(m => m.RequestReceived += null, args);

            return args;
        }

        private string GetStringContent()
        {
            var result = "";
            _OutputStream.Position = 0;
            using(var streamReader = new StreamReader(_OutputStream, true)) {
                result = streamReader.ReadToEnd();
            }

            return result;
        }

        private string GetScriptAddress(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var script = document.DocumentNode.Descendants("script").Single();
            var result = script.GetAttributeValue("src", null);
            return StripRoot(result);
        }

        private string GetLinkAddress(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var link = document.DocumentNode.Descendants("link").Single();
            var result = link.GetAttributeValue("href", null);
            return StripRoot(result);
        }

        private string StripRoot(string address)
        {
            var root = _WebServer.Object.Root;
            if(root[root.Length - 1] == '/') root = root.Substring(0, root.Length - 1);
            if(address.StartsWith(root)) address = address.Substring(root.Length);

            return address;
        }
        #endregion

        #region Constructor and Properties
        [TestMethod]
        public void Bundler_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            Assert.IsNull(_Bundler.WebServer);
            Assert.IsNull(_Bundler.WebSite);
        }
        #endregion

        #region Dispose
        [TestMethod]
        public void Bundler_Dispose_Can_Be_Called_Without_Calling_AttachToWebsite()
        {
            _Bundler.Dispose();
        }

        [TestMethod]
        public void Bundler_Dispose_Can_Be_Called_Twice_Without_Crashing()
        {
            _Bundler.AttachToWebSite(_WebSite.Object);
            _Bundler.Dispose();
            _Bundler.Dispose();
        }

        [TestMethod]
        public void Bundler_Dispose_Unhooks_ConfigurationStorage_Changed()
        {
            _Configuration.GoogleMapSettings.EnableBundling = false;
            _Bundler.AttachToWebSite(_WebSite.Object);

            _Bundler.Dispose();

            _Configuration.GoogleMapSettings.EnableBundling = true;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            var html = @"<HTML><HEAD>" +
                       @"<!-- [[ JS BUNDLE START ]] -->" +
                       @"<SCRIPT SRC=""source-1.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<SCRIPT SRC=""source-2.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<!-- [[ BUNDLE END ]] -->" +
                       @"</HEAD></HTML>";
            var result = _Bundler.BundleHtml("/index.html", html);

            Assert.AreEqual(html, result);
        }

        [TestMethod]
        public void Bundler_Dispose_Unhooks_WebServer()
        {
            AddPathAndFileContent("/source-1.js", "CONTENT OF FIRST FILE");
            AddPathAndFileContent("/source-2.js", "CONTENT OF SECOND FILE");
            _Bundler.AttachToWebSite(_WebSite.Object);
            var html = @"<HTML><HEAD>" +
                       @"<!-- [[ JS BUNDLE START ]] -->" +
                       @"<SCRIPT SRC=""source-1.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<SCRIPT SRC=""source-2.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<!-- [[ BUNDLE END ]] -->" +
                       @"</HEAD></HTML>";
            var result = _Bundler.BundleHtml("/index.html", html);

            _Bundler.Dispose();

            var request = GetScriptAddress(result);
            var args = SendRequest(request);

            Assert.AreEqual(false, args.Handled);
        }
        #endregion

        #region AttachToWebSite
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Bundler_AttachToWebSite_Throws_If_Passed_Null()
        {
            _Bundler.AttachToWebSite(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Bundler_AttachToWebSite_Throws_If_WebSite_Not_Attached_To_WebServer()
        {
            _WebSite.Setup(r => r.WebServer).Returns((IWebServer)null);
            _Bundler.AttachToWebSite(_WebSite.Object);
        }

        [TestMethod]
        public void Bundler_AttachToWebSite_Sets_WebSite_And_WebServer_Properties()
        {
            _Bundler.AttachToWebSite(_WebSite.Object);
            Assert.AreSame(_WebServer.Object, _Bundler.WebServer);
            Assert.AreSame(_WebSite.Object, _Bundler.WebSite);
        }
        #endregion

        #region BundleHtml - Modify HTML
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Bundler_BundleHtml_Throws_If_Called_Before_AttachToWebSite()
        {
            _Bundler.BundleHtml("/index.html", "<HTML></HTML>");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Bundler_BundleHtml_Throws_If_RequestPath_Is_Null()
        {
            _Bundler.AttachToWebSite(_WebSite.Object);
            _Bundler.BundleHtml(null, "<HTML></HTML>");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Bundler_BundleHtml_Throws_If_Content_Is_Null()
        {
            _Bundler.AttachToWebSite(_WebSite.Object);
            _Bundler.BundleHtml("/index.html", null);
        }

        [TestMethod]
        public void Bundler_BundleHtml_Returns_HTML_Unchanged_If_It_Does_Not_Contain_Bundle_Commands()
        {
            _Bundler.AttachToWebSite(_WebSite.Object);

            var html = @"<HTML><HEAD>" +
                       @"<SCRIPT SRC=""source-1.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<SCRIPT SRC=""source-2.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"</HEAD></HTML>";
            var result = _Bundler.BundleHtml("/index.html", html);

            Assert.AreEqual(html, result);
        }

        [TestMethod]
        public void Bundler_BundleHtml_Returns_HTML_Unchanged_If_Bundling_Has_Been_Disabled()
        {
            _Configuration.GoogleMapSettings.EnableBundling = false;

            _Bundler.AttachToWebSite(_WebSite.Object);
            var html = @"<HTML><HEAD>" +
                       @"<!-- [[ JS BUNDLE START ]] -->" +
                       @"<SCRIPT SRC=""source-1.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<SCRIPT SRC=""source-2.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<!-- [[ BUNDLE END ]] -->" +
                       @"</HEAD></HTML>";
            var result = _Bundler.BundleHtml("/index.html", html);

            Assert.AreEqual(html, result);
        }

        [TestMethod]
        public void Bundler_BundleHtml_Responds_To_Changes_In_Configuration()
        {
            // Configuration starts off with bundling turned on by default
            _Bundler.AttachToWebSite(_WebSite.Object);

            // Turn bundling off
            _Configuration.GoogleMapSettings.EnableBundling = false;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            var html = @"<HTML><HEAD>" +
                       @"<!-- [[ JS BUNDLE START ]] -->" +
                       @"<SCRIPT SRC=""source-1.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<SCRIPT SRC=""source-2.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<!-- [[ BUNDLE END ]] -->" +
                       @"</HEAD></HTML>";
            var result = _Bundler.BundleHtml("/index.html", html);

            // Should not see any bundling in the result
            Assert.AreEqual(html, result);
        }

        [TestMethod]
        public void Bundler_BundleHtml_Substitutes_Multiple_Script_Statements_For_One_Script_Statement()
        {
            _Bundler.AttachToWebSite(_WebSite.Object);

            var html = @"<HTML><HEAD>" +
                       @"<!-- [[ JS BUNDLE START ]] -->" +
                       @"<SCRIPT SRC=""source-1.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<SCRIPT SRC=""source-2.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<!-- [[ BUNDLE END ]] -->" +
                       @"</HEAD></HTML>";
            var result = _Bundler.BundleHtml("/index.html", html);

            Assert.AreNotEqual(html, result);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(result);
            Assert.AreEqual(0, htmlDocument.ParseErrors.Count());
            var scriptTags = htmlDocument.DocumentNode.Descendants("script");
            Assert.AreEqual(1, scriptTags.Count());
            var script = scriptTags.Single();
            Assert.AreEqual("head", script.ParentNode.Name, ignoreCase:true);
            Assert.AreEqual(1, script.ChildNodes.Count);
            Assert.IsTrue(script.ChildNodes[0].NodeType == HtmlNodeType.Text);
            Assert.AreEqual("", ((HtmlTextNode)script.ChildNodes[0]).Text);
            var source = script.GetAttributeValue("src", null);
            Assert.IsNotNull(source);
            Assert.AreNotEqual("", source);
            Assert.IsTrue(source.StartsWith("bundle-"));
            Assert.AreEqual(Path.GetFileName(source), source);      // Bundles have no path, the site will serve the same bundle irrespective of path
            Assert.AreNotEqual("source-1.js", source);
            Assert.AreNotEqual("source-2.js", source);
        }

        [TestMethod]
        public void Bundler_BundleHtml_Subtitutes_Multiple_Bundles_In_One_Html_File()
        {
            _Bundler.AttachToWebSite(_WebSite.Object);

            var html = @"<HTML><HEAD>" +
                       @"<!-- [[ JS BUNDLE START ]] -->" +
                       @"<SCRIPT SRC=""source-1.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<SCRIPT SRC=""source-2.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<!-- [[ BUNDLE END ]] -->" +
                       @"<!-- [[ JS BUNDLE START ]] -->" +
                       @"<SCRIPT SRC=""source-3.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<SCRIPT SRC=""source-4.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<!-- [[ BUNDLE END ]] -->" +
                       @"</HEAD></HTML>";
            var result = _Bundler.BundleHtml("/index.html", html);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(result);

            Assert.IsFalse(ContainsScriptTag(result, "source-1.js"));
            Assert.IsFalse(ContainsScriptTag(result, "source-2.js"));
            Assert.IsFalse(ContainsScriptTag(result, "source-3.js"));
            Assert.IsFalse(ContainsScriptTag(result, "source-4.js"));
            Assert.AreEqual(2, htmlDocument.DocumentNode.Descendants("script").Count());
        }

        [TestMethod]
        public void Bundler_BundleHtml_Removes_Blank_Lines_And_Comments_In_Bundle()
        {
            _Bundler.AttachToWebSite(_WebSite.Object);

            var html = "<HTML><HEAD>\r\n" +
                       "    <link rel=\"stylesheet\" href=\"beforeBundle.css\" type=\"text/css\" media=\"screen\" />\r\n" +
                       "    <!-- [[ JS BUNDLE START ]] -->\r\n" +
                       "    <!-- This is a plain comment -->\r\n" +
                       "\r\n" +
                       "    <SCRIPT SRC=\"source-1.js\" TYPE=\"text/javascript\"></SCRIPT>\r\n" +
                       "    <SCRIPT SRC=\"source-2.js\" TYPE=\"text/javascript\"></SCRIPT>\r\n" +
                       "    <!-- [[ BUNDLE END ]] -->\r\n" +
                       "    <link rel=\"stylesheet\" href=\"afterBundle.css\" type=\"text/css\" media=\"screen\" />\r\n" +
                       "</HEAD></HTML>";
            var result = _Bundler.BundleHtml("/index.html", html);

            var document = new HtmlDocument();
            document.LoadHtml(result);
            var node = document.DocumentNode.Descendants("link").First();
            node = node.NextSibling; Assert.AreEqual("\r\n    ", node.InnerText);
            node = node.NextSibling; Assert.AreEqual("script", node.Name);
            node = node.NextSibling; Assert.AreEqual("\r\n    ", node.InnerText);
            node = node.NextSibling; Assert.AreEqual("link", node.Name);
        }

        [TestMethod]
        public void Bundler_BundleHtml_Only_Replaces_Javascript_Tags()
        {
            _Bundler.AttachToWebSite(_WebSite.Object);

            var html = @"<HTML><HEAD>" +
                       @"<!-- [[ JS BUNDLE START ]] -->" +
                       @"<SCRIPT SRC=""source-1.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<SCRIPT SRC=""source-2.js"" TYPE=""text/ecma""></SCRIPT>" +
                       @"<SCRIPT SRC=""source-3.js""></SCRIPT>" +
                       @"<SCRIPT SRC=""source-4.js"" TYPE=""TEXT/JAVASCRIPT""></SCRIPT>" +
                       @"<LINK REL=""stylesheet"" HREF=""source-1.css"" TYPE=""text/css"" MEDIA=""screen"" />" +
                       @"<!-- [[ BUNDLE END ]] -->" +
                       @"</HEAD></HTML>";
            var result = _Bundler.BundleHtml("/index.html", html);

            Assert.IsFalse(ContainsScriptTag(result, "source-1.js"));
            Assert.IsTrue (ContainsScriptTag(result, "source-2.js"));
            Assert.IsTrue (ContainsScriptTag(result, "source-3.js"));
            Assert.IsFalse(ContainsScriptTag(result, "source-4.js"));
            Assert.IsTrue (ContainsCssTag(result, "source-1.css"));
        }

        [TestMethod]
        public void Bundler_BundleHtml_Only_Replaces_Local_Javascript_Tags()
        {
            _Bundler.AttachToWebSite(_WebSite.Object);

            var html = @"<HTML><HEAD>" +
                       @"<!-- [[ JS BUNDLE START ]] -->" +
                       @"<SCRIPT SRC=""source-1.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<SCRIPT SRC=""folder/source-2.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<SCRIPT SRC=""/root/folder/source-3.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<SCRIPT SRC=""http://remote.com/source-4.js"" TYPE=""TEXT/JAVASCRIPT""></SCRIPT>" +
                       @"<SCRIPT SRC=""https://remote.com/source-5.js"" TYPE=""TEXT/JAVASCRIPT""></SCRIPT>" +
                       @"<!-- [[ BUNDLE END ]] -->" +
                       @"</HEAD></HTML>";
            var result = _Bundler.BundleHtml("/index.html", html);

            Assert.IsFalse(ContainsScriptTag(result, "source-1.js"));
            Assert.IsFalse(ContainsScriptTag(result, "folder/source-2.js"));
            Assert.IsFalse(ContainsScriptTag(result, "/root/folder/source-3.js"));
            Assert.IsTrue (ContainsScriptTag(result, "http://remote.com/source-4.js"));
            Assert.IsTrue (ContainsScriptTag(result, "https://remote.com/source-5.js"));
        }

        [TestMethod]
        public void Bundler_BundleHtml_Multiple_Requests_For_The_Same_Bundled_Files_Result_In_The_Same_Bundle()
        {
            _Bundler.AttachToWebSite(_WebSite.Object);

            var html = @"<HTML><HEAD>" +
                       @"<!-- [[ JS BUNDLE START ]] -->" +
                       @"<SCRIPT SRC=""source-1.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<SCRIPT SRC=""source-2.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<!-- [[ BUNDLE END ]] -->" +
                       @"</HEAD></HTML>";

            var firstResult = _Bundler.BundleHtml("/index.html", html);
            var secondResult = _Bundler.BundleHtml("/other.html", html);

            var firstRequest = GetScriptAddress(firstResult);
            var secondRequest = GetScriptAddress(secondResult);

            Assert.AreEqual(firstRequest, secondRequest);
        }

        [TestMethod]
        public void Bundler_BundleHtml_Multiple_Requests_For_The_Same_Bundled_Files_In_Different_Folders_Result_In_Different_Bundles()
        {
            _Bundler.AttachToWebSite(_WebSite.Object);

            var html = @"<HTML><HEAD>" +
                       @"<!-- [[ JS BUNDLE START ]] -->" +
                       @"<SCRIPT SRC=""source-1.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<SCRIPT SRC=""source-2.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<!-- [[ BUNDLE END ]] -->" +
                       @"</HEAD></HTML>";

            var firstResult = _Bundler.BundleHtml("/index.html", html);
            var secondResult = _Bundler.BundleHtml("/subfolder/other.html", html);

            var firstRequest = GetScriptAddress(firstResult);
            var secondRequest = GetScriptAddress(secondResult);

            Assert.AreNotEqual(firstRequest, secondRequest);
        }

        [TestMethod]
        public void Bundler_BundleHtml_Stops_If_The_End_Tag_Is_Malformed()
        {
            _Bundler.AttachToWebSite(_WebSite.Object);

            var html = @"<HTML><HEAD>" +
                       @"<!-- [[ JS BUNDLE START ]] -->" +
                       @"<SCRIPT SRC=""source-1.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<SCRIPT SRC=""source-2.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<!--  [[ BUNDLE END ]] -->" +  // Too many leading spaces - the syntax has to be **EXACT**
                       @"</HEAD></HTML>";
            var result = _Bundler.BundleHtml("/index.html", html);

            Assert.IsTrue(ContainsScriptTag(result, "source-2.js"));

            var document = new HtmlDocument();
            document.LoadHtml(result);
            var comments = document.DocumentNode.Descendants().Where(r => r.NodeType == HtmlNodeType.Comment).OfType<HtmlCommentNode>();
            Assert.IsTrue(comments.Any(r => r.Comment.Contains("BUNDLE PARSER ERROR")));
        }

        [TestMethod]
        public void Bundler_BundleHtml_Stops_If_End_Tag_Has_No_Start_Tag()
        {
            _Bundler.AttachToWebSite(_WebSite.Object);

            var html = @"<HTML><HEAD>" +
                       @"<!--  [[ JS BUNDLE START ]] -->" +
                       @"<SCRIPT SRC=""source-1.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<SCRIPT SRC=""source-2.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<!-- [[ BUNDLE END ]] -->" +
                       @"</HEAD></HTML>";
            var result = _Bundler.BundleHtml("/index.html", html);

            Assert.IsTrue(ContainsScriptTag(result, "source-2.js"));

            var document = new HtmlDocument();
            document.LoadHtml(result);
            var comments = document.DocumentNode.Descendants().Where(r => r.NodeType == HtmlNodeType.Comment).OfType<HtmlCommentNode>();
            Assert.IsTrue(comments.Any(r => r.Comment.Contains("BUNDLE PARSER ERROR")));
        }

        [TestMethod]
        public void Bundler_BundleHtml_Stops_If_Path_To_File_Leaves_Site()
        {
            _Bundler.AttachToWebSite(_WebSite.Object);

            var html = @"<HTML><HEAD>" +
                       @"<!-- [[ JS BUNDLE START ]] -->" +
                       @"<SCRIPT SRC=""../source-1.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<!-- [[ BUNDLE END ]] -->" +
                       @"</HEAD></HTML>";
            var result = _Bundler.BundleHtml("/index.html", html);

            Assert.IsTrue(ContainsScriptTag(result, "../source-1.js"));

            var document = new HtmlDocument();
            document.LoadHtml(result);
            var comments = document.DocumentNode.Descendants().Where(r => r.NodeType == HtmlNodeType.Comment).OfType<HtmlCommentNode>();
            Assert.IsTrue(comments.Any(r => r.Comment.Contains("BUNDLE PARSER ERROR")));
        }

        [TestMethod]
        public void Bundler_BundleHtml_Stops_If_Tags_Are_Nested()
        {
            _Bundler.AttachToWebSite(_WebSite.Object);

            var html = @"<HTML><HEAD>" +
                       @"<!-- [[ JS BUNDLE START ]] -->" +
                       @"<SCRIPT SRC=""source-1.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<!-- [[ JS BUNDLE START ]] -->" +
                       @"<SCRIPT SRC=""source-2.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<!-- [[ BUNDLE END ]] -->" +
                       @"<!-- [[ BUNDLE END ]] -->" +
                       @"</HEAD></HTML>";
            var result = _Bundler.BundleHtml("/index.html", html);

            Assert.IsTrue(ContainsScriptTag(result, "source-1.js"));
            Assert.IsTrue(ContainsScriptTag(result, "source-2.js"));

            var document = new HtmlDocument();
            document.LoadHtml(result);
            var comments = document.DocumentNode.Descendants().Where(r => r.NodeType == HtmlNodeType.Comment).OfType<HtmlCommentNode>();
            Assert.IsTrue(comments.Any(r => r.Comment.Contains("BUNDLE PARSER ERROR")));
        }
        #endregion

        #region BundleHtml - Serve bundle
        [TestMethod]
        public void Bundler_BundleHtml_Serves_Concatenated_Javascript()
        {
            AddPathAndFileContent("/source-1.js", "CONTENT OF FIRST FILE");
            AddPathAndFileContent("/source-2.js", "CONTENT OF SECOND FILE");

            // The extra semi-colons were necessary because the minifier strips off trailing semi-colons
            // at the end of the file which can cause some standalone JS to break when more JS is
            // appended to it in the bundle.
            var expectedContent = "/* /source-1.js */\r\n" +
                                  "CONTENT OF FIRST FILE;\r\n" +
                                  "/* /source-2.js */\r\n" +
                                  "CONTENT OF SECOND FILE;\r\n";

            _Bundler.AttachToWebSite(_WebSite.Object);
            var html = @"<HTML><HEAD>" +
                       @"<!-- [[ JS BUNDLE START ]] -->" +
                       @"<SCRIPT SRC=""source-1.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<SCRIPT SRC=""source-2.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<!-- [[ BUNDLE END ]] -->" +
                       @"</HEAD></HTML>";
            var result = _Bundler.BundleHtml("/index.html", html);

            var request = GetScriptAddress(result);
            var args = SendRequest(request);

            Assert.AreEqual(HttpStatusCode.OK, _Response.Object.StatusCode);
            Assert.AreEqual("application/javascript", _Response.Object.MimeType);
            Assert.AreEqual(ContentClassification.Other, args.Classification);
            Assert.AreEqual(true, args.Handled);
            Assert.AreEqual(expectedContent, GetStringContent());
        }

        [TestMethod]
        public void Bundler_BundleHtml_Minifies_JavaScript()
        {
            AddPathAndFileContent("/source-1.js", "CONTENT OF FIRST FILE");
            AddPathAndFileContent("/source-2.js", "CONTENT OF SECOND FILE");

            _Bundler.AttachToWebSite(_WebSite.Object);
            var html = @"<HTML><HEAD>" +
                       @"<!-- [[ JS BUNDLE START ]] -->" +
                       @"<SCRIPT SRC=""source-1.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<SCRIPT SRC=""source-2.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<!-- [[ BUNDLE END ]] -->" +
                       @"</HEAD></HTML>";
            var result = _Bundler.BundleHtml("/index.html", html);
            var request = GetScriptAddress(result);
            var args = SendRequest(request);

            _Minifier.Verify(r => r.MinifyJavaScript(It.IsAny<string>()), Times.Exactly(2));    // One call per file served.
        }

        [TestMethod]
        public void Bundler_BundleHtml_Compresses_JavaScript()
        {
            AddPathAndFileContent("/source-1.js", "CONTENT OF FIRST FILE");
            AddPathAndFileContent("/source-2.js", "CONTENT OF SECOND FILE");

            _Bundler.AttachToWebSite(_WebSite.Object);
            var html = @"<HTML><HEAD>" +
                       @"<!-- [[ JS BUNDLE START ]] -->" +
                       @"<SCRIPT SRC=""source-1.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<SCRIPT SRC=""source-2.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<!-- [[ BUNDLE END ]] -->" +
                       @"</HEAD></HTML>";
            var result = _Bundler.BundleHtml("/index.html", html);
            var request = GetScriptAddress(result);
            var args = SendRequest(request);

            _Response.Verify(r => r.EnableCompression(_Request.Object), Times.Once());
        }

        [TestMethod]
        public void Bundler_BundleHtml_Sends_UTF8()
        {
            AddPathAndFileContent("/source-1.js", "CONTENT OF FIRST FILE");
            AddPathAndFileContent("/source-2.js", "CONTENT OF SECOND FILE");

            _Bundler.AttachToWebSite(_WebSite.Object);
            var html = @"<HTML><HEAD>" +
                       @"<!-- [[ JS BUNDLE START ]] -->" +
                       @"<SCRIPT SRC=""source-1.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<SCRIPT SRC=""source-2.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<!-- [[ BUNDLE END ]] -->" +
                       @"</HEAD></HTML>";
            var result = _Bundler.BundleHtml("/index.html", html);

            var request = GetScriptAddress(result);
            var args = SendRequest(request);

            var expectedPreamble = Encoding.UTF8.GetPreamble();
            var actualPreamble = _OutputStream.ToArray().Take(expectedPreamble.Length).ToArray();

            Assert.AreEqual("Unicode (UTF-8)", args.Response.ContentEncoding.EncodingName);
            Assert.IsTrue(expectedPreamble.SequenceEqual(actualPreamble));
        }

        [TestMethod]
        public void Bundler_BundleHtml_Can_Cope_With_Relative_Paths_To_Scripts()
        {
            AddPathAndFileContent("/script/source-1.js", "CONTENT OF FIRST FILE");
            AddPathAndFileContent("/source-2.js", "CONTENT OF SECOND FILE");
            var expectedContent = "/* /script/source-1.js */\r\n" +
                                  "CONTENT OF FIRST FILE;\r\n" +
                                  "/* /source-2.js */\r\n" +
                                  "CONTENT OF SECOND FILE;\r\n";

            _Bundler.AttachToWebSite(_WebSite.Object);
            var html = @"<HTML><HEAD>" +
                       @"<!-- [[ JS BUNDLE START ]] -->" +
                       @"<SCRIPT SRC=""script/source-1.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<SCRIPT SRC=""script/../source-2.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<!-- [[ BUNDLE END ]] -->" +
                       @"</HEAD></HTML>";
            var result = _Bundler.BundleHtml("/index.html", html);

            var request = GetScriptAddress(result);
            var args = SendRequest(request);

            Assert.AreEqual(HttpStatusCode.OK, _Response.Object.StatusCode);
            Assert.AreEqual("application/javascript", _Response.Object.MimeType);
            Assert.AreEqual(ContentClassification.Other, args.Classification);
            Assert.AreEqual(true, args.Handled);
            Assert.AreEqual(expectedContent, GetStringContent());
        }

        [TestMethod]
        public void Bundler_BundleHtml_Reports_Failure_To_Retrive_Bundle_Files()
        {
            AddPathAndFileContent("/script/source-1.js", "CONTENT OF FIRST FILE");
            var expectedContent = "/* /script/source-1.js */\r\n" +
                                  "CONTENT OF FIRST FILE;\r\n" +
                                  "/* /script/source-2.js */\r\n" +
                                  "/* Status 404 (NotFound) */\r\n";

            _Bundler.AttachToWebSite(_WebSite.Object);
            var html = @"<HTML><HEAD>" +
                       @"<!-- [[ JS BUNDLE START ]] -->" +
                       @"<SCRIPT SRC=""../script/source-1.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<SCRIPT SRC=""../script/source-2.js"" TYPE=""text/javascript""></SCRIPT>" +
                       @"<!-- [[ BUNDLE END ]] -->" +
                       @"</HEAD></HTML>";
            var result = _Bundler.BundleHtml("/html/index.html", html);

            var request = GetScriptAddress(result);
            var args = SendRequest(request);

            Assert.AreEqual(HttpStatusCode.OK, _Response.Object.StatusCode);
            Assert.AreEqual("application/javascript", _Response.Object.MimeType);
            Assert.AreEqual(ContentClassification.Other, args.Classification);
            Assert.AreEqual(true, args.Handled);
            Assert.AreEqual(expectedContent, GetStringContent());
        }
        #endregion
    }
}
