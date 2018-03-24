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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;

namespace Test.VirtualRadar.Library.Listener
{
    [TestClass]
    public class AirnavXRangeMessageConverterTests
    {
        public TestContext TestContext { get; set; }
        private IAirnavXRangeMessageConverter _Converter;
        private AirnavXRangeJson _Json;

        [TestInitialize]
        public void TestInitialise()
        {
            _Converter = Factory.Resolve<IAirnavXRangeMessageConverter>();
            _Json = new AirnavXRangeJson();
        }

        [TestMethod]
        public void AirnavXRangeMessageConverter_Clears_IsOutOfBand()
        {
            _Json.Aircraft.Add(new AirnavXRangeAircraftJson() { Icao24 = "A" });
            var eventArgs = _Converter.ConvertIntoBaseStationMessageEventArgs(_Json).Single();
            Assert.IsFalse(eventArgs.IsOutOfBand);
        }

        [TestMethod]
        public void AirnavXRangeMessageConverter_Clears_IsSatcomFeed()
        {
            _Json.Aircraft.Add(new AirnavXRangeAircraftJson() { Icao24 = "A" });
            var eventArgs = _Converter.ConvertIntoBaseStationMessageEventArgs(_Json).Single();
            Assert.IsFalse(eventArgs.IsSatcomFeed);
        }

        [TestMethod]
        public void AirnavXRangeMessageConverter_Sets_Correct_MessageType_And_StatusCode()
        {
            _Json.Aircraft.Add(new AirnavXRangeAircraftJson() { Icao24 = "A" });

            var eventArgs = _Converter.ConvertIntoBaseStationMessageEventArgs(_Json).Single();

            Assert.AreEqual(BaseStationMessageType.Transmission, eventArgs.Message.MessageType);
            Assert.AreEqual(BaseStationStatusCode.None, eventArgs.Message.StatusCode);
        }

        [TestMethod]
        public void AirnavXRangeMessageConverter_Clears_IsMlat()
        {
            // I think MLAT is present on the feed but I don't know what's sent... the examples I have at the moment
            // just show an empty array called mlat. For now I'll just clear the flag for all messages.
            var aircraft = new AirnavXRangeAircraftJson() {
                Icao24 = "A",
                Latitude = 1,
                Longitude = 2,
            };
            _Json.Aircraft.Add(aircraft);

            var eventArgs = _Converter.ConvertIntoBaseStationMessageEventArgs(_Json).Single();

            Assert.IsFalse(eventArgs.Message.IsMlat);
        }

        [TestMethod]
        public void AirnavXRangeMessageConverter_Clears_IsTisb()
        {
            // Same story for TISB, the examples have an array called tisb but they're all empty. For now I'll just clear
            // the flag.
            var aircraft = new AirnavXRangeAircraftJson() {
                Icao24 = "A",
                Latitude = 1,
                Longitude = 2,
            };
            _Json.Aircraft.Add(aircraft);

            var eventArgs = _Converter.ConvertIntoBaseStationMessageEventArgs(_Json).Single();

            Assert.IsFalse(eventArgs.Message.IsTisb);
        }

        [TestMethod]
        public void AirnavXRangeMessageConverter_Altitudes_Are_Always_Pressure()
        {
            var aircraft = new AirnavXRangeAircraftJson() {
                Icao24 = "A",
                Altitude = 10,
            };
            _Json.Aircraft.Add(aircraft);

            var eventArgs = _Converter.ConvertIntoBaseStationMessageEventArgs(_Json).Single();

            Assert.IsFalse(eventArgs.Message.Supplementary?.AltitudeIsGeometric.GetValueOrDefault() ?? false);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AirnavXRangeMessageCnvter$")]
        public void AirnavXRangeMessageConverter_Copies_Json_Properties_To_Correct_Message_Property()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var jsonPropertyName = worksheet.String("JsonProperty");
            var jsonValueText = worksheet.EString("JsonValue");
            var messagePropertyName = worksheet.String("MsgProperty");
            var messageValueText = worksheet.EString("MsgValue");
            var supplementaryPropertyName = worksheet.String("SuppProperty");
            var supplementaryValueText = worksheet.EString("SuppValue");

            var isSupplementary = supplementaryPropertyName != null;

            var jsonProperty = typeof(AirnavXRangeAircraftJson).GetProperty(jsonPropertyName);
            if(jsonProperty == null) Assert.Fail(jsonPropertyName);
            var jsonValue = TestUtilities.ChangeType(jsonValueText, jsonProperty.PropertyType, CultureInfo.InvariantCulture);

            var messageProperty = isSupplementary ? null : typeof(BaseStationMessage).GetProperty(messagePropertyName);
            if(messageProperty == null && !isSupplementary) Assert.Fail(messagePropertyName);
            var messageValue = messageProperty == null ? null : TestUtilities.ChangeType(messageValueText, messageProperty.PropertyType, CultureInfo.InvariantCulture);

            var supplementaryProperty = isSupplementary ? typeof(BaseStationSupplementaryMessage).GetProperty(supplementaryPropertyName) : null;
            if(supplementaryProperty == null && isSupplementary) Assert.Fail(supplementaryPropertyName);
            var supplementaryValue = supplementaryProperty == null ? null : TestUtilities.ChangeType(supplementaryValueText, supplementaryProperty.PropertyType, CultureInfo.InvariantCulture);

            var aircraftJson = new AirnavXRangeAircraftJson();
            _Json.Aircraft.Add(aircraftJson);
            jsonProperty.SetValue(aircraftJson, jsonValue, null);

            var eventArgs = _Converter.ConvertIntoBaseStationMessageEventArgs(_Json).Single();
            var supplementary = eventArgs.Message.Supplementary;

            if(messageProperty != null) {
                var actualValue = messageProperty.GetValue(eventArgs.Message, null);
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
        public void AirnavXRangeMessageConverter_Creates_Messages_That_BaseStationMessageCompressor_Can_RoundTrip()
        {
            _Json.Aircraft.Add(new AirnavXRangeAircraftJson() { Icao24 = "ABCDEF", Altitude = 100 });
            var eventArgs = _Converter.ConvertIntoBaseStationMessageEventArgs(_Json).Single();

            var compressor = Factory.Resolve<IBaseStationMessageCompressor>();
            var compressedMessage = compressor.Compress(eventArgs.Message);
            Assert.IsNotNull(compressedMessage);
            Assert.AreNotEqual(0, compressedMessage.Length);
        }
    }
}
