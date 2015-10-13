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
    public class AircraftJsonTests
    {
        [TestMethod]
        public void AircraftJson_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            var aircraftJson = new AircraftJson();

            TestUtilities.TestProperty(aircraftJson, r => r.Altitude, null, 12);
            TestUtilities.TestProperty(aircraftJson, r => r.AltitudeType, null, 1);
            TestUtilities.TestProperty(aircraftJson, r => r.BearingFromHere, null, 12.3);
            TestUtilities.TestProperty(aircraftJson, r => r.Callsign, null, "ND");
            TestUtilities.TestProperty(aircraftJson, r => r.CallsignIsSuspect, null, true);
            TestUtilities.TestProperty(aircraftJson, r => r.ConstructionNumber, null, "Uu");
            TestUtilities.TestProperty(aircraftJson, r => r.Destination, null, "Bb");
            TestUtilities.TestProperty(aircraftJson, r => r.DistanceFromHere, null, 12.45);
            TestUtilities.TestProperty(aircraftJson, r => r.Emergency, null, false);
            TestUtilities.TestProperty(aircraftJson, r => r.EngineType, null, 1);
            TestUtilities.TestProperty(aircraftJson, r => r.EnginePlacement, null, 1);
            TestUtilities.TestProperty(aircraftJson, r => r.FirstSeen, null, DateTime.UtcNow);
            TestUtilities.TestProperty(aircraftJson, r => r.FullCoordinates, null, new List<double?>() { 1.1, 2.2 });
            TestUtilities.TestProperty(aircraftJson, r => r.GroundSpeed, null, 12.4f);
            TestUtilities.TestProperty(aircraftJson, r => r.HasPicture, null, true);
            TestUtilities.TestProperty(aircraftJson, r => r.HasSignalLevel, null, true);
            TestUtilities.TestProperty(aircraftJson, r => r.Icao24, null, "Hh");
            TestUtilities.TestProperty(aircraftJson, r => r.Icao24Country, null, "Jk");
            TestUtilities.TestProperty(aircraftJson, r => r.Icao24Invalid, null, true);
            TestUtilities.TestProperty(aircraftJson, r => r.IsMilitary, null, false);
            TestUtilities.TestProperty(aircraftJson, r => r.IsTisb, null, false);
            TestUtilities.TestProperty(aircraftJson, r => r.Latitude, null, 1.234);
            TestUtilities.TestProperty(aircraftJson, r => r.Longitude, null, 1.234);
            TestUtilities.TestProperty(aircraftJson, r => r.Manufacturer, null, "Aa");
            TestUtilities.TestProperty(aircraftJson, r => r.Model, null, "Mm");
            TestUtilities.TestProperty(aircraftJson, r => r.NumberOfEngines, null, "C");
            TestUtilities.TestProperty(aircraftJson, r => r.OnGround, null, true);
            TestUtilities.TestProperty(aircraftJson, r => r.Operator, null, "Hj");
            TestUtilities.TestProperty(aircraftJson, r => r.OperatorIcao, null, "Ik");
            TestUtilities.TestProperty(aircraftJson, r => r.Origin, null, "Yh");
            TestUtilities.TestProperty(aircraftJson, r => r.PictureHeight, null, 1203);
            TestUtilities.TestProperty(aircraftJson, r => r.PictureWidth, null, 2048);
            TestUtilities.TestProperty(aircraftJson, r => r.PositionIsStale, null, true);
            TestUtilities.TestProperty(aircraftJson, r => r.PositionTime, null, 1234L);
            TestUtilities.TestProperty(aircraftJson, r => r.ReceiverId, null, 9931);
            TestUtilities.TestProperty(aircraftJson, r => r.Registration, null, "Fd");
            TestUtilities.TestProperty(aircraftJson, r => r.ResetTrail, false);
            TestUtilities.TestProperty(aircraftJson, r => r.ShortCoordinates, null, new List<double?>() { 1.1, 2.2 });
            TestUtilities.TestProperty(aircraftJson, r => r.SignalLevel, null, 123);
            TestUtilities.TestProperty(aircraftJson, r => r.Species, null, 12);
            TestUtilities.TestProperty(aircraftJson, r => r.Squawk, null, "4721");
            TestUtilities.TestProperty(aircraftJson, r => r.Stopovers, null, new List<string>() { "Hd" });
            TestUtilities.TestProperty(aircraftJson, r => r.TargetAltitude, null, 1);
            TestUtilities.TestProperty(aircraftJson, r => r.TargetTrack, null, 12.34F);
            TestUtilities.TestProperty(aircraftJson, r => r.Track, null, 12.34f);
            TestUtilities.TestProperty(aircraftJson, r => r.TrackIsHeading, null, true);
            TestUtilities.TestProperty(aircraftJson, r => r.TrailType, null, "a");
            TestUtilities.TestProperty(aircraftJson, r => r.TransponderType, null, 0);
            TestUtilities.TestProperty(aircraftJson, r => r.Type, null, "B747");
            TestUtilities.TestProperty(aircraftJson, r => r.UniqueId, 0, 12);
            TestUtilities.TestProperty(aircraftJson, r => r.UserTag, null, "Abc");
            TestUtilities.TestProperty(aircraftJson, r => r.VerticalRate, null, -239);
            TestUtilities.TestProperty(aircraftJson, r => r.VerticalRateType, null, 1);
            TestUtilities.TestProperty(aircraftJson, r => r.WakeTurbulenceCategory, null, 7);
            TestUtilities.TestProperty(aircraftJson, r => r.YearBuilt, null, "1999");
        }
    }
}
