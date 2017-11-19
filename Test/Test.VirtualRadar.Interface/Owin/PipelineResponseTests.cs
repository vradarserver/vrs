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
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.WebServer;

namespace Test.VirtualRadar.Interface.Owin
{
    [TestClass]
    public class PipelineResponseTests
    {
        public TestContext TestContext { get; set; }

        private Dictionary<string, object> _Environment;
        private PipelineResponse _Response;

        [TestInitialize]
        public void TestInitialise()
        {
            _Environment = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            _Response = new PipelineResponse(_Environment);
            _Environment["owin.ResponseHeaders"] = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        }

        private void SetContentType(string contentType)
        {
            var headers = (Dictionary<string, string[]>)_Environment["owin.ResponseHeaders"];
            if(headers.ContainsKey("Content-Type")) {
                headers["Content-Type"] = new string[] { contentType };
            } else {
                headers.Add("Content-Type", new string[] { contentType });
            }
        }

        [TestMethod]
        public void PipelineResponse_Constructor_Initialises_To_Known_State()
        {
            Assert.AreSame(_Environment, _Response.Environment);

            var defaultCtor = new PipelineResponse();
            Assert.IsNotNull(defaultCtor.Environment);
        }

        [TestMethod]
        public void PipelineResponse_IsJavascriptContentType_Returns_True_For_Javascript_Mime_Types()
        {
            Assert.IsFalse(_Response.IsJavascriptContentType);

            foreach(var mimeType in new string[] { MimeType.Javascript, "application/javascript", "application/ecmascript", "text/javascript", "text/ecmascript" }) {
                SetContentType(mimeType);
                Assert.IsTrue(_Response.IsJavascriptContentType);
                SetContentType(mimeType.ToLower());
                Assert.IsTrue(_Response.IsJavascriptContentType);
                SetContentType(mimeType.ToUpper());
                Assert.IsTrue(_Response.IsJavascriptContentType);
            }
        }

        [TestMethod]
        public void PipelineResponse_IsHtmlContentType_Returns_True_For_Html_Mime_Types()
        {
            Assert.IsFalse(_Response.IsHtmlContentType);

            foreach(var mimeType in new string[] { MimeType.Html, "text/html", "application/xhtml+xml" }) {
                SetContentType(mimeType);
                Assert.IsTrue(_Response.IsHtmlContentType);
                SetContentType(mimeType.ToLower());
                Assert.IsTrue(_Response.IsHtmlContentType);
                SetContentType(mimeType.ToUpper());
                Assert.IsTrue(_Response.IsHtmlContentType);
            }
        }
    }
}
