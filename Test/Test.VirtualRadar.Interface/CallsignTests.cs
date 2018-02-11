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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class CallsignTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "Callsign$")]
        public void Callsign_Ctor_Parses_Callsigns_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            var callsignText = worksheet.EString("Callsign");

            var callsign = new Callsign(callsignText);

            var message = $"Callsign is '{callsignText}'";
            Assert.AreEqual(worksheet.EString("OriginalCallsign"),          callsign.OriginalCallsign,          message);
            Assert.AreEqual(worksheet.EString("Code"),                      callsign.Code,                      message);
            Assert.AreEqual(worksheet.EString("Number"),                    callsign.Number,                    message);
            Assert.AreEqual(worksheet.EString("TrimmedNumber"),             callsign.TrimmedNumber,             message);
            Assert.AreEqual(worksheet.EString("TrimmedCallsign"),           callsign.TrimmedCallsign,           message);
            Assert.AreEqual(worksheet.Bool(   "IsOriginalCallsignValid"),   callsign.IsOriginalCallsignValid,   message);
        }
    }
}
