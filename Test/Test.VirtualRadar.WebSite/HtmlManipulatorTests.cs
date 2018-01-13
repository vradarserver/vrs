// Copyright © 2018 onwards, Andrew Whewell
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
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.WebSite;

namespace Test.VirtualRadar.WebSite
{
    [TestClass]
    public class HtmlManipulatorTests
    {
        public TestContext TestContext { get; set; }
        private HtmlManipulator _HtmlManipulator;
        private Dictionary<string, object> _Environment;
        private string _IndexHtml = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <!-- Fonts -->
    <title></title>
    <script src=""script/vrs/event.js"" type=""text/javascript""></script>
    <!-- [[ BUNDLE END ]] -->
</head>
<body>
    <h1></h1>
    <div></div>
    <div></div>
</body>
</html>
";

        [TestInitialize]
        public void TestInitialise()
        {
            _HtmlManipulator = new HtmlManipulator();
            _Environment = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        private void ConfigureRequest(string path)
        {
            _Environment.Add("owin.RequestPath", path);
        }

        private TextContent GenerateTextContent(string content, Encoding encoding = null, bool hadPreamble = false)
        {
            encoding = encoding ?? Encoding.UTF8;

            var result = new TextContent() {
                Content =       content,
                Encoding =      encoding,
                HadPreamble =   hadPreamble,
            };
            result.IsDirty = false;

            return result;
        }

        #region AddHtmlContentInjector
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void HtmlManipulator_AddHtmlContentInjector_Throws_If_Passed_Null()
        {
            _HtmlManipulator.AddHtmlContentInjector(null);
        }
        #endregion

        #region ManipulateTextResponse
        [TestMethod]
        public void HtmlManipulator_ManipulateTextResponse_Injects_Content_Into_HTML_At_Start_Of_Element()
        {
            var path = "/index.html";

            _HtmlManipulator.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = true,
                Element = "head",
                PathAndFile = path,
                Content = () => "<script src=\"Hello\"></script><script src=\"There\"></script>"
            });

            var textContent = GenerateTextContent(_IndexHtml);
            ConfigureRequest(path);
            _HtmlManipulator.ManipulateTextResponse(_Environment, textContent);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(textContent.Content);

            Assert.IsTrue(textContent.IsDirty);

            var head = htmlDocument.DocumentNode.Descendants("head").Single();
            var firstChild = head.FirstChild;
            Assert.AreEqual("script", firstChild.Name);
            Assert.AreEqual("Hello", firstChild.GetAttributeValue("src", null));

            var nextChild = firstChild.NextSibling;
            Assert.AreEqual("script", nextChild.Name);
            Assert.AreEqual("There", nextChild.GetAttributeValue("src", null));
        }

