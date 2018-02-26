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
using VirtualRadar.Interface.StandingData;

namespace VirtualRadar.Database.BaseStation
{
    /// <summary>
    /// Default implementation of <see cref="IBaseStationDatabase"/>.
    /// </summary>
    public sealed class Database : IBaseStationDatabaseSQLite
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
        /// The transaction currently in force, if any.
        /// </summary>
        private IDbTransaction _Transaction;

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

        /// <summary>
        /// The object that handles the time for us.
        /// </summary>
        private IClock _Clock;

        /// <summary>
        /// The object that looks up code blocks for us.
        /// </summary>
        private IStandingDataManager _StandingDataManager;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IBaseStationDatabaseProvider Provider { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Engine => "SQLite";

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
            _StandingDataManager = Factory.ResolveSingleton<IStandingDataManager>();
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
                _ConfigurationStorage = Factory.ResolveSingleton<IConfigurationStorage>();
                _ConfigurationStorage.ConfigurationChanged += ConfigurationStorage_ConfigurationChanged;
            }

            if(_Clock == null) {
                _Clock = Factory.Resolve<IClock>();
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
                        var builder = Factory.Resolve<ISQLiteConnectionStringBuilder>().Initialise();
                        builder.DataSource = fileName;
                        builder.ReadOnly = !writeSupportEnabled.Value;
                        builder.FailIfMissing = true;
                        builder.DateTimeFormat = SQLiteDateFormats.ISO8601;
                        builder.JournalMode = SQLiteJournalModeEnum.Default;  // <-- Persist causes problems with SBSPopulate
                        var connection = Factory.Resolve<ISQLiteConnectionProvider>().Create(builder.ConnectionString);
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

        #region FileExists, FileIsEmpty
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public bool FileExists()
        {
            var result = !String.IsNullOrEmpty(FileName);

            if(result) {
                var fileSystem = Factory.Resolve<IFileSystemProvider>();
                result = fileSystem.FileExists(FileName);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public bool FileIsEmpty()
        {
            var fileSystem = Factory.Resolve<IFileSystemProvider>();
            return fileSystem.FileSize(FileName) == 0L;
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
                    var configuration = Factory.ResolveSingleton<IConfigurationStorage>().Load();

                    var folder = Path.GetDirectoryName(fileName);
                    if(!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    if(fileMissing) File.Create(fileName).Close();

                    CloseConnection();
                    OpenConnection(fileName, true);
                    try {
                        if(_Connection != null) {
                            _TransactionHelper.PerformInTransaction(_Connection, _Transaction != null, false, r => _Transaction = r, () => {
                                _Connection.Execute(Commands.UpdateSchema, transaction: _Transaction);

                                DBHistory_Insert(new BaseStationDBHistory() { Description = "Database autocreated by Virtual Radar Server", TimeStamp = SQLiteDateHelper.Truncate(Provider.UtcNow) });
                                DBInfo_Insert(new BaseStationDBInfo() { OriginalVersion = 2, CurrentVersion = 2 });
                                Locations_Insert(new BaseStationLocation() { LocationName = "Home", Latitude = configuration.GoogleMapSettings.InitialMapLatitude, Longitude = configuration.GoogleMapSettings.InitialMapLongitude });

                                return true;
                            });
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

            var sqliteException = Factory.Resolve<ISQLiteException>();
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

        #region PerformInTransaction
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool PerformInTransaction(Func<bool> action)
        {
            var result = false;

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) {
                    result = _TransactionHelper.PerformInTransaction(_Connection, _Transaction != null, false, r => _Transaction = r, action);
                }
            }

            return result;
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
        /// <param name="created"></param>
        /// <param name="createNewAircraftFunc"></param>
        /// <returns></returns>
        public BaseStationAircraft GetOrInsertAircraftByCode(string icao24, out bool created)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot insert aircraft when write support is disabled");
            }

            created = false;
            BaseStationAircraft result = null;
            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) {
                    result = Aircraft_GetByIcao(icao24);
                    if(result == null) {
                        var now = _Clock.LocalNow;
                        var codeBlock = _StandingDataManager.FindCodeBlock(icao24);
                        result = new BaseStationAircraft() {
                            AircraftID = 0,
                            ModeS = icao24,
                            FirstCreated = now,
                            LastModified = now,
                            ModeSCountry = codeBlock?.ModeSCountry,
                        };
                        Aircraft_Insert(result);
                        created = true;
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
        public void RecordMissingAircraft(string icao)
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
        public void RecordManyMissingAircraft(IEnumerable<string> icaos)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot record empty aircraft when write support is disabled");

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) {
                    var localNow = _Clock.LocalNow;
                    var allAircraft = Aircraft_GetByIcaos(icaos).ToDictionary(r => ParameterBuilder.NormaliseAircraftIcao(r.ModeS), r => r);

                    _TransactionHelper.PerformInTransaction(_Connection, _Transaction != null, false, r => _Transaction = r, () => {
                        foreach(var icao in icaos) {
                            allAircraft.TryGetValue(ParameterBuilder.NormaliseAircraftIcao(icao), out var aircraft);
                            RecordEmptyAircraft(aircraft, icao, localNow);
                        }
                        return true;
                    });
                }
            }
        }

        private void RecordEmptyAircraft(BaseStationAircraft aircraft, string icao, DateTime localNow)
        {
            const string missingMarker = "Missing";

            if(aircraft != null) {
                aircraft.LastModified = localNow;
                if(String.IsNullOrEmpty(aircraft.Registration) &&
                   String.IsNullOrEmpty(aircraft.Manufacturer) &&
                   String.IsNullOrEmpty(aircraft.Type) &&
                   String.IsNullOrEmpty(aircraft.RegisteredOwners)) {
                    aircraft.UserString1 = missingMarker;
                }
                Aircraft_Update(aircraft);
            } else {
                aircraft = new BaseStationAircraft() {
                    ModeS = icao,
                    UserString1 = missingMarker,
                    FirstCreated = localNow,
                    LastModified = localNow,
                };
                Aircraft_Insert(aircraft);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public BaseStationAircraft UpsertAircraft(BaseStationAircraftUpsertLookup aircraft)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot upsert aircraft when write support is disabled");

            BaseStationAircraft result = null;
            bool updated = false;

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) {
                    var localNow = _Clock.LocalNow;

                    result = Aircraft_GetByIcao(aircraft.ModeS);
                    result = UpsertAircraft(result, aircraft, FillFromUpsertLookup, ref updated);
                }
            }

            if(updated) {
                OnAircraftUpdated(new EventArgs<BaseStationAircraft>(result));
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="allUpsertAircraft"></param>
        /// <returns></returns>
        public BaseStationAircraft[] UpsertManyAircraft(IEnumerable<BaseStationAircraftUpsertLookup> allUpsertAircraft)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot upsert aircraft when write support is disabled");

            var result = new List<BaseStationAircraft>();
            var updated = new List<bool>();

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) {
                    var localNow = _Clock.LocalNow;
                    var icaos = allUpsertAircraft.Select(r => r.ModeS);
                    var allAircraft = Aircraft_GetByIcaos(icaos).ToDictionary(r => ParameterBuilder.NormaliseAircraftIcao(r.ModeS), r => r);

                    _TransactionHelper.PerformInTransaction(_Connection, _Transaction != null, false, r => _Transaction = r, () => {
                        foreach(var icao in icaos) {
                            var thisUpdated = false;
                            allAircraft.TryGetValue(ParameterBuilder.NormaliseAircraftIcao(icao), out var aircraft);
                            var upsertAircraft = allUpsertAircraft.First(r => r.ModeS == icao);
                            aircraft = UpsertAircraft(aircraft, upsertAircraft, FillFromUpsertLookup, ref thisUpdated);
                            if(aircraft != null) {
                                result.Add(aircraft);
                                updated.Add(thisUpdated);
                            }
                        }
                        return true;
                    });
                }
            }

            for(int i = 0;i < result.Count;++i) {
                if(updated[i]) OnAircraftUpdated(new EventArgs<BaseStationAircraft>(result[i]));
            }

            return result.ToArray();
        }

        public BaseStationAircraft[] UpsertManyAircraft(IEnumerable<BaseStationAircraftUpsert> allUpsertAircraft)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot upsert aircraft when write support is disabled");
            }

