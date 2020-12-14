using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;
using Dapper;
using System.Reflection;

namespace Test.VirtualRadar.Database
{
    public class BaseStationDatabaseTests
    {
        public TestContext TestContext { get; set; }

        protected virtual string SchemaPrefix => "";
        protected virtual bool EngineTruncatesMilliseconds => false;

        protected IClassFactory _OriginalClassFactory;
        protected IBaseStationDatabase _Database;
        protected Mock<IBaseStationDatabaseProvider> _Provider;
        protected SearchBaseStationCriteria _Criteria;
        protected Func<IDbConnection> _CreateConnection;
        protected string _SqlReturnNewIdentity;
        protected readonly string[] _SortColumns = new string[] { "callsign", "country", "date", "model", "type", "operator", "reg", "icao", "firstaltitude", "lastaltitude", };
        protected Mock<IConfigurationStorage> _ConfigurationStorage;
        protected Configuration _Configuration;
        protected EventRecorder<EventArgs> _FileNameChangingEvent;
        protected EventRecorder<EventArgs> _FileNameChangedEvent;
        protected EventRecorder<EventArgs<BaseStationAircraft>> _AircraftUpdatedEvent;
        protected readonly string[] _Cultures = new string[] { "en-GB", "de-DE", "fr-FR", "it-IT", "el-GR", "ru-RU" };
        protected Mock<IRuntimeEnvironment> _RuntimeEnvironment;
        protected Mock<ICallsignParser> _CallsignParser;
        protected ClockMock _Clock;
        protected MockFileSystemProvider _FileSystem;
        protected Mock<IStandingDataManager> _StandingDataManager;
        protected CodeBlock _Icao24CodeBlock;

        protected BaseStationAircraft _DefaultAircraft;
        protected BaseStationSession _DefaultSession;
        protected BaseStationLocation _DefaultLocation;
        protected BaseStationFlight _DefaultFlight;

        protected void CommonTestInitialise<T>(Action initialiseDatabase, Func<IDbConnection> createConnection, Action<T> initialiseImplementation, string sqlReturnNewIdentity)
            where T: class, IBaseStationDatabase
        {
            _OriginalClassFactory = Factory.TakeSnapshot();
            _RuntimeEnvironment = TestUtilities.CreateMockSingleton<IRuntimeEnvironment>();

            _Clock = new ClockMock();
            Factory.RegisterInstance<IClock>(_Clock.Object);

            _ConfigurationStorage = TestUtilities.CreateMockSingleton<IConfigurationStorage>();
            _Configuration = new Configuration();
            _ConfigurationStorage.Setup(s => s.Load()).Returns(_Configuration);

            _FileSystem = new MockFileSystemProvider();
            Factory.RegisterInstance<IFileSystemProvider>(_FileSystem);

            _Icao24CodeBlock = new CodeBlock() {
                Country = "United Kingdom",
                IsMilitary = false,
            };
            _StandingDataManager = TestUtilities.CreateMockSingleton<IStandingDataManager>();
            _StandingDataManager.Setup(r => r.FindCodeBlock(It.IsAny<string>())).Returns(_Icao24CodeBlock);

            initialiseDatabase?.Invoke();
            _CreateConnection = createConnection;
            _SqlReturnNewIdentity = sqlReturnNewIdentity;

            var implementation = Factory.Resolve(typeof(T)) as T;
            _Database = implementation;

            _Provider = new Mock<IBaseStationDatabaseProvider>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _Database.Provider = _Provider.Object;
            _Provider.Setup(p => p.UtcNow).Returns(DateTime.UtcNow);
            initialiseImplementation?.Invoke(implementation);

            _CallsignParser = TestUtilities.CreateMockImplementation<ICallsignParser>();

            _Criteria = new SearchBaseStationCriteria() {
                Date = new FilterRange<DateTime>(DateTime.MinValue, DateTime.MaxValue),
                FirstAltitude = new FilterRange<int>(int.MinValue, int.MaxValue),
                LastAltitude = new FilterRange<int>(int.MinValue, int.MaxValue),
            };

            _FileNameChangingEvent = new EventRecorder<EventArgs>();
            _FileNameChangedEvent = new EventRecorder<EventArgs>();
            _AircraftUpdatedEvent = new EventRecorder<EventArgs<BaseStationAircraft>>();

            _DefaultAircraft = new BaseStationAircraft() {
                ModeS = "123456",
            };
            _DefaultLocation = new BaseStationLocation() {
                Altitude = 25,
                Latitude = 54.1,
                Longitude = -0.6,
                LocationName = "Default Location",
            };
            _DefaultSession = new BaseStationSession() {
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddSeconds(30),
            };
            _DefaultFlight = new BaseStationFlight() {
                StartTime = DateTime.Now,
            };
        }

        protected void CommonTestCleanup()
        {
            if(_Database != null) {
                _Database.Dispose();
                _Database = null;
            }

            Factory.RestoreSnapshot(_OriginalClassFactory);
        }

        private MethodInfo _TestInitialise;
        private void RunTestInitialise()
        {
            if(_TestInitialise == null) {
                _TestInitialise = GetType().GetMethods().Single(r => r.GetCustomAttributes(typeof(TestInitializeAttribute), inherit: false).Length != 0);
            }
            _TestInitialise.Invoke(this, new object[0]);
        }

        private MethodInfo _TestCleanup;
        private void RunTestCleanup()
        {
            if(_TestCleanup == null) {
                _TestCleanup = GetType().GetMethods().Single(r => r.GetCustomAttributes(typeof(TestCleanupAttribute), inherit: false).Length != 0);
            }
            _TestCleanup.Invoke(this, new object[0]);
        }

        #region Helpers
        /// <summary>
        /// Retries an action until it either stops giving IO errors or too many errors have occurred.
        /// </summary>
        /// <param name="action"></param>
        /// <remarks>
        /// On my W7 desktop with SSDs the tests occasionally failed because the file was still in use after
        /// it had been disposed of. This function retries file operations until either they stop giving
        /// exceptions or a counter expires.
        /// </remarks>
        protected void RetryAction(Action action)
        {
            const int retries = 20;
            for(var i = 0;i < retries;++i) {
                var pause = false;
                var giveUp = i + 1 == retries;
                try {
                    action();
                    break;
                } catch(IOException) {
                    if(giveUp) {
                        throw;
                    }
                    pause = true;
                } catch(UnauthorizedAccessException) {
                    if(giveUp) {
                        throw;
                    }
                    pause = true;
                }
                if(pause) {
                    Thread.Sleep(250);
                }
            }
        }

        /// <summary>
        /// Removes the milliseconds from a date.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        /// <remarks>
        /// The SQLite implementation of IBaseStationDatabase stores times with the milliseconds stripped off - this helps keep
        /// compatibility with some 3rd party utilities.
        /// </remarks>
        protected DateTime TruncateDate(DateTime date)
        {
            return !EngineTruncatesMilliseconds ? date : new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
        }

        /// <summary>
        /// Removes the milliseconds from a nullable date.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        protected DateTime? TruncateDate(DateTime? date)
        {
            return !EngineTruncatesMilliseconds ? date : date == null ? (DateTime?)null : TruncateDate(date.Value);
        }

        protected BaseStationAircraft CreateAircraft(string icao24 = "123456", string registration = "G-VRST")
        {
            return new BaseStationAircraft() {
                ModeS = icao24,
                Registration = registration,
            };
        }

        /// <summary>
        /// Reads actual details from a spreadsheet, assuming that the fields are in alphabetical order from the first ordinal passed across.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="firstOrdinal"></param>
        /// <param name="copyIntoAircraft"></param>
        /// <returns></returns>
        /// <remarks>
        /// The fields, in ascending order of ordinal, are:
        /// AircraftClass, Country, DeRegDate, Engines, FirstCreated, GenericName, ICAOTypeCode, LastModified, Manufacturer, ModeS, ModeSCountry,
        /// OperatorFlagCode, OwnershipStatus, PopularName, PreviousID, RegisteredOwners, Registration, SerialNo, Status, Type, CofACategory, CofAExpiry
        /// CurrentRegDate, FirstRegDate, InfoUrl, Interested, MTOW, PictureUrl1, PictureUrl2, PictureUrl3, TotalHours, UserNotes, UserTag and YearBuilt.
        /// </remarks>
        protected BaseStationAircraft LoadAircraftFromSpreadsheet(ExcelWorksheetData worksheet, int firstOrdinal = 0, BaseStationAircraft copyIntoAircraft = null)
        {
            var result = copyIntoAircraft == null ? new BaseStationAircraft() : copyIntoAircraft;

            int ordinal = firstOrdinal;
            result.AircraftClass = worksheet.EString(ordinal++);
            result.Country = worksheet.EString(ordinal++);
            result.DeRegDate = worksheet.EString(ordinal++);
            result.Engines = worksheet.EString(ordinal++);
            result.FirstCreated = worksheet.DateTime(ordinal++);
            result.GenericName = worksheet.EString(ordinal++);
            result.ICAOTypeCode = worksheet.EString(ordinal++);
            result.LastModified = worksheet.DateTime(ordinal++);
            result.Manufacturer = worksheet.EString(ordinal++);
            result.ModeS = worksheet.EString(ordinal++);
            result.ModeSCountry = worksheet.EString(ordinal++);
            result.OperatorFlagCode = worksheet.EString(ordinal++);
            result.OwnershipStatus = worksheet.EString(ordinal++);
            result.PopularName = worksheet.EString(ordinal++);
            result.PreviousID = worksheet.EString(ordinal++);
            result.RegisteredOwners = worksheet.EString(ordinal++);
            result.Registration = worksheet.EString(ordinal++);
            result.SerialNo = worksheet.EString(ordinal++);
            result.Status = worksheet.EString(ordinal++);
            result.Type = worksheet.EString(ordinal++);
            result.CofACategory = worksheet.EString(ordinal++);
            result.CofAExpiry = worksheet.EString(ordinal++);
            result.CurrentRegDate = worksheet.EString(ordinal++);
            result.FirstRegDate = worksheet.EString(ordinal++);
            result.InfoUrl = worksheet.EString(ordinal++);
            result.Interested = worksheet.Bool(ordinal++);
            result.MTOW = worksheet.EString(ordinal++);
            result.PictureUrl1 = worksheet.EString(ordinal++);
            result.PictureUrl2 = worksheet.EString(ordinal++);
            result.PictureUrl3 = worksheet.EString(ordinal++);
            result.TotalHours = worksheet.EString(ordinal++);
            result.UserNotes = worksheet.EString(ordinal++);
            result.UserString1 = worksheet.EString(ordinal++);
            result.UserString2 = worksheet.EString(ordinal++);
            result.UserString3 = worksheet.EString(ordinal++);
            result.UserString4 = worksheet.EString(ordinal++);
            result.UserString5 = worksheet.EString(ordinal++);
            result.UserBool1 = worksheet.Bool(ordinal++);
            result.UserBool2 = worksheet.Bool(ordinal++);
            result.UserBool3 = worksheet.Bool(ordinal++);
            result.UserBool4 = worksheet.Bool(ordinal++);
            result.UserBool5 = worksheet.Bool(ordinal++);
            result.UserInt1 = worksheet.Long(ordinal++);
            result.UserInt2 = worksheet.Long(ordinal++);
            result.UserInt3 = worksheet.Long(ordinal++);
            result.UserInt4 = worksheet.Long(ordinal++);
            result.UserInt5 = worksheet.Long(ordinal++);
            result.UserTag = worksheet.EString(ordinal++);
            result.YearBuilt = worksheet.EString(ordinal++);

            return result;
        }

        protected long AddAircraft(BaseStationAircraft aircraft)
        {
            long result = 0;

            using(var connection = _CreateConnection()) {
                connection.Open();

                var dynamicParameters = new DynamicParameters();
                var fieldNames = new StringBuilder();
                var parameters = new StringBuilder();

                foreach(var property in typeof(BaseStationAircraft).GetProperties()) {
                    var fieldName = property.Name;
                    if(fieldName == nameof(BaseStationAircraft.AircraftID)) continue;

                    if(fieldNames.Length > 0) fieldNames.Append(',');
                    if(parameters.Length > 0) parameters.Append(',');

                    fieldNames.Append($"[{fieldName}]");
                    parameters.Append($"@{fieldName}");
                    dynamicParameters.Add(fieldName, property.GetValue(aircraft, null));
                }

                result = connection.ExecuteScalar<long>($"INSERT INTO {SchemaPrefix}[Aircraft] ({fieldNames}) VALUES ({parameters}); {_SqlReturnNewIdentity}", dynamicParameters);
                aircraft.AircraftID = (int)result;
            }

            return result;
        }

        protected BaseStationAircraft PrepareAircraftReference(BaseStationAircraft aircraft)
        {
            if(aircraft == null) {
                aircraft = _DefaultAircraft;
            }
            if(aircraft.AircraftID == 0) {
                AddAircraft(aircraft);
            }

            return aircraft;
        }

        /// <summary>
        /// Creates a flight object.
        /// </summary>
        /// <param name="actual"></param>
        /// <returns></returns>
        protected BaseStationFlight CreateFlight(BaseStationAircraft aircraft = null, string id = null)
        {
            var result = new BaseStationFlight() { Aircraft = new BaseStationAircraft() };
            if(id != null) result.Callsign = id;
            if(aircraft != null) result.Aircraft = aircraft;
            if(result.Aircraft != null) result.AircraftID = result.Aircraft.AircraftID;

            return result;
        }

        /// <summary>
        /// Creates a set of flight and actual records and sets the unique identifiers on each to the ID string passed across.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="setCallsign"></param>
        /// <param name="setRegistration"></param>
        /// <returns></returns>
        protected BaseStationFlight CreateFlight(string id, bool setCallsign = true, bool setRegistration = true)
        {
            var result = CreateFlight();
            result.Aircraft.ModeS = id;
            if(setRegistration) result.Aircraft.Registration = id;
            if(setCallsign) result.Callsign = id;

            return result;
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
        protected BaseStationFlight LoadFlightFromSpreadsheet(ExcelWorksheetData worksheet, int firstOrdinal = 0, BaseStationFlight copyIntoFlight = null)
        {
            int ordinal = firstOrdinal;

            var aircraft = CreateAircraft();
            aircraft.AircraftID = worksheet.Int(ordinal++);

            var result = copyIntoFlight ?? CreateFlight(aircraft) ;
            result.AircraftID = aircraft.AircraftID;
            result.Callsign = worksheet.EString(ordinal++);
            result.EndTime = worksheet.DateTime(ordinal++);
            result.FirstAltitude = worksheet.Int(ordinal++);
            result.FirstGroundSpeed = worksheet.Float(ordinal++);
            result.FirstIsOnGround = worksheet.Bool(ordinal++);
            result.FirstLat = worksheet.Float(ordinal++);
            result.FirstLon = worksheet.Float(ordinal++);
            result.FirstSquawk = worksheet.Int(ordinal++);
            result.FirstTrack = worksheet.Float(ordinal++);
            result.FirstVerticalRate = worksheet.Int(ordinal++);
            result.HadAlert = worksheet.Bool(ordinal++);
            result.HadEmergency = worksheet.Bool(ordinal++);
            result.HadSpi = worksheet.Bool(ordinal++);
            result.LastAltitude = worksheet.Int(ordinal++);
            result.LastGroundSpeed = worksheet.Float(ordinal++);
            result.LastIsOnGround = worksheet.Bool(ordinal++);
            result.LastLat = worksheet.Float(ordinal++);
            result.LastLon = worksheet.Float(ordinal++);
            result.LastSquawk = worksheet.Int(ordinal++);
            result.LastTrack = worksheet.Float(ordinal++);
            result.LastVerticalRate = worksheet.Int(ordinal++);
            result.NumADSBMsgRec = worksheet.Int(ordinal++);
            result.NumModeSMsgRec = worksheet.Int(ordinal++);
            result.NumIDMsgRec = worksheet.Int(ordinal++);
            result.NumSurPosMsgRec = worksheet.Int(ordinal++);
            result.NumAirPosMsgRec = worksheet.Int(ordinal++);
            result.NumAirVelMsgRec = worksheet.Int(ordinal++);
            result.NumSurAltMsgRec = worksheet.Int(ordinal++);
            result.NumSurIDMsgRec = worksheet.Int(ordinal++);
            result.NumAirToAirMsgRec = worksheet.Int(ordinal++);
            result.NumAirCallRepMsgRec = worksheet.Int(ordinal++);
            result.NumPosMsgRec = worksheet.Int(ordinal++);
            result.StartTime = worksheet.DateTime(ordinal++);
            result.UserNotes = worksheet.EString(ordinal++);

            return result;
        }

        protected long AddFlight(BaseStationFlight flight, BaseStationSession session = null, BaseStationAircraft aircraft = null)
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

                foreach(var property in typeof(BaseStationFlight).GetProperties()) {
                    var fieldName = property.Name;
                    var value = property.GetValue(flight, null);

                    switch(fieldName) {
                        case nameof(BaseStationFlight.FlightID):
                        case nameof(BaseStationFlight.Aircraft):
                            continue;
                        case nameof(BaseStationFlight.AircraftID):
                            if(flight.AircraftID == 0) {
                                value = aircraft.AircraftID;
                            }
                            break;
                        case nameof(BaseStationFlight.SessionID):
                            if(flight.SessionID == 0) {
                                value = session.SessionID;
                            }
                            break;
                    }

                    if(fieldNames.Length > 0) fieldNames.Append(',');
                    if(parameters.Length > 0) parameters.Append(',');

                    fieldNames.Append($"[{fieldName}]");
                    parameters.Append($"@{fieldName}");
                    dynamicParameters.Add(fieldName, value);
                }

                result = connection.ExecuteScalar<long>($"INSERT INTO {SchemaPrefix}[Flights] ({fieldNames}) VALUES ({parameters}); {_SqlReturnNewIdentity}", dynamicParameters);
                flight.FlightID = (int)result;
            }

            return result;
        }

        protected BaseStationFlight PrepareFlightReference(BaseStationFlight flight)
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

        protected void AddFlightAndAircraft(BaseStationFlight flight)
        {
            flight.AircraftID = (int)AddAircraft(flight.Aircraft);
            AddFlight(flight);
        }

        protected void ClearDBHistory()
        {
            using(var connection = _CreateConnection()) {
                connection.Execute($"DELETE FROM {SchemaPrefix}[DBHistory]");
            }
        }

