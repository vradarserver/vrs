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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface.Listener;

namespace Test.VirtualRadar.Interface.Listener
{
    [TestClass]
    public class AirnavXRangeAircraftJsonTests
    {
        public TestContext TestContext { get; set; }
        private AirnavXRangeAircraftJson _Json;

        [TestInitialize]
        public void TestInitialise()
        {
            _Json = new AirnavXRangeAircraftJson();
        }

        [TestMethod]
        public void AirnavXRangeAircraftJson_Icao24_GetsOrSets_RawIcao()
        {
            Assert.IsNull(_Json.RawIcao24);
            Assert.IsNull(_Json.Icao24);

            _Json.RawIcao24 = "abc123";
            Assert.AreEqual("ABC123", _Json.Icao24);

            _Json.Icao24 = "FEEEED";
            Assert.AreEqual("feeeed", _Json.RawIcao24);

            _Json.Icao24 = null;
            Assert.IsNull(_Json.RawIcao24);
        }

        [TestMethod]
        public void AirnavXRangeAircraftJson_Squawk_GetsOrSets_RawSquawk()
        {
            Assert.IsNull(_Json.RawSquawk);
            Assert.IsNull(_Json.Squawk);

            _Json.RawSquawk = "1234";
            Assert.AreEqual(1234, _Json.Squawk);

            _Json.Squawk = 5432;
            Assert.AreEqual("5432", _Json.RawSquawk);

            _Json.Squawk = 1;
            Assert.AreEqual("0001", _Json.RawSquawk);

            _Json.Squawk = (int?)null;
            Assert.IsNull(_Json.RawSquawk);
        }

        [TestMethod]
        public void AirnavXRangeAircraftJson_Altitude_GetsOrSets_RawAltitude()
        {
            Assert.IsNull(_Json.RawAltitude);
            Assert.IsNull(_Json.Altitude);

            _Json.RawAltitude = "9400";
            Assert.AreEqual(9400, _Json.Altitude);

            _Json.RawAltitude = "ground";
            Assert.AreEqual(0, _Json.Altitude);

            _Json.Altitude = 12000;
            Assert.AreEqual("12000", _Json.RawAltitude);

            _Json.Altitude = 0;
            Assert.AreEqual("0", _Json.RawAltitude);

            _Json.Altitude = null;
            Assert.IsNull(_Json.RawAltitude);
        }

        [TestMethod]
        public void AirnavXRangeAircraftJson_OnGround_GetsOrSets_RawAltitude()
        {
            Assert.IsNull(_Json.RawAltitude);
            Assert.IsNull(_Json.OnGround);

            _Json.RawAltitude = "ground";
            Assert.IsTrue(_Json.OnGround.Value);

            _Json.RawAltitude = "12000";
            Assert.IsFalse(_Json.OnGround.Value);

            _Json.OnGround = true;
            Assert.AreEqual("ground", _Json.RawAltitude);

            _Json.RawAltitude = "ground";
            _Json.OnGround = false;
            Assert.IsNull(_Json.RawAltitude);

            _Json.RawAltitude = "12000";
            _Json.OnGround = false;
            Assert.AreEqual("12000", _Json.RawAltitude);

            _Json.RawAltitude = "12000";
            _Json.OnGround = null;
            Assert.AreEqual("12000", _Json.RawAltitude);
        }

        [TestMethod]
        public void AirnavXRangeAircraftJson_Callsign_GetsOrSets_RawCallsign()
        {
            Assert.IsNull(_Json.RawCallsign);
            Assert.IsNull(_Json.Callsign);

            _Json.RawCallsign = "VIR1    ";
            Assert.AreEqual("VIR1", _Json.Callsign);

            _Json.Callsign = "ANZ619";
            Assert.AreEqual("ANZ619  ", _Json.RawCallsign);

            _Json.Callsign = null;
            Assert.IsNull(_Json.RawCallsign);

            _Json.Callsign = "OVER-LENGTH";
            Assert.AreEqual("OVER-LENGTH", _Json.RawCallsign);
        }
    }
}
