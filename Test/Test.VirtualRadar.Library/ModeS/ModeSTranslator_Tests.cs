// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Text;
using Test.Framework;
using VirtualRadar.Interface.Feeds;
using VirtualRadar.Interface.ModeS;
using VirtualRadar.Library.ModeS;

namespace Test.VirtualRadar.Library.ModeS
{
    [TestClass]
    public class ModeSTranslator_Tests
    {
        public TestContext TestContext { get; set; }

        private IModeSTranslator _Translator;
        private ReceiverStatistics _Statistics;

        [TestInitialize]
        public void TestInitialise()
        {
            _Statistics = new();
#pragma warning disable CS0618 // Type or member is obsolete (used to warn people not to instantiate directly)
            _Translator = new ModeSTranslator {
                Statistics = _Statistics
            };
#pragma warning restore CS0618
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ModeSTranslator_Translate_Throws_If_Statistics_Is_Not_Initialised()
        {
            _Translator.Statistics = null;
            _Translator.Translate(new byte[0], 0, null, false);
        }

        [TestMethod]
        public void ModeSTranslator_Translate_Decodes_Packets_Correctly()
        {
            var spreadsheet = new SpreadsheetTestData(TestData.RawDecodingTestData, "ModeSTranslate");
            spreadsheet.TestEveryRow(this, row => {
                var expectedValue = new SpreadsheetFieldValue(row, 7);

                var bits = row.String("Packet");
                var bytes = TestDataParser.ConvertBitStringToBytes(bits);

                var reply = _Translator.Translate(bytes.ToArray(), 0, null, false);

                foreach(var replyProperty in reply.GetType().GetProperties()) {
                    switch(replyProperty.Name) {
                        case nameof(ModeSMessage.ACASMessage):                          AssertExtra.SequenceEqualOrNull(expectedValue.GetBytes("MV"), reply.ACASMessage); break;
                        case nameof(ModeSMessage.Altitude):                             Assert.AreEqual(expectedValue.GetNInt("AC"), reply.Altitude); break;
                        case nameof(ModeSMessage.AltitudeIsMetric):                     Assert.AreEqual(expectedValue.GetNBool("AC:M"), reply.AltitudeIsMetric); break;
                        case nameof(ModeSMessage.ApplicationField):                     Assert.AreEqual(expectedValue.GetEnum<ApplicationField>("AF"), reply.ApplicationField); break;
                        case nameof(ModeSMessage.Capability):                           Assert.AreEqual(expectedValue.GetEnum<Capability>("CA"), reply.Capability); break;
                        case nameof(ModeSMessage.CommBMessage):                         AssertExtra.SequenceEqualOrNull(expectedValue.GetBytes("MB"), reply.CommBMessage); break;
                        case nameof(ModeSMessage.CommDMessage):                         AssertExtra.SequenceEqualOrNull(expectedValue.GetBytes("MD"), reply.CommDMessage); break;
                        case nameof(ModeSMessage.ControlField):                         Assert.AreEqual(expectedValue.GetEnum<ControlField>("CF"), reply.ControlField); break;
                        case nameof(ModeSMessage.CrossLinkCapability):                  Assert.AreEqual(expectedValue.GetNBool("CC"), reply.CrossLinkCapability); break;
                        case nameof(ModeSMessage.DownlinkFormat):                       Assert.AreEqual(row.Enum<DownlinkFormat>("DownlinkFormat"), reply.DownlinkFormat); break;
                        case nameof(ModeSMessage.DownlinkRequest):                      Assert.AreEqual(expectedValue.GetNByte("DR", true), reply.DownlinkRequest); break;
                        case nameof(ModeSMessage.DSegmentNumber):                       Assert.AreEqual(expectedValue.GetNByte("ND", true), reply.DSegmentNumber); break;
                        case nameof(ModeSMessage.ElmControl):                           Assert.AreEqual(expectedValue.GetEnum<ElmControl>("KE"), reply.ElmControl); break;
                        case nameof(ModeSMessage.ExtendedSquitterMessage):              AssertExtra.SequenceEqualOrNull(expectedValue.GetBytes("ME"), reply.ExtendedSquitterMessage); break;
                        case nameof(ModeSMessage.ExtendedSquitterSupplementaryMessage): AssertExtra.SequenceEqualOrNull(expectedValue.GetBytes("MEX"), reply.ExtendedSquitterSupplementaryMessage); break;
                        case nameof(ModeSMessage.FlightStatus):                         Assert.AreEqual(expectedValue.GetEnum<FlightStatus>("FS"), reply.FlightStatus); break;
                        case nameof(ModeSMessage.FormattedIcao24):                      Assert.AreEqual(row.String("Icao24"), reply.FormattedIcao24); break;
                        case nameof(ModeSMessage.Icao24):                               Assert.AreEqual(Convert.ToInt32(row.String("Icao24"), 16), reply.Icao24); break;
                        case nameof(ModeSMessage.Identity):                             Assert.AreEqual(expectedValue.GetNShort("ID"), reply.Identity); break;
                        case nameof(ModeSMessage.IsMlat):                               break;
                        case nameof(ModeSMessage.NonIcao24Address):                     Assert.AreEqual(expectedValue.GetNInt("AAX", true), reply.NonIcao24Address); break;
                        case nameof(ModeSMessage.ParityInterrogatorIdentifier):         Assert.AreEqual(expectedValue.GetNInt("PI", true), reply.ParityInterrogatorIdentifier); break;
                        case nameof(ModeSMessage.PossibleCallsign):                     Assert.AreEqual(expectedValue.GetString("PC"), reply.PossibleCallsign); break;
                        case nameof(ModeSMessage.ReplyInformation):                     Assert.AreEqual(expectedValue.GetNByte("RI", true), reply.ReplyInformation); break;
                        case nameof(ModeSMessage.SensitivityLevel):                     Assert.AreEqual(expectedValue.GetNByte("SL", true), reply.SensitivityLevel); break;
                        case nameof(ModeSMessage.UtilityMessage):                       Assert.AreEqual(expectedValue.GetNByte("UM", true), reply.UtilityMessage); break;
                        case nameof(ModeSMessage.VerticalStatus):                       Assert.AreEqual(expectedValue.GetEnum<VerticalStatus>("VS"), reply.VerticalStatus); break;
                        case nameof(ModeSMessage.SignalLevel):                          break;
                        default:                                                        throw new NotImplementedException();
                    }
                }
            });
        }

        [TestMethod]
        public void ModeSTranslator_Translate_Updates_Statistics()
        {
            var spreadsheet = new SpreadsheetTestData(TestData.RawDecodingTestData, "ModeSTranslate");
            spreadsheet.TestEveryRow(this, row => {
                var expectedValue = new SpreadsheetFieldValue(row, 7);

                var bits = row.String("Packet");
                var bytes = TestDataParser.ConvertBitStringToBytes(bits);

                var reply = _Translator.Translate(bytes.ToArray(), 0, null, false);

                Assert.AreEqual(1L, _Statistics.ModeSMessagesReceived);
                for(var i = 0;i < _Statistics.ModeSDFStatistics.Length;++i) {
                    Assert.AreEqual(i == (int)reply.DownlinkFormat ? 1L : 0L, _Statistics.ModeSDFStatistics[i].MessagesReceived, i.ToString());
                }
                bool isLongFrame = (int)reply.DownlinkFormat >= 16;
                Assert.AreEqual(!isLongFrame ? 1L : 0L, _Statistics.ModeSShortFrameMessagesReceived);
                Assert.AreEqual(isLongFrame ? 1L : 0L, _Statistics.ModeSLongFrameMessagesReceived);
            });
        }

        [TestMethod]
        public void ModeSTranslator_Translate_Ignores_Bytes_Before_Packet()
        {
            var bytes = TestDataParser.ConvertBitStringToBytes("00000000 01011 000 11111111 00000000 11111111 00000000 11111111 00000000");
            var reply = _Translator.Translate(bytes.ToArray(), 1, null, false);
            Assert.AreEqual(DownlinkFormat.AllCallReply, reply.DownlinkFormat);
            Assert.AreEqual(0x00FF00, reply.ParityInterrogatorIdentifier);
        }

        [TestMethod]
        public void ModeSTranslator_Translate_Ignores_Bytes_Before_DF24_Packet()
        {
            var bytes = TestDataParser.ConvertBitStringToBytes("00000000 11 0 0 1111 11111111 00000000 11001100 00110011 10101010 01010101 11110000 00001111 11100010 00011101 11111111 00000000 11111111");
            var reply = _Translator.Translate(bytes.ToArray(), 1, null, false);
            Assert.AreEqual(DownlinkFormat.CommD, reply.DownlinkFormat);
            Assert.IsTrue(new byte[] { 0xFF, 0x00, 0xCC, 0x33, 0xAA, 0x55, 0xF0, 0x0F, 0xE2, 0x1D }.SequenceEqual(reply.CommDMessage));
        }

        [TestMethod]
        public void ModeSTranslator_Translate_Copies_SignalLevel_To_ModeSMessage()
        {
            var bytes = TestDataParser.ConvertBitStringToBytes("00000000 01011 000 11111111 00000000 11111111 00000000 11111111 00000000");

            var reply = _Translator.Translate(bytes.ToArray(), 1, 123, false);
            Assert.AreEqual(123, reply.SignalLevel);

            reply = _Translator.Translate(bytes.ToArray(), 1, null, false);
            Assert.AreEqual(null, reply.SignalLevel);
        }

        [TestMethod]
        public void ModeSTranslator_Translate_Copies_IsMlat_To_ModeSMessage()
        {
            var bytes = TestDataParser.ConvertBitStringToBytes("00000000 01011 000 11111111 00000000 11111111 00000000 11111111 00000000");

            var reply = _Translator.Translate(bytes.ToArray(), 1, null, false);
            Assert.IsFalse(reply.IsMlat);

            reply = _Translator.Translate(bytes.ToArray(), 1, null, true);
            Assert.IsTrue(reply.IsMlat);
        }

        [TestMethod]
        public void ModeSTranslator_Translate_Returns_Null_If_Passed_Null_Buffer()
        {
            Assert.IsNull(_Translator.Translate(null, 0, null, false));
        }

        [TestMethod]
        public void ModeSTranslator_Translate_Returns_Null_If_Buffer_Is_Empty()
        {
            Assert.IsNull(_Translator.Translate(Array.Empty<byte>(), 0, null, false));
        }

        [TestMethod]
        public void ModeSTranslator_Translate_Returns_Null_If_Buffer_Does_Not_Contain_Enough_Bytes()
        {
            for(var df = 0;df < 25;++df) {
                var requiredLength = 7;
                switch(df) {
                    case 16:
                    case 17:
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                    case 24:
                        requiredLength = 14;
                        break;
                }

                var correctLength = new byte[requiredLength];
                var correctLengthPlusOne = new byte[requiredLength + 1];
                var shortLength = new byte[requiredLength - 1];

                var encodedDF = (byte)((byte)df << 3);
                correctLength[0] = correctLengthPlusOne[1] = shortLength[0] = encodedDF;

                var failMessage = String.Format("Failed on DF {0}", df);
                Assert.IsNotNull(_Translator.Translate(correctLength, 0, null, false), failMessage);
                Assert.IsNotNull(_Translator.Translate(correctLengthPlusOne, 1, null, false), failMessage);
                Assert.IsNull(_Translator.Translate(shortLength, 0, null, false), failMessage);
                correctLength[1] = encodedDF;
                Assert.IsNull(_Translator.Translate(correctLength, 1, null, false), failMessage);
            }
        }

        [TestMethod]
        [DataRow("01011", "101010101100110000110011000000000000000000000000")] // DF = 11
        [DataRow("10001", "11111111000000001111111111110000000011111111111100000000101010100101010111100011000000001111111100000000")] // DF = 17
        public void ModeSTranslator_Translate_Decodes_Comm_Capability_Correctly(string bitsBeforeCA, string bitsAfterCA)
        {
            foreach(Capability capability in Enum.GetValues(typeof(Capability))) {
                var bits = new StringBuilder(bitsBeforeCA);
                bits.Append(TestDataParser.ConvertToBitString((int)capability, 3));
                bits.Append(bitsAfterCA);

                var bytes = TestDataParser.ConvertBitStringToBytes(bits.ToString());
                var reply = _Translator.Translate(bytes.ToArray(), 0, null, false);

                Assert.AreEqual(capability, reply.Capability);
            }
        }

        [TestMethod]
        [DataRow("00100", "111110101011011110110001111111110000000011111111")] // DF4
        [DataRow("00101", "111111010101111110101010111111110000000011111111")] // DF5
        [DataRow("10100", "11111101010101111011000111111111010101011010101000000000111100000000111100110011111111110000000011111111")] // DF20
        [DataRow("10101", "11111101010101111011000111111111010101011010101000000000111100000000111100110011111111110000000011111111")] // DF21
        public void ModeSTranslator_Translate_Decodes_Flight_Status_Correctly(string bitsBeforeFS, string bitsAfterFS)
        {
            foreach(FlightStatus flightStatus in Enum.GetValues(typeof(FlightStatus))) {
                var bits = new StringBuilder(bitsBeforeFS);
                bits.Append(TestDataParser.ConvertToBitString((int)flightStatus, 3));
                bits.Append(bitsAfterFS);

                var bytes = TestDataParser.ConvertBitStringToBytes(bits.ToString());
                var reply = _Translator.Translate(bytes.ToArray(), 0, null, false);

                Assert.AreEqual(flightStatus, reply.FlightStatus);
            }
        }

        [TestMethod]
        [DataRow("0000001011100101000", "111111110000000011111111")]  // DF0
        [DataRow("0010000011111010101", "111111110000000011111111")]  // DF4
        [DataRow("1000000011100111100", "11111111000000001111000000001111101010100101010111100011111111110000000011111111")]  // DF16
        [DataRow("1010000011111010101", "11111111010101011010101000000000111100000000111100110011111111110000000011111111")]  // DF20
        public void ModeSTranslator_Translate_Decodes_Gillham_Altitudes_Correctly(string bitsBeforeAC, string bitsAfterAC)
        {
            var spreadsheet = new SpreadsheetTestData(TestData.GillhamAltitudeTable, "AllAltitudes");
            for(var rowNumber = 2;rowNumber < spreadsheet.Rows.Count;++rowNumber) {
                var row = spreadsheet.Rows[rowNumber];
                var altitude = row.Int(0);
                var bits = new StringBuilder(bitsBeforeAC);

                for(var bit = 0;bit < 13;++bit) {
                    if(bit == 6 || bit == 8) {
                        bits.Append('0');
                    } else {
                        var index = bit + 1;
                        if(bit > 6) --index;
                        if(bit > 8) --index;
                        bits.Append(row.String(index));
                    }
                }

                bits.Append(bitsAfterAC);

                var bytes = TestDataParser.ConvertBitStringToBytes(bits.ToString());
                var reply = _Translator.Translate(bytes.ToArray(), 0, null, false);

                Assert.AreEqual(altitude, reply.Altitude.Value);
            }
        }

        [TestMethod]
        [DataRow("0000001011100101000", "111111110000000011111111")]  // DF0
        [DataRow("0010000011111010101", "111111110000000011111111")]  // DF4
        [DataRow("1000000011100111100", "11111111000000001111000000001111101010100101010111100011111111110000000011111111")]  // DF16
        [DataRow("1010000011111010101", "11111111010101011010101000000000111100000000111100110011111111110000000011111111")]  // DF20
        public void ModeSTranslator_Translate_Does_Not_Throw_Exceptions_On_Invalid_Gillham_Altitudes(string bitsBeforeAC, string bitsAfterAC)
        {
            for(var acCode = 0;acCode < 2048;++acCode) {
                var bits = new StringBuilder(bitsBeforeAC);
                bits.Append(TestDataParser.ConvertToBitString(acCode, 11));
                bits.Insert(bitsBeforeAC.Length + 8, '0');
                bits.Insert(bitsBeforeAC.Length + 6, '0');
                bits.Append(bitsAfterAC);

                var bytes = TestDataParser.ConvertBitStringToBytes(bits.ToString());
                _Translator.Translate(bytes.ToArray(), 0, null, false);
            }
        }

        [TestMethod]
        [DataRow("0000001011100101000", "111111110000000011111111")]  // DF0
        [DataRow("0010000011111010101", "111111110000000011111111")]  // DF4
        [DataRow("1000000011100111100", "11111111000000001111000000001111101010100101010111100011111111110000000011111111")]  // DF16
        [DataRow("1010000011111010101", "11111111010101011010101000000000111100000000111100110011111111110000000011111111")]  // DF20
        public void ModeSTranslator_Translate_Decodes_QBitOne_Altitudes_Correctly(string bitsBeforeAC, string bitsAfterAC)
        {
            for(var altitude = -1000;altitude < 50200;altitude += 25) {
                var encodedAltitude = (altitude + 1000) / 25;
                var bits = new StringBuilder(bitsBeforeAC);
                var altitudeBits = TestDataParser.ConvertToBitString(encodedAltitude, 11);
                bits.AppendFormat("{0}0{1}1{2}", altitudeBits.Substring(0, 6), altitudeBits.Substring(6, 1), altitudeBits.Substring(7, 4));
                bits.Append(bitsAfterAC);

                var bytes = TestDataParser.ConvertBitStringToBytes(bits.ToString());
                var reply = _Translator.Translate(bytes.ToArray(), 0, null, false);

                Assert.AreEqual(altitude, reply.Altitude.Value);
            }
        }

        [TestMethod]
        [DataRow("0010100011111101010", "111111110000000011111111")]    // DF5
        [DataRow("1010100011111101010", "11111111010101011010101000000000111100000000111100110011111111110000000011111111")] // DF21
        public void ModeSTranslator_Translate_Decodes_Identity_Fields_Correctly(string bitsBeforeID, string bitsAfterID)
        {
            for(var a = 0;a < 8;++a) {
                for(var b = 0;b < 8;++b) {
                    for(var c = 0;c < 8;++c) {
                        for(var d = 0;d < 8;++d) {
                            var expectedIdentity = (short)((a * 1000) + (b * 100) + (c * 10) + d);

                            var aBits = TestDataParser.ConvertToBitString(a, 3);
                            var bBits = TestDataParser.ConvertToBitString(b, 3);
                            var cBits = TestDataParser.ConvertToBitString(c, 3);
                            var dBits = TestDataParser.ConvertToBitString(d, 3);
                            var bits = String.Format("{0}{1}{2}{3}{4}{5}0{6}{7}{8}{9}{10}{11}",
                                cBits[2], aBits[2], cBits[1], aBits[1], cBits[0], aBits[0],
                                bBits[2], dBits[2], bBits[1], dBits[1], bBits[0], dBits[0]);
                            bits = $"{bitsBeforeID}{bits}{bitsAfterID}";

                            var bytes = TestDataParser.ConvertBitStringToBytes(bits);
                            var reply = _Translator.Translate(bytes.ToArray(), 0, null, false);

                            Assert.AreEqual(expectedIdentity, reply.Identity);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void ModeSTranslator_Translate_Fills_Properties_For_Extended_Squitter_From_Non_Transponder_According_To_Control_Field_Content()
        {
            foreach(ControlField controlField in Enum.GetValues(typeof(ControlField))) {
                var bits = new StringBuilder("10010");
                bits.Append(TestDataParser.ConvertToBitString((int)controlField, 3));
                bits.Append("11111111000000001111111111110000000011111111111100000000101010100101010111100011000000001111111100000000");

                var bytes = TestDataParser.ConvertBitStringToBytes(bits.ToString());
                var reply = _Translator.Translate(bytes.ToArray(), 0, null, false);

                Assert.AreEqual(controlField, reply.ControlField);
                switch(controlField) {
                    case ControlField.AdsbDeviceTransmittingIcao24:
                    case ControlField.AdsbRebroadcastOfExtendedSquitter:
                        Assert.AreNotEqual(0, reply.Icao24);
                        Assert.IsNull(reply.NonIcao24Address);
                        Assert.IsNotNull(reply.ExtendedSquitterMessage);
                        Assert.IsNull(reply.ExtendedSquitterSupplementaryMessage);
                        Assert.IsNotNull(reply.ParityInterrogatorIdentifier);
                        break;
                    case ControlField.CoarseFormatTisb:
                    case ControlField.FineFormatTisb:
                    case ControlField.AdsbDeviceNotTransmittingIcao24:
                        Assert.AreEqual(0, reply.Icao24);
                        Assert.IsNotNull(reply.NonIcao24Address);
                        Assert.IsNotNull(reply.ExtendedSquitterMessage);
                        Assert.IsNull(reply.ExtendedSquitterSupplementaryMessage);
                        Assert.IsNotNull(reply.ParityInterrogatorIdentifier);
                        break;
                    default:
                        Assert.AreEqual(0, reply.Icao24);
                        Assert.IsNull(reply.NonIcao24Address);
                        Assert.IsNull(reply.ExtendedSquitterMessage);
                        Assert.IsNotNull(reply.ExtendedSquitterSupplementaryMessage);
                        Assert.IsNotNull(reply.ParityInterrogatorIdentifier);
                        break;
                }
            }
        }

        [TestMethod]
        public void ModeSTranslator_Translate_Fills_Properties_For_Military_Extended_Squitter_According_To_Application_Field_Content()
        {
            foreach(ApplicationField applicationField in Enum.GetValues(typeof(ApplicationField))) {
                var bits = new StringBuilder("10011");
                bits.Append(TestDataParser.ConvertToBitString((int)applicationField, 3));
                bits.Append("11111111000000001111111111110000000011111111111100000000101010100101010111100011000000001111111100000000");

                var bytes = TestDataParser.ConvertBitStringToBytes(bits.ToString());
                var reply = _Translator.Translate(bytes.ToArray(), 0, null, false);

                Assert.AreEqual(applicationField, reply.ApplicationField);
                switch(applicationField) {
                    case ApplicationField.ADSB:
                        Assert.AreNotEqual(0, reply.Icao24);
                        Assert.IsNotNull(reply.ExtendedSquitterMessage);
                        Assert.IsNull(reply.ExtendedSquitterSupplementaryMessage);
                        Assert.IsNotNull(reply.ParityInterrogatorIdentifier);
                        break;
                    default:
                        Assert.AreEqual(0, reply.Icao24);
                        Assert.IsNull(reply.ExtendedSquitterMessage);
                        Assert.IsNotNull(reply.ExtendedSquitterSupplementaryMessage);
                        Assert.IsNull(reply.ParityInterrogatorIdentifier);
                        break;
                }
            }
        }

        [TestMethod]
        [DataRow("10100000111111010101011110110001", "111111110000000011111111")] // DF20
        [DataRow("10101000111111010101100110110010", "111111110000000011111111")] // DF21
        public void ModeSTranslator_Translate_Extracts_Possible_BDS20_Callsigns_Correctly(string bitsBefore, string bitsAfter)
        {
            foreach(var ch in "ABCDEFGHIJKLMNOPQRSTUVWXYZ 0123456789") {
                var expectedIdentification = new String(ch, 8);
                var encodedCh = ch >= 'A' ? (ch - 'A') + 1 : ch == ' ' ? 32 : 48 + (ch - '0');
                var bits = new StringBuilder(bitsBefore);
                bits.Append("0010 0000"); // BDS 2,0
                for(var i = 0;i < 8;++i) {
                    bits.Append(TestDataParser.ConvertToBitString(encodedCh, 6));
                }
                bits.Append(bitsAfter);

                var bytes = TestDataParser.ConvertBitStringToBytes(bits.ToString());
                var reply = _Translator.Translate(bytes.ToArray(), 0, null, false);

                Assert.AreEqual(expectedIdentification, reply.PossibleCallsign, "Failed on character '{0}'", ch);
            }
        }
    }
}
