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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface.Settings;
using System.IO;
using Test.Framework;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface.Settings
{
    [TestClass]
    public class GoogleMapSettingsTests
    {
        private GoogleMapSettings _Implementation;

        [TestInitialize]
        public void TestInitialise()
        {
            _Implementation = new GoogleMapSettings();
        }

        [TestMethod]
        public void GoogleMapSettings_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            CheckProperties(_Implementation);
        }

        public static void CheckProperties(GoogleMapSettings settings, bool assumeInitialConfig = false)
        {
            TestUtilities.TestProperty(settings, r => r.InitialMapLatitude, 51.47, 90.1);
            TestUtilities.TestProperty(settings, r => r.InitialMapLongitude, -0.6, 29.10);
            TestUtilities.TestProperty(settings, r => r.InitialMapType, "ROADMAP", "TERRAIN");
            TestUtilities.TestProperty(settings, r => r.InitialMapZoom, 11, 2);
            TestUtilities.TestProperty(settings, r => r.InitialRefreshSeconds, 1, 12);
            TestUtilities.TestProperty(settings, r => r.InitialSettings, null, "Abc123");
            TestUtilities.TestProperty(settings, r => r.MinimumRefreshSeconds, 1, 22);
            TestUtilities.TestProperty(settings, r => r.ShortTrailLengthSeconds, 30, 600);
            TestUtilities.TestProperty(settings, r => r.InitialDistanceUnit, DistanceUnit.NauticalMiles, DistanceUnit.Kilometres);
            TestUtilities.TestProperty(settings, r => r.InitialHeightUnit, HeightUnit.Feet, HeightUnit.Metres);
            TestUtilities.TestProperty(settings, r => r.InitialSpeedUnit, SpeedUnit.Knots, SpeedUnit.MilesPerHour);
            TestUtilities.TestProperty(settings, r => r.PreferIataAirportCodes, false);
            TestUtilities.TestProperty(settings, r => r.EnableBundling, true);
            TestUtilities.TestProperty(settings, r => r.EnableMinifying, true);
            TestUtilities.TestProperty(settings, r => r.EnableCompression, true);
            TestUtilities.TestProperty(settings, r => r.WebSiteReceiverId, assumeInitialConfig ? 1 : 0, 123);
            TestUtilities.TestProperty(settings, r => r.ClosestAircraftReceiverId, assumeInitialConfig ? 1 : 0, 456);
            TestUtilities.TestProperty(settings, r => r.FlightSimulatorXReceiverId, assumeInitialConfig ? 1 : 0, 789);
            TestUtilities.TestProperty(settings, r => r.ProxyType, ProxyType.Unknown, ProxyType.Forward);
            TestUtilities.TestProperty(settings, r => r.DirectoryEntryKey, null, "ABC123");
            TestUtilities.TestProperty(settings, r => r.EnableCorsSupport, false);
            TestUtilities.TestProperty(settings, r => r.AllowCorsDomains, null, "a.b.com");
            TestUtilities.TestProperty(settings, r => r.GoogleMapsApiKey, null, "Key");
            TestUtilities.TestProperty(settings, r => r.UseGoogleMapsAPIKeyWithLocalRequests, false);
            TestUtilities.TestProperty(settings, r => r.UseSvgGraphicsOnDesktop, true);
            TestUtilities.TestProperty(settings, r => r.UseSvgGraphicsOnMobile, true);
            TestUtilities.TestProperty(settings, r => r.UseSvgGraphicsOnReports, true);
            TestUtilities.TestProperty(settings, r => r.MapProvider, MapProvider.GoogleMaps, MapProvider.OpenStreetMap);
            TestUtilities.TestProperty(settings, r => r.OpenStreetMapTileServerUrl, null, "A url");
        }
    }
}
