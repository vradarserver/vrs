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
using VirtualRadar.Interface.Database;
using System.IO;
using Test.Framework;
using System.Data.SQLite;
using Moq;
using System.Reflection;
using System.Threading;
using System.Globalization;
using InterfaceFactory;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Database
{
    [TestClass]
    public class BaseStationDatabaseTests
    {
        #region Fields, TestContext etc.
        public TestContext TestContext { get; set; }

        private IClassFactory _OriginalClassFactory;
        private IBaseStationDatabase _Database;
        private Mock<IBaseStationDatabaseProvider> _Provider;
        private string _EmptyDatabaseFileName;
        private SQLiteConnectionStringBuilder _ConnectionStringBuilder;
        private SearchBaseStationCriteria _Criteria;
        private readonly string[] _SortColumns = new string[] { "callsign", "country", "date", "model", "type", "operator", "reg", "icao" };
        private string _CreateDatabaseFileName;
        private Mock<IConfigurationStorage> _ConfigurationStorage;
        private Configuration _Configuration;
        private EventRecorder<EventArgs> _FileNameChangingEvent;
        private EventRecorder<EventArgs> _FileNameChangedEvent;
        private EventRecorder<EventArgs<BaseStationAircraft>> _AircraftUpdatedEvent;
        private readonly string[] _Cultures = new string[] { "en-GB", "de-DE", "fr-FR", "it-IT", "el-GR", "ru-RU" };
        private Mock<IRuntimeEnvironment> _RuntimeEnvironment;
        private Mock<ICallsignParser> _CallsignParser;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalClassFactory = Factory.TakeSnapshot();
            _RuntimeEnvironment = TestUtilities.CreateMockSingleton<IRuntimeEnvironment>();

            _ConfigurationStorage = TestUtilities.CreateMockSingleton<IConfigurationStorage>();
            _Configuration = new Configuration();
            _ConfigurationStorage.Setup(s => s.Load()).Returns(_Configuration);

            _CreateDatabaseFileName = Path.Combine(TestContext.TestDeploymentDir, "CreatedDatabase.sqb");
            if(File.Exists(_CreateDatabaseFileName)) File.Delete(_CreateDatabaseFileName);

            _EmptyDatabaseFileName = Path.Combine(TestContext.TestDeploymentDir, "TestCopyBaseStation.sqb");
            File.Copy(Path.Combine(TestContext.TestDeploymentDir, "BaseStation.sqb"), _EmptyDatabaseFileName, true);

            _Database = Factory.Singleton.Resolve<IBaseStationDatabase>();
            _Database.FileName = _EmptyDatabaseFileName;

            _Provider = new Mock<IBaseStationDatabaseProvider>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _Database.Provider = _Provider.Object;
            _Provider.Setup(p => p.UtcNow).Returns(DateTime.UtcNow);

            _ConnectionStringBuilder = new SQLiteConnectionStringBuilder() { DataSource = _EmptyDatabaseFileName };

            _CallsignParser = TestUtilities.CreateMockImplementation<ICallsignParser>();

            _Criteria = new SearchBaseStationCriteria() {
                Date = new FilterRange<DateTime>(DateTime.MinValue, DateTime.MaxValue),
            };

            _FileNameChangingEvent = new EventRecorder<EventArgs>();
            _FileNameChangedEvent = new EventRecorder<EventArgs>();
            _AircraftUpdatedEvent = new EventRecorder<EventArgs<BaseStationAircraft>>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalClassFactory);

            if(_Database != null) {
                _Database.Dispose();
                _Database = null;
            }
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Removes the milliseconds from a date.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        /// <remarks>
        /// The library stores all dates and times with the milliseconds stripped off - this helps keep
        /// compatibility with some 3rd party utilities.
        /// </remarks>
        private DateTime TruncateDate(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
        }

        /// <summary>
        /// Removes the milliseconds from a nullable date.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private DateTime? TruncateDate(DateTime? date)
        {
            return date == null ? (DateTime?)null : TruncateDate(date.Value);
        }

        private BaseStationAircraft CreateAircraft(string icao24 = "123456", string registration = "G-VRST")
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
        private BaseStationAircraft LoadAircraftFromSpreadsheet(ExcelWorksheetData worksheet, int firstOrdinal = 0, BaseStationAircraft copyIntoAircraft = null)
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
            result.UserTag = worksheet.EString(ordinal++);
            result.YearBuilt = worksheet.EString(ordinal++);

            return result;
        }

        private long AddAircraft(BaseStationAircraft aircraft)
        {
            long result = 0;

            using(var connection = new SQLiteConnection(_ConnectionStringBuilder.ConnectionString)) {
                connection.Open();

                using(var command = connection.CreateCommand()) {
                    var fieldNames = new StringBuilder();
                    var parameters = new StringBuilder();

                    foreach(var property in typeof(BaseStationAircraft).GetProperties()) {
                        var fieldName = property.Name;
                        if(fieldName == "AircraftID") continue;

                        if(fieldNames.Length > 0) fieldNames.Append(',');
                        if(parameters.Length > 0) parameters.Append(',');

                        fieldNames.AppendFormat("[{0}]", fieldName);
                        parameters.Append('?');

                        var parameter = command.CreateParameter();
                        parameter.Value = property.GetValue(aircraft, null);
                        command.Parameters.Add(parameter);
                    }


                    command.CommandText = String.Format("INSERT INTO [Aircraft] ({0}) VALUES ({1}); SELECT last_insert_rowid();", fieldNames, parameters);

                    result = (long)command.ExecuteScalar();
                    aircraft.AircraftID = (int)result;
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a flight object.
        /// </summary>
        /// <param name="actual"></param>
        /// <returns></returns>
        private BaseStationFlight CreateFlight(BaseStationAircraft aircraft = null, string id = null)
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
        private BaseStationFlight CreateFlight(string id, bool setCallsign = true, bool setRegistration = true)
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
        private BaseStationFlight LoadFlightFromSpreadsheet(ExcelWorksheetData worksheet, int firstOrdinal = 0, BaseStationFlight copyIntoFlight = null)
        {
            int ordinal = firstOrdinal;

            var aircraft = CreateAircraft();
            aircraft.AircraftID = worksheet.Int(ordinal++);

            var result = copyIntoFlight == null ? CreateFlight(aircraft) : copyIntoFlight;
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

            return result;
        }

        private long AddFlight(BaseStationFlight flight)
        {
            long result = 0;

            using(var connection = new SQLiteConnection(_ConnectionStringBuilder.ConnectionString)) {
                connection.Open();

                using(var command = connection.CreateCommand()) {
                    var fieldNames = new StringBuilder();
                    var parameters = new StringBuilder();

                    foreach(var property in typeof(BaseStationFlight).GetProperties()) {
                        var fieldName = property.Name;
                        object value = property.GetValue(flight, null);

                        if(fieldName == "FlightID") continue;
                        else if(fieldName == "Aircraft") continue;

                        if(fieldNames.Length > 0) fieldNames.Append(',');
                        if(parameters.Length > 0) parameters.Append(',');

                        fieldNames.AppendFormat("[{0}]", fieldName);
                        parameters.Append('?');

                        var parameter = command.CreateParameter();
                        parameter.Value = value;
                        command.Parameters.Add(parameter);
                    }

                    command.CommandText = String.Format("INSERT INTO [Flights] ({0}) VALUES ({1}); SELECT last_insert_rowid();", fieldNames, parameters);

                    result = (long)command.ExecuteScalar();
                    flight.FlightID = (int)result;
                }
            }

            return result;
        }

        private void AddFlightAndAircraft(BaseStationFlight flight)
        {
            flight.AircraftID = (int)AddAircraft(flight.Aircraft);
            AddFlight(flight);
        }

        private long AddDBHistory(BaseStationDBHistory dbHistory)
        {
            long result = 0;

            using(var connection = new SQLiteConnection(_ConnectionStringBuilder.ConnectionString)) {
                connection.Open();

                using(var command = connection.CreateCommand()) {
                    command.CommandText = "INSERT INTO [DBHistory] ([TimeStamp], [Description]) VALUES (?,?); SELECT last_insert_rowid();";
                    command.Parameters.Add(new SQLiteParameter() { Value = dbHistory.TimeStamp });
                    command.Parameters.Add(new SQLiteParameter() { Value = dbHistory.Description });

                    result = (long)command.ExecuteScalar();
                    dbHistory.DBHistoryID = (int)result;
                }
            }

            return result;
        }

        private long AddDBInfo(BaseStationDBInfo dbInfo)
        {
            long result = 0;

            using(var connection = new SQLiteConnection(_ConnectionStringBuilder.ConnectionString)) {
                connection.Open();

                using(var command = connection.CreateCommand()) {
                    command.CommandText = "INSERT INTO [DBInfo] ([OriginalVersion], [CurrentVersion]) VALUES (?,?); SELECT last_insert_rowid();";
                    command.Parameters.Add(new SQLiteParameter() { Value = dbInfo.OriginalVersion });
                    command.Parameters.Add(new SQLiteParameter() { Value = dbInfo.CurrentVersion });

                    result = (long)command.ExecuteScalar();
                }
            }

            return result;
        }

        private long AddSystemEvent(BaseStationSystemEvents systemEvent)
        {
            long result = 0;

            using(var connection = new SQLiteConnection(_ConnectionStringBuilder.ConnectionString)) {
                connection.Open();

                using(var command = connection.CreateCommand()) {
                    command.CommandText = "INSERT INTO [SystemEvents] ([App], [Msg], [TimeStamp]) VALUES (?,?,?); SELECT last_insert_rowid();";
                    command.Parameters.Add(new SQLiteParameter() { Value = systemEvent.App });
                    command.Parameters.Add(new SQLiteParameter() { Value = systemEvent.Msg });
                    command.Parameters.Add(new SQLiteParameter() { Value = systemEvent.TimeStamp });

                    result = (long)command.ExecuteScalar();
                    systemEvent.SystemEventsID = (int)result;
                }
            }

            return result;
        }

        private long AddLocation(BaseStationLocation location)
        {
            long result = 0;

            using(var connection = new SQLiteConnection(_ConnectionStringBuilder.ConnectionString)) {
                connection.Open();

                using(var command = connection.CreateCommand()) {
                    command.CommandText = "INSERT INTO [Locations] ([Altitude], [Latitude], [LocationName], [Longitude]) VALUES (?,?,?,?); SELECT last_insert_rowid();";
                    command.Parameters.Add(new SQLiteParameter() { Value = location.Altitude });
                    command.Parameters.Add(new SQLiteParameter() { Value = location.Latitude });
                    command.Parameters.Add(new SQLiteParameter() { Value = location.LocationName });
                    command.Parameters.Add(new SQLiteParameter() { Value = location.Longitude });

                    result = (long)command.ExecuteScalar();
                    location.LocationID = (int)result;
                }
            }

            return result;
        }

        private long AddSession(BaseStationSession session)
        {
            long result = 0;

            using(var connection = new SQLiteConnection(_ConnectionStringBuilder.ConnectionString)) {
                connection.Open();

                using(var command = connection.CreateCommand()) {
                    command.CommandText = "INSERT INTO [Sessions] ([LocationID], [StartTime], [EndTime]) VALUES (?,?,?); SELECT last_insert_rowid();";
                    command.Parameters.Add(new SQLiteParameter() { Value = session.LocationID });
                    command.Parameters.Add(new SQLiteParameter() { Value = session.StartTime });
                    command.Parameters.Add(new SQLiteParameter() { Value = session.EndTime });

                    result = (long)command.ExecuteScalar();
                    session.SessionID = (int)result;
                }
            }

            return result;
        }
        #endregion

        #region Constructors and properties
        [TestMethod]
        public void BaseStationDatabase_Constructor_Initialises_To_Known_Values_And_Properties_Work()
        {
            _Database.Dispose();
            _Database = Factory.Singleton.Resolve<IBaseStationDatabase>();

            Assert.IsNotNull(_Database.Provider);
            TestUtilities.TestProperty(_Database, "Provider", _Database.Provider, _Provider.Object);

            Assert.AreEqual(null, _Database.FileName);
            Assert.IsFalse(_Database.IsConnected);
            Assert.IsFalse(_Database.WriteSupportEnabled);
        }

        [TestMethod]
        public void BaseStationDatabase_IsConnected_Indicates_State_Of_Connection()
        {
            Assert.IsFalse(_Database.IsConnected);
            _Database.GetAircraftByCode("000000");
            Assert.IsTrue(_Database.IsConnected);
        }

        [TestMethod]
        public void BaseStationDatabase_FileName_Change_Closes_Connection_If_Changed()
        {
            _Database.GetAircraftByCode("000000");
            _Database.FileName = _CreateDatabaseFileName;
            Assert.IsFalse(_Database.IsConnected);
        }

        [TestMethod]
        public void BaseStationDatabase_FileName_Set_Does_Not_Close_Connection_If_Value_Unchanged()
        {
            _Database.GetAircraftByCode("000000");
            _Database.FileName = _Database.FileName;
            Assert.IsTrue(_Database.IsConnected);
        }

        [TestMethod]
        public void BaseStationDatabase_FileName_Change_Can_Allow_Methods_To_Start_Returning_Data()
        {
            AddAircraft(CreateAircraft("123456", "REG"));

            _Database.FileName = "c:\\DoesNotExist\\ThisFileDoesNotExist.ifitdoesthenitwillbetremendousbadluck";

            Assert.IsNull(_Database.GetAircraftByRegistration("REG"));
            Assert.IsFalse(_Database.IsConnected);

            _Database.FileName = _EmptyDatabaseFileName;

            Assert.IsNotNull(_Database.GetAircraftByRegistration("REG"));
            Assert.IsTrue(_Database.IsConnected);
        }

        [TestMethod]
        public void BaseStationDatabse_WriteSupportEnabled_Closes_Connection_If_Changed()
        {
            _Database.GetAircraftByCode("000000");
            _Database.WriteSupportEnabled = true;
            Assert.AreEqual(true, _Database.WriteSupportEnabled);
            Assert.IsFalse(_Database.IsConnected);
        }

        [TestMethod]
        public void BaseStationDatabse_WriteSupportEnabled_Leaves_Connection_If_Not_Changed()
        {
            _Database.GetAircraftByCode("000000");
            _Database.WriteSupportEnabled = false;
            Assert.AreEqual(false, _Database.WriteSupportEnabled);
            Assert.IsTrue(_Database.IsConnected);
        }
        #endregion

        #region MaxParameters
        [TestMethod]
        public void BaseStationDatabase_MaxParameters_Returns_Minus_One_When_Disconnected()
        {
            File.Delete(_EmptyDatabaseFileName);
            Assert.AreEqual(-1, _Database.MaxParameters);
        }

        [TestMethod]
        public void BaseStationDatabase_MaxParameters_Returns_Correct_Value_When_Connected()
        {
            // SQLite's max parameters is a compile time variable. It can be fetched via a C function call but
            // that's a little tricky to do with ADO.NET when you can't use interop, so we're hard-coding to
            // the default, which is 999. The database object lops off a few parameters for internal use,
            // leaving a result of 900. This will need adjusting for other engines.
            Assert.AreEqual(900, _Database.MaxParameters);
        }
        #endregion

        #region FileNameChanging event
        [TestMethod]
        public void BaseStationDatabase_FileNameChanging_Raised_Before_FileName_Changes()
        {
            _Database.FileName = "OLD";
            _Database.FileNameChanging += _FileNameChangingEvent.Handler;
            _FileNameChangingEvent.EventRaised += (s, a) => {
                Assert.AreEqual("OLD", _Database.FileName);
            };

            _Database.FileName = "NEW";

            Assert.AreEqual(1, _FileNameChangingEvent.CallCount);
            Assert.AreSame(_Database, _FileNameChangingEvent.Sender);
        }

        [TestMethod]
        public void BaseStationDatabase_FileNameChanging_Not_Raised_If_FileName_Does_Not_Change()
        {
            _Database.FileName = "OLD";
            _Database.FileNameChanging += _FileNameChangingEvent.Handler;

            _Database.FileName = "OLD";

            Assert.AreEqual(0, _FileNameChangingEvent.CallCount);
        }
        #endregion

        #region FileNameChanged event
        [TestMethod]
        public void BaseStationDatabase_FileNameChanged_Raised_After_FileName_Changes()
        {
            _Database.FileName = "OLD";
            _Database.FileNameChanged += _FileNameChangedEvent.Handler;
            _FileNameChangedEvent.EventRaised += (s, a) => {
                Assert.AreEqual("NEW", _Database.FileName);
            };

            _Database.FileName = "NEW";

            Assert.AreEqual(1, _FileNameChangedEvent.CallCount);
            Assert.AreSame(_Database, _FileNameChangedEvent.Sender);
        }

        [TestMethod]
        public void BaseStationDatabase_FileNameChanged_Not_Raised_If_FileName_Does_Not_Change()
        {
            _Database.FileName = "OLD";
            _Database.FileNameChanged += _FileNameChangedEvent.Handler;

            _Database.FileName = "OLD";

            Assert.AreEqual(0, _FileNameChangedEvent.CallCount);
        }
        #endregion

        #region Dispose
        [TestMethod]
        public void BaseStationDatabaseTests_Dispose_Prevents_Reopening_Of_Connection()
        {
            var flight = CreateFlight("ABC123");
            AddAircraft(flight.Aircraft);
            AddFlight(flight);
            _Database.GetFlights(_Criteria, -1, -1, null, false, null, false);

            _Database.Dispose();

            var flights = _Database.GetFlights(_Criteria, -1, -1, null, false, null, false);
            Assert.AreEqual(0, flights.Count);
        }
        #endregion

        #region TestConnection
        [TestMethod]
        public void BaseStationDatabase_TestConnection_Returns_False_If_File_Could_Not_Be_Opened()
        {
            File.Delete(_EmptyDatabaseFileName);
            Assert.IsFalse(_Database.TestConnection());
        }

        [TestMethod]
        public void BaseStationDatabase_TestConnection_Returns_True_If_File_Could_Be_Opened()
        {
            Assert.IsTrue(_Database.TestConnection());
        }

        [TestMethod]
        public void BaseStationDatabase_TestConnection_Leaves_Connection_Open()
        {
            Assert.IsTrue(_Database.TestConnection());
            Assert.IsTrue(_Database.IsConnected);

            bool seenConnectionOpen = false;
            try {
                File.Delete(_EmptyDatabaseFileName);
            } catch(IOException) {
                seenConnectionOpen = true;
            }
            Assert.IsTrue(seenConnectionOpen);
        }
        #endregion

        #region Connection
        [TestMethod]
        public void BaseStationDatabase_Connection_Does_Not_Create_File_If_Missing()
        {
            File.Delete(_EmptyDatabaseFileName);
            _Database.GetAircraftByRegistration("G-ABCD");
            Assert.IsFalse(File.Exists(_EmptyDatabaseFileName));
        }

        [TestMethod]
        public void BaseStationDatabase_Connection_Does_Not_Try_To_Use_Zero_Length_Files()
        {
            File.Delete(_EmptyDatabaseFileName);
            File.Create(_EmptyDatabaseFileName).Close();

            Assert.IsNull(_Database.GetAircraftByRegistration("G-ABCD"));
            Assert.AreEqual(false, _Database.IsConnected);
        }

        [TestMethod]
        public void BaseStationDatabase_Connection_Can_Work_With_ReadOnly_Access()
        {
            var fileInfo = new FileInfo(_EmptyDatabaseFileName);
            try {
                AddAircraft(CreateAircraft("ABCDEF", "G-AGWP"));
                fileInfo.IsReadOnly = true;
                Assert.IsNotNull(_Database.GetAircraftByRegistration("G-AGWP"));
            } finally {
                fileInfo.IsReadOnly = false;
            }
        }
        #endregion

        #region GetAircraftByRegistration
        [TestMethod]
        public void BaseStationDatatbase_GetAircraftByRegistration_Returns_Null_If_Passed_Null()
        {
            Assert.IsNull(_Database.GetAircraftByRegistration(null));
        }

        [TestMethod]
        public void BaseStationDatabase_GetAircraftByRegistration_Returns_Null_If_Aircraft_Does_Not_Exist()
        {
            Assert.IsNull(_Database.GetAircraftByRegistration("REG"));
        }

        [TestMethod]
        public void BaseStationDatabase_GetAircraftByRegistration_Returns_Null_If_Database_File_Does_Not_Exist()
        {
            File.Delete(_EmptyDatabaseFileName);
            Assert.IsNull(_Database.GetAircraftByRegistration("REG"));
        }

        [TestMethod]
        public void BaseStationDatabase_GetAircraftByRegistration_Returns_Null_If_Database_File_Not_Configured()
        {
            _Database.FileName = null;
            Assert.IsNull(_Database.GetAircraftByRegistration("REG"));
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void BaseStationDatabase_GetAircraftByRegistration_Returns_Aircraft_Object_For_Registration()
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
        [TestMethod]
        public void BaseStationDatatbase_GetAircraftByCode_Returns_Null_If_Passed_Null()
        {
            Assert.IsNull(_Database.GetAircraftByCode(null));
        }

        [TestMethod]
        public void BaseStationDatabase_GetAircraftByCode_Returns_Null_If_Aircraft_Does_Not_Exist()
        {
            Assert.IsNull(_Database.GetAircraftByCode("ABC123"));
        }

        [TestMethod]
        public void BaseStationDatabase_GetAircraftByCode_Returns_Null_If_File_Does_Not_Exist()
        {
            File.Delete(_EmptyDatabaseFileName);
            Assert.IsNull(_Database.GetAircraftByCode("ABC123"));
        }

        [TestMethod]
        public void BaseStationDatabase_GetAircraftByCode_Returns_Null_If_File_Not_Configured()
        {
            _Database.FileName = null;
            Assert.IsNull(_Database.GetAircraftByCode("ABC123"));
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void BaseStationDatabase_GetAircraftByCode_Returns_Aircraft_Object_For_ICAO24_Code()
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
        [TestMethod]
        public void BaseStationDatatbase_GetManyAircraftByCode_Returns_Empty_Collection_If_Passed_Null()
        {
            Assert.AreEqual(0, _Database.GetManyAircraftByCode(null).Count);
        }

        [TestMethod]
        public void BaseStationDatabase_GetManyAircraftByCode_Returns_Empty_Collection_If_Aircraft_Does_Not_Exist()
        {
            Assert.AreEqual(0, _Database.GetManyAircraftByCode(new string[] { "ABC123" }).Count);
        }

        [TestMethod]
        public void BaseStationDatabase_GetManyAircraftByCode_Returns_Empty_Collection_If_File_Does_Not_Exist()
        {
            File.Delete(_EmptyDatabaseFileName);
            Assert.AreEqual(0, _Database.GetManyAircraftByCode(new string[] { "ABC123" }).Count);
        }

        [TestMethod]
        public void BaseStationDatabase_GetManyAircraftByCode_Returns_Empty_Collection_If_File_Not_Configured()
        {
            _Database.FileName = null;
            Assert.AreEqual(0, _Database.GetManyAircraftByCode(new string[] { "ABC123" }).Count);
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void BaseStationDatabase_GetManyAircraftByCode_Returns_Aircraft_Object_For_ICAO24_Code()
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

        [TestMethod]
        public void BaseStationDatabase_GetManyAircraftByCode_Can_Return_More_Than_One_Aircraft()
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

            var firstAndLast = _Database.GetManyAircraftByCode(new string[] { "ABC123", "XYZ789" });

            Assert.AreEqual(2, firstAndLast.Count);
            Assert.IsTrue(firstAndLast.Where(r => r.Value.Registration == "ABC123").Any());
            Assert.IsTrue(firstAndLast.Where(r => r.Value.Registration == "XYZ789").Any());
        }

        [TestMethod]
        public void BaseStationDatabase_GetManyAircraftByCode_Transparently_Handles_Call_Splitting_When_Number_Of_Icaos_Exceeds_MaxParameters()
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

            var allAircraft = _Database.GetManyAircraftByCode(icaos);

            Assert.AreEqual(2, allAircraft.Count);
            Assert.IsNotNull(allAircraft["ABC123"]);
            Assert.IsNotNull(allAircraft["XYZ789"]);
        }
        #endregion

        #region GetManyAircraftAndFlightsCountByCode
        [TestMethod]
        public void BaseStationDatatbase_GetManyAircraftAndFlightsCountByCode_Returns_Empty_Collection_If_Passed_Null()
        {
            Assert.AreEqual(0, _Database.GetManyAircraftAndFlightsCountByCode(null).Count);
        }

        [TestMethod]
        public void BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Returns_Empty_Collection_If_Aircraft_Does_Not_Exist()
        {
            Assert.AreEqual(0, _Database.GetManyAircraftAndFlightsCountByCode(new string[] { "ABC123" }).Count);
        }

        [TestMethod]
        public void BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Returns_Empty_Collection_If_File_Does_Not_Exist()
        {
            File.Delete(_EmptyDatabaseFileName);
            Assert.AreEqual(0, _Database.GetManyAircraftAndFlightsCountByCode(new string[] { "ABC123" }).Count);
        }

        [TestMethod]
        public void BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Returns_Empty_Collection_If_File_Not_Configured()
        {
            _Database.FileName = null;
            Assert.AreEqual(0, _Database.GetManyAircraftAndFlightsCountByCode(new string[] { "ABC123" }).Count);
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Returns_Aircraft_Object_For_ICAO24_Code()
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

        [TestMethod]
        public void BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Can_Return_More_Than_One_Aircraft()
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

        [TestMethod]
        public void BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Returns_Counts_Of_Flights()
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

        [TestMethod]
        public void BaseStationDatabase_GetManyAircraftAndFlightsCountByCode_Transparently_Handles_Call_Splitting_When_Number_Of_Icaos_Exceeds_MaxParameters()
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
        [TestMethod]
        public void BaseStationDatabase_GetAircraftById_Returns_Null_If_Aircraft_Does_Not_Exist()
        {
            Assert.IsNull(_Database.GetAircraftById(1));
        }

        [TestMethod]
        public void BaseStationDatabase_GetAircraftById_Returns_Null_If_File_Does_Not_Exist()
        {
            File.Delete(_EmptyDatabaseFileName);
            Assert.IsNull(_Database.GetAircraftById(1));
        }

        [TestMethod]
        public void BaseStationDatabase_GetAircraftById_Returns_Null_If_File_Not_Configured()
        {
            _Database.FileName = null;
            Assert.IsNull(_Database.GetAircraftById(1));
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void BaseStationDatabase_GetAircraftById_Returns_Aircraft_Object_For_Record_Identifier()
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
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BaseStationDatabase_InsertAircraft_Throws_If_Writes_Disabled()
        {
            _Database.InsertAircraft(new BaseStationAircraft() { ModeS = "123456" });
        }

        [TestMethod]
        public void BaseStationDatabase_InsertAircraft_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            File.Delete(_EmptyDatabaseFileName);

            _Database.InsertAircraft(new BaseStationAircraft() { ModeS = "123456" });
            Assert.AreEqual(null, _Database.GetAircraftByCode("123456"));
        }

        [TestMethod]
        public void BaseStationDatabase_InsertAircraft_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.InsertAircraft(new BaseStationAircraft() { ModeS = "X" });
            Assert.AreEqual(null, _Database.GetAircraftByCode("X"));
        }

        [TestMethod]
        public void BaseStationDatabase_InsertAircraft_Truncates_Milliseconds_From_Date()
        {
            _Database.WriteSupportEnabled = true;

            var time1 = new DateTime(2001, 2, 3, 4, 5, 6, 789);
            var time2 = new DateTime(2009, 8, 7, 6, 5, 4, 321);

            _Database.InsertAircraft(new BaseStationAircraft() { ModeS = "X", FirstCreated = time1, LastModified = time2 });
            var readBack = _Database.GetAircraftByCode("X");

            Assert.AreEqual(TruncateDate(time1), readBack.FirstCreated);
            Assert.AreEqual(TruncateDate(time2), readBack.LastModified);
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void BaseStationDatabase_InsertAircraft_Correctly_Inserts_Record()
        {
            _Database.WriteSupportEnabled = true;

            var worksheet = new ExcelWorksheetData(TestContext);
            var aircraft = LoadAircraftFromSpreadsheet(worksheet);

            _Database.InsertAircraft(aircraft);
            Assert.AreNotEqual(0, aircraft.AircraftID);

            var readBack = _Database.GetAircraftById(aircraft.AircraftID);
            AssertAircraftAreEqual(aircraft, readBack);
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void BaseStationDatabase_InsertAircraft_Works_For_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    TestCleanup();
                    TestInitialise();

                    try {
                        BaseStationDatabase_InsertAircraft_Correctly_Inserts_Record();
                    } catch(Exception ex) {
                        throw new InvalidOperationException(String.Format("Exception thrown when culture was {0}", culture), ex);
                    }
                }
            }
        }
        #endregion

        #region UpdateAircraft
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BaseStationDatabase_UpdateAircraft_Throws_If_Writes_Disabled()
        {
            var aircraft = new BaseStationAircraft() { ModeS = "X" };
            aircraft.AircraftID = (int)AddAircraft(aircraft);

            aircraft.Registration = "C";
            _Database.UpdateAircraft(aircraft);
        }

        [TestMethod]
        public void BaseStationDatabase_UpdateAircraft_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            File.Delete(_EmptyDatabaseFileName);

            _Database.UpdateAircraft(new BaseStationAircraft() { ModeS = "X" });
            Assert.AreEqual(null, _Database.GetAircraftByCode("X"));
        }

        [TestMethod]
        public void BaseStationDatabase_UpdateAircraft_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.UpdateAircraft(new BaseStationAircraft() { ModeS = "X" });
            Assert.AreEqual(null, _Database.GetAircraftByCode("X"));
        }

        [TestMethod]
        public void BaseStationDatabase_UpdateAircraft_Truncates_Milliseconds_From_Date()
        {
            _Database.WriteSupportEnabled = true;

            _Database.InsertAircraft(new BaseStationAircraft() { ModeS = "X" });
            var time1 = new DateTime(2001, 2, 3, 4, 5, 6, 789);
            var time2 = new DateTime(2009, 8, 7, 6, 5, 4, 321);

            var update = _Database.GetAircraftByCode("X");
            update.FirstCreated = time1;
            update.LastModified = time2;
            _Database.UpdateAircraft(update);

            var readBack = _Database.GetAircraftByCode("X");
            Assert.AreEqual(TruncateDate(time1), readBack.FirstCreated);
            Assert.AreEqual(TruncateDate(time2), readBack.LastModified);
        }

        [TestMethod]
        public void BaseStationDatabase_UpdateAircraft_Raises_AircraftUpdated()
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

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void BaseStationDatabase_UpdateAircraft_Correctly_Updates_Record()
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

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void BaseStationDatabase_UpdateAircraft_Works_For_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    TestCleanup();
                    TestInitialise();

                    try {
                        BaseStationDatabase_UpdateAircraft_Correctly_Updates_Record();
                    } catch(Exception ex) {
                        throw new InvalidOperationException(String.Format("Exception thrown when culture was {0}", culture), ex);
                    }
                }
            }
        }
        #endregion

        #region UpdateAircraftModeSCountry
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BaseStationDatabase_UpdateAircraftModeSCountry_Throws_If_Writes_Disabled()
        {
            var aircraft = new BaseStationAircraft() { ModeS = "X" };
            aircraft.AircraftID = (int)AddAircraft(aircraft);

            aircraft.Registration = "C";
            _Database.UpdateAircraftModeSCountry(aircraft.AircraftID, "X");
        }

        [TestMethod]
        public void BaseStationDatabase_UpdateAircraftModeSCountry_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.UpdateAircraftModeSCountry(1, "X");
            Assert.AreEqual(null, _Database.GetAircraftByCode("X"));
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void BaseStationDatabase_UpdateAircraftModeSCountry_Updates_ModeSCountry_For_Existing_Record()
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

        #region DeleteAircraft
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BaseStationDatabase_DeleteAircraft_Throws_If_Writes_Disabled()
        {
            var aircraft = new BaseStationAircraft() { ModeS = "X" };
            aircraft.AircraftID = (int)AddAircraft(aircraft);

            _Database.DeleteAircraft(aircraft);
        }

        [TestMethod]
        public void BaseStationDatabase_DeleteAircraft_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            File.Delete(_EmptyDatabaseFileName);

            _Database.DeleteAircraft(new BaseStationAircraft());
            Assert.IsFalse(File.Exists(_EmptyDatabaseFileName));
        }

        [TestMethod]
        public void BaseStationDatabase_DeleteAircraft_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.DeleteAircraft(new BaseStationAircraft());
            // Nothing much we can do to assure ourselves that nothing has changed here
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetAircraftBy$")]
        public void BaseStationDatabase_DeleteAircraft_Correctly_Deletes_Record()
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
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BaseStationDatabase_GetFlights_Throws_If_Criteria_Is_Null()
        {
            _Database.GetFlights(null, -1, -1, null, false, null, false);
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlights$")]
        public void BaseStationDatabase_GetFlights_Copies_Database_Record_To_Flight_Object()
        {
            Do_GetFlightsAllVersions_Copies_Database_Record_To_Flight_Object(true);
        }

        private void Do_GetFlightsAllVersions_Copies_Database_Record_To_Flight_Object(bool getFlights)
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            var mockFlight = LoadFlightFromSpreadsheet(worksheet);

            var aircraftId = (int)AddAircraft(mockFlight.Aircraft);
            mockFlight.AircraftID = aircraftId;
            mockFlight.SessionID = (int)AddSession(new BaseStationSession() { StartTime = DateTime.Now });
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

        [TestMethod]
        public void BaseStationDatabase_GetFlights_Can_Return_List_Of_All_Flights()
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

        [TestMethod]
        public void BaseStationDatabase_GetFlights_Can_Filter_Flights_By_Equality_Criteria()
        {
            var defaultFlight = CreateFlight("default", false);
            var notEqualFlight = CreateFlight("notEquals", false);
            var equalsFlight = CreateFlight("equals", false);

            foreach(var reverseCondition in new bool[] { false, true }) {
                foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                    TestCleanup();
                    TestInitialise();

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

        [TestMethod]
        public void BaseStationDatabase_GetFlights_Can_Filter_Flights_By_Contains_Criteria()
        {
            var defaultFlight = CreateFlight("default", false);
            var notContainsFlight = CreateFlight("notContains", false);
            var containsFlight = CreateFlight("contains", false);

            foreach(var reverseCondition in new bool[] { false, true }) {
                foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                    TestCleanup();
                    TestInitialise();

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

        [TestMethod]
        public void BaseStationDatabase_GetFlights_Can_Filter_Flights_By_StartsWith_Criteria()
        {
            var defaultFlight = CreateFlight("default", false);
            var notStartsWithFlight = CreateFlight("notContains", false);
            var startsWithFlight = CreateFlight("contains", false);

            foreach(var reverseCondition in new bool[] { false, true }) {
                foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                    TestCleanup();
                    TestInitialise();

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

        [TestMethod]
        public void BaseStationDatabase_GetFlights_Can_Filter_Flights_By_EndsWith_Criteria()
        {
            var defaultFlight = CreateFlight("default", false);
            var notEndsWithFlight = CreateFlight("notContains", false);
            var endsWithFlight = CreateFlight("contains", false);

            foreach(var reverseCondition in new bool[] { false, true }) {
                foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                    TestCleanup();
                    TestInitialise();

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

        [TestMethod]
        public void BaseStationDatabase_GetFlights_Can_Filter_Flights_By_Range_Criteria()
        {
            var belowRangeFlight = CreateFlight("belowRange");
            var startRangeFlight = CreateFlight("startRange");
            var inRangeFlight = CreateFlight("inRange");
            var endRangeFlight = CreateFlight("endRange");
            var aboveRangeFlight = CreateFlight("aboveRange");

            foreach(var reverseCondition in new bool[] { false, true }) {
                foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                    TestCleanup();
                    TestInitialise();

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

        [TestMethod]
        public void BaseStationDatabase_GetFlights_String_Properties_Can_Match_Null_Or_Empty_Fields()
        {
            foreach(var reverseCondition in new bool[] { false, true }) {
                foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                    foreach(var filterValue in new string[] { null, "" }) {
                        foreach(var databaseValue in new string[] { null, "" }) {
                            if(IsFilterStringProperty(criteriaProperty)) {
                                if(criteriaProperty.Name == "Icao") continue;     // these can't be set to null on the database

                                TestCleanup();
                                TestInitialise();

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

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "Callsigns$")]
        public void BaseStationDatabase_GetFlights_Can_Search_For_All_Variations_Of_A_Callsign()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

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
            var comments = worksheet.String("Comments");
            Assert.AreEqual(worksheet.Int("Count"), flights.Count, comments);
            for(var i = 1;i <= 3;++i) {
                var expectCallsign = worksheet.EString(String.Format("Expect{0}", i));
                if(expectCallsign != null) {
                    Assert.IsNotNull(flights.Single(r => r.Callsign == expectCallsign), comments);
                }
            }
        }

        [TestMethod]
        public void BaseStationDatabase_GetFlights_Some_Criteria_Is_Case_Insensitive()
        {
            Do_GetFlightsAllVersions_Some_Criteria_Is_Case_Insensitive(true);
        }

        private void Do_GetFlightsAllVersions_Some_Criteria_Is_Case_Insensitive(bool getFlights)
        {
            var flight = CreateFlight("1");
            foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                if(!getFlights && !IsFlightCriteria(criteriaProperty)) continue;

                TestCleanup();
                TestInitialise();

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

        [TestMethod]
        public void BaseStationDatabase_GetFlights_Some_Criteria_Is_Case_Sensitive()
        {
            Do_GetFlightsAllVersions_Some_Criteria_Is_Case_Sensitive(true);
        }

        private void Do_GetFlightsAllVersions_Some_Criteria_Is_Case_Sensitive(bool getFlights)
        {
            var flight = CreateFlight("1");
            foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                if(!getFlights && !IsFlightCriteria(criteriaProperty)) continue;

                TestCleanup();
                TestInitialise();

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

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlightsRows$")]
        public void BaseStationDatabase_GetFlights_Can_Return_Subset_Of_Rows()
        {
            Do_GetFlightsAllVersions_Can_Return_Subset_Of_Rows(true);
        }

        private void Do_GetFlightsAllVersions_Can_Return_Subset_Of_Rows(bool getFlights)
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

        [TestMethod]
        public void BaseStationDatabase_GetFlights_Ignores_Unknown_Sort_Columns()
        {
            Do_GetFlightsAllVersions_Ignores_Unknown_Sort_Columns(true);
        }

        private void Do_GetFlightsAllVersions_Ignores_Unknown_Sort_Columns(bool getFlights)
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

        [TestMethod]
        public void BaseStationDatabase_GetFlights_Ignores_Case_On_Sort_Column_Names()
        {
            Do_GetFlightsAllVersions_Ignores_Case_On_Sort_Column_Names(true);
        }

        private void Do_GetFlightsAllVersions_Ignores_Case_On_Sort_Column_Names(bool getFlights)
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

        [TestMethod]
        public void BaseStationDatabase_GetFlights_Sorts_By_One_Column_Correctly()
        {
            var defaultFlight = CreateFlight("defaultFlight", false);
            var lowFlight = CreateFlight("lowFlight", false);
            var highFlight = CreateFlight("highFlight", false);

            defaultFlight.FirstAltitude = 1;
            lowFlight.FirstAltitude = 2;
            highFlight.FirstAltitude = 3;

            foreach(var sortColumn in _SortColumns) {
                TestCleanup();
                TestInitialise();

                SetSortColumnValue(defaultFlight, sortColumn, true, false);
                SetSortColumnValue(lowFlight, sortColumn, false, false);
                SetSortColumnValue(highFlight, sortColumn, false, true);

                AddFlightAndAircraft(defaultFlight);
                AddFlightAndAircraft(lowFlight);
                AddFlightAndAircraft(highFlight);

                var flights = _Database.GetFlights(_Criteria, -1, -1, sortColumn, true, null, false);
                Assert.AreEqual(3, flights.Count, sortColumn);
                Assert.AreEqual(1, flights[0].FirstAltitude, sortColumn);
                Assert.AreEqual(2, flights[1].FirstAltitude, sortColumn);
                Assert.AreEqual(3, flights[2].FirstAltitude, sortColumn);

                flights = _Database.GetFlights(_Criteria, -1, -1, sortColumn, false, null, false);
                Assert.AreEqual(3, flights.Count, sortColumn);
                Assert.AreEqual(3, flights[0].FirstAltitude, sortColumn);
                Assert.AreEqual(2, flights[1].FirstAltitude, sortColumn);
                Assert.AreEqual(1, flights[2].FirstAltitude, sortColumn);
            }
        }

        [TestMethod]
        public void BaseStationDatabase_GetFlights_Sorts_By_Two_Columns_Correctly()
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
        [TestMethod]
        public void BaseStationDatabase_GetFlightsForAircraft_Returns_Empty_List_If_Aircraft_Is_Null()
        {
            Assert.AreEqual(0, _Database.GetFlightsForAircraft(null, _Criteria, 0, int.MaxValue, null, false, null, false).Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BaseStationDatabase_GetFlightsForAircraft_Throws_If_Criteria_Is_Null()
        {
            _Database.GetFlightsForAircraft(CreateAircraft(), null, 0, int.MaxValue, null, false, null, false);
        }

        [TestMethod]
        public void BaseStationDatabase_GetFlightsForAircraft_Restricts_Search_To_Single_Aircraft()
        {
            var flight1 = CreateFlight("1");
            var flight2 = CreateFlight("2");

            AddFlightAndAircraft(flight1);
            AddFlightAndAircraft(flight2);

            var flights = _Database.GetFlightsForAircraft(flight2.Aircraft, _Criteria, -1, -1, null, false, null, false);
            Assert.AreEqual(1, flights.Count);
            Assert.AreEqual("2", flights[0].Callsign);
        }

        [TestMethod]
        public void BaseStationDatabase_GetFlightsForAircraft_Does_Not_Create_New_Aircraft_Objects()
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

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlights$")]
        public void BaseStationDatabase_GetFlightsForAircraft_Copies_Database_Record_To_Flight_Object()
        {
            Do_GetFlightsAllVersions_Copies_Database_Record_To_Flight_Object(false);
        }

        [TestMethod]
        public void BaseStationDatabase_GetFlightsForAircraft_Can_Filter_Flights_By_Equality_Criteria()
        {
            var aircraft = CreateAircraft("icao", "reg");
            var defaultFlight = CreateFlight(aircraft);
            var notEqualFlight = CreateFlight(aircraft);
            var equalsFlight = CreateFlight(aircraft);

            foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                if(!IsFlightCriteria(criteriaProperty)) continue;

                TestCleanup();
                TestInitialise();

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

        [TestMethod]
        public void BaseStationDatabase_GetFlightsForAircraft_Can_Filter_Flights_By_Range_Criteria()
        {
            var aircraft = CreateAircraft("icao", "reg");
            var belowRangeFlight = CreateFlight(aircraft, "belowRange");
            var startRangeFlight = CreateFlight(aircraft, "startRange");
            var inRangeFlight = CreateFlight(aircraft, "inRange");
            var endRangeFlight = CreateFlight(aircraft, "endRange");
            var aboveRangeFlight = CreateFlight(aircraft, "aboveRange");

            foreach(var reverseCondition in new bool[] { false, true }) {
                foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                    if(!IsFlightCriteria(criteriaProperty)) continue;

                    TestCleanup();
                    TestInitialise();

                    if(SetRangeCriteria(criteriaProperty, belowRangeFlight, startRangeFlight, inRangeFlight, endRangeFlight, aboveRangeFlight, reverseCondition)) {
                        AddFlight(belowRangeFlight);
                        AddFlight(startRangeFlight);
                        AddFlight(inRangeFlight);
                        AddFlight(endRangeFlight);
                        AddFlight(aboveRangeFlight);

                        var flights = _Database.GetFlightsForAircraft(aircraft, _Criteria, -1, -1, null, false, null, false);
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

        [TestMethod]
        public void BaseStationDatabase_GetFlightsForAircraft_Some_Criteria_Is_Case_Insensitive()
        {
            Do_GetFlightsAllVersions_Some_Criteria_Is_Case_Insensitive(false);
        }

        [TestMethod]
        public void BaseStationDatabase_GetFlightsForAircraft_Some_Criteria_Is_Case_Sensitive()
        {
            Do_GetFlightsAllVersions_Some_Criteria_Is_Case_Sensitive(false);
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlightsRows$")]
        public void BaseStationDatabase_GetFlightsForAircraft_Can_Return_Subset_Of_Rows()
        {
            Do_GetFlightsAllVersions_Can_Return_Subset_Of_Rows(false);
        }

        [TestMethod]
        public void BaseStationDatabase_GetFlightsForAircraft_Ignores_Unknown_Sort_Columns()
        {
            Do_GetFlightsAllVersions_Ignores_Unknown_Sort_Columns(false);
        }

        [TestMethod]
        public void BaseStationDatabase_GetFlightsForAircraft_Ignores_Case_On_Sort_Column_Names()
        {
            Do_GetFlightsAllVersions_Ignores_Case_On_Sort_Column_Names(false);
        }

        [TestMethod]
        public void BaseStationDatabase_GetFlightsForAircraft_Sorts_By_One_Column_Correctly()
        {
            var aircraft = CreateAircraft();

            var defaultFlight = CreateFlight(aircraft, "defaultFlight");
            var lowFlight = CreateFlight(aircraft, "lowFlight");
            var highFlight = CreateFlight(aircraft, "highFlight");

            defaultFlight.FirstAltitude = 1;
            lowFlight.FirstAltitude = 2;
            highFlight.FirstAltitude = 3;

            foreach(var sortColumn in _SortColumns) {
                if(!IsFlightSortColumn(sortColumn)) continue;

                TestCleanup();
                TestInitialise();

                SetSortColumnValue(defaultFlight, sortColumn, true, false);
                SetSortColumnValue(lowFlight, sortColumn, false, false);
                SetSortColumnValue(highFlight, sortColumn, false, true);

                AddAircraft(aircraft);

                AddFlight(defaultFlight);
                AddFlight(lowFlight);
                AddFlight(highFlight);

                var flights = _Database.GetFlights(_Criteria, -1, -1, sortColumn, true, null, false);
                Assert.AreEqual(3, flights.Count, sortColumn);
                Assert.AreEqual(1, flights[0].FirstAltitude, sortColumn);
                Assert.AreEqual(2, flights[1].FirstAltitude, sortColumn);
                Assert.AreEqual(3, flights[2].FirstAltitude, sortColumn);

                flights = _Database.GetFlights(_Criteria, -1, -1, sortColumn, false, null, false);
                Assert.AreEqual(3, flights.Count, sortColumn);
                Assert.AreEqual(3, flights[0].FirstAltitude, sortColumn);
                Assert.AreEqual(2, flights[1].FirstAltitude, sortColumn);
                Assert.AreEqual(1, flights[2].FirstAltitude, sortColumn);
            }
        }

        [TestMethod]
        public void BaseStationDatabase_GetFlightsForAircraft_Sorts_By_Two_Columns_Correctly()
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
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BaseStationDatabase_GetCountOfFlights_Throws_If_Criteria_Is_Null()
        {
            _Database.GetCountOfFlights(null);
        }

        [TestMethod]
        public void BaseStationDatabase_GetCountOfFlights_Returns_Count_Of_Flights_Matching_Criteria()
        {
            AddFlightAndAircraft(CreateFlight("ABC"));
            AddFlightAndAircraft(CreateFlight("XYZ"));

            Assert.AreEqual(2, _Database.GetCountOfFlights(_Criteria));

            _Criteria.Callsign = new FilterString("XYZ");
            Assert.AreEqual(1, _Database.GetCountOfFlights(_Criteria));
        }

        [TestMethod]
        public void BaseStationDatabase_GetCountOfFlights_Counts_Equality_Criteria()
        {
            var defaultFlight = CreateFlight("default", false);
            var notEqualFlight = CreateFlight("notEquals", false);
            var equalsFlight = CreateFlight("equals", false);

            foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                TestCleanup();
                TestInitialise();

                if(SetEqualityCriteria(criteriaProperty, defaultFlight, notEqualFlight, equalsFlight, false)) {
                    AddFlightAndAircraft(defaultFlight);
                    AddFlightAndAircraft(notEqualFlight);
                    AddFlightAndAircraft(equalsFlight);

                    Assert.AreEqual(1, _Database.GetCountOfFlights(_Criteria), "{0}", criteriaProperty.Name);
                }
            }
        }

        [TestMethod]
        public void BaseStationDatabase_GetCountOfFlights_Counts_Range_Criteria()
        {
            var belowRangeFlight = CreateFlight("belowRange");
            var startRangeFlight = CreateFlight("startRange");
            var inRangeFlight = CreateFlight("inRange");
            var endRangeFlight = CreateFlight("endRange");
            var aboveRangeFlight = CreateFlight("aboveRange");

            foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                TestCleanup();
                TestInitialise();

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
        [TestMethod]
        public void BaseStationDatabase_GetCountOfFlightsForAircraft_Returns_Zero_If_Aircraft_Is_Null()
        {
            AddFlightAndAircraft(CreateFlight("1"));

            Assert.AreEqual(0, _Database.GetCountOfFlightsForAircraft(null, _Criteria));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BaseStationDatabase_GetCountOfFlightsForAircraft_Throws_If_Criteria_Is_Null()
        {
            _Database.GetCountOfFlightsForAircraft(CreateAircraft(), null);
        }

        [TestMethod]
        public void BaseStationDatabase_GetCountOfFlightsForAircraft_Returns_Count_Of_Flights_Matching_Criteria()
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

        [TestMethod]
        public void BaseStationDatabase_GetCountOfFlightsForAircraft_Counts_Equality_Criteria()
        {
            var aircraft = CreateAircraft();
            var defaultFlight = CreateFlight(aircraft);
            var notEqualFlight = CreateFlight(aircraft);
            var equalsFlight = CreateFlight(aircraft);

            foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                if(!IsFlightCriteria(criteriaProperty)) continue;

                TestCleanup();
                TestInitialise();

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

        [TestMethod]
        public void BaseStationDatabase_GetCountOfFlightsForAircraft_Counts_Range_Criteria()
        {
            var aircraft = CreateAircraft();
            var belowRangeFlight = CreateFlight(aircraft);
            var startRangeFlight = CreateFlight(aircraft);
            var inRangeFlight = CreateFlight(aircraft);
            var endRangeFlight = CreateFlight(aircraft);
            var aboveRangeFlight = CreateFlight(aircraft);

            foreach(var criteriaProperty in typeof(SearchBaseStationCriteria).GetProperties()) {
                if(!IsFlightCriteria(criteriaProperty)) continue;

                TestCleanup();
                TestInitialise();

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
        [TestMethod]
        public void BaseStationDatabase_GetFlightById_Returns_Null_If_Flight_Does_Not_Exist()
        {
            Assert.IsNull(_Database.GetFlightById(1));
        }

        [TestMethod]
        public void BaseStationDatabase_GetFlightById_Returns_Null_If_File_Does_Not_Exist()
        {
            File.Delete(_EmptyDatabaseFileName);
            Assert.IsNull(_Database.GetFlightById(1));
        }

        [TestMethod]
        public void BaseStationDatabase_GetFlightById_Returns_Null_If_File_Not_Configured()
        {
            _Database.FileName = null;
            Assert.IsNull(_Database.GetFlightById(1));
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlights$")]
        public void BaseStationDatabase_GetFlightById_Returns_Flight_Object_For_Record_Identifier()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var aircraft = new BaseStationAircraft() { ModeS = "A" };
            aircraft.AircraftID = (int)AddAircraft(aircraft);
            var mockFlight = LoadFlightFromSpreadsheet(worksheet);
            mockFlight.SessionID = (int)AddSession(new BaseStationSession() { StartTime = DateTime.Now });
            mockFlight.Aircraft = aircraft;
            mockFlight.AircraftID = aircraft.AircraftID;

            var id = (int)AddFlight(mockFlight);

            var flight = _Database.GetFlightById(id);
            Assert.AreNotSame(flight, mockFlight);

            AssertFlightsAreEqual(mockFlight, flight, false, aircraft.AircraftID);
        }
        #endregion

        #region InsertFlight
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BaseStationDatabase_InsertFlight_Throws_If_Writes_Disabled()
        {
            var aircraftId = (int)AddAircraft(new BaseStationAircraft() { ModeS = "Z" });
            _Database.InsertFlight(new BaseStationFlight() { AircraftID = aircraftId, StartTime = DateTime.Now });
        }

        [TestMethod]
        public void BaseStationDatabase_InsertFlight_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            File.Delete(_EmptyDatabaseFileName);

            _Database.InsertFlight(new BaseStationFlight());
            Assert.IsFalse(File.Exists(_EmptyDatabaseFileName));
        }

        [TestMethod]
        public void BaseStationDatabase_InsertFlight_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.InsertFlight(new BaseStationFlight());
            // Can't think of a good way to test that it didn't do anything, but if it tried to write to a file without knowing the
            // name of the file it'd crash so if the test passes without any exceptions being raised then that's a good sign
        }

        [TestMethod]
        public void BaseStationDatabase_InsertFlight_Truncates_Milliseconds_From_Date()
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

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlights$")]
        public void BaseStationDatabase_InsertFlight_Correctly_Inserts_Record()
        {
            _Database.WriteSupportEnabled = true;

            var worksheet = new ExcelWorksheetData(TestContext);
            var aircraftId = (int)AddAircraft(new BaseStationAircraft() { ModeS = "Y" });
            var sessionId = (int)AddSession(new BaseStationSession() { StartTime = DateTime.Now });
            var flight = LoadFlightFromSpreadsheet(worksheet);
            flight.AircraftID = aircraftId;
            flight.SessionID = sessionId;
            flight.Aircraft = null;

            _Database.InsertFlight(flight);
            Assert.AreNotEqual(0, flight.FlightID);

            var readBack = _Database.GetFlightById(flight.FlightID);
            AssertFlightsAreEqual(flight, readBack, false, aircraftId);
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlights$")]
        public void BaseStationDatabase_InsertFlight_Works_In_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    TestCleanup();
                    TestInitialise();

                    try {
                        BaseStationDatabase_InsertFlight_Correctly_Inserts_Record();
                    } catch(Exception ex) {
                        throw new InvalidOperationException(String.Format("Exception thrown when culture was {0}", culture), ex);
                    }
                }
            }
        }
        #endregion

        #region UpdateFlight
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BaseStationDatabase_UpdateFlight_Throws_If_Writes_Disabled()
        {
            var flight = new BaseStationFlight() {
                AircraftID = (int)AddAircraft(new BaseStationAircraft() { ModeS = "Z" }),
                SessionID = (int)AddSession(new BaseStationSession() { StartTime = DateTime.Now }),
            };

            flight.FlightID = (int)AddFlight(flight);

            flight.FirstTrack = 42;
            _Database.UpdateFlight(flight);
        }

        [TestMethod]
        public void BaseStationDatabase_UpdateFlight_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            File.Delete(_EmptyDatabaseFileName);

            _Database.UpdateFlight(new BaseStationFlight());
            Assert.IsFalse(File.Exists(_EmptyDatabaseFileName));
        }

        [TestMethod]
        public void BaseStationDatabase_UpdateFlight_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.UpdateFlight(new BaseStationFlight());
        }

        [TestMethod]
        public void BaseStationDatabase_UpdateFlight_Truncates_Milliseconds_From_Date()
        {
            _Database.WriteSupportEnabled = true;
            var aircraftId = (int)AddAircraft(new BaseStationAircraft() { ModeS = "Y" });
            var sessionId = (int)AddSession(new BaseStationSession() { StartTime = DateTime.Now });

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

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlights$")]
        public void BaseStationDatabase_UpdateFlight_Correctly_Updates_Record()
        {
            _Database.WriteSupportEnabled = true;
            var worksheet = new ExcelWorksheetData(TestContext);

            var originalAircraftId = (int)AddAircraft(new BaseStationAircraft() { ModeS = "Z" });
            var originalSessionId = (int)AddSession(new BaseStationSession() { StartTime = DateTime.Now });
            var flightId = (int)AddFlight(new BaseStationFlight() { AircraftID = originalAircraftId, SessionID = originalSessionId });

            var update = _Database.GetFlightById(flightId);
            LoadFlightFromSpreadsheet(worksheet, 0, update);
            update.Aircraft = null;

            var newAircraftId = (int)AddAircraft(new BaseStationAircraft() { ModeS = "XYZ" });
            var newSessionId = (int)AddSession(new BaseStationSession() { StartTime = DateTime.Now.AddDays(1) });
            update.AircraftID = newAircraftId;
            update.SessionID = newSessionId;

            _Database.UpdateFlight(update);

            var readBack = _Database.GetFlightById(flightId);
            AssertFlightsAreEqual(update, readBack, false, newAircraftId);
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GetFlights$")]
        public void BaseStationDatabase_UpdateFlight_Works_For_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    TestCleanup();
                    TestInitialise();

                    try {
                        BaseStationDatabase_UpdateFlight_Correctly_Updates_Record();
                    } catch(Exception ex) {
                        throw new InvalidOperationException(String.Format("Exception thrown when culture was {0}", culture), ex);
                    }
                }
            }
        }
        #endregion

        #region DeleteFlight
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BaseStationDatabase_DeleteFlight_Throws_If_Writes_Disabled()
        {
            var flight = new BaseStationFlight() {
                AircraftID = (int)AddAircraft(new BaseStationAircraft() { ModeS = "Z" }),
                SessionID = (int)AddSession(new BaseStationSession() { StartTime = DateTime.Now }),
            };

            flight.FlightID = (int)AddFlight(flight);

            _Database.DeleteFlight(flight);
        }

        [TestMethod]
        public void BaseStationDatabase_DeleteFlight_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            File.Delete(_EmptyDatabaseFileName);

            _Database.DeleteFlight(new BaseStationFlight());
            Assert.IsFalse(File.Exists(_EmptyDatabaseFileName));
        }

        [TestMethod]
        public void BaseStationDatabase_DeleteFlight_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.DeleteFlight(new BaseStationFlight());
        }

        [TestMethod]
        public void BaseStationDatabase_DeleteFlight_Correctly_Deletes_Record()
        {
            _Database.WriteSupportEnabled = true;

            var aircraftId = (int)AddAircraft(new BaseStationAircraft() { ModeS = "Z" });
            var sessionId = (int)AddSession(new BaseStationSession() { StartTime = DateTime.Now });
            var flightId = (int)AddFlight(new BaseStationFlight() { AircraftID = aircraftId, SessionID = sessionId });

            var delete = _Database.GetFlightById(flightId);

            _Database.DeleteFlight(delete);

            Assert.AreEqual(null, _Database.GetFlightById(flightId));

            Assert.AreNotEqual(null, _Database.GetAircraftById(aircraftId));
            Assert.AreEqual(sessionId, _Database.GetSessions()[0].SessionID);
        }
        #endregion

        #region GetDatabaseHistory
        [TestMethod]
        public void BaseStationDatabase_GetDatabaseHistory_Returns_Empty_List_If_File_Does_Not_Exist()
        {
            File.Delete(_EmptyDatabaseFileName);
            Assert.AreEqual(0, _Database.GetDatabaseHistory().Count());
        }

        [TestMethod]
        public void BaseStationDatabase_GetDatabaseHistory_Returns_Empty_List_If_File_Not_Configured()
        {
            _Database.FileName = null;
            Assert.AreEqual(0, _Database.GetDatabaseHistory().Count());
        }

        [TestMethod]
        public void BaseStationDatabase_GetDatabaseHistory_Retrieves_All_Records_In_DBHistory_Table()
        {
            var timeStamp1 = DateTime.Now;
            var timeStamp2 = DateTime.Now.AddSeconds(10);

            AddDBHistory(new BaseStationDBHistory() { Description = "A", TimeStamp = timeStamp1 });
            AddDBHistory(new BaseStationDBHistory() { Description = "B", TimeStamp = timeStamp2 });

            var history = _Database.GetDatabaseHistory();
            Assert.AreEqual(2, history.Count());

            var historyA = history.Where(h => h.Description == "A").FirstOrDefault();
            var historyB = history.Where(h => h.Description == "B").FirstOrDefault();

            Assert.AreEqual(timeStamp1, historyA.TimeStamp);
            Assert.AreEqual(timeStamp2, historyB.TimeStamp);
        }
        #endregion

        #region GetDatabaseVersion
        [TestMethod]
        public void BaseStationDatabase_GetDatabaseVersion_Returns_Null_If_File_Does_Not_Exist()
        {
            File.Delete(_EmptyDatabaseFileName);
            Assert.AreEqual(null, _Database.GetDatabaseVersion());
        }

        [TestMethod]
        public void BaseStationDatabase_GetDatabaseVersion_Returns_Null_If_File_Not_Configured()
        {
            _Database.FileName = null;
            Assert.AreEqual(null, _Database.GetDatabaseVersion());
        }

        [TestMethod]
        public void BaseStationDatabase_GetDatabaseVersion_Retrieves_Record_In_DBInfo_Table()
        {
            // The table has no key, it appears that the intention is to only ever have one record in the table
            AddDBInfo(new BaseStationDBInfo() { OriginalVersion = 2, CurrentVersion = 3 });

            var dbInfo = _Database.GetDatabaseVersion();
            Assert.AreEqual(2, dbInfo.OriginalVersion);
            Assert.AreEqual(3, dbInfo.CurrentVersion);
        }

        [TestMethod]
        public void BaseStationDatabase_GetDatabaseVersion_Retrieves_Last_Record_In_DBInfo_Table()
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
        [TestMethod]
        public void BaseStationDatabase_GetSystemEvents_Returns_Empty_List_If_File_Does_Not_Exist()
        {
            File.Delete(_EmptyDatabaseFileName);
            Assert.AreEqual(0, _Database.GetSystemEvents().Count());
        }

        [TestMethod]
        public void BaseStationDatabase_GetSystemEvents_Returns_Empty_List_If_File_Not_Configured()
        {
            _Database.FileName = null;
            Assert.AreEqual(0, _Database.GetSystemEvents().Count());
        }

        [TestMethod]
        public void BaseStationDatabase_GetSystemEvents_Retrieves_All_Records_In_SystemEvents_Table()
        {
            var timeStamp1 = DateTime.Now;
            var timeStamp2 = DateTime.Now.AddHours(2.1);
            AddSystemEvent(new BaseStationSystemEvents() { App = "A", Msg = "B", TimeStamp = timeStamp1 });
            AddSystemEvent(new BaseStationSystemEvents() { App = "2", Msg = "3", TimeStamp = timeStamp2 });

            var systemEvents = _Database.GetSystemEvents();
            Assert.AreEqual(2, systemEvents.Count());

            Assert.IsTrue(systemEvents.Where(s => s.App == "A" && s.Msg == "B" && s.TimeStamp == timeStamp1 && s.SystemEventsID != 0).Any());
            Assert.IsTrue(systemEvents.Where(s => s.App == "2" && s.Msg == "3" && s.TimeStamp == timeStamp2 && s.SystemEventsID != 0).Any());
        }
        #endregion

        #region InsertSystemEvents
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BaseStationDatabase_InsertSystemEvents_Throws_If_Writes_Disabled()
        {
            _Database.InsertSystemEvent(new BaseStationSystemEvents() { App = "A", Msg = "D", TimeStamp = DateTime.Now });
        }

        [TestMethod]
        public void BaseStationDatabase_InsertSystemEvents_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            File.Delete(_EmptyDatabaseFileName);

            _Database.InsertSystemEvent(new BaseStationSystemEvents() { App = "A", Msg = "D", TimeStamp = DateTime.Now });
            Assert.AreEqual(0, _Database.GetSystemEvents().Count);
        }

        [TestMethod]
        public void BaseStationDatabase_InsertSystemEvents_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.InsertSystemEvent(new BaseStationSystemEvents() { App = "A", Msg = "D", TimeStamp = DateTime.Now });
            Assert.AreEqual(0, _Database.GetSystemEvents().Count);
        }

        [TestMethod]
        public void BaseStationDatabase_InsertSystemEvents_Correctly_Inserts_Record()
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

        [TestMethod]
        public void BaseStationDatabase_InsertSystemEvents_Works_For_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    TestCleanup();
                    TestInitialise();

                    try {
                        BaseStationDatabase_InsertSystemEvents_Correctly_Inserts_Record();
                    } catch(Exception ex) {
                        throw new InvalidOperationException(String.Format("Exception thrown when culture was {0}", culture), ex);
                    }
                }
            }
        }
        #endregion

        #region UpdateSystemEvents
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BaseStationDatabase_UpdateSystemEvents_Throws_If_Writes_Disabled()
        {
            AddSystemEvent(new BaseStationSystemEvents() { App = "K", Msg = "Z", TimeStamp = DateTime.Now });
            var update = _Database.GetSystemEvents()[0];
            _Database.UpdateSystemEvent(update);
        }

        [TestMethod]
        public void BaseStationDatabase_UpdateSystemEvents_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            File.Delete(_EmptyDatabaseFileName);

            _Database.UpdateSystemEvent(new BaseStationSystemEvents());
            Assert.AreEqual(0, _Database.GetSystemEvents().Count);
        }

        [TestMethod]
        public void BaseStationDatabase_UpdateSystemEvents_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.UpdateSystemEvent(new BaseStationSystemEvents());
            Assert.AreEqual(0, _Database.GetSystemEvents().Count);
        }

        [TestMethod]
        public void BaseStationDatabase_UpdateSystemEvents_Correctly_Updates_Record()
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

        [TestMethod]
        public void BaseStationDatabase_UpdateSystemEvents_Works_For_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    TestCleanup();
                    TestInitialise();

                    try {
                        BaseStationDatabase_UpdateSystemEvents_Correctly_Updates_Record();
                    } catch(Exception ex) {
                        throw new InvalidOperationException(String.Format("Exception thrown when culture was {0}", culture), ex);
                    }
                }
            }
        }
        #endregion

        #region DeleteSystemEvents
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BaseStationDatabase_DeleteSystemEvents_Throws_If_Writes_Disabled()
        {
            AddSystemEvent(new BaseStationSystemEvents() { App = "K", Msg = "Z", TimeStamp = DateTime.Now });
            var delete = _Database.GetSystemEvents()[0];
            _Database.DeleteSystemEvent(delete);
        }

        [TestMethod]
        public void BaseStationDatabase_DeleteSystemEvents_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            File.Delete(_EmptyDatabaseFileName);

            _Database.DeleteSystemEvent(new BaseStationSystemEvents());
            Assert.AreEqual(0, _Database.GetSystemEvents().Count);
        }

        [TestMethod]
        public void BaseStationDatabase_DeleteSystemEvents_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.DeleteSystemEvent(new BaseStationSystemEvents());
            Assert.AreEqual(0, _Database.GetSystemEvents().Count);
        }

        [TestMethod]
        public void BaseStationDatabase_DeleteSystemEvents_Correctly_Deletes_Record()
        {
            _Database.WriteSupportEnabled = true;

            AddSystemEvent(new BaseStationSystemEvents() { App = "K", Msg = "Z", TimeStamp = DateTime.Now });
            var delete = _Database.GetSystemEvents()[0];

            _Database.DeleteSystemEvent(delete);

            Assert.AreEqual(0, _Database.GetSystemEvents().Count());
        }
        #endregion

        #region GetLocations
        [TestMethod]
        public void BaseStationDatabase_GetLocations_Returns_Empty_List_If_File_Does_Not_Exist()
        {
            File.Delete(_EmptyDatabaseFileName);
            Assert.AreEqual(0, _Database.GetLocations().Count());
        }

        [TestMethod]
        public void BaseStationDatabase_GetLocations_Returns_Empty_List_If_File_Not_Configured()
        {
            _Database.FileName = null;
            Assert.AreEqual(0, _Database.GetLocations().Count());
        }

        [TestMethod]
        public void BaseStationDatabase_GetLocations_Retrieves_All_Records_In_Locations_Table()
        {
            AddLocation(new BaseStationLocation() { LocationName = "A", Latitude = 1.2, Longitude = 9.8, Altitude = 6.5 });
            AddLocation(new BaseStationLocation() { LocationName = "B", Latitude = 6.5, Longitude = 4.3, Altitude = 2.1 });

            var locations = _Database.GetLocations();
            Assert.AreEqual(2, locations.Count());

            Assert.IsTrue(locations.Where(n => n.LocationName == "A").Any());
            Assert.IsTrue(locations.Where(n => n.LocationName == "B").Any());
        }
        #endregion

        #region InsertLocation
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BaseStationDatabase_InsertLocation_Throws_If_Writes_Disabled()
        {
            _Database.InsertLocation(new BaseStationLocation() { LocationName = "X" });
        }

        [TestMethod]
        public void BaseStationDatabase_InsertLocation_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            File.Delete(_EmptyDatabaseFileName);

            _Database.InsertLocation(new BaseStationLocation() { LocationName = "X" });
            Assert.AreEqual(0, _Database.GetLocations().Count);
        }

        [TestMethod]
        public void BaseStationDatabase_InsertLocation_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.InsertLocation(new BaseStationLocation() { LocationName = "X" });
            Assert.AreEqual(0, _Database.GetLocations().Count);
        }

        [TestMethod]
        public void BaseStationDatabase_InsertLocation_Correctly_Inserts_Record()
        {
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

        [TestMethod]
        public void BaseStationDatabase_InsertLocation_Works_For_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    TestCleanup();
                    TestInitialise();

                    try {
                        BaseStationDatabase_InsertLocation_Correctly_Inserts_Record();
                    } catch(Exception ex) {
                        throw new InvalidOperationException(String.Format("Exception thrown when culture was {0}", culture), ex);
                    }
                }
            }
        }
        #endregion

        #region UpdateLocation
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BaseStationDatabase_UpdateLocation_Throws_If_Writes_Disabled()
        {
            var location = new BaseStationLocation() { LocationName = "B" };
            location.LocationID = (int)AddLocation(location);

            location.LocationName = "C";
            _Database.UpdateLocation(location);
        }

        [TestMethod]
        public void BaseStationDatabase_UpdateLocation_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            File.Delete(_EmptyDatabaseFileName);

            _Database.UpdateLocation(new BaseStationLocation() { LocationName = "X" });
            Assert.AreEqual(0, _Database.GetLocations().Count);
        }

        [TestMethod]
        public void BaseStationDatabase_UpdateLocation_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.UpdateLocation(new BaseStationLocation() { LocationName = "X" });
            Assert.AreEqual(0, _Database.GetLocations().Count);
        }

        [TestMethod]
        public void BaseStationDatabase_UpdateLocation_Correctly_Updates_Record()
        {
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

        [TestMethod]
        public void BaseStationDatabase_UpdateLocation_Works_For_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    TestCleanup();
                    TestInitialise();

                    try {
                        BaseStationDatabase_UpdateLocation_Correctly_Updates_Record();
                    } catch(Exception ex) {
                        throw new InvalidOperationException(String.Format("Exception thrown when culture was {0}", culture), ex);
                    }
                }
            }
        }
        #endregion

        #region DeleteLocation
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BaseStationDatabase_DeleteLocation_Throws_If_Writes_Disabled()
        {
            var location = new BaseStationLocation() { LocationName = "B" };
            location.LocationID = (int)AddLocation(location);

            _Database.DeleteLocation(location);
        }

        [TestMethod]
        public void BaseStationDatabase_DeleteLocation_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            File.Delete(_EmptyDatabaseFileName);

            _Database.DeleteLocation(new BaseStationLocation() { LocationName = "X" });
            Assert.AreEqual(0, _Database.GetLocations().Count);
        }

        [TestMethod]
        public void BaseStationDatabase_DeleteLocation_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.DeleteLocation(new BaseStationLocation() { LocationName = "X" });
            Assert.AreEqual(0, _Database.GetLocations().Count);
        }

        [TestMethod]
        public void BaseStationDatabase_DeleteLocation_Correctly_Deletes_Record()
        {
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
        [TestMethod]
        public void BaseStationDatabase_GetSessions_Returns_Empty_List_If_File_Does_Not_Exist()
        {
            File.Delete(_EmptyDatabaseFileName);
            Assert.AreEqual(0, _Database.GetSessions().Count());
        }

        [TestMethod]
        public void BaseStationDatabase_GetSessions_Returns_Empty_List_If_File_Not_Configured()
        {
            _Database.FileName = null;

            Assert.AreEqual(0, _Database.GetSessions().Count());
        }

        [TestMethod]
        public void BaseStationDatabase_GetSessions_Retrieves_All_Records_In_Sessions_Table()
        {
            var location1 = (int)AddLocation(new BaseStationLocation() { LocationName = "A" });
            var location2 = (int)AddLocation(new BaseStationLocation() { LocationName = "B" });
            var startTime1 = DateTime.Now;
            var startTime2 = DateTime.Now.AddYears(1);
            var endTime1 = startTime1.AddSeconds(10);
            var endTime2 = startTime2.AddMinutes(10);

            AddSession(new BaseStationSession() { LocationID = location1, StartTime = startTime1, EndTime = endTime1 });
            AddSession(new BaseStationSession() { LocationID = location2, StartTime = startTime2, EndTime = endTime2 });

            var sessions = _Database.GetSessions();

            Assert.AreEqual(2, sessions.Count);
            Assert.IsTrue(sessions.Where(s => s.LocationID == location1 && s.StartTime == startTime1 && s.EndTime == endTime1 && s.SessionID != 0).Any());
            Assert.IsTrue(sessions.Where(s => s.LocationID == location2 && s.StartTime == startTime2 && s.EndTime == endTime2 && s.SessionID != 0).Any());
        }
        #endregion

        #region InsertSession
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BaseStationDatabase_InsertSession_Throws_If_Writes_Disabled()
        {
            _Database.InsertSession(new BaseStationSession());
        }

        [TestMethod]
        public void BaseStationDatabase_InsertSession_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            File.Delete(_EmptyDatabaseFileName);

            _Database.InsertSession(new BaseStationSession());
            Assert.AreEqual(0, _Database.GetSessions().Count);
        }

        [TestMethod]
        public void BaseStationDatabase_InsertSession_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.InsertSession(new BaseStationSession());
            Assert.AreEqual(0, _Database.GetSessions().Count);
        }

        [TestMethod]
        public void BaseStationDatabase_InsertSession_Inserts_Record_Correctly()
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

        [TestMethod]
        public void BaseStationDatabase_InsertSession_Works_For_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    TestCleanup();
                    TestInitialise();

                    try {
                        BaseStationDatabase_InsertSession_Inserts_Record_Correctly();
                    } catch(Exception ex) {
                        throw new InvalidOperationException(String.Format("Exception thrown when culture was {0}", culture), ex);
                    }
                }
            }
        }

        [TestMethod]
        public void BaseStationDatabase_InsertSession_Copes_If_There_Are_No_Locations()
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
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BaseStationDatabase_UpdateSession_Throws_If_Writes_Disabled()
        {
            var locationId = (int)AddLocation(new BaseStationLocation() { LocationName = "X" });
            var session = new BaseStationSession() { LocationID = locationId, StartTime = DateTime.Now };
            session.SessionID = (int)AddSession(session);

            session.EndTime = DateTime.Now;
            _Database.UpdateSession(session);
        }

        [TestMethod]
        public void BaseStationDatabase_UpdateSession_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            File.Delete(_EmptyDatabaseFileName);

            _Database.UpdateSession(new BaseStationSession());
            Assert.AreEqual(0, _Database.GetSessions().Count);
        }

        [TestMethod]
        public void BaseStationDatabase_UpdateSession_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.UpdateSession(new BaseStationSession());
            Assert.AreEqual(0, _Database.GetSessions().Count);
        }

        [TestMethod]
        public void BaseStationDatabase_UpdateSession_Correctly_Updates_Record()
        {
            _Database.WriteSupportEnabled = true;
            var locationId = (int)AddLocation(new BaseStationLocation() { LocationName = "X" });
            var newLocationId = (int)AddLocation(new BaseStationLocation() { LocationName = "Y" });
            AddSession(new BaseStationSession() {
                EndTime = null,
                LocationID = locationId,
                StartTime = new DateTime(2001, 2, 3, 4, 5, 6, 789),
            });

            var update = _Database.GetSessions()[0];
            var originalId = update.SessionID;
            update.EndTime = new DateTime(2007, 8, 9, 10, 11, 12, 772);
            update.LocationID = newLocationId;
            update.StartTime = new DateTime(2006, 7, 8, 9, 10, 11, 124);

            _Database.UpdateSession(update);

            var allSessions = _Database.GetSessions();
            Assert.AreEqual(1, allSessions.Count);
            var readBack = allSessions[0];
            Assert.AreEqual(originalId, readBack.SessionID);
            Assert.AreEqual(new DateTime(2007, 8, 9, 10, 11, 12), readBack.EndTime);
            Assert.AreEqual(newLocationId, readBack.LocationID);
            Assert.AreEqual(new DateTime(2006, 7, 8, 9, 10, 11), readBack.StartTime);
        }

        [TestMethod]
        public void BaseStationDatabase_UpdateSession_Works_For_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    TestCleanup();
                    TestInitialise();

                    try {
                        BaseStationDatabase_UpdateSession_Correctly_Updates_Record();
                    } catch(Exception ex) {
                        throw new InvalidOperationException(String.Format("Exception thrown when culture was {0}", culture), ex);
                    }
                }
            }
        }
        #endregion

        #region DeleteSession
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BaseStationDatabase_DeleteSession_Throws_If_Writes_Disabled()
        {
            var locationId = (int)AddLocation(new BaseStationLocation() { LocationName = "X" });
            var session = new BaseStationSession() { LocationID = locationId, StartTime = DateTime.Now };
            session.SessionID = (int)AddSession(session);

            _Database.DeleteSession(session);
        }

        [TestMethod]
        public void BaseStationDatabase_DeleteSession_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Database.WriteSupportEnabled = true;
            File.Delete(_EmptyDatabaseFileName);

            _Database.DeleteSession(new BaseStationSession());
            Assert.AreEqual(0, _Database.GetSessions().Count);
        }

        [TestMethod]
        public void BaseStationDatabase_DeleteSession_Does_Nothing_If_File_Not_Configured()
        {
            _Database.WriteSupportEnabled = true;
            _Database.FileName = null;

            _Database.DeleteSession(new BaseStationSession());
            Assert.AreEqual(0, _Database.GetSessions().Count);
        }

        [TestMethod]
        public void BaseStationDatabase_DeleteSession_Correctly_Deletes_Record()
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

        #region CreateDatabaseIfMissing
        [TestMethod]
        public void BaseStationDatabase_CreateDatabaseIfMissing_Does_Nothing_If_FileName_Is_Null()
        {
            _Database.CreateDatabaseIfMissing(null);
        }

        [TestMethod]
        public void BaseStationDatabase_CreateDatabaseIfMissing_Does_Nothing_If_FileName_Is_Empty()
        {
            _Database.CreateDatabaseIfMissing("");
        }

        [TestMethod]
        public void BaseStationDatabase_CreateDatabaseIfMissing_Creates_Path_To_Database_File()
        {
            string folder = Path.Combine(TestContext.TestDeploymentDir, "SubFolderForCDIF");
            if(Directory.Exists(folder)) Directory.Delete(folder, true);

            _CreateDatabaseFileName = Path.Combine(folder, "TheFile.sdb");

            _Database.CreateDatabaseIfMissing(_CreateDatabaseFileName);

            Assert.IsTrue(Directory.Exists(folder));
        }

        [TestMethod]
        public void BaseStationDatabase_CreateDatabaseIfMissing_Creates_File_If_Missing()
        {
            if(File.Exists(_CreateDatabaseFileName)) File.Delete(_CreateDatabaseFileName);

            _Database.CreateDatabaseIfMissing(_CreateDatabaseFileName);

            Assert.IsTrue(File.Exists(_CreateDatabaseFileName));
        }

        [TestMethod]
        public void BaseStationDatabase_CreateDatabaseIfMissing_Writes_Correct_History_Record_If_File_Empty()
        {
            File.Create(_CreateDatabaseFileName).Close();

            _Database.CreateDatabaseIfMissing(_CreateDatabaseFileName);
            _Database.FileName = _CreateDatabaseFileName;

            Assert.IsTrue(_Database.GetDatabaseHistory().Where(h => h.IsCreationOfDatabaseByVirtualRadarServer).Any());
        }

        [TestMethod]
        public void BaseStationDatabase_CreateDatabaseIfMissing_Does_Nothing_If_File_Not_Empty()
        {
            _CreateDatabaseFileName = _EmptyDatabaseFileName;
            _Database.CreateDatabaseIfMissing(_CreateDatabaseFileName);

            Assert.AreEqual(0, _Database.GetDatabaseHistory().Count());
        }

        [TestMethod]
        public void BaseStationDatabase_CreateDatabaseIfMissing_Writes_Correct_History_Record_If_Missing()
        {
            var timeStamp = new DateTime(2001, 2, 3, 4, 5, 6, 789);
            _Provider.Setup(p => p.UtcNow).Returns(timeStamp);

            _Database.CreateDatabaseIfMissing(_CreateDatabaseFileName);
            _Database.FileName = _CreateDatabaseFileName;

            var allHistory = _Database.GetDatabaseHistory();
            Assert.AreEqual(1, allHistory.Count());

            var vrsCreationNote = allHistory.Where(h => h.IsCreationOfDatabaseByVirtualRadarServer).Single();
            Assert.AreEqual(TruncateDate(timeStamp), vrsCreationNote.TimeStamp);
            Assert.AreNotEqual(0, vrsCreationNote.DBHistoryID);
        }

        [TestMethod]
        public void BaseStationDatabase_CreateDatabaseIfMissing_Writes_Correct_History_Record_In_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    TestCleanup();
                    TestInitialise();

                    try {
                        BaseStationDatabase_CreateDatabaseIfMissing_Writes_Correct_History_Record_If_Missing();
                    } catch(Exception ex) {
                        throw new InvalidOperationException(String.Format("Exception thrown when culture was {0}", culture), ex);
                    }
                }
            }
        }

        [TestMethod]
        public void BaseStationDatabase_CreateDatabaseIfMissing_Writes_Correct_Info_Record()
        {
            _Database.CreateDatabaseIfMissing(_CreateDatabaseFileName);
            _Database.FileName = _CreateDatabaseFileName;

            var dbInfo = _Database.GetDatabaseVersion();
            Assert.AreEqual(2, dbInfo.OriginalVersion);
            Assert.AreEqual(2, dbInfo.CurrentVersion);
        }

        [TestMethod]
        public void BaseStationDatabase_CreateDatabaseIfMissing_Writes_Correct_Info_Record_In_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    TestCleanup();
                    TestInitialise();

                    try {
                        BaseStationDatabase_CreateDatabaseIfMissing_Writes_Correct_Info_Record();
                    } catch(Exception ex) {
                        throw new InvalidOperationException(String.Format("Exception thrown when culture was {0}", culture), ex);
                    }
                }
            }
        }

        [TestMethod]
        public void BaseStationDatabase_CreateDatabaseIfMissing_Writes_Google_Map_Default_Centre_As_Default_Location()
        {
            _Configuration.GoogleMapSettings.InitialMapLatitude = 2.3456;
            _Configuration.GoogleMapSettings.InitialMapLongitude = -7.8901;
            _ConfigurationStorage.Raise(c => c.ConfigurationChanged += null, EventArgs.Empty);

            _Database.CreateDatabaseIfMissing(_CreateDatabaseFileName);
            _Database.FileName = _CreateDatabaseFileName;

            var locations = _Database.GetLocations();
            Assert.AreEqual(1, locations.Count());

            var location = locations.First();
            Assert.AreEqual("Home", location.LocationName);
            Assert.AreEqual(0.0, location.Altitude);
            Assert.AreEqual(2.3456, location.Latitude, 0.00001);
            Assert.AreEqual(-7.8901, location.Longitude, 0.00001);
        }

        [TestMethod]
        public void BaseStationDatabase_CreateDatabaseIfMissing_Writes_Default_Location_In_Different_Cultures()
        {
            foreach(var culture in _Cultures) {
                using(var switcher = new CultureSwitcher(culture)) {
                    TestCleanup();
                    TestInitialise();

                    try {
                        BaseStationDatabase_CreateDatabaseIfMissing_Writes_Google_Map_Default_Centre_As_Default_Location();
                    } catch(Exception ex) {
                        throw new InvalidOperationException(String.Format("Exception thrown when culture was {0}", culture), ex);
                    }
                }
            }
        }

        [TestMethod]
        public void BaseStationDatabase_CreateDatabaseIfMissing_Creates_Session_Table()
        {
            _Database.CreateDatabaseIfMissing(_CreateDatabaseFileName);
            _Database.FileName = _CreateDatabaseFileName;

            var location = _Database.GetLocations()[0];
            var session = new BaseStationSession() { LocationID = location.LocationID };

            _Database.WriteSupportEnabled = true;
            _Database.InsertSession(session);
        }

        [TestMethod]
        public void BaseStationDatabase_CreateDatabaseIfMissing_Creates_SystemEvents_Table()
        {
            _Database.CreateDatabaseIfMissing(_CreateDatabaseFileName);
            _Database.FileName = _CreateDatabaseFileName;

            _Database.GetSystemEvents();
        }

        [TestMethod]
        public void BaseStationDatabase_CreateDatabaseIfMissing_Creates_Aircraft_Table()
        {
            _Database.CreateDatabaseIfMissing(_CreateDatabaseFileName);
            _Database.FileName = _CreateDatabaseFileName;

            _Database.WriteSupportEnabled = true;
            _Database.InsertAircraft(new BaseStationAircraft() { ModeS = "X" });
        }

        [TestMethod]
        public void BaseStationDatabase_CreateDatabaseIfMissing_Creates_Flights_Table()
        {
            _Database.CreateDatabaseIfMissing(_CreateDatabaseFileName);
            _Database.FileName = _CreateDatabaseFileName;

            var aircraft = new BaseStationAircraft() { ModeS = "1" };
            var session = new BaseStationSession() { StartTime = DateTime.Now };
            _Database.WriteSupportEnabled = true;
            _Database.InsertSession(session);
            _Database.InsertAircraft(aircraft);

            _Database.InsertFlight(new BaseStationFlight() { AircraftID = aircraft.AircraftID, SessionID = session.SessionID });
        }

        [TestMethod]
        public void BaseStationDatabase_CreateDatabaseIfMissing_Creates_Session_Trigger_To_Delete_Flights()
        {
            _Database.CreateDatabaseIfMissing(_CreateDatabaseFileName);
            _Database.FileName = _CreateDatabaseFileName;

            _Database.WriteSupportEnabled = true;

            var session = new BaseStationSession() { StartTime = DateTime.Now };
            _Database.InsertSession(session);

            var aircraft = new BaseStationAircraft() { ModeS = "K" };
            _Database.InsertAircraft(aircraft);

            var flight = new BaseStationFlight() { AircraftID = aircraft.AircraftID, SessionID = session.SessionID };
            _Database.InsertFlight(flight);

            _Database.DeleteSession(session);

            Assert.IsNull(_Database.GetFlightById(flight.FlightID));
            Assert.IsNotNull(_Database.GetAircraftById(aircraft.AircraftID));
        }

        [TestMethod]
        public void BaseStationDatabase_CreateDatabaseIfMissing_Creates_Aircraft_Trigger_To_Delete_Flights()
        {
            _Database.CreateDatabaseIfMissing(_CreateDatabaseFileName);
            _Database.FileName = _CreateDatabaseFileName;

            _Database.WriteSupportEnabled = true;

            var session = new BaseStationSession() { StartTime = DateTime.Now };
            _Database.InsertSession(session);

            var aircraft = new BaseStationAircraft() { ModeS = "K" };
            _Database.InsertAircraft(aircraft);

            var flight = new BaseStationFlight() { AircraftID = aircraft.AircraftID, SessionID = session.SessionID };
            _Database.InsertFlight(flight);

            _Database.DeleteAircraft(aircraft);

            Assert.IsNull(_Database.GetFlightById(flight.FlightID));
            Assert.AreEqual(session.SessionID, _Database.GetSessions()[0].SessionID);
        }
        #endregion

        #region Criteria & sort test helpers - IsFlightCriteria, IsFlightSortColumn, SetEqualityCriteria, SetRangeCriteria, SetSortColumnValue
        /// <summary>
        /// Returns true if the criteria property refers to a flight property as opposed to an actual property (e.g.
        /// callsign is a flight property and returns true while registration is an actual property and returns false).
        /// </summary>
        /// <param name="criteriaProperty"></param>
        /// <returns></returns>
        private bool IsFlightCriteria(PropertyInfo criteriaProperty)
        {
            switch(criteriaProperty.Name) {
                case "Date":
                case "IsEmergency":
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
        private bool IsFlightSortColumn(string sortColumn)
        {
            switch(sortColumn) {
                case "date":
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
        private bool IsFilterStringProperty(PropertyInfo criteriaProperty)
        {
            return typeof(FilterString).IsAssignableFrom(criteriaProperty.PropertyType);
        }

        /// <summary>
        /// Sets a property on a flight based on the name of a criteria property.
        /// </summary>
        /// <param name="criteriaProperty"></param>
        /// <param name="flight"></param>
        /// <param name="value"></param>
        private void SetStringAircraftProperty(PropertyInfo criteriaProperty, BaseStationFlight flight, string value)
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
        private bool SetEqualityCriteria(PropertyInfo criteriaProperty, BaseStationFlight defaultFlight, BaseStationFlight notEqualFlight, BaseStationFlight equalsFlight, bool reverseCondition)
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
        private bool SetContainsCriteria(PropertyInfo criteriaProperty, BaseStationFlight defaultFlight, BaseStationFlight notContainsFlight, BaseStationFlight containsFlight, bool reverseCondition)
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
        private bool SetStartsWithCriteria(PropertyInfo criteriaProperty, BaseStationFlight defaultFlight, BaseStationFlight notStartsWithFlight, BaseStationFlight startsWithFlight, bool reverseCondition)
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
        private bool SetEndsWithCriteria(PropertyInfo criteriaProperty, BaseStationFlight defaultFlight, BaseStationFlight notEndsWithFlight, BaseStationFlight endsWithFlight, bool reverseCondition)
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
        private bool SetRangeCriteria(PropertyInfo criteriaProperty, BaseStationFlight belowRangeFlight, BaseStationFlight startRangeFlight, BaseStationFlight inRangeFlight, BaseStationFlight endRangeFlight, BaseStationFlight aboveRangeFlight, bool reverseCondition)
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

        private void SetSortColumnValue(BaseStationFlight flight, string sortColumn, bool isDefault, bool isHigh)
        {
            var stringValue = isDefault ? sortColumn == "reg" || sortColumn == "icao" ? "" : null : isHigh ? "B" : "A";
            var dateValue = isDefault ? default(DateTime) : isHigh ? new DateTime(2001, 1, 2) : new DateTime(2001, 1, 1);

            switch(sortColumn) {
                case "callsign":    flight.Callsign = stringValue; break;
                case "country":     flight.Aircraft.ModeSCountry = stringValue; break;
                case "date":        flight.StartTime = dateValue; break;
                case "model":       flight.Aircraft.Type = stringValue; break;
                case "type":        flight.Aircraft.ICAOTypeCode = stringValue; break;
                case "operator":    flight.Aircraft.RegisteredOwners = stringValue; break;
                case "reg":         flight.Aircraft.Registration = stringValue; break;
                case "icao":        flight.Aircraft.ModeS = stringValue; break;
                default:            throw new NotImplementedException();
            }
        }
        #endregion

        #region AssertAircraftAreEqual, AssertFlightsAreEqual
        private static void AssertAircraftAreEqual(BaseStationAircraft expected, BaseStationAircraft actual, long id = -1L)
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
            Assert.AreEqual(expected.UserTag, actual.UserTag);
            Assert.AreEqual(expected.YearBuilt, actual.YearBuilt);
        }

        private static void AssertFlightsAreEqual(BaseStationFlight expected, BaseStationFlight actual, bool expectAircraftFilled, int expectedAircraftId)
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
        }
        #endregion

        #region Transactions
        [TestMethod]
        public void BaseStationDatabase_Transactions_Can_Commit_Operations_To_Database()
        {
            _Database.WriteSupportEnabled = true;
            _Database.StartTransaction();
            _Database.InsertAircraft(new BaseStationAircraft() { ModeS = "Z" });
            _Database.EndTransaction();

            var aircraft = _Database.GetAircraftByCode("Z");
            Assert.AreEqual("Z", aircraft.ModeS);
            Assert.AreNotEqual(0, aircraft.AircraftID);
        }

        [TestMethod]
        public void BaseStationDatabase_Transactions_Can_Rollback_Inserts()
        {
            _Database.WriteSupportEnabled = true;
            _Database.StartTransaction();
            _Database.InsertAircraft(new BaseStationAircraft() { ModeS = "Z" });
            _Database.RollbackTransaction();

            Assert.AreEqual(null, _Database.GetAircraftByCode("Z"));
        }

        [TestMethod]
        public void BaseStationDatabase_Transactions_Can_Be_Nested()
        {
            _Database.WriteSupportEnabled = true;
            _Database.StartTransaction();
            _Database.InsertAircraft(new BaseStationAircraft() { ModeS = "Z" });

            _Database.StartTransaction();
            var aircraft = _Database.GetAircraftByCode("Z");
            aircraft.Registration = "P";
            _Database.UpdateAircraft(aircraft);
            _Database.EndTransaction();

            _Database.EndTransaction();

            Assert.AreEqual("P", _Database.GetAircraftByCode("Z").Registration);
        }

        [TestMethod]
        public void BaseStationDatabase_Transactions_Can_Rollback_Outer_Level_When_Inner_Level_Rollsback()
        {
            _Database.WriteSupportEnabled = true;
            _Database.StartTransaction();
            _Database.InsertAircraft(new BaseStationAircraft() { ModeS = "Z" });

            _Database.StartTransaction();
            var aircraft = _Database.GetAircraftByCode("Z");
            aircraft.Registration = "P";
            _Database.UpdateAircraft(aircraft);
            _Database.RollbackTransaction();

            _Database.EndTransaction();

            Assert.AreEqual(null, _Database.GetAircraftByCode("Z"));
        }
        #endregion
    }
}
