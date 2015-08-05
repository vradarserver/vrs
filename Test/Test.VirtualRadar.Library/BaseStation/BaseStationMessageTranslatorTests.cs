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
using VirtualRadar.Library;
using VirtualRadar.Interface.BaseStation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using InterfaceFactory;

namespace Test.VirtualRadar.Library.BaseStation
{
    [TestClass]
    public class BaseStationMessageTranslatorTests
    {
        private IBaseStationMessageTranslator _Implementation;

        [TestInitialize]
        public void TestInitialise()
        {
            _Implementation = Factory.Singleton.Resolve<IBaseStationMessageTranslator>();
        }

        public TestContext TestContext { get; set; }

        #region Translate
        [TestMethod]
        public void BaseStationMessageTranslator_Translate_Returns_Unknown_Message_When_Passed_Null_For_The_Message()
        {
            Assert.AreEqual(BaseStationMessageType.Unknown, _Implementation.Translate(null, null).MessageType);
        }

        [TestMethod]
        [ExpectedException(typeof(BaseStationTranslatorException))]
        public void BaseStationMessageTranslator_Throws_When_Passed_Message_With_Unparseable_Chunk()
        {
            _Implementation.Translate("MSG,4,189,7675,4780BC,2196271,2011/11/09,09:17:48.526,2011/11/09,09:17:49.229,,,168Z1,91.7,,,-896,,,,,", 123);
        }

        [TestMethod]
        public void BaseStationMessageTranslator_Translate_Returns_Empty_Message_When_Passed_Empty_String()
        {
            var message = _Implementation.Translate("", 123);
            Assert.AreEqual(BaseStationMessageType.Unknown, message.MessageType);
        }

        [TestMethod]
        public void BaseStationMessageTranslator_Translate_Parses_Message_Types_Correctly()
        {
            foreach(BaseStationMessageType messageType in Enum.GetValues(typeof(BaseStationMessageType))) {
                if(messageType == BaseStationMessageType.Unknown) continue;
                string messageTypeText = BaseStationMessageHelper.ConvertToString(messageType);
                string text = String.Format("{0},1,5,2056,7404F2,11267,2008/11/28,23:48:18.611,2008/11/28,23:53:19.161,,,,,,,,,,,,", messageTypeText);
                BaseStationMessage message = _Implementation.Translate(text, null);
                Assert.AreEqual(messageType, message.MessageType, messageType.ToString());
            }
        }

        [TestMethod]
        public void BaseStationMessageTranslator_Translate_Parses_Unknown_Message_Type_Correctly()
        {
            BaseStationMessage message = _Implementation.Translate("NEW2ME,1,5,2056,7404F2,11267,2008/11/28,23:48:18.611,2008/11/28,23:53:19.161,,,,,,,,,,,,", 123);
            Assert.AreEqual(BaseStationMessageType.Unknown, message.MessageType);
        }

        [TestMethod]
        public void BaseStationMessageTranslator_Translate_Parses_Transmission_Types_Correctly()
        {
            foreach(BaseStationTransmissionType transmissionType in Enum.GetValues(typeof(BaseStationTransmissionType))) {
                if(transmissionType == BaseStationTransmissionType.None) continue;
                string typeText = BaseStationMessageHelper.ConvertToString(transmissionType);
                BaseStationMessage message = _Implementation.Translate(String.Format("MSG,{0},5,2056,7404F2,11267,2008/11/28,23:48:18.611,2008/11/28,23:53:19.161,,,,,,,,,,,,", typeText), 123);
                Assert.AreEqual(transmissionType, message.TransmissionType, transmissionType.ToString());
            }
        }

        [TestMethod]
        public void BaseStationMessageTranslator_Translate_Ignores_TransmissionType_For_All_NonTransmission_Messages()
        {
            foreach(BaseStationMessageType messageType in Enum.GetValues(typeof(BaseStationMessageType))) {
                if(messageType == BaseStationMessageType.Unknown) continue;
                foreach(BaseStationTransmissionType transmissionType in Enum.GetValues(typeof(BaseStationTransmissionType))) {
                    if(transmissionType == BaseStationTransmissionType.None) continue;
                    string text = String.Format("{0},{1},5,2056,7404F2,11267,2008/11/28,23:48:18.611,2008/11/28,23:53:19.161,,,,,,,,,,,,",
                        BaseStationMessageHelper.ConvertToString(messageType),
                        BaseStationMessageHelper.ConvertToString(transmissionType));
                    string errorText = String.Format("{0} and {1}", messageType, transmissionType);
                    BaseStationMessage message = _Implementation.Translate(text, 123);
                    if(messageType == BaseStationMessageType.Transmission) Assert.AreEqual(transmissionType, message.TransmissionType, errorText);
                    else Assert.AreEqual(BaseStationTransmissionType.None, message.TransmissionType, errorText);
                }
            }
        }

        [TestMethod]
        public void BaseStationMessageTranslator_Translate_Parses_Status_Codes_Correctly()
        {
            foreach(BaseStationStatusCode statusCode in Enum.GetValues(typeof(BaseStationStatusCode))) {
                if(statusCode == BaseStationStatusCode.None) continue;
                string text = String.Format("STA,1,5,2056,7404F2,11267,2008/11/28,23:48:18.611,2008/11/28,23:53:19.161,{0},,,,,,,,,,,", BaseStationMessageHelper.ConvertToString(statusCode));
                BaseStationMessage message = _Implementation.Translate(text, 123);
                Assert.AreEqual(statusCode, message.StatusCode, statusCode.ToString());
            }
        }