            var result = new List<BaseStationAircraft>();
            var updated = new List<bool>();

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) {
                    var localNow = _Clock.LocalNow;
                    var icaos = allUpsertAircraft.Select(r => r.ModeS);
                    var allAircraft = Aircraft_GetByIcaos(icaos).ToDictionary(r => ParameterBuilder.NormaliseAircraftIcao(r.ModeS), r => r);

                    _TransactionHelper.PerformInTransaction(_Connection, _Transaction != null, false, r => _Transaction = r, () => {
                        foreach(var icao in icaos) {
                            var thisUpdated = false;
                            allAircraft.TryGetValue(ParameterBuilder.NormaliseAircraftIcao(icao), out var aircraft);
                            var upsertAircraft = allUpsertAircraft.First(r => r.ModeS == icao);
                            aircraft = UpsertAircraft(aircraft, upsertAircraft, FillFromBaseStationAircraftUpsert, ref thisUpdated);
                            if(aircraft != null) {
                                result.Add(aircraft);
                                updated.Add(thisUpdated);
                            }
                        }
                        return true;
                    });
                }
            }

            for(var i = 0;i < result.Count;++i) {
                if(updated[i]) {
                    OnAircraftUpdated(new EventArgs<BaseStationAircraft>(result[i]));
                }
            }

            return result.ToArray();
        }

        private BaseStationAircraft FillFromUpsertLookup(BaseStationAircraft destination, BaseStationAircraftUpsertLookup source)
        {
            if(destination == null) {
                destination = new BaseStationAircraft() {
                    ModeS =         source.ModeS,
                    FirstCreated =  source.LastModified,
                };
            }
            destination.LastModified =     source.LastModified;
            destination.Registration =     source.Registration;
            destination.Country =          source.Country;
            destination.ModeSCountry =     source.ModeSCountry;
            destination.Manufacturer =     source.Manufacturer;
            destination.Type =             source.Type;
            destination.ICAOTypeCode =     source.ICAOTypeCode;
            destination.RegisteredOwners = source.RegisteredOwners;
            destination.OperatorFlagCode = source.OperatorFlagCode;
            destination.SerialNo =         source.SerialNo;
            destination.YearBuilt =        source.YearBuilt;
            destination.UserString1 =      destination.UserString1 == "Missing" ? null : destination.UserString1;

            return destination;
        }

        private BaseStationAircraft FillFromBaseStationAircraftUpsert(BaseStationAircraft destination, BaseStationAircraftUpsert source)
        {
            if(destination == null) {
                destination = new BaseStationAircraft();
            }
            source.ApplyTo(destination);

            return destination;
        }

        /// <summary>
        /// Does the work for the UpsertAircraftByCode methods.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="upsertAircraft"></param>
        /// <param name="fillAircraft"></param>
        /// <param name="updated"></param>
        /// <returns></returns>
        private BaseStationAircraft UpsertAircraft<T>(BaseStationAircraft aircraft, T upsertAircraft, Func<BaseStationAircraft, T, BaseStationAircraft> fillAircraft, ref bool updated)
        {
            var isNewAircraft = aircraft == null;
            updated = !isNewAircraft;

            aircraft = fillAircraft(aircraft, upsertAircraft);
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
            }, transaction: _Transaction);
        }

        BaseStationAircraft[] Aircraft_GetAll()
        {
            return _Connection.Query<BaseStationAircraft>("SELECT * FROM [Aircraft]", transaction: _Transaction).ToArray();
        }

        BaseStationAircraft Aircraft_GetByIcao(string icao)
        {
            return _Connection.Query<BaseStationAircraft>("SELECT * FROM [Aircraft] WHERE [ModeS] = @icao", new {
                @icao = icao,
            }, transaction: _Transaction).FirstOrDefault();
        }

        BaseStationAircraft[] Aircraft_GetByIcaos(IEnumerable<string> icaos)
        {
            var icaosArray = icaos.ToArray();
            var result = new List<BaseStationAircraft>();

            const int batchSize = 200;
            for(var i = 0;i < icaosArray.Length;i += batchSize) {
                var batchIcaos = icaosArray.Skip(i).Take(batchSize).ToArray();
                var batchResults = _Connection.Query<BaseStationAircraft>("SELECT * FROM [Aircraft] WHERE [ModeS] IN @icaos", new {
                    @icaos = batchIcaos,
                }, transaction: _Transaction).ToArray();
                result.AddRange(batchResults);
            }

            return result.ToArray();
        }

        BaseStationAircraft Aircraft_GetById(int aircraftId)
        {
            return _Connection.Query<BaseStationAircraft>("SELECT * FROM [Aircraft] WHERE [AircraftID] = @aircraftId", new {
                @aircraftId = aircraftId,
            }, transaction: _Transaction).FirstOrDefault();
        }

        BaseStationAircraft Aircraft_GetByRegistration(string registration)
        {
            return _Connection.Query<BaseStationAircraft>("SELECT * FROM [Aircraft] WHERE [Registration] = @registration", new {
                @registration = registration,
            }, transaction: _Transaction).FirstOrDefault();
        }

        void Aircraft_Insert(BaseStationAircraft aircraft)
        {
            aircraft.FirstCreated = SQLiteDateHelper.Truncate(aircraft.FirstCreated);
            aircraft.LastModified = SQLiteDateHelper.Truncate(aircraft.LastModified);

            aircraft.AircraftID = (int)_Connection.ExecuteScalar<long>(
                Commands.Aircraft_Insert,
                ParameterBuilder.FromAircraft(aircraft, includeAircraftID:false),
                transaction: _Transaction
            );
        }

        void Aircraft_Update(BaseStationAircraft aircraft)
        {
            aircraft.FirstCreated = SQLiteDateHelper.Truncate(aircraft.FirstCreated);
            aircraft.LastModified = SQLiteDateHelper.Truncate(aircraft.LastModified);

            _Connection.Execute(
                Commands.Aircraft_Update,
                ParameterBuilder.FromAircraft(aircraft),
                transaction: _Transaction
            );
        }

        void Aircraft_UpdateModeSCountry(int aircraftId, string modeSCountry)
        {
            _Connection.Execute("UPDATE [Aircraft] SET [ModeSCountry] = @modeSCountry WHERE [AircraftID] = @aircraftID", new {
                @aircraftID =   aircraftId,
                @modeSCountry = modeSCountry,
            }, transaction: _Transaction);
        }

        BaseStationAircraftAndFlightsCount[] AircraftAndFlightsCount_GetByIcaos(IEnumerable<string> icaos)
        {
            return _Connection.Query<BaseStationAircraftAndFlightsCount>(Commands.Aircraft_GetAircraftAndFlightsCountByIcao, new {
                @icaos = icaos,
            }, transaction: _Transaction).ToArray();
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
            }, transaction: _Transaction);
        }

        private BaseStationFlight Flights_GetById(int flightId)
        {
            return _Connection.Query<BaseStationFlight>("SELECT * FROM [Flights] WHERE [FlightID] = @flightID", new {
                @flightID = flightId,
            }, transaction: _Transaction).FirstOrDefault();
        }

        private BaseStationFlight[] Flights_GetAllForAircraft(int aircraftID)
        {
            return _Connection.Query<BaseStationFlight>("SELECT * FROM [Flights] WHERE [AircraftID] = @aircraftID", new {
                aircraftID = aircraftID,
            }, transaction: _Transaction).ToArray();
        }

        private void Flights_Insert(BaseStationFlight flight)
        {
            flight.FlightID = (int)_Connection.ExecuteScalar<long>(
                Commands.Flights_Insert,
                ParameterBuilder.FromFlight(flight, includeFlightID: false),
                transaction: _Transaction
            );
        }

        private void Flights_Update(BaseStationFlight flight)
        {
            _Connection.Execute(
                Commands.Flights_Update,
                ParameterBuilder.FromFlight(flight),
                transaction: _Transaction
            );
        }

        private int Flights_GetCountByCriteria(BaseStationAircraft aircraft, SearchBaseStationCriteria criteria)
        {
            StringBuilder commandText = new StringBuilder();
            commandText.Append(CreateSearchBaseStationCriteriaSql(aircraft, criteria, justCount: true));
            var criteriaAndProperties = DynamicSql.GetFlightsCriteria(aircraft, criteria);
            if(criteriaAndProperties.SqlChunk.Length > 0) commandText.AppendFormat(" WHERE {0}", criteriaAndProperties.SqlChunk);

            return (int)_Connection.ExecuteScalar<long>(commandText.ToString(), criteriaAndProperties.Parameters, transaction: _Transaction);
        }

        private List<BaseStationFlight> Flights_GetByCriteria(BaseStationAircraft aircraft, SearchBaseStationCriteria criteria, int fromRow, int toRow, string sort1, bool sort1Ascending, string sort2, bool sort2Ascending)
        {
            List<BaseStationFlight> result = null;

            sort1 = DynamicSql.CriteriaSortFieldToColumnName(sort1);
            sort2 = DynamicSql.CriteriaSortFieldToColumnName(sort2);

            StringBuilder commandText = new StringBuilder();
            commandText.Append(CreateSearchBaseStationCriteriaSql(aircraft, criteria, justCount: false));
            var criteriaAndProperties = DynamicSql.GetFlightsCriteria(aircraft, criteria);
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
                result = _Connection.Query<BaseStationFlight>(commandText.ToString(), criteriaAndProperties.Parameters, transaction: _Transaction).ToList();
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
                    transaction: _Transaction, splitOn: "AircraftID"
                ).ToList();
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="upsertFlights"></param>
        /// <returns></returns>
        public BaseStationFlight[] UpsertManyFlights(IEnumerable<BaseStationFlightUpsert> upsertFlights)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot delete flights when writes are disabled");
            }

            var upserted = new List<BaseStationFlight>();
            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) {
                    BaseStationAircraft aircraft = null;
                    BaseStationSession  session = null;
                    var aircraftFlights = new LinkedList<BaseStationFlight>();

                    foreach(var upsertFlight in upsertFlights.OrderBy(r => r.AircraftID)) {
                        if(aircraft?.AircraftID != upsertFlight.AircraftID) {
                            aircraft = Aircraft_GetById(upsertFlight.AircraftID);

                            aircraftFlights.Clear();
                            if(aircraft != null) {
                                foreach(var flight in Flights_GetAllForAircraft(aircraft.AircraftID).OrderBy(r => r.StartTime)) {
                                    aircraftFlights.AddFirst(flight);
                                }
                            }
                        }
                        if(session?.SessionID != upsertFlight.SessionID) {
                            session = Sessions_GetById(upsertFlight.SessionID);
                        }

                        if(aircraft != null && session != null) {
                            var flightNode = FindFlightWithStartTime(aircraftFlights, upsertFlight.StartTime, removeNode: true);
                            var dbFlight = flightNode?.Value;

                            if(dbFlight == null) {
                                dbFlight = upsertFlight.ToBaseStationFlight();
                                Flights_Insert(dbFlight);
                            } else {
                                upsertFlight.ApplyTo(dbFlight);
                                Flights_Update(dbFlight);
                            }

                            upserted.Add(dbFlight);
                        }
                    }
                }
            }

            return upserted.ToArray();
        }

        private LinkedListNode<BaseStationFlight> FindFlightWithStartTime(LinkedList<BaseStationFlight> orderedFlights, DateTime startTime, bool removeNode)
        {
            LinkedListNode<BaseStationFlight> result = null;

            for(var node = orderedFlights.First;node != null && node.Value.StartTime <= startTime;node = node.Next) {
                if(node.Value.StartTime == startTime) {
                    result = node;

                    if(removeNode) {
                        orderedFlights.Remove(result);
                    }

                    break;
                }
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

                if(criteria.FilterByAircraftFirst()) result.Append("[Aircraft] LEFT JOIN [Flights]");
                else                                 result.Append("[Flights] LEFT JOIN [Aircraft]");

                result.Append(" ON ([Aircraft].[AircraftID] = [Flights].[AircraftID])");
            }

            return result.ToString();
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
            return _Connection.Query<BaseStationDBHistory>("SELECT * FROM [DBHistory]", transaction: _Transaction).ToList();
        }

        private void DBHistory_Insert(BaseStationDBHistory record)
        {
            record.DBHistoryID = (int)_Connection.ExecuteScalar<long>(
                Commands.DBHistory_Insert,
                ParameterBuilder.FromDBHistory(record, includeHistoryID: false),
                transaction: _Transaction
            );
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
            return _Connection.Query<BaseStationDBInfo>("SELECT * FROM [DBInfo]", transaction: _Transaction).ToArray();
        }

        private void DBInfo_Insert(BaseStationDBInfo record)
        {
            _Connection.Execute(
                Commands.DBInfo_Insert,
                ParameterBuilder.FromDBInfo(record),
                transaction: _Transaction
            );
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
            }, transaction: _Transaction);
        }

        private List<BaseStationSystemEvents> SystemEvents_GetAll()
        {
            return _Connection.Query<BaseStationSystemEvents>("SELECT * FROM [SystemEvents]", transaction: _Transaction).ToList();
        }

        private void SystemEvents_Insert(BaseStationSystemEvents systemEvent)
        {
            systemEvent.SystemEventsID = (int)_Connection.ExecuteScalar<long>(
                Commands.SystemEvents_Insert,
                ParameterBuilder.FromSystemEvent(systemEvent, includeSystemEventID: false),
                transaction: _Transaction
            );
        }

        private void SystemEvents_Update(BaseStationSystemEvents systemEvent)
        {
            _Connection.Execute(
                Commands.SystemEvents_Update,
                ParameterBuilder.FromSystemEvent(systemEvent),
                transaction: _Transaction
            );
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
            }, transaction: _Transaction);
        }

        private BaseStationLocation[] Locations_GetAll()
        {
            return _Connection.Query<BaseStationLocation>("SELECT * FROM [Locations]", transaction: _Transaction).ToArray();
        }

        private void Locations_Insert(BaseStationLocation location)
        {
            location.LocationID = (int)_Connection.ExecuteScalar<long>(
                Commands.Locations_Insert,
                ParameterBuilder.FromLocation(location, includeLocationID: false),
                transaction: _Transaction
            );
        }

        private void Locations_Update(BaseStationLocation location)
        {
            _Connection.Execute(
                Commands.Locations_Update,
                ParameterBuilder.FromLocation(location),
                transaction: _Transaction
            );
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
            }, transaction: _Transaction);
        }

        private List<BaseStationSession> Sessions_GetAll()
        {
            return _Connection.Query<BaseStationSession>("SELECT * FROM [Sessions]", transaction: _Transaction).ToList();
        }

        private BaseStationSession Sessions_GetById(int sessionID)
        {
            return _Connection.Query<BaseStationSession>(
                "SELECT * FROM [Sessions] WHERE [SessionID] = @sessionID", new {
                    sessionID =  sessionID,
                }, transaction: _Transaction
            ).FirstOrDefault();
        }

        private void Sessions_Insert(BaseStationSession session)
        {
            session.SessionID = (int)_Connection.ExecuteScalar<long>(
                Commands.Sessions_Insert,
                ParameterBuilder.FromSession(session, includeSessionID: false),
                transaction: _Transaction
            );
        }

        private void Sessions_Update(BaseStationSession session)
        {
            _Connection.Execute(
                Commands.Sessions_Update,
                ParameterBuilder.FromSession(session),
                transaction: _Transaction
            );
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
            var configuration = Factory.ResolveSingleton<IConfigurationStorage>().Load();
            if(configuration.BaseStationSettings.DatabaseFileName != FileName) {
                OnFileNameChanging(EventArgs.Empty);
                OnFileNameChanged(EventArgs.Empty);
            }
        }
        #endregion
    }
}