        protected long AddDBHistory(BaseStationDBHistory dbHistory)
        {
            long result = 0;

            using(var connection = _CreateConnection()) {
                connection.Open();

                result = connection.ExecuteScalar<long>($@"
                    INSERT INTO {SchemaPrefix}[DBHistory] (
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

        protected long AddDBInfo(BaseStationDBInfo dbInfo)
        {
            long result = 0;

            using(var connection = _CreateConnection()) {
                connection.Open();

                result = connection.ExecuteScalar<long>($@"
                    INSERT INTO {SchemaPrefix}[DBInfo] (
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

        protected void ClearSystemEvents()
        {
            using(var connection = _CreateConnection()) {
                connection.Execute($"DELETE FROM {SchemaPrefix}[SystemEvents]");
            }
        }

        protected long AddSystemEvent(BaseStationSystemEvents systemEvent)
        {
            long result = 0;

            using(var connection = _CreateConnection()) {
                connection.Open();

                result = connection.ExecuteScalar<long>($@"
                    INSERT INTO {SchemaPrefix}[SystemEvents] (
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

        protected void ClearLocations()
        {
            using(var connection = _CreateConnection()) {
                connection.Execute($"DELETE FROM {SchemaPrefix}[Locations]");
            }
        }

        protected long AddLocation(BaseStationLocation location)
        {
            long result = 0;

            using(var connection = _CreateConnection()) {
                connection.Open();

                result = connection.ExecuteScalar<long>($@"
                    INSERT INTO {SchemaPrefix}[Locations] (
                        [Altitude]
                       ,[Latitude]
                       ,[LocationName]
                       ,[Longitude]
                    ) VALUES (
                        @altitude
                       ,@latitude
                       ,@locationName
                       ,@longitude
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

        protected BaseStationLocation PrepareLocationReference(BaseStationLocation location)
        {
            if(location == null) {
                location = _DefaultLocation;
            }
            if(location.LocationID == 0) {
                AddLocation(location);
            }

            return location;
        }

        protected long AddSession(BaseStationSession session)
        {
            long result = 0;

            using(var connection = _CreateConnection()) {
                connection.Open();

                result = connection.ExecuteScalar<long>($@"
                    INSERT INTO {SchemaPrefix}[Sessions] (
                        [LocationID]
                       ,[StartTime]
                       ,[EndTime]
                    ) VALUES (
                        @locationID
                       ,@startTime
                       ,@endTime
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

        protected BaseStationSession PrepareSessionReference(BaseStationSession session)
        {
            if(session == null) {
                session = _DefaultSession;
            }
            if(session.LocationID == 0) {
                session.LocationID = PrepareLocationReference(null).LocationID;
            }
            if(session.SessionID == 0) {
                AddSession(session);
            }

            return session;
        }

        protected void AssertAircraftAreEqual(BaseStationAircraft expected, BaseStationAircraft actual, long id = -1L)
        {
            Assert.AreEqual(id == -1L ? expected.AircraftID : (int)id, actual.AircraftID);
            Assert.AreEqual(expected.AircraftClass, actual.AircraftClass);
            Assert.AreEqual(expected.Country, actual.Country);
            Assert.AreEqual(expected.DeRegDate, actual.DeRegDate);
            Assert.AreEqual(expected.Engines, actual.Engines);
            Assert.AreEqual(expected.FirstCreated, actual.FirstCreated);
            Assert.AreEqual(expected.GenericName, actual.GenericName);
            Assert.AreEqual(expected.ICAOTypeCode, actual.ICAOTypeCode);
            Assert.AreEqual(expected.LastModified, actual.LastModified);
            Assert.AreEqual(expected.Manufacturer, actual.Manufacturer);
            Assert.AreEqual(expected.ModeS, actual.ModeS);
            Assert.AreEqual(expected.ModeSCountry, actual.ModeSCountry);
            Assert.AreEqual(expected.OperatorFlagCode, actual.OperatorFlagCode);
            Assert.AreEqual(expected.OwnershipStatus, actual.OwnershipStatus);
            Assert.AreEqual(expected.PopularName, actual.PopularName);
            Assert.AreEqual(expected.PreviousID, actual.PreviousID);
            Assert.AreEqual(expected.RegisteredOwners, actual.RegisteredOwners);
            Assert.AreEqual(expected.Registration, actual.Registration);
            Assert.AreEqual(expected.SerialNo, actual.SerialNo);
            Assert.AreEqual(expected.Status, actual.Status);
            Assert.AreEqual(expected.Type, actual.Type);
            Assert.AreEqual(expected.CofACategory, actual.CofACategory);
            Assert.AreEqual(expected.CofAExpiry, actual.CofAExpiry);
            Assert.AreEqual(expected.CurrentRegDate, actual.CurrentRegDate);
            Assert.AreEqual(expected.FirstRegDate, actual.FirstRegDate);
            Assert.AreEqual(expected.InfoUrl, actual.InfoUrl);
            Assert.AreEqual(expected.Interested, actual.Interested);
            Assert.AreEqual(expected.MTOW, actual.MTOW);
            Assert.AreEqual(expected.PictureUrl1, actual.PictureUrl1);
            Assert.AreEqual(expected.PictureUrl2, actual.PictureUrl2);
            Assert.AreEqual(expected.PictureUrl3, actual.PictureUrl3);
            Assert.AreEqual(expected.TotalHours, actual.TotalHours);
            Assert.AreEqual(expected.UserNotes, actual.UserNotes);
            Assert.AreEqual(expected.UserString1, actual.UserString1);
            Assert.AreEqual(expected.UserString2, actual.UserString2);
            Assert.AreEqual(expected.UserString3, actual.UserString3);
            Assert.AreEqual(expected.UserString4, actual.UserString4);
            Assert.AreEqual(expected.UserString5, actual.UserString5);
            Assert.AreEqual(expected.UserBool1, actual.UserBool1);
            Assert.AreEqual(expected.UserBool2, actual.UserBool2);
            Assert.AreEqual(expected.UserBool3, actual.UserBool3);
            Assert.AreEqual(expected.UserBool4, actual.UserBool4);
            Assert.AreEqual(expected.UserBool5, actual.UserBool5);
            Assert.AreEqual(expected.UserInt1, actual.UserInt1);
            Assert.AreEqual(expected.UserInt2, actual.UserInt2);
            Assert.AreEqual(expected.UserInt3, actual.UserInt3);
            Assert.AreEqual(expected.UserInt4, actual.UserInt4);
            Assert.AreEqual(expected.UserInt5, actual.UserInt5);
            Assert.AreEqual(expected.UserTag, actual.UserTag);
            Assert.AreEqual(expected.YearBuilt, actual.YearBuilt);
        }

        protected static void AssertFlightsAreEqual(BaseStationFlight expected, BaseStationFlight actual, bool expectAircraftFilled, int expectedAircraftId)
        {
            Assert.AreEqual(expectedAircraftId, actual.AircraftID);
            if(expectAircraftFilled) Assert.AreEqual(expectedAircraftId, actual.Aircraft.AircraftID);
            else                     Assert.IsNull(actual.Aircraft);
            Assert.AreEqual(expected.Callsign, actual.Callsign);
            Assert.AreEqual(expected.EndTime, actual.EndTime);
            Assert.AreEqual(expected.FirstAltitude, actual.FirstAltitude);
            Assert.AreEqual(expected.FirstGroundSpeed, actual.FirstGroundSpeed);
            Assert.AreEqual(expected.FirstIsOnGround, actual.FirstIsOnGround);
            Assert.AreEqual(expected.FirstLat, actual.FirstLat);
            Assert.AreEqual(expected.FirstLon, actual.FirstLon);
            Assert.AreEqual(expected.FirstSquawk, actual.FirstSquawk);
            Assert.AreEqual(expected.FirstTrack, actual.FirstTrack);
            Assert.AreEqual(expected.FirstVerticalRate, actual.FirstVerticalRate);
            Assert.AreEqual(expected.HadAlert, actual.HadAlert);
            Assert.AreEqual(expected.HadEmergency, actual.HadEmergency);
            Assert.AreEqual(expected.HadSpi, actual.HadSpi);
            Assert.AreEqual(expected.LastAltitude, actual.LastAltitude);
            Assert.AreEqual(expected.LastGroundSpeed, actual.LastGroundSpeed);
            Assert.AreEqual(expected.LastIsOnGround, actual.LastIsOnGround);
            Assert.AreEqual(expected.LastLat, actual.LastLat);
            Assert.AreEqual(expected.LastLon, actual.LastLon);
            Assert.AreEqual(expected.LastSquawk, actual.LastSquawk);
            Assert.AreEqual(expected.LastTrack, actual.LastTrack);
            Assert.AreEqual(expected.LastVerticalRate, actual.LastVerticalRate);
            Assert.AreEqual(expected.NumADSBMsgRec, actual.NumADSBMsgRec);
            Assert.AreEqual(expected.NumModeSMsgRec, actual.NumModeSMsgRec);
            Assert.AreEqual(expected.NumIDMsgRec, actual.NumIDMsgRec);
            Assert.AreEqual(expected.NumSurPosMsgRec, actual.NumSurPosMsgRec);
            Assert.AreEqual(expected.NumAirPosMsgRec, actual.NumAirPosMsgRec);
            Assert.AreEqual(expected.NumAirVelMsgRec, actual.NumAirVelMsgRec);
            Assert.AreEqual(expected.NumSurAltMsgRec, actual.NumSurAltMsgRec);
            Assert.AreEqual(expected.NumSurIDMsgRec, actual.NumSurIDMsgRec);
            Assert.AreEqual(expected.NumAirToAirMsgRec, actual.NumAirToAirMsgRec);
            Assert.AreEqual(expected.NumAirCallRepMsgRec, actual.NumAirCallRepMsgRec);
            Assert.AreEqual(expected.NumPosMsgRec, actual.NumPosMsgRec);
            Assert.AreEqual(expected.StartTime, actual.StartTime);
            Assert.AreEqual(expected.SessionID, actual.SessionID);
            Assert.AreEqual(expected.UserNotes, actual.UserNotes);
        }
        #endregion

        #region Criteria & sort test helpers - IsFlightCriteria, IsFlightSortColumn, SetEqualityCriteria, SetRangeCriteria, SetSortColumnValue
        /// <summary>
        /// Returns true if the criteria property refers to a flight property as opposed to an actual property (e.g.
        /// callsign is a flight property and returns true while registration is an actual property and returns false).
        /// </summary>
        /// <param name="criteriaProperty"></param>
        /// <returns></returns>
        protected bool IsFlightCriteria(PropertyInfo criteriaProperty)
        {
            switch(criteriaProperty.Name) {
                case "Date":
                case "IsEmergency":
                case "FirstAltitude":
                case "LastAltitude":
                case "Callsign":                return true;

                case "Icao":
                case "Operator":
                case "Country":
                case "Registration":
                case "Type":
                case "UseAlternateCallsigns":   return false;

                default:                        throw new NotImplementedException(criteriaProperty.Name);
            }
        }

        /// <summary>
        /// Returns true if the sort column passed across refers to a property on the flight as opposed to a property on the aircraft.
        /// </summary>
        /// <param name="sortColumn"></param>
        /// <returns></returns>
        protected bool IsFlightSortColumn(string sortColumn)
        {
            switch(sortColumn) {
                case "date":
                case "firstaltitude":
                case "lastaltitude":
                case "callsign":    return true;

                case "country":
                case "model":
                case "type":
                case "operator":
                case "reg":
                case "icao":        return false;

                default:            throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns true if the criteria property refers to a string filter.
        /// </summary>
        /// <param name="criteriaProperty"></param>
        /// <returns></returns>
        protected bool IsFilterStringProperty(PropertyInfo criteriaProperty)
        {
            return typeof(FilterString).IsAssignableFrom(criteriaProperty.PropertyType);
        }

        /// <summary>
        /// Sets a property on a flight based on the name of a criteria property.
        /// </summary>
        /// <param name="criteriaProperty"></param>
        /// <param name="flight"></param>
        /// <param name="value"></param>
        protected void SetStringAircraftProperty(PropertyInfo criteriaProperty, BaseStationFlight flight, string value)
        {
            switch(criteriaProperty.Name) {
                case "Callsign":        flight.Callsign = value; break;
                case "Operator":        flight.Aircraft.RegisteredOwners = value; break;
                case "Registration":    flight.Aircraft.Registration = value; break;
                case "Icao":            flight.Aircraft.ModeS = value; break;
                case "Country":         flight.Aircraft.ModeSCountry = value; break;
                case "Type":            flight.Aircraft.ICAOTypeCode = value; break;
                default:
                    // only pass properties that pass "IsFilterStringProperty" to this method.
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Sets up the flights passed across for an equality criteria test.
        /// </summary>
        /// <param name="criteriaProperty"></param>
        /// <param name="defaultFlight"></param>
        /// <param name="notEqualFlight"></param>
        /// <param name="equalsFlight"></param>
        /// <param name="reverseCondition"></param>
        /// <returns>Returns true if the criteria property is an equality criteria.</returns>
        protected bool SetEqualityCriteria(PropertyInfo criteriaProperty, BaseStationFlight defaultFlight, BaseStationFlight notEqualFlight, BaseStationFlight equalsFlight, bool reverseCondition)
        {
            bool result = true;

            string flightPropertyName = criteriaProperty.Name;
            string aircraftPropertyName = null;
            object criteriaValue = null;
            object defaultValue = null;
            object notEqualValue = null;
            object equalValue = null;

            switch(criteriaProperty.Name) {
                case "Callsign":
                    criteriaValue = new FilterString("A") { ReverseCondition = reverseCondition };
                    defaultValue = aircraftPropertyName == "ModeS" ? "" : null;
                    notEqualValue = "AA";
                    equalValue = "A";
                    break;
                case "Registration":
                case "Icao":
                case "Operator":
                case "Country":
                case "Type":
                    flightPropertyName = null;
                    switch(criteriaProperty.Name) {
                        case "Icao":        aircraftPropertyName = "ModeS"; break;
                        case "Operator":    aircraftPropertyName = "RegisteredOwners"; break;
                        case "Country":     aircraftPropertyName = "ModeSCountry"; break;
                        case "Type":        aircraftPropertyName = "ICAOTypeCode"; break;
                        default:            aircraftPropertyName = criteriaProperty.Name; break;
                    }
                    goto case "Callsign";
                case "Date":
                case "UseAlternateCallsigns":
                case "FirstAltitude":
                case "LastAltitude":
                    result = false;
                    break;
                case "IsEmergency":
                    criteriaValue = new FilterBool(true) { ReverseCondition = reverseCondition };
                    defaultValue = false;
                    notEqualValue = false;
                    equalValue = true;
                    flightPropertyName = "HadEmergency";
                    break;
                default:
                    throw new NotImplementedException();
            }

            if(result) {
                criteriaProperty.SetValue(_Criteria, criteriaValue, null);

                if(aircraftPropertyName != null) {
                    var aircraftProperty = typeof(BaseStationAircraft).GetProperty(aircraftPropertyName);
                    aircraftProperty.SetValue(defaultFlight.Aircraft, defaultValue, null);
                    aircraftProperty.SetValue(notEqualFlight.Aircraft, notEqualValue, null);
                    aircraftProperty.SetValue(equalsFlight.Aircraft, equalValue, null);
                } else {
                    var flightProperty = typeof(BaseStationFlight).GetProperty(flightPropertyName);
                    flightProperty.SetValue(defaultFlight, defaultValue, null);
                    flightProperty.SetValue(notEqualFlight, notEqualValue, null);
                    flightProperty.SetValue(equalsFlight, equalValue, null);
                }
            }

            return result;
        }

        /// <summary>
        /// Sets up the flights passed across for a contains criteria test.
        /// </summary>
        /// <param name="criteriaProperty"></param>
        /// <param name="defaultFlight"></param>
        /// <param name="notContainsFlight"></param>
        /// <param name="containsFlight"></param>
        /// <param name="reverseCondition"></param>
        /// <returns>Returns true if the criteria property is an contains criteria.</returns>
        protected bool SetContainsCriteria(PropertyInfo criteriaProperty, BaseStationFlight defaultFlight, BaseStationFlight notContainsFlight, BaseStationFlight containsFlight, bool reverseCondition)
        {
            bool result = true;

            string flightPropertyName = criteriaProperty.Name;
            string aircraftPropertyName = null;
            object criteriaValue = null;
            object defaultValue = null;
            object notContainsValue = null;
            object containsValue = null;

            switch(criteriaProperty.Name) {
                case "Callsign":
                    criteriaValue = new FilterString("B") { Condition = FilterCondition.Contains, ReverseCondition = reverseCondition };
                    defaultValue = aircraftPropertyName == "ModeS" ? "" : null;
                    notContainsValue = "DEF";
                    containsValue = "ABC";
                    break;
                case "Registration":
                case "Icao":
                case "Operator":
                case "Country":
                case "Type":
                    flightPropertyName = null;
                    switch(criteriaProperty.Name) {
                        case "Icao":        aircraftPropertyName = "ModeS"; break;
                        case "Operator":    aircraftPropertyName = "RegisteredOwners"; break;
                        case "Country":     aircraftPropertyName = "ModeSCountry"; break;
                        case "Type":        aircraftPropertyName = "ICAOTypeCode"; break;
                        default:            aircraftPropertyName = criteriaProperty.Name; break;
                    }
                    goto case "Callsign";
                case "Date":
                case "IsEmergency":
                case "UseAlternateCallsigns":
                case "FirstAltitude":
                case "LastAltitude":
                    result = false;
                    break;
                default:
                    throw new NotImplementedException();
            }

            if(result) {
                criteriaProperty.SetValue(_Criteria, criteriaValue, null);

                if(aircraftPropertyName != null) {
                    var aircraftProperty = typeof(BaseStationAircraft).GetProperty(aircraftPropertyName);
                    aircraftProperty.SetValue(defaultFlight.Aircraft, defaultValue, null);
                    aircraftProperty.SetValue(notContainsFlight.Aircraft, notContainsValue, null);
                    aircraftProperty.SetValue(containsFlight.Aircraft, containsValue, null);
                } else {
                    var flightProperty = typeof(BaseStationFlight).GetProperty(flightPropertyName);
                    flightProperty.SetValue(defaultFlight, defaultValue, null);
                    flightProperty.SetValue(notContainsFlight, notContainsValue, null);
                    flightProperty.SetValue(containsFlight, containsValue, null);
                }
            }

            return result;
        }

        /// <summary>
        /// Sets up the flights passed across for a starts with criteria test.
        /// </summary>
        /// <param name="criteriaProperty"></param>
        /// <param name="defaultFlight"></param>
        /// <param name="notStartsWithFlight"></param>
        /// <param name="startsWithFlight"></param>
        /// <param name="reverseCondition"></param>
        /// <returns>Returns true if the criteria property is an starts with criteria.</returns>
        protected bool SetStartsWithCriteria(PropertyInfo criteriaProperty, BaseStationFlight defaultFlight, BaseStationFlight notStartsWithFlight, BaseStationFlight startsWithFlight, bool reverseCondition)
        {
            bool result = true;

            string flightPropertyName = criteriaProperty.Name;
            string aircraftPropertyName = null;
            object criteriaValue = null;
            object defaultValue = null;
            object notStartsWithValue = null;
            object startsWithValue = null;

            switch(criteriaProperty.Name) {
                case "Callsign":
                    criteriaValue = new FilterString("AB") { Condition = FilterCondition.StartsWith, ReverseCondition = reverseCondition };
                    defaultValue = aircraftPropertyName == "ModeS" ? "" : null;
                    notStartsWithValue = "DEF";
                    startsWithValue = "ABC";
                    break;
                case "Registration":
                case "Icao":
                case "Operator":
                case "Country":
                case "Type":
                    flightPropertyName = null;
                    switch(criteriaProperty.Name) {
                        case "Icao":        aircraftPropertyName = "ModeS"; break;
                        case "Operator":    aircraftPropertyName = "RegisteredOwners"; break;
                        case "Country":     aircraftPropertyName = "ModeSCountry"; break;
                        case "Type":        aircraftPropertyName = "ICAOTypeCode"; break;
                        default:            aircraftPropertyName = criteriaProperty.Name; break;
                    }
                    goto case "Callsign";
                case "Date":
                case "IsEmergency":
                case "UseAlternateCallsigns":
                case "FirstAltitude":
                case "LastAltitude":
                    result = false;
                    break;
                default:
                    throw new NotImplementedException();
            }

            if(result) {
                criteriaProperty.SetValue(_Criteria, criteriaValue, null);

                if(aircraftPropertyName != null) {
                    var aircraftProperty = typeof(BaseStationAircraft).GetProperty(aircraftPropertyName);
                    aircraftProperty.SetValue(defaultFlight.Aircraft, defaultValue, null);
                    aircraftProperty.SetValue(notStartsWithFlight.Aircraft, notStartsWithValue, null);
                    aircraftProperty.SetValue(startsWithFlight.Aircraft, startsWithValue, null);
                } else {
                    var flightProperty = typeof(BaseStationFlight).GetProperty(flightPropertyName);
                    flightProperty.SetValue(defaultFlight, defaultValue, null);
                    flightProperty.SetValue(notStartsWithFlight, notStartsWithValue, null);
                    flightProperty.SetValue(startsWithFlight, startsWithValue, null);
                }
            }

            return result;
        }

        /// <summary>
        /// Sets up the flights passed across for a ends with criteria test.
        /// </summary>
        /// <param name="criteriaProperty"></param>
        /// <param name="defaultFlight"></param>
        /// <param name="notEndsWithFlight"></param>
        /// <param name="endsWithFlight"></param>
        /// <param name="reverseCondition"></param>
        /// <returns>Returns true if the criteria property is an ends with criteria.</returns>
        protected bool SetEndsWithCriteria(PropertyInfo criteriaProperty, BaseStationFlight defaultFlight, BaseStationFlight notEndsWithFlight, BaseStationFlight endsWithFlight, bool reverseCondition)
        {
            bool result = true;

            string flightPropertyName = criteriaProperty.Name;
            string aircraftPropertyName = null;
            object criteriaValue = null;
            object defaultValue = null;
            object notEndsWithValue = null;
            object endsWithValue = null;

            switch(criteriaProperty.Name) {
                case "Callsign":
                    criteriaValue = new FilterString("BC") { Condition = FilterCondition.EndsWith, ReverseCondition = reverseCondition };
                    defaultValue = aircraftPropertyName == "ModeS" ? "" : null;
                    notEndsWithValue = "DEF";
                    endsWithValue = "ABC";
                    break;
                case "Registration":
                case "Icao":
                case "Operator":
                case "Country":
                case "Type":
                    flightPropertyName = null;
                    switch(criteriaProperty.Name) {
                        case "Icao":        aircraftPropertyName = "ModeS"; break;
                        case "Operator":    aircraftPropertyName = "RegisteredOwners"; break;
                        case "Country":     aircraftPropertyName = "ModeSCountry"; break;
                        case "Type":        aircraftPropertyName = "ICAOTypeCode"; break;
                        default:            aircraftPropertyName = criteriaProperty.Name; break;
                    }
                    goto case "Callsign";
                case "Date":
                case "IsEmergency":
                case "UseAlternateCallsigns":
                case "FirstAltitude":
                case "LastAltitude":
                    result = false;
                    break;
                default:
                    throw new NotImplementedException();
            }

            if(result) {
                criteriaProperty.SetValue(_Criteria, criteriaValue, null);

                if(aircraftPropertyName != null) {
                    var aircraftProperty = typeof(BaseStationAircraft).GetProperty(aircraftPropertyName);
                    aircraftProperty.SetValue(defaultFlight.Aircraft, defaultValue, null);
                    aircraftProperty.SetValue(notEndsWithFlight.Aircraft, notEndsWithValue, null);
                    aircraftProperty.SetValue(endsWithFlight.Aircraft, endsWithValue, null);
                } else {
                    var flightProperty = typeof(BaseStationFlight).GetProperty(flightPropertyName);
                    flightProperty.SetValue(defaultFlight, defaultValue, null);
                    flightProperty.SetValue(notEndsWithFlight, notEndsWithValue, null);
                    flightProperty.SetValue(endsWithFlight, endsWithValue, null);
                }
            }

            return result;
        }

        /// <summary>
        /// Sets up the flights for a range criteria test.
        /// </summary>
        /// <param name="criteriaProperty"></param>
        /// <param name="belowRangeFlight"></param>
        /// <param name="startRangeFlight"></param>
        /// <param name="inRangeFlight"></param>
        /// <param name="endRangeFlight"></param>
        /// <param name="aboveRangeFlight"></param>
        /// <param name="reverseCondition"></param>
        /// <returns></returns>
        protected bool SetRangeCriteria(PropertyInfo criteriaProperty, BaseStationFlight belowRangeFlight, BaseStationFlight startRangeFlight, BaseStationFlight inRangeFlight, BaseStationFlight endRangeFlight, BaseStationFlight aboveRangeFlight, bool reverseCondition)
        {
            bool result = true;

            switch(criteriaProperty.Name) {
                case "Date":
                    var startTime = new DateTime(2001, 2, 3, 4, 5, 6);
                    belowRangeFlight.StartTime = startTime.AddSeconds(-1);
                    _Criteria.Date.LowerValue = startRangeFlight.StartTime = startTime;
                    inRangeFlight.StartTime = startTime.AddSeconds(1);
                    _Criteria.Date.UpperValue = endRangeFlight.StartTime = startTime.AddSeconds(2);
                    aboveRangeFlight.StartTime = startTime.AddSeconds(3);

                    _Criteria.Date.ReverseCondition =reverseCondition;
                    break;
                case "FirstAltitude":
                    var firstAltitude = 100;
                    belowRangeFlight.FirstAltitude = firstAltitude - 1;
                    _Criteria.FirstAltitude.LowerValue = startRangeFlight.FirstAltitude = firstAltitude;
                    inRangeFlight.FirstAltitude = firstAltitude + 1;
                    _Criteria.FirstAltitude.UpperValue = endRangeFlight.FirstAltitude = firstAltitude + 2;
                    aboveRangeFlight.FirstAltitude = firstAltitude + 3;

                    _Criteria.FirstAltitude.ReverseCondition = reverseCondition;
                    break;
                case "LastAltitude":
                    var lastAltitude = 100;
                    belowRangeFlight.LastAltitude = lastAltitude - 1;
                    _Criteria.LastAltitude.LowerValue = startRangeFlight.LastAltitude = lastAltitude;
                    inRangeFlight.LastAltitude = lastAltitude + 1;
                    _Criteria.LastAltitude.UpperValue = endRangeFlight.LastAltitude = lastAltitude + 2;
                    aboveRangeFlight.LastAltitude = lastAltitude + 3;

                    _Criteria.LastAltitude.ReverseCondition = reverseCondition;
                    break;
                case "Callsign":
                case "IsEmergency":
                case "Operator":
                case "Registration":
                case "Icao":
                case "Country":
                case "UseAlternateCallsigns":
                case "Type":
                    result = false;
                    break;
                default:
                    throw new NotImplementedException(criteriaProperty.Name);
            }

            return result;
        }

        protected void SetSortColumnValue(BaseStationFlight flight, string sortColumn, bool isDefault, bool isHigh)
        {
            var stringValue = isDefault ? sortColumn == "reg" || sortColumn == "icao" ? "" : null : isHigh ? "B" : "A";
            var dateValue = isDefault ? default(DateTime) : isHigh ? new DateTime(2001, 1, 2) : new DateTime(2001, 1, 1);
            var intValue = isDefault ? (int?)null : isHigh ? 10 : -10;

            switch(sortColumn) {
                case "callsign":        flight.Callsign = stringValue; break;
                case "country":         flight.Aircraft.ModeSCountry = stringValue; break;
                case "date":            flight.StartTime = dateValue; break;
                case "firstaltitude":   flight.FirstAltitude = intValue; break;
                case "lastaltitude":    flight.LastAltitude = intValue; break;
                case "model":           flight.Aircraft.Type = stringValue; break;
                case "type":            flight.Aircraft.ICAOTypeCode = stringValue; break;
                case "operator":        flight.Aircraft.RegisteredOwners = stringValue; break;
                case "reg":             flight.Aircraft.Registration = stringValue; break;
                case "icao":            flight.Aircraft.ModeS = stringValue; break;
                default:                throw new NotImplementedException();
            }
        }
        #endregion

        #region Constructors and Properties
        protected void BaseStationDatabase_Constructor_Initialises_To_Known_Values_And_Properties_Work()
        {
            _Database.Dispose();
            _Database = Factory.Resolve<IBaseStationDatabase>();

            Assert.IsNotNull(_Database.Provider);
            TestUtilities.TestProperty(_Database, "Provider", _Database.Provider, _Provider.Object);

            Assert.AreEqual(null, _Database.FileName);
            Assert.IsFalse(_Database.IsConnected);
            Assert.IsFalse(_Database.WriteSupportEnabled);
        }
        #endregion

        #region GetAircraftByRegistration
        protected void BaseStationDatabase_GetAircraftByRegistration_Returns_Null_If_Passed_Null()
        {
            Assert.IsNull(_Database.GetAircraftByRegistration(null));
        }

        protected void BaseStationDatabase_GetAircraftByRegistration_Returns_Null_If_Aircraft_Does_Not_Exist()
        {
            Assert.IsNull(_Database.GetAircraftByRegistration("REG"));
        }

        protected void BaseStationDatabase_GetAircraftByRegistration_Returns_Aircraft_Object_For_Registration()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            var mockAircraft = LoadAircraftFromSpreadsheet(worksheet);

            var id = AddAircraft(mockAircraft);

            var aircraft = _Database.GetAircraftByRegistration(mockAircraft.Registration);
            Assert.AreNotSame(aircraft, mockAircraft);

            AssertAircraftAreEqual(mockAircraft, aircraft, id);
        }
        #endregion

        #region GetAircraftByCode
        protected void BaseStationDatatbase_GetAircraftByCode_Returns_Null_If_Passed_Null()
        {
            Assert.IsNull(_Database.GetAircraftByCode(null));
        }

        protected void BaseStationDatabase_GetAircraftByCode_Returns_Null_If_Aircraft_Does_Not_Exist()
        {
            Assert.IsNull(_Database.GetAircraftByCode("ABC123"));
        }

        protected void BaseStationDatabase_GetAircraftByCode_Returns_Null_If_File_Not_Configured()
        {
            _Database.FileName = null;
            Assert.IsNull(_Database.GetAircraftByCode("ABC123"));
        }

        protected void BaseStationDatabase_GetAircraftByCode_Returns_Aircraft_Object_For_ICAO24_Code()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            var mockAircraft = LoadAircraftFromSpreadsheet(worksheet);

            var id = AddAircraft(mockAircraft);

            var aircraft = _Database.GetAircraftByCode(mockAircraft.ModeS);
            Assert.AreNotSame(aircraft, mockAircraft);

            AssertAircraftAreEqual(mockAircraft, aircraft, id);
        }
        #endregion

        #region GetManyAircraftByCode
        protected void BaseStationDatatbase_GetManyAircraftByCode_Returns_Empty_Collection_If_Passed_Null()
        {
            Assert.AreEqual(0, _Database.GetManyAircraftByCode(null).Count);
        }

        protected void BaseStationDatabase_GetManyAircraftByCode_Returns_Empty_Collection_If_Aircraft_Does_Not_Exist()
        {
            Assert.AreEqual(0, _Database.GetManyAircraftByCode(new string[] { "ABC123" }).Count);
        }

        protected void BaseStationDatabase_GetManyAircraftByCode_Returns_Aircraft_Object_For_ICAO24_Code()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            var mockAircraft = LoadAircraftFromSpreadsheet(worksheet);

            var id = AddAircraft(mockAircraft);

            var manyAircraft = _Database.GetManyAircraftByCode(new string[] { mockAircraft.ModeS });
            Assert.AreEqual(1, manyAircraft.Count);

            var aircraft = manyAircraft.First().Value;
            Assert.AreNotSame(aircraft, mockAircraft);

            AssertAircraftAreEqual(mockAircraft, aircraft, id);
        }

        protected void BaseStationDatabase_GetManyAircraftByCode_Can_Return_More_Than_One_Aircraft()
        {
            var flight1 = CreateFlight("ABC123", setRegistration: true);
            var flight2 = CreateFlight("EFG456", setRegistration: true);
            var flight3 = CreateFlight("XYZ789", setRegistration: true);

            AddFlight(flight1);
            AddFlight(flight2);
            AddFlight(flight3);

            var firstAndLast = _Database.GetManyAircraftByCode(new string[] { "ABC123", "XYZ789" });

            Assert.AreEqual(2, firstAndLast.Count);
            Assert.IsTrue(firstAndLast.Where(r => r.Value.Registration == "ABC123").Any());
            Assert.IsTrue(firstAndLast.Where(r => r.Value.Registration == "XYZ789").Any());
        }

        protected void BaseStationDatabase_GetManyAircraftByCode_Transparently_Handles_Call_Splitting_When_Number_Of_Icaos_Exceeds_MaxParameters()
        {
            var flight1 = CreateFlight("ABC123", setRegistration: true);
            var flight2 = CreateFlight("XYZ789", setRegistration: true);

            AddAircraft(flight1.Aircraft);
            AddAircraft(flight2.Aircraft);

            AddFlight(flight1);
            AddFlight(flight2);

            var icaos = new string[_Database.MaxParameters + 1];
            for(var i = 0;i < icaos.Length;++i) {
                icaos[i] = "";
            }
            icaos[0] = "ABC123";
            icaos[icaos.Length - 1] = "XYZ789";

            var allAircraft = _Database.GetManyAircraftByCode(icaos);

            Assert.AreEqual(2, allAircraft.Count);
            Assert.IsNotNull(allAircraft["ABC123"]);
            Assert.IsNotNull(allAircraft["XYZ789"]);
        }
        #endregion

        #region GetManyAircraftAndFlightsCountByCode
        protected void BaseStationDatatbase_GetManyAircraftAndFlightsCountByCode_Returns_Empty_Collection_If_Passed_Null()
        {
            Assert.AreEqual(0, _Database.GetManyAircraftAndFlightsCountByCode(null).Count);
        }

        protected void BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Returns_Empty_Collection_If_Aircraft_Does_Not_Exist()
        {
            Assert.AreEqual(0, _Database.GetManyAircraftAndFlightsCountByCode(new string[] { "ABC123" }).Count);
        }

        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        protected void BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Returns_Aircraft_Object_For_ICAO24_Code()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            var mockAircraft = LoadAircraftFromSpreadsheet(worksheet);

            var id = AddAircraft(mockAircraft);

            var manyAircraft = _Database.GetManyAircraftAndFlightsCountByCode(new string[] { mockAircraft.ModeS });
            Assert.AreEqual(1, manyAircraft.Count);

            var aircraft = manyAircraft.First().Value;
            Assert.AreNotSame(aircraft, mockAircraft);

            AssertAircraftAreEqual(mockAircraft, aircraft, id);
        }

        protected void BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Can_Return_More_Than_One_Aircraft()
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

            var firstAndLast = _Database.GetManyAircraftAndFlightsCountByCode(new string[] { "ABC123", "XYZ789" });

            Assert.AreEqual(2, firstAndLast.Count);
            Assert.IsTrue(firstAndLast.Where(r => r.Value.Registration == "ABC123").Any());
            Assert.IsTrue(firstAndLast.Where(r => r.Value.Registration == "XYZ789").Any());
        }

        protected void BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Returns_Counts_Of_Flights()
        {
            var flight1 = CreateFlight("ABC123", setRegistration: true);
            flight1.AircraftID = (int)AddAircraft(flight1.Aircraft);
            var flight2 = CreateFlight(flight1.Aircraft, "XYZ999");

            AddFlight(flight1);
            AddFlight(flight2);

            var allAircraft = _Database.GetManyAircraftAndFlightsCountByCode(new string[] { "ABC123" });

            Assert.AreEqual(1, allAircraft.Count);
            Assert.AreEqual(2, allAircraft.First().Value.FlightsCount);
        }

        protected void BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Transparently_Handles_Call_Splitting_When_Number_Of_Icaos_Exceeds_MaxParameters()
        {
            var flight1 = CreateFlight("ABC123", setRegistration: true);
            var flight2 = CreateFlight("XYZ789", setRegistration: true);

            AddAircraft(flight1.Aircraft);
            AddAircraft(flight2.Aircraft);

            AddFlight(flight1);
            AddFlight(flight2);

            var icaos = new string[_Database.MaxParameters + 1];
            icaos[0] = "ABC123";
            icaos[icaos.Length - 1] = "XYZ789";

            var allAircraft = _Database.GetManyAircraftAndFlightsCountByCode(icaos);

            Assert.AreEqual(2, allAircraft.Count);
            Assert.IsNotNull(allAircraft["ABC123"]);
            Assert.IsNotNull(allAircraft["XYZ789"]);
        }
        #endregion

        #region GetAircraftById
        protected void BaseStationDatabase_GetAircraftById_Returns_Null_If_Aircraft_Does_Not_Exist()
        {
            Assert.IsNull(_Database.GetAircraftById(1));
        }

        protected void BaseStationDatabase_GetAircraftById_Returns_Aircraft_Object_For_Record_Identifier()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            var mockAircraft = LoadAircraftFromSpreadsheet(worksheet);

            var id = (int)AddAircraft(mockAircraft);

            var aircraft = _Database.GetAircraftById(id);
            Assert.AreNotSame(aircraft, mockAircraft);

            AssertAircraftAreEqual(mockAircraft, aircraft, id);
        }
        #endregion

        #region InsertAircraft
        protected void BaseStationDatabase_InsertAircraft_Throws_If_Writes_Disabled()
        {
            _Database.InsertAircraft(new BaseStationAircraft() { ModeS = "123456" });
        }

        protected void BaseStationDatabase_InsertAircraft_Correctly_Inserts_Record()
        {
            _Database.WriteSupportEnabled = true;

            var worksheet = new ExcelWorksheetData(TestContext);
            var aircraft = LoadAircraftFromSpreadsheet(worksheet);

            _Database.InsertAircraft(aircraft);
            Assert.AreNotEqual(0, aircraft.AircraftID);

            var readBack = _Database.GetAircraftById(aircraft.AircraftID);
            AssertAircraftAreEqual(aircraft, readBack);
        }

        protected void BaseStationDatabase_InsertAircraft_Works_For_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    RunTestCleanup();
                    RunTestInitialise();

                    try {
                        BaseStationDatabase_InsertAircraft_Correctly_Inserts_Record();
                    } catch(Exception ex) {
                        throw new InvalidOperationException($"Exception thrown when culture was {culture}", ex);
                    }
                }
            }
        }
        #endregion

        #region GetOrInsertAircraftByCode
        protected void BaseStationDatabase_GetOrInsertAircraftByCode_Throws_If_Writes_Disabled()
        {
            _Database.GetOrInsertAircraftByCode("123456", out var created);
        }

        protected void BaseStationDatabase_GetOrInsertAircraftByCode_Returns_Record_If_It_Exists()
        {
            _Database.WriteSupportEnabled = true;
            _Database.InsertAircraft(new BaseStationAircraft() { ModeS = "123456" });

            var result = _Database.GetOrInsertAircraftByCode("123456", out var created);

            Assert.AreNotEqual(0, result.AircraftID);
            Assert.AreEqual("123456", result.ModeS);
            Assert.AreEqual(false, created);
        }

        protected void BaseStationDatabase_GetOrInsertAircraftByCode_Correctly_Inserts_Record()
        {
            _Database.WriteSupportEnabled = true;

            var aircraft = _Database.GetOrInsertAircraftByCode("Abc123", out bool created);
            Assert.AreNotEqual(0, aircraft.AircraftID);

            var readBack = _Database.GetAircraftById(aircraft.AircraftID);
            AssertAircraftAreEqual(new BaseStationAircraft() {
                AircraftID =    aircraft.AircraftID,
                FirstCreated =  TruncateDate(_Clock.LocalNowValue),
                LastModified =  TruncateDate(_Clock.LocalNowValue),
                ModeS =         "Abc123",
                ModeSCountry =  "United Kingdom",
            }, readBack);
            Assert.AreEqual(true, created);
        }

        protected void BaseStationDatabase_GetOrInsertAircraftByCode_Looks_Up_ModeSCountry()
        {
            _Icao24CodeBlock.Country = "USA";

            _Database.WriteSupportEnabled = true;

            var aircraft = _Database.GetOrInsertAircraftByCode("abc123", out bool created);
            Assert.AreEqual("USA", aircraft.ModeSCountry);
        }

        protected void BaseStationDatabase_GetOrInsertAircraftByCode_Deals_With_Null_CodeBlock()
        {
            _StandingDataManager.Setup(r => r.FindCodeBlock("abc123")).Returns((CodeBlock)null);

            _Database.WriteSupportEnabled = true;

            var aircraft = _Database.GetOrInsertAircraftByCode("abc123", out bool created);
            Assert.IsNull(aircraft.ModeSCountry);
        }

        protected void BaseStationDatabase_GetOrInsertAircraftByCode_Deals_With_Null_Country()
        {
            _Icao24CodeBlock.Country = null;

            _Database.WriteSupportEnabled = true;
            var aircraft = _Database.GetOrInsertAircraftByCode("abc123", out bool created);
            Assert.IsNull(aircraft.ModeSCountry);
        }

        protected void BaseStationDatabase_GetOrInsertAircraftByCode_Deals_With_Unknown_Country()
        {
            _Icao24CodeBlock.Country = "Unknown Country";

            _Database.WriteSupportEnabled = true;
            var aircraft = _Database.GetOrInsertAircraftByCode("abc123", out bool created);
            Assert.IsNull(aircraft.ModeSCountry);
        }

        protected void BaseStationDatabase_GetOrInsertAircraftByCode_Truncates_Milliseconds_From_Date()
        {
            _Database.WriteSupportEnabled = true;

            var time = new DateTime(2001, 2, 3, 4, 5, 6, 789);
            _Clock.LocalNowValue = time;

            _Database.GetOrInsertAircraftByCode("X", out var created);
            var readBack = _Database.GetAircraftByCode("X");
            Assert.AreEqual(TruncateDate(time), readBack.FirstCreated);
            Assert.AreEqual(TruncateDate(time), readBack.LastModified);
        }
        #endregion

        #region UpdateAircraft
        protected void BaseStationDatabase_UpdateAircraft_Throws_If_Writes_Disabled()
        {
            var aircraft = new BaseStationAircraft() { ModeS = "X" };
            aircraft.AircraftID = (int)AddAircraft(aircraft);

            aircraft.Registration = "C";
            _Database.UpdateAircraft(aircraft);
        }

        protected void BaseStationDatabase_UpdateAircraft_Raises_AircraftUpdated()
        {
            _Database.WriteSupportEnabled = true;

            var aircraft = new BaseStationAircraft() { ModeS = "X" };

            _Database.AircraftUpdated += _AircraftUpdatedEvent.Handler;
            _AircraftUpdatedEvent.EventRaised += (sender, args) => {
                Assert.AreSame(aircraft, args.Value);
            };

            _Database.InsertAircraft(aircraft);
            Assert.AreEqual(0, _AircraftUpdatedEvent.CallCount);

            _Database.UpdateAircraft(aircraft);
            Assert.AreEqual(1, _AircraftUpdatedEvent.CallCount);
            Assert.AreSame(_Database, _AircraftUpdatedEvent.Sender);
        }

        protected void BaseStationDatabase_UpdateAircraft_Correctly_Updates_Record()
        {
            _Database.WriteSupportEnabled = true;
            var worksheet = new ExcelWorksheetData(TestContext);

            var id = (int)AddAircraft(new BaseStationAircraft() { ModeS = "ZZZZZZ" });

            var update = _Database.GetAircraftById(id);
            LoadAircraftFromSpreadsheet(worksheet, 0, update);

            _Database.UpdateAircraft(update);

            var readBack = _Database.GetAircraftById(id);
            AssertAircraftAreEqual(update, readBack, id);
        }

        protected void BaseStationDatabase_UpdateAircraft_Works_For_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    RunTestCleanup();
                    RunTestInitialise();

                    try {
                        BaseStationDatabase_UpdateAircraft_Correctly_Updates_Record();
                    } catch(Exception ex) {
                        throw new InvalidOperationException($"Exception thrown when culture was {culture}", ex);
                    }
                }
            }
        }
        #endregion

        #region UpdateAircraftModeSCountry
        protected void BaseStationDatabase_UpdateAircraftModeSCountry_Throws_If_Writes_Disabled()
        {
            var aircraft = new BaseStationAircraft() { ModeS = "X" };
            aircraft.AircraftID = (int)AddAircraft(aircraft);

            aircraft.Registration = "C";
            _Database.UpdateAircraftModeSCountry(aircraft.AircraftID, "X");
        }

        protected void BaseStationDatabase_UpdateAircraftModeSCountry_Updates_ModeSCountry_For_Existing_Record()
        {
            _Database.WriteSupportEnabled = true;
            var worksheet = new ExcelWorksheetData(TestContext);

            var id = (int)AddAircraft(new BaseStationAircraft() { ModeS = "ZZZZZZ" });

            var update = _Database.GetAircraftById(id);
            LoadAircraftFromSpreadsheet(worksheet, 0, update);

            _Database.UpdateAircraft(update);

            update.ModeSCountry = "Updated Mode-S Country";
            _Database.UpdateAircraftModeSCountry(update.AircraftID, update.ModeSCountry);

            var readBack = _Database.GetAircraftById(id);
            AssertAircraftAreEqual(update, readBack, id);

            update.ModeSCountry = null;
            _Database.UpdateAircraftModeSCountry(update.AircraftID, update.ModeSCountry);

            readBack = _Database.GetAircraftById(id);
            AssertAircraftAreEqual(update, readBack, id);
        }
        #endregion

        #region RecordMissingAircraft
        protected void BaseStationDatabase_RecordMissingAircraft_Throws_Exception_If_Writes_Not_Enabled()
        {
            _Database.RecordMissingAircraft("123456");
        }

        protected void BaseStationDatabase_RecordMissingAircraft_Creates_Almost_Empty_Aircraft_Record()
        {
            _Database.WriteSupportEnabled = true;

            _Database.RecordMissingAircraft("123456");

            var aircraft = _Database.GetAircraftByCode("123456");
            Assert.IsNotNull(aircraft);
            Assert.AreEqual("123456", aircraft.ModeS);
            Assert.AreEqual("Missing", aircraft.UserString1);
            Assert.AreEqual(TruncateDate(_Clock.LocalNowValue), aircraft.FirstCreated);
            Assert.AreEqual(TruncateDate(_Clock.LocalNowValue), aircraft.LastModified);
            Assert.IsNull(aircraft.Registration);
            Assert.IsNull(aircraft.Manufacturer);
            Assert.IsNull(aircraft.Type);
            Assert.IsNull(aircraft.RegisteredOwners);
        }

        protected void BaseStationDatabase_RecordMissingAircraft_Updates_Existing_Empty_Records()
        {
            _Database.WriteSupportEnabled = true;
            _Database.RecordMissingAircraft("123456");

            var createdDate = _Clock.LocalNowValue;
            _Clock.AddMilliseconds(60000);
            _Database.RecordMissingAircraft("123456");

            var aircraft = _Database.GetAircraftByCode("123456");
            Assert.AreEqual(TruncateDate(createdDate),          aircraft.FirstCreated);
            Assert.AreEqual(TruncateDate(_Clock.LocalNowValue), aircraft.LastModified);
        }

        protected void BaseStationDatabase_RecordMissingAircraft_Updates_Existing_Empty_Records_With_Wrong_UserString1()
        {
            _Database.WriteSupportEnabled = true;
            _Database.InsertAircraft(new BaseStationAircraft() { ModeS = "123456", FirstCreated = _Clock.LocalNowValue, LastModified = _Clock.LocalNowValue, });

            var createdDate = _Clock.LocalNowValue;
            _Clock.AddMilliseconds(60000);
            _Database.RecordMissingAircraft("123456");

            var aircraft = _Database.GetAircraftByCode("123456");
            Assert.AreEqual("Missing",                          aircraft.UserString1);
            Assert.AreEqual(TruncateDate(createdDate),          aircraft.FirstCreated);
            Assert.AreEqual(TruncateDate(_Clock.LocalNowValue), aircraft.LastModified);
        }

        protected void BaseStationDatabase_RecordMissingAircraft_Only_Updates_Time_On_Existing_Records_With_Values()
        {
            foreach(var property in new String[] { "Registration", "Manufacturer", "Model", "Operator" }) {
                RunTestCleanup();
                RunTestInitialise();

                _Database.WriteSupportEnabled = true;
                var aircraft = new BaseStationAircraft() { ModeS = "123456", UserString1 = "something", FirstCreated = _Clock.LocalNowValue, LastModified = _Clock.LocalNowValue, };
                switch(property) {
                    case "Registration":    aircraft.Registration = "A"; break;
                    case "Manufacturer":    aircraft.Manufacturer = "A"; break;
                    case "Model":           aircraft.Type = "A"; break;
                    case "Operator":        aircraft.RegisteredOwners = "A"; break;
                    default:                throw new NotImplementedException();
                }
                _Database.InsertAircraft(aircraft);

                var createdDate = _Clock.LocalNowValue;
                _Clock.AddMilliseconds(60000);
                _Database.RecordMissingAircraft("123456");

                aircraft = _Database.GetAircraftByCode("123456");
                Assert.AreEqual("something",                        aircraft.UserString1);
                Assert.AreEqual(TruncateDate(createdDate),          aircraft.FirstCreated);
                Assert.AreEqual(TruncateDate(_Clock.LocalNowValue), aircraft.LastModified);
            }
        }
        #endregion

        #region RecordManyMissingAircraft
        protected void BaseStationDatabase_RecordManyMissingAircraft_Throws_Exception_If_Writes_Not_Enabled()
        {
            _Database.RecordManyMissingAircraft(new string[] { "A", "B" });
        }

        protected void BaseStationDatabase_RecordManyMissingAircraft_Creates_Almost_Empty_Aircraft_Record()
        {
            _Database.WriteSupportEnabled = true;

            _Database.RecordManyMissingAircraft(new string[] { "A", "B" });

            var aircraft = _Database.GetAircraftByCode("A");
            Assert.IsNotNull(aircraft);
            Assert.AreEqual("A",                                aircraft.ModeS);
            Assert.AreEqual("Missing",                          aircraft.UserString1);
            Assert.AreEqual(TruncateDate(_Clock.LocalNowValue), aircraft.FirstCreated);
            Assert.AreEqual(TruncateDate(_Clock.LocalNowValue), aircraft.LastModified);

            aircraft = _Database.GetAircraftByCode("B");
            Assert.IsNotNull(aircraft);
            Assert.AreEqual("B",                                aircraft.ModeS);
            Assert.AreEqual("Missing",                          aircraft.UserString1);
            Assert.AreEqual(TruncateDate(_Clock.LocalNowValue), aircraft.FirstCreated);
            Assert.AreEqual(TruncateDate(_Clock.LocalNowValue), aircraft.LastModified);
        }

        protected void BaseStationDatabase_RecordManyMissingAircraft_Updates_Existing_Empty_Records()
        {
            _Database.WriteSupportEnabled = true;
            _Database.RecordManyMissingAircraft(new string[] { "123456" });

            var createdDate = _Clock.LocalNowValue;
            _Clock.AddMilliseconds(60000);
            _Database.RecordManyMissingAircraft(new string[] { "123456" });

            var aircraft = _Database.GetAircraftByCode("123456");
            Assert.AreEqual(TruncateDate(createdDate),          aircraft.FirstCreated);
            Assert.AreEqual(TruncateDate(_Clock.LocalNowValue), aircraft.LastModified);
        }

        protected void BaseStationDatabase_RecordManyMissingAircraft_Only_Updates_LastModified_Time_On_Existing_Records_With_Registrations()
        {
            _Database.WriteSupportEnabled = true;
            _Database.InsertAircraft(new BaseStationAircraft() { ModeS = "123456", Registration = "A", FirstCreated = _Clock.LocalNowValue, LastModified = _Clock.LocalNowValue });

            var createdDate = _Clock.LocalNowValue;
            _Clock.AddMilliseconds(60000);
            _Database.RecordManyMissingAircraft(new string[] { "123456" });

            var aircraft = _Database.GetAircraftByCode("123456");
            Assert.AreEqual("A",                                aircraft.Registration);
            Assert.AreEqual(TruncateDate(createdDate),          aircraft.FirstCreated);
            Assert.AreEqual(TruncateDate(_Clock.LocalNowValue), aircraft.LastModified);
        }
        #endregion

        #region UpsertAircraftLookup
        protected void BaseStationDatabase_UpsertAircraftLookup_Throws_Exception_If_Writes_Not_Enabled()
        {
            _Database.UpsertAircraftLookup(new BaseStationAircraftUpsertLookup() { ModeS = "123456", }, false);
        }

        protected void BaseStationDatabase_UpsertAircraftLookup_Inserts_New_Lookups()
        {
            _Database.WriteSupportEnabled = true;

            var now = new DateTime(2001, 2, 3, 4, 5, 6, 789);
            _Database.UpsertAircraftLookup(new BaseStationAircraftUpsertLookup() {
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

            var aircraft = _Database.GetAircraftByCode("123456");
            Assert.IsNotNull(aircraft);

            Assert.AreEqual("123456",           aircraft.ModeS);
            Assert.AreEqual("UK",               aircraft.Country);
            Assert.AreEqual("A380",             aircraft.ICAOTypeCode);
            Assert.AreEqual(TruncateDate(now),  aircraft.FirstCreated);
            Assert.AreEqual(TruncateDate(now),  aircraft.LastModified);
            Assert.AreEqual("Airbus",           aircraft.Manufacturer);
            Assert.AreEqual("France",           aircraft.ModeSCountry);
            Assert.AreEqual("TWA",              aircraft.OperatorFlagCode);
            Assert.AreEqual("Transworld",       aircraft.RegisteredOwners);
            Assert.AreEqual("G-ABCD",           aircraft.Registration);
            Assert.AreEqual("9182",             aircraft.SerialNo);
            Assert.AreEqual("Big Plane",        aircraft.Type);
            Assert.AreEqual("1992",             aircraft.YearBuilt);
        }

        protected void BaseStationDatabase_UpsertAircraftLookup_Updates_Existing_Aircraft()
        {
            var createdDate = new DateTime(1999, 8, 7, 6, 5, 4, 321);
            var now = new DateTime(2001, 2, 3, 4, 5, 6, 789);

            _Database.WriteSupportEnabled = true;
            _Database.InsertAircraft(new BaseStationAircraft() {
                ModeS =         "123456",
                Registration =  "N12345",
                FirstCreated =  createdDate,
                LastModified =  createdDate,
            });

            _Database.UpsertAircraftLookup(new BaseStationAircraftUpsertLookup() {
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

            var aircraft = _Database.GetAircraftByCode("123456");
            Assert.IsNotNull(aircraft);

            Assert.AreEqual("123456",                   aircraft.ModeS);
            Assert.AreEqual("Germany",                  aircraft.Country);
            Assert.AreEqual("B747",                     aircraft.ICAOTypeCode);
            Assert.AreEqual(TruncateDate(createdDate),  aircraft.FirstCreated);
            Assert.AreEqual(TruncateDate(now),          aircraft.LastModified);
            Assert.AreEqual("Boeing",                   aircraft.Manufacturer);
            Assert.AreEqual("USA",                      aircraft.ModeSCountry);
            Assert.AreEqual("BAW",                      aircraft.OperatorFlagCode);
            Assert.AreEqual("British Airways",          aircraft.RegisteredOwners);
            Assert.AreEqual("D-WXYZ",                   aircraft.Registration);
            Assert.AreEqual("00119",                    aircraft.SerialNo);
            Assert.AreEqual("Big Jobs",                 aircraft.Type);
            Assert.AreEqual("1979",                     aircraft.YearBuilt);
        }

        protected void BaseStationDatabase_UpsertAircraftLookup_Can_Ignore_Existing_Aircraft_When_Required()
        {
            var createdDate = new DateTime(1999, 8, 7, 6, 5, 4, 321);
            var now = new DateTime(2001, 2, 3, 4, 5, 6, 789);

            _Database.WriteSupportEnabled = true;
            _Database.InsertAircraft(new BaseStationAircraft() {
                ModeS =             "123456",
                Registration =      "G-ABCD",
                FirstCreated =      createdDate,
                LastModified =      createdDate,
            });

            _Database.UpsertAircraftLookup(new BaseStationAircraftUpsertLookup() {
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

            var aircraft = _Database.GetAircraftByCode("123456");
            Assert.IsNotNull(aircraft);

            Assert.AreEqual("123456",                   aircraft.ModeS);
            Assert.IsNull(                              aircraft.Country);
            Assert.IsNull(                              aircraft.ICAOTypeCode);
            Assert.AreEqual(TruncateDate(createdDate),  aircraft.FirstCreated);
            Assert.AreEqual(TruncateDate(createdDate),  aircraft.LastModified);
            Assert.IsNull(                              aircraft.Manufacturer);
            Assert.IsNull(                              aircraft.ModeSCountry);
            Assert.IsNull(                              aircraft.OperatorFlagCode);
            Assert.IsNull(                              aircraft.RegisteredOwners);
            Assert.AreEqual("G-ABCD",                   aircraft.Registration);
            Assert.IsNull(                              aircraft.SerialNo);
            Assert.IsNull(                              aircraft.Type);
            Assert.IsNull(                              aircraft.YearBuilt);
        }

        protected void BaseStationDatabase_UpsertAircraftLookup_Will_Overwrite_Missing_Aircraft_When_Required()
        {
            var createdDate = new DateTime(1999, 8, 7, 6, 5, 4, 321);
            var now = new DateTime(2001, 2, 3, 4, 5, 6, 789);

            _Database.WriteSupportEnabled = true;
            _Database.InsertAircraft(new BaseStationAircraft() {
                ModeS =             "123456",
                UserString1 =       "Missing",
            });

            _Database.UpsertAircraftLookup(new BaseStationAircraftUpsertLookup() {
                ModeS =             "123456",
                Registration =      "D-WXYZ",
            }, onlyUpdateIfMarkedAsMissing: true);

            var aircraft = _Database.GetAircraftByCode("123456");
            Assert.IsNotNull(aircraft);

            Assert.AreEqual("123456", aircraft.ModeS);
            Assert.AreEqual("D-WXYZ", aircraft.Registration);
        }

        protected void BaseStationDatabase_UpsertAircraftLookup_Will_Not_Consider_Aircraft_Missing_If_They_Have_Registration()
        {
            var createdDate = new DateTime(1999, 8, 7, 6, 5, 4, 321);
            var now = new DateTime(2001, 2, 3, 4, 5, 6, 789);

            _Database.WriteSupportEnabled = true;
            _Database.InsertAircraft(new BaseStationAircraft() {
                ModeS =             "123456",
                Registration =      "G-ABCD",
                UserString1 =       "Missing",
            });

            _Database.UpsertAircraftLookup(new BaseStationAircraftUpsertLookup() {
                ModeS =             "123456",
                Registration =      "D-WXYZ",
            }, onlyUpdateIfMarkedAsMissing: true);

            var aircraft = _Database.GetAircraftByCode("123456");
            Assert.IsNotNull(aircraft);

            Assert.AreEqual("123456", aircraft.ModeS);
            Assert.AreEqual("G-ABCD", aircraft.Registration);
        }

        protected void BaseStationDatabase_UpsertAircraftLookup_Raises_AircraftUpdated_On_Update()
        {
            _Database.WriteSupportEnabled = true;
            _Database.InsertAircraft(new BaseStationAircraft() { ModeS = "123456", Registration = "ABC" });
            _Database.AircraftUpdated += _AircraftUpdatedEvent.Handler;

            _Database.UpsertAircraftLookup(new BaseStationAircraftUpsertLookup() { ModeS = "123456", Registration = "XYZ", }, false);

            Assert.AreEqual(1, _AircraftUpdatedEvent.CallCount);
            Assert.AreEqual("123456", _AircraftUpdatedEvent.Args.Value.ModeS);
            Assert.AreEqual("XYZ", _AircraftUpdatedEvent.Args.Value.Registration);
        }

        protected void BaseStationDatabase_UpsertAircraftLookup_Does_Not_Raise_AircraftUpdated_On_Insert()
        {
            _Database.WriteSupportEnabled = true;
            _Database.AircraftUpdated += _AircraftUpdatedEvent.Handler;

            _Database.UpsertAircraftLookup(new BaseStationAircraftUpsertLookup() { ModeS = "123456", Registration = "XYZ", }, false);

            Assert.AreEqual(0, _AircraftUpdatedEvent.CallCount);
        }
        #endregion

        #region UpsertManyAircraft
        protected void BaseStationDatabase_UpsertManyAircraft_LookupVersion_Throws_Exception_If_Writes_Not_Enabled()
        {
            _Database.UpsertManyAircraftLookup(new BaseStationAircraftUpsertLookup[] {
                new BaseStationAircraftUpsertLookup() { ModeS = "A" },
            }, false);
        }

        protected void BaseStationDatabase_UpsertManyAircraft_FullVersion_Throws_Exception_If_Writes_Not_Enabled()
        {
            _Database.UpsertManyAircraft(new BaseStationAircraftUpsert[] {
                new BaseStationAircraftUpsert() { ModeS = "A" },
            });
        }

        protected void BaseStationDatabase_UpsertManyAircraft_Inserts_New_Lookups()
        {
            _Database.WriteSupportEnabled = true;

            var now = new DateTime(2001, 2, 3, 4, 5, 6, 789);
            _Database.UpsertManyAircraftLookup(new BaseStationAircraftUpsertLookup[] {
                new BaseStationAircraftUpsertLookup() {
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
                new BaseStationAircraftUpsertLookup() {
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

            var aircraft = _Database.GetAircraftByCode("123456");
            Assert.IsNotNull(aircraft);
            Assert.AreEqual("123456",           aircraft.ModeS);
            Assert.AreEqual("UK",               aircraft.Country);
            Assert.AreEqual("A380",             aircraft.ICAOTypeCode);
            Assert.AreEqual(TruncateDate(now),  aircraft.FirstCreated);
            Assert.AreEqual(TruncateDate(now),  aircraft.LastModified);
            Assert.AreEqual("Airbus",           aircraft.Manufacturer);
            Assert.AreEqual("France",           aircraft.ModeSCountry);
            Assert.AreEqual("TWA",              aircraft.OperatorFlagCode);
            Assert.AreEqual("Transworld",       aircraft.RegisteredOwners);
            Assert.AreEqual("G-ABCD",           aircraft.Registration);
            Assert.AreEqual("9182",             aircraft.SerialNo);
            Assert.AreEqual("Big Plane",        aircraft.Type);
            Assert.AreEqual("1992",             aircraft.YearBuilt);

            aircraft = _Database.GetAircraftByCode("789ABC");
            Assert.IsNotNull(aircraft);
            Assert.AreEqual("789ABC",           aircraft.ModeS);
            Assert.AreEqual("Germany",          aircraft.Country);
            Assert.AreEqual("B747",             aircraft.ICAOTypeCode);
            Assert.AreEqual(TruncateDate(now),  aircraft.FirstCreated);
            Assert.AreEqual(TruncateDate(now),  aircraft.LastModified);
            Assert.AreEqual("Boeing",           aircraft.Manufacturer);
            Assert.AreEqual("USA",              aircraft.ModeSCountry);
            Assert.AreEqual("BAW",              aircraft.OperatorFlagCode);
            Assert.AreEqual("British Airways",  aircraft.RegisteredOwners);
            Assert.AreEqual("D-WXYZ",           aircraft.Registration);
            Assert.AreEqual("00119",            aircraft.SerialNo);
            Assert.AreEqual("Big Jobs",         aircraft.Type);
            Assert.AreEqual("1979",             aircraft.YearBuilt);
        }

        protected void BaseStationDatabase_UpsertManyAircraft_Inserts_New_Aircraft()
        {
            _Database.WriteSupportEnabled = true;

            var now = new DateTime(2001, 2, 3, 4, 5, 6, 789);
            _Database.UpsertManyAircraft(new BaseStationAircraftUpsert[] {
                new BaseStationAircraftUpsert() {
                    ModeS =             "123456",
                    Country =           "UK",
                    FirstCreated =      now,
                    YearBuilt =         "1992",
                    UserString4 =       "Esoteric"
                },
                new BaseStationAircraftUpsert() {
                    ModeS =             "789ABC",
                    LastModified =      now,
                    Manufacturer =      "Boeing",
                    UserBool2 =         true,
                },
            });

            var aircraft = _Database.GetAircraftByCode("123456");
            Assert.IsNotNull(aircraft);
            Assert.AreEqual("123456",           aircraft.ModeS);
            Assert.AreEqual("UK",               aircraft.Country);
            Assert.AreEqual(TruncateDate(now),  aircraft.FirstCreated);
            Assert.AreEqual("1992",             aircraft.YearBuilt);
            Assert.AreEqual("Esoteric",         aircraft.UserString4);

            aircraft = _Database.GetAircraftByCode("789ABC");
            Assert.IsNotNull(aircraft);
            Assert.AreEqual("789ABC",           aircraft.ModeS);
            Assert.AreEqual(TruncateDate(now),  aircraft.LastModified);
            Assert.AreEqual("Boeing",           aircraft.Manufacturer);
            Assert.AreEqual(true,               aircraft.UserBool2);
        }

        protected void BaseStationDatabase_UpsertManyAircraft_Updates_Existing_Lookups()
        {
            var createdDate = new DateTime(1999, 8, 7, 6, 5, 4, 321);
            var now = new DateTime(2001, 2, 3, 4, 5, 6, 789);

            _Database.WriteSupportEnabled = true;
            _Database.InsertAircraft(new BaseStationAircraft() {
                ModeS =         "123456",
                Registration =  "N12345",
                FirstCreated =  createdDate,
                LastModified =  createdDate,
            });

            _Database.UpsertManyAircraftLookup(new BaseStationAircraftUpsertLookup[] {
                new BaseStationAircraftUpsertLookup() {
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

            var aircraft = _Database.GetAircraftByCode("123456");
            Assert.IsNotNull(aircraft);

            Assert.AreEqual("123456",                   aircraft.ModeS);
            Assert.AreEqual("Germany",                  aircraft.Country);
            Assert.AreEqual("B747",                     aircraft.ICAOTypeCode);
            Assert.AreEqual(TruncateDate(createdDate),  aircraft.FirstCreated);
            Assert.AreEqual(TruncateDate(now),          aircraft.LastModified);
            Assert.AreEqual("Boeing",                   aircraft.Manufacturer);
            Assert.AreEqual("USA",                      aircraft.ModeSCountry);
            Assert.AreEqual("BAW",                      aircraft.OperatorFlagCode);
            Assert.AreEqual("British Airways",          aircraft.RegisteredOwners);
            Assert.AreEqual("D-WXYZ",                   aircraft.Registration);
            Assert.AreEqual("00119",                    aircraft.SerialNo);
            Assert.AreEqual("Big Jobs",                 aircraft.Type);
            Assert.AreEqual("1979",                     aircraft.YearBuilt);
        }

        protected void BaseStationDatabase_UpsertManyAircraft_Updates_Existing_Aircraft()
        {
            var createdDate = new DateTime(1999, 8, 7, 6, 5, 4, 321);
            var now = new DateTime(2001, 2, 3, 4, 5, 6, 789);

            _Database.WriteSupportEnabled = true;
            _Database.InsertAircraft(new BaseStationAircraft() {
                ModeS =         "123456",
                Registration =  "N12345",
                FirstCreated =  createdDate,
                LastModified =  createdDate,
            });

            _Database.UpsertManyAircraft(new BaseStationAircraftUpsert[] {
                new BaseStationAircraft() {
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

            var aircraft = _Database.GetAircraftByCode("123456");
            Assert.IsNotNull(aircraft);

            Assert.AreEqual("123456",                   aircraft.ModeS);
            Assert.AreEqual("Germany",                  aircraft.Country);
            Assert.AreEqual("B747",                     aircraft.ICAOTypeCode);
            Assert.AreEqual(TruncateDate(now),          aircraft.FirstCreated);
            Assert.AreEqual(TruncateDate(createdDate),  aircraft.LastModified);
            Assert.AreEqual("Boeing",                   aircraft.Manufacturer);
            Assert.AreEqual("USA",                      aircraft.ModeSCountry);
            Assert.AreEqual("BAW",                      aircraft.OperatorFlagCode);
            Assert.AreEqual("British Airways",          aircraft.RegisteredOwners);
            Assert.AreEqual("D-WXYZ",                   aircraft.Registration);
            Assert.AreEqual("00119",                    aircraft.SerialNo);
            Assert.AreEqual("Big Jobs",                 aircraft.Type);
            Assert.AreEqual("1979",                     aircraft.YearBuilt);
            Assert.AreEqual(true,                       aircraft.UserBool4);
        }

        protected void BaseStationDatabase_UpsertManyAircraft_Raises_AircraftUpdated_On_Update_Lookup()
        {
            _Database.WriteSupportEnabled = true;
            _Database.InsertAircraft(new BaseStationAircraft() { ModeS = "AAAAAA", Registration = "---" });
            _Database.InsertAircraft(new BaseStationAircraft() { ModeS = "BBBBBB", Registration = "===" });
            _Database.AircraftUpdated += _AircraftUpdatedEvent.Handler;

            _Database.UpsertManyAircraftLookup(new BaseStationAircraftUpsertLookup[] {
                new BaseStationAircraftUpsertLookup() { ModeS = "AAAAAA", Registration = "111" },
                new BaseStationAircraftUpsertLookup() { ModeS = "BBBBBB", Registration = "222" },
            }, false);

            Assert.AreEqual(2, _AircraftUpdatedEvent.CallCount);
            Assert.AreEqual("111", _AircraftUpdatedEvent.AllArgs.Single(r => r.Value.ModeS == "AAAAAA").Value.Registration);
            Assert.AreEqual("222", _AircraftUpdatedEvent.AllArgs.Single(r => r.Value.ModeS == "BBBBBB").Value.Registration);
        }

        protected void BaseStationDatabase_UpsertManyAircraft_Raises_AircraftUpdated_On_Update_Aircraft()
        {
            _Database.WriteSupportEnabled = true;
            _Database.InsertAircraft(new BaseStationAircraft() { ModeS = "AAAAAA", Registration = "---" });
            _Database.InsertAircraft(new BaseStationAircraft() { ModeS = "BBBBBB", Registration = "===" });
            _Database.AircraftUpdated += _AircraftUpdatedEvent.Handler;

            _Database.UpsertManyAircraft(new BaseStationAircraftUpsert[] {
                new BaseStationAircraftUpsert() { ModeS = "AAAAAA", Registration = "111" },
                new BaseStationAircraftUpsert() { ModeS = "BBBBBB", Registration = "222" },
            });

            Assert.AreEqual(2, _AircraftUpdatedEvent.CallCount);
            Assert.AreEqual("111", _AircraftUpdatedEvent.AllArgs.Single(r => r.Value.ModeS == "AAAAAA").Value.Registration);
            Assert.AreEqual("222", _AircraftUpdatedEvent.AllArgs.Single(r => r.Value.ModeS == "BBBBBB").Value.Registration);
        }

        protected void BaseStationDatabase_UpsertManyAircraft_Does_Not_Raise_AircraftUpdated_On_Insert()
        {
            _Database.WriteSupportEnabled = true;
            _Database.UpsertManyAircraftLookup(new BaseStationAircraftUpsertLookup[] {
                new BaseStationAircraftUpsertLookup() { ModeS = "AAAAAA", Registration = "111" },
                new BaseStationAircraftUpsertLookup() { ModeS = "BBBBBB", Registration = "222" },
            }, false);

            Assert.AreEqual(0, _AircraftUpdatedEvent.CallCount);
        }
        #endregion

        #region DeleteAircraft
        protected void BaseStationDatabase_DeleteAircraft_Throws_If_Writes_Disabled()
        {
            var aircraft = new BaseStationAircraft() { ModeS = "X" };
            aircraft.AircraftID = (int)AddAircraft(aircraft);

            _Database.DeleteAircraft(aircraft);
        }

        protected void BaseStationDatabase_DeleteAircraft_Correctly_Deletes_Record()
        {
            _Database.WriteSupportEnabled = true;
            var worksheet = new ExcelWorksheetData(TestContext);
            var id = (int)AddAircraft(LoadAircraftFromSpreadsheet(worksheet));

            var aircraft = _Database.GetAircraftById(id);
            _Database.DeleteAircraft(aircraft);

            Assert.AreEqual(null, _Database.GetAircraftById(id));
        }
        #endregion

        #region GetFlights
        protected void BaseStationDatabase_GetFlights_Throws_If_Criteria_Is_Null()
        {
            _Database.GetFlights(null, -1, -1, null, false, null, false);
        }

        protected void BaseStationDatabase_GetFlights_Copies_Database_Record_To_Flight_Object()
        {
            Do_GetFlightsAllVersions_Copies_Database_Record_To_Flight_Object(true);
        }

        protected void Do_GetFlightsAllVersions_Copies_Database_Record_To_Flight_Object(bool getFlights)
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            var mockFlight = LoadFlightFromSpreadsheet(worksheet);

            var aircraftId = (int)AddAircraft(mockFlight.Aircraft);
            mockFlight.AircraftID = aircraftId;
            mockFlight.SessionID = PrepareSessionReference(new BaseStationSession() { StartTime = DateTime.Now }).SessionID;
            var flightId = AddFlight(mockFlight);

            List<BaseStationFlight> flights = null;
            if(getFlights) flights = _Database.GetFlights(_Criteria, -1, -1, null, false, null, false);
            else           flights = _Database.GetFlightsForAircraft(mockFlight.Aircraft, _Criteria, -1, -1, null, false, null, false);

            Assert.AreEqual(1, flights.Count);

            Assert.AreNotSame(flights[0], mockFlight);
            if(getFlights) Assert.AreNotSame(flights[0].Aircraft, mockFlight.Aircraft);
            else           Assert.AreSame(flights[0].Aircraft, mockFlight.Aircraft);

            AssertFlightsAreEqual(mockFlight, flights[0], true, aircraftId);
        }

        protected void BaseStationDatabase_GetFlights_Can_Return_List_Of_All_Flights()
        {
            var flight1 = CreateFlight("ABC123");
            var flight2 = CreateFlight("XYZ789");

            AddAircraft(flight1.Aircraft);
            AddAircraft(flight2.Aircraft);

            AddFlight(flight1);
            AddFlight(flight2);

            var flights = _Database.GetFlights(_Criteria, -1, -1, null, false, null, false);

            Assert.AreEqual(2, flights.Count);
            Assert.IsTrue(flights.Where(f => f.Callsign == "ABC123").Any());
            Assert.IsTrue(flights.Where(f => f.Callsign == "XYZ789").Any());
        }

        protected void BaseStationDatabase_GetFlights_Can_Filter_Flights_By_Equality_Criteria()
        {
            var defaultFlight = CreateFlight("default", false);
            var notEqualFlight = CreateFlight("notEquals", false);
            var equalsFlight = CreateFlight("equals", false);

            foreach(var reverseCondition in new bool[] { false, true }) {
                foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                    RunTestCleanup();
                    RunTestInitialise();

                    if(SetEqualityCriteria(criteriaProperty, defaultFlight, notEqualFlight, equalsFlight, reverseCondition)) {
                        AddFlightAndAircraft(defaultFlight);
                        AddFlightAndAircraft(notEqualFlight);
                        AddFlightAndAircraft(equalsFlight);

                        var flights = _Database.GetFlights(_Criteria, -1, -1, null, false, null, false);
                        if(!reverseCondition) {
                            Assert.AreEqual(1, flights.Count, criteriaProperty.Name);
                            Assert.AreEqual(equalsFlight.FlightID, flights[0].FlightID, criteriaProperty.Name);
                        } else {
                            var expectedCount = 1;
                            switch(criteriaProperty.Name) {
                                // NOT NULL properties will return 2 records, nullable properties will ignore nulls and just return 1
                                case "Icao":
                                case "IsEmergency":
                                    expectedCount = 2;
                                    break;
                            }
                            Assert.AreEqual(expectedCount, flights.Count, criteriaProperty.Name);
                            Assert.IsFalse(flights.Any(r => r.FlightID == equalsFlight.FlightID), criteriaProperty.Name);
                        }
                    }
                }
            }
        }

        protected void BaseStationDatabase_GetFlights_Can_Filter_Flights_By_Contains_Criteria()
        {
            var defaultFlight = CreateFlight("default", false);
            var notContainsFlight = CreateFlight("notContains", false);
            var containsFlight = CreateFlight("contains", false);

            foreach(var reverseCondition in new bool[] { false, true }) {
                foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                    RunTestCleanup();
                    RunTestInitialise();

                    if(SetContainsCriteria(criteriaProperty, defaultFlight, notContainsFlight, containsFlight, reverseCondition)) {
                        AddFlightAndAircraft(defaultFlight);
                        AddFlightAndAircraft(notContainsFlight);
                        AddFlightAndAircraft(containsFlight);

                        var flights = _Database.GetFlights(_Criteria, -1, -1, null, false, null, false);
                        if(!reverseCondition) {
                            Assert.AreEqual(1, flights.Count, criteriaProperty.Name);
                            Assert.AreEqual(containsFlight.FlightID, flights[0].FlightID, criteriaProperty.Name);
                        } else {
                            var expectedCount = 1;
                            switch(criteriaProperty.Name) {
                                // NOT NULL properties will return 2 records, nullable properties will ignore nulls and just return 1
                                case "Icao":
                                case "IsEmergency":
                                    expectedCount = 2;
                                    break;
                            }
                            Assert.AreEqual(expectedCount, flights.Count, criteriaProperty.Name);
                            Assert.IsFalse(flights.Any(r => r.FlightID == containsFlight.FlightID), criteriaProperty.Name);
                        }
                    }
                }
            }
        }

        protected void BaseStationDatabase_GetFlights_Can_Filter_Flights_By_StartsWith_Criteria()
        {
            var defaultFlight = CreateFlight("default", false);
            var notStartsWithFlight = CreateFlight("notContains", false);
            var startsWithFlight = CreateFlight("contains", false);

            foreach(var reverseCondition in new bool[] { false, true }) {
                foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                    RunTestCleanup();
                    RunTestInitialise();

                    if(SetStartsWithCriteria(criteriaProperty, defaultFlight, notStartsWithFlight, startsWithFlight, reverseCondition)) {
                        AddFlightAndAircraft(defaultFlight);
                        AddFlightAndAircraft(notStartsWithFlight);
                        AddFlightAndAircraft(startsWithFlight);

                        var flights = _Database.GetFlights(_Criteria, -1, -1, null, false, null, false);
                        if(!reverseCondition) {
                            Assert.AreEqual(1, flights.Count, criteriaProperty.Name);
                            Assert.AreEqual(startsWithFlight.FlightID, flights[0].FlightID, criteriaProperty.Name);
                        } else {
                            var expectedCount = 1;
                            switch(criteriaProperty.Name) {
                                // NOT NULL properties will return 2 records, nullable properties will ignore nulls and just return 1
                                case "Icao":
                                case "IsEmergency":
                                    expectedCount = 2;
                                    break;
                            }
                            Assert.AreEqual(expectedCount, flights.Count, criteriaProperty.Name);
                            Assert.IsFalse(flights.Any(r => r.FlightID == startsWithFlight.FlightID), criteriaProperty.Name);
                        }
                    }
                }
            }
        }

        protected void BaseStationDatabase_GetFlights_Can_Filter_Flights_By_EndsWith_Criteria()
        {
            var defaultFlight = CreateFlight("default", false);
            var notEndsWithFlight = CreateFlight("notContains", false);
            var endsWithFlight = CreateFlight("contains", false);

            foreach(var reverseCondition in new bool[] { false, true }) {
                foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                    RunTestCleanup();
                    RunTestInitialise();

                    if(SetEndsWithCriteria(criteriaProperty, defaultFlight, notEndsWithFlight, endsWithFlight, reverseCondition)) {
                        AddFlightAndAircraft(defaultFlight);
                        AddFlightAndAircraft(notEndsWithFlight);
                        AddFlightAndAircraft(endsWithFlight);

                        var flights = _Database.GetFlights(_Criteria, -1, -1, null, false, null, false);
                        if(!reverseCondition) {
                            Assert.AreEqual(1, flights.Count, criteriaProperty.Name);
                            Assert.AreEqual(endsWithFlight.FlightID, flights[0].FlightID, criteriaProperty.Name);
                        } else {
                            var expectedCount = 1;
                            switch(criteriaProperty.Name) {
                                // NOT NULL properties will return 2 records, nullable properties will ignore nulls and just return 1
                                case "Icao":
                                case "IsEmergency":
                                    expectedCount = 2;
                                    break;
                            }
                            Assert.AreEqual(expectedCount, flights.Count, criteriaProperty.Name);
                            Assert.IsFalse(flights.Any(r => r.FlightID == endsWithFlight.FlightID), criteriaProperty.Name);
                        }
                    }
                }
            }
        }

        protected void BaseStationDatabase_GetFlights_Can_Filter_Flights_By_Range_Criteria()
        {
            var belowRangeFlight = CreateFlight("belowRange");
            var startRangeFlight = CreateFlight("startRange");
            var inRangeFlight = CreateFlight("inRange");
            var endRangeFlight = CreateFlight("endRange");
            var aboveRangeFlight = CreateFlight("aboveRange");

            foreach(var reverseCondition in new bool[] { false, true }) {
                foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                    RunTestCleanup();
                    RunTestInitialise();

                    if(SetRangeCriteria(criteriaProperty, belowRangeFlight, startRangeFlight, inRangeFlight, endRangeFlight, aboveRangeFlight, reverseCondition)) {
                        AddFlightAndAircraft(belowRangeFlight);
                        AddFlightAndAircraft(startRangeFlight);
                        AddFlightAndAircraft(inRangeFlight);
                        AddFlightAndAircraft(endRangeFlight);
                        AddFlightAndAircraft(aboveRangeFlight);

                        var flights = _Database.GetFlights(_Criteria, -1, -1, null, false, null, false);
                        var message = "";
                        foreach(var flight in flights) message = String.Format("{0}{1}{2}", message, message.Length == 0 ? "" : ", " , flight.Callsign);

                        if(!reverseCondition) {
                            Assert.AreEqual(3, flights.Count, message);
                            Assert.IsTrue(flights.Where(f => f.Callsign == "startRange").Any(), message);
                            Assert.IsTrue(flights.Where(f => f.Callsign == "inRange").Any(), message);
                            Assert.IsTrue(flights.Where(f => f.Callsign == "endRange").Any(), message);
                        } else {
                            Assert.AreEqual(2, flights.Count, message);
                            Assert.IsTrue(flights.Where(f => f.Callsign == "belowRange").Any(), message);
                            Assert.IsTrue(flights.Where(f => f.Callsign == "aboveRange").Any(), message);
                        }
                    }
                }
            }
        }

        protected void BaseStationDatabase_GetFlights_String_Properties_Can_Match_Null_Or_Empty_Fields()
        {
            foreach(var reverseCondition in new bool[] { false, true }) {
                foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                    foreach(var filterValue in new string[] { null, "" }) {
                        foreach(var databaseValue in new string[] { null, "" }) {
                            if(IsFilterStringProperty(criteriaProperty)) {
                                if(criteriaProperty.Name == "Icao") continue;     // these can't be set to null on the database

                                RunTestCleanup();
                                RunTestInitialise();

                                var filter = new FilterString(filterValue) { Condition = FilterCondition.Equals, ReverseCondition = reverseCondition };
                                criteriaProperty.SetValue(_Criteria, filter, null);

                                var nullFlight = CreateFlight("nullFlight", false, false);
                                var notNullFlight = CreateFlight("notNullFlight", false, false);
                                SetStringAircraftProperty(criteriaProperty, nullFlight, databaseValue);
                                SetStringAircraftProperty(criteriaProperty, notNullFlight, "A");
                                AddFlightAndAircraft(nullFlight);
                                AddFlightAndAircraft(notNullFlight);

                                var flights = _Database.GetFlights(_Criteria, -1, -1, null, false, null, false);

                                var message = String.Format("{0}/{1}/Filter:{2}/DB:{3}", criteriaProperty.Name, reverseCondition, filterValue == null ? "null" : "empty", databaseValue == null ? "null" : "empty");
                                Assert.AreEqual(1, flights.Count, message);
                                var expected = reverseCondition ? "notNullFlight" : "nullFlight";
                                Assert.AreEqual(expected, flights[0].Aircraft.ModeS, message);
                            }
                        }
                    }
                }
            }
        }

        protected void BaseStationDatabase_GetFlights_Can_Search_For_All_Variations_Of_A_Callsign()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            var comments = worksheet.String("Comments");

            // Setup flights in database
            for(var i = 1;i <= 3;++i) {
                var flightCallsign = worksheet.EString(String.Format("Callsign{0}", i));
                if(flightCallsign != null) {
                    var flight = CreateFlight("Flight" + i.ToString());
                    flight.Callsign = flightCallsign;
                    AddFlightAndAircraft(flight);
                }
            }

            // Setup alternate codes
            var alternates = new List<string>();
            for(var i = 1;i <= 3;++i) {
                var altCallsign = worksheet.String(String.Format("Alt{0}", i));
                if(!String.IsNullOrEmpty(altCallsign)) alternates.Add(altCallsign);
            }

            // Setup criteria
            var callsign = worksheet.EString("Callsign");
            if(callsign != null) {
                alternates.Add(callsign);       // The alternates API always returns the callsign you asked for at a minimum unless it's null or empty
                _CallsignParser.Setup(r => r.GetAllAlternateCallsigns(callsign)).Returns(alternates);
            }

            var findAlternates = worksheet.Bool("FindAlts");
            var condition = worksheet.ParseEnum<FilterCondition>("Condition");
            var reverseCondition = worksheet.Bool("Reverse");
            _Criteria.Callsign = new FilterString(callsign) { Condition = condition, ReverseCondition = reverseCondition };
            _Criteria.UseAlternateCallsigns = findAlternates;

            // Get flights
            var flights = _Database.GetFlights(_Criteria, -1, -1, null, false, null, false);

            // Assert that we got what we expected
            Assert.AreEqual(worksheet.Int("Count"), flights.Count, comments);
            for(var i = 1;i <= 3;++i) {
                var expectCallsign = worksheet.EString(String.Format("Expect{0}", i));
                if(expectCallsign != null) {
                    Assert.IsNotNull(flights.Single(r => r.Callsign == expectCallsign), comments);
                }
            }
        }

        protected void BaseStationDatabase_GetFlights_Some_Criteria_Is_Case_Insensitive()
        {
            Do_GetFlightsAllVersions_Some_Criteria_Is_Case_Insensitive(true);
        }

        protected void Do_GetFlightsAllVersions_Some_Criteria_Is_Case_Insensitive(bool getFlights)
        {
            var flight = CreateFlight("1");
            foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                if(!getFlights && !IsFlightCriteria(criteriaProperty)) continue;

                RunTestCleanup();
                RunTestInitialise();

                bool isStringProperty = true;
                switch(criteriaProperty.Name) {
                    case "Callsign":        _Criteria.Callsign = new FilterString("a"); flight.Callsign = "A"; break;
                    case "Registration":    _Criteria.Registration = new FilterString("a"); flight.Aircraft.Registration = "A"; break;
                    case "Icao":            _Criteria.Icao = new FilterString("a"); flight.Aircraft.ModeS = "A"; break;
                    case "Type":            _Criteria.Type = new FilterString("a"); flight.Aircraft.ICAOTypeCode = "A"; break;
                    case "Operator":
                    case "Country":
                    case "Date":
                    case "IsEmergency":
                    case "UseAlternateCallsigns":
                    case "FirstAltitude":
                    case "LastAltitude":
                        isStringProperty = false;
                        break;
                    default:                throw new NotImplementedException(criteriaProperty.Name);
                }

                if(isStringProperty) {
                    AddFlightAndAircraft(flight);

                    List<BaseStationFlight> flights = null;
                    if(getFlights) flights = _Database.GetFlights(_Criteria, -1, -1, null, true, null, true);
                    else           flights = _Database.GetFlightsForAircraft(flight.Aircraft, _Criteria, -1, -1, null, true, null, true);

                    Assert.AreEqual(1, flights.Count, criteriaProperty.Name);
                }
            }
        }

        protected void BaseStationDatabase_GetFlights_Some_Criteria_Is_Case_Sensitive()
        {
            Do_GetFlightsAllVersions_Some_Criteria_Is_Case_Sensitive(true);
        }

        protected void Do_GetFlightsAllVersions_Some_Criteria_Is_Case_Sensitive(bool getFlights)
        {
            var flight = CreateFlight("1");
            foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                if(!getFlights && !IsFlightCriteria(criteriaProperty)) continue;

                RunTestCleanup();
                RunTestInitialise();

                bool isStringProperty = true;
                switch(criteriaProperty.Name) {
                    case "Operator":        _Criteria.Operator = new FilterString("a"); flight.Aircraft.RegisteredOwners = "A"; break;
                    case "Country":         _Criteria.Country = new FilterString("a"); flight.Aircraft.ModeSCountry = "A"; break;
                    case "Callsign":
                    case "Registration":
                    case "Icao":
                    case "Date":
                    case "Type":
                    case "IsEmergency":
                    case "UseAlternateCallsigns":
                    case "FirstAltitude":
                    case "LastAltitude":
                        isStringProperty = false;
                        break;
                    default:                throw new NotImplementedException(criteriaProperty.Name);
                }

                if(isStringProperty) {
                    AddFlightAndAircraft(flight);

                    List<BaseStationFlight> flights = null;
                    if(getFlights) flights = _Database.GetFlights(_Criteria, -1, -1, null, true, null, true);
                    else           flights = _Database.GetFlightsForAircraft(flight.Aircraft, _Criteria, -1, -1, null, true, null, true);

                    Assert.AreEqual(0, flights.Count, criteriaProperty.Name);
                }
            }
        }

        protected void BaseStationDatabase_GetFlights_Can_Return_Subset_Of_Rows()
        {
            Do_GetFlightsAllVersions_Can_Return_Subset_Of_Rows(true);
        }

        protected void Do_GetFlightsAllVersions_Can_Return_Subset_Of_Rows(bool getFlights)
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            int flightCount = worksheet.Int("Flights");

            var aircraft = CreateAircraft();
            AddAircraft(aircraft);

            for(int flightNumber = 0;flightNumber < flightCount;++flightNumber) {
                var flight = CreateFlight(aircraft, (flightNumber + 1).ToString());
                AddFlight(flight);
            }

            List<BaseStationFlight> flights;
            if(getFlights) flights = _Database.GetFlights(_Criteria, worksheet.Int("StartRow"), worksheet.Int("EndRow"), "CALLSIGN", true, null, false);
            else           flights = _Database.GetFlightsForAircraft(aircraft, _Criteria, worksheet.Int("StartRow"), worksheet.Int("EndRow"), "CALLSIGN", true, null, false);

            var rows = "";
            foreach(var flight in flights) {
                rows = String.Format("{0}{1}{2}", rows, rows.Length == 0 ? "" : ",", flight.Callsign);
            }

            Assert.AreEqual(worksheet.Int("ExpectedCount"), flights.Count);
            Assert.AreEqual(worksheet.EString("ExpectedRows"), rows);
        }

        protected void BaseStationDatabase_GetFlights_Ignores_Unknown_Sort_Columns()
        {
            Do_GetFlightsAllVersions_Ignores_Unknown_Sort_Columns(true);
        }

        protected void Do_GetFlightsAllVersions_Ignores_Unknown_Sort_Columns(bool getFlights)
        {
            var aircraft = CreateAircraft();
            AddAircraft(aircraft);
            AddFlight(CreateFlight(aircraft, "1"));
            AddFlight(CreateFlight(aircraft, "2"));

            List<BaseStationFlight> flights;
            if(getFlights) flights = _Database.GetFlights(_Criteria, -1, -1, "ThisColumnDoesNotExist", true, "AndNeitherDoesThis", false);
            else           flights = _Database.GetFlightsForAircraft(aircraft, _Criteria, -1, -1, "ThisColumnDoesNotExist", true, "AndNeitherDoesThis", false);

            Assert.AreEqual(2, flights.Count);
        }

        protected void BaseStationDatabase_GetFlights_Ignores_Case_On_Sort_Column_Names()
        {
            Do_GetFlightsAllVersions_Ignores_Case_On_Sort_Column_Names(true);
        }

        protected void Do_GetFlightsAllVersions_Ignores_Case_On_Sort_Column_Names(bool getFlights)
        {
            var aircraft = CreateAircraft();
            AddAircraft(aircraft);

            AddFlight(CreateFlight(aircraft, "ABC"));
            AddFlight(CreateFlight(aircraft, "XYZ"));

            List<BaseStationFlight> flights;
            if(getFlights) flights = _Database.GetFlights(_Criteria, -1, -1, "callsign", true, null, false);
            else           flights = _Database.GetFlightsForAircraft(aircraft, _Criteria, -1, -1, "callsign", true, null, false);

            Assert.AreEqual("ABC", flights[0].Callsign);
            Assert.AreEqual("XYZ", flights[1].Callsign);

            if(getFlights) flights = _Database.GetFlights(_Criteria, -1, -1, "callsign", false, null, false);
            else           flights = _Database.GetFlightsForAircraft(aircraft, _Criteria, -1, -1, "callsign", false, null, false);

            Assert.AreEqual("XYZ", flights[0].Callsign);
            Assert.AreEqual("ABC", flights[1].Callsign);
        }

        protected void BaseStationDatabase_GetFlights_Sorts_By_One_Column_Correctly()
        {
            var defaultFlight = CreateFlight("defaultFlight", false);
            var lowFlight = CreateFlight("lowFlight", false);
            var highFlight = CreateFlight("highFlight", false);

            defaultFlight.NumPosMsgRec = 1;
            lowFlight.NumPosMsgRec = 2;
            highFlight.NumPosMsgRec = 3;

            foreach(var sortColumn in _SortColumns) {
                RunTestCleanup();
                RunTestInitialise();

                SetSortColumnValue(defaultFlight, sortColumn, true, false);
                SetSortColumnValue(lowFlight, sortColumn, false, false);
                SetSortColumnValue(highFlight, sortColumn, false, true);

                AddFlightAndAircraft(defaultFlight);
                AddFlightAndAircraft(lowFlight);
                AddFlightAndAircraft(highFlight);

                var flights = _Database.GetFlights(_Criteria, -1, -1, sortColumn, true, null, false);
                Assert.AreEqual(3, flights.Count, sortColumn);
                Assert.AreEqual(1, flights[0].NumPosMsgRec, sortColumn);
                Assert.AreEqual(2, flights[1].NumPosMsgRec, sortColumn);
                Assert.AreEqual(3, flights[2].NumPosMsgRec, sortColumn);

                flights = _Database.GetFlights(_Criteria, -1, -1, sortColumn, false, null, false);
                Assert.AreEqual(3, flights.Count, sortColumn);
                Assert.AreEqual(3, flights[0].NumPosMsgRec, sortColumn);
                Assert.AreEqual(2, flights[1].NumPosMsgRec, sortColumn);
                Assert.AreEqual(1, flights[2].NumPosMsgRec, sortColumn);
            }
        }

        protected void BaseStationDatabase_GetFlights_Sorts_By_Two_Columns_Correctly()
        {
            var firstFlight = CreateFlight("1");
            var secondFlight = CreateFlight("2");
            var thirdFlight = CreateFlight("3");
            var fourthFlight = CreateFlight("4");

            firstFlight.Aircraft.Type = "1";
            secondFlight.Aircraft.Type = thirdFlight.Aircraft.Type = "2";
            fourthFlight.Aircraft.Type = "3";

            secondFlight.Aircraft.Registration = "A";
            thirdFlight.Aircraft.Registration = "B";

            AddFlightAndAircraft(firstFlight);
            AddFlightAndAircraft(secondFlight);
            AddFlightAndAircraft(thirdFlight);
            AddFlightAndAircraft(fourthFlight);

            var flights = _Database.GetFlights(_Criteria, -1, -1, "model", true, "reg", true);
            Assert.AreEqual("1", flights[0].Callsign);
            Assert.AreEqual("2", flights[1].Callsign);
            Assert.AreEqual("3", flights[2].Callsign);
            Assert.AreEqual("4", flights[3].Callsign);

            flights = _Database.GetFlights(_Criteria, -1, -1, "model", true, "reg", false);
            Assert.AreEqual("1", flights[0].Callsign);
            Assert.AreEqual("3", flights[1].Callsign);
            Assert.AreEqual("2", flights[2].Callsign);
            Assert.AreEqual("4", flights[3].Callsign);
        }
        #endregion

        #region GetFlightsForAircraft
        protected void BaseStationDatabase_GetFlightsForAircraft_Returns_Empty_List_If_Aircraft_Is_Null()
        {
            Assert.AreEqual(0, _Database.GetFlightsForAircraft(null, _Criteria, 0, int.MaxValue, null, false, null, false).Count);
        }

        protected void BaseStationDatabase_GetFlightsForAircraft_Throws_If_Criteria_Is_Null()
        {
            _Database.GetFlightsForAircraft(CreateAircraft(), null, 0, int.MaxValue, null, false, null, false);
        }

        protected void BaseStationDatabase_GetFlightsForAircraft_Restricts_Search_To_Single_Aircraft()
        {
            var flight1 = CreateFlight("1");
            var flight2 = CreateFlight("2");

            AddFlightAndAircraft(flight1);
            AddFlightAndAircraft(flight2);

            var flights = _Database.GetFlightsForAircraft(flight2.Aircraft, _Criteria, -1, -1, null, false, null, false);
            Assert.AreEqual(1, flights.Count);
            Assert.AreEqual("2", flights[0].Callsign);
        }

        protected void BaseStationDatabase_GetFlightsForAircraft_Does_Not_Create_New_Aircraft_Objects()
        {
            var aircraft = CreateAircraft("icao", "reg");
            AddAircraft(aircraft);

            AddFlight(CreateFlight(aircraft));
            AddFlight(CreateFlight(aircraft));

            var flights = _Database.GetFlightsForAircraft(aircraft, _Criteria, -1, -1, null, false, null, false);
            Assert.AreEqual(2, flights.Count);
            Assert.AreSame(aircraft, flights[0].Aircraft);
            Assert.AreSame(aircraft, flights[1].Aircraft);
        }

        protected void BaseStationDatabase_GetFlightsForAircraft_Copies_Database_Record_To_Flight_Object()
        {
            Do_GetFlightsAllVersions_Copies_Database_Record_To_Flight_Object(false);
        }

        protected void BaseStationDatabase_GetFlightsForAircraft_Can_Filter_Flights_By_Equality_Criteria()
        {
            foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                if(!IsFlightCriteria(criteriaProperty)) continue;

                RunTestCleanup();
                RunTestInitialise();

                var aircraft = CreateAircraft("icao", "reg");
                PrepareAircraftReference(aircraft);

                var defaultFlight = CreateFlight(aircraft);
                var notEqualFlight = CreateFlight(aircraft);
                var equalsFlight = CreateFlight(aircraft);

                if(SetEqualityCriteria(criteriaProperty, defaultFlight, notEqualFlight, equalsFlight, false)) {
                    AddFlight(defaultFlight);
                    AddFlight(notEqualFlight);
                    AddFlight(equalsFlight);

                    var flights = _Database.GetFlightsForAircraft(aircraft, _Criteria, -1, -1, null, false, null, false);
                    Assert.AreEqual(1, flights.Count, criteriaProperty.Name);
                    Assert.AreEqual(equalsFlight.FlightID, flights[0].FlightID, criteriaProperty.Name);
                }
            }
        }

        protected void BaseStationDatabase_GetFlightsForAircraft_Can_Filter_Flights_By_Range_Criteria()
        {
            foreach(var reverseCondition in new bool[] { false, true }) {
                foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                    if(!IsFlightCriteria(criteriaProperty)) continue;

                    RunTestCleanup();
                    RunTestInitialise();

                    var aircraft = CreateAircraft("icao", "reg");
                    PrepareAircraftReference(aircraft);

                    var belowRangeFlight = CreateFlight(aircraft, "belowRange");
                    var startRangeFlight = CreateFlight(aircraft, "startRange");
                    var inRangeFlight = CreateFlight(aircraft, "inRange");
                    var endRangeFlight = CreateFlight(aircraft, "endRange");
                    var aboveRangeFlight = CreateFlight(aircraft, "aboveRange");

                    if(SetRangeCriteria(criteriaProperty, belowRangeFlight, startRangeFlight, inRangeFlight, endRangeFlight, aboveRangeFlight, reverseCondition)) {
                        AddFlight(belowRangeFlight);
                        AddFlight(startRangeFlight);
                        AddFlight(inRangeFlight);
                        AddFlight(endRangeFlight);
                        AddFlight(aboveRangeFlight);

                        var flights = _Database.GetFlightsForAircraft(aircraft, _Criteria, -1, -1, null, false, null, false);
                        var message = String.Format("{0}:", criteriaProperty.Name);
                        foreach(var flight in flights) message = String.Format("{0} {1}", message, flight.Callsign);

                        if(!reverseCondition) {
                            Assert.AreEqual(3, flights.Count, message);
                            Assert.IsTrue(flights.Any(f => f.Callsign == "startRange"), message);
                            Assert.IsTrue(flights.Any(f => f.Callsign == "inRange"), message);
                            Assert.IsTrue(flights.Any(f => f.Callsign == "endRange"), message);
                        } else {
                            Assert.AreEqual(2, flights.Count, message);
                            Assert.IsTrue(flights.Any(f => f.Callsign == "belowRange"), message);
                            Assert.IsTrue(flights.Any(f => f.Callsign == "aboveRange"), message);
                        }
                    }
                }
            }
        }

        protected void BaseStationDatabase_GetFlightsForAircraft_Some_Criteria_Is_Case_Insensitive()
        {
            Do_GetFlightsAllVersions_Some_Criteria_Is_Case_Insensitive(false);
        }

        protected void BaseStationDatabase_GetFlightsForAircraft_Some_Criteria_Is_Case_Sensitive()
        {
            Do_GetFlightsAllVersions_Some_Criteria_Is_Case_Sensitive(false);
        }

        protected void BaseStationDatabase_GetFlightsForAircraft_Can_Return_Subset_Of_Rows()
        {
            Do_GetFlightsAllVersions_Can_Return_Subset_Of_Rows(false);
        }

        protected void BaseStationDatabase_GetFlightsForAircraft_Ignores_Unknown_Sort_Columns()
        {
            Do_GetFlightsAllVersions_Ignores_Unknown_Sort_Columns(false);
        }

        protected void BaseStationDatabase_GetFlightsForAircraft_Ignores_Case_On_Sort_Column_Names()
        {
            Do_GetFlightsAllVersions_Ignores_Case_On_Sort_Column_Names(false);
        }

        protected void BaseStationDatabase_GetFlightsForAircraft_Sorts_By_One_Column_Correctly()
        {
            var aircraft = CreateAircraft();

            var defaultFlight = CreateFlight(aircraft, "defaultFlight");
            var lowFlight = CreateFlight(aircraft, "lowFlight");
            var highFlight = CreateFlight(aircraft, "highFlight");

            defaultFlight.NumPosMsgRec = 1;
            lowFlight.NumPosMsgRec = 2;
            highFlight.NumPosMsgRec = 3;

            foreach(var sortColumn in _SortColumns) {
                if(!IsFlightSortColumn(sortColumn)) continue;

                RunTestCleanup();
                RunTestInitialise();

                SetSortColumnValue(defaultFlight, sortColumn, true, false);
                SetSortColumnValue(lowFlight, sortColumn, false, false);
                SetSortColumnValue(highFlight, sortColumn, false, true);

                AddAircraft(aircraft);

                AddFlight(defaultFlight);
                AddFlight(lowFlight);
                AddFlight(highFlight);

                var flights = _Database.GetFlights(_Criteria, -1, -1, sortColumn, true, null, false);
                Assert.AreEqual(3, flights.Count, sortColumn);
                Assert.AreEqual(1, flights[0].NumPosMsgRec, sortColumn);
                Assert.AreEqual(2, flights[1].NumPosMsgRec, sortColumn);
                Assert.AreEqual(3, flights[2].NumPosMsgRec, sortColumn);

                flights = _Database.GetFlights(_Criteria, -1, -1, sortColumn, false, null, false);
                Assert.AreEqual(3, flights.Count, sortColumn);
                Assert.AreEqual(3, flights[0].NumPosMsgRec, sortColumn);
                Assert.AreEqual(2, flights[1].NumPosMsgRec, sortColumn);
                Assert.AreEqual(1, flights[2].NumPosMsgRec, sortColumn);
            }
        }

        protected void BaseStationDatabase_GetFlightsForAircraft_Sorts_By_Two_Columns_Correctly()
        {
            var aircraft = CreateAircraft();
            AddAircraft(aircraft);

            var firstFlight = CreateFlight(aircraft, "1");
            var secondFlight = CreateFlight(aircraft, "2");
            var thirdFlight = CreateFlight(aircraft, "2");
            var fourthFlight = CreateFlight(aircraft, "3");

            firstFlight.FirstAltitude = 1;
            secondFlight.FirstAltitude = 2;
            thirdFlight.FirstAltitude = 3;
            fourthFlight.FirstAltitude = 4;

            var startTime = new DateTime(2010, 2, 3);
            firstFlight.StartTime = startTime;
            secondFlight.StartTime = startTime.AddDays(1);
            thirdFlight.StartTime = startTime.AddDays(2);
            fourthFlight.StartTime = startTime.AddDays(3);

            AddFlight(firstFlight);
            AddFlight(secondFlight);
            AddFlight(thirdFlight);
            AddFlight(fourthFlight);

            var flights = _Database.GetFlightsForAircraft(aircraft, _Criteria, -1, -1, "callsign", true, "date", true);
            Assert.AreEqual(1, flights[0].FirstAltitude);
            Assert.AreEqual(2, flights[1].FirstAltitude);
            Assert.AreEqual(3, flights[2].FirstAltitude);
            Assert.AreEqual(4, flights[3].FirstAltitude);

            flights = _Database.GetFlightsForAircraft(aircraft, _Criteria, -1, -1, "callsign", true, "date", false);
            Assert.AreEqual(1, flights[0].FirstAltitude);
            Assert.AreEqual(3, flights[1].FirstAltitude);
            Assert.AreEqual(2, flights[2].FirstAltitude);
            Assert.AreEqual(4, flights[3].FirstAltitude);
        }
        #endregion

        #region GetCountOfFlights
        protected void BaseStationDatabase_GetCountOfFlights_Throws_If_Criteria_Is_Null()
        {
            _Database.GetCountOfFlights(null);
        }

        protected void BaseStationDatabase_GetCountOfFlights_Returns_Count_Of_Flights_Matching_Criteria()
        {
            AddFlightAndAircraft(CreateFlight("ABC"));
            AddFlightAndAircraft(CreateFlight("XYZ"));

            Assert.AreEqual(2, _Database.GetCountOfFlights(_Criteria));

            _Criteria.Callsign = new FilterString("XYZ");
            Assert.AreEqual(1, _Database.GetCountOfFlights(_Criteria));
        }

        protected void BaseStationDatabase_GetCountOfFlights_Counts_Equality_Criteria()
        {
            var defaultFlight = CreateFlight("default", false);
            var notEqualFlight = CreateFlight("notEquals", false);
            var equalsFlight = CreateFlight("equals", false);

            foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                RunTestCleanup();
                RunTestInitialise();

                if(SetEqualityCriteria(criteriaProperty, defaultFlight, notEqualFlight, equalsFlight, false)) {
                    AddFlightAndAircraft(defaultFlight);
                    AddFlightAndAircraft(notEqualFlight);
                    AddFlightAndAircraft(equalsFlight);

                    Assert.AreEqual(1, _Database.GetCountOfFlights(_Criteria), "{0}", criteriaProperty.Name);
                }
            }
        }

        protected void BaseStationDatabase_GetCountOfFlights_Counts_Range_Criteria()
        {
            var belowRangeFlight = CreateFlight("belowRange");
            var startRangeFlight = CreateFlight("startRange");
            var inRangeFlight = CreateFlight("inRange");
            var endRangeFlight = CreateFlight("endRange");
            var aboveRangeFlight = CreateFlight("aboveRange");

            foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                RunTestCleanup();
                RunTestInitialise();

                if(SetRangeCriteria(criteriaProperty, belowRangeFlight, startRangeFlight, inRangeFlight, endRangeFlight, aboveRangeFlight, false)) {
                    AddFlightAndAircraft(belowRangeFlight);
                    AddFlightAndAircraft(startRangeFlight);
                    AddFlightAndAircraft(inRangeFlight);
                    AddFlightAndAircraft(endRangeFlight);
                    AddFlightAndAircraft(aboveRangeFlight);

                    Assert.AreEqual(3, _Database.GetCountOfFlights(_Criteria), criteriaProperty.Name);
                }
            }
        }
        #endregion

        #region GetCountOfFlightsForAircraft
        protected void BaseStationDatabase_GetCountOfFlightsForAircraft_Returns_Zero_If_Aircraft_Is_Null()
        {
            AddFlightAndAircraft(CreateFlight("1"));

            Assert.AreEqual(0, _Database.GetCountOfFlightsForAircraft(null, _Criteria));
        }

        [ExpectedException(typeof(ArgumentNullException))]
        protected void BaseStationDatabase_GetCountOfFlightsForAircraft_Throws_If_Criteria_Is_Null()
        {
            _Database.GetCountOfFlightsForAircraft(CreateAircraft(), null);
        }

        protected void BaseStationDatabase_GetCountOfFlightsForAircraft_Returns_Count_Of_Flights_Matching_Criteria()
        {
            var aircraft = CreateAircraft();
            AddAircraft(aircraft);

            AddFlight(CreateFlight(aircraft, "ABC"));
            AddFlight(CreateFlight(aircraft, "XYZ"));
            AddFlightAndAircraft(CreateFlight("XYZ"));  // <-- different actual, should not be included

            Assert.AreEqual(2, _Database.GetCountOfFlightsForAircraft(aircraft, _Criteria));

            _Criteria.Callsign = new FilterString("XYZ");
            Assert.AreEqual(1, _Database.GetCountOfFlightsForAircraft(aircraft, _Criteria));
        }

        protected void BaseStationDatabase_GetCountOfFlightsForAircraft_Counts_Equality_Criteria()
        {
            var aircraft = CreateAircraft();
            var defaultFlight = CreateFlight(aircraft);
            var notEqualFlight = CreateFlight(aircraft);
            var equalsFlight = CreateFlight(aircraft);

            foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                if(!IsFlightCriteria(criteriaProperty)) continue;

                RunTestCleanup();
                RunTestInitialise();

                if(SetEqualityCriteria(criteriaProperty, defaultFlight, notEqualFlight, equalsFlight, false)) {
                    var aircraftId = (int)AddAircraft(aircraft);
                    defaultFlight.AircraftID = notEqualFlight.AircraftID = equalsFlight.AircraftID = aircraftId;

                    AddFlight(defaultFlight);
                    AddFlight(notEqualFlight);
                    AddFlight(equalsFlight);

                    Assert.AreEqual(1, _Database.GetCountOfFlightsForAircraft(aircraft, _Criteria));
                }
            }
        }

        protected void BaseStationDatabase_GetCountOfFlightsForAircraft_Counts_Range_Criteria()
        {
            var aircraft = CreateAircraft();
            var belowRangeFlight = CreateFlight(aircraft);
            var startRangeFlight = CreateFlight(aircraft);
            var inRangeFlight = CreateFlight(aircraft);
            var endRangeFlight = CreateFlight(aircraft);
            var aboveRangeFlight = CreateFlight(aircraft);

            foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                if(!IsFlightCriteria(criteriaProperty)) continue;

                RunTestCleanup();
                RunTestInitialise();

                if(SetRangeCriteria(criteriaProperty, belowRangeFlight, startRangeFlight, inRangeFlight, endRangeFlight, aboveRangeFlight, false)) {
                    var aircraftId = (int)AddAircraft(aircraft);
                    belowRangeFlight.AircraftID = startRangeFlight.AircraftID = inRangeFlight.AircraftID = endRangeFlight.AircraftID = aboveRangeFlight.AircraftID = aircraftId;

                    AddFlight(belowRangeFlight);
                    AddFlight(startRangeFlight);
                    AddFlight(inRangeFlight);
                    AddFlight(endRangeFlight);
                    AddFlight(aboveRangeFlight);

                    Assert.AreEqual(3, _Database.GetCountOfFlightsForAircraft(aircraft, _Criteria));
                }
            }
        }
        #endregion

        #region GetFlightById
        protected void BaseStationDatabase_GetFlightById_Returns_Null_If_Flight_Does_Not_Exist()
        {
            Assert.IsNull(_Database.GetFlightById(1));
        }

        protected void BaseStationDatabase_GetFlightById_Returns_Flight_Object_For_Record_Identifier()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var aircraft = new BaseStationAircraft() { ModeS = "A" };
            AddAircraft(aircraft);

            var mockFlight = LoadFlightFromSpreadsheet(worksheet);
            mockFlight.SessionID = PrepareSessionReference(new BaseStationSession() { StartTime = DateTime.Now }).SessionID;
            mockFlight.Aircraft = aircraft;
            mockFlight.AircraftID = aircraft.AircraftID;

            var id = (int)AddFlight(mockFlight);

            var flight = _Database.GetFlightById(id);
            Assert.AreNotSame(flight, mockFlight);

            AssertFlightsAreEqual(mockFlight, flight, false, aircraft.AircraftID);
        }
        #endregion

        #region InsertFlight
        protected void BaseStationDatabase_InsertFlight_Throws_If_Writes_Disabled()
        {
            var aircraftId = (int)AddAircraft(new BaseStationAircraft() { ModeS = "Z" });
            _Database.InsertFlight(new BaseStationFlight() { AircraftID = aircraftId, StartTime = DateTime.Now });
        }

        protected void BaseStationDatabase_InsertFlight_Truncates_Milliseconds_From_Date()
        {
            _Database.WriteSupportEnabled = true;
            var aircraftId = (int)AddAircraft(new BaseStationAircraft() { ModeS = "Y" });
            var sessionId = (int)AddSession(new BaseStationSession() { StartTime = DateTime.Now });

            var time1 = new DateTime(2001, 2, 3, 4, 5, 6, 789);
            var time2 = new DateTime(2009, 8, 7, 6, 5, 4, 321);

            var flight = new BaseStationFlight() { AircraftID = aircraftId, SessionID = sessionId, StartTime = time1, EndTime = time2 };
            _Database.InsertFlight(flight);

            var readBack = _Database.GetFlightById(flight.FlightID);
            Assert.AreEqual(TruncateDate(time1), readBack.StartTime);
            Assert.AreEqual(TruncateDate(time2), readBack.EndTime);
        }

        protected void BaseStationDatabase_InsertFlight_Correctly_Inserts_Record()
        {
            _Database.WriteSupportEnabled = true;

            var worksheet = new ExcelWorksheetData(TestContext);
            var aircraftId = PrepareAircraftReference(new BaseStationAircraft() { ModeS = "Y" }).AircraftID;
            var sessionId = PrepareSessionReference(new BaseStationSession() { StartTime = DateTime.Now }).SessionID;
            var flight = LoadFlightFromSpreadsheet(worksheet);
            flight.AircraftID = aircraftId;
            flight.SessionID = sessionId;
            flight.Aircraft = null;

            _Database.InsertFlight(flight);
            Assert.AreNotEqual(0, flight.FlightID);

            var readBack = _Database.GetFlightById(flight.FlightID);
            AssertFlightsAreEqual(flight, readBack, false, aircraftId);
        }

        protected void BaseStationDatabase_InsertFlight_Works_In_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    RunTestCleanup();
                    RunTestInitialise();

                    try {
                        BaseStationDatabase_InsertFlight_Correctly_Inserts_Record();
                    } catch(Exception ex) {
                        throw new InvalidOperationException($"Exception thrown when culture was {culture}", ex);
                    }
                }
            }
        }
        #endregion

        #region UpdateFlight
        protected void BaseStationDatabase_UpdateFlight_Throws_If_Writes_Disabled()
        {
            var flight = new BaseStationFlight() {
                AircraftID = PrepareAircraftReference(new BaseStationAircraft() { ModeS = "Z" }).AircraftID,
                SessionID = PrepareSessionReference(new BaseStationSession() { StartTime = DateTime.Now }).SessionID,
            };

            flight.FlightID = (int)AddFlight(flight);

            flight.FirstTrack = 42;
            _Database.UpdateFlight(flight);
        }

        protected void BaseStationDatabase_UpdateFlight_Truncates_Milliseconds_From_Date()
        {
            _Database.WriteSupportEnabled = true;
            var aircraftId = PrepareAircraftReference(new BaseStationAircraft() { ModeS = "Y" }).AircraftID;
            var sessionId = PrepareSessionReference(new BaseStationSession() { StartTime = DateTime.Now }).SessionID;

            var flight = new BaseStationFlight() { AircraftID = aircraftId, SessionID = sessionId };
            _Database.InsertFlight(flight);

            var time1 = new DateTime(2001, 2, 3, 4, 5, 6, 789);
            var time2 = new DateTime(2009, 8, 7, 6, 5, 4, 321);

            var update = _Database.GetFlightById(flight.FlightID);
            update.StartTime = time1;
            update.EndTime = time2;
            _Database.UpdateFlight(update);

            var readBack = _Database.GetFlightById(flight.FlightID);
            Assert.AreEqual(TruncateDate(time1), readBack.StartTime);
            Assert.AreEqual(TruncateDate(time2), readBack.EndTime);
        }

        protected void BaseStationDatabase_UpdateFlight_Correctly_Updates_Record()
        {
            _Database.WriteSupportEnabled = true;
            var worksheet = new ExcelWorksheetData(TestContext);

            var originalAircraftId = PrepareAircraftReference(new BaseStationAircraft() { ModeS = "Z" }).AircraftID;
            var originalSessionId = PrepareSessionReference(new BaseStationSession() { StartTime = DateTime.Now }).SessionID;
            var flightId = PrepareFlightReference(new BaseStationFlight() { AircraftID = originalAircraftId, SessionID = originalSessionId }).FlightID;

            var update = _Database.GetFlightById(flightId);
            LoadFlightFromSpreadsheet(worksheet, 0, update);
            update.Aircraft = null;

            var newAircraftId = PrepareAircraftReference(new BaseStationAircraft() { ModeS = "XYZ" }).AircraftID;
            var newSessionId = PrepareSessionReference(new BaseStationSession() { StartTime = DateTime.Now.AddDays(1) }).SessionID;
            update.AircraftID = newAircraftId;
            update.SessionID = newSessionId;

            _Database.UpdateFlight(update);

            var readBack = _Database.GetFlightById(flightId);
            AssertFlightsAreEqual(update, readBack, false, newAircraftId);
        }

        protected void BaseStationDatabase_UpdateFlight_Works_For_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    RunTestCleanup();
                    RunTestInitialise();

                    try {
                        BaseStationDatabase_UpdateFlight_Correctly_Updates_Record();
                    } catch(Exception ex) {
                        throw new InvalidOperationException($"Exception thrown when culture was {culture}", ex);
                    }
                }
            }
        }
        #endregion

        #region DeleteFlight
        protected void BaseStationDatabase_DeleteFlight_Throws_If_Writes_Disabled()
        {
            var flight = new BaseStationFlight() {
                AircraftID = PrepareAircraftReference(new BaseStationAircraft() { ModeS = "Z" }).AircraftID,
                SessionID = PrepareSessionReference(new BaseStationSession() { StartTime = DateTime.Now }).SessionID,
            };

            flight.FlightID = (int)AddFlight(flight);

            _Database.DeleteFlight(flight);
        }

        protected void BaseStationDatabase_DeleteFlight_Correctly_Deletes_Record()
        {
            _Database.WriteSupportEnabled = true;

            var aircraftId = PrepareAircraftReference(new BaseStationAircraft() { ModeS = "Z" }).AircraftID;
            var sessionId = PrepareSessionReference(new BaseStationSession() { StartTime = DateTime.Now }).SessionID;
            var flightId = PrepareFlightReference(new BaseStationFlight() { AircraftID = aircraftId, SessionID = sessionId }).FlightID;

            var delete = _Database.GetFlightById(flightId);

            _Database.DeleteFlight(delete);

            Assert.AreEqual(null, _Database.GetFlightById(flightId));

            Assert.AreNotEqual(null, _Database.GetAircraftById(aircraftId));
            Assert.AreEqual(sessionId, _Database.GetSessions()[0].SessionID);
        }
        #endregion

        #region UpsertManyFlights
        protected void BaseStationDatabase_UpsertManyFlights_Throws_If_Writes_Disabled()
        {
            _Database.WriteSupportEnabled = true;

            var aircraftId = PrepareAircraftReference(new BaseStationAircraft() { ModeS = "123456" }).AircraftID;
            var sessionId = PrepareSessionReference(new BaseStationSession() { StartTime = DateTime.Now }).SessionID;

            _Database.WriteSupportEnabled = false;

            _Database.UpsertManyFlights(new BaseStationFlight[] {
                new BaseStationFlight() { AircraftID = aircraftId, SessionID = sessionId, StartTime = DateTime.Now },
            });
        }

        protected void BaseStationDatabase_UpsertManyFlights_Inserts_New_Flights()
        {
            _Database.WriteSupportEnabled = true;

            var worksheet = new ExcelWorksheetData(TestContext);
            var aircraftId = PrepareAircraftReference(new BaseStationAircraft() { ModeS = "Y" }).AircraftID;
            var sessionId = PrepareSessionReference(new BaseStationSession() { StartTime = DateTime.Now }).SessionID;
            var flight = LoadFlightFromSpreadsheet(worksheet);

            flight.AircraftID = aircraftId;
            flight.SessionID =  sessionId;
            flight.Aircraft =   null;

            var flights = _Database.UpsertManyFlights(new BaseStationFlightUpsert[] {
                new BaseStationFlightUpsert(flight),
            });

            Assert.AreEqual(1, flights.Length);
            AssertFlightsAreEqual(flight, flights[0], false, aircraftId);
        }

        protected void BaseStationDatabase_UpsertManyFlights_Updates_Existing_Flights()
        {
            _Database.WriteSupportEnabled = true;

            var worksheet = new ExcelWorksheetData(TestContext);
            var aircraftId = PrepareAircraftReference(new BaseStationAircraft() { ModeS = "Y" }).AircraftID;
            var sessionId = PrepareSessionReference(new BaseStationSession() { StartTime = DateTime.Now }).SessionID;
            var startTime = worksheet.DateTime("StartTime");

            var originalFlight = new BaseStationFlight() {
                AircraftID = aircraftId,
                SessionID =  sessionId,
                StartTime =  startTime,
            };
            _Database.InsertFlight(originalFlight);

            var flight = LoadFlightFromSpreadsheet(worksheet);
            flight.AircraftID = aircraftId;
            flight.SessionID =  sessionId;
            flight.Aircraft =   null;

            var flights = _Database.UpsertManyFlights(new BaseStationFlightUpsert[] {
                new BaseStationFlightUpsert(flight),
            });

            Assert.AreEqual(1, flights.Length);
            Assert.AreEqual(originalFlight.FlightID, flights[0].FlightID);
            AssertFlightsAreEqual(flight, flights[0], false, aircraftId);
        }
        #endregion

        #region GetDatabaseHistory
        protected void BaseStationDatabase_GetDatabaseHistory_Retrieves_All_Records_In_DBHistory_Table()
        {
            var timeStamp1 = DateTime.Now;
            var timeStamp2 = DateTime.Now.AddSeconds(10);

            ClearDBHistory();
            AddDBHistory(new BaseStationDBHistory() { Description = "A", TimeStamp = timeStamp1 });
            AddDBHistory(new BaseStationDBHistory() { Description = "B", TimeStamp = timeStamp2 });

            var history = _Database.GetDatabaseHistory();
            Assert.AreEqual(2, history.Count);

            var historyA = history.FirstOrDefault(h => h.Description == "A");
            var historyB = history.FirstOrDefault(h => h.Description == "B");

            Assert.AreEqual(timeStamp1, historyA.TimeStamp);
            Assert.AreEqual(timeStamp2, historyB.TimeStamp);
        }
        #endregion

        #region GetDatabaseVersion
        protected void BaseStationDatabase_GetDatabaseVersion_Retrieves_Record_In_DBInfo_Table()
        {
            // The table has no key, it appears that the intention is to only ever have one record in the table
            AddDBInfo(new BaseStationDBInfo() { OriginalVersion = 2, CurrentVersion = 3 });

            var dbInfo = _Database.GetDatabaseVersion();
            Assert.AreEqual(2, dbInfo.OriginalVersion);
            Assert.AreEqual(3, dbInfo.CurrentVersion);
        }

        protected void BaseStationDatabase_GetDatabaseVersion_Retrieves_Last_Record_In_DBInfo_Table()
        {
            // While there should only be one record in the table there's nothing to stop there being more than
            // one, in which case we need to decide what to do. We just retrieve the last record but we can't
            // really tell which one that might be, so we just need to make sure it doesn't blow up.
            AddDBInfo(new BaseStationDBInfo() { OriginalVersion = 2, CurrentVersion = 3 });
            AddDBInfo(new BaseStationDBInfo() { OriginalVersion = 1, CurrentVersion = 2 });

            var dbInfo = _Database.GetDatabaseVersion();
            Assert.IsTrue((dbInfo.OriginalVersion == 2 && dbInfo.CurrentVersion == 3) || (dbInfo.OriginalVersion == 1 && dbInfo.CurrentVersion == 2));
        }
        #endregion

        #region GetSystemEvents
        protected void BaseStationDatabase_GetSystemEvents_Retrieves_All_Records_In_SystemEvents_Table()
        {
            ClearSystemEvents();

            var timeStamp1 = DateTime.Now;
            var timeStamp2 = DateTime.Now.AddHours(2.1);
            AddSystemEvent(new BaseStationSystemEvents() { App = "A", Msg = "B", TimeStamp = timeStamp1 });
            AddSystemEvent(new BaseStationSystemEvents() { App = "2", Msg = "3", TimeStamp = timeStamp2 });

            var systemEvents = _Database.GetSystemEvents();
            Assert.AreEqual(2, systemEvents.Count);

            Assert.IsTrue(systemEvents.Any(s => s.App == "A" && s.Msg == "B" && s.TimeStamp == timeStamp1 && s.SystemEventsID != 0));
            Assert.IsTrue(systemEvents.Any(s => s.App == "2" && s.Msg == "3" && s.TimeStamp == timeStamp2 && s.SystemEventsID != 0));
        }
        #endregion

        #region InsertSystemEvents
        protected void BaseStationDatabase_InsertSystemEvents_Throws_If_Writes_Disabled()
        {
            _Database.InsertSystemEvent(new BaseStationSystemEvents() { App = "A", Msg = "D", TimeStamp = DateTime.Now });
        }

        protected void BaseStationDatabase_InsertSystemEvents_Correctly_Inserts_Record()
        {
            _Database.WriteSupportEnabled = true;

            var timestamp = DateTime.Now;
            var systemEvent = new BaseStationSystemEvents() { App = "123456789.12345", Msg = new String('X', 100), TimeStamp = timestamp };

            _Database.InsertSystemEvent(systemEvent);
            Assert.AreNotEqual(0, systemEvent.SystemEventsID);

            var readBack = _Database.GetSystemEvents()[0];
            Assert.AreEqual(systemEvent.SystemEventsID, readBack.SystemEventsID);
            Assert.AreEqual("123456789.12345", readBack.App);
            Assert.AreEqual(new String('X', 100), readBack.Msg);
            Assert.AreEqual(TruncateDate(timestamp), readBack.TimeStamp);
        }

        protected void BaseStationDatabase_InsertSystemEvents_Works_For_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    RunTestCleanup();
                    RunTestInitialise();

                    try {
                        BaseStationDatabase_InsertSystemEvents_Correctly_Inserts_Record();
                    } catch(Exception ex) {
                        throw new InvalidOperationException($"Exception thrown when culture was {culture}", ex);
                    }
                }
            }
        }
        #endregion

        #region UpdateSystemEvents
        protected void BaseStationDatabase_UpdateSystemEvents_Throws_If_Writes_Disabled()
        {
            AddSystemEvent(new BaseStationSystemEvents() { App = "K", Msg = "Z", TimeStamp = DateTime.Now });
            var update = _Database.GetSystemEvents()[0];
            _Database.UpdateSystemEvent(update);
        }

        protected void BaseStationDatabase_UpdateSystemEvents_Correctly_Updates_Record()
        {
            _Database.WriteSupportEnabled = true;

            AddSystemEvent(new BaseStationSystemEvents() { App = "K", Msg = "Z", TimeStamp = DateTime.Now });
            var update = _Database.GetSystemEvents()[0];

            var timestamp = new DateTime(2001, 2, 3, 4, 5, 6, 789);
            update.App = "123456789.12345";
            update.Msg = new String('X', 100);
            update.TimeStamp = timestamp;

            _Database.UpdateSystemEvent(update);

            var allSystemEvents = _Database.GetSystemEvents();
            Assert.AreEqual(1, allSystemEvents.Count());

            var readBack = allSystemEvents[0];
            Assert.AreEqual(update.SystemEventsID, readBack.SystemEventsID);
            Assert.AreEqual("123456789.12345", readBack.App);
            Assert.AreEqual(new String('X', 100), readBack.Msg);
            Assert.AreEqual(TruncateDate(timestamp), readBack.TimeStamp);
        }

        protected void BaseStationDatabase_UpdateSystemEvents_Works_For_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    RunTestCleanup();
                    RunTestInitialise();

                    try {
                        BaseStationDatabase_UpdateSystemEvents_Correctly_Updates_Record();
                    } catch(Exception ex) {
                        throw new InvalidOperationException($"Exception thrown when culture was {culture}", ex);
                    }
                }
            }
        }
        #endregion

        #region DeleteSystemEvents
        protected void BaseStationDatabase_DeleteSystemEvents_Throws_If_Writes_Disabled()
        {
            AddSystemEvent(new BaseStationSystemEvents() { App = "K", Msg = "Z", TimeStamp = DateTime.Now });
            var delete = _Database.GetSystemEvents()[0];
            _Database.DeleteSystemEvent(delete);
        }

        protected void BaseStationDatabase_DeleteSystemEvents_Correctly_Deletes_Record()
        {
            _Database.WriteSupportEnabled = true;

            AddSystemEvent(new BaseStationSystemEvents() { App = "K", Msg = "Z", TimeStamp = DateTime.Now });
            var delete = _Database.GetSystemEvents()[0];

            _Database.DeleteSystemEvent(delete);

            Assert.AreEqual(0, _Database.GetSystemEvents().Count);
        }
        #endregion

        #region GetLocations
        protected void BaseStationDatabase_GetLocations_Retrieves_All_Records_In_Locations_Table()
        {
            ClearLocations();
            AddLocation(new BaseStationLocation() { LocationName = "A", Latitude = 1.2, Longitude = 9.8, Altitude = 6.5 });
            AddLocation(new BaseStationLocation() { LocationName = "B", Latitude = 6.5, Longitude = 4.3, Altitude = 2.1 });

            var locations = _Database.GetLocations();
            Assert.AreEqual(2, locations.Count);

            Assert.IsTrue(locations.Any(n => n.LocationName == "A"));
            Assert.IsTrue(locations.Any(n => n.LocationName == "B"));
        }
        #endregion

        #region InsertLocation
        protected void BaseStationDatabase_InsertLocation_Throws_If_Writes_Disabled()
        {
            _Database.InsertLocation(new BaseStationLocation() { LocationName = "X" });
        }

        protected void BaseStationDatabase_InsertLocation_Correctly_Inserts_Record()
        {
            ClearLocations();
            _Database.WriteSupportEnabled = true;

            var location = new BaseStationLocation() {
                Altitude = 1.2468,
                Latitude = 51.3921,
                Longitude = 123.9132,
                LocationName = "123456789.123456789.",
            };

            _Database.InsertLocation(location);
            Assert.AreNotEqual(0, location.LocationID);

            var readBack = _Database.GetLocations()[0];
            Assert.AreEqual(location.LocationID, readBack.LocationID);
            Assert.AreEqual(1.2468, readBack.Altitude, 0.00001);
            Assert.AreEqual(51.3921, readBack.Latitude, 0.00001);
            Assert.AreEqual(123.9132, readBack.Longitude, 0.00001);
            Assert.AreEqual("123456789.123456789.", readBack.LocationName);
        }

        protected void BaseStationDatabase_InsertLocation_Works_For_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    RunTestCleanup();
                    RunTestInitialise();

                    try {
                        BaseStationDatabase_InsertLocation_Correctly_Inserts_Record();
                    } catch(Exception ex) {
                        throw new InvalidOperationException($"Exception thrown when culture was {culture}", ex);
                    }
                }
            }
        }
        #endregion

        #region UpdateLocation
        protected void BaseStationDatabase_UpdateLocation_Throws_If_Writes_Disabled()
        {
            var location = new BaseStationLocation() { LocationName = "B" };
            location.LocationID = (int)AddLocation(location);

            location.LocationName = "C";
            _Database.UpdateLocation(location);
        }

        protected void BaseStationDatabase_UpdateLocation_Correctly_Updates_Record()
        {
            ClearLocations();
            _Database.WriteSupportEnabled = true;
            AddLocation(new BaseStationLocation() {
                Altitude = 1.2468,
                Latitude = 51.3921,
                Longitude = 123.9132,
                LocationName = "123456789.123456789.",
            });

            var update = _Database.GetLocations()[0];
            var originalId = update.LocationID;
            update.Altitude = 4.6543;
            update.Latitude = 7.6533;
            update.Longitude = 9.2341;
            update.LocationName = "Hello";

            _Database.UpdateLocation(update);

            var allLocations = _Database.GetLocations();
            Assert.AreEqual(1, allLocations.Count);
            var readBack = allLocations[0];
            Assert.AreEqual(originalId, readBack.LocationID);
            Assert.AreEqual(4.6543, readBack.Altitude, 0.00001);
            Assert.AreEqual(7.6533, readBack.Latitude, 0.00001);
            Assert.AreEqual(9.2341, readBack.Longitude, 0.00001);
            Assert.AreEqual("Hello", readBack.LocationName);
        }

        protected void BaseStationDatabase_UpdateLocation_Works_For_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    RunTestCleanup();
                    RunTestInitialise();

                    try {
                        BaseStationDatabase_UpdateLocation_Correctly_Updates_Record();
                    } catch(Exception ex) {
                        throw new InvalidOperationException($"Exception thrown when culture was {culture}", ex);
                    }
                }
            }
        }
        #endregion

        #region DeleteLocation
        protected void BaseStationDatabase_DeleteLocation_Throws_If_Writes_Disabled()
        {
            var location = new BaseStationLocation() { LocationName = "B" };
            location.LocationID = (int)AddLocation(location);

            _Database.DeleteLocation(location);
        }

        protected void BaseStationDatabase_DeleteLocation_Correctly_Deletes_Record()
        {
            ClearLocations();
            _Database.WriteSupportEnabled = true;
            AddLocation(new BaseStationLocation() {
                Altitude = 1.2468,
                Latitude = 51.3921,
                Longitude = 123.9132,
                LocationName = "123456789.123456789.",
            });

            var update = _Database.GetLocations()[0];
            var originalId = update.LocationID;

            _Database.DeleteLocation(update);

            Assert.AreEqual(0, _Database.GetLocations().Count);
        }
        #endregion

        #region GetSessions
        protected void BaseStationDatabase_GetSessions_Retrieves_All_Records_In_Sessions_Table()
        {
            var location1 = PrepareLocationReference(new BaseStationLocation() { LocationName = "A" }).LocationID;
            var location2 = PrepareLocationReference(new BaseStationLocation() { LocationName = "B" }).LocationID;
            var startTime1 = DateTime.Now;
            var startTime2 = DateTime.Now.AddYears(1);
            var endTime1 = startTime1.AddSeconds(10);
            var endTime2 = startTime2.AddMinutes(10);

            AddSession(new BaseStationSession() { LocationID = location1, StartTime = startTime1, EndTime = endTime1 });
            AddSession(new BaseStationSession() { LocationID = location2, StartTime = startTime2, EndTime = endTime2 });

            var sessions = _Database.GetSessions();

            Assert.AreEqual(2, sessions.Count);
            Assert.IsTrue(sessions.Any(s => s.LocationID == location1 && s.StartTime == startTime1 && s.EndTime == endTime1 && s.SessionID != 0));
            Assert.IsTrue(sessions.Any(s => s.LocationID == location2 && s.StartTime == startTime2 && s.EndTime == endTime2 && s.SessionID != 0));
        }
        #endregion

        #region InsertSession
        protected void BaseStationDatabase_InsertSession_Throws_If_Writes_Disabled()
        {
            _Database.InsertSession(new BaseStationSession());
        }

        protected void BaseStationDatabase_InsertSession_Inserts_Record_Correctly()
        {
            _Database.WriteSupportEnabled = true;
            var locationId = (int)AddLocation(new BaseStationLocation() { LocationName = "X" });
            var startTime = new DateTime(2001, 2, 3, 4, 5, 6, 789);
            var endTime = startTime.AddDays(1);

            var record = new BaseStationSession() {
                EndTime = endTime,
                LocationID = locationId,
                StartTime = startTime,
            };

            _Database.InsertSession(record);
            Assert.AreNotEqual(0, record.SessionID);

            var allSessions = _Database.GetSessions();
            Assert.AreEqual(1, allSessions.Count);
            var readBack = allSessions[0];
            Assert.AreEqual(record.SessionID, readBack.SessionID);
            Assert.AreEqual(locationId, readBack.LocationID);
            Assert.AreEqual(TruncateDate(startTime), readBack.StartTime);
            Assert.AreEqual(TruncateDate(endTime), readBack.EndTime);
        }

        protected void BaseStationDatabase_InsertSession_Works_For_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    RunTestCleanup();
                    RunTestInitialise();

                    try {
                        BaseStationDatabase_InsertSession_Inserts_Record_Correctly();
                    } catch(Exception ex) {
                        throw new InvalidOperationException($"Exception thrown when culture was {culture}", ex);
                    }
                }
            }
        }

        protected void BaseStationDatabase_InsertSession_Copes_If_There_Are_No_Locations()
        {
            _Database.WriteSupportEnabled = true;
            _Database.InsertSession(new BaseStationSession() {
                LocationID = 0,
                StartTime = DateTime.Now,
            });

            var readBack = _Database.GetSessions()[0];
            Assert.AreEqual(0, readBack.LocationID);
            Assert.AreNotEqual(0, readBack.SessionID);
        }
        #endregion

        #region UpdateSession
        protected void BaseStationDatabase_UpdateSession_Throws_If_Writes_Disabled()
        {
            var locationId = (int)AddLocation(new BaseStationLocation() { LocationName = "X" });
            var session = new BaseStationSession() { LocationID = locationId, StartTime = DateTime.Now };
            session.SessionID = (int)AddSession(session);

            session.EndTime = DateTime.Now;
            _Database.UpdateSession(session);
        }

        protected void BaseStationDatabase_UpdateSession_Correctly_Updates_Record(bool timeGetsRounded)
        {
            _Database.WriteSupportEnabled = true;
            var locationId = (int)AddLocation(new BaseStationLocation() { LocationName = "X" });
            var newLocationId = (int)AddLocation(new BaseStationLocation() { LocationName = "Y" });
            AddSession(new BaseStationSession() {
                EndTime = null,
                LocationID = locationId,
                StartTime = new DateTime(2001, 2, 3, 4, 5, 6, 789),
            });

            var endTime =   new DateTime(2007, 8, 9, 10, 11, 12, 772);
            var startTime = new DateTime(2006, 7, 8, 9,  10, 11, 124);
            var expectedEndTime =   timeGetsRounded ? new DateTime(2007, 8, 9, 10, 11, 12) : endTime;
            var expectedStartTime = timeGetsRounded ? new DateTime(2006, 7, 8, 9, 10, 11) : startTime;

            var update = _Database.GetSessions()[0];
            var originalId = update.SessionID;
            update.EndTime = endTime;
            update.LocationID = newLocationId;
            update.StartTime = startTime;

            _Database.UpdateSession(update);

            var allSessions = _Database.GetSessions();
            Assert.AreEqual(1, allSessions.Count);
            var readBack = allSessions[0];
            Assert.AreEqual(originalId, readBack.SessionID);
            Assert.AreEqual(expectedEndTime, readBack.EndTime);
            Assert.AreEqual(newLocationId, readBack.LocationID);
            Assert.AreEqual(expectedStartTime, readBack.StartTime);
        }

        protected void BaseStationDatabase_UpdateSession_Works_For_Different_Cultures(bool timeGetsRounded)
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    RunTestCleanup();
                    RunTestInitialise();

                    try {
                        BaseStationDatabase_UpdateSession_Correctly_Updates_Record(timeGetsRounded);
                    } catch(Exception ex) {
                        throw new InvalidOperationException($"Exception thrown when culture was {culture}", ex);
                    }
                }
            }
        }
        #endregion

        #region DeleteSession
        protected void BaseStationDatabase_DeleteSession_Throws_If_Writes_Disabled()
        {
            var locationId = (int)AddLocation(new BaseStationLocation() { LocationName = "X" });
            var session = new BaseStationSession() { LocationID = locationId, StartTime = DateTime.Now };
            session.SessionID = (int)AddSession(session);

            _Database.DeleteSession(session);
        }

        protected void BaseStationDatabase_DeleteSession_Correctly_Deletes_Record()
        {
            _Database.WriteSupportEnabled = true;
            var locationId = (int)AddLocation(new BaseStationLocation() { LocationName = "X" });
            AddSession(new BaseStationSession() {
                EndTime = null,
                LocationID = locationId,
                StartTime = new DateTime(2001, 2, 3, 4, 5, 6),
            });

            var record = _Database.GetSessions()[0];
            _Database.DeleteSession(record);

            Assert.AreEqual(0, _Database.GetSessions().Count);
        }
        #endregion

        #region Transactions
        protected void BaseStationDatabase_Transactions_Can_Commit_Operations_To_Database()
        {
            _Database.WriteSupportEnabled = true;
            _Database.PerformInTransaction(() => {
                _Database.InsertAircraft(new BaseStationAircraft() { ModeS = "Z" });
                return true;
            });

            var aircraft = _Database.GetAircraftByCode("Z");
            Assert.AreEqual("Z", aircraft.ModeS);
            Assert.AreNotEqual(0, aircraft.AircraftID);
        }

        protected void BaseStationDatabase_Transactions_Can_Rollback_Inserts()
        {
            _Database.WriteSupportEnabled = true;
            _Database.PerformInTransaction(() => {
                _Database.InsertAircraft(new BaseStationAircraft() { ModeS = "Z" });
                return false;
            });

            Assert.AreEqual(null, _Database.GetAircraftByCode("Z"));
        }

        protected void BaseStationDatabase_Transactions_Cannot_Be_Nested()
        {
            _Database.WriteSupportEnabled = true;
            _Database.PerformInTransaction(() => {
                _Database.InsertAircraft(new BaseStationAircraft() { ModeS = "Z" });

                _Database.PerformInTransaction(() => {
                    var aircraft = _Database.GetAircraftByCode("Z");
                    aircraft.Registration = "P";
                    _Database.UpdateAircraft(aircraft);
                    return true;
                });

                return true;
            });
        }
        #endregion
    }
}