        [TestMethod]
        public void BaseStationMessageTranslator_Translate_Only_Translate_Status_Codes_For_Status_Messages()
        {
            foreach(BaseStationMessageType messageType in Enum.GetValues(typeof(BaseStationMessageType))) {
                if(messageType == BaseStationMessageType.Unknown) continue;
                foreach(BaseStationStatusCode statusCode in Enum.GetValues(typeof(BaseStationStatusCode))) {
                    if(statusCode == BaseStationStatusCode.None) continue;
                    string text = String.Format("{0},1,5,2056,7404F2,11267,2008/11/28,23:48:18.611,2008/11/28,23:53:19.161,{1},,,,,,,,,,,",
                        BaseStationMessageHelper.ConvertToString(messageType),
                        BaseStationMessageHelper.ConvertToString(statusCode));
                    string errorText = String.Format("{0} and {1}", messageType, statusCode);
                    BaseStationMessage message = _Implementation.Translate(text, 123);
                    if(messageType == BaseStationMessageType.StatusChange) Assert.AreEqual(statusCode, message.StatusCode, errorText);
                    else Assert.AreEqual(BaseStationMessageHelper.ConvertToString(statusCode), message.Callsign, errorText);
                }
            }
        }

        [TestMethod]
        public void BaseStationMessageTranslator_Translate_Strips_At_Signs_In_Callsign()
        {
            BaseStationMessage message = _Implementation.Translate("MSG,1,5,2056,7404F2,11267,2008/11/28,23:48:18.611,2008/11/28,23:53:19.161,A@B,,,,,,,,,,,", 123);
            Assert.AreEqual("AB", message.Callsign);
        }

        [TestMethod]
        public void BaseStationMessageTranslator_Translate_Converts_Zero_Squawks_To_Null()
        {
            BaseStationMessage message = _Implementation.Translate("MSG,1,5,2056,7404F2,11267,2008/11/28,23:48:18.611,2008/11/28,23:53:19.161,ABC,,,,,,,0,,,,", 123);
            Assert.AreEqual(null, message.Squawk);
        }

        [TestMethod]
        public void BaseStationMessageTranslator_Translate_Copies_SignalLevel_To_BaseStationMessage()
        {
            var message = _Implementation.Translate("MSG,1,5,2056,7404F2,11267,2008/11/28,23:48:18.611,2008/11/28,23:53:19.161,A,,,,,,,,,,,", 123);
            Assert.AreEqual(123, message.SignalLevel);

            message = _Implementation.Translate("MSG,1,5,2056,7404F2,11267,2008/11/28,23:48:18.611,2008/11/28,23:53:19.161,A,,,,,,,,,,,", null);
            Assert.AreEqual(null, message.SignalLevel);
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "MessageTranslator$")]
        public void BaseStationMessageTranslator_Translate_Translates_Messages_Correctly()
        {
            ExcelWorksheetData worksheet = new ExcelWorksheetData(TestContext);

            BaseStationMessage translated = _Implementation.Translate(worksheet.String("Text"), 123);
            Assert.AreEqual(Enum.Parse(typeof(BaseStationMessageType), worksheet.String("MessageType")), translated.MessageType);
            Assert.AreEqual(Enum.Parse(typeof(BaseStationTransmissionType), worksheet.String("TransmissionType")), translated.TransmissionType);
            Assert.AreEqual(Enum.Parse(typeof(BaseStationStatusCode), worksheet.String("StatusCode")), translated.StatusCode);
            Assert.AreEqual(worksheet.Int("SessionId"), translated.SessionId);
            Assert.AreEqual(worksheet.Int("AircraftId"), translated.AircraftId);
            Assert.AreEqual(worksheet.String("Icao24"), translated.Icao24);
            Assert.AreEqual(worksheet.Int("FlightId"), translated.FlightId);
            Assert.AreEqual(worksheet.DateTime("MessageGenerated"), translated.MessageGenerated);
            Assert.AreEqual(worksheet.DateTime("MessageLogged"), translated.MessageLogged);
            Assert.AreEqual(worksheet.String("Callsign"), translated.Callsign);
            Assert.AreEqual(worksheet.NFloat("Altitude"), translated.Altitude);
            Assert.AreEqual(worksheet.NFloat("GroundSpeed"), translated.GroundSpeed);
            Assert.AreEqual(worksheet.NFloat("Track"), translated.Track);
            Assert.AreEqual(worksheet.NDouble("Latitude"), translated.Latitude);
            Assert.AreEqual(worksheet.NDouble("Longitude"), translated.Longitude);
            Assert.AreEqual(worksheet.NFloat("VerticalRate"), translated.VerticalRate);
            Assert.AreEqual(worksheet.NInt("Squawk"), translated.Squawk);
            Assert.AreEqual(worksheet.NBool("SquawkHasChanged"), translated.SquawkHasChanged);
            Assert.AreEqual(worksheet.NBool("Emergency"), translated.Emergency);
            Assert.AreEqual(worksheet.NBool("IdentActive"), translated.IdentActive);
            Assert.AreEqual(worksheet.NBool("OnGround"), translated.OnGround);
            Assert.AreEqual(worksheet.Bool("IsMlat"), translated.IsMlat);
        }
        #endregion
    }
}
