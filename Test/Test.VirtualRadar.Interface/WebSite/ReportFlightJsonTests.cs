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
using VirtualRadar.Interface.WebSite;
using Test.Framework;

namespace Test.VirtualRadar.Interface.WebSite
{
    [TestClass]
    public class ReportFlightJsonTests
    {
        [TestMethod]
        public void ReportFlightJson_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            var json = new ReportFlightJson();

            TestUtilities.TestProperty(json, r => r.AircraftIndex, null, 1);
            TestUtilities.TestProperty(json, r => r.Callsign, null, "Ab");
            TestUtilities.TestProperty(json, r => r.EndTime, DateTime.MinValue, DateTime.Now);
            TestUtilities.TestProperty(json, r => r.FirstAltitude, 0, 1);
            TestUtilities.TestProperty(json, r => r.FirstGroundSpeed, 0, 1);
            TestUtilities.TestProperty(json, r => r.FirstIsOnGround, false);
            TestUtilities.TestProperty(json, r => r.FirstLatitude, 0.0, 1.0);
            TestUtilities.TestProperty(json, r => r.FirstLongitude, 0.0, 1.0);
            TestUtilities.TestProperty(json, r => r.FirstSquawk, 0, 1);
            TestUtilities.TestProperty(json, r => r.FirstTrack, 0f, 1f);
            TestUtilities.TestProperty(json, r => r.FirstVerticalRate, 0, 1);
            TestUtilities.TestProperty(json, r => r.HadAlert, false);
            TestUtilities.TestProperty(json, r => r.HadEmergency, false);
            TestUtilities.TestProperty(json, r => r.HadSpi, false);
            TestUtilities.TestProperty(json, r => r.LastAltitude, 0, 1);
            TestUtilities.TestProperty(json, r => r.LastGroundSpeed, 0, 1);
            TestUtilities.TestProperty(json, r => r.LastIsOnGround, false);
            TestUtilities.TestProperty(json, r => r.LastLatitude, 0.0, 1.0);
            TestUtilities.TestProperty(json, r => r.LastLongitude, 0.0, 1.0);
            TestUtilities.TestProperty(json, r => r.LastSquawk, 0, 1);
            TestUtilities.TestProperty(json, r => r.LastTrack, 0f, 1f);
            TestUtilities.TestProperty(json, r => r.LastVerticalRate, 0, 1);
            TestUtilities.TestProperty(json, r => r.NumADSBMsgRec, 0, 1);
            TestUtilities.TestProperty(json, r => r.NumModeSMsgRec, 0, 1);
            TestUtilities.TestProperty(json, r => r.NumPosMsgRec, 0, 1);
            TestUtilities.TestProperty(json, r => r.RouteIndex, 0, 1);
            TestUtilities.TestProperty(json, r => r.RowNumber, 0, 1);
            TestUtilities.TestProperty(json, r => r.StartTime, DateTime.MinValue, DateTime.Now);
        }
    }
}
