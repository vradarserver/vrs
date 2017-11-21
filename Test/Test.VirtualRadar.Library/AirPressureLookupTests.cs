// Copyright © 2016 onwards, Andrew Whewell
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
using Test.Framework;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Library
{
    [TestClass]
    public class AirPressureLookupTests
    {
        public TestContext TestContext { get; set; }

        private IAirPressureLookup _Lookup;
        private List<AirPressure> _AirPressures;
        private DateTime _Now;

        [TestInitialize]
        public void TestInitialise()
        {
            _Lookup = Factory.Singleton.ResolveNewInstance<IAirPressureLookup>();
            _AirPressures = new List<AirPressure>();
            _Now = DateTime.UtcNow;
        }

        [TestMethod]
        public void AirPressureLookup_FetchTimeUtc_Returns_Date_Passed_In_Last_Call_To_LoadAirPressures()
        {
            Assert.AreEqual(default(DateTime), _Lookup.FetchTimeUtc);

            _Lookup.LoadAirPressures(_AirPressures, _Now);
            Assert.AreEqual(_Now, _Lookup.FetchTimeUtc);

            _Now = _Now.AddSeconds(10);
            _Lookup.LoadAirPressures(_AirPressures, _Now);
            Assert.AreEqual(_Now, _Lookup.FetchTimeUtc);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AirPressureLookup_LoadAirPressures_Throws_If_Passed_Null()
        {
            _Lookup.LoadAirPressures(null, _Now);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AirPressureLookup$")]
        public void AirPressureLookup_FindClosest_Returns_Correct_AirPressure_Record()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            for(var i = 1;i <= 3;++i) {
                var latitudeColumn = String.Format("Lat{0}", i);
                var longitudeColumn = String.Format("Lng{0}", i);
                if(worksheet.String(latitudeColumn) != null) {
                    _AirPressures.Add(new AirPressure() {
                        Latitude = worksheet.Float(latitudeColumn),
                        Longitude = worksheet.Float(longitudeColumn),
                        AgeSeconds = (short)i,
                    });
                }
            }
            _Lookup.LoadAirPressures(_AirPressures, _Now);

            var expected = worksheet.Int("Closest");
            var actual = _Lookup.FindClosest(worksheet.Float("Lat"), worksheet.Float("Lng"));

            Assert.AreEqual(expected, actual.AgeSeconds);
        }

        [TestMethod]
        public void AirPressureLookup_FindClosest_Returns_Null_If_Load_Has_Not_Been_Called()
        {
            Assert.IsNull(_Lookup.FindClosest(1, 2));
        }
    }
}
