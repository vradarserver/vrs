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
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;

namespace Test.VirtualRadar.Interface.Database
{
    [TestClass]
    public class TrackHistoryStateTests
    {
        [TestMethod]
        public void TrackHistoryState_Merge_States_Merges_States_Correctly()
        {
            foreach(var startWithDefaultValue in new bool[] { true, false }) {
                foreach(var supplyStateToMergeInto in new bool[] { false, true }) {
                    var initialState = new TrackHistoryState();
                    var nextState = new TrackHistoryState();
                    var defaultState = new TrackHistoryState();

                    var expected = new TrackHistoryState();

                    var states = new List<TrackHistoryState>();
                    if(!supplyStateToMergeInto) {
                        states.Add(initialState);
                    }
                    states.Add(nextState);
                    states.Add(defaultState);

                    object initialValue = null;
                    object nextValue = null;
                    foreach(var property in typeof(TrackHistoryState).GetProperties()) {
                        switch(property.Name) {
                            case nameof(TrackHistoryState.Callsign):
                                initialValue = "BAW1";
                                nextValue = "VIR1";
                                break;
                            case nameof(TrackHistoryState.SpeedTypeID):
                                initialValue = SpeedType.GroundSpeed;
                                nextValue = SpeedType.TrueAirSpeed;
                                break;
                            case nameof(TrackHistoryState.Latitude):
                            case nameof(TrackHistoryState.Longitude):
                                initialValue = 1.2;
                                nextValue = 3.4;
                                break;
                            case nameof(TrackHistoryState.AirPressureInHg):
                            case nameof(TrackHistoryState.GroundSpeedKnots):
                            case nameof(TrackHistoryState.TargetTrack):
                            case nameof(TrackHistoryState.TrackDegrees):
                                initialValue = 1.2F;
                                nextValue = 3.4F;
                                break;
                            case nameof(TrackHistoryState.AltitudeFeet):
                            case nameof(TrackHistoryState.ReceiverID):
                            case nameof(TrackHistoryState.SequenceNumber):
                            case nameof(TrackHistoryState.SignalLevel):
                            case nameof(TrackHistoryState.SquawkOctal):
                            case nameof(TrackHistoryState.TargetAltitudeFeet):
                            case nameof(TrackHistoryState.VerticalRateFeetMin):
                                initialValue = 1;
                                nextValue = 2;
                                break;
                            case nameof(TrackHistoryState.IdentActive):
                            case nameof(TrackHistoryState.IsCallsignSuspect):
                            case nameof(TrackHistoryState.IsMlat):
                            case nameof(TrackHistoryState.IsTisb):
                            case nameof(TrackHistoryState.TrackIsHeading):
                                initialValue = true;
                                nextValue = false;
                                break;
                            case nameof(TrackHistoryState.AltitudeTypeID):
                            case nameof(TrackHistoryState.VerticalRateTypeID):
                                initialValue = AltitudeType.Barometric;
                                nextValue = AltitudeType.Geometric;
                                break;

                            case nameof(TrackHistoryState.TimestampUtc):
                                initialValue = DateTime.UtcNow;
                                nextValue = DateTime.UtcNow.AddSeconds(1);
                                break;

                            case nameof(TrackHistoryState.TrackHistoryID):
                            case nameof(TrackHistoryState.TrackHistoryStateID):
                                break;

                            default:
                                throw new NotImplementedException($"Need code for {nameof(TrackHistoryState)}.{property.Name}");
                        }

                        if(nextValue != null) {
                            if(!startWithDefaultValue) {
                                property.SetValue(initialState, initialValue);
                            }
                            property.SetValue(nextState, nextValue);
                            property.SetValue(expected, nextValue);

                            var result = TrackHistoryState.MergeStates(states, intoState: supplyStateToMergeInto ? initialState : null);
                            if(supplyStateToMergeInto) {
                                Assert.AreSame(initialState, result);
                            }

                            TestUtilities.TestObjectPropertiesAreEqual(expected, result);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TrackHistoryState_MergeStates_Always_Uses_Last_TrackHistoryID()
        {
            var merged = TrackHistoryState.MergeStates(new TrackHistoryState[] {
                new TrackHistoryState() { TrackHistoryID = 1 },
                new TrackHistoryState() { TrackHistoryID = 2 },
            });

            Assert.AreEqual(2, merged.TrackHistoryID);
        }

        [TestMethod]
        public void TrackHistoryState_MergeStates_Never_Overwrites_TrackHistoryStateID()
        {
            var merged = TrackHistoryState.MergeStates(new TrackHistoryState[] {
                new TrackHistoryState() { TrackHistoryStateID = 1 },
                new TrackHistoryState() { TrackHistoryStateID = 2 },
            });

            Assert.AreEqual(0, merged.TrackHistoryID);
        }
    }
}
