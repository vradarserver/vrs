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
using Test.Framework;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class JavascriptHelperTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "ToJavascriptTicks$")]
        public void JavascriptHelper_ToJavascriptTicks_Returns_Correct_Value_When_Passed_Ticks()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            Assert.AreEqual(worksheet.Long("JavascriptTicks"), JavascriptHelper.ToJavascriptTicks(worksheet.DateTime("Date").Ticks));
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "ToJavascriptTicks$")]
        public void JavascriptHelper_ToJavascriptTicks_Returns_Correct_Value_When_Passed_Date()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            Assert.AreEqual(worksheet.Long("JavascriptTicks"), JavascriptHelper.ToJavascriptTicks(worksheet.DateTime("Date")));
        }

        [TestMethod]
        public void JavascriptHelper_FormatStringLiteral_Returns_String_Surrounded_By_Single_Quotes()
        {
            Assert.AreEqual(@"'1'", JavascriptHelper.FormatStringLiteral("1"));
        }

        [TestMethod]
        public void JavascriptHelper_FormatStringLiteral_Escapes_Single_Quotes_In_String()
        {
            Assert.AreEqual(@"'p\'s and q\'s'", JavascriptHelper.FormatStringLiteral("p's and q's"));
        }

        [TestMethod]
        public void JavascriptHelper_FormatStringLiteral_Returns_Word_Null_If_Passed_Null()
        {
            Assert.AreEqual(@"null", JavascriptHelper.FormatStringLiteral(null));
        }
    }
}
