using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
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
        protected MockClock _Clock;
        protected MockStandingDataManager _StandingData;
        protected EventRecorder<EventArgs<KineticAircraft>> _AircraftUpdatedEvent;
        protected SearchBaseStationCriteria _Criteria;

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
            _Clock = new();
            _StandingData = new();

            _AircraftUpdatedEvent = new EventRecorder<EventArgs<KineticAircraft>>();

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

            _Criteria = new SearchBaseStationCriteria() {
                Date =          new FilterRange<DateTime>(DateTime.MinValue, DateTime.MaxValue),
                FirstAltitude = new FilterRange<int>(int.MinValue, int.MaxValue),
                LastAltitude =  new FilterRange<int>(int.MinValue, int.MaxValue),
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

        #region GetOrInsertAircraftByCode
        protected void Common_GetOrInsertAircraftByCode_Throws_If_Writes_Disabled()
        {
            _Implementation.GetOrInsertAircraftByCode("123456", out var created);
        }

        protected void Common_GetOrInsertAircraftByCode_Returns_Record_If_It_Exists()
        {
            _Implementation.WriteSupportEnabled = true;
            _Implementation.InsertAircraft(new() { ModeS = "123456" });

            var result = _Implementation.GetOrInsertAircraftByCode("123456", out var created);

            Assert.AreNotEqual(0, result.AircraftID);
            Assert.AreEqual("123456", result.ModeS);
            Assert.AreEqual(false, created);
        }

        protected void Common_GetOrInsertAircraftByCode_Correctly_Inserts_Record()
        {
            _Implementation.WriteSupportEnabled = true;

            _StandingData.AllCodeBlocksByIcao24.Add("Abc123", new() {
                Country = "Foovania",
            });

            var aircraft = _Implementation.GetOrInsertAircraftByCode("Abc123", out bool created);
            Assert.AreNotEqual(0, aircraft.AircraftID);

            var readBack = _Implementation.GetAircraftById(aircraft.AircraftID);
            AssertAircraftAreEqual(new() {
                AircraftID =    aircraft.AircraftID,
                FirstCreated =  _Clock.Object.Now.DateTime,
                LastModified =  _Clock.Object.Now.DateTime,
                ModeS =         "Abc123",
                ModeSCountry =  "Foovania",
            }, readBack);
            Assert.AreEqual(true, created);
        }

        protected void Common_GetOrInsertAircraftByCode_Deals_With_Null_CodeBlock()
        {
            _StandingData.AllCodeBlocksByIcao24.Add("abc123", null);
            _Implementation.WriteSupportEnabled = true;

            var aircraft = _Implementation.GetOrInsertAircraftByCode("abc123", out bool created);

            Assert.IsNull(aircraft.ModeSCountry);
        }

        protected void Common_GetOrInsertAircraftByCode_Deals_With_Null_Country()
        {
            _Implementation.WriteSupportEnabled = true;
            var aircraft = _Implementation.GetOrInsertAircraftByCode("abc123", out bool created);
            Assert.IsNull(aircraft.ModeSCountry);
        }

        protected void Common_GetOrInsertAircraftByCode_Deals_With_Unknown_Country()
        {
            _StandingData.AllCodeBlocksByIcao24.Add("abc123", new() { Country = "Unknown Country", });

            _Implementation.WriteSupportEnabled = true;
            var aircraft = _Implementation.GetOrInsertAircraftByCode("abc123", out bool created);
            Assert.IsNull(aircraft.ModeSCountry);
        }

        protected void Common_GetOrInsertAircraftByCode_Truncates_Milliseconds_From_Date()
        {
            _Implementation.WriteSupportEnabled = true;

            var time = new DateTime(2001, 2, 3, 4, 5, 6, 789);
            _Clock.Now = time;

            _Implementation.GetOrInsertAircraftByCode("X", out var created);
            var readBack = _Implementation.GetAircraftByCode("X");
            Assert.AreEqual(time, readBack.FirstCreated);
            Assert.AreEqual(time, readBack.LastModified);
        }
        #endregion

        #region UpdateAircraft
        protected void Common_UpdateAircraft_Throws_If_Writes_Disabled()
        {
            var aircraft = new KineticAircraft() { ModeS = "X", };
            aircraft.AircraftID = (int)AddAircraft(aircraft);

            aircraft.Registration = "C";
            _Implementation.UpdateAircraft(aircraft);
        }

        protected void Common_UpdateAircraft_Raises_AircraftUpdated()
        {
            _Implementation.WriteSupportEnabled = true;

            var aircraft = new KineticAircraft() { ModeS = "X" };

            _Implementation.AircraftUpdated += _AircraftUpdatedEvent.Handler;
            _AircraftUpdatedEvent.EventRaised += (sender, args) => {
                Assert.AreSame(aircraft, args.Value);
            };

            _Implementation.InsertAircraft(aircraft);
            Assert.AreEqual(0, _AircraftUpdatedEvent.CallCount);

            _Implementation.UpdateAircraft(aircraft);
            Assert.AreEqual(1, _AircraftUpdatedEvent.CallCount);
            Assert.AreSame(_Implementation, _AircraftUpdatedEvent.Sender);
        }

        protected void Common_UpdateAircraft_Correctly_Updates_Record()
        {
            var spreadsheet = new SpreadsheetTestData(TestData.BaseStationDatabaseTests_xslx, "GetAircraftBy");
            spreadsheet.TestEveryRow(this, row => {
                _Implementation.WriteSupportEnabled = true;
                var id = (int)AddAircraft(new() { ModeS = "ZZZZZZ" });

                var update = _Implementation.GetAircraftById(id);
                LoadAircraftFromSpreadsheetRow(row, 0, update);

                _Implementation.UpdateAircraft(update);

                var readBack = _Implementation.GetAircraftById(id);
                AssertAircraftAreEqual(update, readBack, id);
            });
        }

        protected void Common_UpdateAircraft_Works_For_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    RunTestCleanup();
                    RunTestInitialise();

                    try {
                        Common_UpdateAircraft_Correctly_Updates_Record();
                    } catch(Exception ex) {
                        throw new InvalidOperationException($"Exception thrown when culture was {culture}", ex);
                    }
                }
            }
        }
        #endregion

        #region UpdateAircraftModeSCountry
        protected void Common_UpdateAircraftModeSCountry_Throws_If_Writes_Disabled()
        {
            var aircraft = new KineticAircraft() { ModeS = "X" };
            aircraft.AircraftID = (int)AddAircraft(aircraft);

            aircraft.Registration = "C";
            _Implementation.UpdateAircraftModeSCountry(aircraft.AircraftID, "X");
        }

        protected void Common_UpdateAircraftModeSCountry_Updates_ModeSCountry_For_Existing_Record()
        {
            var spreadsheet = new SpreadsheetTestData(TestData.BaseStationDatabaseTests_xslx, "GetAircraftBy");
            spreadsheet.TestEveryRow(this, row => {
                _Implementation.WriteSupportEnabled = true;
                var id = (int)AddAircraft(new() { ModeS = "ZZZZZZ" });

                var update = _Implementation.GetAircraftById(id);
                LoadAircraftFromSpreadsheetRow(row, 0, update);

                _Implementation.UpdateAircraft(update);

                update.ModeSCountry = "Updated Mode-S Country";
                _Implementation.UpdateAircraftModeSCountry(update.AircraftID, update.ModeSCountry);

                var readBack = _Implementation.GetAircraftById(id);
                AssertAircraftAreEqual(update, readBack, id);

                update.ModeSCountry = null;
                _Implementation.UpdateAircraftModeSCountry(update.AircraftID, update.ModeSCountry);

                readBack = _Implementation.GetAircraftById(id);
                AssertAircraftAreEqual(update, readBack, id);
            });
        }
        #endregion

        #region RecordMissingAircraft
        protected void Common_RecordMissingAircraft_Throws_Exception_If_Writes_Not_Enabled()
        {
            _Implementation.RecordMissingAircraft("123456");
        }

        protected void Common_RecordMissingAircraft_Creates_Almost_Empty_Aircraft_Record()
        {
            _Implementation.WriteSupportEnabled = true;

            _Implementation.RecordMissingAircraft("123456");

            var aircraft = _Implementation.GetAircraftByCode("123456");
            Assert.IsNotNull(aircraft);
            Assert.AreEqual("123456", aircraft.ModeS);
            Assert.AreEqual("Missing", aircraft.UserString1);
            Assert.AreEqual(_Clock.Now.DateTime, aircraft.FirstCreated);
            Assert.AreEqual(_Clock.Now.DateTime, aircraft.LastModified);
            Assert.IsNull(aircraft.Registration);
            Assert.IsNull(aircraft.Manufacturer);
            Assert.IsNull(aircraft.Type);
            Assert.IsNull(aircraft.RegisteredOwners);
        }

        protected void Common_RecordMissingAircraft_Updates_Existing_Empty_Records()
        {
            _Implementation.WriteSupportEnabled = true;
            _Implementation.RecordMissingAircraft("123456");

            var createdDate = _Clock.Now;
            _Clock.Now = _Clock.Now.AddMinutes(1);
            _Implementation.RecordMissingAircraft("123456");

            var aircraft = _Implementation.GetAircraftByCode("123456");
            Assert.AreEqual(createdDate, aircraft.FirstCreated);
            Assert.AreEqual(_Clock.Now,  aircraft.LastModified);
        }

        protected void Common_RecordMissingAircraft_Updates_Existing_Empty_Records_With_Wrong_UserString1()
        {
            _Implementation.WriteSupportEnabled = true;
            _Implementation.InsertAircraft(new () { ModeS = "123456", FirstCreated = _Clock.Now.DateTime, LastModified = _Clock.Now.DateTime, });

            var createdDate = _Clock.Now.DateTime;
            _Clock.Now = _Clock.Now.AddMinutes(1);
            _Implementation.RecordMissingAircraft("123456");

            var aircraft = _Implementation.GetAircraftByCode("123456");
            Assert.AreEqual("Missing",      aircraft.UserString1);
            Assert.AreEqual(createdDate,    aircraft.FirstCreated);
            Assert.AreEqual(_Clock.Now,     aircraft.LastModified);
        }

        protected void Common_RecordMissingAircraft_Only_Updates_Time_On_Existing_Records_With_Values()
        {
            foreach(var property in new String[] { "Registration", "Manufacturer", "Model", "Operator" }) {
                RunTestCleanup();
                RunTestInitialise();

                _Implementation.WriteSupportEnabled = true;
                var aircraft = new KineticAircraft() { ModeS = "123456", UserString1 = "something", FirstCreated = _Clock.Now.DateTime, LastModified = _Clock.Now.DateTime, };
                switch(property) {
                    case "Registration":    aircraft.Registration = "A"; break;
                    case "Manufacturer":    aircraft.Manufacturer = "A"; break;
                    case "Model":           aircraft.Type = "A"; break;
                    case "Operator":        aircraft.RegisteredOwners = "A"; break;
                    default:                throw new NotImplementedException();
                }
                _Implementation.InsertAircraft(aircraft);

                var createdDate = _Clock.Now.DateTime;
                _Clock.Now = _Clock.Now.AddMinutes(1);
                _Implementation.RecordMissingAircraft("123456");

                aircraft = _Implementation.GetAircraftByCode("123456");
                Assert.AreEqual("something",    aircraft.UserString1);
                Assert.AreEqual(createdDate,    aircraft.FirstCreated);
                Assert.AreEqual(_Clock.Now,     aircraft.LastModified);
            }
        }
        #endregion

        #region RecordManyMissingAircraft
        protected void Common_RecordManyMissingAircraft_Throws_Exception_If_Writes_Not_Enabled()
        {
            _Implementation.RecordManyMissingAircraft(new string[] { "A", "B" });
        }

        protected void Common_RecordManyMissingAircraft_Creates_Almost_Empty_Aircraft_Record()
        {
            _Implementation.WriteSupportEnabled = true;

            _Implementation.RecordManyMissingAircraft(new string[] { "A", "B" });

            var aircraft = _Implementation.GetAircraftByCode("A");
            Assert.IsNotNull(aircraft);
            Assert.AreEqual("A",        aircraft.ModeS);
            Assert.AreEqual("Missing",  aircraft.UserString1);
            Assert.AreEqual(_Clock.Now, aircraft.FirstCreated);
            Assert.AreEqual(_Clock.Now, aircraft.LastModified);

            aircraft = _Implementation.GetAircraftByCode("B");
            Assert.IsNotNull(aircraft);
            Assert.AreEqual("B",        aircraft.ModeS);
            Assert.AreEqual("Missing",  aircraft.UserString1);
            Assert.AreEqual(_Clock.Now, aircraft.FirstCreated);
            Assert.AreEqual(_Clock.Now, aircraft.LastModified);
        }

        protected void Common_RecordManyMissingAircraft_Updates_Existing_Empty_Records()
        {
            _Implementation.WriteSupportEnabled = true;
            _Implementation.RecordManyMissingAircraft(new string[] { "123456" });

            var createdDate = _Clock.Now;
            _Clock.Now = _Clock.Now.AddMinutes(1);
            _Implementation.RecordManyMissingAircraft(new string[] { "123456" });

            var aircraft = _Implementation.GetAircraftByCode("123456");
            Assert.AreEqual(createdDate,  aircraft.FirstCreated);
            Assert.AreEqual(_Clock.Now,   aircraft.LastModified);
        }

        protected void Common_RecordManyMissingAircraft_Only_Updates_LastModified_Time_On_Existing_Records_With_Registrations()
        {
            _Implementation.WriteSupportEnabled = true;
            _Implementation.InsertAircraft(new() { ModeS = "123456", Registration = "A", FirstCreated = _Clock.Now.LocalDateTime, LastModified = _Clock.Now.LocalDateTime });

            var createdDate = _Clock.Now;
            _Clock.Now = _Clock.Now.AddMinutes(1);
            _Implementation.RecordManyMissingAircraft(new string[] { "123456" });

            var aircraft = _Implementation.GetAircraftByCode("123456");
            Assert.AreEqual("A",            aircraft.Registration);
            Assert.AreEqual(createdDate,    aircraft.FirstCreated);
            Assert.AreEqual(_Clock.Now,     aircraft.LastModified);
        }
        #endregion

        #region UpsertAircraftLookup
        protected void Common_UpsertAircraftLookup_Throws_Exception_If_Writes_Not_Enabled()
        {
            _Implementation.UpsertAircraftLookup(new() { ModeS = "123456", }, false);
        }

        protected void Common_UpsertAircraftLookup_Inserts_New_Lookups()
        {
            _Implementation.WriteSupportEnabled = true;

            var now = new DateTime(2001, 2, 3, 4, 5, 6, 789);
            _Implementation.UpsertAircraftLookup(new() {
                ModeS =             "123456",
                Country =           "UK",
                ICAOTypeCode =      "A380",
                LastModified =      now,
                Manufacturer =      "Airbus",
                ModeSCountry =      "France",
                OperatorFlagCode =  "TWA",
                RegisteredOwners =  "Transworld",
                Registration =      "G-ABCD",
                SerialNo =          "9182",
                Type =              "Big Plane",
                YearBuilt =         "1992",
            }, false);

            var aircraft = _Implementation.GetAircraftByCode("123456");
            Assert.IsNotNull(aircraft);

            Assert.AreEqual("123456",       aircraft.ModeS);
            Assert.AreEqual("UK",           aircraft.Country);
            Assert.AreEqual("A380",         aircraft.ICAOTypeCode);
            Assert.AreEqual(now,            aircraft.FirstCreated);
            Assert.AreEqual(now,            aircraft.LastModified);
            Assert.AreEqual("Airbus",       aircraft.Manufacturer);
            Assert.AreEqual("France",       aircraft.ModeSCountry);
            Assert.AreEqual("TWA",          aircraft.OperatorFlagCode);
            Assert.AreEqual("Transworld",   aircraft.RegisteredOwners);
            Assert.AreEqual("G-ABCD",       aircraft.Registration);
            Assert.AreEqual("9182",         aircraft.SerialNo);
            Assert.AreEqual("Big Plane",    aircraft.Type);
            Assert.AreEqual("1992",         aircraft.YearBuilt);
        }

        protected void Common_UpsertAircraftLookup_Updates_Existing_Aircraft()
        {
            var createdDate = new DateTime(1999, 8, 7, 6, 5, 4, 321);
            var now = new DateTime(2001, 2, 3, 4, 5, 6, 789);

            _Implementation.WriteSupportEnabled = true;
            _Implementation.InsertAircraft(new() {
                ModeS =         "123456",
                Registration =  "N12345",
                FirstCreated =  createdDate,
                LastModified =  createdDate,
            });

            _Implementation.UpsertAircraftLookup(new() {
                ModeS =             "123456",
                Country =           "Germany",
                ICAOTypeCode =      "B747",
                LastModified =      now,
                Manufacturer =      "Boeing",
                ModeSCountry =      "USA",
                OperatorFlagCode =  "BAW",
                RegisteredOwners =  "British Airways",
                Registration =      "D-WXYZ",
                SerialNo =          "00119",
                Type =              "Big Jobs",
                YearBuilt =         "1979",
            }, false);

            var aircraft = _Implementation.GetAircraftByCode("123456");
            Assert.IsNotNull(aircraft);

            Assert.AreEqual("123456",           aircraft.ModeS);
            Assert.AreEqual("Germany",          aircraft.Country);
            Assert.AreEqual("B747",             aircraft.ICAOTypeCode);
            Assert.AreEqual(createdDate,        aircraft.FirstCreated);
            Assert.AreEqual(now,                aircraft.LastModified);
            Assert.AreEqual("Boeing",           aircraft.Manufacturer);
            Assert.AreEqual("USA",              aircraft.ModeSCountry);
            Assert.AreEqual("BAW",              aircraft.OperatorFlagCode);
            Assert.AreEqual("British Airways",  aircraft.RegisteredOwners);
            Assert.AreEqual("D-WXYZ",           aircraft.Registration);
            Assert.AreEqual("00119",            aircraft.SerialNo);
            Assert.AreEqual("Big Jobs",         aircraft.Type);
            Assert.AreEqual("1979",             aircraft.YearBuilt);
        }

        protected void Common_UpsertAircraftLookup_Can_Ignore_Existing_Aircraft_When_Required()
        {
            var createdDate = new DateTime(1999, 8, 7, 6, 5, 4, 321);
            var now = new DateTime(2001, 2, 3, 4, 5, 6, 789);

            _Implementation.WriteSupportEnabled = true;
            _Implementation.InsertAircraft(new() {
                ModeS =             "123456",
                Registration =      "G-ABCD",
                FirstCreated =      createdDate,
                LastModified =      createdDate,
            });

            _Implementation.UpsertAircraftLookup(new() {
                ModeS =             "123456",
                Country =           "Germany",
                ICAOTypeCode =      "B747",
                LastModified =      now,
                Manufacturer =      "Boeing",
                ModeSCountry =      "USA",
                OperatorFlagCode =  "BAW",
                RegisteredOwners =  "British Airways",
                Registration =      "D-WXYZ",
                SerialNo =          "00119",
                Type =              "Big Jobs",
                YearBuilt =         "1979",
            }, onlyUpdateIfMarkedAsMissing: true);

            var aircraft = _Implementation.GetAircraftByCode("123456");
            Assert.IsNotNull(aircraft);

            Assert.AreEqual("123456",       aircraft.ModeS);
            Assert.IsNull(                  aircraft.Country);
            Assert.IsNull(                  aircraft.ICAOTypeCode);
            Assert.AreEqual(createdDate,    aircraft.FirstCreated);
            Assert.AreEqual(createdDate,    aircraft.LastModified);
            Assert.IsNull(                  aircraft.Manufacturer);
            Assert.IsNull(                  aircraft.ModeSCountry);
            Assert.IsNull(                  aircraft.OperatorFlagCode);
            Assert.IsNull(                  aircraft.RegisteredOwners);
            Assert.AreEqual("G-ABCD",       aircraft.Registration);
            Assert.IsNull(                  aircraft.SerialNo);
            Assert.IsNull(                  aircraft.Type);
            Assert.IsNull(                  aircraft.YearBuilt);
        }

        protected void Common_UpsertAircraftLookup_Will_Overwrite_Missing_Aircraft_When_Required()
        {
            _Implementation.WriteSupportEnabled = true;
            _Implementation.InsertAircraft(new() {
                ModeS =             "123456",
                UserString1 =       "Missing",
            });

            _Implementation.UpsertAircraftLookup(new() {
                ModeS =             "123456",
                Registration =      "D-WXYZ",
            }, onlyUpdateIfMarkedAsMissing: true);

            var aircraft = _Implementation.GetAircraftByCode("123456");
            Assert.IsNotNull(aircraft);

            Assert.AreEqual("123456", aircraft.ModeS);
            Assert.AreEqual("D-WXYZ", aircraft.Registration);
        }

        protected void Common_UpsertAircraftLookup_Will_Not_Consider_Aircraft_Missing_If_They_Have_Registration()
        {
            _Implementation.WriteSupportEnabled = true;
            _Implementation.InsertAircraft(new() {
                ModeS =             "123456",
                Registration =      "G-ABCD",
                UserString1 =       "Missing",
            });

            _Implementation.UpsertAircraftLookup(new() {
                ModeS =             "123456",
                Registration =      "D-WXYZ",
            }, onlyUpdateIfMarkedAsMissing: true);

            var aircraft = _Implementation.GetAircraftByCode("123456");
            Assert.IsNotNull(aircraft);

            Assert.AreEqual("123456", aircraft.ModeS);
            Assert.AreEqual("G-ABCD", aircraft.Registration);
        }

        protected void Common_UpsertAircraftLookup_Raises_AircraftUpdated_On_Update()
        {
            _Implementation.WriteSupportEnabled = true;
            _Implementation.InsertAircraft(new() { ModeS = "123456", Registration = "ABC" });
            _Implementation.AircraftUpdated += _AircraftUpdatedEvent.Handler;

            _Implementation.UpsertAircraftLookup(new() { ModeS = "123456", Registration = "XYZ", }, false);

            Assert.AreEqual(1, _AircraftUpdatedEvent.CallCount);
            Assert.AreEqual("123456", _AircraftUpdatedEvent.Args.Value.ModeS);
            Assert.AreEqual("XYZ", _AircraftUpdatedEvent.Args.Value.Registration);
        }

        protected void Common_UpsertAircraftLookup_Does_Not_Raise_AircraftUpdated_On_Insert()
        {
            _Implementation.WriteSupportEnabled = true;
            _Implementation.AircraftUpdated += _AircraftUpdatedEvent.Handler;

            _Implementation.UpsertAircraftLookup(new() { ModeS = "123456", Registration = "XYZ", }, false);

            Assert.AreEqual(0, _AircraftUpdatedEvent.CallCount);
        }
        #endregion

        #region UpsertManyAircraft
        protected void Common_UpsertManyAircraft_LookupVersion_Throws_Exception_If_Writes_Not_Enabled()
        {
            _Implementation.UpsertManyAircraftLookup(new KineticAircraftLookup[] {
                new() { ModeS = "A" },
            }, false);
        }

        protected void Common_UpsertManyAircraft_FullVersion_Throws_Exception_If_Writes_Not_Enabled()
        {
            _Implementation.UpsertManyAircraft(new KineticAircraftKeyless[] {
                new() { ModeS = "A" },
            });
        }

        protected void Common_UpsertManyAircraft_Inserts_New_Lookups()
        {
            _Implementation.WriteSupportEnabled = true;

            var now = new DateTime(2001, 2, 3, 4, 5, 6, 789);
            _Implementation.UpsertManyAircraftLookup(new KineticAircraftLookup[] {
                new() {
                    ModeS =             "123456",
                    Country =           "UK",
                    ICAOTypeCode =      "A380",
                    LastModified =      now,
                    Manufacturer =      "Airbus",
                    ModeSCountry =      "France",
                    OperatorFlagCode =  "TWA",
                    RegisteredOwners =  "Transworld",
                    Registration =      "G-ABCD",
                    SerialNo =          "9182",
                    Type =              "Big Plane",
                    YearBuilt =         "1992",
                },
                new() {
                    ModeS =             "789ABC",
                    Country =           "Germany",
                    ICAOTypeCode =      "B747",
                    LastModified =      now,
                    Manufacturer =      "Boeing",
                    ModeSCountry =      "USA",
                    OperatorFlagCode =  "BAW",
                    RegisteredOwners =  "British Airways",
                    Registration =      "D-WXYZ",
                    SerialNo =          "00119",
                    Type =              "Big Jobs",
                    YearBuilt =         "1979",
                },
            }, false);

            var aircraft = _Implementation.GetAircraftByCode("123456");
            Assert.IsNotNull(aircraft);
            Assert.AreEqual("123456",           aircraft.ModeS);
            Assert.AreEqual("UK",               aircraft.Country);
            Assert.AreEqual("A380",             aircraft.ICAOTypeCode);
            Assert.AreEqual(now,                aircraft.FirstCreated);
            Assert.AreEqual(now,                aircraft.LastModified);
            Assert.AreEqual("Airbus",           aircraft.Manufacturer);
            Assert.AreEqual("France",           aircraft.ModeSCountry);
            Assert.AreEqual("TWA",              aircraft.OperatorFlagCode);
            Assert.AreEqual("Transworld",       aircraft.RegisteredOwners);
            Assert.AreEqual("G-ABCD",           aircraft.Registration);
            Assert.AreEqual("9182",             aircraft.SerialNo);
            Assert.AreEqual("Big Plane",        aircraft.Type);
            Assert.AreEqual("1992",             aircraft.YearBuilt);

            aircraft = _Implementation.GetAircraftByCode("789ABC");
            Assert.IsNotNull(aircraft);
            Assert.AreEqual("789ABC",           aircraft.ModeS);
            Assert.AreEqual("Germany",          aircraft.Country);
            Assert.AreEqual("B747",             aircraft.ICAOTypeCode);
            Assert.AreEqual(now,                aircraft.FirstCreated);
            Assert.AreEqual(now,                aircraft.LastModified);
            Assert.AreEqual("Boeing",           aircraft.Manufacturer);
            Assert.AreEqual("USA",              aircraft.ModeSCountry);
            Assert.AreEqual("BAW",              aircraft.OperatorFlagCode);
            Assert.AreEqual("British Airways",  aircraft.RegisteredOwners);
            Assert.AreEqual("D-WXYZ",           aircraft.Registration);
            Assert.AreEqual("00119",            aircraft.SerialNo);
            Assert.AreEqual("Big Jobs",         aircraft.Type);
            Assert.AreEqual("1979",             aircraft.YearBuilt);
        }

        protected void Common_UpsertManyAircraft_Inserts_New_Aircraft()
        {
            _Implementation.WriteSupportEnabled = true;

            var now = new DateTime(2001, 2, 3, 4, 5, 6, 789);
            _Implementation.UpsertManyAircraft(new KineticAircraftKeyless[] {
                new() {
                    ModeS =             "123456",
                    Country =           "UK",
                    FirstCreated =      now,
                    YearBuilt =         "1992",
                    UserString4 =       "Esoteric"
                },
                new() {
                    ModeS =             "789ABC",
                    LastModified =      now,
                    Manufacturer =      "Boeing",
                    UserBool2 =         true,
                },
            });

            var aircraft = _Implementation.GetAircraftByCode("123456");
            Assert.IsNotNull(aircraft);
            Assert.AreEqual("123456",           aircraft.ModeS);
            Assert.AreEqual("UK",               aircraft.Country);
            Assert.AreEqual(now,                aircraft.FirstCreated);
            Assert.AreEqual("1992",             aircraft.YearBuilt);
            Assert.AreEqual("Esoteric",         aircraft.UserString4);

            aircraft = _Implementation.GetAircraftByCode("789ABC");
            Assert.IsNotNull(aircraft);
            Assert.AreEqual("789ABC",           aircraft.ModeS);
            Assert.AreEqual(now,                aircraft.LastModified);
            Assert.AreEqual("Boeing",           aircraft.Manufacturer);
            Assert.AreEqual(true,               aircraft.UserBool2);
        }

        protected void Common_UpsertManyAircraft_Updates_Existing_Lookups()
        {
            var createdDate = new DateTime(1999, 8, 7, 6, 5, 4, 321);
            var now = new DateTime(2001, 2, 3, 4, 5, 6, 789);

            _Implementation.WriteSupportEnabled = true;
            _Implementation.InsertAircraft(new() {
                ModeS =         "123456",
                Registration =  "N12345",
                FirstCreated =  createdDate,
                LastModified =  createdDate,
            });

            _Implementation.UpsertManyAircraftLookup(new KineticAircraftLookup[] {
                new() {
                    ModeS =             "123456",
                    Country =           "Germany",
                    ICAOTypeCode =      "B747",
                    LastModified =      now,
                    Manufacturer =      "Boeing",
                    ModeSCountry =      "USA",
                    OperatorFlagCode =  "BAW",
                    RegisteredOwners =  "British Airways",
                    Registration =      "D-WXYZ",
                    SerialNo =          "00119",
                    Type =              "Big Jobs",
                    YearBuilt =         "1979",
                },
            }, false);

            var aircraft = _Implementation.GetAircraftByCode("123456");
            Assert.IsNotNull(aircraft);

            Assert.AreEqual("123456",           aircraft.ModeS);
            Assert.AreEqual("Germany",          aircraft.Country);
            Assert.AreEqual("B747",             aircraft.ICAOTypeCode);
            Assert.AreEqual(createdDate,        aircraft.FirstCreated);
            Assert.AreEqual(now,                aircraft.LastModified);
            Assert.AreEqual("Boeing",           aircraft.Manufacturer);
            Assert.AreEqual("USA",              aircraft.ModeSCountry);
            Assert.AreEqual("BAW",              aircraft.OperatorFlagCode);
            Assert.AreEqual("British Airways",  aircraft.RegisteredOwners);
            Assert.AreEqual("D-WXYZ",           aircraft.Registration);
            Assert.AreEqual("00119",            aircraft.SerialNo);
            Assert.AreEqual("Big Jobs",         aircraft.Type);
            Assert.AreEqual("1979",             aircraft.YearBuilt);
        }

        protected void Common_UpsertManyAircraft_Updates_Existing_Aircraft()
        {
            var createdDate = new DateTime(1999, 8, 7, 6, 5, 4, 321);
            var now = new DateTime(2001, 2, 3, 4, 5, 6, 789);

            _Implementation.WriteSupportEnabled = true;
            _Implementation.InsertAircraft(new() {
                ModeS =         "123456",
                Registration =  "N12345",
                FirstCreated =  createdDate,
                LastModified =  createdDate,
            });

            _Implementation.UpsertManyAircraft(new KineticAircraftKeyless[] {
                new() {
                    ModeS =             "123456",
                    Country =           "Germany",
                    ICAOTypeCode =      "B747",
                    FirstCreated =      now,
                    LastModified =      createdDate,
                    Manufacturer =      "Boeing",
                    ModeSCountry =      "USA",
                    OperatorFlagCode =  "BAW",
                    RegisteredOwners =  "British Airways",
                    Registration =      "D-WXYZ",
                    SerialNo =          "00119",
                    Type =              "Big Jobs",
                    YearBuilt =         "1979",
                    UserBool4 =         true,
                },
            });

            var aircraft = _Implementation.GetAircraftByCode("123456");
            Assert.IsNotNull(aircraft);

            Assert.AreEqual("123456",           aircraft.ModeS);
            Assert.AreEqual("Germany",          aircraft.Country);
            Assert.AreEqual("B747",             aircraft.ICAOTypeCode);
            Assert.AreEqual(now,                aircraft.FirstCreated);
            Assert.AreEqual(createdDate,        aircraft.LastModified);
            Assert.AreEqual("Boeing",           aircraft.Manufacturer);
            Assert.AreEqual("USA",              aircraft.ModeSCountry);
            Assert.AreEqual("BAW",              aircraft.OperatorFlagCode);
            Assert.AreEqual("British Airways",  aircraft.RegisteredOwners);
            Assert.AreEqual("D-WXYZ",           aircraft.Registration);
            Assert.AreEqual("00119",            aircraft.SerialNo);
            Assert.AreEqual("Big Jobs",         aircraft.Type);
            Assert.AreEqual("1979",             aircraft.YearBuilt);
            Assert.AreEqual(true,               aircraft.UserBool4);
        }

        protected void Common_UpsertManyAircraft_Raises_AircraftUpdated_On_Update_Lookup()
        {
            _Implementation.WriteSupportEnabled = true;
            _Implementation.InsertAircraft(new() { ModeS = "AAAAAA", Registration = "---" });
            _Implementation.InsertAircraft(new() { ModeS = "BBBBBB", Registration = "===" });
            _Implementation.AircraftUpdated += _AircraftUpdatedEvent.Handler;

            _Implementation.UpsertManyAircraftLookup(new KineticAircraftLookup[] {
                new() { ModeS = "AAAAAA", Registration = "111" },
                new() { ModeS = "BBBBBB", Registration = "222" },
            }, false);

            Assert.AreEqual(2, _AircraftUpdatedEvent.CallCount);
            Assert.AreEqual("111", _AircraftUpdatedEvent.AllArgs.Single(r => r.Value.ModeS == "AAAAAA").Value.Registration);
            Assert.AreEqual("222", _AircraftUpdatedEvent.AllArgs.Single(r => r.Value.ModeS == "BBBBBB").Value.Registration);
        }

        protected void Common_UpsertManyAircraft_Raises_AircraftUpdated_On_Update_Aircraft()
        {
            _Implementation.WriteSupportEnabled = true;
            _Implementation.InsertAircraft(new() { ModeS = "AAAAAA", Registration = "---" });
            _Implementation.InsertAircraft(new() { ModeS = "BBBBBB", Registration = "===" });
            _Implementation.AircraftUpdated += _AircraftUpdatedEvent.Handler;

            _Implementation.UpsertManyAircraft(new KineticAircraftKeyless[] {
                new() { ModeS = "AAAAAA", Registration = "111" },
                new() { ModeS = "BBBBBB", Registration = "222" },
            });

            Assert.AreEqual(2, _AircraftUpdatedEvent.CallCount);
            Assert.AreEqual("111", _AircraftUpdatedEvent.AllArgs.Single(r => r.Value.ModeS == "AAAAAA").Value.Registration);
            Assert.AreEqual("222", _AircraftUpdatedEvent.AllArgs.Single(r => r.Value.ModeS == "BBBBBB").Value.Registration);
        }

        protected void Common_UpsertManyAircraft_Does_Not_Raise_AircraftUpdated_On_Insert()
        {
            _Implementation.WriteSupportEnabled = true;
            _Implementation.UpsertManyAircraftLookup(new KineticAircraftLookup[] {
                new() { ModeS = "AAAAAA", Registration = "111" },
                new() { ModeS = "BBBBBB", Registration = "222" },
            }, false);

            Assert.AreEqual(0, _AircraftUpdatedEvent.CallCount);
        }
        #endregion

        #region DeleteAircraft
        protected void Common_DeleteAircraft_Throws_If_Writes_Disabled()
        {
            var aircraft = new KineticAircraft() { ModeS = "X" };
            aircraft.AircraftID = (int)AddAircraft(aircraft);

            _Implementation.DeleteAircraft(aircraft);
        }

        protected void Common_DeleteAircraft_Correctly_Deletes_Record()
        {
            var spreadsheet = new SpreadsheetTestData(TestData.BaseStationDatabaseTests_xslx, "GetAircraftBy");
            spreadsheet.TestEveryRow(this, row => {
                _Implementation.WriteSupportEnabled = true;
                var id = (int)AddAircraft(LoadAircraftFromSpreadsheetRow(row));

                var aircraft = _Implementation.GetAircraftById(id);
                _Implementation.DeleteAircraft(aircraft);

                Assert.AreEqual(null, _Implementation.GetAircraftById(id));
            });
        }
        #endregion

        #region GetFlights
        protected void Common_GetFlights_Throws_If_Criteria_Is_Null()
        {
            _Implementation.GetFlights(null, -1, -1, null, false, null, false);
        }

        protected void Common_GetFlights_Copies_Database_Record_To_Flight_Object()
        {
            Do_GetFlightsAllVersions_Copies_Database_Record_To_Flight_Object(true);
        }

        protected void Do_GetFlightsAllVersions_Copies_Database_Record_To_Flight_Object(bool getFlights)
        {
            var spreadsheet = new SpreadsheetTestData(TestData.BaseStationDatabaseTests_xslx, "GetFlights");
            spreadsheet.TestEveryRow(this, row => {
                var mockFlight = LoadFlightFromSpreadsheetRow(row);

                var aircraftId = (int)AddAircraft(mockFlight.Aircraft);
                mockFlight.AircraftID = aircraftId;
                mockFlight.SessionID = PrepareSessionReference(new() { StartTime = DateTime.Now }).SessionID;
                var flightId = AddFlight(mockFlight);

                List<KineticFlight> flights = null;
                if(getFlights) {
                    flights = _Implementation.GetFlights(_Criteria, -1, -1, null, false, null, false);
                } else {
                    throw new NotImplementedException(); //flights = _Implementation.GetFlightsForAircraft(mockFlight.Aircraft, _Criteria, -1, -1, null, false, null, false);
                }

                Assert.AreEqual(1, flights.Count);

                Assert.AreNotSame(flights[0], mockFlight);
                if(getFlights) {
                    Assert.AreNotSame(flights[0].Aircraft, mockFlight.Aircraft);
                } else {
                    Assert.AreSame(flights[0].Aircraft, mockFlight.Aircraft);
                }

                AssertFlightsAreEqual(mockFlight, flights[0], true, aircraftId);
            });
        }
        #endregion
    }
}
