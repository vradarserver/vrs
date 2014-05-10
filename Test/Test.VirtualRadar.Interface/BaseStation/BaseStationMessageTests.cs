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
using System.Collections.Generic;
using System.Text;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;

namespace Test.VirtualRadar.Interface.BaseStation
{
    [TestClass]
    public class BaseStationMessageTests
    {
        public TestContext TestContext { get; set; }

        private BaseStationMessage _Implementation;

        [TestInitialize]
        public void TestInitialise()
        {
            _Implementation = new BaseStationMessage();
        }

        [TestMethod]
        public void BaseStationMessage_Initialises_To_Known_State_And_Properties_Work()
        {
            TestUtilities.TestProperty(_Implementation, r => r.AircraftId, 0, 1);
            TestUtilities.TestProperty(_Implementation, r => r.Altitude, null, 99);
            TestUtilities.TestProperty(_Implementation, r => r.Callsign, null, "AAbb");
            TestUtilities.TestProperty(_Implementation, r => r.Emergency, null, true);
            TestUtilities.TestProperty(_Implementation, r => r.FlightId, 0, 12);
            TestUtilities.TestProperty(_Implementation, r => r.GroundSpeed, null, 2.5f);
            TestUtilities.TestProperty(_Implementation, r => r.IdentActive, null, true);
            TestUtilities.TestProperty(_Implementation, r => r.Latitude, null, 2.3);
            TestUtilities.TestProperty(_Implementation, r => r.Longitude, null, -12.4);
            TestUtilities.TestProperty(_Implementation, r => r.MessageGenerated, DateTime.MinValue, DateTime.Now);
            TestUtilities.TestProperty(_Implementation, r => r.MessageLogged, DateTime.MinValue, DateTime.Now);
            TestUtilities.TestProperty(_Implementation, r => r.MessageType, BaseStationMessageType.Unknown, BaseStationMessageType.StatusChange);
            TestUtilities.TestProperty(_Implementation, r => r.Icao24, null, "1203");
            TestUtilities.TestProperty(_Implementation, r => r.OnGround, null, false);
            TestUtilities.TestProperty(_Implementation, r => r.ReceiverId, 0, 1234);
            TestUtilities.TestProperty(_Implementation, r => r.SessionId, 0, 123);
            TestUtilities.TestProperty(_Implementation, r => r.SignalLevel, null, 123);
            TestUtilities.TestProperty(_Implementation, r => r.Squawk, null, 9000);
            TestUtilities.TestProperty(_Implementation, r => r.SquawkHasChanged, null, false);
            TestUtilities.TestProperty(_Implementation, r => r.StatusCode, BaseStationStatusCode.None, BaseStationStatusCode.OK);
            TestUtilities.TestProperty(_Implementation, r => r.Supplementary, null, new BaseStationSupplementaryMessage());
            TestUtilities.TestProperty(_Implementation, r => r.Track, null, 123.543F);
            TestUtilities.TestProperty(_Implementation, r => r.TransmissionType, BaseStationTransmissionType.None, BaseStationTransmissionType.SurfacePosition);
            TestUtilities.TestProperty(_Implementation, r => r.VerticalRate, null, 123);
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "ToBaseStationString$")]
        public void BaseStationMessage_ToBaseStationString_Builds_Valid_BaseStation_Messages_From_A_BaseStationMessage_Object()
        {
            Do_BaseStationMessage_ToBaseStationString_Builds_Valid_BaseStation_Messages_From_A_BaseStationMessage_Object();
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "ToBaseStationString$")]
        public void BaseStationMessage_ToBaseStationString_Builds_Valid_BaseStation_Messages_From_A_BaseStationMessage_Object_For_Different_Cultures()
        {
            foreach(var cultureName in new string[] { "en-GB", "fr-FR", "de-DE", "ru-RU" }) {
                using(var cultureSwitcher = new CultureSwitcher(cultureName)) {
                    try {
                        Do_BaseStationMessage_ToBaseStationString_Builds_Valid_BaseStation_Messages_From_A_BaseStationMessage_Object();
                    } catch(Exception ex) {
                        Assert.Fail("Failed to correctly translate a BaseStation message when culture was {0} - {1}", cultureName, ex.ToString());
                    }
                }
            }
        }

        private void Do_BaseStationMessage_ToBaseStationString_Builds_Valid_BaseStation_Messages_From_A_BaseStationMessage_Object()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            _Implementation.MessageType = worksheet.ParseEnum<BaseStationMessageType>("MessageType");
            _Implementation.TransmissionType = worksheet.ParseEnum<BaseStationTransmissionType>("TransmissionType");
            _Implementation.StatusCode = worksheet.ParseEnum<BaseStationStatusCode>("StatusCode");
            _Implementation.SessionId = worksheet.Int("SessionId");
            _Implementation.AircraftId = worksheet.Int("AircraftId");
            _Implementation.Icao24 = worksheet.String("Icao24");
            _Implementation.FlightId = worksheet.Int("FlightId");
            _Implementation.MessageGenerated = worksheet.DateTime("MessageGenerated");
            _Implementation.MessageLogged = worksheet.DateTime("MessageLogged");
            _Implementation.Callsign = worksheet.String("Callsign");
            _Implementation.Altitude = worksheet.NInt("Altitude");
            _Implementation.GroundSpeed = worksheet.NFloat("GroundSpeed");
            _Implementation.Track = worksheet.NFloat("Track");
            _Implementation.Latitude = worksheet.NDouble("Latitude");
            _Implementation.Longitude = worksheet.NDouble("Longitude");
            _Implementation.VerticalRate = worksheet.NInt("VerticalRate");
            _Implementation.Squawk = worksheet.NInt("Squawk");
            _Implementation.SquawkHasChanged = worksheet.NBool("SquawkHasChanged");
            _Implementation.Emergency = worksheet.NBool("Emergency");
            _Implementation.IdentActive = worksheet.NBool("IdentActive");
            _Implementation.OnGround = worksheet.NBool("OnGround");

            Assert.AreEqual(worksheet.String("Text"), _Implementation.ToBaseStationString());
        }

        [TestMethod]
        public void BaseStationMessage_ToString_Only_Shows_6_Decimal_Places_For_Latitude_And_Longitude()
        {
            var message = new BaseStationMessage() {
                Latitude = 0.123456789012345,
                Longitude = 0.543210987654321,
            };

            var text = message.ToBaseStationString();

            Assert.AreNotEqual(-1, text.IndexOf(",0.123457,"));
            Assert.AreNotEqual(-1, text.IndexOf(",0.543211,"));
        }

        [TestMethod]
        public void BaseStationMessage_ToString_Only_Shows_1_Decimal_Place_For_Track()
        {
            var message = new BaseStationMessage() {
                Track = 0.654321F,
            };

            var text = message.ToBaseStationString();

            Assert.AreNotEqual(-1, text.IndexOf(",0.7,"));
        }

        [TestMethod]
        public void BaseStationMessage_ToString_Only_Shows_1_Decimal_Place_For_GroundSpeed()
        {
            var message = new BaseStationMessage() {
                GroundSpeed = 0.654321F,
            };

            var text = message.ToBaseStationString();

            Assert.AreNotEqual(-1, text.IndexOf(",0.7,"));
        }
    }
}
