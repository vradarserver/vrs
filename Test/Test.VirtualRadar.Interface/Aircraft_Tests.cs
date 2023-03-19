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
            _Aircraft.FullCoordinates.Add(new Coordinate(1, 2, 3, 4, null));
            _Aircraft.ShortCoordinates.Add(new Coordinate(1, 2, 3, 4, null));
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
            Dictionary<string, object> values = new Dictionary<string,object>();

            var intValue = 1;
            double doubleValue = 121.3414;
            var longValue = (long)int.MaxValue + 1L;
            var dateTimeValue = new DateTime(2001, 1, 2);

            var allBoolPropertyNames = typeof(Aircraft).GetProperties().Where(r => r.PropertyType == typeof(bool) || r.PropertyType == typeof(bool?)).Select(r => r.Name).ToList();
            foreach(var trueBool in allBoolPropertyNames) {
                TestInitialise();
                values.Clear();

                for(int setChangedValues = 0;setChangedValues < 2;++setChangedValues) {
                    // Set the properties of the object to clone using reflection to make sure
                    // we don't forget to update this test in the future...
                    foreach(var property in typeof(Aircraft).GetProperties()) {
                        if(property.Name == "DataVersion") continue;
                        var isChangedValue = property.Name.EndsWith("Changed");
                        if(   (setChangedValues == 0 && isChangedValue)
                           || (setChangedValues == 1 && !isChangedValue)
                        ) {
                            continue;
                        }

                        ++intValue;
                        ++doubleValue;
                        ++longValue;
                        dateTimeValue = dateTimeValue.AddDays(1);
                        ++_Aircraft.DataVersion;
                        var stringValue = String.Format("A{0}", intValue);
                        var guidValue = Guid.NewGuid();
                        var airport = new Airport() { IcaoCode = stringValue, IataCode = stringValue };

                        if(!values.ContainsKey("DataVersion")) values.Add("DataVersion", _Aircraft.DataVersion);
                        else                                   values["DataVersion"] = _Aircraft.DataVersion;

                        object value = null;
                        switch(property.Name) {
                            case nameof(Aircraft.UniqueId):                        value = _Aircraft.UniqueId = intValue; break;
                            case nameof(Aircraft.Icao24):                          value = _Aircraft.Icao24 = stringValue; break;
                            case nameof(Aircraft.Icao24Changed):                   value = _Aircraft.Icao24Changed; break;
                            case nameof(Aircraft.Icao24Invalid):                   value = _Aircraft.Icao24Invalid = property.Name == trueBool; break;
                            case nameof(Aircraft.Icao24InvalidChanged):            value = _Aircraft.Icao24InvalidChanged; break;
                            case nameof(Aircraft.Callsign):                        value = _Aircraft.Callsign = stringValue; break;
                            case nameof(Aircraft.CallsignChanged):                 value = _Aircraft.CallsignChanged; break;
                            case nameof(Aircraft.CallsignIsSuspect):               value = _Aircraft.CallsignIsSuspect = property.Name == trueBool; break;
                            case nameof(Aircraft.CallsignIsSuspectChanged):        value = _Aircraft.CallsignIsSuspectChanged; break;
                            case nameof(Aircraft.CountMessagesReceived):           value = _Aircraft.CountMessagesReceived = longValue; break;
                            case nameof(Aircraft.CountMessagesReceivedChanged):    value = _Aircraft.CountMessagesReceivedChanged; break;
                            case nameof(Aircraft.Altitude):                        value = _Aircraft.Altitude = intValue; break;
                            case nameof(Aircraft.AltitudeChanged):                 value = _Aircraft.AltitudeChanged; break;
                            case nameof(Aircraft.AltitudeType):                    value = _Aircraft.AltitudeType = AltitudeType.Geometric; break;
                            case nameof(Aircraft.AltitudeTypeChanged):             value = _Aircraft.AltitudeTypeChanged; break;
                            case nameof(Aircraft.GroundSpeed):                     value = _Aircraft.GroundSpeed = intValue; break;
                            case nameof(Aircraft.GroundSpeedChanged):              value = _Aircraft.GroundSpeedChanged; break;
                            case nameof(Aircraft.Latitude):                        value = _Aircraft.Latitude = doubleValue; break;
                            case nameof(Aircraft.LatitudeChanged):                 value = _Aircraft.LatitudeChanged; break;
                            case nameof(Aircraft.Longitude):                       value = _Aircraft.Longitude = doubleValue; break;
                            case nameof(Aircraft.LongitudeChanged):                value = _Aircraft.LongitudeChanged; break;
                            case nameof(Aircraft.PositionIsMlat):                  value = _Aircraft.PositionIsMlat = property.Name == trueBool; break;
                            case nameof(Aircraft.PositionIsMlatChanged):           value = _Aircraft.PositionIsMlatChanged; break;
                            case nameof(Aircraft.PositionReceiverId):              value = _Aircraft.PositionReceiverId = guidValue; break;
                            case nameof(Aircraft.PositionReceiverIdChanged):       value = _Aircraft.PositionReceiverIdChanged; break;
                            case nameof(Aircraft.PositionTime):                    value = _Aircraft.PositionTime = dateTimeValue; break;
                            case nameof(Aircraft.PositionTimeChanged):             value = _Aircraft.PositionTimeChanged; break;
                            case nameof(Aircraft.Track):                           value = _Aircraft.Track = doubleValue; break;
                            case nameof(Aircraft.TrackChanged):                    value = _Aircraft.TrackChanged; break;
                            case nameof(Aircraft.IsTransmittingTrack):             value = _Aircraft.IsTransmittingTrack = property.Name == trueBool; break;
                            case nameof(Aircraft.VerticalRate):                    value = _Aircraft.VerticalRate = intValue; break;
                            case nameof(Aircraft.VerticalRateChanged):             value = _Aircraft.VerticalRateChanged; break;
                            case nameof(Aircraft.Squawk):                          value = _Aircraft.Squawk = intValue; break;
                            case nameof(Aircraft.SquawkChanged):                   value = _Aircraft.SquawkChanged; break;
                            case nameof(Aircraft.Emergency):                       value = _Aircraft.Emergency = property.Name == trueBool; break;
                            case nameof(Aircraft.EmergencyChanged):                value = _Aircraft.EmergencyChanged; break;
                            case nameof(Aircraft.Registration):                    value = _Aircraft.Registration = stringValue; break;
                            case nameof(Aircraft.RegistrationChanged):             value = _Aircraft.RegistrationChanged; break;
                            case nameof(Aircraft.FirstSeen):                       value = _Aircraft.FirstSeen = dateTimeValue; break;
                            case nameof(Aircraft.FirstSeenChanged):                value = _Aircraft.FirstSeenChanged; break;
                            case nameof(Aircraft.FlightsCount):                    value = _Aircraft.FlightsCount = intValue; break;
                            case nameof(Aircraft.FlightsCountChanged):             value = _Aircraft.FlightsCountChanged; break;
                            case nameof(Aircraft.LastUpdate):                      value = _Aircraft.LastUpdate = dateTimeValue; break;
                            case nameof(Aircraft.LastModeSUpdate):                 value = _Aircraft.LastModeSUpdate = dateTimeValue; break;
                            case nameof(Aircraft.LastSatcomUpdate):                value = _Aircraft.LastSatcomUpdate = dateTimeValue; break;
                            case nameof(Aircraft.Type):                            value = _Aircraft.Type = stringValue; break;
                            case nameof(Aircraft.TypeChanged):                     value = _Aircraft.TypeChanged; break;
                            case nameof(Aircraft.Manufacturer):                    value = _Aircraft.Manufacturer = stringValue; break;
                            case nameof(Aircraft.ManufacturerChanged):             value = _Aircraft.ManufacturerChanged; break;
                            case nameof(Aircraft.Model):                           value = _Aircraft.Model = stringValue; break;
                            case nameof(Aircraft.ModelChanged):                    value = _Aircraft.ModelChanged; break;
                            case nameof(Aircraft.ConstructionNumber):              value = _Aircraft.ConstructionNumber = stringValue; break;
                            case nameof(Aircraft.ConstructionNumberChanged):       value = _Aircraft.ConstructionNumberChanged; break;
                            case nameof(Aircraft.Origin):                          value = _Aircraft.Origin = airport; break;
                            case nameof(Aircraft.OriginChanged):                   value = _Aircraft.OriginChanged; break;
                            case nameof(Aircraft.Destination):                     value = _Aircraft.Destination = airport; break;
                            case nameof(Aircraft.DestinationChanged):              value = _Aircraft.DestinationChanged; break;
                            case nameof(Aircraft.SignalLevel):                     value = _Aircraft.SignalLevel = intValue; break;
                            case nameof(Aircraft.SignalLevelChanged):              value = _Aircraft.SignalLevelChanged; break;
                            case nameof(Aircraft.Stopovers):                       value = _Aircraft.Stopovers = new Airport[] { airport, }; break;
                            case nameof(Aircraft.StopoversChanged):                value = _Aircraft.StopoversChanged; break;
                            case nameof(Aircraft.Operator):                        value = _Aircraft.Operator = stringValue; break;
                            case nameof(Aircraft.OperatorChanged):                 value = _Aircraft.OperatorChanged; break;
                            case nameof(Aircraft.OperatorIcao):                    value = _Aircraft.OperatorIcao = stringValue; break;
                            case nameof(Aircraft.OperatorIcaoChanged):             value = _Aircraft.OperatorIcaoChanged; break;
                            case nameof(Aircraft.WakeTurbulenceCategory):          value = _Aircraft.WakeTurbulenceCategory = WakeTurbulenceCategory.Light; break;
                            case nameof(Aircraft.WakeTurbulenceCategoryChanged):   value = _Aircraft.WakeTurbulenceCategoryChanged; break;
                            case nameof(Aircraft.EnginePlacement):                 value = _Aircraft.EnginePlacement = EnginePlacement.AftMounted; break;
                            case nameof(Aircraft.EnginePlacementChanged):          value = _Aircraft.EnginePlacementChanged; break;
                            case nameof(Aircraft.EngineType):                      value = _Aircraft.EngineType = EngineType.Electric; break;
                            case nameof(Aircraft.EngineTypeChanged):               value = _Aircraft.EngineTypeChanged; break;
                            case nameof(Aircraft.NumberOfEngines):                 value = _Aircraft.NumberOfEngines = stringValue; break;
                            case nameof(Aircraft.NumberOfEnginesChanged):          value = _Aircraft.NumberOfEnginesChanged; break;
                            case nameof(Aircraft.Species):                         value = _Aircraft.Species = Species.Gyrocopter; break;
                            case nameof(Aircraft.SpeciesChanged):                  value = _Aircraft.SpeciesChanged; break;
                            case nameof(Aircraft.IsMilitary):                      value = _Aircraft.IsMilitary = property.Name == trueBool; break;
                            case nameof(Aircraft.IsMilitaryChanged):               value = _Aircraft.IsMilitaryChanged; break;
                            case nameof(Aircraft.Icao24Country):                   value = _Aircraft.Icao24Country = stringValue; break;
                            case nameof(Aircraft.Icao24CountryChanged):            value = _Aircraft.Icao24CountryChanged; break;
                            case nameof(Aircraft.PictureFileName):                 value = _Aircraft.PictureFileName = stringValue; break;
                            case nameof(Aircraft.PictureFileNameChanged):          value = _Aircraft.PictureFileNameChanged; break;
                            case nameof(Aircraft.FirstCoordinateChanged):          value = _Aircraft.FirstCoordinateChanged = longValue; break;
                            case nameof(Aircraft.LastCoordinateChanged):           value = _Aircraft.LastCoordinateChanged = longValue; break;
                            case nameof(Aircraft.LatestCoordinateTime):            value = _Aircraft.LatestCoordinateTime = dateTimeValue; break;
                            case nameof(Aircraft.FullCoordinates):                 _Aircraft.FullCoordinates.Add(new Coordinate(1, 2, 3, 4, 5)); value = _Aircraft.FullCoordinates[0]; break;
                            case nameof(Aircraft.ShortCoordinates):                _Aircraft.ShortCoordinates.Add(new Coordinate(11, 12, 13, 14, 15)); value = _Aircraft.ShortCoordinates[0]; break;
                            case nameof(Aircraft.IcaoCompliantRegistration):       value = _Aircraft.Registration; break;
                            case nameof(Aircraft.IsInteresting):                   value = _Aircraft.IsInteresting = property.Name == trueBool; break;
                            case nameof(Aircraft.IsInterestingChanged):            value = _Aircraft.IsInterestingChanged; break;
                            case nameof(Aircraft.OnGround):                        value = _Aircraft.OnGround = property.Name == trueBool; break;
                            case nameof(Aircraft.OnGroundChanged):                 value = _Aircraft.OnGroundChanged; break;
                            case nameof(Aircraft.SpeedType):                       value = _Aircraft.SpeedType = SpeedType.TrueAirSpeed; break;
                            case nameof(Aircraft.SpeedTypeChanged):                value = _Aircraft.SpeedTypeChanged; break;
                            case nameof(Aircraft.UserTag):                         value = _Aircraft.UserTag = stringValue; break;
                            case nameof(Aircraft.UserTagChanged):                  value = _Aircraft.UserTagChanged; break;
                            case nameof(Aircraft.UserNotes):                       value = _Aircraft.UserNotes = stringValue; break;
                            case nameof(Aircraft.UserNotesChanged):                value = _Aircraft.UserNotesChanged; break;
                            case nameof(Aircraft.ReceiverId):                      value = _Aircraft.ReceiverId = guidValue; break;
                            case nameof(Aircraft.ReceiverIdChanged):               value = _Aircraft.ReceiverIdChanged; break;
                            case nameof(Aircraft.PictureWidth):                    value = _Aircraft.PictureWidth = intValue; break;
                            case nameof(Aircraft.PictureWidthChanged):             value = _Aircraft.PictureWidthChanged; break;
                            case nameof(Aircraft.PictureHeight):                   value = _Aircraft.PictureHeight = intValue; break;
                            case nameof(Aircraft.PictureHeightChanged):            value = _Aircraft.PictureHeightChanged; break;
                            case nameof(Aircraft.VerticalRateType):                value = _Aircraft.VerticalRateType = AltitudeType.Geometric; break;
                            case nameof(Aircraft.VerticalRateTypeChanged):         value = _Aircraft.VerticalRateTypeChanged; break;
                            case nameof(Aircraft.TrackIsHeading):                  value = _Aircraft.TrackIsHeading = property.Name == trueBool; break;
                            case nameof(Aircraft.TrackIsHeadingChanged):           value = _Aircraft.TrackIsHeadingChanged; break;
                            case nameof(Aircraft.TransponderType):                 value = _Aircraft.TransponderType = TransponderType.Adsb2; break;
                            case nameof(Aircraft.TransponderTypeChanged):          value = _Aircraft.TransponderTypeChanged; break;
                            case nameof(Aircraft.TargetAltitude):                  value = _Aircraft.TargetAltitude = intValue; break;
                            case nameof(Aircraft.TargetAltitudeChanged):           value = _Aircraft.TargetAltitudeChanged; break;
                            case nameof(Aircraft.TargetTrack):                     value = _Aircraft.TargetTrack = doubleValue; break;
                            case nameof(Aircraft.TargetTrackChanged):              value = _Aircraft.TargetTrackChanged; break;
                            case nameof(Aircraft.IsTisb):                          value = _Aircraft.IsTisb = property.Name == trueBool; break;
                            case nameof(Aircraft.IsTisbChanged):                   value = _Aircraft.IsTisbChanged; break;
                            case nameof(Aircraft.YearBuilt):                       value = _Aircraft.YearBuilt = stringValue; break;
                            case nameof(Aircraft.YearBuiltChanged):                value = _Aircraft.YearBuiltChanged; break;
                            case nameof(Aircraft.GeometricAltitude):               value = _Aircraft.GeometricAltitude = intValue; break;
                            case nameof(Aircraft.GeometricAltitudeChanged):        value = _Aircraft.GeometricAltitudeChanged; break;
                            case nameof(Aircraft.AirPressureInHg):                 value = _Aircraft.AirPressureInHg = doubleValue; break;
                            case nameof(Aircraft.AirPressureInHgChanged):          value = _Aircraft.AirPressureInHgChanged; break;
                            case nameof(Aircraft.AirPressureLookedUpUtc):          value = _Aircraft.AirPressureLookedUpUtc = dateTimeValue; break;
                            case nameof(Aircraft.IdentActive):                     value = _Aircraft.IdentActive = property.Name == trueBool; break;
                            case nameof(Aircraft.IdentActiveChanged):              value = _Aircraft.IdentActiveChanged; break;
                            case nameof(Aircraft.IsCharterFlight):                 value = _Aircraft.IsCharterFlight = property.Name == trueBool; break;
                            case nameof(Aircraft.IsCharterFlightChanged):          value = _Aircraft.IsCharterFlightChanged; break;
                            case nameof(Aircraft.IsPositioningFlight):             value = _Aircraft.IsPositioningFlight = property.Name == trueBool; break;
                            case nameof(Aircraft.IsPositioningFlightChanged):      value = _Aircraft.IsPositioningFlightChanged; break;
                            default:                                                throw new NotImplementedException();
                        }

                        Assert.IsNotNull(value, property.Name);
                        values.Add(property.Name, value);
                    }
                }

                var clone = (Aircraft)_Aircraft.Clone();

                foreach(var property in typeof(Aircraft).GetProperties()) {
                    var expected = values[property.Name];
                    switch(property.Name) {
                        case nameof(Aircraft.Stopovers):               Assert.AreSame(expected, clone.Stopovers, property.Name); break;
                        case nameof(Aircraft.FullCoordinates):         Assert.AreEqual(expected, clone.FullCoordinates[0], property.Name); break;
                        case nameof(Aircraft.ShortCoordinates):        Assert.AreEqual(expected, clone.ShortCoordinates[0], property.Name); break;
                        default:
                            var actual = property.GetValue(clone, null);
                            Assert.AreEqual(expected, actual, property.Name); break;
                    }
                }
            }
        }

        [TestMethod]
        public void UpdateCoordinates_Adds_Coordinate_To_Full_And_Short_Trails()
        {
            var now = DateTime.UtcNow;

            _Aircraft.DataVersion = now.Ticks + 1;
            _Aircraft.Latitude = 10;
            _Aircraft.Longitude = 20;
            _Aircraft.Track = 30;
            _Aircraft.Altitude = 20000;
            _Aircraft.GroundSpeed = 250;
            _Aircraft.UpdateCoordinates(now, 30);

            Assert.AreEqual(1, _Aircraft.FullCoordinates.Count);
            Assert.AreEqual(1, _Aircraft.ShortCoordinates.Count);

            var fullCoordinate = _Aircraft.FullCoordinates[0];
            Assert.AreEqual(10, fullCoordinate.Latitude);
            Assert.AreEqual(20, fullCoordinate.Longitude);
            Assert.AreEqual(30, fullCoordinate.Heading);
            Assert.AreEqual(20000, fullCoordinate.Altitude);
            Assert.AreEqual(250, fullCoordinate.GroundSpeed);
            Assert.AreEqual(now.Ticks + 1, fullCoordinate.DataVersion);
            Assert.AreEqual(now.Ticks, fullCoordinate.Tick);

            var shortCoordinate = _Aircraft.ShortCoordinates[0];
            Assert.AreEqual(10, shortCoordinate.Latitude);
            Assert.AreEqual(20, shortCoordinate.Longitude);
            Assert.AreEqual(30, shortCoordinate.Heading);
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
            _Aircraft.Latitude = 10;
            _Aircraft.Longitude = 20;
            _Aircraft.Track = 30;
            _Aircraft.UpdateCoordinates(now, 30);

            _Aircraft.UpdateCoordinates(now, 30);

            Assert.AreEqual(now.Ticks + 1, _Aircraft.FirstCoordinateChanged);
            Assert.AreEqual(now.Ticks + 1, _Aircraft.LastCoordinateChanged);
            Assert.AreEqual(now, _Aircraft.LatestCoordinateTime);

            var nowPlusOne = now.AddSeconds(1);
            _Aircraft.DataVersion = nowPlusOne.Ticks;
            _Aircraft.Latitude = 10.0001;
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
                _Aircraft.Latitude = latitude;
                _Aircraft.Longitude = 20;
                _Aircraft.Track = null;

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
            _Aircraft.Latitude = 10;
            _Aircraft.Longitude = 20;
            _Aircraft.UpdateCoordinates(time, 30);

            _Aircraft.DataVersion = timePlusPoint999.Ticks;
            _Aircraft.Latitude = 10.0001;
            _Aircraft.Longitude = 20.0001;
            _Aircraft.UpdateCoordinates(timePlusPoint999, 30);

            _Aircraft.DataVersion = timePlusOne.Ticks;
            _Aircraft.Latitude = 10.0002;
            _Aircraft.Longitude = 20.0002;
            _Aircraft.UpdateCoordinates(timePlusOne, 30);

            Assert.AreEqual(2, _Aircraft.ShortCoordinates.Count);
            Assert.AreEqual(2, _Aircraft.FullCoordinates.Count);

            Assert.AreEqual(10, _Aircraft.ShortCoordinates[0].Latitude);
            Assert.AreEqual(10.0002, _Aircraft.ShortCoordinates[1].Latitude);

            Assert.AreEqual(10, _Aircraft.FullCoordinates[0].Latitude);
            Assert.AreEqual(10.0002, _Aircraft.FullCoordinates[1].Latitude);

            // That has the basic stuff covered, but we want to make sure that we're using the latest time and not the time of the first coordinate
            _Aircraft.DataVersion = timePlusOnePoint999.Ticks;
            _Aircraft.Latitude = 10.0003;
            _Aircraft.Longitude = 20.0003;
            _Aircraft.UpdateCoordinates(timePlusOnePoint999, 30);

            Assert.AreEqual(2, _Aircraft.ShortCoordinates.Count);
        }

        [TestMethod]
        public void UpdateCoordinates_Ignores_Trail_Updates_When_The_Position_Does_Not_Change()
        {
            var time = DateTime.UtcNow;

            _Aircraft.DataVersion = time.Ticks;
            _Aircraft.Latitude = 51;
            _Aircraft.Longitude = 12;
            _Aircraft.Track = 40;

            _Aircraft.UpdateCoordinates(time, 30);

            time = time.AddSeconds(1);
            _Aircraft.DataVersion = time.Ticks;
            _Aircraft.Track = 50;
            _Aircraft.UpdateCoordinates(time, 30);

            Assert.AreEqual(1, _Aircraft.FullCoordinates.Count);
            Assert.AreEqual(40, _Aircraft.FullCoordinates[0].Heading);

            Assert.AreEqual(1, _Aircraft.ShortCoordinates.Count);
            Assert.AreEqual(40, _Aircraft.ShortCoordinates[0].Heading);
        }

        [TestMethod]
        public void UpdateCoordinates_Updates_Trail_When_Altitude_Changes_And_Poisition_Does_Not()
        {
            var time = DateTime.UtcNow;

            _Aircraft.DataVersion = time.Ticks;
            _Aircraft.Latitude = 51;
            _Aircraft.Longitude = 12;
            _Aircraft.Track = 40;
            _Aircraft.Altitude = 10000;

            _Aircraft.UpdateCoordinates(time, 30);

            time = time.AddSeconds(1);
            _Aircraft.DataVersion = time.Ticks;
            _Aircraft.Altitude = 11000;
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
            _Aircraft.Latitude = 51;
            _Aircraft.Longitude = 12;
            _Aircraft.Track = 40;
            _Aircraft.GroundSpeed = 100;

            _Aircraft.UpdateCoordinates(time, 30);

            time = time.AddSeconds(1);
            _Aircraft.DataVersion = time.Ticks;
            _Aircraft.GroundSpeed = 150;
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
                        _Aircraft.Latitude = row.Double(latInColumn);
                        _Aircraft.Longitude = row.Double(lngInColumn);
                        _Aircraft.Altitude = row.NInt(altInColumn);
                        _Aircraft.GroundSpeed = row.NFloat(spdInColumn);
                        _Aircraft.Track = row.NFloat(trkInColumn);
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
                        _Aircraft.Latitude = row.NDouble(latitudeColumn);
                        _Aircraft.Longitude = row.NDouble(longitudeColumn);
                        _Aircraft.Track = row.NDouble(trackColumn);
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
                _Aircraft.Latitude = row.Double("Lat1");
                _Aircraft.Longitude = row.Double("Lng1");
                _Aircraft.Track = 10;
                _Aircraft.UpdateCoordinates(time, shortSeconds);

                int milliseconds = row.Int("Milliseconds");

                time = time.AddMilliseconds(milliseconds);
                _Aircraft.DataVersion = time.Ticks;
                _Aircraft.Latitude = row.Double("Lat2");
                _Aircraft.Longitude = row.Double("Lng2");
                _Aircraft.Track = 20;
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
            _Aircraft.Latitude = 1;
            _Aircraft.Longitude = 1;
            _Aircraft.UpdateCoordinates(now, 30);

            Assert.AreEqual(now, _Aircraft.PositionTime.Value);
        }

        [TestMethod]
        public void UpdateCoordinates_Does_Not_Set_PositionTime_If_Coordinates_Have_Not_Changed_Since_Last_Update()
        {
            var now = DateTime.UtcNow;

            _Aircraft.DataVersion = now.Ticks;
            _Aircraft.Latitude = 1;
            _Aircraft.Longitude = 1;
            _Aircraft.UpdateCoordinates(now, 30);

            Assert.AreEqual(now, _Aircraft.PositionTime.Value);

            var newTime = now.AddSeconds(10);
            _Aircraft.DataVersion = newTime.Ticks;
            _Aircraft.UpdateCoordinates(newTime, 30);

            Assert.AreEqual(now, _Aircraft.PositionTime.Value);

            newTime = now.AddSeconds(20);
            _Aircraft.DataVersion = newTime.Ticks;
            _Aircraft.Latitude = 2;
            _Aircraft.UpdateCoordinates(newTime, 30);

            Assert.AreEqual(newTime, _Aircraft.PositionTime.Value);

            newTime = now.AddSeconds(25);
            _Aircraft.DataVersion = newTime.Ticks;
            _Aircraft.Longitude = 2;
            _Aircraft.UpdateCoordinates(newTime, 30);

            Assert.AreEqual(newTime, _Aircraft.PositionTime.Value);
        }
    }
}
