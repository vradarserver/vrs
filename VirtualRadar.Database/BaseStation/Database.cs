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
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.SQLite;

namespace VirtualRadar.Database.BaseStation
{
    /// <summary>
    /// Default implementation of <see cref="IBaseStationDatabase"/>.
    /// </summary>
    sealed class Database : IBaseStationDatabase
    {
        #region Private class - DefaultProvider
        /// <summary>
        /// The default implementation of <see cref="IBaseStationDatabaseProvider"/>.
        /// </summary>
        class DefaultProvider : IBaseStationDatabaseProvider
        {
            public DateTime UtcNow { get { return DateTime.UtcNow; } }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The object that manages the ADO.NET for the Aircraft table for us.
        /// </summary>
        private AircraftTable _AircraftTable = new AircraftTable();

        /// <summary>
        /// The object that manages the ADO.NET for the Flights table for us.
        /// </summary>
        private FlightsTable _FlightTable = new FlightsTable();

        /// <summary>
        /// The object that manages the ADO.NET for the DBHistory table for us.
        /// </summary>
        private DBHistoryTable _DbHistoryTable = new DBHistoryTable();

        /// <summary>
        /// The object that manages the ADO.NET for the DBInfo table for us.
        /// </summary>
        private DBInfoTable _DbInfoTable = new DBInfoTable();

        /// <summary>
        /// The object that manages the ADO.NET for the Locations table for us.
        /// </summary>
        private LocationsTable _LocationsTable = new LocationsTable();

        /// <summary>
        /// The object that manages the ADO.NET for the Sessions table for us.
        /// </summary>
        private SessionsTable _SessionsTable = new SessionsTable();

        /// <summary>
        /// The object that manages the ADO.NET calls to the SystemEvents table for us.
        /// </summary>
        private SystemEventsTable _SystemEventsTable = new SystemEventsTable();

        /// <summary>
        /// The object that we synchronise threads on.
        /// </summary>
        private object _ConnectionLock = new Object();

        /// <summary>
        /// The connection to the database.
        /// </summary>
        private IDbConnection _Connection;

        /// <summary>
        /// The configuration loader whose events we have hooked.
        /// </summary>
        private IConfigurationStorage _ConfigurationStorage;

        /// <summary>
        /// The object that manages nestable transactions for us.
        /// </summary>
        private TransactionHelper _TransactionHelper = new TransactionHelper();

        /// <summary>
        /// The object that can write to the database log for us. This is created when <see cref="LogFileName"/> is set
        /// and the connection is open.
        /// </summary>
        private TextWriter _DatabaseLog;

        /// <summary>
        /// True if the object has been disposed.
        /// </summary>
        private bool _Disposed;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IBaseStationDatabaseProvider Provider { get; set; }

        private string _FileName;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string FileName
        {
            get { return _FileName; }
            set
            {
                if(_FileName != value) {
                    OnFileNameChanging(EventArgs.Empty);
                    _FileName = value;
                    CloseConnection();
                    OnFileNameChanged(EventArgs.Empty);
                }
            }
        }

