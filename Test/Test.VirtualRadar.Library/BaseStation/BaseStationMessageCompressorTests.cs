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
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Library;
using Test.Framework;
using InterfaceFactory;

namespace Test.VirtualRadar.Library.BaseStation
{
    [TestClass]
    public class BaseStationMessageCompressorTests
    {
        #region TestContext, fields, initialise, cleanup
        public TestContext TestContext { get; set; }
        private IBaseStationMessageCompressor _Compressor;
        private BaseStationMessage _Message;

        [TestInitialize]
        public void TestInitialise()
        {
            _Compressor = Factory.Resolve<IBaseStationMessageCompressor>();

            _Message = new BaseStationMessage();
            _Message.MessageType = BaseStationMessageType.Transmission;
            _Message.TransmissionType = BaseStationTransmissionType.SurveillanceAlt;
            _Message.Icao24 = "405012";
        }
        #endregion

        #region Compress / Decompress
        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "MessageCompressor$")]
        public void BaseStationMessageCompressor_Compress_And_Decompress_Work_As_Expected()
        {
            ExcelWorksheetData worksheet = new ExcelWorksheetData(TestContext);

            var messageIn = new BaseStationMessage();
            messageIn.MessageType = worksheet.ParseEnum<BaseStationMessageType>("MessageType");
            messageIn.TransmissionType = worksheet.ParseEnum<BaseStationTransmissionType>("TransmissionType");
            messageIn.StatusCode = worksheet.ParseEnum<BaseStationStatusCode>("StatusCode");
            messageIn.Icao24 = worksheet.EString("Icao24");
            messageIn.SessionId = worksheet.Int("SessionId");
            messageIn.AircraftId = worksheet.Int("AircraftId");
            messageIn.FlightId = worksheet.Int("FlightId");
            messageIn.MessageGenerated = worksheet.DateTime("MessageGenerated");
            messageIn.MessageLogged = worksheet.DateTime("MessageLogged");
            messageIn.Callsign = worksheet.String("Callsign");
            messageIn.Altitude = worksheet.NInt("Altitude");
            messageIn.GroundSpeed = worksheet.NInt("GroundSpeed");
            messageIn.Track = worksheet.NFloat("Track");
            messageIn.Latitude = worksheet.NDouble("Latitude");
            messageIn.Longitude = worksheet.NDouble("Longitude");
            messageIn.VerticalRate = worksheet.NInt("VerticalRate");
            messageIn.Squawk = worksheet.NInt("Squawk");
            messageIn.SquawkHasChanged = worksheet.NBool("SquawkHasChanged");
            messageIn.Emergency = worksheet.NBool("Emergency");
            messageIn.IdentActive = worksheet.NBool("IdentActive");
            messageIn.OnGround = worksheet.NBool("OnGround");

            int expectedLength = worksheet.Int("Length");

            byte[] bytes = _Compressor.Compress(messageIn);
            Assert.AreEqual(expectedLength, bytes.Length);

            DateTime earliestDate = DateTime.Now;
            BaseStationMessage messageOut = _Compressor.Decompress(bytes);
            DateTime latestDate = DateTime.Now;

            if(bytes.Length == 0) Assert.IsNull(messageOut);
            else {
                Assert.AreEqual(messageIn.MessageType, messageOut.MessageType);
                Assert.AreEqual(messageIn.TransmissionType, messageOut.TransmissionType);
                Assert.AreEqual(BaseStationStatusCode.None, messageOut.StatusCode);
                Assert.AreEqual(messageIn.Icao24, messageOut.Icao24);
                Assert.AreEqual(0, messageOut.SessionId);
                Assert.AreEqual(0, messageOut.AircraftId);
                Assert.AreEqual(0, messageOut.FlightId);
                Assert.AreEqual((double)earliestDate.Ticks, (double)messageOut.MessageGenerated.Ticks, (double)latestDate.Ticks - (double)earliestDate.Ticks);
                Assert.AreEqual((double)earliestDate.Ticks, (double)messageOut.MessageLogged.Ticks, (double)latestDate.Ticks - (double)earliestDate.Ticks);
                Assert.AreEqual(messageIn.Callsign, messageOut.Callsign);
                Assert.AreEqual(messageIn.Altitude, messageOut.Altitude);
                Assert.AreEqual(messageIn.GroundSpeed, messageOut.GroundSpeed);
                Assert.AreEqual(messageIn.Track, messageOut.Track);
                if(messageIn.Latitude == null) Assert.IsNull(messageOut.Latitude);
                else Assert.AreEqual(messageIn.Latitude.Value, messageOut.Latitude.Value, 0.000001);
                if(messageIn.Longitude == null) Assert.IsNull(messageOut.Longitude);
                else Assert.AreEqual(messageIn.Longitude.Value, messageOut.Longitude.Value, 0.000001);
                Assert.AreEqual(messageIn.VerticalRate, messageOut.VerticalRate);
                Assert.AreEqual(messageIn.Squawk, messageOut.Squawk);
                Assert.AreEqual(messageIn.SquawkHasChanged, messageOut.SquawkHasChanged);
                Assert.AreEqual(messageIn.Emergency, messageOut.Emergency);
                Assert.AreEqual(messageIn.IdentActive, messageOut.IdentActive);
                Assert.AreEqual(messageIn.OnGround, messageOut.OnGround);
            }
        }

        [TestMethod]
        public void BaseStationMessageCompressor_Compress_Writes_Length_Into_First_Byte()
        {
            var bytes = _Compressor.Compress(_Message);
            Assert.AreEqual(bytes.Length, (int)bytes[0]);
        }
        #endregion

        #region IsCompressedMessage
        [TestMethod]
        public void BaseStationMessageCompressor_IsCompressedMessage_Returns_True_If_Bytes_Represent_Compressed_Message()
        {
            Assert.IsTrue(_Compressor.IsCompressedMessage(_Compressor.Compress(_Message)));
        }

        [TestMethod]
        public void BaseStationMessageCompresser_IsCompressedMessage_Returns_False_If_Passed_Null()
        {
            Assert.IsFalse(_Compressor.IsCompressedMessage(null));
        }

        [TestMethod]
        public void BaseStationMessageCompresser_IsCompressedMessage_Returns_False_If_Passed_Empty_Array()
        {
            Assert.IsFalse(_Compressor.IsCompressedMessage(new byte[] {}));
        }

        [TestMethod]
        public void BaseStationMessageCompressor_IsCompressedMessage_Returns_False_If_Length_Does_Not_Match_Array_Length()
        {
            var bytes = _Compressor.Compress(_Message);
            bytes[0] = 0xff;
            Assert.AreEqual(false, _Compressor.IsCompressedMessage(bytes));
        }

        [TestMethod]
        public void BaseStationMessageCompressor_IsCompressedMessage_Returns_False_If_Array_Is_Shorter_Than_Three()
        {
            for(int length = 1;length < 3;++length) {
                var bytes = new byte[length];
                bytes[0] = (byte)length;
                Assert.AreEqual(false, _Compressor.IsCompressedMessage(bytes));
            }
        }

        [TestMethod]
        public void BaseStationMessageCompressor_IsCompressedMessage_Returns_False_If_Any_NonZero_Byte_Is_Set_To_Zero()
        {
            var bytes = _Compressor.Compress(_Message);
            for(int i = 1;i < bytes.Length;++i) {
                if(bytes[i] != 0) {
                    var copy = new byte[bytes.Length];
                    Array.Copy(bytes, copy, bytes.Length);
                    copy[i] = 0;

                    Assert.AreEqual(false, _Compressor.IsCompressedMessage(copy), i.ToString());
                }
            }
        }

        [TestMethod]
        public void BaseStationMessageCompressor_IsCompressedMessage_Returns_False_If_Bytes_Transposed()
        {
            var bytes = _Compressor.Compress(_Message);
            for(int i = 1;i < bytes.Length;++i) {
                for(int s = bytes.Length - 1;s > 0;--s) {
                    if(bytes[i] != bytes[s]) {
                        var copy = new byte[bytes.Length];
                        Array.Copy(bytes, copy, bytes.Length);
                        copy[i] = bytes[s];
                        copy[s] = bytes[i];

                        Assert.AreEqual(false, _Compressor.IsCompressedMessage(copy), "Transposed positions {0} and {1}", i, s);
                    }
                }
            }
        }
        #endregion
    }
}
