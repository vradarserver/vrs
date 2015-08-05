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
using Test.Framework;

namespace Test.VirtualRadar.Interface.ModeS
{
    [TestClass]
    public class ModeSMessageTests
    {
        private ModeSMessage _Message;

        [TestInitialize]
        public void TestInitialise()
        {
            _Message = new ModeSMessage();
        }

        [TestMethod]
        public void ModeSMessage_Initialises_To_Known_Values_And_Properties_Work()
        {
            TestUtilities.TestProperty(_Message, r => r.ACASMessage, null, new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 });
            TestUtilities.TestProperty(_Message, r => r.Altitude, null, 99999);
            TestUtilities.TestProperty(_Message, r => r.AltitudeIsMetric, null, false);
            TestUtilities.TestProperty(_Message, r => r.ApplicationField, null, ApplicationField.ADSB);
            TestUtilities.TestProperty(_Message, r => r.Capability, null, Capability.HasCommACommBAndAirborne);
            TestUtilities.TestProperty(_Message, r => r.CommBMessage, null, new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 });
            TestUtilities.TestProperty(_Message, r => r.CommDMessage, null, new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a });
            TestUtilities.TestProperty(_Message, r => r.ControlField, null, ControlField.AdsbDeviceTransmittingIcao24);
            TestUtilities.TestProperty(_Message, r => r.CrossLinkCapability, null, true);
            TestUtilities.TestProperty(_Message, r => r.DownlinkFormat, DownlinkFormat.ShortAirToAirSurveillance, DownlinkFormat.AllCallReply);
            TestUtilities.TestProperty(_Message, r => r.DownlinkRequest, null, (byte)255);
            TestUtilities.TestProperty(_Message, r => r.DSegmentNumber, null, (byte)7);
            TestUtilities.TestProperty(_Message, r => r.ElmControl, null, ElmControl.DownlinkTransmission);
            TestUtilities.TestProperty(_Message, r => r.ExtendedSquitterMessage, null, new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 });
            TestUtilities.TestProperty(_Message, r => r.ExtendedSquitterSupplementaryMessage, null, new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a });
            TestUtilities.TestProperty(_Message, r => r.FlightStatus, null, FlightStatus.OnGround);
            TestUtilities.TestProperty(_Message, r => r.Icao24, 0, 0xffffff);
            TestUtilities.TestProperty(_Message, r => r.Identity, null, (short)4095);
            TestUtilities.TestProperty(_Message, r => r.IsMlat, false);
            TestUtilities.TestProperty(_Message, r => r.NonIcao24Address, null, 0xff1234);
            TestUtilities.TestProperty(_Message, r => r.ParityInterrogatorIdentifier, null, 0xffffff);
            TestUtilities.TestProperty(_Message, r => r.PossibleCallsign, null, "ABC123");
            TestUtilities.TestProperty(_Message, r => r.ReplyInformation, null, (byte)15);
            TestUtilities.TestProperty(_Message, r => r.SensitivityLevel, null, (byte)4);
            TestUtilities.TestProperty(_Message, r => r.SignalLevel, null, 1230);
            TestUtilities.TestProperty(_Message, r => r.UtilityMessage, null, (byte)255);
            TestUtilities.TestProperty(_Message, r => r.VerticalStatus, null, VerticalStatus.OnGround);
        }

        [TestMethod]
        public void ModeSMessage_FormattedIcao24_Formats_Icao24_Property()
        {
            Assert.AreEqual("000000", _Message.FormattedIcao24);

            _Message.Icao24 = 0x0000ab;
            Assert.AreEqual("0000AB", _Message.FormattedIcao24);

            _Message.Icao24 = 0x010203;
            Assert.AreEqual("010203", _Message.FormattedIcao24);

            _Message.Icao24 = 0xffffff;
            Assert.AreEqual("FFFFFF", _Message.FormattedIcao24);

            // The formatting of ICAO24 values larger than 24 bits is undefined, we don't test it.
        }
    }
}
