// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using VirtualRadar.Interface;
using VirtualRadar.Interface.KineticData;
using VirtualRadar.Interface.Options;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;

namespace VirtualRadar.Database.SQLite.KineticData
{
    /// <summary>
    /// Default implementation of <see cref="IBaseStationDatabase"/>.
    /// </summary>
    public sealed class Database : IBaseStationDatabaseSQLite
    {
        private readonly IFileSystem _FileSystem;
        private readonly ISharedConfiguration _SharedConfiguration;
        private readonly IClock _Clock;
        private readonly IStandingDataManager _StandingDataManager;
        private readonly BaseStationDatabaseOptions _BaseStationDatabaseOptions;

        /// <summary>
        /// The value in UserString1 that indicates that the record is for an aircraft that could not be found by the online lookup.
        /// </summary>
        const string _MissingMarkerInUserString1 = "Missing";

        /// <summary>
        /// The object that we synchronise threads on.
        /// </summary>
        private readonly object _ConnectionLock = new();

        /// <summary>
        /// True if the object has been disposed.
        /// </summary>
        private bool _Disposed;

        /// <summary>
        /// The connection to the database. This is null if the connection settings have not
        /// been configured, the BaseStation.sqb file does not exist etc.
        /// </summary>
        private BaseStationContext _Context;

        /// <summary>
        /// The object that handles nested transactions for us.
        /// </summary>
        private TransactionHelper _TransactionHelper;

        /// <inheritdoc/>
        public string Engine => "SQLite";

        /// <inheritdoc/>
        public bool IsConnected => _Context != null;

        private bool _WriteSupportEnabled;
        /// <inheritdoc/>
        public bool WriteSupportEnabled
        {
            get => _WriteSupportEnabled;
            set {
                if(_WriteSupportEnabled != value) {
                    _WriteSupportEnabled = value;
                    CloseConnection();
                }
            }
        }

        /// <inheritdoc/>
        public int MaxParameters
        {
            get {
                lock(_ConnectionLock) {
                    OpenConnection();
                    return IsConnected ? 100 : -1;
                }
            }
        }

        /// <inheritdoc/>
        public event EventHandler<EventArgs<KineticAircraft>> AircraftUpdated;

