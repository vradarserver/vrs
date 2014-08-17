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
using System.Net;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class CidrTests
    {
        #region TestContext, Fields, TestInitialise
        public TestContext TestContext { get; set; }
        #endregion

        #region GetExcelIPAddress
        private IPAddress GetExcelIPAddress(ExcelWorksheetData worksheet, string columnName)
        {
            var expected = worksheet.String(columnName);
            var result = IPAddress.Parse(expected);

            return result;
        }
        #endregion

        #region Constructor
        [TestMethod]
        public void Cidr_Default_Constructor_Initialises_To_Known_State()
        {
            var cidr = new Cidr();
            Assert.AreEqual(IPAddress.None, cidr.Address);
            Assert.AreEqual(IPAddress.Parse("0.0.0.0"), cidr.MaskedAddress);
            Assert.AreEqual(0, cidr.BitmaskBits);
            Assert.AreEqual((uint)0, cidr.IPv4Bitmask);
        }
        #endregion

        #region Parse
        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "CidrParse$")]
        public void Cidr_Parse_Parses_Addresses_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            // Uncomment this block if you want to restrict the test to just those rows with a RunThis of true
            // var runThis = worksheet.NBool("RunThis").GetValueOrDefault();
            // if(!runThis) Assert.Fail();

            var address = worksheet.EString("Parse");
            Exception exception = null;
            Cidr cidr = null;
            try {
                cidr = Cidr.Parse(address);
            } catch(Exception ex) {
                exception = ex;
            }

            Assert.AreEqual(exception != null, worksheet.NBool("Exception").GetValueOrDefault(), exception == null ? "Did not see exception" : exception.Message);
            if(exception == null) {
                Assert.AreEqual(GetExcelIPAddress(worksheet, "Address"), cidr.Address);
                Assert.AreEqual(GetExcelIPAddress(worksheet, "MaskedAddress"), cidr.MaskedAddress);
                Assert.AreEqual(worksheet.Int("BitmaskBits"), cidr.BitmaskBits);
                Assert.AreEqual(worksheet.UInt("IPv4Bitmask"), cidr.IPv4Bitmask);
                Assert.AreEqual(GetExcelIPAddress(worksheet, "From"), cidr.FirstMatchingAddress);
                Assert.AreEqual(GetExcelIPAddress(worksheet, "To"), cidr.LastMatchingAddress);
            }
        }
        #endregion

        #region TryParse
        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "CidrParse$")]
        public void Cidr_TryParse_Parses_Addresses_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            Cidr cidr;
            var address = worksheet.EString("Parse");
            var parsed = Cidr.TryParse(address, out cidr);

            Assert.AreEqual(!parsed, worksheet.NBool("Exception").GetValueOrDefault());
            if(parsed) {
                Assert.AreEqual(GetExcelIPAddress(worksheet, "Address"), cidr.Address);
                Assert.AreEqual(GetExcelIPAddress(worksheet, "MaskedAddress"), cidr.MaskedAddress);
                Assert.AreEqual(worksheet.Int("BitmaskBits"), cidr.BitmaskBits);
                Assert.AreEqual(worksheet.UInt("IPv4Bitmask"), cidr.IPv4Bitmask);
            }
        }
        #endregion

        #region Equals, GetHashCode
        [TestMethod]
        public void Cidr_Equals_Returns_True_When_Properties_Match()
        {
            var obj1 = Cidr.Parse("1.2.3.4/32");
            var obj2 = Cidr.Parse("1.2.3.4/32");
            var obj3 = Cidr.Parse("5.6.7.8/32");
            var obj4 = Cidr.Parse("1.2.3.4/31");

            Assert.IsTrue(obj1.Equals(obj1));
            Assert.IsTrue(obj1.Equals(obj2));

            Assert.IsFalse(obj1.Equals(null));
            Assert.IsFalse(obj1.Equals(obj3));
            Assert.IsFalse(obj1.Equals(obj4));
        }

        [TestMethod]
        public void Cidr_GetHashCode_Returns_Same_Value_For_Equals_CIDRs()
        {
            var obj1 = Cidr.Parse("1.2.3.4/32");
            var obj2 = Cidr.Parse("1.2.3.4/32");

            Assert.AreEqual(obj1.GetHashCode(), obj2.GetHashCode());
        }
        #endregion

        #region Matches
        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "CidrMatches$")]
        public void Cidr_Matches_Compares_Addresses_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            // Uncomment this block if you want to restrict the test to just those rows with a RunThis of true
            // var runThis = worksheet.NBool("RunThis").GetValueOrDefault();
            // if(!runThis) Assert.Fail();

            var cidr = Cidr.Parse(worksheet.String("CIDR"));
            var address = GetExcelIPAddress(worksheet, "Address");

            var expected = worksheet.Bool("Result");
            var actual = cidr.Matches(address);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Cidr_Matches_Returns_False_If_Passed_Null()
        {
            var cidr = new Cidr();
            Assert.AreEqual(false, cidr.Matches(null));
        }

        [TestMethod]
        public void Cidr_Matches_Returns_False_If_Passed_An_IPv6()
        {
            var cidr = new Cidr();
            Assert.AreEqual(false, cidr.Matches(IPAddress.Parse("2001:0DB8:AC10:FE01::")));
        }
        #endregion
    }
}
