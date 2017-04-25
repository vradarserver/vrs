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
using VirtualRadar.Interface.WebSite;
using Test.Framework;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface.WebSite
{
    [TestClass]
    public class ProximityGadgetAircraftJsonTests
    {
        public TestContext TestContext { get; set; }

        private List<IAircraft> CreateAircraft(int count)
        {
            var result = new List<IAircraft>();

            for(var i = 0;i < count;++i) {
                result.Add(TestUtilities.CreateMockInstance<IAircraft>().Object);
            }

            return result;
        }

        [TestMethod]
        public void ProximityGadgetAircraftJson_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            var json = new ProximityGadgetAircraftJson();

            TestUtilities.TestProperty(json, "ClosestAircraft", null, new ProximityGadgetClosestAircraftJson());
            TestUtilities.TestProperty(json, "WarningMessage", null, "Abc");
            Assert.AreEqual(0, json.EmergencyAircraft.Count);
        }

        [TestMethod]
        public void ProximityGadgetAircraftJson_ToModel_Returns_Null_If_Passed_Null()
        {
            Assert.IsNull(ProximityGadgetAircraftJson.ToModel(null, 1.0, 1.0));
        }

        [TestMethod]
        public void ProximityGadgetAircraftJson_ToModel_Warns_If_Location_Not_Specified()
        {
            var warning = "Position not supplied";
            var aircraft = CreateAircraft(1);
            aircraft[0].Latitude = 1;
            aircraft[0].Longitude = 1;

            var model = ProximityGadgetAircraftJson.ToModel(aircraft, null, null);
            Assert.AreEqual(warning, model.WarningMessage);

            model = ProximityGadgetAircraftJson.ToModel(aircraft, 1, null);
            Assert.AreEqual(warning, model.WarningMessage);

            model = ProximityGadgetAircraftJson.ToModel(aircraft, null, 1);
            Assert.AreEqual(warning, model.WarningMessage);

            model = ProximityGadgetAircraftJson.ToModel(aircraft, 1, 1);
            Assert.AreEqual(null, model.WarningMessage);
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "ClosestAircraftSelection$")]
        public void ProximityGadgetAircraftJson_ToModel_Returns_The_Closest_Aircraft()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var originLatitude = worksheet.NDouble("GadgetLat");
            var originLongitude = worksheet.NDouble("GadgetLng");

            var aircraft = CreateAircraft(2);
            for(int i = 1;i <= 2;++i) {
                aircraft[i - 1].Callsign = i.ToString();
                aircraft[i - 1].Latitude = worksheet.NFloat(String.Format("AC{0}Lat", i));
                aircraft[i - 1].Longitude = worksheet.NFloat(String.Format("AC{0}Lng", i));
            }

            var model = ProximityGadgetAircraftJson.ToModel(aircraft, originLatitude, originLongitude);

            var closestCallsign = worksheet.String("Closest");
            if(closestCallsign == null) {
                Assert.IsNull(model.ClosestAircraft);
            } else {
                Assert.AreEqual(closestCallsign, model.ClosestAircraft.Callsign);
            }
        }

        [TestMethod]
        public void ProximityGadgetAircraftJson_ToModel_Returns_List_Of_Aircraft_Transmitting_An_Emergency_Squawk()
        {
            var aircraft = CreateAircraft(2);

            var model = ProximityGadgetAircraftJson.ToModel(aircraft, 1, 1);
            Assert.AreEqual(0, model.EmergencyAircraft.Count);

            aircraft[0].Emergency = true;
            aircraft[0].Callsign = "TEST";
            model = ProximityGadgetAircraftJson.ToModel(aircraft, 1, 1);
            Assert.AreEqual(1, model.EmergencyAircraft.Count);
            Assert.AreEqual("TEST", model.EmergencyAircraft[0].Callsign);

            aircraft[1].Emergency = true;
            model = ProximityGadgetAircraftJson.ToModel(aircraft, 1, 1);
            Assert.AreEqual(2, model.EmergencyAircraft.Count);
        }
    }
}
