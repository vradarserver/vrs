// Copyright © 2012 onwards, Andrew Whewell
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
using VirtualRadar.Interface.ModeS;
using InterfaceFactory;
using Test.Framework;
using System.IO;
using VirtualRadar.Interface;
using Moq;

namespace Test.VirtualRadar.Library.ModeS
{
    [TestClass]
    public class ModeSTranslatorTests
    {
        public TestContext TestContext { get; set; }

        private IClassFactory _OriginalFactory;
        private IModeSTranslator _Translator;
        private Mock<IStatistics> _Statistics;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalFactory = Factory.TakeSnapshot();

            _Statistics = StatisticsHelper.CreateLockableStatistics();
            _Translator = Factory.Resolve<IModeSTranslator>();
            _Translator.Statistics = _Statistics.Object;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalFactory);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ModeSTranslator_Translate_Throws_If_Statistics_Is_Not_Initialised()
        {
            _Translator.Statistics = null;
            _Translator.Translate(new byte[0], 0, null, false);
        }

        [TestMethod]
        [DataSource("Data Source='RawDecodingTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "ModeSTranslate$")]
        public void ModeSTranslator_Translate_Decodes_Packets_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            var expectedValue = new SpreadsheetFieldValue(worksheet, 7);

            var bits = worksheet.String("Packet");
            var bytes = TestUtilities.ConvertBitStringToBytes(bits);

            var reply = _Translator.Translate(bytes.ToArray(), 0, null, false);

            foreach(var replyProperty in reply.GetType().GetProperties()) {
                switch(replyProperty.Name) {
                    case "ACASMessage":                          Assert.IsTrue(TestUtilities.SequenceEqual(expectedValue.GetBytes("MV"), reply.ACASMessage)); break;
                    case "Altitude":                             Assert.AreEqual(expectedValue.GetNInt("AC"), reply.Altitude); break;
                    case "AltitudeIsMetric":                     Assert.AreEqual(expectedValue.GetNBool("AC:M"), reply.AltitudeIsMetric); break;
                    case "ApplicationField":                     Assert.AreEqual(expectedValue.GetEnum<ApplicationField>("AF"), reply.ApplicationField); break;
                    case "Capability":                           Assert.AreEqual(expectedValue.GetEnum<Capability>("CA"), reply.Capability); break;
                    case "CommBMessage":                         Assert.IsTrue(TestUtilities.SequenceEqual(expectedValue.GetBytes("MB"), reply.CommBMessage)); break;
                    case "CommDMessage":                         Assert.IsTrue(TestUtilities.SequenceEqual(expectedValue.GetBytes("MD"), reply.CommDMessage)); break;
                    case "ControlField":                         Assert.AreEqual(expectedValue.GetEnum<ControlField>("CF"), reply.ControlField); break;
                    case "CrossLinkCapability":                  Assert.AreEqual(expectedValue.GetNBool("CC"), reply.CrossLinkCapability); break;
                    case "DownlinkFormat":                       Assert.AreEqual(worksheet.ParseEnum<DownlinkFormat>("DownlinkFormat"), reply.DownlinkFormat); break;
                    case "DownlinkRequest":                      Assert.AreEqual(expectedValue.GetNByte("DR", true), reply.DownlinkRequest); break;
                    case "DSegmentNumber":                       Assert.AreEqual(expectedValue.GetNByte("ND", true), reply.DSegmentNumber); break;
                    case "ElmControl":                           Assert.AreEqual(expectedValue.GetEnum<ElmControl>("KE"), reply.ElmControl); break;
                    case "ExtendedSquitterMessage":              Assert.IsTrue(TestUtilities.SequenceEqual(expectedValue.GetBytes("ME"), reply.ExtendedSquitterMessage)); break;
                    case "ExtendedSquitterSupplementaryMessage": Assert.IsTrue(TestUtilities.SequenceEqual(expectedValue.GetBytes("MEX"), reply.ExtendedSquitterSupplementaryMessage)); break;
                    case "FlightStatus":                         Assert.AreEqual(expectedValue.GetEnum<FlightStatus>("FS"), reply.FlightStatus); break;
                    case "FormattedIcao24":                      Assert.AreEqual(worksheet.String("Icao24"), reply.FormattedIcao24); break;
                    case "Icao24":                               Assert.AreEqual(Convert.ToInt32(worksheet.String("Icao24"), 16), reply.Icao24); break;
                    case "Identity":                             Assert.AreEqual(expectedValue.GetNShort("ID"), reply.Identity); break;
                    case "IsMlat":                               break;
                    case "NonIcao24Address":                     Assert.AreEqual(expectedValue.GetNInt("AAX", true), reply.NonIcao24Address); break;
                    case "ParityInterrogatorIdentifier":         Assert.AreEqual(expectedValue.GetNInt("PI", true), reply.ParityInterrogatorIdentifier); break;
                    case "PossibleCallsign":                     Assert.AreEqual(expectedValue.GetString("PC"), reply.PossibleCallsign); break;
                    case "ReplyInformation":                     Assert.AreEqual(expectedValue.GetNByte("RI", true), reply.ReplyInformation); break;
                    case "SensitivityLevel":                     Assert.AreEqual(expectedValue.GetNByte("SL", true), reply.SensitivityLevel); break;
                    case "UtilityMessage":                       Assert.AreEqual(expectedValue.GetNByte("UM", true), reply.UtilityMessage); break;
                    case "VerticalStatus":                       Assert.AreEqual(expectedValue.GetEnum<VerticalStatus>("VS"), reply.VerticalStatus); break;
                    case "SignalLevel":                          break;
                    default:                                     throw new NotImplementedException();
                }
            }
        }

        [TestMethod]
        [DataSource("Data Source='RawDecodingTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "ModeSTranslate$")]
        public void ModeSTranslator_Translate_Updates_Statistics()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            var expectedValue = new SpreadsheetFieldValue(worksheet, 7);

            var bits = worksheet.String("Packet");
            var bytes = TestUtilities.ConvertBitStringToBytes(bits);

            var reply = _Translator.Translate(bytes.ToArray(), 0, null, false);

            Assert.AreEqual(1L, _Statistics.Object.ModeSMessagesReceived);
            for(var i = 0;i < _Statistics.Object.ModeSDFStatistics.Length;++i) {
                Assert.AreEqual(i == (int)reply.DownlinkFormat ? 1L : 0L, _Statistics.Object.ModeSDFStatistics[i].MessagesReceived, i.ToString());
            }
            bool isLongFrame = (int)reply.DownlinkFormat >= 16;
            Assert.AreEqual(!isLongFrame ? 1L : 0L, _Statistics.Object.ModeSShortFrameMessagesReceived);
            Assert.AreEqual(isLongFrame ? 1L : 0L, _Statistics.Object.ModeSLongFrameMessagesReceived);
        }

        [TestMethod]
        public void ModeSTranslator_Translate_Ignores_Bytes_Before_Packet()
        {
            List<byte> bytes = TestUtilities.ConvertBitStringToBytes("00000000 01011 000 11111111 00000000 11111111 00000000 11111111 00000000");
            var reply = _Translator.Translate(bytes.ToArray(), 1, null, false);
            Assert.AreEqual(DownlinkFormat.AllCallReply, reply.DownlinkFormat);
            Assert.AreEqual(0x00FF00, reply.ParityInterrogatorIdentifier);
        }

        [TestMethod]
        public void ModeSTranslator_Translate_Ignores_Bytes_Before_DF24_Packet()
        {
            List<byte> bytes = TestUtilities.ConvertBitStringToBytes("00000000 11 0 0 1111 11111111 00000000 11001100 00110011 10101010 01010101 11110000 00001111 11100010 00011101 11111111 00000000 11111111");
            var reply = _Translator.Translate(bytes.ToArray(), 1, null, false);
            Assert.AreEqual(DownlinkFormat.CommD, reply.DownlinkFormat);
            Assert.IsTrue(new byte[] { 0xFF, 0x00, 0xCC, 0x33, 0xAA, 0x55, 0xF0, 0x0F, 0xE2, 0x1D }.SequenceEqual(reply.CommDMessage));
        }

        [TestMethod]
        public void ModeSTranslator_Translate_Copies_SignalLevel_To_ModeSMessage()
        {
            List<byte> bytes = TestUtilities.ConvertBitStringToBytes("00000000 01011 000 11111111 00000000 11111111 00000000 11111111 00000000");

            var reply = _Translator.Translate(bytes.ToArray(), 1, 123, false);
            Assert.AreEqual(123, reply.SignalLevel);

            reply = _Translator.Translate(bytes.ToArray(), 1, null, false);
            Assert.AreEqual(null, reply.SignalLevel);
        }

        [TestMethod]
        public void ModeSTranslator_Translate_Copies_IsMlat_To_ModeSMessage()
        {
            List<byte> bytes = TestUtilities.ConvertBitStringToBytes("00000000 01011 000 11111111 00000000 11111111 00000000 11111111 00000000");

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
            Assert.IsNull(_Translator.Translate(new byte[] {}, 0, null, false));
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

                byte[] correctLength = new byte[requiredLength];
                byte[] correctLengthPlusOne = new byte[requiredLength + 1];
                byte[] shortLength = new byte[requiredLength - 1];

                byte encodedDF = (byte)((byte)df << 3);
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
        public void ModeSTranslator_Translate_Decodes_Comm_Capability_Correctly()
        {
            DoCapabilityCheck("01011", "101010101100110000110011000000000000000000000000"); // DF = 11
            DoCapabilityCheck("10001", "11111111000000001111111111110000000011111111111100000000101010100101010111100011000000001111111100000000"); // DF = 17
        }

        private void DoCapabilityCheck(string bitsBeforeCA, string bitsAfterCA)
        {
            foreach(Capability capability in Enum.GetValues(typeof(Capability))) {
                var bits = new StringBuilder(bitsBeforeCA);
                bits.Append(TestUtilities.ConvertToBitString((int)capability, 3));
                bits.Append(bitsAfterCA);

                var bytes = TestUtilities.ConvertBitStringToBytes(bits.ToString());
                var reply = _Translator.Translate(bytes.ToArray(), 0, null, false);

                Assert.AreEqual(capability, reply.Capability);
            }
        }

        [TestMethod]
        public void ModeSTranslator_Translate_Decodes_Flight_Status_Correctly()
        {
            DoFlightStatusCheck("00100", "111110101011011110110001111111110000000011111111"); // DF4
            DoFlightStatusCheck("00101", "111111010101111110101010111111110000000011111111"); // DF5
            DoFlightStatusCheck("10100", "11111101010101111011000111111111010101011010101000000000111100000000111100110011111111110000000011111111"); // DF20
            DoFlightStatusCheck("10101", "11111101010101111011000111111111010101011010101000000000111100000000111100110011111111110000000011111111"); // DF21
        }

        private void DoFlightStatusCheck(string bitsBeforeFS, string bitsAfterFS)
        {
            foreach(FlightStatus flightStatus in Enum.GetValues(typeof(FlightStatus))) {
                var bits = new StringBuilder(bitsBeforeFS);
                bits.Append(TestUtilities.ConvertToBitString((int)flightStatus, 3));
                bits.Append(bitsAfterFS);

                var bytes = TestUtilities.ConvertBitStringToBytes(bits.ToString());
                var reply = _Translator.Translate(bytes.ToArray(), 0, null, false);

                Assert.AreEqual(flightStatus, reply.FlightStatus);
            }
        }

        [TestMethod]
        public void ModeSTranslator_Translate_Decodes_Gillham_Altitudes_Correctly()
        {
            DoGillhamAltitudeDecodingCheck("0000001011100101000", "111111110000000011111111");  // DF0
            DoGillhamAltitudeDecodingCheck("0010000011111010101", "111111110000000011111111");  // DF4
            DoGillhamAltitudeDecodingCheck("1000000011100111100", "11111111000000001111000000001111101010100101010111100011111111110000000011111111");  // DF16
            DoGillhamAltitudeDecodingCheck("1010000011111010101", "11111111010101011010101000000000111100000000111100110011111111110000000011111111");  // DF20
        }

        private void DoGillhamAltitudeDecodingCheck(string bitsBeforeAC, string bitsAfterAC)
        {
            var fileName = Path.Combine(TestContext.TestDeploymentDir, "GillhamAltitudeTable.csv");
            var lines = File.ReadAllLines(fileName);
            for(var lineNumber = 2;lineNumber < lines.Length;++lineNumber) {
                var cells = lines[lineNumber].Split(',');
                var altitude = int.Parse(cells[0]);
                var bits = new StringBuilder(bitsBeforeAC);

                for(int bit = 0;bit < 13;++bit) {
                    if(bit == 6 || bit == 8) bits.Append('0');
                    else {
                        var index = bit + 1;
                        if(bit > 6) --index;
                        if(bit > 8) --index;
                        bits.Append(cells[index]);
                    }
                }

                bits.Append(bitsAfterAC);

                var bytes = TestUtilities.ConvertBitStringToBytes(bits.ToString());
                var reply = _Translator.Translate(bytes.ToArray(), 0, null, false);

                Assert.AreEqual(altitude, reply.Altitude.Value);
            }
        }

        [TestMethod]
        public void ModeSTranslator_Translate_Does_Not_Throw_Exceptions_On_Invalid_Gillham_Altitudes()
        {
            DoGillhamGracefulFailCheck("0000001011100101000", "111111110000000011111111");  // DF0
            DoGillhamGracefulFailCheck("0010000011111010101", "111111110000000011111111");  // DF4
            DoGillhamGracefulFailCheck("1000000011100111100", "11111111000000001111000000001111101010100101010111100011111111110000000011111111");  // DF16
            DoGillhamGracefulFailCheck("1010000011111010101", "11111111010101011010101000000000111100000000111100110011111111110000000011111111");  // DF20
        }

        private void DoGillhamGracefulFailCheck(string bitsBeforeAC, string bitsAfterAC)
        {
            for(var acCode = 0;acCode < 2048;++acCode) {
                var bits = new StringBuilder(bitsBeforeAC);
                bits.Append(TestUtilities.ConvertToBitString(acCode, 11));
                bits.Insert(bitsBeforeAC.Length + 8, '0');
                bits.Insert(bitsBeforeAC.Length + 6, '0');
                bits.Append(bitsAfterAC);

                var bytes = TestUtilities.ConvertBitStringToBytes(bits.ToString());
                _Translator.Translate(bytes.ToArray(), 0, null, false);
            }
        }

        [TestMethod]
        public void ModeSTranslator_Translate_Decodes_QBitOne_Altitudes_Correctly()
        {
            DoQBitOneAltitudeCheck("0000001011100101000", "111111110000000011111111");  // DF0
            DoQBitOneAltitudeCheck("0010000011111010101", "111111110000000011111111");  // DF4
            DoQBitOneAltitudeCheck("1000000011100111100", "11111111000000001111000000001111101010100101010111100011111111110000000011111111");  // DF16
            DoQBitOneAltitudeCheck("1010000011111010101", "11111111010101011010101000000000111100000000111100110011111111110000000011111111");  // DF20
        }

        private void DoQBitOneAltitudeCheck(string bitsBeforeAC, string bitsAfterAC)
        {
            for(int altitude = -1000;altitude < 50200;altitude += 25) {
                var encodedAltitude = (altitude + 1000) / 25;
                var bits = new StringBuilder(bitsBeforeAC);
                var altitudeBits = TestUtilities.ConvertToBitString(encodedAltitude, 11);
                bits.AppendFormat("{0}0{1}1{2}", altitudeBits.Substring(0, 6), altitudeBits.Substring(6, 1), altitudeBits.Substring(7, 4));
                bits.Append(bitsAfterAC);

                var bytes = TestUtilities.ConvertBitStringToBytes(bits.ToString());
                var reply = _Translator.Translate(bytes.ToArray(), 0, null, false);

                Assert.AreEqual(altitude, reply.Altitude.Value);
            }
        }

        [TestMethod]
        public void ModeSTranslator_Translate_Decodes_Identity_Fields_Correctly()
        {
            DoIdentityCheck("0010100011111101010", "111111110000000011111111");    // DF5
            DoIdentityCheck("1010100011111101010", "11111111010101011010101000000000111100000000111100110011111111110000000011111111"); // DF21
        }

        private void DoIdentityCheck(string bitsBeforeID, string bitsAfterID)
        {
            for(int a = 0;a < 8;++a) {
                for(int b = 0;b < 8;++b) {
                    for(int c = 0;c < 8;++c) {
                        for(int d = 0;d < 8;++d) {
                            short expectedIdentity = (short)((a * 1000) + (b * 100) + (c * 10) + d);

                            var aBits = TestUtilities.ConvertToBitString(a, 3);
                            var bBits = TestUtilities.ConvertToBitString(b, 3);
                            var cBits = TestUtilities.ConvertToBitString(c, 3);
                            var dBits = TestUtilities.ConvertToBitString(d, 3);
                            var bits = String.Format("{0}{1}{2}{3}{4}{5}0{6}{7}{8}{9}{10}{11}",
                                cBits[2], aBits[2], cBits[1], aBits[1], cBits[0], aBits[0],
                                bBits[2], dBits[2], bBits[1], dBits[1], bBits[0], dBits[0]);
                            bits = String.Format("{0}{1}{2}", bitsBeforeID, bits, bitsAfterID);

                            var bytes = TestUtilities.ConvertBitStringToBytes(bits);
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
                bits.Append(TestUtilities.ConvertToBitString((int)controlField, 3));
                bits.Append("11111111000000001111111111110000000011111111111100000000101010100101010111100011000000001111111100000000");

                var bytes = TestUtilities.ConvertBitStringToBytes(bits.ToString());
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
                bits.Append(TestUtilities.ConvertToBitString((int)applicationField, 3));
                bits.Append("11111111000000001111111111110000000011111111111100000000101010100101010111100011000000001111111100000000");

                var bytes = TestUtilities.ConvertBitStringToBytes(bits.ToString());
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
        public void ModeSTranslator_Translate_Extracts_Possible_BDS20_Callsigns_Correctly()
        {
            DoPossibleCallsignCheck("10100000111111010101011110110001", "111111110000000011111111"); // DF20
            DoPossibleCallsignCheck("10101000111111010101100110110010", "111111110000000011111111"); // DF21
        }

        private void DoPossibleCallsignCheck(string bitsBefore, string bitsAfter)
        {
            foreach(var ch in "ABCDEFGHIJKLMNOPQRSTUVWXYZ 0123456789") {
                var expectedIdentification = new String(ch, 8);
                var encodedCh = ch >= 'A' ? (ch - 'A') + 1 : ch == ' ' ? 32 : 48 + (ch - '0');
                var bits = new StringBuilder(bitsBefore);
                bits.Append("0010 0000"); // BDS 2,0
                for(var i = 0;i < 8;++i) {
                    bits.Append(TestUtilities.ConvertToBitString(encodedCh, 6));
                }
                bits.Append(bitsAfter);

                var bytes = TestUtilities.ConvertBitStringToBytes(bits.ToString());
                var reply = _Translator.Translate(bytes.ToArray(), 0, null, false);

                Assert.AreEqual(expectedIdentification, reply.PossibleCallsign, "Failed on character '{0}'", ch);
            }
        }
    }
}
