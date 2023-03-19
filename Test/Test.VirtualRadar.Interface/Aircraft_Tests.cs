// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.StandingData;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class Aircraft_Tests
    {
        private Aircraft _Aircraft;

        [TestInitialize]
        public void TestInitialise()
        {
            _Aircraft = new();
        }

        [TestMethod]
        public void ResetCoordinates_Resets_Coordinate_Properties()
        {
            _Aircraft.LatestCoordinateTime = DateTime.Now;
            _Aircraft.FullCoordinates.Add(new Coordinate(1, 2, 3f, 4f, null));
            _Aircraft.ShortCoordinates.Add(new Coordinate(1, 2, 3f, 4f, null));
            _Aircraft.FirstCoordinateChanged = 88;
            _Aircraft.LastCoordinateChanged = 99;

            _Aircraft.ResetCoordinates();

            Assert.AreEqual(DateTime.MinValue, _Aircraft.LatestCoordinateTime);
            Assert.AreEqual(0, _Aircraft.FullCoordinates.Count);
            Assert.AreEqual(0, _Aircraft.ShortCoordinates.Count);
            Assert.AreEqual(0, _Aircraft.FirstCoordinateChanged);
            Assert.AreEqual(0, _Aircraft.LastCoordinateChanged);
        }

        [TestMethod]
        public void Clone_Copies_Every_Property_Correctly()
        {
            var values = new Dictionary<string,object>();
            var versions = new Dictionary<string,long>();

            var intValue = 1;
            var doubleValue = 121.3414;
            var longValue = (long)int.MaxValue + 1L;
            var dateTimeValue = new DateTime(2001, 1, 2);

            foreach(var property in typeof(Aircraft).GetProperties()) {
                ++intValue;
                ++longValue;
                ++doubleValue;
                dateTimeValue = dateTimeValue.AddDays(1);
                var ver = ++_Aircraft.DataVersion;
                var stringValue = $"A{intValue}";
                var guidValue = Guid.NewGuid();
                var airport = new Airport() { IcaoCode = stringValue, IataCode = "" };

                if(values.ContainsKey(nameof(Aircraft.DataVersion))) {
                    values[nameof(Aircraft.DataVersion)] = ver;
                }

                object value = null;
                switch(property.Name) {
                    case nameof(Aircraft.DataVersion):                      value = ver; break;
                    case nameof(Aircraft.UniqueId):                         value = _Aircraft.UniqueId = intValue; break;
                    case nameof(Aircraft.Icao24):                           value = _Aircraft.Icao24.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.Icao24Invalid):                    value = _Aircraft.Icao24Invalid.SetValue(true, ver); break;
                    case nameof(Aircraft.Callsign):                         value = _Aircraft.Callsign.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.CallsignIsSuspect):                value = _Aircraft.CallsignIsSuspect.SetValue(true, ver); break;
                    case nameof(Aircraft.CountMessagesReceived):            value = _Aircraft.CountMessagesReceived.SetValue(longValue, ver); break;
                    case nameof(Aircraft.Altitude):                         value = _Aircraft.Altitude.SetValue(intValue, ver); break;
                    case nameof(Aircraft.AltitudeType):                     value = _Aircraft.AltitudeType.SetValue(AltitudeType.Geometric, ver); break;
                    case nameof(Aircraft.GroundSpeed):                      value = _Aircraft.GroundSpeed.SetValue(intValue, ver); break;
                    case nameof(Aircraft.Latitude):                         value = _Aircraft.Latitude.SetValue(doubleValue, ver); break;
                    case nameof(Aircraft.Longitude):                        value = _Aircraft.Longitude.SetValue(doubleValue, ver); break;
                    case nameof(Aircraft.PositionIsMlat):                   value = _Aircraft.PositionIsMlat.SetValue(true, ver); break;
                    case nameof(Aircraft.PositionReceiverId):               value = _Aircraft.PositionReceiverId.SetValue(guidValue, ver); break;
                    case nameof(Aircraft.PositionTime):                     value = _Aircraft.PositionTime.SetValue(dateTimeValue, ver); break;
                    case nameof(Aircraft.Track):                            value = _Aircraft.Track.SetValue(doubleValue, ver); break;
                    case nameof(Aircraft.IsTransmittingTrack):              value = _Aircraft.IsTransmittingTrack = true; break;
                    case nameof(Aircraft.VerticalRate):                     value = _Aircraft.VerticalRate.SetValue(intValue, ver); break;
                    case nameof(Aircraft.Squawk):                           value = _Aircraft.Squawk.SetValue(intValue, ver); break;
                    case nameof(Aircraft.Emergency):                        value = _Aircraft.Emergency.SetValue(true, ver); break;
                    case nameof(Aircraft.Registration):                     value = _Aircraft.Registration.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.FirstSeen):                        value = _Aircraft.FirstSeen.SetValue(dateTimeValue, ver); break;
                    case nameof(Aircraft.FlightsCount):                     value = _Aircraft.FlightsCount.SetValue(intValue, ver); break;
                    case nameof(Aircraft.LastUpdate):                       value = _Aircraft.LastUpdate = dateTimeValue; break;
                    case nameof(Aircraft.LastModeSUpdate):                  value = _Aircraft.LastModeSUpdate = dateTimeValue; break;
                    case nameof(Aircraft.LastSatcomUpdate):                 value = _Aircraft.LastSatcomUpdate = dateTimeValue; break;
                    case nameof(Aircraft.Type):                             value = _Aircraft.Type.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.Manufacturer):                     value = _Aircraft.Manufacturer.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.Model):                            value = _Aircraft.Model.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.ConstructionNumber):               value = _Aircraft.ConstructionNumber.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.Origin):                           value = _Aircraft.Origin.SetValue(airport, ver); break;
                    case nameof(Aircraft.Destination):                      value = _Aircraft.Destination.SetValue(airport, ver); break;
                    case nameof(Aircraft.SignalLevel):                      value = _Aircraft.SignalLevel.SetValue(intValue, ver); break;
                    case nameof(Aircraft.Stopovers):                        value = new Airport[] { airport }; _Aircraft.Stopovers.SetValue((Airport[])value, ver); break;
                    case nameof(Aircraft.Operator):                         value = _Aircraft.Operator.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.OperatorIcao):                     value = _Aircraft.OperatorIcao.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.WakeTurbulenceCategory):           value = _Aircraft.WakeTurbulenceCategory.SetValue(WakeTurbulenceCategory.Light, ver); break;
                    case nameof(Aircraft.EnginePlacement):                  value = _Aircraft.EnginePlacement.SetValue(EnginePlacement.AftMounted, ver); break;
                    case nameof(Aircraft.EngineType):                       value = _Aircraft.EngineType.SetValue(EngineType.Electric, ver); break;
                    case nameof(Aircraft.NumberOfEngines):                  value = _Aircraft.NumberOfEngines.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.Species):                          value = _Aircraft.Species.SetValue(Species.Gyrocopter, ver); break;
                    case nameof(Aircraft.IsMilitary):                       value = _Aircraft.IsMilitary.SetValue(true, ver); break;
                    case nameof(Aircraft.Icao24Country):                    value = _Aircraft.Icao24Country.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.PictureFileName):                  value = _Aircraft.PictureFileName.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.FirstCoordinateChanged):           value = _Aircraft.FirstCoordinateChanged = longValue; break;
                    case nameof(Aircraft.LastCoordinateChanged):            value = _Aircraft.LastCoordinateChanged = longValue; break;
                    case nameof(Aircraft.LatestCoordinateTime):             value = _Aircraft.LatestCoordinateTime = dateTimeValue; break;
                    case nameof(Aircraft.FullCoordinates):                  _Aircraft.FullCoordinates.Add(new Coordinate(1, 2, 3F, 4F, 5F)); value = _Aircraft.FullCoordinates[0]; break;
                    case nameof(Aircraft.ShortCoordinates):                 _Aircraft.ShortCoordinates.Add(new Coordinate(11, 12, 13F, 14F, 15F)); value = _Aircraft.ShortCoordinates[0]; break;
                    case nameof(Aircraft.IcaoCompliantRegistration):        break;
                    case nameof(Aircraft.IsInteresting):                    value = _Aircraft.IsInteresting.SetValue(true, ver); break;
                    case nameof(Aircraft.OnGround):                         value = _Aircraft.OnGround.SetValue(true, ver); break;
                    case nameof(Aircraft.SpeedType):                        value = _Aircraft.SpeedType.SetValue(SpeedType.TrueAirSpeed, ver); break;
                    case nameof(Aircraft.UserTag):                          value = _Aircraft.UserTag.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.UserNotes):                        value = _Aircraft.UserNotes.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.ReceiverId):                       value = _Aircraft.ReceiverId.SetValue(guidValue, ver); break;
                    case nameof(Aircraft.PictureWidth):                     value = _Aircraft.PictureWidth.SetValue(intValue, ver); break;
                    case nameof(Aircraft.PictureHeight):                    value = _Aircraft.PictureHeight.SetValue(intValue, ver); break;
                    case nameof(Aircraft.VerticalRateType):                 value = _Aircraft.VerticalRateType.SetValue(AltitudeType.Geometric, ver); break;
                    case nameof(Aircraft.TrackIsHeading):                   value = _Aircraft.TrackIsHeading.SetValue(true, ver); break;
                    case nameof(Aircraft.TransponderType):                  value = _Aircraft.TransponderType.SetValue(TransponderType.Adsb2, ver); break;
                    case nameof(Aircraft.TargetAltitude):                   value = _Aircraft.TargetAltitude.SetValue(intValue, ver); break;
                    case nameof(Aircraft.TargetTrack):                      value = _Aircraft.TargetTrack.SetValue(doubleValue, ver); break;
                    case nameof(Aircraft.IsTisb):                           value = _Aircraft.IsTisb.SetValue(true, ver); break;
                    case nameof(Aircraft.YearBuilt):                        value = _Aircraft.YearBuilt.SetValue(stringValue, ver); break;
                    case nameof(Aircraft.GeometricAltitude):                value = _Aircraft.GeometricAltitude.SetValue(intValue, ver); break;
                    case nameof(Aircraft.AirPressureInHg):                  value = _Aircraft.AirPressureInHg.SetValue(doubleValue, ver); break;
                    case nameof(Aircraft.AirPressureLookedUpUtc):           value = _Aircraft.AirPressureLookedUpUtc = dateTimeValue; break;
                    case nameof(Aircraft.IdentActive):                      value = _Aircraft.IdentActive.SetValue(true, ver); break;
                    case nameof(Aircraft.IsCharterFlight):                  value = _Aircraft.IsCharterFlight.SetValue(true, ver); break;
                    case nameof(Aircraft.IsPositioningFlight):              value = _Aircraft.IsPositioningFlight.SetValue(true, ver); break;
                    default:                                                throw new NotImplementedException();
                }

                if(value != null) {
                    values.Add(property.Name, value);

                    if(property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(VersionedValue<>)) {
                        versions[property.Name] = ver;
                    }
                }
            }

            var clone = _Aircraft.Clone();

            foreach(var property in typeof(Aircraft).GetProperties()) {
                if(values.TryGetValue(property.Name, out var expected)) {
                    switch(property.Name) {
                        case nameof(Aircraft.Stopovers):               Assert.IsTrue(((Airport[])expected).SequenceEqual(clone.Stopovers.Value), property.Name); break;
                        case nameof(Aircraft.FullCoordinates):         Assert.AreEqual(expected, clone.FullCoordinates[0], property.Name); break;
                        case nameof(Aircraft.ShortCoordinates):        Assert.AreEqual(expected, clone.ShortCoordinates[0], property.Name); break;
                        default:
                            var actual = property.GetValue(clone, null);

                            object actualVer = null;
                            if(property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(VersionedValue<>)) {
                                var versionPropertyInfo = actual.GetType().GetProperty(nameof(VersionedValue<int>.DataVersion));
                                actualVer = versionPropertyInfo.GetValue(actual, null);

                                var valuePropertyInfo = actual.GetType().GetProperty(nameof(VersionedValue<int>.Value));
                                actual = valuePropertyInfo.GetValue(actual, null);
                            }

                            Assert.AreEqual(expected, actual, property.Name);
                            if(actualVer != null) {
                                Assert.AreEqual(versions[property.Name], (long)actualVer);
                            }

                            break;
                    }
                }
            }
        }

        [TestMethod]
        public void UpdateCoordinates_Adds_Coordinate_To_Full_And_Short_Trails()
        {
            var now = DateTime.UtcNow;

            _Aircraft.DataVersion = now.Ticks + 1;
            _Aircraft.Latitude.SetValue(10, 0);
            _Aircraft.Longitude.SetValue(20, 0);
            _Aircraft.Track.SetValue(30, 0);
            _Aircraft.Altitude.SetValue(20000, 0);
            _Aircraft.GroundSpeed.SetValue(250, 0);
            _Aircraft.UpdateCoordinates(now, 30);

            Assert.AreEqual(1, _Aircraft.FullCoordinates.Count);
            Assert.AreEqual(1, _Aircraft.ShortCoordinates.Count);

            var fullCoordinate = _Aircraft.FullCoordinates[0];
            Assert.AreEqual(10f, fullCoordinate.Latitude);
            Assert.AreEqual(20f, fullCoordinate.Longitude);
            Assert.AreEqual(30f, fullCoordinate.Heading);
            Assert.AreEqual(20000, fullCoordinate.Altitude);
            Assert.AreEqual(250, fullCoordinate.GroundSpeed);
            Assert.AreEqual(now.Ticks + 1, fullCoordinate.DataVersion);
            Assert.AreEqual(now.Ticks, fullCoordinate.Tick);

            var shortCoordinate = _Aircraft.ShortCoordinates[0];
            Assert.AreEqual(10f, shortCoordinate.Latitude);
            Assert.AreEqual(20f, shortCoordinate.Longitude);
            Assert.AreEqual(30f, shortCoordinate.Heading);
            Assert.AreEqual(20000, shortCoordinate.Altitude);
            Assert.AreEqual(250, shortCoordinate.GroundSpeed);
            Assert.AreEqual(now.Ticks + 1, shortCoordinate.DataVersion);
            Assert.AreEqual(now.Ticks, shortCoordinate.Tick);
        }

        [TestMethod]
        public void UpdateCoordinates_Updates_Coordinate_Times()
        {
            var now = DateTime.UtcNow;

            _Aircraft.DataVersion = now.Ticks + 1;
            _Aircraft.Latitude.SetValue(10, 0);
            _Aircraft.Longitude.SetValue(20, 0);
            _Aircraft.Track.SetValue(30, 0);
            _Aircraft.UpdateCoordinates(now, 30);

            _Aircraft.UpdateCoordinates(now, 30);

            Assert.AreEqual(now.Ticks + 1, _Aircraft.FirstCoordinateChanged);
            Assert.AreEqual(now.Ticks + 1, _Aircraft.LastCoordinateChanged);
            Assert.AreEqual(now, _Aircraft.LatestCoordinateTime);

            var nowPlusOne = now.AddSeconds(1);
            _Aircraft.DataVersion = nowPlusOne.Ticks;
            _Aircraft.Latitude.SetValue(10.0001, 0);
            _Aircraft.UpdateCoordinates(nowPlusOne, 30);

            Assert.AreEqual(now.Ticks + 1, _Aircraft.FirstCoordinateChanged);
            Assert.AreEqual(nowPlusOne.Ticks, _Aircraft.LastCoordinateChanged);
            Assert.AreEqual(nowPlusOne, _Aircraft.LatestCoordinateTime);
        }

        [TestMethod]
        public void UpdateCoordinates_Does_Not_Update_Trail_When_Message_Does_Not_Have_Position()
        {
            var now = DateTime.UtcNow;
            _Aircraft.DataVersion = now.Ticks;
            _Aircraft.UpdateCoordinates(now, 30);

            Assert.AreEqual(0, _Aircraft.ShortCoordinates.Count);
            Assert.AreEqual(0, _Aircraft.FullCoordinates.Count);
            Assert.AreEqual(0, _Aircraft.FirstCoordinateChanged);
            Assert.AreEqual(0, _Aircraft.LastCoordinateChanged);
            Assert.AreEqual(DateTime.MinValue, _Aircraft.LatestCoordinateTime);
        }

        [TestMethod]
        public void UpdateCoordinates_Copes_When_Messages_Come_In_From_Aircraft_With_No_Track()
        {
            // During initial testing it was found that if an aircraft transmitted three (or more) positions at different
            // locations over at least 3 seconds, but without moving more than 250 metres over those three messages, then
            // a null reference exception was thrown

            var now = DateTime.UtcNow;

            for(var i = 0;i < 3;++i) {
                now = now.AddSeconds(i * 2);

                var latitude = 10.0 + ((double)i / 100000.0);
                _Aircraft.DataVersion = now.Ticks;
                _Aircraft.Latitude.SetValue(latitude, 0);
                _Aircraft.Longitude.SetValue(20, 0);
                _Aircraft.Track.SetValue(null, 0);

                _Aircraft.UpdateCoordinates(now, 30);

                var lastCoordinate = _Aircraft.FullCoordinates.LastOrDefault();
                Assert.AreEqual(latitude, lastCoordinate.Latitude);
            }
        }

        [TestMethod]
        public void UpdateCoordinates_Ignores_Trail_Updates_Less_Than_A_Second_Apart()
        {
            var time = new DateTime(2001, 1, 1, 19, 01, 10);
            var timePlusPoint999 = time.AddMilliseconds(999);
            var timePlusOne = time.AddSeconds(1);
            var timePlusOnePoint999 = timePlusOne.AddMilliseconds(999);

            _Aircraft.DataVersion = time.Ticks;
            _Aircraft.Latitude.SetValue(10, 0);
            _Aircraft.Longitude.SetValue(20, 0);
            _Aircraft.UpdateCoordinates(time, 30);

            _Aircraft.DataVersion = timePlusPoint999.Ticks;
            _Aircraft.Latitude.SetValue(10.0001, 0);
            _Aircraft.Longitude.SetValue(20.0001, 0);
            _Aircraft.UpdateCoordinates(timePlusPoint999, 30);

            _Aircraft.DataVersion = timePlusOne.Ticks;
            _Aircraft.Latitude.SetValue(10.0002, 0);
            _Aircraft.Longitude.SetValue(20.0002, 0);
            _Aircraft.UpdateCoordinates(timePlusOne, 30);

            Assert.AreEqual(2, _Aircraft.ShortCoordinates.Count);
            Assert.AreEqual(2, _Aircraft.FullCoordinates.Count);

            Assert.AreEqual(10f, _Aircraft.ShortCoordinates[0].Latitude);
            Assert.AreEqual(10.0002f, _Aircraft.ShortCoordinates[1].Latitude);

            Assert.AreEqual(10, _Aircraft.FullCoordinates[0].Latitude);
            Assert.AreEqual(10.0002, _Aircraft.FullCoordinates[1].Latitude);

            // That has the basic stuff covered, but we want to make sure that we're using the latest time and not the time of the first coordinate
            _Aircraft.DataVersion = timePlusOnePoint999.Ticks;
            _Aircraft.Latitude.SetValue(10.0003, 0);
            _Aircraft.Longitude.SetValue(20.0003, 0);
            _Aircraft.UpdateCoordinates(timePlusOnePoint999, 30);

            Assert.AreEqual(2, _Aircraft.ShortCoordinates.Count);
        }

        [TestMethod]
        public void UpdateCoordinates_Ignores_Trail_Updates_When_The_Position_Does_Not_Change()
        {
            var time = DateTime.UtcNow;

            _Aircraft.DataVersion = time.Ticks;
            _Aircraft.Latitude.SetValue(51, 0);
            _Aircraft.Longitude.SetValue(12, 0);
            _Aircraft.Track.SetValue(40, 0);

            _Aircraft.UpdateCoordinates(time, 30);

            time = time.AddSeconds(1);
            _Aircraft.DataVersion = time.Ticks;
            _Aircraft.Track.SetValue(50, 0);
            _Aircraft.UpdateCoordinates(time, 30);

            Assert.AreEqual(1, _Aircraft.FullCoordinates.Count);
            Assert.AreEqual(40f, _Aircraft.FullCoordinates[0].Heading);

            Assert.AreEqual(1, _Aircraft.ShortCoordinates.Count);
            Assert.AreEqual(40f, _Aircraft.ShortCoordinates[0].Heading);
        }

        [TestMethod]
        public void UpdateCoordinates_Updates_Trail_When_Altitude_Changes_And_Poisition_Does_Not()
        {
            var time = DateTime.UtcNow;

            _Aircraft.DataVersion = time.Ticks;
            _Aircraft.Latitude.SetValue(51, 0);
            _Aircraft.Longitude.SetValue(12, 0);
            _Aircraft.Track.SetValue(40, 0);
            _Aircraft.Altitude.SetValue(10000, 0);

            _Aircraft.UpdateCoordinates(time, 30);

            time = time.AddSeconds(1);
            _Aircraft.DataVersion = time.Ticks;
            _Aircraft.Altitude.SetValue(11000, 0);
            _Aircraft.UpdateCoordinates(time, 30);

            Assert.AreEqual(2, _Aircraft.FullCoordinates.Count);
            Assert.AreEqual(11000, _Aircraft.FullCoordinates[1].Altitude);

            Assert.AreEqual(2, _Aircraft.ShortCoordinates.Count);
            Assert.AreEqual(11000, _Aircraft.ShortCoordinates[1].Altitude);
        }

        [TestMethod]
        public void UpdateCoordinates_Updates_Trail_When_GroundSpeed_Changes_And_Poisition_Does_Not()
        {
            var time = DateTime.UtcNow;

            _Aircraft.DataVersion = time.Ticks;
            _Aircraft.Latitude.SetValue(51, 0);
            _Aircraft.Longitude.SetValue(12, 0);
            _Aircraft.Track.SetValue(40, 0);
            _Aircraft.GroundSpeed.SetValue(100, 0);

            _Aircraft.UpdateCoordinates(time, 30);

            time = time.AddSeconds(1);
            _Aircraft.DataVersion = time.Ticks;
            _Aircraft.GroundSpeed.SetValue(150, 0);
            _Aircraft.UpdateCoordinates(time, 30);

            Assert.AreEqual(2, _Aircraft.FullCoordinates.Count);
            Assert.AreEqual(150, _Aircraft.FullCoordinates[1].GroundSpeed);

            Assert.AreEqual(2, _Aircraft.ShortCoordinates.Count);
            Assert.AreEqual(150, _Aircraft.ShortCoordinates[1].GroundSpeed);
        }

        [TestMethod]
        public void UpdateCoordinates_Replaces_Last_Full_Coordinate_If_Track_Hardly_Changes()
        {
            // If an update changes the position but the aircraft remains on the same ground track as the previous position
            // then there's no need to keep the previous position, it can be replaced with the current one. 

            var spreadsheet = new SpreadsheetTestData(TestData.AircraftTests, "FullCoordinateTrim");
            spreadsheet.TestEveryRow(this, row => {

                var time = DateTime.Now;

                var expected = new List<Coordinate>();
                for(int i = 1;i <= 3;++i) {
                    var latInColumn = String.Format("Lat{0}", i);
                    var lngInColumn = String.Format("Lng{0}", i);
                    var trkInColumn = String.Format("Trk{0}", i);
                    var altInColumn = String.Format("Alt{0}", i);
                    var spdInColumn = String.Format("Spd{0}", i);

                    var latOutColumn = String.Format("CLat{0}", i);
                    var lngOutColumn = String.Format("CLng{0}", i);
                    var trkOutColumn = String.Format("CTrk{0}", i);
                    var altOutColumn = String.Format("CAlt{0}", i);
                    var spdOutColumn = String.Format("CSpd{0}", i);

                    if(row.String(latInColumn) != null) {
                        time = time.AddHours(1);
                        _Aircraft.DataVersion = time.Ticks;
                        _Aircraft.Latitude.SetValue(row.Double(latInColumn), 0);
                        _Aircraft.Longitude.SetValue(row.Double(lngInColumn), 0);
                        _Aircraft.Altitude.SetValue(row.NInt(altInColumn), 0);
                        _Aircraft.GroundSpeed.SetValue(row.NFloat(spdInColumn), 0);
                        _Aircraft.Track.SetValue(row.NFloat(trkInColumn), 0);
                        _Aircraft.UpdateCoordinates(time, 300 * 60 * 60);

                        Assert.AreEqual(i, _Aircraft.ShortCoordinates.Count);
                    }

                    if(row.String(latOutColumn) != null) {
                        expected.Add(new Coordinate(
                            0,
                            0,
                            row.Float(latOutColumn),
                            row.Float(lngOutColumn),
                            row.NFloat(trkOutColumn),
                            row.NInt(altOutColumn),
                            row.NFloat(spdOutColumn)
                        ));
                    }
                }

                Assert.AreEqual(expected.Count, _Aircraft.FullCoordinates.Count);

                for(var i = 0;i < expected.Count;++i) {
                    var expCoordinate = expected[i];
                    var fullCoordinate = _Aircraft.FullCoordinates[i];

                    Assert.AreEqual(expCoordinate, fullCoordinate, i.ToString());
                    Assert.AreEqual(expCoordinate.Heading, fullCoordinate.Heading, i.ToString());
                }
            });
        }

        [TestMethod]
        public void UpdateCoordinates_Trims_Short_Coordinates_After_Time_Limit_Has_Passed()
        {
            var spreadsheet = new SpreadsheetTestData(TestData.AircraftTests, "ShortCoordinatesTrim");
            spreadsheet.TestEveryRow(this, row => {
                var seconds = row.Int("Seconds");
                var baseTime = DateTime.UtcNow;

                for(var i = 1;i <= 3;++i) {
                    var secondsGapColumn = String.Format("SecondsGap{0}", i);
                    var latitudeColumn = String.Format("Latitude{0}", i);
                    var longitudeColumn = String.Format("Longitude{0}", i);
                    var trackColumn = String.Format("Track{0}", i);

                    if(row.String(secondsGapColumn) != null) {
                        var utcNow = baseTime.AddSeconds(row.Int(secondsGapColumn));
                        _Aircraft.DataVersion = utcNow.Ticks;
                        _Aircraft.Latitude.SetValue(row.NDouble(latitudeColumn), 0);
                        _Aircraft.Longitude.SetValue(row.NDouble(longitudeColumn), 0);
                        _Aircraft.Track.SetValue(row.NDouble(trackColumn), 0);
                        _Aircraft.UpdateCoordinates(utcNow, seconds);
                    }
                }

                Assert.AreEqual(row.Int("ExpectedCount"), _Aircraft.ShortCoordinates.Count);
                for(var i = 0;i < _Aircraft.ShortCoordinates.Count;++i) {
                    var expectedTrackColumn = String.Format("ExpectedTrack{0}", i);
                    Assert.AreEqual(row.NFloat(expectedTrackColumn), _Aircraft.ShortCoordinates[i].Heading, i.ToString());
                }
            });
        }

        [TestMethod]
        public void UpdateCoordinates_Resets_Coordinates_If_Aircraft_Looks_To_Be_Moving_Impossibly_Quickly()
        {
            // Some aircraft's transponders report the wrong position. Further, some radios misinterpret position updates at
            // extreme ranges. We can guess the position is wrong if the position and timing of two updates would require the
            // aircraft to be moving at incredibly high speeds. When this is detected we should reset the trails, otherwise the
            // user sees long lines drawn across the map, or a scribble effect if the transponder is just reporting nonsense.

            var spreadsheet = new SpreadsheetTestData(TestData.AircraftTests, "ResetCoordinates");
            spreadsheet.TestEveryRow(this, row => {
                var time = DateTime.UtcNow;
                var shortSeconds = 24 * 60 * 60; // seconds in a day

                _Aircraft.DataVersion = time.Ticks;
                _Aircraft.Latitude.SetValue(row.Double("Lat1"), 0);
                _Aircraft.Longitude.SetValue(row.Double("Lng1"), 0);
                _Aircraft.Track.SetValue(10, 0);
                _Aircraft.UpdateCoordinates(time, shortSeconds);

                int milliseconds = row.Int("Milliseconds");

                time = time.AddMilliseconds(milliseconds);
                _Aircraft.DataVersion = time.Ticks;
                _Aircraft.Latitude.SetValue(row.Double("Lat2"), 0);
                _Aircraft.Longitude.SetValue(row.Double("Lng2"), 0);
                _Aircraft.Track.SetValue(20, 0);
                _Aircraft.UpdateCoordinates(time, shortSeconds);

                if(!row.Bool("ResetTrail")) {
                    if(milliseconds >= 1000) {
                        Assert.AreEqual(2, _Aircraft.FullCoordinates.Count);
                        Assert.AreEqual(2, _Aircraft.ShortCoordinates.Count);
                    } else {
                        Assert.AreEqual(1, _Aircraft.FullCoordinates.Count);
                        Assert.AreEqual(1, _Aircraft.ShortCoordinates.Count);
                        Assert.AreEqual(10, _Aircraft.FullCoordinates[0].Heading);
                        Assert.AreEqual(10, _Aircraft.ShortCoordinates[0].Heading);
                    }
                } else {
                    if(milliseconds < 1000) {
                        Assert.AreEqual(0, _Aircraft.FullCoordinates.Count);
                        Assert.AreEqual(0, _Aircraft.ShortCoordinates.Count);
                    } else {
                        Assert.AreEqual(1, _Aircraft.FullCoordinates.Count);
                        Assert.AreEqual(1, _Aircraft.ShortCoordinates.Count);
                        Assert.AreEqual(20, _Aircraft.FullCoordinates[0].Heading);
                        Assert.AreEqual(20, _Aircraft.ShortCoordinates[0].Heading);
                    }
                }
            });
        }

        [TestMethod]
        public void UpdateCoordinates_Sets_PositonTime_When_Latitude_And_Longitude_Are_Changed()
        {
            var now = DateTime.UtcNow;

            _Aircraft.DataVersion = now.Ticks;
            _Aircraft.Latitude.SetValue(1, 0);
            _Aircraft.Longitude.SetValue(1, 0);
            _Aircraft.UpdateCoordinates(now, 30);

            Assert.AreEqual(now, _Aircraft.PositionTime.Value);
        }

        [TestMethod]
        public void UpdateCoordinates_Does_Not_Set_PositionTime_If_Coordinates_Have_Not_Changed_Since_Last_Update()
        {
            var now = DateTime.UtcNow;

            _Aircraft.DataVersion = now.Ticks;
            _Aircraft.Latitude.SetValue(1, 0);
            _Aircraft.Longitude.SetValue(1, 0);
            _Aircraft.UpdateCoordinates(now, 30);

            Assert.AreEqual(now, _Aircraft.PositionTime.Value);

            var newTime = now.AddSeconds(10);
            _Aircraft.DataVersion = newTime.Ticks;
            _Aircraft.UpdateCoordinates(newTime, 30);

            Assert.AreEqual(now, _Aircraft.PositionTime.Value);

            newTime = now.AddSeconds(20);
            _Aircraft.DataVersion = newTime.Ticks;
            _Aircraft.Latitude.SetValue(2, 0);
            _Aircraft.UpdateCoordinates(newTime, 30);

            Assert.AreEqual(newTime, _Aircraft.PositionTime.Value);

            newTime = now.AddSeconds(25);
            _Aircraft.DataVersion = newTime.Ticks;
            _Aircraft.Longitude.SetValue(2, 0);
            _Aircraft.UpdateCoordinates(newTime, 30);

            Assert.AreEqual(newTime, _Aircraft.PositionTime.Value);
        }
    }
}
