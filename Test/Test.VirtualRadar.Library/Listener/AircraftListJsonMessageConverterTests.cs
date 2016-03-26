// Copyright © 2015 onwards, Andrew Whewell
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
using System.Globalization;
using System.Linq;
using System.Text;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.WebSite;

namespace Test.VirtualRadar.Library.Listener
{
    [TestClass]
    public class AircraftListJsonMessageConverterTests
    {
        public TestContext TestContext { get; set; }
        private IAircraftListJsonMessageConverter _Converter;
        private AircraftListJson _AircraftListJson;

        [TestInitialize]
        public void TestInitialise()
        {
            _Converter = Factory.Singleton.Resolve<IAircraftListJsonMessageConverter>();
            _AircraftListJson = new AircraftListJson();
        }

        [TestMethod]
        public void AircraftListJsonMessageConverter_Sets_Correct_MessageType_And_StatusCode()
        {
            _AircraftListJson.Aircraft.Add(new AircraftJson() { Icao24 = "A" });

            var message = _Converter.ConvertIntoBaseStationMessages(_AircraftListJson).Single();

            Assert.AreEqual(BaseStationMessageType.Transmission, message.MessageType);
            Assert.AreEqual(BaseStationStatusCode.None, message.StatusCode);
        }

        [TestMethod]
        public void AircraftListJsonMessageConverter_Sets_IsMlat_When_Position_Is_Present_And_Position_Is_MLAT()
        {
            var aircraft = new AircraftJson() {
                Icao24 = "A",
                Latitude = 1,
                Longitude = 2,
                PositionIsMlat = true,
            };
            _AircraftListJson.Aircraft.Add(aircraft);

            var message = _Converter.ConvertIntoBaseStationMessages(_AircraftListJson).Single();

            Assert.IsTrue(message.IsMlat);
        }

        [TestMethod]
        public void AircraftListJsonMessageConverter_Clears_IsMlat_When_Position_Is_Present_And_Position_Is_Not_MLAT()
        {
            var aircraft = new AircraftJson() {
                Icao24 = "A",
                Latitude = 1,
                Longitude = 2,
                PositionIsMlat = false,
            };
            _AircraftListJson.Aircraft.Add(aircraft);

            var message = _Converter.ConvertIntoBaseStationMessages(_AircraftListJson).Single();

            Assert.IsFalse(message.IsMlat);
        }

        [TestMethod]
        public void AircraftListJsonMessageConverter_Clears_IsMlat_When_Position_Is_Not_Present_And_Position_Is_Not_MLAT()
        {
            var aircraft = new AircraftJson() {
                Icao24 = "A",
                PositionIsMlat = true,
            };
            _AircraftListJson.Aircraft.Add(aircraft);

            var message = _Converter.ConvertIntoBaseStationMessages(_AircraftListJson).Single();

            Assert.IsFalse(message.IsMlat);
        }

        [TestMethod]
        public void AircraftListJsonMessageConverter_Clears_IsMlat_When_Position_Is_Present_And_Position_Is_MLAT_Is_Null()
        {
            var aircraft = new AircraftJson() {
                Icao24 = "A",
                Latitude = 1,
                Longitude = 2,
                PositionIsMlat = null
            };
            _AircraftListJson.Aircraft.Add(aircraft);

            var message = _Converter.ConvertIntoBaseStationMessages(_AircraftListJson).Single();

            Assert.IsFalse(message.IsMlat);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AircraftListJsonMessageCnvter$")]
        public void AircraftListJsonMessageConverter_Copies_Json_Properties_To_Correct_Message_Property()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var jsonPropertyName = worksheet.String("JsonProperty");
            var jsonValueText = worksheet.EString("JsonValue");
            var messagePropertyName = worksheet.String("MsgProperty");
            var messageValueText = worksheet.EString("MsgValue");
            var supplementaryPropertyName = worksheet.String("SuppProperty");
            var supplementaryValueText = worksheet.EString("SuppValue");

            var isSupplementary = supplementaryPropertyName != null;

            var jsonProperty = typeof(AircraftJson).GetProperty(jsonPropertyName);
            if(jsonProperty == null) Assert.Fail(jsonPropertyName);
            var jsonValue = TestUtilities.ChangeType(jsonValueText, jsonProperty.PropertyType, CultureInfo.InvariantCulture);

            var messageProperty = isSupplementary ? null : typeof(BaseStationMessage).GetProperty(messagePropertyName);
            if(messageProperty == null && !isSupplementary) Assert.Fail(messagePropertyName);
            var messageValue = messageProperty == null ? null : TestUtilities.ChangeType(messageValueText, messageProperty.PropertyType, CultureInfo.InvariantCulture);

            var supplementaryProperty = isSupplementary ? typeof(BaseStationSupplementaryMessage).GetProperty(supplementaryPropertyName) : null;
            if(supplementaryProperty == null && isSupplementary) Assert.Fail(supplementaryPropertyName);
            var supplementaryValue = supplementaryProperty == null ? null : TestUtilities.ChangeType(supplementaryValueText, supplementaryProperty.PropertyType, CultureInfo.InvariantCulture);

            var aircraftJson = new AircraftJson();
            _AircraftListJson.Aircraft.Add(aircraftJson);
            jsonProperty.SetValue(aircraftJson, jsonValue, null);

            var message = _Converter.ConvertIntoBaseStationMessages(_AircraftListJson).Single();
            var supplementary = message.Supplementary;

            if(messageProperty != null) {
                var actualValue = messageProperty.GetValue(message, null);
                Assert.AreEqual(messageValue, actualValue, jsonPropertyName);
            } else {
                if(supplementary == null) {
                    if(supplementaryValue != null) {
                        Assert.Fail(jsonPropertyName);
                    }
                } else {
                    var actualValue = supplementaryProperty.GetValue(supplementary, null);
                    Assert.AreEqual(supplementaryValue, actualValue, jsonPropertyName);
                }
            }
        }