        /// <summary>
        /// Raises <see cref="AircraftUpdated"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnAircraftUpdated(EventArgs<KineticAircraft> args) => EventHelper.Raise(AircraftUpdated, this, args);

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="fileSystem"></param>
        /// <param name="sharedConfiguration"></param>
        /// <param name="clock"></param>
        /// <param name="standingDataManager"></param>
        /// <param name="baseStationDatabaseOptions"></param>
        public Database(
            IFileSystem fileSystem,
            ISharedConfiguration sharedConfiguration,
            IClock clock,
            IStandingDataManager standingDataManager,
            IOptions<BaseStationDatabaseOptions> baseStationDatabaseOptions
        )
        {
            _FileSystem = fileSystem;
            _SharedConfiguration = sharedConfiguration;
            _Clock = clock;
            _StandingDataManager = standingDataManager;
            _BaseStationDatabaseOptions = baseStationDatabaseOptions.Value;

            _SharedConfiguration.ConfigurationChanged += SharedConfiguration_ConfigurationChanged;

            _TransactionHelper = new TransactionHelper(
                beginCallback: null,
                commitCallback: () => _Context.SaveChanges(),
                rollbackCallback: () => CloseConnection()
            );
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~Database() => Dispose(false);

        /// <inheritdoc/>
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
                }
            }
        }

        /// <summary>
        /// Closes the connection and disposes of it if open.
        /// </summary>
        private void CloseConnection()
        {
            lock(_ConnectionLock) {
                if(_Context != null) {
                    _Context.Dispose();
                    _Context = null;
                }
            }
        }

        /// <summary>
        /// Opens the connection if not already open.
        /// </summary>
        private void OpenConnection(string fileName = null, bool? writeSupportEnabled = null)
        {
            lock(_ConnectionLock) {
                if(_Context == null && !_Disposed) {
                    bool inCreateMode = fileName != null
                                     && writeSupportEnabled.GetValueOrDefault();

                    var config = _SharedConfiguration.Get();

                    fileName ??= config.BaseStationSettings.DatabaseFileName;
                    writeSupportEnabled ??= WriteSupportEnabled;

                    bool fileExists = _FileSystem.FileExists(fileName);
                    bool zeroLength = fileExists && _FileSystem.FileSize(fileName) == 0;

                    if(!String.IsNullOrEmpty(fileName) && fileExists && (inCreateMode || !zeroLength)) {
                        _Context = new BaseStationContext(fileName, writeSupportEnabled.Value, _BaseStationDatabaseOptions);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public bool TestConnection()
        {
            bool result = false;
            lock(_ConnectionLock) {
                OpenConnection();
                result = _Context != null;
            }

            return result;
        }

        /// <inheritdoc/>
        public bool FileExists()
        {
            var config = _SharedConfiguration.Get();
            var fileName = config.BaseStationSettings.DatabaseFileName;

            var result = !String.IsNullOrEmpty(fileName);
            if(result) {
                result = _FileSystem.FileExists(fileName);
            }

            return result;
        }

        /// <inheritdoc/>
        public bool FileIsEmpty()
        {
            var config = _SharedConfiguration.Get();
            return _FileSystem.FileSize(config.BaseStationSettings.DatabaseFileName) == 0L;
        }

        /// <inheritdoc/>
        public void CreateDatabaseIfMissing(string fileName)
        {
            if(!String.IsNullOrEmpty(fileName)) {
                bool fileMissing = !_FileSystem.FileExists(fileName);
                bool fileEmpty = fileMissing || _FileSystem.FileSize(fileName) == 0;
                if(fileMissing || fileEmpty) {
                    var folder = _FileSystem.GetDirectory(fileName);
                    _FileSystem.CreateDirectoryIfNotExists(folder);

                    var config = _SharedConfiguration.Get();

                    CloseConnection();
                    OpenConnection(fileName, writeSupportEnabled: true);
                    try {
                        if(_Context != null) {
                            _Context.Database.EnsureCreated();
                            _Context.Database.ExecuteSql(FormattableStringFactory.Create(
                                BaseStationSqbScripts.UpdateSchema
                            ));
                            _Context.DBHistories.Add(new() {
                                Description = "Database autocreated by Virtual Radar Server",
                                TimeStamp = _Clock.UtcNow,
                            });
                            _Context.DBInfos.Add(new() {
                                OriginalVersion = 2,
                                CurrentVersion = 2,
                            });
                            _Context.Locations.Add(new() {
                                LocationName = "Home",
                                Latitude = config.GoogleMapSettings.InitialMapLatitude, 
                                Longitude = config.GoogleMapSettings.InitialMapLongitude,
                            });
                            _Context.SaveChanges();
                        }
                    } finally {
                        CloseConnection();
                    }
                }
            }
        }

        /// <inheritdoc/>
        public bool AttemptAutoFix(Exception errorException)
        {
            bool result = false;

            if(errorException is SqliteException sqliteException) {
                switch((SqliteErrorCode)sqliteException.SqliteErrorCode) {
                    case SqliteErrorCode.SQLITE_IOERR:
                        // One of two things. Either the file is trashed, in which case we need to extract everything
                        // and rebuild the index, or they've had something writing to the file and the journal is
                        // hanging around after a crash. We're going to try to fix the journal problem as that's the
                        // most common for now.

                        var fileName = _SharedConfiguration.Get().BaseStationSettings.DatabaseFileName;
                        var journalFileName = String.Format("{0}-journal", fileName);
                        if(_FileSystem.FileExists(journalFileName)) {
                            // The easiest way to fix this is to open the file in read-write mode. If that fails then
                            // we should get rid of the journal file.
                            result = FixByOpeningInReadWriteMode();
                            if(!result || _FileSystem.FileExists(journalFileName)) {
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
            var result = false;

            string newFileName = null;
            for(var i = 1;i < 1000 && newFileName == null;++i) {
                newFileName = String.Format("{0} (bad){1}", journalFileName, i == 1 ? "" : String.Format(" ({0})", i));
                if(_FileSystem.FileExists(newFileName)) {
                    newFileName = null;
                }
            }

            if(newFileName != null) {
                try {
                    _FileSystem.MoveFile(journalFileName, newFileName, overwrite: false);
                    result = true;
                } catch {
                    result = false;
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public bool PerformInTransaction(Func<bool> action)
        {
            var result = false;

            lock(_ConnectionLock) {
                OpenConnection();
                if(IsConnected) {
                    result = _TransactionHelper.PerformInTransaction(action);
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public KineticAircraft GetAircraftByRegistration(string registration)
        {
            KineticAircraft result = null;

            lock(_ConnectionLock) {
                OpenConnection();
                if(IsConnected) {
                    result = Aircraft_GetByRegistration(registration);
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public KineticAircraft GetAircraftByCode(string icao24)
        {
            KineticAircraft result = null;

            lock(_ConnectionLock) {
                OpenConnection();
                if(IsConnected) {
                    result = Aircraft_GetByIcao(icao24);
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public List<KineticAircraft> GetAllAircraft()
        {
            var result = new List<KineticAircraft>();

            lock(_ConnectionLock) {
                OpenConnection();
                if(IsConnected) {
                    result.AddRange(Aircraft_GetAll());
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public Dictionary<string, KineticAircraft> GetManyAircraftByCode(IEnumerable<string> icao24s)
        {
            var result = new Dictionary<string, KineticAircraft>();

            if(icao24s != null && icao24s.Any()) {
                lock(_ConnectionLock) {
                    OpenConnection();
                    if(IsConnected) {
                        var maxParameters = MaxParameters;
                        var offset = 0;
                        var countIcao24s = icao24s.Count();

                        do {
                            var chunk = icao24s.Skip(offset).Take(maxParameters).ToArray();
                            foreach(var aircraft in Aircraft_GetByIcaos(chunk)) {
                                if(aircraft.ModeS != null && !result.ContainsKey(aircraft.ModeS)) {
                                    result.Add(aircraft.ModeS, aircraft);
                                }
                            }
                            offset += maxParameters;
                        } while(offset < countIcao24s);
                    }
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public Dictionary<string, KineticAircraftAndFlightsCount> GetManyAircraftAndFlightsCountByCode(IEnumerable<string> icao24s)
        {
            var result = new Dictionary<string, KineticAircraftAndFlightsCount>();

            if(icao24s != null && icao24s.Any()) {
                lock(_ConnectionLock) {
                    OpenConnection();
                    if(IsConnected) {
                        var maxParameters = MaxParameters;
                        var offset = 0;
                        var countIcao24s = icao24s.Count();

                        do {
                            var chunk = icao24s.Skip(offset).Take(maxParameters).ToArray();
                            foreach(var aircraft in AircraftAndFlightsCount_GetByIcaos(chunk)) {
                                if(aircraft.ModeS != null && !result.ContainsKey(aircraft.ModeS)) {
                                    result.Add(aircraft.ModeS, aircraft);
                                }
                            }
                            offset += maxParameters;
                        } while(offset < countIcao24s);
                    }
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public KineticAircraft GetAircraftById(int id)
        {
            KineticAircraft result = null;

            lock(_ConnectionLock) {
                OpenConnection();
                if(IsConnected) {
                    result = Aircraft_GetById(id);
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public void InsertAircraft(KineticAircraft aircraft)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot insert aircraft when write support is disabled");
            }

            PerformInTransaction(() => {
                Aircraft_Insert(aircraft);
                return true;
            });
        }

        /// <inheritdoc/>
        public KineticAircraft GetOrInsertAircraftByCode(string icao24, out bool created)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot insert aircraft when write support is disabled");
            }

            created = false;
            var innerCreated = created;
            KineticAircraft result = null;
            PerformInTransaction(() => {
                result = Aircraft_GetByIcao(icao24);
                if(result == null) {
                    var now = _Clock.Now.LocalDateTime;
                    var codeBlock = _StandingDataManager.FindCodeBlock(icao24);
                    result = new KineticAircraft() {
                        AircraftID = 0,
                        ModeS = icao24,
                        FirstCreated = now,
                        LastModified = now,
                        ModeSCountry = codeBlock?.ModeSCountry,
                    };
                    Aircraft_Insert(result);
                    innerCreated = true;
                }
                return true;
            });
            created = innerCreated;

            return result;
        }

        /// <inheritdoc/>
        public void UpdateAircraft(KineticAircraft aircraft)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot update aircraft when write support is disabled");
            }

            PerformInTransaction(() => {
                Aircraft_Update(aircraft);
                return true;
            });

            OnAircraftUpdated(new(aircraft));
        }

        /// <inheritdoc/>
        public void UpdateAircraftModeSCountry(int aircraftId, string modeSCountry)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot update the Mode-S country for an aircraft when write support is disabled");
            }

            PerformInTransaction(() => {
                Aircraft_UpdateModeSCountry(aircraftId, modeSCountry);
                return true;
            });
        }

        /// <inheritdoc/>
        public void RecordMissingAircraft(string icao)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot record empty aircraft when write support is disabled");
            }

            PerformInTransaction(() => {
                var localNow = _Clock.Now.LocalDateTime;

                var aircraft = Aircraft_GetByIcao(icao);
                RecordEmptyAircraft(aircraft, icao, localNow);

                return true;
            });
        }

        /// <inheritdoc/>
        public void RecordManyMissingAircraft(IEnumerable<string> icaos)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot record empty aircraft when write support is disabled");
            }

            var localNow = _Clock.Now.LocalDateTime;
            var allAircraft = Aircraft_GetByIcaos(icaos).ToDictionary(r => ParameterBuilder.NormaliseAircraftIcao(r.ModeS), r => r);

            PerformInTransaction(() => {
                foreach(var icao in icaos) {
                    allAircraft.TryGetValue(ParameterBuilder.NormaliseAircraftIcao(icao), out var aircraft);
                    RecordEmptyAircraft(aircraft, icao, localNow);
                }
                return true;
            });
        }

        private void RecordEmptyAircraft(KineticAircraft aircraft, string icao, DateTime localNow)
        {
            if(aircraft != null) {
                aircraft.LastModified = localNow;
                if(String.IsNullOrEmpty(aircraft.Registration) &&
                   String.IsNullOrEmpty(aircraft.Manufacturer) &&
                   String.IsNullOrEmpty(aircraft.Type) &&
                   String.IsNullOrEmpty(aircraft.RegisteredOwners)) {
                    aircraft.UserString1 = _MissingMarkerInUserString1;
                }
                Aircraft_Update(aircraft);
            } else {
                aircraft = new KineticAircraft() {
                    ModeS = icao,
                    UserString1 = _MissingMarkerInUserString1,
                    FirstCreated = localNow,
                    LastModified = localNow,
                };
                Aircraft_Insert(aircraft);
            }
        }

        /// <inheritdoc/>
        public KineticAircraft UpsertAircraftLookup(KineticAircraftLookup aircraft, bool onlyUpdateIfMarkedAsMissing)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot upsert aircraft when write support is disabled");
            }

            KineticAircraft result = null;
            bool updated = false;

            PerformInTransaction(() => {
                var localNow = _Clock.Now.LocalDateTime;

                result = Aircraft_GetByIcao(aircraft.ModeS);
                if(result == null || CanUpdateAircraftLookup(result, onlyUpdateIfMarkedAsMissing)) {
                    result = UpsertAircraft(result, aircraft, FillFromUpsertLookup, ref updated);
                }

                return true;
            });

            if(updated) {
                OnAircraftUpdated(new(result));
            }

            return result;
        }

        /// <inheritdoc/>
        /// <returns></returns>
        public KineticAircraft[] UpsertManyAircraftLookup(IEnumerable<KineticAircraftLookup> allUpsertAircraft, bool onlyUpdateIfMarkedAsMissing)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot upsert aircraft when write support is disabled");
            }

            var result = new List<KineticAircraft>();
            var updated = new List<bool>();

            var localNow = _Clock.Now.LocalDateTime;
            var icaos = allUpsertAircraft.Select(r => r.ModeS);
            var allAircraft = Aircraft_GetByIcaos(icaos).ToDictionary(r => ParameterBuilder.NormaliseAircraftIcao(r.ModeS), r => r);

            PerformInTransaction(() => {
                foreach(var icao in icaos) {
                    var thisUpdated = false;
                    allAircraft.TryGetValue(ParameterBuilder.NormaliseAircraftIcao(icao), out var aircraft);
                    if(aircraft == null || CanUpdateAircraftLookup(aircraft, onlyUpdateIfMarkedAsMissing)) {
                        var upsertAircraft = allUpsertAircraft.First(r => r.ModeS == icao);
                        aircraft = UpsertAircraft(aircraft, upsertAircraft, FillFromUpsertLookup, ref thisUpdated);
                        if(aircraft != null) {
                            result.Add(aircraft);
                            updated.Add(thisUpdated);
                        }
                    }
                }
                return true;
            });

            for(int i = 0;i < result.Count;++i) {
                if(updated[i]) {
                    OnAircraftUpdated(new(result[i]));
                }
            }

            return result.ToArray();
        }

        private static bool CanUpdateAircraftLookup(KineticAircraft result, bool onlyUpdateIfMarkedAsMissing)
        {
            return result == null
                || !onlyUpdateIfMarkedAsMissing
                || (
                       result.UserString1 == _MissingMarkerInUserString1
                    && String.IsNullOrEmpty(result.Registration?.Trim())
                   );
        }

        public KineticAircraft[] UpsertManyAircraft(IEnumerable<KineticAircraftKeyless> allUpsertAircraft)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot upsert aircraft when write support is disabled");
            }

            var result = new List<KineticAircraft>();
            var updated = new List<bool>();

            var localNow = _Clock.Now.LocalDateTime;
            var icaos = allUpsertAircraft.Select(r => r.ModeS);
            var allAircraft = Aircraft_GetByIcaos(icaos)
                .ToDictionary(r => ParameterBuilder.NormaliseAircraftIcao(r.ModeS), r => r);

            PerformInTransaction(() => {
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

            for(var i = 0;i < result.Count;++i) {
                if(updated[i]) {
                    OnAircraftUpdated(new(result[i]));
                }
            }

            return result.ToArray();
        }

        private KineticAircraft FillFromUpsertLookup(KineticAircraft destination, KineticAircraftLookup source)
        {
            if(destination == null) {
                destination = new KineticAircraft() {
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

        private KineticAircraft FillFromBaseStationAircraftUpsert(KineticAircraft destination, KineticAircraftKeyless source)
        {
            destination ??= new();
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
        private KineticAircraft UpsertAircraft<T>(KineticAircraft aircraft, T upsertAircraft, Func<KineticAircraft, T, KineticAircraft> fillAircraft, ref bool updated)
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

        /// <inheritdoc/>
        public void DeleteAircraft(KineticAircraft aircraft)
        {
            if(!WriteSupportEnabled) {
                throw new InvalidOperationException("You cannot delete aircraft when write support is disabled");
            }

            PerformInTransaction(() => {
                Aircraft_Delete(aircraft);
                return true;
            });
        }

        void Aircraft_Delete(KineticAircraft aircraft) => _Context.Aircraft.Remove(aircraft);

        KineticAircraft[] Aircraft_GetAll() => _Context.Aircraft.ToArray();

        KineticAircraft Aircraft_GetByIcao(string icao)
        {
            icao = ParameterBuilder.NormaliseAircraftIcao(icao);
            return _Context.Aircraft.FirstOrDefault(r => r.ModeS == icao);
        }

        KineticAircraft[] Aircraft_GetByIcaos(IEnumerable<string> icaos)
        {
            var icaosArray = icaos.Select(r => ParameterBuilder.NormaliseAircraftIcao(r)).ToArray();
            var result = new List<KineticAircraft>();

            const int batchSize = 100;
            for(var i = 0;i < icaosArray.Length;i += batchSize) {
                var batchIcaos = icaosArray.Skip(i).Take(batchSize).ToArray();
                var batchResults = _Context.Aircraft
                    .Where(r => batchIcaos.Contains(r.ModeS));
                result.AddRange(batchResults);
            }

            return result.ToArray();
        }

        KineticAircraft Aircraft_GetById(int aircraftId) => _Context.Aircraft.FirstOrDefault(r => r.AircraftID == aircraftId);

        KineticAircraft Aircraft_GetByRegistration(string registration) => _Context.Aircraft.FirstOrDefault(r => r.RegisteredOwners == registration);

        void Aircraft_Insert(KineticAircraft aircraft) => _Context.Aircraft.Add(aircraft);

        void Aircraft_Update(KineticAircraft aircraft)
        {
            // nop
        }

        void Aircraft_UpdateModeSCountry(int aircraftId, string modeSCountry)
        {
            var aircraft = Aircraft_GetById(aircraftId);
            if(aircraft != null) {
                aircraft.ModeSCountry = modeSCountry;
            }
        }

        KineticAircraftAndFlightsCount[] AircraftAndFlightsCount_GetByIcaos(IEnumerable<string> icaos)
        {
            var icaosArray = icaos.Select(r => ParameterBuilder.NormaliseAircraftIcao(r)).ToArray();
            var result = new List<KineticAircraftAndFlightsCount>();

            const int batchSize = 100;
            for(var i = 0;i < icaosArray.Length;i += batchSize) {
                var batchIcaos = icaosArray.Skip(i).Take(batchSize).ToArray();
                var batchResults = _Context.Aircraft
                    .Where(r => batchIcaos.Contains(r.ModeS))
                    .Select(r => new {
                        Aircraft = r,
                        CountFlights = _Context.Flights.Count(f => f.AircraftID == r.AircraftID),
                    })
                    .Select(r => new KineticAircraftAndFlightsCount(r.Aircraft, r.CountFlights));
                result.AddRange(batchResults);
            }

            return result.ToArray();
        }
        /*
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
            if(criteria.Callsign != null)       criteria.Callsign.ToUpperInvariant();
            if(criteria.Icao != null)           criteria.Icao.ToUpperInvariant();
            if(criteria.Registration != null)   criteria.Registration.ToUpperInvariant();
            if(criteria.Type != null)           criteria.Type.ToUpperInvariant();
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
        */

        private void SharedConfiguration_ConfigurationChanged(object sender, EventArgs args)
        {
            CloseConnection();
        }
    }
}
