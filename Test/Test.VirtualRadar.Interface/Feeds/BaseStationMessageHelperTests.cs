// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Interface.Feeds;

namespace Test.VirtualRadar.Interface.Feeds
{
    [TestClass]
    public class BaseStationMessageHelperTests
    {
        [TestMethod]
        public void ConvertToBaseStationMessageType_Converts_Text_Correctly()
        {
            foreach(var messageType in Enum.GetValues<BaseStationMessageType>()) {
                string text = null;
                switch(messageType) {
                    case BaseStationMessageType.NewAircraft:        text = "AIR"; break;
                    case BaseStationMessageType.NewIdentifier:      text = "ID"; break;
                    case BaseStationMessageType.StatusChange:       text = "STA"; break;
                    case BaseStationMessageType.Transmission:       text = "MSG"; break;
                    case BaseStationMessageType.Unknown:            continue;
                    case BaseStationMessageType.UserClicked:        text = "SEL"; break;
                    case BaseStationMessageType.UserDoubleClicked:  text = "CLK"; break;
                    default:                                        throw new NotImplementedException();
                }
                var isMlat = false;
                Assert.AreEqual(messageType, BaseStationMessageHelper.ConvertToBaseStationMessageType(text, ref isMlat), messageType.ToString());
            }
        }

        [TestMethod]
        public void ConvertToBaseStationMessageType_Converts_Unknown_Text_Correctly()
        {
            foreach(var text in new string[] { null, "", "HHH", " MSG", "ID " }) {
                var isMlat = false;
                Assert.AreEqual(BaseStationMessageType.Unknown, BaseStationMessageHelper.ConvertToBaseStationMessageType(text, ref isMlat));
            }
        }

        [TestMethod]
        public void ConvertToBaseStationMessageType_Converts_Mlat_Message_Types_Correctly()
        {
            var isMlat = false;

            var messageType = BaseStationMessageHelper.ConvertToBaseStationMessageType("MLAT", ref isMlat);
            Assert.AreEqual(BaseStationMessageType.Transmission, messageType);
            Assert.IsTrue(isMlat);

            isMlat = false;
            messageType = BaseStationMessageHelper.ConvertToBaseStationMessageType("MSG", ref isMlat);
            Assert.AreEqual(BaseStationMessageType.Transmission, messageType);
            Assert.IsFalse(isMlat);         // Actually this is untouched, but it's definitely not true
        }

        [TestMethod]
        public void BaseStationMessageHelperTests_ConvertToString_BaseStationMessageType_Converts_All_Types_Correctly()
        {
            foreach(var messageType in Enum.GetValues<BaseStationMessageType>()) {
                var text = BaseStationMessageHelper.ConvertToString(messageType);
                switch(messageType) {
                    case BaseStationMessageType.NewAircraft:        Assert.AreEqual("AIR", text); break;
                    case BaseStationMessageType.NewIdentifier:      Assert.AreEqual("ID", text); break;
                    case BaseStationMessageType.StatusChange:       Assert.AreEqual("STA", text); break;
                    case BaseStationMessageType.Transmission:       Assert.AreEqual("MSG", text); break;
                    case BaseStationMessageType.Unknown:            Assert.AreEqual("", text); break;
                    case BaseStationMessageType.UserClicked:        Assert.AreEqual("SEL", text); break;
                    case BaseStationMessageType.UserDoubleClicked:  Assert.AreEqual("CLK", text); break;
                    default:                                        throw new NotImplementedException();
                }
            }
        }

        [TestMethod]
        public void ConvertToBaseStationTransmissionType_Converts_Text_Correctly()
        {
            foreach(var transmissionType in Enum.GetValues<BaseStationTransmissionType>()) {
                string text = null;
                switch(transmissionType) {
                    case BaseStationTransmissionType.AirbornePosition:              text = "3"; break;
                    case BaseStationTransmissionType.AirborneVelocity:              text = "4"; break;
                    case BaseStationTransmissionType.AirToAir:                      text = "7"; break;
                    case BaseStationTransmissionType.AllCallReply:                  text = "8"; break;
                    case BaseStationTransmissionType.IdentificationAndCategory:     text = "1"; break;
                    case BaseStationTransmissionType.None:                          continue;
                    case BaseStationTransmissionType.SurfacePosition:               text = "2"; break;
                    case BaseStationTransmissionType.SurveillanceAlt:               text = "5"; break;
                    case BaseStationTransmissionType.SurveillanceId:                text = "6"; break;
                    default:                                                        throw new NotImplementedException();
                }
                Assert.AreEqual(transmissionType, BaseStationMessageHelper.ConvertToBaseStationTransmissionType(text), transmissionType.ToString());
            }
        }

