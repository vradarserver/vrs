using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.KineticData;
using VirtualRadar.Interface.Options;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.Database.SQLite
{
    public class CommonBaseStationDatabaseTests
    {
        protected virtual string _SchemaPrefix => "";
        protected virtual bool _EngineTruncatesMilliseconds => false;
        protected virtual string _SqlReturnNewIdentity => null;
        protected IBaseStationDatabase _Implementation;
        protected Func<IDbConnection> _CreateConnection;
        protected EnvironmentOptions _EnvironmentOptions;
        protected BaseStationDatabaseOptions _BaseStationDatabaseOptions;
        protected MockSharedConfiguration _SharedConfiguration;
        protected Configuration _Configuration;
        protected MockClock _MockClock;
        protected MockStandingDataManager _StandingData;

        protected readonly string[] _Cultures = new string[] {
            "en-GB",
            "de-DE",
            "fr-FR",
            "it-IT",
            "el-GR",
            "ru-RU",
        };

        protected KineticAircraft _DefaultAircraft;
        protected KineticSession _DefaultSession;
        protected KineticLocation _DefaultLocation;
        protected KineticFlight _DefaultFlight;

        protected void CommonTestInitialise()
        {
            _SharedConfiguration = new();
            _Configuration = _SharedConfiguration.Configuration;
            _EnvironmentOptions = new() {
                WorkingFolder = Path.GetTempPath(),
            };
            _BaseStationDatabaseOptions = new();
            _MockClock = new();
            _StandingData = new();

            _DefaultAircraft = new() {
                ModeS = "123456",
            };
            _DefaultFlight = new() {
                StartTime = DateTime.Now,
            };
            _DefaultLocation = new() {
                Altitude = 25,
                Latitude = 54.1,
                Longitude = -0.6,
                LocationName = "Default Location",
            };
            _DefaultSession = new() {
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddSeconds(30),
            };
        }

        private MethodInfo _TestInitialise;
        private void RunTestInitialise()
        {
            if(_TestInitialise == null) {
                _TestInitialise = GetType()
                    .GetMethods()
                    .Single(r => r.GetCustomAttributes(typeof(TestInitializeAttribute), inherit: false).Length != 0);
            }
            _TestInitialise.Invoke(this, Array.Empty<object>());
        }

        private MethodInfo _TestCleanup;
        private void RunTestCleanup()
        {
            if(_TestCleanup == null) {
                _TestCleanup = GetType()
                    .GetMethods()
                    .Single(r => r.GetCustomAttributes(typeof(TestCleanupAttribute), inherit: false).Length != 0);
            }
            _TestCleanup.Invoke(this, Array.Empty<object>());
        }

        #region Aircraft Utility Methods
        protected long AddAircraft(KineticAircraft aircraft)
        {
            long result = 0;

            using(var connection = _CreateConnection()) {
                connection.Open();

                var dynamicParameters = new Dapper.DynamicParameters();
                var fieldNames = new StringBuilder();
                var parameters = new StringBuilder();

                foreach(var property in typeof(KineticAircraft).GetProperties()) {
                    var fieldName = property.Name;
                    if(fieldName == nameof(KineticAircraft.AircraftID)) {
                        continue;
                    }

                    if(fieldNames.Length > 0) {
                        fieldNames.Append(',');
                    }
                    if(parameters.Length > 0) {
                        parameters.Append(',');
                    }

                    fieldNames.Append($"[{fieldName}]");
                    parameters.Append($"@{fieldName}");

                    dynamicParameters.Add(fieldName, property.GetValue(aircraft, null));
                }

                result = connection.ExecuteScalar<long>($"INSERT INTO {_SchemaPrefix}[Aircraft] ({fieldNames}) VALUES ({parameters}); {_SqlReturnNewIdentity}", dynamicParameters);
                aircraft.AircraftID = (int)result;
            }

            return result;
        }

        protected static KineticAircraft CreateAircraft(string icao24 = "123456", string registration = "G-VRST")
        {
            return new KineticAircraft() {
                ModeS = icao24,
                Registration = registration,
            };
        }

        protected KineticAircraft PrepareAircraftReference(KineticAircraft aircraft)
        {
            aircraft ??= _DefaultAircraft;
            if(aircraft.AircraftID == 0) {
                AddAircraft(aircraft);
            }

            return aircraft;
        }

        protected KineticAircraft LoadAircraftFromSpreadsheetRow(SpreadsheetTestDataRow row, int firstOrdinal = 0, KineticAircraft copyIntoAircraft = null)
        {
            var result = copyIntoAircraft ?? new();

            int ordinal = firstOrdinal;
            result.AircraftClass =      row.EString(ordinal++);
            result.Country =            row.EString(ordinal++);
            result.DeRegDate =          row.EString(ordinal++);
            result.Engines =            row.EString(ordinal++);
            result.FirstCreated =       row.DateTime(ordinal++);
            result.GenericName =        row.EString(ordinal++);
            result.ICAOTypeCode =       row.EString(ordinal++);
            result.LastModified =       row.DateTime(ordinal++);
            result.Manufacturer =       row.EString(ordinal++);
            result.ModeS =              row.EString(ordinal++);
            result.ModeSCountry =       row.EString(ordinal++);
            result.OperatorFlagCode =   row.EString(ordinal++);
            result.OwnershipStatus =    row.EString(ordinal++);
            result.PopularName =        row.EString(ordinal++);
            result.PreviousID =         row.EString(ordinal++);
            result.RegisteredOwners =   row.EString(ordinal++);
            result.Registration =       row.EString(ordinal++);
            result.SerialNo =           row.EString(ordinal++);
            result.Status =             row.EString(ordinal++);
            result.Type =               row.EString(ordinal++);
            result.CofACategory =       row.EString(ordinal++);
            result.CofAExpiry =         row.EString(ordinal++);
            result.CurrentRegDate =     row.EString(ordinal++);
            result.FirstRegDate =       row.EString(ordinal++);
            result.InfoUrl =            row.EString(ordinal++);
            result.Interested =         row.Bool(ordinal++);
            result.MTOW =               row.EString(ordinal++);
            result.PictureUrl1 =        row.EString(ordinal++);
            result.PictureUrl2 =        row.EString(ordinal++);
            result.PictureUrl3 =        row.EString(ordinal++);
            result.TotalHours =         row.EString(ordinal++);
            result.UserNotes =          row.EString(ordinal++);
            result.UserString1 =        row.EString(ordinal++);
            result.UserString2 =        row.EString(ordinal++);
            result.UserString3 =        row.EString(ordinal++);
            result.UserString4 =        row.EString(ordinal++);
            result.UserString5 =        row.EString(ordinal++);
            result.UserBool1 =          row.Bool(ordinal++);
            result.UserBool2 =          row.Bool(ordinal++);
            result.UserBool3 =          row.Bool(ordinal++);
            result.UserBool4 =          row.Bool(ordinal++);
            result.UserBool5 =          row.Bool(ordinal++);
            result.UserInt1 =           row.Long(ordinal++);
            result.UserInt2 =           row.Long(ordinal++);
            result.UserInt3 =           row.Long(ordinal++);
            result.UserInt4 =           row.Long(ordinal++);
            result.UserInt5 =           row.Long(ordinal++);
            result.UserTag =            row.EString(ordinal++);
            result.YearBuilt =          row.EString(ordinal++);

            return result;
        }

        protected void AssertAircraftAreEqual(KineticAircraft expected, KineticAircraft actual, long id = -1L)
        {
            Assert.AreEqual(id == -1L ? expected.AircraftID : (int)id,  actual.AircraftID);
            Assert.AreEqual(expected.AircraftClass,                     actual.AircraftClass);
            Assert.AreEqual(expected.Country,                           actual.Country);
            Assert.AreEqual(expected.DeRegDate,                         actual.DeRegDate);
            Assert.AreEqual(expected.Engines,                           actual.Engines);
            Assert.AreEqual(expected.FirstCreated,                      actual.FirstCreated);
            Assert.AreEqual(expected.GenericName,                       actual.GenericName);
            Assert.AreEqual(expected.ICAOTypeCode,                      actual.ICAOTypeCode);
            Assert.AreEqual(expected.LastModified,                      actual.LastModified);
            Assert.AreEqual(expected.Manufacturer,                      actual.Manufacturer);
            Assert.AreEqual(expected.ModeS,                             actual.ModeS);
            Assert.AreEqual(expected.ModeSCountry,                      actual.ModeSCountry);
            Assert.AreEqual(expected.OperatorFlagCode,                  actual.OperatorFlagCode);
            Assert.AreEqual(expected.OwnershipStatus,                   actual.OwnershipStatus);
            Assert.AreEqual(expected.PopularName,                       actual.PopularName);
            Assert.AreEqual(expected.PreviousID,                        actual.PreviousID);
            Assert.AreEqual(expected.RegisteredOwners,                  actual.RegisteredOwners);
            Assert.AreEqual(expected.Registration,                      actual.Registration);
            Assert.AreEqual(expected.SerialNo,                          actual.SerialNo);
            Assert.AreEqual(expected.Status,                            actual.Status);
            Assert.AreEqual(expected.Type,                              actual.Type);
            Assert.AreEqual(expected.CofACategory,                      actual.CofACategory);
            Assert.AreEqual(expected.CofAExpiry,                        actual.CofAExpiry);
            Assert.AreEqual(expected.CurrentRegDate,                    actual.CurrentRegDate);
            Assert.AreEqual(expected.FirstRegDate,                      actual.FirstRegDate);
            Assert.AreEqual(expected.InfoUrl,                           actual.InfoUrl);
            Assert.AreEqual(expected.Interested,                        actual.Interested);
            Assert.AreEqual(expected.MTOW,                              actual.MTOW);
            Assert.AreEqual(expected.PictureUrl1,                       actual.PictureUrl1);
            Assert.AreEqual(expected.PictureUrl2,                       actual.PictureUrl2);
            Assert.AreEqual(expected.PictureUrl3,                       actual.PictureUrl3);
            Assert.AreEqual(expected.TotalHours,                        actual.TotalHours);
            Assert.AreEqual(expected.UserNotes,                         actual.UserNotes);
            Assert.AreEqual(expected.UserString1,                       actual.UserString1);
            Assert.AreEqual(expected.UserString2,                       actual.UserString2);
            Assert.AreEqual(expected.UserString3,                       actual.UserString3);
            Assert.AreEqual(expected.UserString4,                       actual.UserString4);
            Assert.AreEqual(expected.UserString5,                       actual.UserString5);
            Assert.AreEqual(expected.UserBool1,                         actual.UserBool1);
            Assert.AreEqual(expected.UserBool2,                         actual.UserBool2);
            Assert.AreEqual(expected.UserBool3,                         actual.UserBool3);
            Assert.AreEqual(expected.UserBool4,                         actual.UserBool4);
            Assert.AreEqual(expected.UserBool5,                         actual.UserBool5);
            Assert.AreEqual(expected.UserInt1,                          actual.UserInt1);
            Assert.AreEqual(expected.UserInt2,                          actual.UserInt2);
            Assert.AreEqual(expected.UserInt3,                          actual.UserInt3);
            Assert.AreEqual(expected.UserInt4,                          actual.UserInt4);
            Assert.AreEqual(expected.UserInt5,                          actual.UserInt5);
            Assert.AreEqual(expected.UserTag,                           actual.UserTag);
            Assert.AreEqual(expected.YearBuilt,                         actual.YearBuilt);
        }
        #endregion

        #region Flight Utility Methods
        /// <summary>
        /// Creates a flight object.
        /// </summary>
        /// <param name="actual"></param>
        /// <returns></returns>
        protected KineticFlight CreateFlight(KineticAircraft aircraft = null, string id = null)
        {
            var result = new KineticFlight() {
                Aircraft = new()
            };
            if(id != null) {
                result.Callsign = id;
            }
            if(aircraft != null) {
                result.Aircraft = aircraft;
            }
            if(result.Aircraft != null) {
                result.AircraftID = result.Aircraft.AircraftID;
            }

            return result;
        }

        /// <summary>
        /// Creates a set of flight and actual records and sets the unique identifiers on each to the ID string passed across.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="setCallsign"></param>
        /// <param name="setRegistration"></param>
        /// <returns></returns>
        protected KineticFlight CreateFlight(string id, bool setCallsign = true, bool setRegistration = true)
        {
            var result = CreateFlight();
            result.Aircraft.ModeS = id;
            if(setRegistration) {
                result.Aircraft.Registration = id;
            }
            if(setCallsign) {
                result.Callsign = id;
            }

            return result;
        }

        protected long AddFlight(KineticFlight flight, KineticSession session = null, KineticAircraft aircraft = null)
        {
            long result = 0;

            if(flight.SessionID == 0) {
                session = PrepareSessionReference(session);
            }
            if(flight.AircraftID == 0) {
                aircraft = PrepareAircraftReference(aircraft ?? flight.Aircraft);
            }

            using(var connection = _CreateConnection()) {
                connection.Open();

                var dynamicParameters = new DynamicParameters();
                var fieldNames = new StringBuilder();
                var parameters = new StringBuilder();

                foreach(var property in typeof(KineticFlight).GetProperties()) {
                    var fieldName = property.Name;
                    var value = property.GetValue(flight, null);

                    switch(fieldName) {
                        case nameof(KineticFlight.FlightID):
                        case nameof(KineticFlight.Aircraft):
                            continue;
                        case nameof(KineticFlight.AircraftID):
                            if(flight.AircraftID == 0) {
                                value = aircraft.AircraftID;
                            }
                            break;
                        case nameof(KineticFlight.SessionID):
                            if(flight.SessionID == 0) {
                                value = session.SessionID;
                            }
                            break;
                    }

                    if(fieldNames.Length > 0) {
                        fieldNames.Append(',');
                    }
                    if(parameters.Length > 0) {
                        parameters.Append(',');
                    }

                    fieldNames.Append($"[{fieldName}]");
                    parameters.Append($"@{fieldName}");
                    dynamicParameters.Add(fieldName, value);
                }

                result = connection.ExecuteScalar<long>($"INSERT INTO {_SchemaPrefix}[Flights] ({fieldNames}) VALUES ({parameters}); {_SqlReturnNewIdentity}", dynamicParameters);
                flight.FlightID = (int)result;
            }

            return result;
        }

        protected void AddFlightAndAircraft(KineticFlight flight)
        {
            flight.AircraftID = (int)AddAircraft(flight.Aircraft);
            AddFlight(flight);
        }

        protected KineticFlight PrepareFlightReference(KineticFlight flight)
        {
            if(flight == null) {
                flight = _DefaultFlight;
            }
            if(flight.SessionID == 0) {
                flight.SessionID = PrepareSessionReference(null).SessionID;
            }
            if(flight.AircraftID == 0) {
                flight.Aircraft = flight.Aircraft ?? _DefaultAircraft;
                flight.AircraftID = PrepareAircraftReference(flight.Aircraft).AircraftID;
            }
            if(flight.FlightID == 0) {
                AddFlight(flight);
            }

            return flight;
        }

        /// <summary>
        /// Returns a mock flight record with values filled from a spreadsheet row, starting from the column number passed across.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="firstOrdinal"></param>
        /// <returns></returns>
        /// <remarks>
        /// The columns are read in the following order:
        /// AircraftID, Callsign, EndTime, FirstAltitude, FirstGroundSpeed, FirstIsOnGround, FirstLat, FirstLon, FirstSquawk, FirstTrack, FirstVerticalRate, HadAlert,
        /// HadEmergency, HadSpi, LastAltitude, LastGroundSpeed, LastIsOnGround, LastLat, LastLon, LastSquawk, LastTrack, LastVerticalRate, NumADSBMsgRec, NumModeSMsgRec,
        /// NumIDMsgRec, NumSurPosMsgRec, NumAirPosMsgRec, NumAirVelMsgRec, NumSurAltMsgRec, NumSurIDMsgRec, NumAirToAirMsgRec, NumAirCallRepMsgRec, NumPosMsgRec
        /// and StartTime
        /// </remarks>
        protected KineticFlight LoadFlightFromSpreadsheetRow(SpreadsheetTestDataRow row, int firstOrdinal = 0, KineticFlight copyIntoFlight = null)
        {
            int ordinal = firstOrdinal;

            var aircraft = CreateAircraft();
            aircraft.AircraftID = row.Int(ordinal++);

            var result = copyIntoFlight ?? CreateFlight(aircraft) ;
            result.AircraftID =             aircraft.AircraftID;
            result.Callsign =               row.EString(ordinal++);
            result.EndTime =                row.DateTime(ordinal++);
            result.FirstAltitude =          row.Int(ordinal++);
            result.FirstGroundSpeed =       row.Float(ordinal++);
            result.FirstIsOnGround =        row.Bool(ordinal++);
            result.FirstLat =               row.Float(ordinal++);
            result.FirstLon =               row.Float(ordinal++);
            result.FirstSquawk =            row.Int(ordinal++);
            result.FirstTrack =             row.Float(ordinal++);
            result.FirstVerticalRate =      row.Int(ordinal++);
            result.HadAlert =               row.Bool(ordinal++);
            result.HadEmergency =           row.Bool(ordinal++);
            result.HadSpi =                 row.Bool(ordinal++);
            result.LastAltitude =           row.Int(ordinal++);
            result.LastGroundSpeed =        row.Float(ordinal++);
            result.LastIsOnGround =         row.Bool(ordinal++);
            result.LastLat =                row.Float(ordinal++);
            result.LastLon =                row.Float(ordinal++);
            result.LastSquawk =             row.Int(ordinal++);
            result.LastTrack =              row.Float(ordinal++);
            result.LastVerticalRate =       row.Int(ordinal++);
            result.NumADSBMsgRec =          row.Int(ordinal++);
            result.NumModeSMsgRec =         row.Int(ordinal++);
            result.NumIDMsgRec =            row.Int(ordinal++);
            result.NumSurPosMsgRec =        row.Int(ordinal++);
            result.NumAirPosMsgRec =        row.Int(ordinal++);
            result.NumAirVelMsgRec =        row.Int(ordinal++);
            result.NumSurAltMsgRec =        row.Int(ordinal++);
            result.NumSurIDMsgRec =         row.Int(ordinal++);
            result.NumAirToAirMsgRec =      row.Int(ordinal++);
            result.NumAirCallRepMsgRec =    row.Int(ordinal++);
            result.NumPosMsgRec =           row.Int(ordinal++);
            result.StartTime =              row.DateTime(ordinal++);
            result.UserNotes =              row.EString(ordinal++);

            return result;
        }

        protected static void AssertFlightsAreEqual(KineticFlight expected, KineticFlight actual, bool expectAircraftFilled, int expectedAircraftId)
        {
            Assert.AreEqual(expectedAircraftId, actual.AircraftID);
            if(expectAircraftFilled) {
                Assert.AreEqual(expectedAircraftId, actual.Aircraft.AircraftID);
            } else {
                Assert.IsNull(actual.Aircraft);
            }

            Assert.AreEqual(expected.Callsign,              actual.Callsign);
            Assert.AreEqual(expected.EndTime,               actual.EndTime);
            Assert.AreEqual(expected.FirstAltitude,         actual.FirstAltitude);
            Assert.AreEqual(expected.FirstGroundSpeed,      actual.FirstGroundSpeed);
            Assert.AreEqual(expected.FirstIsOnGround,       actual.FirstIsOnGround);
            Assert.AreEqual(expected.FirstLat,              actual.FirstLat);
            Assert.AreEqual(expected.FirstLon,              actual.FirstLon);
            Assert.AreEqual(expected.FirstSquawk,           actual.FirstSquawk);
            Assert.AreEqual(expected.FirstTrack,            actual.FirstTrack);
            Assert.AreEqual(expected.FirstVerticalRate,     actual.FirstVerticalRate);
            Assert.AreEqual(expected.HadAlert,              actual.HadAlert);
            Assert.AreEqual(expected.HadEmergency,          actual.HadEmergency);
            Assert.AreEqual(expected.HadSpi,                actual.HadSpi);
            Assert.AreEqual(expected.LastAltitude,          actual.LastAltitude);
            Assert.AreEqual(expected.LastGroundSpeed,       actual.LastGroundSpeed);
            Assert.AreEqual(expected.LastIsOnGround,        actual.LastIsOnGround);
            Assert.AreEqual(expected.LastLat,               actual.LastLat);
            Assert.AreEqual(expected.LastLon,               actual.LastLon);
            Assert.AreEqual(expected.LastSquawk,            actual.LastSquawk);
            Assert.AreEqual(expected.LastTrack,             actual.LastTrack);
            Assert.AreEqual(expected.LastVerticalRate,      actual.LastVerticalRate);
            Assert.AreEqual(expected.NumADSBMsgRec,         actual.NumADSBMsgRec);
            Assert.AreEqual(expected.NumModeSMsgRec,        actual.NumModeSMsgRec);
            Assert.AreEqual(expected.NumIDMsgRec,           actual.NumIDMsgRec);
            Assert.AreEqual(expected.NumSurPosMsgRec,       actual.NumSurPosMsgRec);
            Assert.AreEqual(expected.NumAirPosMsgRec,       actual.NumAirPosMsgRec);
            Assert.AreEqual(expected.NumAirVelMsgRec,       actual.NumAirVelMsgRec);
            Assert.AreEqual(expected.NumSurAltMsgRec,       actual.NumSurAltMsgRec);
            Assert.AreEqual(expected.NumSurIDMsgRec,        actual.NumSurIDMsgRec);
            Assert.AreEqual(expected.NumAirToAirMsgRec,     actual.NumAirToAirMsgRec);
            Assert.AreEqual(expected.NumAirCallRepMsgRec,   actual.NumAirCallRepMsgRec);
            Assert.AreEqual(expected.NumPosMsgRec,          actual.NumPosMsgRec);
            Assert.AreEqual(expected.StartTime,             actual.StartTime);
            Assert.AreEqual(expected.SessionID,             actual.SessionID);
            Assert.AreEqual(expected.UserNotes,             actual.UserNotes);
        }
        #endregion

        #region DBHistory Utility Methods
        protected void ClearDBHistory()
        {
            using(var connection = _CreateConnection()) {
                connection.Open();
                connection.Execute($"DELETE FROM {_SchemaPrefix}[DBHistory]");
            }
        }

        protected long AddDBHistory(KineticDBHistory dbHistory)
        {
            long result = 0;

            using(var connection = _CreateConnection()) {
                connection.Open();

                result = connection.ExecuteScalar<long>($@"
                    INSERT INTO {_SchemaPrefix}[DBHistory] (
                        [TimeStamp]
                       ,[Description]
                    ) VALUES (
                        @TimeStamp
                       ,@Description
                    ); {_SqlReturnNewIdentity}", new {
                        dbHistory.TimeStamp,
                        dbHistory.Description,
                    }
                );
                dbHistory.DBHistoryID = (int)result;
            }

            return result;
        }
        #endregion

        #region DBInfo Utility Methods
        protected long AddDBInfo(KineticDBInfo dbInfo)
        {
            long result = 0;

            using(var connection = _CreateConnection()) {
                connection.Open();

                result = connection.ExecuteScalar<long>($@"
                    INSERT INTO {_SchemaPrefix}[DBInfo] (
                        [OriginalVersion]
                       ,[CurrentVersion]
                    ) VALUES (
                        @OriginalVersion
                       ,@CurrentVersion
                    );{_SqlReturnNewIdentity}", new {
                        dbInfo.OriginalVersion,
                        dbInfo.CurrentVersion,
                    }
                );
            }

            return result;
        }
        #endregion

        #region SystemEvents Utility Methods
        protected void ClearSystemEvents()
        {
            using(var connection = _CreateConnection()) {
                connection.Open();
                connection.Execute($"DELETE FROM {_SchemaPrefix}[SystemEvents]");
            }
        }

        protected long AddSystemEvent(KineticSystemEvents systemEvent)
        {
            long result = 0;

            using(var connection = _CreateConnection()) {
                connection.Open();

                result = connection.ExecuteScalar<long>($@"
                    INSERT INTO {_SchemaPrefix}[SystemEvents] (
                        [App]
                       ,[Msg]
                       ,[TimeStamp]
                    ) VALUES (
                        @App
                       ,@Msg
                       ,@TimeStamp
                    ); {_SqlReturnNewIdentity}", new {
                        systemEvent.App,
                        systemEvent.Msg,
                        systemEvent.TimeStamp,
                    }
                );
                systemEvent.SystemEventsID = (int)result;
            }

            return result;
        }
        #endregion

        #region Location Utility Methods
        protected void ClearLocations()
        {
            using(var connection = _CreateConnection()) {
                connection.Open();
                connection.Execute($"DELETE FROM {_SchemaPrefix}[Locations]");
            }
        }

        protected long AddLocation(KineticLocation location)
        {
            long result = 0;

            using(var connection = _CreateConnection()) {
                connection.Open();

                result = connection.ExecuteScalar<long>($@"
                    INSERT INTO {_SchemaPrefix}[Locations] (
                        [Altitude]
                       ,[Latitude]
                       ,[LocationName]
                       ,[Longitude]
                    ) VALUES (
                        @Altitude
                       ,@Latitude
                       ,@LocationName
                       ,@Longitude
                    ); {_SqlReturnNewIdentity}", new {
                        location.Altitude,
                        location.Latitude,
                        location.LocationName,
                        location.Longitude,
                    }
                );
                location.LocationID = (int)result;
            }

            return result;
        }

        protected KineticLocation PrepareLocationReference(KineticLocation location)
        {
            location ??= _DefaultLocation;
            if(location.LocationID == 0) {
                AddLocation(location);
            }

            return location;
        }
        #endregion

        #region Session Utility Methods
        protected long AddSession(KineticSession session)
        {
            long result = 0;

            using(var connection = _CreateConnection()) {
                connection.Open();

                result = connection.ExecuteScalar<long>($@"
                    INSERT INTO {_SchemaPrefix}[Sessions] (
                        [LocationID]
                       ,[StartTime]
                       ,[EndTime]
                    ) VALUES (
                        @LocationID
                       ,@StartTime
                       ,@EndTime
                    ); {_SqlReturnNewIdentity}", new {
                        session.LocationID,
                        session.StartTime,
                        session.EndTime,
                    }
                );
                session.SessionID = (int)result;
            }

            return result;
        }

        protected KineticSession PrepareSessionReference(KineticSession session)
        {
            session ??= _DefaultSession;
            if(session.LocationID == 0) {
                session.LocationID = PrepareLocationReference(null).LocationID;
            }
            if(session.SessionID == 0) {
                AddSession(session);
            }

            return session;
        }
        #endregion

        #region GetAircraftByRegistration
        protected void Common_GetAircraftByRegistration_Returns_Null_If_Passed_Null()
        {
            Assert.IsNull(_Implementation.GetAircraftByRegistration(null));
        }

        protected void Common_GetAircraftByRegistration_Returns_Null_If_Aircraft_Does_Not_Exist()
        {
            Assert.IsNull(_Implementation.GetAircraftByRegistration("REG"));
        }

        protected void Common_GetAircraftByRegistration_Returns_Aircraft_Object_For_Registration()
        {
            var spreadsheet = new SpreadsheetTestData(TestData.BaseStationDatabaseTests_xslx, "GetAircraftBy");
            spreadsheet.TestEveryRow(this, row => {
                var mockAircraft = LoadAircraftFromSpreadsheetRow(row);

                var id = AddAircraft(mockAircraft);

                var aircraft = _Implementation.GetAircraftByRegistration(mockAircraft.Registration);
                Assert.AreNotSame(aircraft, mockAircraft);

                AssertAircraftAreEqual(mockAircraft, aircraft, id);
            });
        }
        #endregion

        #region GetAircraftByCode
        protected void Common_GetAircraftByCode_Returns_Null_If_Passed_Null()
        {
            Assert.IsNull(_Implementation.GetAircraftByCode(null));
        }

        protected void Common_GetAircraftByCode_Returns_Null_If_Aircraft_Does_Not_Exist()
        {
            Assert.IsNull(_Implementation.GetAircraftByCode("ABC123"));
        }

        protected void Common_GetAircraftByCode_Returns_Aircraft_Object_For_ICAO24_Code()
        {
            var spreadsheet = new SpreadsheetTestData(TestData.BaseStationDatabaseTests_xslx, "GetAircraftBy");
            spreadsheet.TestEveryRow(this, row => {
                var mockAircraft = LoadAircraftFromSpreadsheetRow(row);
                var id = AddAircraft(mockAircraft);

                var aircraft = _Implementation.GetAircraftByCode(mockAircraft.ModeS);

                Assert.AreNotSame(aircraft, mockAircraft);
                AssertAircraftAreEqual(mockAircraft, aircraft, id);
            });
        }
        #endregion

        #region GetManyAircraftByCode
        protected void Common_GetManyAircraftByCode_Returns_Empty_Collection_If_Passed_Null()
        {
            Assert.AreEqual(0, _Implementation.GetManyAircraftByCode(null).Count);
        }

        protected void Common_GetManyAircraftByCode_Returns_Empty_Collection_If_Aircraft_Does_Not_Exist()
        {
            Assert.AreEqual(0, _Implementation.GetManyAircraftByCode(new string[] { "ABC123" }).Count);
        }

        protected void Common_GetManyAircraftByCode_Returns_Aircraft_Object_For_ICAO24_Code()
        {
            var spreadsheet = new SpreadsheetTestData(TestData.BaseStationDatabaseTests_xslx, "GetAircraftBy");
            spreadsheet.TestEveryRow(this, row => {
                var mockAircraft = LoadAircraftFromSpreadsheetRow(row);
                var id = AddAircraft(mockAircraft);

                var manyAircraft = _Implementation.GetManyAircraftByCode(new string[] { mockAircraft.ModeS });
                Assert.AreEqual(1, manyAircraft.Count);

                var aircraft = manyAircraft.First().Value;
                Assert.AreNotSame(aircraft, mockAircraft);

                AssertAircraftAreEqual(mockAircraft, aircraft, id);
            });
        }

        protected void Common_GetManyAircraftByCode_Can_Return_More_Than_One_Aircraft()
        {
            var flight1 = CreateFlight("ABC123", setRegistration: true);
            var flight2 = CreateFlight("EFG456", setRegistration: true);
            var flight3 = CreateFlight("XYZ789", setRegistration: true);

            AddFlight(flight1);
            AddFlight(flight2);
            AddFlight(flight3);

            var firstAndLast = _Implementation.GetManyAircraftByCode(new string[] { "ABC123", "XYZ789" });

            Assert.AreEqual(2, firstAndLast.Count);
            Assert.IsTrue(firstAndLast.Where(r => r.Value.Registration == "ABC123").Any());
            Assert.IsTrue(firstAndLast.Where(r => r.Value.Registration == "XYZ789").Any());
        }

        protected void Common_GetManyAircraftByCode_Transparently_Handles_Call_Splitting_When_Number_Of_Icaos_Exceeds_MaxParameters()
        {
            var flight1 = CreateFlight("ABC123", setRegistration: true);
            var flight2 = CreateFlight("XYZ789", setRegistration: true);

            AddAircraft(flight1.Aircraft);
            AddAircraft(flight2.Aircraft);

            AddFlight(flight1);
            AddFlight(flight2);

            var icaos = new string[_Implementation.MaxParameters + 1];
            Array.Fill(icaos, "");
            icaos[0] =  "ABC123";
            icaos[^1] = "XYZ789";

            var allAircraft = _Implementation.GetManyAircraftByCode(icaos);

            Assert.AreEqual(2, allAircraft.Count);
            Assert.IsNotNull(allAircraft["ABC123"]);
            Assert.IsNotNull(allAircraft["XYZ789"]);
        }
        #endregion

        #region GetManyAircraftAndFlightsCountByCode
        protected void Common_GetManyAircraftAndFlightsCountByCode_Returns_Empty_Collection_If_Passed_Null()
        {
            Assert.AreEqual(0, _Implementation.GetManyAircraftAndFlightsCountByCode(null).Count);
        }

        protected void Common_GetManyAircraftAndFlightsCountByCode_Returns_Empty_Collection_If_Aircraft_Does_Not_Exist()
        {
            Assert.AreEqual(0, _Implementation.GetManyAircraftAndFlightsCountByCode(new string[] { "ABC123" }).Count);
        }

        protected void Common_GetManyAircraftAndFlightsCountByCode_Returns_Aircraft_Object_For_ICAO24_Code()
        {
            var spreadsheet = new SpreadsheetTestData(TestData.BaseStationDatabaseTests_xslx, "GetAircraftBy");
            spreadsheet.TestEveryRow(this, row => {
                var mockAircraft = LoadAircraftFromSpreadsheetRow(row);

                var id = AddAircraft(mockAircraft);

                var manyAircraft = _Implementation.GetManyAircraftAndFlightsCountByCode(new string[] { mockAircraft.ModeS });
                Assert.AreEqual(1, manyAircraft.Count);

                var aircraft = manyAircraft.First().Value;
                Assert.AreNotSame(aircraft, mockAircraft);

                AssertAircraftAreEqual(mockAircraft, aircraft, id);
            });
        }

        protected void Common_GetManyAircraftAndFlightsCountByCode_Can_Return_More_Than_One_Aircraft()
        {
            var flight1 = CreateFlight("ABC123", setRegistration: true);
            var flight2 = CreateFlight("EFG456", setRegistration: true);
            var flight3 = CreateFlight("XYZ789", setRegistration: true);

            AddAircraft(flight1.Aircraft);
            AddAircraft(flight2.Aircraft);
            AddAircraft(flight3.Aircraft);

            AddFlight(flight1);
            AddFlight(flight2);
            AddFlight(flight3);

            var firstAndLast = _Implementation.GetManyAircraftAndFlightsCountByCode(new string[] { "ABC123", "XYZ789" });

            Assert.AreEqual(2, firstAndLast.Count);
            Assert.IsTrue(firstAndLast.Where(r => r.Value.Registration == "ABC123").Any());
            Assert.IsTrue(firstAndLast.Where(r => r.Value.Registration == "XYZ789").Any());
        }

        protected void Common_GetManyAircraftAndFlightsCountByCode_Returns_Counts_Of_Flights()
        {
            var flight1 = CreateFlight("ABC123", setRegistration: true);
            flight1.AircraftID = (int)AddAircraft(flight1.Aircraft);
            var flight2 = CreateFlight(flight1.Aircraft, "XYZ999");

            AddFlight(flight1);
            AddFlight(flight2);

            var allAircraft = _Implementation.GetManyAircraftAndFlightsCountByCode(new string[] { "ABC123" });

            Assert.AreEqual(1, allAircraft.Count);
            Assert.AreEqual(2, allAircraft.First().Value.FlightsCount);
        }

        protected void Common_GetManyAircraftAndFlightsCountByCode_Transparently_Handles_Call_Splitting_When_Number_Of_Icaos_Exceeds_MaxParameters()
        {
            var flight1 = CreateFlight("ABC123", setRegistration: true);
            var flight2 = CreateFlight("XYZ789", setRegistration: true);

            AddAircraft(flight1.Aircraft);
            AddAircraft(flight2.Aircraft);

            AddFlight(flight1);
            AddFlight(flight2);

            var icaos = new string[_Implementation.MaxParameters + 1];
            icaos[0] = "ABC123";
            icaos[icaos.Length - 1] = "XYZ789";

            var allAircraft = _Implementation.GetManyAircraftAndFlightsCountByCode(icaos);

            Assert.AreEqual(2, allAircraft.Count);
            Assert.IsNotNull(allAircraft["ABC123"]);
            Assert.IsNotNull(allAircraft["XYZ789"]);
        }
        #endregion

        #region GetAircraftById
        protected void Common_GetAircraftById_Returns_Null_If_Aircraft_Does_Not_Exist()
        {
            Assert.IsNull(_Implementation.GetAircraftById(1));
        }

        protected void Common_GetAircraftById_Returns_Aircraft_Object_For_Record_Identifier()
        {
            var spreadsheet = new SpreadsheetTestData(TestData.BaseStationDatabaseTests_xslx, "GetAircraftBy");
            spreadsheet.TestEveryRow(this, row => {
                var mockAircraft = LoadAircraftFromSpreadsheetRow(row);
                var id = (int)AddAircraft(mockAircraft);

                var aircraft = _Implementation.GetAircraftById(id);

                Assert.AreNotSame(aircraft, mockAircraft);
                AssertAircraftAreEqual(mockAircraft, aircraft, id);
            });
        }
        #endregion

        #region InsertAircraft
        protected void Common_InsertAircraft_Throws_If_Writes_Disabled()
        {
            _Implementation.InsertAircraft(new() { ModeS = "123456" });
        }

        protected void Common_InsertAircraft_Correctly_Inserts_Record()
        {
            var spreadsheet = new SpreadsheetTestData(TestData.BaseStationDatabaseTests_xslx, "GetAircraftBy");
            spreadsheet.TestEveryRow(this, row => {
                var aircraft = LoadAircraftFromSpreadsheetRow(row);
                _Implementation.WriteSupportEnabled = true;
                _Implementation.InsertAircraft(aircraft);
                Assert.AreNotEqual(0, aircraft.AircraftID);

                var readBack = _Implementation.GetAircraftById(aircraft.AircraftID);

                AssertAircraftAreEqual(aircraft, readBack);
            });
        }

        protected void Common_InsertAircraft_Works_For_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    try {
                        Common_InsertAircraft_Correctly_Inserts_Record();
                    } catch(Exception ex) {
                        throw new InvalidOperationException($"Exception thrown when culture was {culture}", ex);
                    }
                }
            }
        }
        #endregion
    }
}
