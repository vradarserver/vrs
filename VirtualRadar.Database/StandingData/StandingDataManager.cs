// Copyright © 2013 onwards, Andrew Whewell
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.SQLite;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Localisation;
using Dapper;

namespace VirtualRadar.Database.StandingData
{
    /// <summary>
    /// The SQLite implementation of <see cref="IStandingDataManager"/>.
    /// </summary>
    class StandingDataManager : IStandingDataManager
    {
        #region Private class - DefaultProvider
        class DefaultProvider : IStandingDataManagerProvider
        {
            public bool FileExists(string fileName)
            {
                return File.Exists(fileName);
            }

            public string[] ReadAllLines(string fileName)
            {
                return File.ReadAllLines(fileName);
            }
        }
        #endregion

        #region Private class - CodeBlockBitMask
        /// <summary>
        /// A private class detailing information about a CodeBlock entry.
        /// </summary>
        class CodeBlockBitMask
        {
            public CodeBlock CodeBlock;
            public int BitMask;
            public int SignificantBitMask;

            public bool CodeMatches(int icao24)
            {
                return BitMask == (icao24 & SignificantBitMask);
            }
        }
        #endregion

        #region Fields
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
        #endregion

        #region Properties
        private static StandingDataManager _StandingDataManager;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IStandingDataManager Singleton
        {
            get
            {
                if(_StandingDataManager == null) _StandingDataManager = new StandingDataManager();
                return _StandingDataManager;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IStandingDataManagerProvider Provider { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public object Lock { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string RouteStatus { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool CodeBlocksLoaded { get; private set; }
        #endregion

        #region Events
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
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public StandingDataManager()
        {
            Lock = new object();
            Provider = new DefaultProvider();
            RouteStatus = Strings.NotLoaded;

            var configurationStorage = Factory.ResolveSingleton<IConfigurationStorage>();
            _DatabaseFileName = Path.Combine(configurationStorage.Folder, "StandingData.sqb");
            _StateFileName = Path.Combine(configurationStorage.Folder, "FlightNumberCoverage.csv");
        }
        #endregion

        #region CreateOpenConnection
        /// <summary>
        /// Returns an open connection to the standing data database.
        /// </summary>
        /// <returns></returns>
        private IDbConnection CreateOpenConnection()
        {
            var connectionStringBuilder = Factory.Resolve<ISQLiteConnectionStringBuilder>().Initialise();
            connectionStringBuilder.DataSource = _DatabaseFileName;
            connectionStringBuilder.DateTimeFormat = SQLiteDateFormats.ISO8601;
            connectionStringBuilder.FailIfMissing = true;
            connectionStringBuilder.ReadOnly = true;
            connectionStringBuilder.JournalMode = SQLiteJournalModeEnum.Off;  // <-- standing data is *ALWAYS* read-only, we don't need to create a journal

            var result = Factory.Resolve<ISQLiteConnectionProvider>().Create(connectionStringBuilder.ConnectionString);
            result.Open();

            return result;
        }
        #endregion

        #region Load
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Load()
        {
            lock(Lock) {
                _FilesValid = SetRouteStatus();
                _DatabaseVersion = GetDatabaseVersionNumber();
                CacheCodeBlocks();
                LoadOverrides();
            }
            OnLoadCompleted(EventArgs.Empty);
        }

        /// <summary>
        /// Updates <see cref="RouteStatus"/> to show the current state of the routes.
        /// </summary>
        private bool SetRouteStatus()
        {
            bool result = false;

            if(!Provider.FileExists(_DatabaseFileName) ||
               !Provider.FileExists(_StateFileName)) {
                RouteStatus = Strings.SomeRouteFilesMissing;
            } else {
                string[] lines = Provider.ReadAllLines(_StateFileName);
                if(lines.Length < 2) RouteStatus = Strings.RouteStateFileInvalid;
                else {
                    string[] chunks = lines[1].Split(new char[] { ',' });
                    if(chunks.Length < 3) RouteStatus = Strings.RouteStateFileContentInvalid;
                    else {
                        DateTime startDate = DateTime.MinValue, endDate = DateTime.MinValue;
                        int countRoutes = 0;
                        if(!DateTime.TryParseExact(chunks[0], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate) ||
                           !DateTime.TryParseExact(chunks[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate) ||
                           !int.TryParse(chunks[2], out countRoutes)) {
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

            using(var connection = CreateOpenConnection()) {
                result = connection.ExecuteScalar<int>("SELECT [Version] FROM [DatabaseVersion]");
            }

            return result;
        }

        private void CacheCodeBlocks()
        {
            var newCache = new List<CodeBlockBitMask>();
            if(_FilesValid) {
                using(var connection = CreateOpenConnection()) {
                    foreach(var cacheEntry in connection.Query<CodeBlockBitMask, CodeBlock, CodeBlockBitMask>(@"
                        SELECT [BitMask]
                              ,[SignificantBitMask]
                              ,[IsMilitary]
                              ,[Country]
                        FROM   [CodeBlockView]
                    ", map: (bitmask, codeblock) => {
                        bitmask.CodeBlock = codeblock;
                        return bitmask;
                    }, splitOn: nameof(CodeBlock.IsMilitary)
                    )) {
                        newCache.Add(cacheEntry);
                    }
                }
                newCache.Sort((CodeBlockBitMask lhs, CodeBlockBitMask rhs) => { return -(lhs.SignificantBitMask - rhs.SignificantBitMask); });
            }

            lock(_CodeBlockCacheLock) {
                _CodeBlockCache = newCache;
                CodeBlocksLoaded = _CodeBlockCache.Count > 0;
            }
        }

        private void LoadOverrides()
        {
            var configurationStorage = Factory.ResolveSingleton<IConfigurationStorage>();
            var log = Factory.ResolveSingleton<ILog>();

            var codeBlocksFileName = Path.Combine(configurationStorage.Folder, "LocalAircraft.txt");
            if(Provider.FileExists(codeBlocksFileName)) LoadCodeBlocksOverrides(codeBlocksFileName, log);

            var fakeModelCodesFileName = Path.Combine(configurationStorage.Folder, "FakeModelCodes.txt");
            LocalFakeModelCodesOverrides(fakeModelCodesFileName, log);
        }

        private void LoadCodeBlocksOverrides(string fileName, ILog log)
        {
            var newCodeBlocks = new List<CodeBlockBitMask>();

            string country = null;
            int lineNumber = 0;
            foreach(var line in Provider.ReadAllLines(fileName)) {
                ++lineNumber;
                var text = line.Trim();
                var commentPosn = text.IndexOf('#');
                if(commentPosn != -1) text = text.Substring(0, commentPosn).Trim();
                if(text != "") {
                    if(text.StartsWith("[") && text.EndsWith("]")) {
                        country = text.Substring(1, text.Length - 2).Trim();
                    } else {
                        var chunks = text.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if(chunks.Length > 0) {
                            if(String.IsNullOrEmpty(country)) log.WriteLine("Missing country at line {0} of local codeblock override", lineNumber);
                            else {
                                var icao = chunks[0];
                                var mil = chunks.Length > 1 ? chunks[1] : "";

                                int bitmask = CustomConvert.Icao24(icao);
                                if(bitmask == -1) {
                                    log.WriteLine("Invalid ICAO {0} at line {1} of local codeblock override", icao, lineNumber);
                                    continue;
                                }

                                var isMilitary = mil.ToUpper() == "MIL";
                                if(!isMilitary && mil.ToUpper() != "CIV") {
                                    log.WriteLine("Invalid military/civilian designator '{0}' - must be one of 'mil' or 'civ' at line {1} of local codeblock override", mil, lineNumber);
                                    continue;
                                }

                                newCodeBlocks.Add(new CodeBlockBitMask() {
                                    BitMask = bitmask,
                                    SignificantBitMask = 0xffffff,
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

        private void LocalFakeModelCodesOverrides(string fileName, ILog log)
        {
            List<string> groundVehicleCodes = null;
            List<string> towerCodes = null;

            if(Provider.FileExists(fileName)) {
                var section = 0;
                foreach(var line in Provider.ReadAllLines(fileName)) {
                    var text = line.Trim();
                    var commentPosn = text.IndexOf('#');
                    if(commentPosn != -1) text = text.Substring(0, commentPosn).Trim();
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

            _FakeGroundVehicleCodes = groundVehicleCodes == null ? new string[] { "-GND" } : groundVehicleCodes.ToArray();
            _FakeTowerCodes = towerCodes == null ? new string[] { "-TWR" } : towerCodes.ToArray();
        }
        #endregion

        #region FindRoute
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="callSign"></param>
        /// <returns></returns>
        public Route FindRoute(string callSign)
        {
            Route result = null;

            const string selectFields = @"
                SELECT [RouteId]
                      ,[FromAirportIcao]
                      ,[FromAirportIata]
                      ,[FromAirportName]
                      ,[FromAirportLatitude]
                      ,[FromAirportLongitude]
                      ,[FromAirportAltitude]
                      ,[FromAirportLocation]
                      ,[FromAirportCountry]
                      ,[ToAirportIcao]
                      ,[ToAirportIata]
                      ,[ToAirportName]
                      ,[ToAirportLatitude]
                      ,[ToAirportLongitude]
                      ,[ToAirportAltitude]
                      ,[ToAirportLocation]
                      ,[ToAirportCountry]
                FROM   [RouteView]
            ";

            if(!String.IsNullOrEmpty(callSign)) {
                string airlineCode = null, flightCode = null;
                if(_DatabaseVersion < 3) {
                    SplitCallsign(callSign, out airlineCode, out flightCode);
                }
                if(_DatabaseVersion >= 3 || (!String.IsNullOrEmpty(airlineCode) && !String.IsNullOrEmpty(flightCode) && (airlineCode.Length == 2 || airlineCode.Length == 3))) {
                    lock(Lock) {
                        if(_FilesValid) {
                            using(var connection = CreateOpenConnection()) {
                                var selectCommand = selectFields;
                                var parameters = new DynamicParameters();

                                if(_DatabaseVersion >= 3) {
                                    selectCommand = $"{selectCommand} WHERE [Callsign] = @callsign";
                                    parameters.Add("callsign", callSign);
                                } else {
                                    var airlineField = airlineCode.Length == 2 ? "Iata" : "Icao";
                                    selectCommand = $"{selectCommand} WHERE [Operator{airlineField}] = @airlineCode AND [FlightNumber] = @flightCode";
                                    parameters.Add("airlineCode", airlineCode);
                                    parameters.Add("flightCode",  flightCode);
                                }

                                var routeView = connection.QueryFirstOrDefault<RouteViewModel>(selectCommand, parameters);
                                if(routeView != null) {
                                    result = new Route() {
                                        From = CreateAirport(
                                            icao:       routeView.FromAirportIcao,
                                            iata:       routeView.FromAirportIata,
                                            name:       routeView.FromAirportName,
                                            latitude:   routeView.FromAirportLatitude,
                                            longitude:  routeView.FromAirportLongitude,
                                            altitude:   routeView.FromAirportAltitude,
                                            location:   routeView.FromAirportLocation,
                                            country:    routeView.FromAirportCountry
                                        ),
                                        To = CreateAirport(
                                            icao:       routeView.ToAirportIcao,
                                            iata:       routeView.ToAirportIata,
                                            name:       routeView.ToAirportName,
                                            latitude:   routeView.ToAirportLatitude,
                                            longitude:  routeView.ToAirportLongitude,
                                            altitude:   routeView.ToAirportAltitude,
                                            location:   routeView.ToAirportLocation,
                                            country:    routeView.ToAirportCountry
                                        ),
                                    };
                                    LoadStopovers(connection, null, null, routeView.RouteId, result.Stopovers);
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Fills the airports list with the stopovers for the route in sequence number order.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="routeId"></param>
        /// <param name="airports"></param>
        private void LoadStopovers(IDbConnection connection, IDbTransaction transaction, TextWriter log, long routeId, ICollection<Airport> airports)
        {
            foreach(var routeStop in connection.Query<RouteStopViewModel>(@"
                SELECT   [AirportIcao]
                        ,[AirportIata]
                        ,[AirportName]
                        ,[AirportLatitude]
                        ,[AirportLongitude]
                        ,[AirportAltitude]
                        ,[AirportLocation]
                        ,[AirportCountry]
                FROM     [RouteStopView]
                WHERE    [RouteId] = @routeId
                ORDER BY [SequenceNo] ASC
            ", new {
                routeId = routeId,
            })) {
                airports.Add(CreateAirport(
                    icao:       routeStop.AirportIcao,
                    iata:       routeStop.AirportIata,
                    name:       routeStop.AirportName,
                    latitude:   routeStop.AirportLatitude,
                    longitude:  routeStop.AirportLongitude,
                    altitude:   routeStop.AirportAltitude,
                    location:   routeStop.AirportLocation,
                    country:    routeStop.AirportCountry
                ));
            }
        }

        /// <summary>
        /// Creates an airport object from the constituent parts passed across.
        /// </summary>
        /// <param name="icao"></param>
        /// <param name="iata"></param>
        /// <param name="name"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="altitude"></param>
        /// <param name="location"></param>
        /// <param name="country"></param>
        /// <returns></returns>
        private Airport CreateAirport(string icao, string iata, string name, double? latitude, double? longitude, int? altitude, string location, string country)
        {
            var result = new Airport() {
                IcaoCode = icao,
                IataCode = iata,
                Latitude = latitude,
                Longitude = longitude,
                AltitudeFeet = altitude,
                Country = country,
            };
            result.Name = Describe.AirportName(name, location);

            return result;
        }

        /// <summary>
        /// Splits a callsign up into the airline code and the flight code.
        /// </summary>
        /// <param name="callSign"></param>
        /// <param name="airlineCode"></param>
        /// <param name="flightCode"></param>
        /// <remarks>
        /// E.G. a callsign of ANZ039C would be split into an airline code of ANZ and a flight code
        /// of 039C.
        /// </remarks>
        private void SplitCallsign(string callSign, out string airlineCode, out string flightCode)
        {
            airlineCode = flightCode = null;

            if(!String.IsNullOrEmpty(callSign) && callSign.Length >= 3) {
                if(Char.IsDigit(callSign[2])) {
                    airlineCode = callSign.Substring(0, 2);
                    flightCode = callSign.Substring(2);
                } else {
                    airlineCode = callSign.Substring(0, 3);
                    flightCode = callSign.Substring(3);
                }

                flightCode = flightCode.TrimStart(new char[] { '0' });
            }
        }
        #endregion

        #region FindAircraftType
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public AircraftType FindAircraftType(string type)
        {
            AircraftType result = null;

            if(_FakeGroundVehicleCodes != null && _FakeGroundVehicleCodes.Length > 0 && _FakeGroundVehicleCodes.Contains(type)) {
                result = CreateGroundVehicleRecord(type);
            } else if(_FakeTowerCodes != null && _FakeTowerCodes.Length > 0 && _FakeTowerCodes.Contains(type)) {
                result = CreateTowerRecord(type);
            }

            if(result == null) {
                lock(Lock) {
                    if(_FilesValid) {
                        using(var connection = CreateOpenConnection()) {
                            foreach(var aircraftType in connection.Query<AircraftType, string, string, AircraftType>(@"
                                SELECT [Icao]             AS [Type]
                                      ,[WakeTurbulenceId] AS [WakeTurbulenceCategory]
                                      ,[SpeciesId]        AS [Species]
                                      ,[EngineTypeId]     AS [EngineType]
                                      ,[Engines]" +
                                       (_DatabaseVersion >= 5 ? ",[EnginePlacementId] AS [EnginePlacement]" : "") +
                                @"    ,[Manufacturer]
                                      ,[Model]
                                FROM   [AircraftTypeNoEnumsView]
                                WHERE  [Icao] = @icao
                            ",
                            map: (acType, manufacturer, model) => {
                                if(!String.IsNullOrEmpty(manufacturer)) {
                                    acType.Manufacturers.Add(manufacturer);
                                }
                                if(!String.IsNullOrEmpty(model)) {
                                    acType.Models.Add(model);
                                }
                                return acType;
                            },
                            splitOn: $"Manufacturer,Model",
                            param: new {
                                icao = type
                            })) {
                                if(result == null) {
                                    result = aircraftType;
                                } else {
                                    if(aircraftType.Manufacturers.Count > 0) {
                                        result.Manufacturers.Add(aircraftType.Manufacturers[0]);
                                    }
                                    if(aircraftType.Models.Count > 0) {
                                        result.Models.Add(aircraftType.Models[0]);
                                    }
                                }
                            };
                        }
                    }
                }
            }

            return result;
        }

        private AircraftType CreateGroundVehicleRecord(string type)
        {
            return new AircraftType() {
                Type = type,
                Species = Species.GroundVehicle,
            };
        }

        private AircraftType CreateTowerRecord(string type)
        {
            return new AircraftType() {
                Type = type,
                Species = Species.Tower,
            };
        }
        #endregion

        #region FindAirlinesForCode
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public IEnumerable<Airline> FindAirlinesForCode(string code)
        {
            var result = new List<Airline>();

            if(!String.IsNullOrEmpty(code)) {
                lock(Lock) {
                    if(_FilesValid) {
                        using(var connection = CreateOpenConnection()) {
                            var codeType = code.Length == 2 ? "Iata" : "Icao";
                            result.AddRange(connection.Query<Airline>(@"
                                SELECT [Icao] AS [IcaoCode]
                                      ,[Iata] AS [IataCode]
                                      ,[Name]" +
                                       (_DatabaseVersion >= 6 ? ",[PositioningFlightPattern], [CharterFlightPattern]" : "") +
                            $"  FROM [Operator] WHERE [{codeType}] = @code",
                            new {
                                code
                            }));
                        }
                    }
                }
            }

            return result;
        }
        #endregion

        #region FindCodeBlock
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao24"></param>
        /// <returns></returns>
        public CodeBlock FindCodeBlock(string icao24)
        {
            CodeBlock result = null;

            if(!String.IsNullOrEmpty(icao24)) {
                int icaoValue = CustomConvert.Icao24(icao24);
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
                    if(result == null && _FilesValid) result = new CodeBlock();
                }
            }

            return result;
        }
        #endregion
    }
}