        [TestMethod]
        public void AircraftListJsonMessageConverter_Creates_Messages_That_BaseStationMessageCompressor_Can_RoundTrip()
        {
            // There was a bug on the initial release of the aircraft list (JSON) format whereby if you created a
            // CompressedVRS rebroadcast server on an aircraft list receiver then it wouldn't send any messages over
            // the link. It was caused by the compressor ignoring the messages generated by the converter.

            _AircraftListJson.Aircraft.Add(new AircraftJson() { Icao24 = "ABCDEF", Altitude = 100 });
            var message = _Converter.ConvertIntoBaseStationMessages(_AircraftListJson).Single();

            var compressor = Factory.Singleton.Resolve<IBaseStationMessageCompressor>();
            var compressedMessage = compressor.Compress(message);
            Assert.IsNotNull(compressedMessage);
            Assert.AreNotEqual(0, compressedMessage.Length);
        }

        [TestMethod]
        public void AircraftListJsonMessageConverter_Deals_Correctly_With_Altitude_Types_From_Current_Versions_Of_VRS()
        {
            var json = new AircraftJson() { Icao24 = "ABCDEF", Altitude = 100, GeometricAltitude = 200 };
            _AircraftListJson.Aircraft.Add(json);

            // If the altitude type is Barometric then JSON Altitude should be copied into Altitude
            json.AltitudeType = (int)AltitudeType.Barometric;
            var message = _Converter.ConvertIntoBaseStationMessages(_AircraftListJson).Single();
            Assert.AreEqual(100, message.Altitude);

            // If the altitude type is Geometric then the JSON Altitude may have been calculated - the GeometricAltitude
            // should be used instead.
            json.AltitudeType = (int)AltitudeType.Geometric;
            message = _Converter.ConvertIntoBaseStationMessages(_AircraftListJson).Single();
            Assert.AreEqual(200, message.Altitude);
        }

        [TestMethod]
        public void AircraftListJsonMessageConverter_Deals_Correctly_With_Altitude_Types_From_Older_Versions_Of_VRS()
        {
            // If the Geometric altitude is null then the feed is coming from an older version of VRS
            // that didn't send Geometric, Altitude and AltitudeType as a set. In that case we always
            // use Altitude regardless of altitude type

            var json = new AircraftJson() { Icao24 = "ABCDEF", Altitude = 100 };
            _AircraftListJson.Aircraft.Add(json);

            // If altitude type is missing then use altitude...
            var message = _Converter.ConvertIntoBaseStationMessages(_AircraftListJson).Single();
            Assert.AreEqual(100, message.Altitude);

            // If altitude type is barometric then use altitude...
            json.AltitudeType = (int)AltitudeType.Barometric;
            message = _Converter.ConvertIntoBaseStationMessages(_AircraftListJson).Single();
            Assert.AreEqual(100, message.Altitude);

            // ... and if altitude type is geometric then use altitude
            json.AltitudeType = (int)AltitudeType.Geometric;
            message = _Converter.ConvertIntoBaseStationMessages(_AircraftListJson).Single();
            Assert.AreEqual(100, message.Altitude);
        }
    }
}