        [TestMethod]
        public void HtmlManipulator_ManipulateTextResponse_Does_Not_Inject_Content_Into_Other_Html_Files()
        {
            var path = "/index.html";

            _HtmlManipulator.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = true,
                Element = "head",
                PathAndFile = path,
                Content = () => "<script src=\"Hello\"></script><script src=\"There\"></script>"
            });

            var textContent = GenerateTextContent(_IndexHtml);
            ConfigureRequest("/not-index.html");
            _HtmlManipulator.ManipulateTextResponse(_Environment, textContent);

            Assert.IsFalse(textContent.IsDirty);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(textContent.Content);

            var head = htmlDocument.DocumentNode.Descendants("head").Single();
            var firstChild = head.FirstChild;
            Assert.AreNotEqual("Hello", firstChild.GetAttributeValue("src", null));
        }

        [TestMethod]
        public void HtmlManipulator_ManipulateTextResponse_Injects_Content_Regardless_Of_PathAndFile_Case()
        {
            var path = "/index.html";

            _HtmlManipulator.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = true,
                Element = "head",
                PathAndFile = path,
                Content = () => "<script src=\"Hello\"></script>"
            });

            var textContent = GenerateTextContent(_IndexHtml);
            ConfigureRequest("/InDEX.HTml");
            _HtmlManipulator.ManipulateTextResponse(_Environment, textContent);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(textContent.Content);

            var node = htmlDocument.DocumentNode.Descendants("head").Single().FirstChild;
            Assert.AreEqual("Hello", node.GetAttributeValue("src", null));
        }

        [TestMethod]
        public void HtmlManipulator_ManipulateTextResponse_Injects_Content_Regardless_Of_Element_Case()
        {
            var path = "/index.html";
            var headIsLowerCase = _IndexHtml.Contains("<head>");

            _HtmlManipulator.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = true,
                Element = headIsLowerCase ? "HEAD" : "head",
                PathAndFile = path,
                Content = () => "<script src=\"Hello\"></script>"
            });

            var textContent = GenerateTextContent(_IndexHtml);
            ConfigureRequest(path);
            _HtmlManipulator.ManipulateTextResponse(_Environment, textContent);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(textContent.Content);

            var node = htmlDocument.DocumentNode.Descendants("head").Single().FirstChild;
            Assert.AreEqual("Hello", node.GetAttributeValue("src", null));
        }

        [TestMethod]
        public void HtmlManipulator_ManipulateTextResponse_Null_PathAndFile_Injects_Content_Into_Every_Html_File()
        {
            var path = "/index.html";

            _HtmlManipulator.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = true,
                Element = "head",
                PathAndFile = null,
                Content = () => "<script src=\"Hello\"></script><script src=\"There\"></script>"
            });

            var textContent = GenerateTextContent(_IndexHtml);
            ConfigureRequest(path);
            _HtmlManipulator.ManipulateTextResponse(_Environment, textContent);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(textContent.Content);

            var head = htmlDocument.DocumentNode.Descendants("head").Single();
            var firstChild = head.FirstChild;
            Assert.AreEqual("Hello", firstChild.GetAttributeValue("src", null));
        }

        [TestMethod]
        public void HtmlManipulator_ManipulateTextResponse_Injects_Content_Into_HTML_At_End_Of_Element()
        {
            var path = "/index.html";

            _HtmlManipulator.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = false,
                Element = "body",
                PathAndFile = path,
                Content = () => "<div>Hello, this was injected</div><div>And so was this</div>"
            });

            var textContent = GenerateTextContent(_IndexHtml);
            ConfigureRequest(path);
            _HtmlManipulator.ManipulateTextResponse(_Environment, textContent);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(textContent.Content);

            var body = htmlDocument.DocumentNode.Descendants("body").Single();
            var lastChild = body.ChildNodes.Where(r => r.Name == "div").Last();
            Assert.AreEqual("And so was this", lastChild.InnerText);

            var previous = lastChild.PreviousSibling;
            Assert.AreEqual("div", previous.Name);
            Assert.AreEqual("Hello, this was injected", previous.InnerText);
        }

        [TestMethod]
        public void HtmlManipulator_ManipulateTextResponse_Injects_Into_First_Element_When_Multiple_Are_Present_And_AtStart_Is_True()
        {
            var path = "/index.html";

            _HtmlManipulator.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = true,
                Element = "div",
                PathAndFile = path,
                Content = () => "Hello"
            });

            var textContent = GenerateTextContent(_IndexHtml);
            ConfigureRequest(path);
            _HtmlManipulator.ManipulateTextResponse(_Environment, textContent);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(textContent.Content);

            var node = htmlDocument.DocumentNode.Descendants("div").First();
            var injected = node.FirstChild;
            Assert.AreEqual("#text", injected.Name);
            Assert.AreEqual("Hello", injected.InnerText);
        }

        [TestMethod]
        public void HtmlManipulator_ManipulateTextResponse_Injects_Into_Last_Element_When_Multiple_Are_Present_And_AtStart_Is_False()
        {
            var path = "/index.html";

            _HtmlManipulator.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = false,
                Element = "div",
                PathAndFile = path,
                Content = () => "Hello"
            });

            var textContent = GenerateTextContent(_IndexHtml);
            ConfigureRequest(path);
            _HtmlManipulator.ManipulateTextResponse(_Environment, textContent);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(textContent.Content);

            var node = htmlDocument.DocumentNode.Descendants("div").Last();
            var injected = node.LastChild;
            Assert.AreEqual("#text", injected.Name);
            Assert.AreEqual("Hello", injected.InnerText);
        }

        [TestMethod]
        public void HtmlManipulator_ManipulateTextResponse_Does_Nothing_When_Element_Does_Not_Exist()
        {
            var path = "/index.html";

            _HtmlManipulator.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = false,
                Element = "IMadeThisUp",
                PathAndFile = path,
                Content = () => "Hello"
            });

            var textContent = GenerateTextContent(_IndexHtml);
            ConfigureRequest(path);
            _HtmlManipulator.ManipulateTextResponse(_Environment, textContent);

            Assert.AreEqual(false, textContent.IsDirty);
            Assert.AreEqual(_IndexHtml, textContent.Content);
        }

        [TestMethod]
        public void HtmlManipulator_ManipulateTextResponse_Does_Nothing_When_Element_Is_Null()
        {
            var path = "/index.html";

            _HtmlManipulator.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = false,
                Element = null,
                PathAndFile = path,
                Content = () => "Hello"
            });

            var textContent = GenerateTextContent(_IndexHtml);
            ConfigureRequest(path);
            _HtmlManipulator.ManipulateTextResponse(_Environment, textContent);

            Assert.AreEqual(false, textContent.IsDirty);
            Assert.AreEqual(_IndexHtml, textContent.Content);
        }

        [TestMethod]
        public void HtmlManipulator_ManipulateTextResponse_Does_Nothing_When_Element_Is_Empty_String()
        {
            var path = "/index.html";

            _HtmlManipulator.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = false,
                Element = "",
                PathAndFile = path,
                Content = () => "Hello"
            });

            var textContent = GenerateTextContent(_IndexHtml);
            ConfigureRequest(path);
            _HtmlManipulator.ManipulateTextResponse(_Environment, textContent);

            Assert.AreEqual(false, textContent.IsDirty);
            Assert.AreEqual(_IndexHtml, textContent.Content);
        }

        [TestMethod]
        public void HtmlManipulator_ManipulateTextResponse_Does_Nothing_When_Content_Is_Null()
        {
            var path = "/index.html";

            _HtmlManipulator.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = false,
                Element = "div",
                PathAndFile = path,
                Content = null
            });

            var textContent = GenerateTextContent(_IndexHtml);
            ConfigureRequest(path);
            _HtmlManipulator.ManipulateTextResponse(_Environment, textContent);

            Assert.AreEqual(false, textContent.IsDirty);
            Assert.AreEqual(_IndexHtml, textContent.Content);
        }

        [TestMethod]
        public void HtmlManipulator_ManipulateTextResponse_Does_Nothing_When_Content_Returns_Null()
        {
            var path = "/index.html";

            _HtmlManipulator.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = false,
                Element = "div",
                PathAndFile = path,
                Content = () => null
            });

            var textContent = GenerateTextContent(_IndexHtml);
            ConfigureRequest(path);
            _HtmlManipulator.ManipulateTextResponse(_Environment, textContent);

            Assert.AreEqual(false, textContent.IsDirty);
            Assert.AreEqual(_IndexHtml, textContent.Content);
        }

        [TestMethod]
        public void HtmlManipulator_ManipulateTextResponse_Does_Nothing_When_Content_Returns_Empty_String()
        {
            var path = "/index.html";

            _HtmlManipulator.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = false,
                Element = "div",
                PathAndFile = path,
                Content = () => ""
            });

            var textContent = GenerateTextContent(_IndexHtml);
            ConfigureRequest(path);
            _HtmlManipulator.ManipulateTextResponse(_Environment, textContent);

            Assert.AreEqual(false, textContent.IsDirty);
            Assert.AreEqual(_IndexHtml, textContent.Content);
        }

        [TestMethod]
        public void HtmlManipulator_ManipulateTextResponse_Calls_Multiple_AtStart_Injectors_In_Correct_Order()
        {
            var path = "/index.html";

            _HtmlManipulator.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = true,
                Element = "body",
                Priority = 1,
                PathAndFile = path,
                Content = () => "<p>First</p>"
            });

            _HtmlManipulator.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = true,
                Element = "body",
                Priority = 2,
                PathAndFile = path,
                Content = () => "<p>Second</p>"
            });

            var textContent = GenerateTextContent(_IndexHtml);
            ConfigureRequest(path);
            _HtmlManipulator.ManipulateTextResponse(_Environment, textContent);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(textContent.Content);

            var body = htmlDocument.DocumentNode.Descendants("body").Single();
            var first = body.FirstChild;
            var second = first.NextSibling;

            Assert.AreEqual("First", first.InnerText);
            Assert.AreEqual("Second", second.InnerText);
        }

        [TestMethod]
        public void HtmlManipulator_ManipulateTextResponse_Calls_Multiple_AtEnd_Injectors_In_Correct_Order()
        {
            var path = "/index.html";

            _HtmlManipulator.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = false,
                Element = "body",
                Priority = 1,
                PathAndFile = path,
                Content = () => "<p>First</p>"
            });

            _HtmlManipulator.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = false,
                Element = "body",
                Priority = 2,
                PathAndFile = path,
                Content = () => "<p>Second</p>"
            });

            var textContent = GenerateTextContent(_IndexHtml);
            ConfigureRequest(path);
            _HtmlManipulator.ManipulateTextResponse(_Environment, textContent);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(textContent.Content);

            var body = htmlDocument.DocumentNode.Descendants("body").Single();
            var last = body.LastChild;
            var secondToLast = last.PreviousSibling;

            Assert.AreEqual("First", last.InnerText);
            Assert.AreEqual("Second", secondToLast.InnerText);
        }
        #endregion

        #region RemoveHtmlContentInjector
        [TestMethod]
        public void HtmlManipulator_RemoveHtmlContentInjector_Prevents_Injector_From_Injecting_Content()
        {
            var path = "/index.html";

            var injector = new HtmlContentInjector() {
                AtStart = true,
                Element = "head",
                PathAndFile = path,
                Content = () => "<script src=\"Hello\"></script>"
            };

            _HtmlManipulator.AddHtmlContentInjector(injector);
            _HtmlManipulator.RemoveHtmlContentInjector(injector);

            var textContent = GenerateTextContent(_IndexHtml);
            ConfigureRequest(path);
            _HtmlManipulator.ManipulateTextResponse(_Environment, textContent);

            Assert.AreEqual(false, textContent.IsDirty);
            Assert.AreEqual(_IndexHtml, textContent.Content);
        }

        [TestMethod]
        public void HtmlManipulator_RemoveHtmlContentInjector_Does_Not_Care_If_Injector_Was_Never_Injected()
        {
            _HtmlManipulator.RemoveHtmlContentInjector(new HtmlContentInjector() {
                AtStart = true,
                Element = "head",
                PathAndFile = "/index.html",
                Content = () => "<script src=\"Hello\"></script>"
            });
        }

        [TestMethod]
        public void HtmlManipulator_RemoveHtmlContentInjector_Does_Not_Care_If_Injector_Is_Null()
        {
            _HtmlManipulator.RemoveHtmlContentInjector(null);
        }
        #endregion
    }
}
