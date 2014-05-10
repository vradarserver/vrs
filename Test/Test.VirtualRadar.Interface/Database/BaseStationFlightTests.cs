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
using VirtualRadar.Interface.Database;
using Test.Framework;
using InterfaceFactory;

namespace Test.VirtualRadar.Interface.Database
{
    [TestClass]
    public class BaseStationFlightTests
    {
        [TestMethod]
        public void BaseStationFlight_Initialises_To_Known_State_And_Properties_Work()
        {
            var baseStationFlight = new BaseStationFlight();

            TestUtilities.TestProperty(baseStationFlight, r => r.Aircraft, null, new BaseStationAircraft());
            TestUtilities.TestProperty(baseStationFlight, r => r.AircraftID, 0, 1224);
            TestUtilities.TestProperty(baseStationFlight, r => r.Callsign, null, "Aa");
            TestUtilities.TestProperty(baseStationFlight, r => r.EndTime, null, DateTime.Today);
            TestUtilities.TestProperty(baseStationFlight, r => r.FirstAltitude, null, 1212);
            TestUtilities.TestProperty(baseStationFlight, r => r.FirstGroundSpeed, null, 123.132f);
            TestUtilities.TestProperty(baseStationFlight, r => r.FirstIsOnGround, false);
            TestUtilities.TestProperty(baseStationFlight, r => r.FirstLat, null, 1239.85);
            TestUtilities.TestProperty(baseStationFlight, r => r.FirstLon, null, 93.556);
            TestUtilities.TestProperty(baseStationFlight, r => r.FirstSquawk, null, 59);
            TestUtilities.TestProperty(baseStationFlight, r => r.FirstTrack, null, 95457.423f);
            TestUtilities.TestProperty(baseStationFlight, r => r.FirstVerticalRate, null, 123);
            TestUtilities.TestProperty(baseStationFlight, r => r.FlightID, 0, 1543);
            TestUtilities.TestProperty(baseStationFlight, r => r.HadAlert, false);
            TestUtilities.TestProperty(baseStationFlight, r => r.HadEmergency, false);
            TestUtilities.TestProperty(baseStationFlight, r => r.HadSpi, false);
            TestUtilities.TestProperty(baseStationFlight, r => r.LastAltitude, null, 1212);
            TestUtilities.TestProperty(baseStationFlight, r => r.LastGroundSpeed, null, 123.132f);
            TestUtilities.TestProperty(baseStationFlight, r => r.LastIsOnGround, false);
            TestUtilities.TestProperty(baseStationFlight, r => r.LastLat, null, 1239.85);
            TestUtilities.TestProperty(baseStationFlight, r => r.LastLon, null, 93.556);
            TestUtilities.TestProperty(baseStationFlight, r => r.LastSquawk, null, 59);
            TestUtilities.TestProperty(baseStationFlight, r => r.LastTrack, null, 95457.423f);
            TestUtilities.TestProperty(baseStationFlight, r => r.LastVerticalRate, null, 123);
            TestUtilities.TestProperty(baseStationFlight, r => r.NumADSBMsgRec, null, int.MaxValue);
            TestUtilities.TestProperty(baseStationFlight, r => r.NumModeSMsgRec, null, int.MaxValue);
            TestUtilities.TestProperty(baseStationFlight, r => r.NumIDMsgRec, null, int.MaxValue);
            TestUtilities.TestProperty(baseStationFlight, r => r.NumSurPosMsgRec, null, int.MaxValue);
            TestUtilities.TestProperty(baseStationFlight, r => r.NumAirPosMsgRec, null, int.MaxValue);
            TestUtilities.TestProperty(baseStationFlight, r => r.NumAirVelMsgRec, null, int.MaxValue);
            TestUtilities.TestProperty(baseStationFlight, r => r.NumSurAltMsgRec, null, int.MaxValue);
            TestUtilities.TestProperty(baseStationFlight, r => r.NumSurIDMsgRec, null, int.MaxValue);
            TestUtilities.TestProperty(baseStationFlight, r => r.NumAirToAirMsgRec, null, int.MaxValue);
            TestUtilities.TestProperty(baseStationFlight, r => r.NumAirCallRepMsgRec, null, int.MaxValue);
            TestUtilities.TestProperty(baseStationFlight, r => r.NumPosMsgRec, null, int.MaxValue);
            TestUtilities.TestProperty(baseStationFlight, r => r.SessionID, 0, int.MaxValue);
            TestUtilities.TestProperty(baseStationFlight, r => r.StartTime, DateTime.MinValue, DateTime.Now);
        }
    }
}
