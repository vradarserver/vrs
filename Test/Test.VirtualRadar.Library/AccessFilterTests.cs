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
using System.Net;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.Library
{
    [TestClass]
    public class AccessFilterTests
    {
        #region TestContext, Fields, TestInitialise
        public TestContext TestContext { get; set; }

        private IAccessFilter _Filter;
        private Access _Access;

        [TestInitialize]
        public void TestInitialise()
        {
            _Filter = Factory.Resolve<IAccessFilter>();
            _Access = new Access();
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }
        #endregion

        #region Ctors
        [TestMethod]
        public void AccessFilter_Constructor_Initialises_To_Known_Values()
        {
            Assert.IsNull(_Filter.Access);
        }
        #endregion

        #region Initialise
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AccessFilter_Initialise_Throws_If_Passed_Null()
        {
            _Filter.Initialise(null);
        }

        [TestMethod]
        public void AccessFilter_Initialise_Throws_If_Access_Contains_Garbage_Addresses()
        {
            _Access.Addresses.Add("This is garbage");
            
            // The exact type of exception is undefined, it depends on the address passed in
            var seenException = false;
            try {
                _Filter.Initialise(_Access);
            } catch {
                seenException = true;
            }

            Assert.IsTrue(seenException);
        }

        [TestMethod]
        public void AccessFilter_Initialise_Exposes_Access_Passed_Across()
        {
            _Filter.Initialise(_Access);
            Assert.AreEqual(_Access, _Filter.Access);
        }
        #endregion

        #region Allow
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AccessFilter_Allow_Throws_Exception_If_Initialise_Is_Not_Called_First()
        {
            _Filter.Allow(IPAddress.Parse("192.168.0.1"));
        }

        [TestMethod]
        public void AccessFilter_Allow_Returns_True_When_Unrestricted_And_No_Addresses()
        {
            _Access.DefaultAccess = DefaultAccess.Unrestricted;
            _Filter.Initialise(_Access);

            Assert.IsTrue(_Filter.Allow(IPAddress.Parse("192.168.0.1")));
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AllowFilter$")]
        public void AccessFilter_Allow_Behaves_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            // Uncomment this block to only run tests that have True in the RunThis column
            //var runThis = worksheet.Bool("RunThis");
            //if(!runThis) Assert.Fail();

            _Access.DefaultAccess = worksheet.ParseEnum<DefaultAccess>("DefaultAccess");
            for(var i = 1;i <= 2;++i) {
                var addressColumnName = String.Format("Address{0}", i);
                var address = worksheet.EString(addressColumnName);
                if(address != null) _Access.Addresses.Add(address);
                else                break;
            }
            _Filter.Initialise(_Access);

            var ipAddressText = worksheet.String("IPAddress");
            var ipAddress = IPAddress.Parse(ipAddressText);
            var allow = _Filter.Allow(ipAddress);

            var expected = worksheet.Bool("Allow");
            Assert.AreEqual(expected, allow);
        }
        #endregion
    }
}