        [TestMethod]
        public void ConvertToBaseStationTransmissionType_Converts_Unknown_Text_Correctly()
        {
            foreach(var text in new string[] { null, "", "0", "9", "10", "AA" }) {
                Assert.AreEqual(BaseStationTransmissionType.None, BaseStationMessageHelper.ConvertToBaseStationTransmissionType(text));
            }
        }

        [TestMethod]
        public void BaseStationMessageHelperTests_ConvertToString_BaseStationTransmissionType_Converts_All_Types_Correctly()
        {
            foreach(var transmissionType in Enum.GetValues<BaseStationTransmissionType>()) {
                var text = BaseStationMessageHelper.ConvertToString(transmissionType);
                switch(transmissionType) {
                    case BaseStationTransmissionType.AirbornePosition:              Assert.AreEqual("3", text); break;
                    case BaseStationTransmissionType.AirborneVelocity:              Assert.AreEqual("4", text); break;
                    case BaseStationTransmissionType.AirToAir:                      Assert.AreEqual("7", text); break;
                    case BaseStationTransmissionType.AllCallReply:                  Assert.AreEqual("8", text); break;
                    case BaseStationTransmissionType.IdentificationAndCategory:     Assert.AreEqual("1", text); break;
                    case BaseStationTransmissionType.None:                          Assert.AreEqual("", text); break;
                    case BaseStationTransmissionType.SurfacePosition:               Assert.AreEqual("2", text); break;
                    case BaseStationTransmissionType.SurveillanceAlt:               Assert.AreEqual("5", text); break;
                    case BaseStationTransmissionType.SurveillanceId:                Assert.AreEqual("6", text); break;
                    default:                                                        throw new NotImplementedException();
                }
            }
        }

        [TestMethod]
        public void ConvertToBaseStationStatusCode_Converts_Text_Correctly()
        {
            foreach(var statusCode in Enum.GetValues<BaseStationStatusCode>()) {
                string text = null;
                switch(statusCode) {
                    case BaseStationStatusCode.Delete:          text = "AD"; break;
                    case BaseStationStatusCode.None:            continue;
                    case BaseStationStatusCode.OK:              text = "OK"; break;
                    case BaseStationStatusCode.PositionLost:    text = "PL"; break;
                    case BaseStationStatusCode.Remove:          text = "RM"; break;
                    case BaseStationStatusCode.SignalLost:      text = "SL"; break;
                    default:                                    throw new NotImplementedException();
                }
                Assert.AreEqual(statusCode, BaseStationMessageHelper.ConvertToBaseStationStatusCode(text), statusCode.ToString());
            }
        }

        [TestMethod]
        public void ConvertToBaseStationStatusCode_Converts_Unknown_Text_Correctly()
        {
            foreach(var text in new string[] { null, "", "ad", " OK", "SL " }) {
                Assert.AreEqual(BaseStationStatusCode.None, BaseStationMessageHelper.ConvertToBaseStationStatusCode(text));
            }
        }

        [TestMethod]
        public void BaseStationMessageHelperTests_ConvertToString_BaseStationStatusCode_Converts_All_Types_Correctly()
        {
            foreach(var statusCode in Enum.GetValues<BaseStationStatusCode>()) {
                var text = BaseStationMessageHelper.ConvertToString(statusCode);
                switch(statusCode) {
                    case BaseStationStatusCode.Delete:          Assert.AreEqual("AD", text); break;
                    case BaseStationStatusCode.None:            Assert.AreEqual("", text); break;
                    case BaseStationStatusCode.OK:              Assert.AreEqual("OK", text); break;
                    case BaseStationStatusCode.PositionLost:    Assert.AreEqual("PL", text); break;
                    case BaseStationStatusCode.Remove:          Assert.AreEqual("RM", text); break;
                    case BaseStationStatusCode.SignalLost:      Assert.AreEqual("SL", text); break;
                    default:                                    throw new NotImplementedException();
                }
            }
        }

    }
}
