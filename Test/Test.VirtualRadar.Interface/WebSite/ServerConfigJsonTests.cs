// Copyright © 2013 onwards, Andrew Whewell
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
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface.WebSite
{
    [TestClass]
    public class ServerConfigJsonTests
    {
        [TestMethod]
        public void ServerConfigJson_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            var json = new ServerConfigJson();

            Assert.AreEqual(0, json.Receivers.Count);

            TestUtilities.TestProperty(json, r => r.InitialDistanceUnit, null, "Abc");
            TestUtilities.TestProperty(json, r => r.InitialHeightUnit, null, "Abc");
            TestUtilities.TestProperty(json, r => r.InitialLatitude, 0.00, 1.234);
            TestUtilities.TestProperty(json, r => r.InitialLongitude, 0.00, 1.234);
            TestUtilities.TestProperty(json, r => r.InitialMapType, null, "Abc");
            TestUtilities.TestProperty(json, r => r.InitialSettings, null, "Abc");
            TestUtilities.TestProperty(json, r => r.InitialSpeedUnit, null, "Abc");
            TestUtilities.TestProperty(json, r => r.InitialZoom, 0, 123);
            TestUtilities.TestProperty(json, r => r.InternetClientCanRunReports, false);
            TestUtilities.TestProperty(json, r => r.InternetClientCanShowPinText, false);
            TestUtilities.TestProperty(json, r => r.InternetClientsCanPlayAudio, false);
            TestUtilities.TestProperty(json, r => r.InternetClientsCanSeeAircraftPictures, false);
            TestUtilities.TestProperty(json, r => r.InternetClientsCanSeePolarPlots, false);
            TestUtilities.TestProperty(json, r => r.InternetClientsCanSubmitRoutes, false);
            TestUtilities.TestProperty(json, r => r.InternetClientTimeoutMinutes, 0, 123);
            TestUtilities.TestProperty(json, r => r.IsAudioEnabled, false);
            TestUtilities.TestProperty(json, r => r.IsLocalAddress, false);
            TestUtilities.TestProperty(json, r => r.IsMono, false);
            TestUtilities.TestProperty(json, r => r.MinimumRefreshSeconds, 0, 123);
            TestUtilities.TestProperty(json, r => r.RefreshSeconds, 0, 123);
            TestUtilities.TestProperty(json, r => r.UseMarkerLabels, false);
            TestUtilities.TestProperty(json, r => r.VrsVersion, null, "Abc");
        }

        [TestMethod]
        public void ServerConfigJson_Clone_Creates_Copy()
        {
            foreach(var property in typeof(ServerConfigJson).GetProperties()) {
                for(var pass = 0;pass < 2;++pass) {
                    var json = new ServerConfigJson();

                    object expected = null;
                    switch(property.Name) {
                        case "InitialDistanceUnit":                     expected = json.InitialDistanceUnit = pass == 0 ? "A": "B"; break;
                        case "InitialHeightUnit":                       expected = json.InitialHeightUnit = pass == 0 ? "A" : "B"; break;
                        case "InitialLatitude":                         expected = json.InitialLatitude = pass == 0 ? 1.234 : 5.678; break;
                        case "InitialLongitude":                        expected = json.InitialLongitude = pass == 0 ? 1.234 : 5.678; break;
                        case "InitialMapType":                          expected = json.InitialMapType = pass == 0 ? "A" : "B"; break;
                        case "InitialSettings":                         expected = json.InitialSettings = pass == 0 ? "A" : "B"; break;
                        case "InitialSpeedUnit":                        expected = json.InitialSpeedUnit = pass == 0 ? "A" : "B"; break;
                        case "InitialZoom":                             expected = json.InitialZoom = pass == 0 ? 1 : 2; break;
                        case "InternetClientCanRunReports":             expected = json.InternetClientCanRunReports = pass == 0; break;
                        case "InternetClientCanShowPinText":            expected = json.InternetClientCanShowPinText = pass == 0; break;
                        case "InternetClientsCanPlayAudio":             expected = json.InternetClientsCanPlayAudio = pass == 0; break;
                        case "InternetClientsCanSeeAircraftPictures":   expected = json.InternetClientsCanSeeAircraftPictures = pass == 0; break;
                        case "InternetClientsCanSeePolarPlots":         expected = json.InternetClientsCanSeePolarPlots = pass == 0; break;
                        case "InternetClientsCanSubmitRoutes":          expected = json.InternetClientsCanSubmitRoutes = pass == 0; break;
                        case "InternetClientTimeoutMinutes":            expected = json.InternetClientTimeoutMinutes = pass == 0 ? 1 : 2; break;
                        case "IsAudioEnabled":                          expected = json.IsAudioEnabled = pass == 0; break;
                        case "IsLocalAddress":                          expected = json.IsLocalAddress = pass == 0; break;
                        case "IsMono":                                  expected = json.IsMono = pass == 0; break;
                        case "MinimumRefreshSeconds":                   expected = json.MinimumRefreshSeconds = pass == 0 ? 1 : 2; break;
                        case "RefreshSeconds":                          expected = json.RefreshSeconds = pass == 0 ? 1 : 2; break;
                        case "UseMarkerLabels":                         expected = json.UseMarkerLabels = pass == 0; break;
                        case "VrsVersion":                              expected = json.VrsVersion = pass == 0 ? "A" : "B"; break;
                        case "Receivers":
                            json.Receivers.Add(new ServerReceiverJson() {
                                UniqueId = pass == 0 ? 1 : 2,
                                Name = pass == 0 ? "First" : "Second",
                            });
                            break;
                        default:                                throw new NotImplementedException(property.Name);
                    }

                    var actual = (ServerConfigJson)json.Clone();

                    if(property.Name != "Receivers") {
                        var actualValue = property.GetValue(actual, null);
                        Assert.AreEqual(expected, actualValue, "for property {0}", property.Name);
                    } else {
                        Assert.AreEqual(json.Receivers.Count, actual.Receivers.Count);
                        Assert.AreNotSame(json.Receivers[0], actual.Receivers[0]);
                        Assert.AreEqual(json.Receivers[0].UniqueId, actual.Receivers[0].UniqueId);
                        Assert.AreEqual(json.Receivers[0].Name, actual.Receivers[0].Name);
                    }
                }
            }
        }
    }
}
