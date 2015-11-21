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
using Dapper;

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
        /// The object that can parse callsigns out into alternates for us.
        /// </summary>
        private ICallsignParser _CallsignParser;

        /// <summary>
        /// True if the object has been disposed.
        /// </summary>
        private bool _Disposed;

        /// <summary>
        /// The object that handles the time for us.
        /// </summary>
        private IClock _Clock;
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
            EventHelper.Raise(FileNameChanging, this, args);
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
            EventHelper.Raise(FileNameChanged, this, args);
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
            EventHelper.Raise(AircraftUpdated, this, args);
        }
        #endregion

        #region Constructors and finaliser
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Database()
        {
            Provider = new DefaultProvider();
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

        #region Initialise, OpenConnection, CloseConnection, TestConnection
        /// <summary>
        /// Does first-time initialisation.
        /// </summary>
        private void Initialise()
        {
            if(_ConfigurationStorage == null) {
                _ConfigurationStorage = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton;
                _ConfigurationStorage.ConfigurationChanged += ConfigurationStorage_ConfigurationChanged;
            }

            if(_Clock == null) {
                _Clock = Factory.Singleton.Resolve<IClock>();
            }
        }

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
            Initialise();

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
                                _Connection.Execute(Commands.UpdateSchema, transaction: _TransactionHelper.Transaction);

                                DBHistory_Insert(new BaseStationDBHistory() { Description = "Database autocreated by Virtual Radar Server", TimeStamp = SQLiteDateHelper.Truncate(Provider.UtcNow) });
                                DBInfo_Insert(new BaseStationDBInfo() { OriginalVersion = 2, CurrentVersion = 2 });
                                Locations_Insert(new BaseStationLocation() { LocationName = "Home", Latitude = configuration.GoogleMapSettings.InitialMapLatitude, Longitude = configuration.GoogleMapSettings.InitialMapLongitude });

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
                if(_Connection != null) {
                    _TransactionHelper.StartTransaction(_Connection);
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void EndTransaction()
        {
            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) {
                    _TransactionHelper.EndTransaction();
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void RollbackTransaction()
        {
            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) {
                    _TransactionHelper.RollbackTransaction();
                }
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
                if(_Connection != null) result = Aircraft_GetByRegistration(registration);
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
                if(_Connection != null) result = Aircraft_GetByIcao(icao24);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public List<BaseStationAircraft> GetAllAircraft()
        {
            var result = new List<BaseStationAircraft>();

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) result.AddRange(Aircraft_GetAll());
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
                            foreach(var aircraft in Aircraft_GetByIcaos(chunk)) {
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
                            foreach(var aircraft in AircraftAndFlightsCount_GetByIcaos(chunk)) {
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
                if(_Connection != null) result = Aircraft_GetById(id);
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

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) {
                    Aircraft_Insert(aircraft);
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao24"></param>
        /// <param name="createNewAircraftFunc"></param>
        /// <returns></returns>
        public BaseStationAircraft GetOrInsertAircraftByCode(string icao24, Func<string, BaseStationAircraft> createNewAircraftFunc)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot insert aircraft when write support is disabled");

            BaseStationAircraft result = null;
            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) {
                    result = Aircraft_GetByIcao(icao24);
                    if(result == null) {
                        result = createNewAircraftFunc(icao24);
                        Aircraft_Insert(result);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraft"></param>
        public void UpdateAircraft(BaseStationAircraft aircraft)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot update aircraft when write support is disabled");

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) {
                    Aircraft_Update(aircraft);
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
                if(_Connection != null) {
                    Aircraft_UpdateModeSCountry(aircraftId, modeSCountry);
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao"></param>
        public void RecordEmptyAircraft(string icao)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot record empty aircraft when write support is disabled");

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) {
                    var localNow = _Clock.LocalNow;

                    var aircraft = Aircraft_GetByIcao(icao);
                    RecordEmptyAircraft(aircraft, icao, localNow);
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icaos"></param>
        public void RecordManyEmptyAircraft(IEnumerable<string> icaos)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot record empty aircraft when write support is disabled");

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) {
                    var localNow = _Clock.LocalNow;
                    Func<string, string> normaliseIcao = (icao) => { return icao.ToUpper(); };
                    var allAircraft = Aircraft_GetByIcaos(icaos).ToDictionary(r => normaliseIcao(r.ModeS), r => r);

                    _TransactionHelper.StartTransaction(_Connection);
                    try {
                        foreach(var icao in icaos) {
                            BaseStationAircraft aircraft;
                            allAircraft.TryGetValue(normaliseIcao(icao), out aircraft);
                            RecordEmptyAircraft(aircraft, icao, localNow);
                        }
                        _TransactionHelper.EndTransaction();
                    } catch {
                        _TransactionHelper.RollbackTransaction();
                        throw;
                    }
                }
            }
        }

        private void RecordEmptyAircraft(BaseStationAircraft aircraft, string icao, DateTime localNow)
        {
            if(aircraft != null) {
                if(String.IsNullOrEmpty(aircraft.Registration)) {
                    aircraft.LastModified = localNow;
                    Aircraft_Update(aircraft);
                }
            } else {
                aircraft = new BaseStationAircraft() {
                    ModeS = icao,
                    FirstCreated = localNow,
                    LastModified = localNow,
                };
                Aircraft_Insert(aircraft);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao"></param>
        /// <param name="fillAircraft"></param>
        /// <returns></returns>
        public BaseStationAircraft UpsertAircraftByCode(string icao, Func<BaseStationAircraft, BaseStationAircraft> fillAircraft)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot upsert aircraft when write support is disabled");

            BaseStationAircraft result = null;
            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) {
                    var localNow = _Clock.LocalNow;

                    result = Aircraft_GetByIcao(icao);
                    result = UpsertAircraftByCode(result, icao, fillAircraft);
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icaos"></param>
        /// <param name="fillAircraft"></param>
        /// <returns></returns>
        public BaseStationAircraft[] UpsertManyAircraftByCodes(IEnumerable<string> icaos, Func<BaseStationAircraft, BaseStationAircraft> fillAircraft)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot upsert aircraft when write support is disabled");

            var result = new List<BaseStationAircraft>();
            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) {
                    var localNow = _Clock.LocalNow;
                    Func<string, string> normaliseIcao = (icao) => { return icao.ToUpper(); };
                    var allAircraft = Aircraft_GetByIcaos(icaos).ToDictionary(r => normaliseIcao(r.ModeS), r => r);

                    _TransactionHelper.StartTransaction(_Connection);
                    try {
                        foreach(var icao in icaos) {
                            BaseStationAircraft aircraft;
                            allAircraft.TryGetValue(normaliseIcao(icao), out aircraft);
                            aircraft = UpsertAircraftByCode(aircraft, icao, fillAircraft);
                            if(aircraft != null) result.Add(aircraft);
                        }
                        _TransactionHelper.EndTransaction();
                    } catch {
                        _TransactionHelper.RollbackTransaction();
                        throw;
                    }
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Does the work for the UpsertAircraftByCode methods.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="icao"></param>
        /// <param name="fillAircraft"></param>
        /// <returns></returns>
        private BaseStationAircraft UpsertAircraftByCode(BaseStationAircraft aircraft, string icao, Func<BaseStationAircraft, BaseStationAircraft> fillAircraft)
        {
            var isNewAircraft = aircraft == null;
            if(isNewAircraft) {
                aircraft = new BaseStationAircraft() {
                    ModeS = icao,
                    FirstCreated = _Clock.LocalNow,
                    LastModified = _Clock.LocalNow,
                };
            }

            aircraft = fillAircraft(aircraft);
            if(aircraft != null) {
                if(isNewAircraft) {
                    Aircraft_Insert(aircraft);
                } else {
                    Aircraft_Update(aircraft);
                }
            }

            return aircraft;
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
                if(_Connection != null) {
                    Aircraft_Delete(aircraft);
                }
            }
        }

        void Aircraft_Delete(BaseStationAircraft aircraft)
        {
            _Connection.Execute("DELETE FROM [Aircraft] WHERE [AircraftID] = @aircraftID", new {
                @aircraftID = aircraft.AircraftID,
            }, transaction: _TransactionHelper.Transaction);
        }

        BaseStationAircraft[] Aircraft_GetAll()
        {
            return _Connection.Query<BaseStationAircraft>("SELECT * FROM [Aircraft]", transaction: _TransactionHelper.Transaction).ToArray();
        }

        BaseStationAircraft Aircraft_GetByIcao(string icao)
        {
            return _Connection.Query<BaseStationAircraft>("SELECT * FROM [Aircraft] WHERE [ModeS] = @icao", new {
                @icao = icao,
            }, transaction: _TransactionHelper.Transaction).FirstOrDefault();
        }

        BaseStationAircraft[] Aircraft_GetByIcaos(IEnumerable<string> icaos)
        {
            return _Connection.Query<BaseStationAircraft>("SELECT * FROM [Aircraft] WHERE [ModeS] IN @icaos", new {
                @icaos = icaos,
            }, transaction: _TransactionHelper.Transaction).ToArray();
        }

        BaseStationAircraft Aircraft_GetById(int aircraftId)
        {
            return _Connection.Query<BaseStationAircraft>("SELECT * FROM [Aircraft] WHERE [AircraftID] = @aircraftId", new {
                @aircraftId = aircraftId,
            }, transaction: _TransactionHelper.Transaction).FirstOrDefault();
        }

        BaseStationAircraft Aircraft_GetByRegistration(string registration)
        {
            return _Connection.Query<BaseStationAircraft>("SELECT * FROM [Aircraft] WHERE [Registration] = @registration", new {
                @registration = registration,
            }, transaction: _TransactionHelper.Transaction).FirstOrDefault();
        }

        void Aircraft_Insert(BaseStationAircraft aircraft)
        {
            aircraft.FirstCreated = SQLiteDateHelper.Truncate(aircraft.FirstCreated);
            aircraft.LastModified = SQLiteDateHelper.Truncate(aircraft.LastModified);

            aircraft.AircraftID = (int)_Connection.ExecuteScalar<long>(Commands.Aircraft_Insert, new {
                @aircraftClass      = aircraft.AircraftClass,
                @cofACategory       = aircraft.CofACategory,
                @cofAExpiry         = aircraft.CofAExpiry,
                @country            = aircraft.Country,
                @currentRegDate     = aircraft.CurrentRegDate,
                @deRegDate          = aircraft.DeRegDate,
                @engines            = aircraft.Engines,
                @firstCreated       = aircraft.FirstCreated,
                @firstRegDate       = aircraft.FirstRegDate,
                @genericName        = aircraft.GenericName,
                @iCAOTypeCode       = aircraft.ICAOTypeCode,
                @infoUrl            = aircraft.InfoUrl,
                @interested         = aircraft.Interested,
                @lastModified       = aircraft.LastModified,
                @manufacturer       = aircraft.Manufacturer,
                @modeS              = aircraft.ModeS,
                @modeSCountry       = aircraft.ModeSCountry,
                @mtow               = aircraft.MTOW,
                @operatorFlagCode   = aircraft.OperatorFlagCode,
                @ownershipStatus    = aircraft.OwnershipStatus,
                @pictureUrl1        = aircraft.PictureUrl1,
                @pictureUrl2        = aircraft.PictureUrl2,
                @pictureUrl3        = aircraft.PictureUrl3,
                @popularName        = aircraft.PopularName,
                @previousID         = aircraft.PreviousID,
                @registeredOwners   = aircraft.RegisteredOwners,
                @registration       = aircraft.Registration,
                @serialNo           = aircraft.SerialNo,
                @status             = aircraft.Status,
                @totalHours         = aircraft.TotalHours,
                @type               = aircraft.Type,
                @userNotes          = aircraft.UserNotes,
                @userTag            = aircraft.UserTag,
                @yearBuilt          = aircraft.YearBuilt,
            }, transaction: _TransactionHelper.Transaction);
        }

        void Aircraft_Update(BaseStationAircraft aircraft)
        {
            aircraft.FirstCreated = SQLiteDateHelper.Truncate(aircraft.FirstCreated);
            aircraft.LastModified = SQLiteDateHelper.Truncate(aircraft.LastModified);

            _Connection.Execute(Commands.Aircraft_Update, new {
                @aircraftClass      = aircraft.AircraftClass,
                @cofACategory       = aircraft.CofACategory,
                @cofAExpiry         = aircraft.CofAExpiry,
                @country            = aircraft.Country,
                @currentRegDate     = aircraft.CurrentRegDate,
                @deRegDate          = aircraft.DeRegDate,
                @engines            = aircraft.Engines,
                @firstCreated       = aircraft.FirstCreated,
                @firstRegDate       = aircraft.FirstRegDate,
                @genericName        = aircraft.GenericName,
                @iCAOTypeCode       = aircraft.ICAOTypeCode,
                @infoUrl            = aircraft.InfoUrl,
                @interested         = aircraft.Interested,
                @lastModified       = aircraft.LastModified,
                @manufacturer       = aircraft.Manufacturer,
                @modeS              = aircraft.ModeS,
                @modeSCountry       = aircraft.ModeSCountry,
                @mtow               = aircraft.MTOW,
                @operatorFlagCode   = aircraft.OperatorFlagCode,
                @ownershipStatus    = aircraft.OwnershipStatus,
                @pictureUrl1        = aircraft.PictureUrl1,
                @pictureUrl2        = aircraft.PictureUrl2,
                @pictureUrl3        = aircraft.PictureUrl3,
                @popularName        = aircraft.PopularName,
                @previousID         = aircraft.PreviousID,
                @registeredOwners   = aircraft.RegisteredOwners,
                @registration       = aircraft.Registration,
                @serialNo           = aircraft.SerialNo,
                @status             = aircraft.Status,
                @totalHours         = aircraft.TotalHours,
                @type               = aircraft.Type,
                @userNotes          = aircraft.UserNotes,
                @userTag            = aircraft.UserTag,
                @yearBuilt          = aircraft.YearBuilt,
                @aircraftID         = aircraft.AircraftID,
            }, transaction: _TransactionHelper.Transaction);
        }

        void Aircraft_UpdateModeSCountry(int aircraftId, string modeSCountry)
        {
            _Connection.Execute("UPDATE [Aircraft] SET [ModeSCountry] = @modeSCountry WHERE [AircraftID] = @aircraftID", new {
                @aircraftID =   aircraftId,
                @modeSCountry = modeSCountry,
            }, transaction: _TransactionHelper.Transaction);
        }

        BaseStationAircraftAndFlightsCount[] AircraftAndFlightsCount_GetByIcaos(IEnumerable<string> icaos)
        {
            return _Connection.Query<BaseStationAircraftAndFlightsCount>(Commands.Aircraft_GetAircraftAndFlightsCountByIcao, new {
                @icaos = icaos,
            }, transaction: _TransactionHelper.Transaction).ToArray();
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
            NormaliseCriteria(criteria);

            int result = 0;

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) result = Flights_GetCountByCriteria(null, criteria);
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
            NormaliseCriteria(criteria);

            int result = 0;

            if(aircraft != null) {
                lock(_ConnectionLock) {
                    OpenConnection();
                    if(_Connection != null) result = Flights_GetCountByCriteria(aircraft, criteria);
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
            NormaliseCriteria(criteria);

            List<BaseStationFlight> result = new List<BaseStationFlight>();

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) result = Flights_GetByCriteria(null, criteria, fromRow, toRow, sortField1, sortField1Ascending, sortField2, sortField2Ascending);
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
            NormaliseCriteria(criteria);

            List<BaseStationFlight> result = new List<BaseStationFlight>();

            if(aircraft != null) {
                lock(_ConnectionLock) {
                    OpenConnection();
                    if(_Connection != null) result = Flights_GetByCriteria(aircraft, criteria, fromRow, toRow, sort1, sort1Ascending, sort2, sort2Ascending);
                }
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
                if(_Connection != null) result = Flights_GetById(id);
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
                if(_Connection != null) {
                    Flights_Insert(flight);
                }
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
                if(_Connection != null) {
                    Flights_Update(flight);
                }
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
                if(_Connection != null) {
                    Flights_Delete(flight);
                }
            }
        }

        private void Flights_Delete(BaseStationFlight flight)
        {
            _Connection.Execute("DELETE FROM [Flights] WHERE [FlightID] = @flightID", new {
                @flightID = flight.FlightID,
            }, transaction: _TransactionHelper.Transaction);
        }

        private BaseStationFlight Flights_GetById(int flightId)
        {
            return _Connection.Query<BaseStationFlight>("SELECT * FROM [Flights] WHERE [FlightID] = @flightID", new {
                @flightID = flightId,
            }, transaction: _TransactionHelper.Transaction).FirstOrDefault();
        }

        private void Flights_Insert(BaseStationFlight flight)
        {
            flight.FlightID = (int)_Connection.ExecuteScalar<long>(Commands.Flights_Insert, new {
                @sessionID              = flight.SessionID,
                @aircraftID             = flight.AircraftID,
                @startTime              = flight.StartTime,
                @endTime                = flight.EndTime,
                @callsign               = flight.Callsign,
                @numPosMsgRec           = flight.NumPosMsgRec,
                @numADSBMsgRec          = flight.NumADSBMsgRec,
                @numModeSMsgRec         = flight.NumModeSMsgRec,
                @numIDMsgRec            = flight.NumIDMsgRec,
                @numSurPosMsgRec        = flight.NumSurPosMsgRec,
                @numAirPosMsgRec        = flight.NumAirPosMsgRec,
                @numAirVelMsgRec        = flight.NumAirVelMsgRec,
                @numSurAltMsgRec        = flight.NumSurAltMsgRec,
                @numSurIDMsgRec         = flight.NumSurIDMsgRec,
                @numAirToAirMsgRec      = flight.NumAirToAirMsgRec,
                @numAirCallRepMsgRec    = flight.NumAirCallRepMsgRec,
                @firstIsOnGround        = flight.FirstIsOnGround,
                @lastIsOnGround         = flight.LastIsOnGround,
                @firstLat               = flight.FirstLat,
                @lastLat                = flight.LastLat,
                @firstLon               = flight.FirstLon,
                @lastLon                = flight.LastLon,
                @firstGroundSpeed       = flight.FirstGroundSpeed,
                @lastGroundSpeed        = flight.LastGroundSpeed,
                @firstAltitude          = flight.FirstAltitude,
                @lastAltitude           = flight.LastAltitude,
                @firstVerticalRate      = flight.FirstVerticalRate,
                @lastVerticalRate       = flight.LastVerticalRate,
                @firstTrack             = flight.FirstTrack,
                @lastTrack              = flight.LastTrack,
                @firstSquawk            = flight.FirstSquawk,
                @lastSquawk             = flight.LastSquawk,
                @hadAlert               = flight.HadAlert,
                @hadEmergency           = flight.HadEmergency,
                @hadSPI                 = flight.HadSpi,
            }, transaction: _TransactionHelper.Transaction);
        }

        private void Flights_Update(BaseStationFlight flight)
        {
            _Connection.Execute(Commands.Flights_Update, new {
                @sessionID              = flight.SessionID,
                @aircraftID             = flight.AircraftID,
                @startTime              = flight.StartTime,
                @endTime                = flight.EndTime,
                @callsign               = flight.Callsign,
                @numPosMsgRec           = flight.NumPosMsgRec,
                @numADSBMsgRec          = flight.NumADSBMsgRec,
                @numModeSMsgRec         = flight.NumModeSMsgRec,
                @numIDMsgRec            = flight.NumIDMsgRec,
                @numSurPosMsgRec        = flight.NumSurPosMsgRec,
                @numAirPosMsgRec        = flight.NumAirPosMsgRec,
                @numAirVelMsgRec        = flight.NumAirVelMsgRec,
                @numSurAltMsgRec        = flight.NumSurAltMsgRec,
                @numSurIDMsgRec         = flight.NumSurIDMsgRec,
                @numAirToAirMsgRec      = flight.NumAirToAirMsgRec,
                @numAirCallRepMsgRec    = flight.NumAirCallRepMsgRec,
                @firstIsOnGround        = flight.FirstIsOnGround,
                @lastIsOnGround         = flight.LastIsOnGround,
                @firstLat               = flight.FirstLat,
                @lastLat                = flight.LastLat,
                @firstLon               = flight.FirstLon,
                @lastLon                = flight.LastLon,
                @firstGroundSpeed       = flight.FirstGroundSpeed,
                @lastGroundSpeed        = flight.LastGroundSpeed,
                @firstAltitude          = flight.FirstAltitude,
                @lastAltitude           = flight.LastAltitude,
                @firstVerticalRate      = flight.FirstVerticalRate,
                @lastVerticalRate       = flight.LastVerticalRate,
                @firstTrack             = flight.FirstTrack,
                @lastTrack              = flight.LastTrack,
                @firstSquawk            = flight.FirstSquawk,
                @lastSquawk             = flight.LastSquawk,
                @hadAlert               = flight.HadAlert,
                @hadEmergency           = flight.HadEmergency,
                @hadSPI                 = flight.HadSpi,
                @flightID               = flight.FlightID,
            }, transaction: _TransactionHelper.Transaction);
        }

        private int Flights_GetCountByCriteria(BaseStationAircraft aircraft, SearchBaseStationCriteria criteria)
        {
            StringBuilder commandText = new StringBuilder();
            commandText.Append(CreateSearchBaseStationCriteriaSql(aircraft, criteria, justCount: true));
            var criteriaAndProperties = GetFlightsCriteria(aircraft, criteria);
            if(criteriaAndProperties.SqlChunk.Length > 0) commandText.AppendFormat(" WHERE {0}", criteriaAndProperties.SqlChunk);

            return (int)_Connection.ExecuteScalar<long>(commandText.ToString(), criteriaAndProperties.Parameters, transaction: _TransactionHelper.Transaction);
        }

        private List<BaseStationFlight> Flights_GetByCriteria(BaseStationAircraft aircraft, SearchBaseStationCriteria criteria, int fromRow, int toRow, string sort1, bool sort1Ascending, string sort2, bool sort2Ascending)
        {
            List<BaseStationFlight> result = null;

            sort1 = CriteriaSortFieldToColumnName(sort1);
            sort2 = CriteriaSortFieldToColumnName(sort2);

            StringBuilder commandText = new StringBuilder();
            commandText.Append(CreateSearchBaseStationCriteriaSql(aircraft, criteria, justCount: false));
            var criteriaAndProperties = GetFlightsCriteria(aircraft, criteria);
            if(criteriaAndProperties.SqlChunk.Length > 0) commandText.AppendFormat(" WHERE {0}", criteriaAndProperties.SqlChunk);
            if(sort1 != null || sort2 != null) {
                commandText.Append(" ORDER BY ");
                if(sort1 != null) commandText.AppendFormat("{0} {1}", sort1, sort1Ascending ? "ASC" : "DESC");
                if(sort2 != null) commandText.AppendFormat("{0}{1} {2}", sort1 == null ? "" : ", ", sort2, sort2Ascending ? "ASC" : "DESC");
            }

            commandText.Append(" LIMIT @limit OFFSET @offset");
            int limit = toRow == -1 || toRow < fromRow ? int.MaxValue : (toRow - Math.Max(0, fromRow)) + 1;
            int offset = fromRow < 0 ? 0 : fromRow;
            criteriaAndProperties.Parameters.Add("limit", limit);
            criteriaAndProperties.Parameters.Add("offset", offset);

            if(aircraft != null) {
                result = _Connection.Query<BaseStationFlight>(commandText.ToString(), criteriaAndProperties.Parameters, transaction: _TransactionHelper.Transaction).ToList();
                foreach(var flight in result) {
                    flight.Aircraft = aircraft;
                }
            } else {
                var aircraftInstances = new Dictionary<int, BaseStationAircraft>();
                Func<BaseStationAircraft, BaseStationAircraft> getAircraftInstance = (a) => {
                    BaseStationAircraft instance = null;
                    if(a != null) {
                        if(!aircraftInstances.TryGetValue(a.AircraftID, out instance)) {
                            instance = a;
                            aircraftInstances.Add(a.AircraftID, instance);
                        }
                    }
                    return instance;
                };

                // The results are always declared as flights then aircraft
                result = _Connection.Query<BaseStationFlight, BaseStationAircraft, BaseStationFlight>(
                    commandText.ToString(),
                    (f, a) => { f.Aircraft = getAircraftInstance(a); return f; },
                    criteriaAndProperties.Parameters,
                    transaction: _TransactionHelper.Transaction, splitOn: "AircraftID"
                ).ToList();
            }

            return result;
        }
        #endregion

        #region Flight searches by criteria
        /// <summary>
        /// Normalises strings in the criteria object.
        /// </summary>
        /// <remarks>
        /// Previous versions of VRS would implement case insensitivity in searches by using COLLATE NOCASE
        /// statements in the SQL. This had the unfortunate side-effect of producing very slow searches, so
        /// it has been removed. All searches are consequently case-sensitive but it is assumed that some
        /// fields are always written in upper case. This method converts the criteria for those fields to
        /// upper-case.
        /// </remarks>
        /// <param name="criteria"></param>
        public static void NormaliseCriteria(SearchBaseStationCriteria criteria)
        {
            if(criteria.Callsign != null)       criteria.Callsign.ToUpper();
            if(criteria.Icao != null)           criteria.Icao.ToUpper();
            if(criteria.Registration != null)   criteria.Registration.ToUpper();
            if(criteria.Type != null)           criteria.Type.ToUpper();
        }

        /// <summary>
        /// Builds a select statement from criteria.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="criteria"></param>
        /// <param name="justCount"></param>
        /// <returns></returns>
        private string CreateSearchBaseStationCriteriaSql(BaseStationAircraft aircraft, SearchBaseStationCriteria criteria, bool justCount)
        {
            StringBuilder result = new StringBuilder();

            if(aircraft != null) {
                result.AppendFormat("SELECT {0} FROM [Flights]", justCount ? "COUNT(*)" : "[Flights].*");
            } else {
                result.AppendFormat("SELECT {0}{1}{2} FROM ",
                        justCount ? "COUNT(*)" : "[Flights].*",
                        justCount ? "" : ", ",
                        justCount ? "" : "[Aircraft].*");

                if(FilterByAircraftFirst(criteria)) result.Append("[Aircraft] LEFT JOIN [Flights]");
                else                                result.Append("[Flights] LEFT JOIN [Aircraft]");

                result.Append(" ON ([Aircraft].[AircraftID] = [Flights].[AircraftID])");
            }

            return result.ToString();
        }

        /// <summary>
        /// Returns true if the criteria attempts to restrict the search to a single aircraft.
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private bool FilterByAircraftFirst(SearchBaseStationCriteria criteria)
        {
            return (criteria.Icao != null         && !String.IsNullOrEmpty(criteria.Icao.Value)) ||
                   (criteria.Registration != null && !String.IsNullOrEmpty(criteria.Registration.Value));
        }

        /// <summary>
        /// Returns the WHERE portion of an SQL statement contains the fields describing the criteria passed across.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private CriteriaAndProperties GetFlightsCriteria(BaseStationAircraft aircraft, SearchBaseStationCriteria criteria)
        {
            var result = new CriteriaAndProperties();
            StringBuilder command = new StringBuilder();

            if(aircraft != null) {
                DynamicSqlBuilder.AddWhereClause(command, "[Flights].[AircraftID]", " = @aircraftID");
                result.Parameters.Add("aircraftID", aircraft.AircraftID);
            }

            if(criteria.UseAlternateCallsigns && criteria.Callsign != null && criteria.Callsign.Condition == FilterCondition.Equals && !String.IsNullOrEmpty(criteria.Callsign.Value)) {
                GetAlternateCallsignCriteria(command, result.Parameters, criteria.Callsign, "[Flights].[Callsign]");
            } else {
                DynamicSqlBuilder.AddCriteria(command, criteria.Callsign, result.Parameters, "[Flights].[Callsign]", "callsign");
            }

            DynamicSqlBuilder.AddCriteria(command, criteria.Date,           result.Parameters, "[Flights].[StartTime]",         "fromStartTime", "toStartTime");
            DynamicSqlBuilder.AddCriteria(command, criteria.Operator,       result.Parameters, "[Aircraft].[RegisteredOwners]", "registeredOwners");
            DynamicSqlBuilder.AddCriteria(command, criteria.Registration,   result.Parameters, "[Aircraft].[Registration]",     "registration");
            DynamicSqlBuilder.AddCriteria(command, criteria.Icao,           result.Parameters, "[Aircraft].[ModeS]",            "icao");
            DynamicSqlBuilder.AddCriteria(command, criteria.Country,        result.Parameters, "[Aircraft].[ModeSCountry]",     "modeSCountry");
            DynamicSqlBuilder.AddCriteria(command, criteria.IsEmergency,                       "[Flights].[HadEmergency]");
            DynamicSqlBuilder.AddCriteria(command, criteria.Type,           result.Parameters, "[Aircraft].[ICAOTypeCode]",     "modelIcao");
            DynamicSqlBuilder.AddCriteria(command, criteria.FirstAltitude,  result.Parameters, "[Flights].[FirstAltitude]",     "fromFirstAltitude", "toFirstAltitude");
            DynamicSqlBuilder.AddCriteria(command, criteria.LastAltitude,   result.Parameters, "[Flights].[LastAltitude]",      "fromLastAltitude", "toLastAltitude");

            result.SqlChunk = command.ToString();
            return result;
        }

        /// <summary>
        /// Builds up the criteria and properties for all alternate callsigns.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        /// <param name="criteria"></param>
        /// <param name="callsignField"></param>
        private void GetAlternateCallsignCriteria(StringBuilder command, DynamicParameters parameters, FilterString criteria, string callsignField)
        {
            if(criteria != null && !String.IsNullOrEmpty(criteria.Value)) {
                if(_CallsignParser == null) _CallsignParser = Factory.Singleton.Resolve<ICallsignParser>();
                var alternates = _CallsignParser.GetAllAlternateCallsigns(criteria.Value);
                for(var i = 0;i < alternates.Count;++i) {
                    var isFirst = i == 0;
                    var isLast = i + 1 == alternates.Count;
                    var callsign = alternates[i];
                    var parameterName = String.Format("callsign{0}", i + 1);
                    DynamicSqlBuilder.AddWhereClause(command, callsignField,
                        String.Format(" {0} @{1}", !criteria.ReverseCondition ? "=" : "<>", parameterName),
                        useOR: !isFirst && !criteria.ReverseCondition,
                        openParenthesis: isFirst,
                        closeParenthesis: isLast);
                    parameters.Add(parameterName, callsign);
                }
            }
        }

        /// <summary>
        /// Translates from the sort field to an SQL field name.
        /// </summary>
        /// <param name="sortField"></param>
        /// <returns></returns>
        private string CriteriaSortFieldToColumnName(string sortField)
        {
            string result = null;
            if(sortField != null) {
                switch(sortField.ToUpperInvariant()) {
                    case "CALLSIGN":        result = "[Flights].[Callsign]"; break;
                    case "DATE":            result = "[Flights].[StartTime]"; break;
                    case "FIRSTALTITUDE":   result = "[Flights].[FirstAltitude]"; break;
                    case "LASTALTITUDE":    result = "[Flights].[LastAltitude]"; break;

                    case "COUNTRY":         result = "[Aircraft].[ModeSCountry]"; break;
                    case "MODEL":           result = "[Aircraft].[Type]"; break;
                    case "TYPE":            result = "[Aircraft].[ICAOTypeCode]"; break;
                    case "OPERATOR":        result = "[Aircraft].[RegisteredOwners]"; break;
                    case "REG":             result = "[Aircraft].[Registration]"; break;
                    case "ICAO":            result = "[Aircraft].[ModeS]"; break;
                }
            }

            return result;
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
                if(_Connection != null) result = DBHistory_GetAll();
                else                    result = new List<BaseStationDBHistory>();
            }

            return result;
        }

        private List<BaseStationDBHistory> DBHistory_GetAll()
        {
            return _Connection.Query<BaseStationDBHistory>("SELECT * FROM [DBHistory]", transaction: _TransactionHelper.Transaction).ToList();
        }

        private void DBHistory_Insert(BaseStationDBHistory record)
        {
            record.DBHistoryID = (int)_Connection.ExecuteScalar<long>(Commands.DBHistory_Insert, new {
                @timeStamp      = record.TimeStamp,
                @description    = record.Description,
            }, transaction: _TransactionHelper.Transaction);
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
                if(_Connection != null) result = DBInfo_GetAll().Last();
            }

            return result;
        }

        private BaseStationDBInfo[] DBInfo_GetAll()
        {
            return _Connection.Query<BaseStationDBInfo>("SELECT * FROM [DBInfo]", transaction: _TransactionHelper.Transaction).ToArray();
        }

        private void DBInfo_Insert(BaseStationDBInfo record)
        {
            _Connection.Execute(Commands.DBInfo_Insert, new {
                @originalVersion    = record.OriginalVersion,
                @currentVersion     = record.CurrentVersion,
            }, transaction: _TransactionHelper.Transaction);
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
                else                    result = SystemEvents_GetAll();
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
                if(_Connection != null) {
                    SystemEvents_Insert(systemEvent);
                }
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
                if(_Connection != null) {
                    SystemEvents_Update(systemEvent);
                }
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
                if(_Connection != null) {
                    SystemEvents_Delete(systemEvent);
                }
            }
        }

        private void SystemEvents_Delete(BaseStationSystemEvents systemEvent)
        {
            _Connection.Execute("DELETE FROM [SystemEvents] WHERE [SystemEventsID] = @systemEventsID", new {
                @systemEventsID = systemEvent.SystemEventsID,
            }, transaction: _TransactionHelper.Transaction);
        }

        private List<BaseStationSystemEvents> SystemEvents_GetAll()
        {
            return _Connection.Query<BaseStationSystemEvents>("SELECT * FROM [SystemEvents]", transaction: _TransactionHelper.Transaction).ToList();
        }

        private void SystemEvents_Insert(BaseStationSystemEvents systemEvent)
        {
            systemEvent.SystemEventsID = (int)_Connection.ExecuteScalar<long>(Commands.SystemEvents_Insert, new {
                @timeStamp  = systemEvent.TimeStamp,
                @app        = systemEvent.App,
                @msg        = systemEvent.Msg,
            }, transaction: _TransactionHelper.Transaction);
        }

        private void SystemEvents_Update(BaseStationSystemEvents systemEvent)
        {
            _Connection.Execute(Commands.SystemEvents_Update, new {
                @timeStamp      = systemEvent.TimeStamp,
                @app            = systemEvent.App,
                @msg            = systemEvent.Msg,
                @systemEventsID = systemEvent.SystemEventsID,
            }, transaction: _TransactionHelper.Transaction);
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
                if(_Connection != null) result = Locations_GetAll();
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
                if(_Connection != null) {
                    Locations_Insert(location);
                }
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
                if(_Connection != null) {
                    Locations_Update(location);
                }
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
                if(_Connection != null) {
                    Locations_Delete(location);
                }
            }
        }

        private void Locations_Delete(BaseStationLocation location)
        {
            _Connection.Execute("DELETE FROM [Locations] WHERE [LocationID] = @locationID", new {
                locationID = location.LocationID,
            }, transaction: _TransactionHelper.Transaction);
        }

        private BaseStationLocation[] Locations_GetAll()
        {
            return _Connection.Query<BaseStationLocation>("SELECT * FROM [Locations]", transaction: _TransactionHelper.Transaction).ToArray();
        }

        private void Locations_Insert(BaseStationLocation location)
        {
            location.LocationID = (int)_Connection.ExecuteScalar<long>(Commands.Locations_Insert, new {
                @locationName   = location.LocationName,
                @latitude       = location.Latitude,
                @longitude      = location.Longitude,
                @altitude       = location.Altitude,
            }, transaction: _TransactionHelper.Transaction);
        }

        private void Locations_Update(BaseStationLocation location)
        {
            _Connection.Execute(Commands.Locations_Update, new {
                @locationName   = location.LocationName,
                @latitude       = location.Latitude,
                @longitude      = location.Longitude,
                @altitude       = location.Altitude,
                @locationID     = location.LocationID,
            }, transaction: _TransactionHelper.Transaction);
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
                if(_Connection != null) result = Sessions_GetAll();
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
                if(_Connection != null) {
                    Sessions_Insert(session);
                }
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
                if(_Connection != null) {
                    Sessions_Update(session);
                }
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
                if(_Connection != null) {
                    Sessions_Delete(session);
                }
            }
        }

        private void Sessions_Delete(BaseStationSession session)
        {
            _Connection.Execute("DELETE FROM [Sessions] WHERE [SessionID] = @sessionID", new {
                @sessionID = session.SessionID,
            }, transaction: _TransactionHelper.Transaction);
        }

        private List<BaseStationSession> Sessions_GetAll()
        {
            return _Connection.Query<BaseStationSession>("SELECT * FROM [Sessions]", transaction: _TransactionHelper.Transaction).ToList();
        }

        private void Sessions_Insert(BaseStationSession session)
        {
            session.SessionID = (int)_Connection.ExecuteScalar<long>(Commands.Sessions_Insert, new {
                @locationID = session.LocationID,
                @startTime  = session.StartTime,
                @endTime    = session.EndTime,
            }, transaction: _TransactionHelper.Transaction);
        }

        private void Sessions_Update(BaseStationSession session)
        {
            _Connection.Execute(Commands.Sessions_Update, new {
                @locationID = session.LocationID,
                @startTime  = session.StartTime,
                @endTime    = session.EndTime,
                @sessionID  = session.SessionID,
            }, transaction: _TransactionHelper.Transaction);
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
