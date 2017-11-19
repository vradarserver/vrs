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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;

namespace Test.VirtualRadar.Owin.StreamManipulator
{
    [TestClass]
    public class ManipulatorTests
    {
        public class TextManipulator : ITextResponseManipulator
        {
            public IDictionary<string, object> Environment { get; private set; }
            public TextContent TextContent { get; private set; }
            public int CallCount { get; private set; }
            public Action<IDictionary<string, object>, TextContent> Callback { get; set; }

            public void ManipulateTextResponse(IDictionary<string, object> environment, TextContent textContent)
            {
                Environment = environment;
                TextContent = textContent;
                ++CallCount;

                if(Callback != null) {
                    Callback(environment, textContent);
                }
            }
        }

        public TestContext TestContext { get; set; }
        private IClassFactory _Snapshot;
        protected MockOwinEnvironment _Environment;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();
            _Environment = new MockOwinEnvironment();

            ExtraInitialise();
        }

        protected virtual void ExtraInitialise()
        {
            ;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            ExtraCleanup();

            Factory.RestoreSnapshot(_Snapshot);
        }

        protected virtual void ExtraCleanup()
        {
            ;
        }

        protected void SetResponseContent(string mimeType, string body, Encoding encoding = null, bool addPreamble = false)
        {
            encoding = encoding ?? Encoding.UTF8;

            var preamble = !addPreamble ? new byte[0] : encoding.GetPreamble();
            var bodyBytes = encoding.GetBytes(body);

            _Environment.Response.ContentType = mimeType;
            _Environment.Response.ContentLength = preamble.Length + bodyBytes.Length;
            _Environment.Response.Body.Write(preamble, 0, preamble.Length);
            _Environment.Response.Body.Write(bodyBytes, 0, bodyBytes.Length);
        }

        protected TextContent GetResponseContent()
        {
            return TextContent.Load(_Environment.ResponseBodyBytes);
        }
    }
}
