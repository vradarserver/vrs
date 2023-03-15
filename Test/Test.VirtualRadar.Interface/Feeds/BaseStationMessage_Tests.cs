// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Feeds;

namespace Test.VirtualRadar.Interface.Feeds
{
    [TestClass]
    public class BaseStationMessage_Tests
    {
        public TestContext TestContext { get; set; }

        private BaseStationMessage _Implementation;

        [TestInitialize]
        public void TestInitialise()
        {
            _Implementation = new BaseStationMessage();
        }

        [TestMethod]
        public void ToBaseStationString_Builds_Valid_BaseStation_Messages_From_A_BaseStationMessage_Object()
        {
            Do_BaseStationMessage_ToBaseStationString_Builds_Valid_BaseStation_Messages_From_A_BaseStationMessage_Object();
        }

        [TestMethod]
        public void ToBaseStationString_Builds_Valid_BaseStation_Messages_From_A_BaseStationMessage_Object_For_Different_Cultures()
        {
            foreach(var cultureName in new string[] { "en-GB", "fr-FR", "de-DE", "uk-UA" }) {
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
            var spreadsheet = new SpreadsheetTestData(TestData.FeedTests, "ToBaseStationString");
            spreadsheet.TestEveryRow(this, row => {
                _Implementation.MessageType = row.Enum<BaseStationMessageType>("MessageType");
                _Implementation.TransmissionType = row.Enum<BaseStationTransmissionType>("TransmissionType");
                _Implementation.StatusCode = row.Enum<BaseStationStatusCode>("StatusCode");
                _Implementation.SessionId = row.Int("SessionId");
                _Implementation.AircraftId = row.Int("AircraftId");
                _Implementation.Icao24 = row.String("Icao24");
                _Implementation.FlightId = row.Int("FlightId");
                _Implementation.MessageGenerated = row.DateTime("MessageGenerated");
                _Implementation.MessageLogged = row.DateTime("MessageLogged");
                _Implementation.Callsign = row.String("Callsign");
                _Implementation.Altitude = row.NInt("Altitude");
                _Implementation.GroundSpeed = row.NFloat("GroundSpeed");
                _Implementation.Track = row.NFloat("Track");
                _Implementation.Latitude = row.NDouble("Latitude");
                _Implementation.Longitude = row.NDouble("Longitude");
                _Implementation.VerticalRate = row.NInt("VerticalRate");
                _Implementation.Squawk = row.NInt("Squawk");
                _Implementation.SquawkHasChanged = row.NBool("SquawkHasChanged");
                _Implementation.Emergency = row.NBool("Emergency");
                _Implementation.IdentActive = row.NBool("IdentActive");
                _Implementation.OnGround = row.NBool("OnGround");
                _Implementation.IsMlat = row.Bool("IsMlat");

                var emitExtendedBaseStationFormat = row.Bool("Extended");

                Assert.AreEqual(row.String("Text"), _Implementation.ToBaseStationString(emitExtendedBaseStationFormat));
            });
        }

        [TestMethod]
        public void ToString_Only_Shows_6_Decimal_Places_For_Latitude_And_Longitude()
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
        public void ToString_Only_Shows_1_Decimal_Place_For_Track()
        {
            var message = new BaseStationMessage() {
                Track = 0.654321F,
            };

            var text = message.ToBaseStationString();

            Assert.AreNotEqual(-1, text.IndexOf(",0.7,"));
        }

        [TestMethod]
        public void ToString_Only_Shows_1_Decimal_Place_For_GroundSpeed()
        {
            var message = new BaseStationMessage() {
                GroundSpeed = 0.654321F,
            };

            var text = message.ToBaseStationString();

            Assert.AreNotEqual(-1, text.IndexOf(",0.7,"));
        }
    }
}
