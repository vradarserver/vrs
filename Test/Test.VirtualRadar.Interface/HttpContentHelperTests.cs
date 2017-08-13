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
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class HttpContentHelperTests
    {
        [TestMethod]
        public void HttpContentHelper_FormUrlEncoded_Returns_FormUrlEncodedContent()
        {
            var content = HttpContentHelper.FormUrlEncoded(new [,] {
                { "key1", "value1" },
                { "key2", "value2" },
            });
            var text = content.ReadAsStringAsync().Result;

            Assert.AreEqual("key1=value1&key2=value2", text);
        }

        [TestMethod]
        public void HttpContentHelper_FormUrlEncoded_Sets_The_ContentType_Header()
        {
            var content = HttpContentHelper.FormUrlEncoded(new [,] { { "key1", "value1" } });
            Assert.AreEqual("application/x-www-form-urlencoded", content.Headers.ContentType.ToString());
        }

        [TestMethod]
        public void HttpContentHelper_FormUrlEncoded_Handles_Null_Value_Correctly()
        {
            var content = HttpContentHelper.FormUrlEncoded(new [,] { { "key", null } });
            var text = content.ReadAsStringAsync().Result;

            Assert.AreEqual("key=", text);
        }

        [TestMethod]
        public void HttpContentHelper_FormUrlEncoded_Handles_Empty_Value_Correctly()
        {
            var content = HttpContentHelper.FormUrlEncoded(new [,] { { "key", "" } });
            var text = content.ReadAsStringAsync().Result;

            Assert.AreEqual("key=", text);
        }

        [TestMethod]
        public void HttpContentHelper_FormUrlEncoded_Encodes_Keys()
        {
            var content = HttpContentHelper.FormUrlEncoded(new [,] { { "key&", "1" } });
            var text = content.ReadAsStringAsync().Result;

            Assert.AreEqual("key%26=1", text);
        }

        [TestMethod]
        public void HttpContentHelper_FormUrlEncoded_Encodes_Values()
        {
            var content = HttpContentHelper.FormUrlEncoded(new [,] { { "key", "&" } });
            var text = content.ReadAsStringAsync().Result;

            Assert.AreEqual("key=%26", text);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void HttpContentHelper_FormUrlEncoded_Throws_On_Null_Array()
        {
            HttpContentHelper.FormUrlEncoded(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void HttpContentHelper_FormUrlEncoded_Throws_On_Second_Dimension_Too_Small()
        {
            HttpContentHelper.FormUrlEncoded(new [,] { { "1" } });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void HttpContentHelper_FormUrlEncoded_Throws_On_Second_Dimension_Too_Large()
        {
            HttpContentHelper.FormUrlEncoded(new [,] { { "1", "2", "3" } });
        }
    }
}
