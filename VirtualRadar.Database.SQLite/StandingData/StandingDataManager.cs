// Copyright © 2013 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Data;
using System.Globalization;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Options;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Localisation;

namespace VirtualRadar.Database.SQLite.StandingData
{
    /// <summary>
    /// The SQLite implementation of <see cref="IStandingDataManager"/>.
    /// </summary>
    [Obsolete("Do not create instances of this directly. Use dependency injection instead. This is only public so that it can be unit tested")]
    public class StandingDataManager : IStandingDataManager
    {
        /// <summary>
        /// The environment options that the class was started with.
        /// </summary>
        private EnvironmentOptions _EnvironmentOptions;

        /// <summary>
        /// The manager options that the class was started with.
        /// </summary>
        private StandingDataManagerOptions _StandingDataManagerOptions;

        /// <summary>
        /// The object that handles access to the file system for us.
        /// </summary>
        private IFileSystem _FileSystem;

        /// <summary>
        /// The log.
        /// </summary>
        private ILog _Log;

        /// <summary>
        /// The lock object.
        /// </summary>
        private object _SyncLock = new();

        /// <summary>
        /// The full path and filename of the standing data database file.
        /// </summary>
        private string _DatabaseFileName;

        /// <summary>
        /// The full path and filename of the route state file.
        /// </summary>
        private string _StateFileName;

        /// <summary>
        /// True if the files exist and appear to be correct.
        /// </summary>
        private bool _FilesValid;

        /// <summary>
        /// The version number of the database.
        /// </summary>
        private int _DatabaseVersion;

        /// <summary>
        /// A copy of every code block stored on disk.
        /// </summary>
        private List<CodeBlockBitMask> _CodeBlockCache = new List<CodeBlockBitMask>();

        /// <summary>
        /// The object that is used to lock _CodeBlockCache while it's being loaded.
        /// </summary>
        private object _CodeBlockCacheLock = new object();

        /// <summary>
        /// A list of every fake ground vehicle model code;
        /// </summary>
        private string[] _FakeGroundVehicleCodes;