        private string _LogFileName;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string LogFileName
        {
            get { return _LogFileName; }
            set
            {
                if(_LogFileName != value) {
                    _LogFileName = value;
                    OpenDatabaseLog();
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsConnected
        {
            get { return _Connection != null; }
        }

        private bool _WriteSupportEnabled;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool WriteSupportEnabled
        {
            get { return _WriteSupportEnabled; }
            set { if(_WriteSupportEnabled != value) { _WriteSupportEnabled = value; CloseConnection(); } }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int MaxParameters
        {
            get
            {
                lock(_ConnectionLock) {
                    OpenConnection();
                    return _Connection != null ? 900 : -1;
                }
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler FileNameChanging;

        /// <summary>
        /// Raises <see cref="FileNameChanging"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnFileNameChanging(EventArgs args)
        {
            if(FileNameChanging != null) FileNameChanging(this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler FileNameChanged;

        /// <summary>
        /// Raises <see cref="FileNameChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnFileNameChanged(EventArgs args)
        {
            if(FileNameChanged != null) FileNameChanged(this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<BaseStationAircraft>> AircraftUpdated;

        /// <summary>
        /// Raises <see cref="AircraftUpdated"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnAircraftUpdated(EventArgs<BaseStationAircraft> args)
        {
            if(AircraftUpdated != null) AircraftUpdated(this, args);
        }
        #endregion

        #region Constructors and finaliser
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Database()
        {
            Provider = new DefaultProvider();
            _ConfigurationStorage = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton;
            _ConfigurationStorage.ConfigurationChanged += ConfigurationStorage_ConfigurationChanged;
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~Database()
        {
            Dispose(false);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of or finalises the object. Note that the object is sealed.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if(disposing && !_Disposed) {
                lock(_ConnectionLock) {
                    _Disposed = true;

                    _TransactionHelper.Abandon();
                    CloseConnection();

                    if(_AircraftTable != null) {
                        _AircraftTable.Dispose();
                        _AircraftTable = null;
                    }

                    if(_FlightTable != null) {
                        _FlightTable.Dispose();
                        _FlightTable = null;
                    }

                    if(_DbHistoryTable != null) {
                        _DbHistoryTable.Dispose();
                        _DbHistoryTable = null;
                    }

                    if(_DbInfoTable != null) {
                        _DbInfoTable.Dispose();
                        _DbInfoTable = null;
                    }

                    if(_LocationsTable != null) {
                        _LocationsTable.Dispose();
                        _LocationsTable = null;
                    }

                    if(_SessionsTable != null) {
                        _SessionsTable.Dispose();
                        _SessionsTable = null;
                    }

                    if(_SystemEventsTable != null) {
                        _SystemEventsTable.Dispose();
                        _SystemEventsTable = null;
                    }

                    if(_ConfigurationStorage != null) {
                        _ConfigurationStorage.ConfigurationChanged -= ConfigurationStorage_ConfigurationChanged;
                        _ConfigurationStorage = null;
                    }

                    if(_DatabaseLog != null) {
                        _DatabaseLog.Flush();
                        _DatabaseLog.Dispose();
                        _DatabaseLog = null;
                    }
                }
            }
        }
        #endregion

        #region OpenConnection, CloseConnection, TestConnection
        /// <summary>
        /// Closes the connection and disposes of it if open.
        /// </summary>
        private void CloseConnection()
        {
            lock(_ConnectionLock) {
                if(_Connection != null) {
                    _Connection.Dispose();
                    _Connection = null;
                }
            }
        }

        /// <summary>
        /// Opens the connection if not already open.
        /// </summary>
        private void OpenConnection(string fileName = null, bool? writeSupportEnabled = null)
        {
            lock(_ConnectionLock) {
                if(_Connection == null && !_Disposed) {
                    bool inCreateMode = fileName != null && writeSupportEnabled.GetValueOrDefault();

                    if(fileName == null) fileName = FileName;
                    if(writeSupportEnabled == null) writeSupportEnabled = WriteSupportEnabled;

                    bool fileExists = File.Exists(fileName);
                    bool zeroLength = fileExists && new FileInfo(fileName).Length == 0;

                    if(!String.IsNullOrEmpty(fileName) && fileExists && (inCreateMode || !zeroLength)) {
                        var builder = Factory.Singleton.Resolve<ISQLiteConnectionStringBuilder>().Initialise();
                        builder.DataSource = fileName;
                        builder.ReadOnly = !writeSupportEnabled.Value;
                        builder.FailIfMissing = true;
                        builder.DateTimeFormat = SQLiteDateFormats.ISO8601;
                        builder.JournalMode = SQLiteJournalModeEnum.Default;  // <-- Persist causes problems with SBSPopulate
                        var connection = Factory.Singleton.Resolve<ISQLiteConnectionProvider>().Create(builder.ConnectionString);
                        _Connection = connection;
                        _Connection.Open();

                        OpenDatabaseLog();
                    }
                }
            }
        }

        /// <summary>
        /// Opens the log that holds traces of database accesses.
        /// </summary>
        private void OpenDatabaseLog()
        {
            if((_DatabaseLog == null && _LogFileName != null) || (_DatabaseLog != null && _LogFileName == null)) {
                if(_DatabaseLog != null) _DatabaseLog.Dispose();
                _DatabaseLog = null;

                if(LogFileName != null) {
                    var folder = Path.GetDirectoryName(LogFileName);
                    if(!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    _DatabaseLog = new StreamWriter(LogFileName, true);
                    _DatabaseLog.WriteLine("Started logging at {0} (UTC)", DateTime.UtcNow);
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public bool TestConnection()
        {
            bool result = false;
            lock(_ConnectionLock) {
                OpenConnection();
                result = _Connection != null;
            }

            return result;
        }
        #endregion

        #region CreateDatabaseIfMissing
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="fileName"></param>
        public void CreateDatabaseIfMissing(string fileName)
        {
            if(!String.IsNullOrEmpty(fileName)) {
                bool fileMissing = !File.Exists(fileName);
                bool fileEmpty = fileMissing || new FileInfo(fileName).Length == 0;
                if(fileMissing || fileEmpty) {
                    var configuration = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton.Load();

                    var folder = Path.GetDirectoryName(fileName);
                    if(!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    if(fileMissing) File.Create(fileName).Close();

                    CloseConnection();
                    OpenConnection(fileName, true);
                    try {
                        if(_Connection != null) {
                            var transaction = _Connection.BeginTransaction();
                            try {
                                _DbHistoryTable.CreateTable(_Connection, _DatabaseLog);
                                _DbInfoTable.CreateTable(_Connection, _DatabaseLog);
                                _SystemEventsTable.CreateTable(_Connection, _DatabaseLog);
                                _LocationsTable.CreateTable(_Connection, _DatabaseLog);
                                _SessionsTable.CreateTable(_Connection, _DatabaseLog);
                                _AircraftTable.CreateTable(_Connection, _DatabaseLog);
                                _FlightTable.CreateTable(_Connection, _DatabaseLog);

                                _SessionsTable.CreateTriggers(_Connection, _DatabaseLog);
                                _AircraftTable.CreateTriggers(_Connection, _DatabaseLog);

                                _DbHistoryTable.Insert(_Connection, null, _DatabaseLog, new BaseStationDBHistory() { Description = "Database autocreated by Virtual Radar Server", TimeStamp = SQLiteDateHelper.Truncate(Provider.UtcNow) });
                                _DbInfoTable.Insert(_Connection, null, _DatabaseLog, new BaseStationDBInfo() { OriginalVersion = 2, CurrentVersion = 2 });
                                _LocationsTable.Insert(_Connection, null, _DatabaseLog, new BaseStationLocation() { LocationName = "Home", Latitude = configuration.GoogleMapSettings.InitialMapLatitude, Longitude = configuration.GoogleMapSettings.InitialMapLongitude });

                                transaction.Commit();
                            } catch(Exception ex) {
                                Debug.WriteLine(String.Format("Database.CreateDatabaseIfMissing caught exception {0}", ex.ToString()));
                                transaction.Rollback();
                                throw;
                            }
                        }
                    } finally {
                        CloseConnection();
                    }
                }
            }
        }
        #endregion

        #region AttemptAutoFix
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="errorException"></param>
        /// <returns></returns>
        public bool AttemptAutoFix(Exception errorException)
        {
            bool result = false;

            var sqliteException = Factory.Singleton.Resolve<ISQLiteException>();
            sqliteException.Initialise(errorException);
            if(sqliteException.IsSQLiteException) {
                switch(sqliteException.ErrorCode) {
                    case SQLiteErrorCode.IoErr:
                        // One of two things. Either the file is trashed, in which case we need to extract everything
                        // and rebuild the index, or they've had something writing to the file and the journal is
                        // hanging around after a crash. We're going to try to fix the journal problem as that's the
                        // most common for now.

                        var journalFileName = String.Format("{0}-journal", FileName);
                        if(File.Exists(journalFileName)) {
                            // The easiest way to fix this is to open the file in read-write mode. If that fails then
                            // we should get rid of the journal file.
                            result = FixByOpeningInReadWriteMode();
                            if(!result || File.Exists(journalFileName)) {
                                result = FixByRenamingJournal(journalFileName);
                            }
                        }
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Forces the connection to be temporarily opened in read-write mode.
        /// Returns true if it didn't throw an exception. Swallows all exceptions.
        /// </summary>
        /// <returns></returns>
        private bool FixByOpeningInReadWriteMode()
        {
            var result = false;

            try {
                CloseConnection();
                OpenConnection(writeSupportEnabled: true);
                result = true;
            } catch {
                result = false;
            } finally {
                try {
                    CloseConnection();
                } catch {
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// Renames the journal file. Returns true if the file could be successfully renamed.
        /// </summary>
        /// <param name="journalFileName"></param>
        /// <returns></returns>
        private bool FixByRenamingJournal(string journalFileName)
        {
            bool result = false;

            string newFileName = null;
            for(var i = 1;i < 1000 && newFileName == null;++i) {
                newFileName = String.Format("{0} (bad){1}", journalFileName, i == 1 ? "" : String.Format(" ({0})", i));
                if(File.Exists(newFileName)) newFileName = null;
            }

            if(newFileName != null) {
                try {
                    File.Move(journalFileName, newFileName);
                    result = true;
                } catch {
                    result = false;
                }
            }

            return result;
        }
        #endregion

        #region StartTransaction, EndTransaction, RollbackTransaction
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void StartTransaction()
        {
            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) _TransactionHelper.StartTransaction(_Connection);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void EndTransaction()
        {
            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) _TransactionHelper.EndTransaction();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void RollbackTransaction()
        {
            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) _TransactionHelper.RollbackTransaction();
            }
        }
        #endregion

        #region Aircraft table handling
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        public BaseStationAircraft GetAircraftByRegistration(string registration)
        {
            BaseStationAircraft result = null;

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) result = _AircraftTable.GetByRegistration(_Connection, _TransactionHelper.Transaction, _DatabaseLog, registration);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao24"></param>
        /// <returns></returns>
        public BaseStationAircraft GetAircraftByCode(string icao24)
        {
            BaseStationAircraft result = null;

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) result = _AircraftTable.GetByIcao(_Connection, _TransactionHelper.Transaction, _DatabaseLog, icao24);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao24s"></param>
        /// <returns></returns>
        public Dictionary<string, BaseStationAircraft> GetManyAircraftByCode(IEnumerable<string> icao24s)
        {
            var result = new Dictionary<string, BaseStationAircraft>();

            if(icao24s != null && icao24s.Any()) {
                lock(_ConnectionLock) {
                    OpenConnection();
                    if(_Connection != null) {
                        var maxParameters = MaxParameters;
                        var offset = 0;
                        var countIcao24s = icao24s.Count();

                        do {
                            var chunk = icao24s.Skip(offset).Take(maxParameters).ToArray();
                            foreach(var aircraft in _AircraftTable.GetManyByIcao(_Connection, _TransactionHelper.Transaction, _DatabaseLog, chunk)) {
                                if(aircraft.ModeS != null && !result.ContainsKey(aircraft.ModeS)) result.Add(aircraft.ModeS, aircraft);
                            }
                            offset += maxParameters;
                        } while(offset < countIcao24s);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao24s"></param>
        /// <returns></returns>
        public Dictionary<string, BaseStationAircraftAndFlightsCount> GetManyAircraftAndFlightsCountByCode(IEnumerable<string> icao24s)
        {
            var result = new Dictionary<string, BaseStationAircraftAndFlightsCount>();

            if(icao24s != null && icao24s.Any()) {
                lock(_ConnectionLock) {
                    OpenConnection();
                    if(_Connection != null) {
                        var maxParameters = MaxParameters;
                        var offset = 0;
                        var countIcao24s = icao24s.Count();

                        do {
                            var chunk = icao24s.Skip(offset).Take(maxParameters).ToArray();
                            foreach(var aircraft in _AircraftTable.GetManyAircraftAndFlightsByIcao(_Connection, _TransactionHelper.Transaction, _DatabaseLog, chunk)) {
                                if(aircraft.ModeS != null && !result.ContainsKey(aircraft.ModeS)) result.Add(aircraft.ModeS, aircraft);
                            }
                            offset += maxParameters;
                        } while(offset < countIcao24s);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public BaseStationAircraft GetAircraftById(int id)
        {
            BaseStationAircraft result = null;

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) result = _AircraftTable.GetById(_Connection, _TransactionHelper.Transaction, _DatabaseLog, id);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraft"></param>
        public void InsertAircraft(BaseStationAircraft aircraft)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot insert aircraft when write support is disabled");

            aircraft.FirstCreated = SQLiteDateHelper.Truncate(aircraft.FirstCreated);
            aircraft.LastModified = SQLiteDateHelper.Truncate(aircraft.LastModified);

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) aircraft.AircraftID = _AircraftTable.Insert(_Connection, _TransactionHelper.Transaction, _DatabaseLog, aircraft);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraft"></param>
        public void UpdateAircraft(BaseStationAircraft aircraft)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot update aircraft when write support is disabled");

            aircraft.FirstCreated = SQLiteDateHelper.Truncate(aircraft.FirstCreated);
            aircraft.LastModified = SQLiteDateHelper.Truncate(aircraft.LastModified);

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) {
                    _AircraftTable.Update(_Connection, _TransactionHelper.Transaction, _DatabaseLog, aircraft);
                }
            }

            OnAircraftUpdated(new EventArgs<BaseStationAircraft>(aircraft));
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraftId"></param>
        /// <param name="modeSCountry"></param>
        public void UpdateAircraftModeSCountry(int aircraftId, string modeSCountry)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot update the Mode-S country for an aircraft when write support is disabled");

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) _AircraftTable.UpdateModeSCountry(_Connection, _TransactionHelper.Transaction, _DatabaseLog, aircraftId, modeSCountry);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraft"></param>
        public void DeleteAircraft(BaseStationAircraft aircraft)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot delete aircraft when write support is disabled");

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) _AircraftTable.Delete(_Connection, _TransactionHelper.Transaction, _DatabaseLog, aircraft);
            }
        }
        #endregion

        #region Flights table handling
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public int GetCountOfFlights(SearchBaseStationCriteria criteria)
        {
            if(criteria == null) throw new ArgumentNullException("criteria");
            FlightsTable.NormaliseCriteria(criteria);

            int result = 0;

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) result = _FlightTable.GetCount(_Connection, null, _DatabaseLog, criteria);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public int GetCountOfFlightsForAircraft(BaseStationAircraft aircraft, SearchBaseStationCriteria criteria)
        {
            if(criteria == null) throw new ArgumentNullException("criteria");
            FlightsTable.NormaliseCriteria(criteria);

            int result = 0;

            if(aircraft != null) {
                lock(_ConnectionLock) {
                    OpenConnection();
                    if(_Connection != null) result = _FlightTable.GetCountForAircraft(_Connection, null, _DatabaseLog, aircraft, criteria);
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="criteria"></param>
        /// <param name="fromRow"></param>
        /// <param name="toRow"></param>
        /// <param name="sort1"></param>
        /// <param name="sort1Ascending"></param>
        /// <param name="sort2"></param>
        /// <param name="sort2Ascending"></param>
        /// <returns></returns>
        public List<BaseStationFlight> GetFlightsForAircraft(BaseStationAircraft aircraft, SearchBaseStationCriteria criteria, int fromRow, int toRow, string sort1, bool sort1Ascending, string sort2, bool sort2Ascending)
        {
            if(criteria == null) throw new ArgumentNullException("criteria");
            FlightsTable.NormaliseCriteria(criteria);

            List<BaseStationFlight> result = new List<BaseStationFlight>();

            if(aircraft != null) {
                lock(_ConnectionLock) {
                    OpenConnection();
                    if(_Connection != null) result = _FlightTable.GetForAircraft(_Connection, null, _DatabaseLog, aircraft, criteria, fromRow, toRow, sort1, sort1Ascending, sort2, sort2Ascending);
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="fromRow"></param>
        /// <param name="toRow"></param>
        /// <param name="sortField1"></param>
        /// <param name="sortField1Ascending"></param>
        /// <param name="sortField2"></param>
        /// <param name="sortField2Ascending"></param>
        /// <returns></returns>
        public List<BaseStationFlight> GetFlights(SearchBaseStationCriteria criteria, int fromRow, int toRow, string sortField1, bool sortField1Ascending, string sortField2, bool sortField2Ascending)
        {
            if(criteria == null) throw new ArgumentNullException("criteria");
            FlightsTable.NormaliseCriteria(criteria);

            List<BaseStationFlight> result = new List<BaseStationFlight>();

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) result = _FlightTable.GetFlights(_Connection, null, _DatabaseLog, criteria, fromRow, toRow, sortField1, sortField1Ascending, sortField2, sortField2Ascending);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public BaseStationFlight GetFlightById(int id)
        {
            BaseStationFlight result = null;

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) result = _FlightTable.GetById(_Connection, _TransactionHelper.Transaction, _DatabaseLog, id);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="flight"></param>
        public void InsertFlight(BaseStationFlight flight)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot insert flights when writes are disabled");

            flight.StartTime = SQLiteDateHelper.Truncate(flight.StartTime);
            flight.EndTime = SQLiteDateHelper.Truncate(flight.EndTime);

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) flight.FlightID = _FlightTable.Insert(_Connection, _TransactionHelper.Transaction, _DatabaseLog, flight);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="flight"></param>
        public void UpdateFlight(BaseStationFlight flight)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot update flights when writes are disabled");

            flight.StartTime = SQLiteDateHelper.Truncate(flight.StartTime);
            flight.EndTime = SQLiteDateHelper.Truncate(flight.EndTime);

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) _FlightTable.Update(_Connection, _TransactionHelper.Transaction, _DatabaseLog, flight);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="flight"></param>
        public void DeleteFlight(BaseStationFlight flight)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot delete flights when writes are disabled");

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) _FlightTable.Delete(_Connection, _TransactionHelper.Transaction, _DatabaseLog, flight);
            }
        }
        #endregion

        #region DBHistory table handling
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IList<BaseStationDBHistory> GetDatabaseHistory()
        {
            IList<BaseStationDBHistory> result = null;

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) result = _DbHistoryTable.GetAllRecords(_Connection, _TransactionHelper.Transaction, _DatabaseLog);
                else                    result = new List<BaseStationDBHistory>();
            }

            return result;
        }
        #endregion

        #region DBInfo table handling
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public BaseStationDBInfo GetDatabaseVersion()
        {
            BaseStationDBInfo result = null;

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) result = _DbInfoTable.GetAllRecords(_Connection, _TransactionHelper.Transaction, _DatabaseLog).Last();
            }

            return result;
        }
        #endregion

        #region SystemEvents table handling
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IList<BaseStationSystemEvents> GetSystemEvents()
        {
            IList<BaseStationSystemEvents> result = null;

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection == null) result = new List<BaseStationSystemEvents>();
                else                    result = _SystemEventsTable.GetAllRecords(_Connection, _TransactionHelper.Transaction, _DatabaseLog);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="systemEvent"></param>
        public void InsertSystemEvent(BaseStationSystemEvents systemEvent)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot insert a system event record when write support is disabled");

            systemEvent.TimeStamp = SQLiteDateHelper.Truncate(systemEvent.TimeStamp);

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) systemEvent.SystemEventsID = _SystemEventsTable.Insert(_Connection, _TransactionHelper.Transaction, _DatabaseLog, systemEvent);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="systemEvent"></param>
        public void UpdateSystemEvent(BaseStationSystemEvents systemEvent)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot update a system event record when write support is disabled");

            systemEvent.TimeStamp = SQLiteDateHelper.Truncate(systemEvent.TimeStamp);

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) _SystemEventsTable.Update(_Connection, _TransactionHelper.Transaction, _DatabaseLog, systemEvent);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="systemEvent"></param>
        public void DeleteSystemEvent(BaseStationSystemEvents systemEvent)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot delete a system event record when write support is disabled");

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) _SystemEventsTable.Delete(_Connection, _TransactionHelper.Transaction, _DatabaseLog, systemEvent);
            }
        }
        #endregion

        #region Locations table handling
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IList<BaseStationLocation> GetLocations()
        {
            IList<BaseStationLocation> result = null;

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) result = _LocationsTable.GetAllRecords(_Connection, _TransactionHelper.Transaction, _DatabaseLog);
                else                    result = new BaseStationLocation[] {};
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="location"></param>
        public void InsertLocation(BaseStationLocation location)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot insert new location records when write support is disabled");

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) location.LocationID = _LocationsTable.Insert(_Connection, _TransactionHelper.Transaction, _DatabaseLog, location);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="location"></param>
        public void UpdateLocation(BaseStationLocation location)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot update location records when write support is disabled");

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) _LocationsTable.Update(_Connection, _TransactionHelper.Transaction, _DatabaseLog, location);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="location"></param>
        public void DeleteLocation(BaseStationLocation location)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot delete location records when write support is disabled");

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) _LocationsTable.Delete(_Connection, _TransactionHelper.Transaction, _DatabaseLog, location);
            }
        }
        #endregion

        #region Sessions table handling
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IList<BaseStationSession> GetSessions()
        {
            IList<BaseStationSession> result = null;

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) result = _SessionsTable.GetAllRecords(_Connection, _TransactionHelper.Transaction, _DatabaseLog);
                else                    result = new BaseStationSession[] {};
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="session"></param>
        public void InsertSession(BaseStationSession session)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You must enable writes before you can insert a session record");

            session.StartTime = SQLiteDateHelper.Truncate(session.StartTime);
            session.EndTime = SQLiteDateHelper.Truncate(session.EndTime);

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) session.SessionID = _SessionsTable.Insert(_Connection, _TransactionHelper.Transaction, _DatabaseLog, session);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="session"></param>
        public void UpdateSession(BaseStationSession session)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You must enable writes before you can update a session record");

            session.StartTime = SQLiteDateHelper.Truncate(session.StartTime);
            session.EndTime = SQLiteDateHelper.Truncate(session.EndTime);

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) _SessionsTable.Update(_Connection, _TransactionHelper.Transaction, _DatabaseLog, session);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="session"></param>
        public void DeleteSession(BaseStationSession session)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You must enable writes before you can delete a session record");

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) _SessionsTable.Delete(_Connection, _TransactionHelper.Transaction, _DatabaseLog, session);
            }
        }
        #endregion

        #region Events subscribed
        /// <summary>
        /// Raised when the configuration is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ConfigurationStorage_ConfigurationChanged(object sender, EventArgs args)
        {
            var configuration = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton.Load();
            if(configuration.BaseStationSettings.DatabaseFileName != FileName) {
                OnFileNameChanging(EventArgs.Empty);
                OnFileNameChanged(EventArgs.Empty);
            }
        }
        #endregion
    }
}
