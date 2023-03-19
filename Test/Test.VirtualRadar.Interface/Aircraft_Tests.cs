//// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Interface;
using VirtualRadar.Interface.StandingData;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class Aircraft_Tests
    {
        [TestMethod]
        public void Clone_Copies_Every_Property_Correctly()
        {
            var values = new Dictionary<string,object>();
            var versions = new Dictionary<string,int>();
            var aircraft = new Aircraft();

            var intValue = 1;
            var doubleValue = 121.3414;
            var longValue = (long)int.MaxValue + 1L;
            var dateTimeValue = new DateTime(2001, 1, 2);

            foreach(var property in typeof(Aircraft).GetProperties()) {
                ++intValue;
                ++longValue;
                ++doubleValue;
                dateTimeValue = dateTimeValue.AddDays(1);
                var ver = ++aircraft.DataVersion;
                var stringValue = $"A{intValue}";
                var guidValue = Guid.NewGuid();
                var airport = new Airport() { IcaoCode = stringValue, IataCode = "" };

                if(values.ContainsKey(nameof(Aircraft.DataVersion))) {
                    values[nameof(Aircraft.DataVersion)] = ver;
                }

                object value = null;
                switch(property.Name) {
                    case nameof(Aircraft.DataVersion):                      value = ver; break;
                    case nameof(Aircraft.UniqueId):                         value = aircraft.UniqueId = intValue; break;
                    case nameof(Aircraft.Icao24):                           value = aircraft.Icao24.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.Icao24Invalid):                    value = aircraft.Icao24Invalid.SetValue(true, ver); break;
                    case nameof(Aircraft.Callsign):                         value = aircraft.Callsign.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.CallsignIsSuspect):                value = aircraft.CallsignIsSuspect.SetValue(true, ver); break;
                    case nameof(Aircraft.CountMessagesReceived):            value = aircraft.CountMessagesReceived.SetValue(longValue, ver); break;
                    case nameof(Aircraft.Altitude):                         value = aircraft.Altitude.SetValue(intValue, ver); break;
                    case nameof(Aircraft.AltitudeType):                     value = aircraft.AltitudeType.SetValue(AltitudeType.Geometric, ver); break;
                    case nameof(Aircraft.GroundSpeed):                      value = aircraft.GroundSpeed.SetValue(intValue, ver); break;
                    case nameof(Aircraft.Latitude):                         value = aircraft.Latitude.SetValue(doubleValue, ver); break;
                    case nameof(Aircraft.Longitude):                        value = aircraft.Longitude.SetValue(doubleValue, ver); break;
                    case nameof(Aircraft.PositionIsMlat):                   value = aircraft.PositionIsMlat.SetValue(true, ver); break;
                    case nameof(Aircraft.PositionReceiverId):               value = aircraft.PositionReceiverId.SetValue(guidValue, ver); break;
                    case nameof(Aircraft.PositionTime):                     value = aircraft.PositionTime.SetValue(dateTimeValue, ver); break;
                    case nameof(Aircraft.Track):                            value = aircraft.Track.SetValue(doubleValue, ver); break;
                    case nameof(Aircraft.IsTransmittingTrack):              value = aircraft.IsTransmittingTrack = true; break;
                    case nameof(Aircraft.VerticalRate):                     value = aircraft.VerticalRate.SetValue(intValue, ver); break;
                    case nameof(Aircraft.Squawk):                           value = aircraft.Squawk.SetValue(intValue, ver); break;
                    case nameof(Aircraft.Emergency):                        value = aircraft.Emergency.SetValue(true, ver); break;
                    case nameof(Aircraft.Registration):                     value = aircraft.Registration.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.FirstSeen):                        value = aircraft.FirstSeen.SetValue(dateTimeValue, ver); break;
                    case nameof(Aircraft.FlightsCount):                     value = aircraft.FlightsCount.SetValue(intValue, ver); break;
                    case nameof(Aircraft.LastUpdate):                       value = aircraft.LastUpdate = dateTimeValue; break;
                    case nameof(Aircraft.LastModeSUpdate):                  value = aircraft.LastModeSUpdate = dateTimeValue; break;
                    case nameof(Aircraft.LastSatcomUpdate):                 value = aircraft.LastSatcomUpdate = dateTimeValue; break;
                    case nameof(Aircraft.Type):                             value = aircraft.Type.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.Manufacturer):                     value = aircraft.Manufacturer.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.Model):                            value = aircraft.Model.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.ConstructionNumber):               value = aircraft.ConstructionNumber.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.Origin):                           value = aircraft.Origin.SetValue(airport, ver); break;
                    case nameof(Aircraft.Destination):                      value = aircraft.Destination.SetValue(airport, ver); break;
                    case nameof(Aircraft.SignalLevel):                      value = aircraft.SignalLevel.SetValue(intValue, ver); break;
                    case nameof(Aircraft.Stopovers):                        value = new Airport[] { airport }; aircraft.Stopovers.SetValue((Airport[])value, ver); break;
                    case nameof(Aircraft.Operator):                         value = aircraft.Operator.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.OperatorIcao):                     value = aircraft.OperatorIcao.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.WakeTurbulenceCategory):           value = aircraft.WakeTurbulenceCategory.SetValue(WakeTurbulenceCategory.Light, ver); break;
                    case nameof(Aircraft.EnginePlacement):                  value = aircraft.EnginePlacement.SetValue(EnginePlacement.AftMounted, ver); break;
                    case nameof(Aircraft.EngineType):                       value = aircraft.EngineType.SetValue(EngineType.Electric, ver); break;
                    case nameof(Aircraft.NumberOfEngines):                  value = aircraft.NumberOfEngines.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.Species):                          value = aircraft.Species.SetValue(Species.Gyrocopter, ver); break;
                    case nameof(Aircraft.IsMilitary):                       value = aircraft.IsMilitary.SetValue(true, ver); break;
                    case nameof(Aircraft.Icao24Country):                    value = aircraft.Icao24Country.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.PictureFileName):                  value = aircraft.PictureFileName.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.FirstCoordinateChanged):           value = aircraft.FirstCoordinateChanged = longValue; break;
                    case nameof(Aircraft.LastCoordinateChanged):            value = aircraft.LastCoordinateChanged = longValue; break;
                    case nameof(Aircraft.LatestCoordinateTime):             value = aircraft.LatestCoordinateTime = dateTimeValue; break;
                    case nameof(Aircraft.FullCoordinates):                  aircraft.FullCoordinates.Add(new Coordinate(1, 2, 3F, 4F, 5F)); value = aircraft.FullCoordinates[0]; break;
                    case nameof(Aircraft.ShortCoordinates):                 aircraft.ShortCoordinates.Add(new Coordinate(11, 12, 13F, 14F, 15F)); value = aircraft.ShortCoordinates[0]; break;
                    case nameof(Aircraft.IcaoCompliantRegistration):        break;
                    case nameof(Aircraft.IsInteresting):                    value = aircraft.IsInteresting.SetValue(true, ver); break;
                    case nameof(Aircraft.OnGround):                         value = aircraft.OnGround.SetValue(true, ver); break;
                    case nameof(Aircraft.SpeedType):                        value = aircraft.SpeedType.SetValue(SpeedType.TrueAirSpeed, ver); break;
                    case nameof(Aircraft.UserTag):                          value = aircraft.UserTag.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.UserNotes):                        value = aircraft.UserNotes.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.ReceiverId):                       value = aircraft.ReceiverId.SetValue(guidValue, ver); break;
                    case nameof(Aircraft.PictureWidth):                     value = aircraft.PictureWidth.SetValue(intValue, ver); break;
                    case nameof(Aircraft.PictureHeight):                    value = aircraft.PictureHeight.SetValue(intValue, ver); break;
                    case nameof(Aircraft.VerticalRateType):                 value = aircraft.VerticalRateType.SetValue(AltitudeType.Geometric, ver); break;
                    case nameof(Aircraft.TrackIsHeading):                   value = aircraft.TrackIsHeading.SetValue(true, ver); break;
                    case nameof(Aircraft.TransponderType):                  value = aircraft.TransponderType.SetValue(TransponderType.Adsb2, ver); break;
                    case nameof(Aircraft.TargetAltitude):                   value = aircraft.TargetAltitude.SetValue(intValue, ver); break;
                    case nameof(Aircraft.TargetTrack):                      value = aircraft.TargetTrack.SetValue(doubleValue, ver); break;
                    case nameof(Aircraft.IsTisb):                           value = aircraft.IsTisb.SetValue(true, ver); break;
                    case nameof(Aircraft.YearBuilt):                        value = aircraft.YearBuilt.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.GeometricAltitude):                value = aircraft.GeometricAltitude.SetValue(intValue, ver); break;
                    case nameof(Aircraft.AirPressureInHg):                  value = aircraft.AirPressureInHg.SetValue(doubleValue, ver); break;
                    case nameof(Aircraft.AirPressureLookedUpUtc):           value = aircraft.AirPressureLookedUpUtc = dateTimeValue; break;
                    case nameof(Aircraft.IdentActive):                      value = aircraft.IdentActive.SetValue(true, ver); break;
                    case nameof(Aircraft.IsCharterFlight):                  value = aircraft.IsCharterFlight.SetValue(true, ver); break;
                    case nameof(Aircraft.IsPositioningFlight):              value = aircraft.IsPositioningFlight.SetValue(true, ver); break;
                    default:                                                throw new NotImplementedException();
                }

                if(value != null) {
                    values.Add(property.Name, value);
                }
            }

            var clone = aircraft.Clone();

            foreach(var property in typeof(Aircraft).GetProperties()) {
                if(values.TryGetValue(property.Name, out var expected)) {
                    switch(property.Name) {
                        case nameof(Aircraft.Stopovers):               Assert.IsTrue(((Airport[])expected).SequenceEqual(clone.Stopovers.Value), property.Name); break;
                        case nameof(Aircraft.FullCoordinates):         Assert.AreEqual(expected, clone.FullCoordinates[0], property.Name); break;
                        case nameof(Aircraft.ShortCoordinates):        Assert.AreEqual(expected, clone.ShortCoordinates[0], property.Name); break;
                        default:
                            var actual = property.GetValue(clone, null);

                            if(property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(VersionedValue<>)) {
                                var valuePropertyInfo = actual.GetType().GetProperty(nameof(VersionedValue<int>.Value));
                                actual = valuePropertyInfo.GetValue(actual, null);
                            }

                            Assert.AreEqual(expected, actual, property.Name); break;
                    }
                }
            }
        }
    }
}