        /// <summary>
        /// A list of every fake tower model code.
        /// </summary>
        private string[] _FakeTowerCodes;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string RouteStatus { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool CodeBlocksLoaded { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler LoadCompleted;

        /// <summary>
        /// Raises <see cref="LoadCompleted"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnLoadCompleted(EventArgs args)
        {
            EventHelper.Raise(LoadCompleted, this, args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public StandingDataManager(
            IOptions<EnvironmentOptions> environmentOptions,
            IOptions<StandingDataManagerOptions> standingDataManagerOptions,
            IFileSystem fileSystem,
            ILog log
        )
        {
            _EnvironmentOptions = environmentOptions.Value;
            _StandingDataManagerOptions = standingDataManagerOptions.Value;
            _FileSystem = fileSystem;
            _Log = log;

            RouteStatus = Strings.NotLoaded;

            var workingFolder = _EnvironmentOptions.WorkingFolder;
            _DatabaseFileName = Path.Combine(workingFolder, "StandingData.sqb");
            _StateFileName = Path.Combine(workingFolder, "FlightNumberCoverage.csv");
        }

        private StandingDataContext CreateContext()
        {
            var result = new StandingDataContext(_FileSystem, _EnvironmentOptions, _StandingDataManagerOptions);
            result.Database.ExecuteSql($"PRAGMA journal_mode = off");

            return result;
        }

        /// <inheritdoc/>
        public void Lock(Action<IStandingDataManager> action)
        {
            if(action != null) {
                lock(_SyncLock) {
                    action(this);
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Load()
        {
            Lock(_ => {
                _FilesValid = SetRouteStatus();
                _DatabaseVersion = GetDatabaseVersionNumber();
                CacheCodeBlocks();
                LoadOverrides();
            });
            OnLoadCompleted(EventArgs.Empty);
        }

        /// <summary>
        /// Updates <see cref="RouteStatus"/> to show the current state of the routes.
        /// </summary>
        private bool SetRouteStatus()
        {
            var result = false;

            if(   !_FileSystem.FileExists(_DatabaseFileName)
               || !_FileSystem.FileExists(_StateFileName)
            ) {
                RouteStatus = Strings.SomeRouteFilesMissing;
            } else {
                var lines = _FileSystem.ReadAllLines(_StateFileName);
                if(lines.Length < 2) {
                    RouteStatus = Strings.RouteStateFileInvalid;
                } else {
                    var chunks = lines[1].Split(new char[] { ',' });
                    if(chunks.Length < 3) {
                        RouteStatus = Strings.RouteStateFileContentInvalid;
                    } else {
                        if(!DateTime.TryParseExact(chunks[0], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate)
                           || !DateTime.TryParseExact(chunks[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate)
                           || !int.TryParse(chunks[2], out var countRoutes)
                        ) {
                            RouteStatus = Strings.CannotParseRouteFile;
                        } else {
                            RouteStatus = String.Format(Strings.RouteFileStatus, countRoutes, startDate, endDate);
                            result = true;
                        }
                    }
                }
            }

            return result;
        }

        private int GetDatabaseVersionNumber()
        {
            var result = -1;

            using(var context = CreateContext()) {
                result = context.DatabaseVersions
                    .First()
                    .Version;
            }

            return result;
        }


        private void CacheCodeBlocks()
        {
            var newCache = new List<CodeBlockBitMask>();

            if(_FilesValid) {
                using(var context = CreateContext()) {
                    var allCodeBlocks = context.CodeBlocks
                        .Include(codeBlock => codeBlock.Country)
                        .AsNoTracking()
                    ;

                    foreach(var codeBlock in allCodeBlocks) {
                        newCache.Add(new() {
                            CodeBlock = new() {
                                Country =           codeBlock.Country?.Name ?? Strings.Unknown,
                                IsMilitary =        codeBlock.IsMilitary,
                            },
                            BitMask =               codeBlock.BitMask,
                            SignificantBitMask =    codeBlock.SignificantBitMask,
                        });
                    }
                }

                // Sort into descending order of significant bitmask
                newCache.Sort((CodeBlockBitMask lhs, CodeBlockBitMask rhs) => -(lhs.SignificantBitMask - rhs.SignificantBitMask));
            }

            lock(_CodeBlockCacheLock) {
                _CodeBlockCache = newCache;
                CodeBlocksLoaded = _CodeBlockCache.Count > 0;
            }
        }

        private void LoadOverrides()
        {
            var codeBlocksFileName = _FileSystem.Combine(_EnvironmentOptions.WorkingFolder, "LocalAircraft.txt");
            if(_FileSystem.FileExists(codeBlocksFileName)) {
                LoadCodeBlocksOverrides(codeBlocksFileName);
            }

            var fakeModelCodesFileName = Path.Combine(_EnvironmentOptions.WorkingFolder, "FakeModelCodes.txt");
            LocalFakeModelCodesOverrides(fakeModelCodesFileName);
        }

        private void LoadCodeBlocksOverrides(string fileName)
        {
            var newCodeBlocks = new List<CodeBlockBitMask>();

            string country = null;
            var lineNumber = 0;
            foreach(var line in _FileSystem.ReadAllLines(fileName)) {
                ++lineNumber;
                var text = line.Trim();
                var commentPosn = text.IndexOf('#');
                if(commentPosn != -1) {
                    text = text[..commentPosn].Trim();
                }
                if(text != "") {
                    if(text.StartsWith("[") && text.EndsWith("]")) {
                        country = text[1..^1].Trim();
                    } else {
                        var chunks = text.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if(chunks.Length > 0) {
                            if(String.IsNullOrEmpty(country)) {
                                _Log.WriteLine($"Missing country at line {lineNumber} of local codeblock override");
                            } else {
                                var icao = chunks[0];
                                var mil = chunks.Length > 1 ? chunks[1] : "";

                                var bitmask = CustomConvert.Icao24(icao);
                                if(bitmask == -1) {
                                    _Log.WriteLine($"Invalid ICAO {icao} at line {lineNumber} of local codeblock override");
                                    continue;
                                }

                                var isMilitary = mil.Equals("MIL", StringComparison.InvariantCultureIgnoreCase);
                                if(!isMilitary && !mil.Equals("CIV", StringComparison.InvariantCultureIgnoreCase)) {
                                    _Log.WriteLine($"Invalid military/civilian designator '{mil}' - must be one of 'mil' or 'civ' at line {lineNumber} of local codeblock override");
                                    continue;
                                }

                                newCodeBlocks.Add(new CodeBlockBitMask() {
                                    BitMask = bitmask,
                                    SignificantBitMask = 0xFFFFFF,
                                    CodeBlock = new CodeBlock() {
                                        IsMilitary = isMilitary,
                                        Country = country,
                                    }
                                });
                            }
                        }
                    }
                }
            }

            if(newCodeBlocks.Count > 0) {
                lock(_CodeBlockCacheLock) {
                    var newCache = new List<CodeBlockBitMask>(newCodeBlocks);
                    newCache.AddRange(_CodeBlockCache);
                    _CodeBlockCache = newCache;
                }
            }
        }

        private void LocalFakeModelCodesOverrides(string fileName)
        {
            List<string> groundVehicleCodes = null;
            List<string> towerCodes = null;

            if(_FileSystem.FileExists(fileName)) {
                var section = 0;
                foreach(var line in _FileSystem.ReadAllLines(fileName)) {
                    var text = line.Trim();
                    var commentPosn = text.IndexOf('#');
                    if(commentPosn != -1) {
                        text = text[..commentPosn].Trim();
                    }
                    if(text != "") {
                        if(text == "[GroundVehicleCodes]") {
                            section = 1;
                            groundVehicleCodes = new List<string>();
                        } else if(text == "[TowerCodes]") {
                            section = 2;
                            towerCodes = new List<string>();
                        } else if(section != 0) {
                            switch(section) {
                                case 1:     groundVehicleCodes.Add(text); break;
                                case 2:     towerCodes.Add(text); break;
                            }
                        }
                    }
                }
            }

            _FakeGroundVehicleCodes = groundVehicleCodes?.ToArray() ?? new string[] { "-GND" };
            _FakeTowerCodes = towerCodes?.ToArray() ?? new string[] { "-TWR" };
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="callSign"></param>
        /// <returns></returns>
        public Route FindRoute(string callSign)
        {
            Route result = null;

            if(!String.IsNullOrEmpty(callSign)) {
                if(_DatabaseVersion >= 3) {
                    // Support for lookups against callsign alone was introduced in 2013. I don't think I
                    // need to support the older formats in .NET Core.
                    Lock(_ => {
                        if(_FilesValid) {
                            using(var context = CreateContext()) {
                                var route = context.Routes
                                    .Where(route => route.Callsign == callSign)
                                    .Include(route => route.FromAirport)
                                        .ThenInclude(airport => airport.Country)
                                    .Include(route => route.ToAirport)
                                        .ThenInclude(airport => airport.Country)
                                    .Include(route => route.RouteStops)
                                        .ThenInclude(routeStop => routeStop.Airport)
                                            .ThenInclude(airport => airport.Country)
                                    .AsNoTracking()
                                    .AsSplitQuery()
                                    .FirstOrDefault();

                                result = ModelToPublicConverter.ToRoute(route);
                            }
                        }
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public AircraftType FindAircraftType(string type)
        {
            AircraftType result = null;

            if(_FakeGroundVehicleCodes != null && _FakeGroundVehicleCodes.Length > 0 && _FakeGroundVehicleCodes.Contains(type)) {
                result = ModelToPublicConverter.ToAircraftType(type, Species.GroundVehicle);
            } else if(_FakeTowerCodes != null && _FakeTowerCodes.Length > 0 && _FakeTowerCodes.Contains(type)) {
                result = ModelToPublicConverter.ToAircraftType(type, Species.Tower);
            } else {
                Lock(_ => {
                    if(_FilesValid) {
                        using(var context = CreateContext()) {
                            var aircraftType = context.AircraftTypes
                                .Where(aircraftType => aircraftType.Icao == type)
                                .Include(aircraftType => aircraftType.AircraftTypeModels)
                                    .ThenInclude(aircraftTypeModel => aircraftTypeModel.Model)
                                        .ThenInclude(model => model.Manufacturer)
                                .AsSplitQuery()
                                .AsNoTracking()
                                .FirstOrDefault();

                            result = ModelToPublicConverter.ToAircraftType(aircraftType);
                        }
                    }
                });
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public IEnumerable<Airline> FindAirlinesForCode(string code)
        {
            var result = new List<Airline>();

            if(!String.IsNullOrWhiteSpace(code)) {
                Lock(_ => {
                    if(_FilesValid) {
                        using(var context = CreateContext()) {
                            IQueryable<OperatorModel> queryResults = context.Operators;
                            queryResults = code.Length == 2
                                ? queryResults.Where(r => r.Iata == code)
                                : queryResults.Where(r => r.Icao == code);

                            result.AddRange(
                                queryResults.Select(
                                    model => ModelToPublicConverter.ToAirline(model)
                                )
                            );
                        }
                    }
                });
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao24"></param>
        /// <returns></returns>
        public CodeBlock FindCodeBlock(string icao24)
        {
            CodeBlock result = null;

            if(!String.IsNullOrEmpty(icao24)) {
                var icaoValue = CustomConvert.Icao24(icao24);
                if(icaoValue != -1) {
                    List<CodeBlockBitMask> codeBlockCache;
                    lock(_CodeBlockCacheLock) {
                        codeBlockCache = _CodeBlockCache;
                    }

                    foreach(var entry in codeBlockCache) {
                        if(entry.CodeMatches(icaoValue)) {
                            result = entry.CodeBlock;
                            break;
                        }
                    }
                    if(result == null && _FilesValid) {
                        result = new();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public Airport FindAirportForCode(string code)
        {
            Airport result = null;

            if(!String.IsNullOrWhiteSpace(code)) {
                Lock(_ => {
                    if(_FilesValid) {
                        using(var context = CreateContext()) {
                            IQueryable<AirportModel> airports = context.Airports;
                            airports = code.Length == 4
                                ? airports.Where(r => r.Icao == code)
                                : airports.Where(r => r.Iata == code);

                            var airport = airports
                                .Include(airport => airport.Country)
                                .AsNoTracking()
                                .FirstOrDefault();

                            result = ModelToPublicConverter.ToAirport(airport);
                        }
                    }
                });
            }

            return result;
        }

    }
}
