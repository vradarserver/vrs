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

            var configurationStorage = Factory.Singleton.ResolveSingleton<IConfigurationStorage>();
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
            var connectionStringBuilder = Factory.Singleton.Resolve<ISQLiteConnectionStringBuilder>().Initialise();
            connectionStringBuilder.DataSource = _DatabaseFileName;
            connectionStringBuilder.DateTimeFormat = SQLiteDateFormats.ISO8601;
            connectionStringBuilder.FailIfMissing = true;
            connectionStringBuilder.ReadOnly = true;
            connectionStringBuilder.JournalMode = SQLiteJournalModeEnum.Off;  // <-- standing data is *ALWAYS* read-only, we don't need to create a journal

            var result = Factory.Singleton.Resolve<ISQLiteConnectionProvider>().Create(connectionStringBuilder.ConnectionString);
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
                Sql.RunSql(connection, null, "SELECT [Version] FROM [DatabaseVersion]", null, reader => {
                    result = Sql.GetInt32(reader, 0);
                    return false;
                }, false, null);
            }

            return result;
        }

        private void CacheCodeBlocks()
        {
            var newCache = new List<CodeBlockBitMask>();
            if(_FilesValid) {
                using(var connection = CreateOpenConnection()) {
                    Sql.RunSql(connection, null,
                        "SELECT [BitMask]" +
                        "      ,[SignificantBitMask]" +
                        "      ,[IsMilitary]" +
                        "      ,[Country]" +
                        "  FROM [CodeBlockView]",
                    null, (reader) => {
                        var cacheEntry = new CodeBlockBitMask() {
                            BitMask =               Sql.GetInt32(reader, 0),
                            SignificantBitMask =    Sql.GetInt32(reader, 1),
                            CodeBlock = new CodeBlock() {
                                IsMilitary =        Sql.GetBool(reader, 2),
                                Country =           Sql.GetString(reader, 3),
                            },
                        };
                        newCache.Add(cacheEntry);
                        return true;
                    }, false, null);
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
            var configurationStorage = Factory.Singleton.ResolveSingleton<IConfigurationStorage>();
            var log = Factory.Singleton.ResolveSingleton<ILog>();

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

            const string selectFields = "SELECT [RouteId]" +
                                        "      ,[FromAirportIcao]" +
                                        "      ,[FromAirportIata]" +
                                        "      ,[FromAirportName]" +
                                        "      ,[FromAirportLatitude]" +
                                        "      ,[FromAirportLongitude]" +
                                        "      ,[FromAirportAltitude]" +
                                        "      ,[FromAirportLocation]" +
                                        "      ,[FromAirportCountry]" +
                                        "      ,[ToAirportIcao]" +
                                        "      ,[ToAirportIata]" +
                                        "      ,[ToAirportName]" +
                                        "      ,[ToAirportLatitude]" +
                                        "      ,[ToAirportLongitude]" +
                                        "      ,[ToAirportAltitude]" +
                                        "      ,[ToAirportLocation]" +
                                        "      ,[ToAirportCountry]" +
                                        "  FROM [RouteView]";

            if(!String.IsNullOrEmpty(callSign)) {
                string airlineCode = null, flightCode = null;
                if(_DatabaseVersion < 3) SplitCallsign(callSign, out airlineCode, out flightCode);
                if(_DatabaseVersion >= 3 || (!String.IsNullOrEmpty(airlineCode) && !String.IsNullOrEmpty(flightCode) && (airlineCode.Length == 2 || airlineCode.Length == 3))) {
                    lock(Lock) {
                        if(_FilesValid) {
                            using(var connection = CreateOpenConnection()) {
                                var selectCommand = selectFields;
                                var parameters = new Dictionary<string, object>();
                                if(_DatabaseVersion >= 3) {
                                    selectCommand = String.Format("{0} WHERE [Callsign] = @callsign", selectCommand);
                                    parameters.Add("@callsign", callSign);
                                } else {
                                    var airlineField = airlineCode.Length == 2 ? "Iata" : "Icao";
                                    selectCommand = String.Format("{0} WHERE [Operator{1}] = @airlineCode AND [FlightNumber] = @flightCode", selectCommand, airlineField);
                                    parameters.Add("@airlineCode",   airlineCode);
                                    parameters.Add("@flightCode",    flightCode);
                                }

                                Sql.RunSql(connection, null, selectCommand, parameters, (reader) => {
                                    var routeId = Sql.GetInt64(reader, 0);
                                    result = new Route() {
                                        From = CreateAirport(
                                            icao:       Sql.GetString(reader, 1),
                                            iata:       Sql.GetString(reader, 2),
                                            name:       Sql.GetString(reader, 3),
                                            latitude:   Sql.GetNDouble(reader, 4),
                                            longitude:  Sql.GetNDouble(reader, 5),
                                            altitude:   Sql.GetNInt32(reader, 6),
                                            location:   Sql.GetString(reader, 7),
                                            country:    Sql.GetString(reader, 8)
                                        ),
                                        To = CreateAirport(
                                            icao:       Sql.GetString(reader, 9),
                                            iata:       Sql.GetString(reader, 10),
                                            name:       Sql.GetString(reader, 11),
                                            latitude:   Sql.GetNDouble(reader, 12),
                                            longitude:  Sql.GetNDouble(reader, 13),
                                            altitude:   Sql.GetNInt32(reader, 14),
                                            location:   Sql.GetString(reader, 15),
                                            country:    Sql.GetString(reader, 16)
                                        ),
                                    };
                                    LoadStopovers(connection, null, null, routeId, result.Stopovers);
                                    return false;
                                }, false, null);
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
            Sql.RunSql(connection, transaction,
                    "SELECT [AirportIcao]" +
                    "      ,[AirportIata]" +
                    "      ,[AirportName]" +
                    "      ,[AirportLatitude]" +
                    "      ,[AirportLongitude]" +
                    "      ,[AirportAltitude]" +
                    "      ,[AirportLocation]" +
                    "      ,[AirportCountry]" +
                    "  FROM [RouteStopView]" +
                    " WHERE [RouteId] = @routeId" +
                    " ORDER BY [SequenceNo] ASC",
                new Dictionary<string, object>() {
                    { "@routeId", routeId },
                }, (reader) => {
                    airports.Add(CreateAirport(
                        icao:       Sql.GetString(reader, 0),
                        iata:       Sql.GetString(reader, 1),
                        name:       Sql.GetString(reader, 2),
                        latitude:   Sql.GetNDouble(reader, 3),
                        longitude:  Sql.GetNDouble(reader, 4),
                        altitude:   Sql.GetNInt32(reader, 5),
                        location:   Sql.GetString(reader, 6),
                        country:    Sql.GetString(reader, 7)
                    ));
                    return true;
                }, false, log
            );
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
                            Sql.RunSql(connection, null,
                                "SELECT [Icao]" +
                                "      ,[WakeTurbulenceId]" +
                                "      ,[SpeciesId]" +
                                "      ,[EngineTypeId]" +
                                "      ,[Engines]" +
                                "      ,[Model]" +
                                "      ,[Manufacturer]" +
                                (_DatabaseVersion >= 5 ? ",[EnginePlacementId]" : "") +
                                "  FROM [AircraftTypeNoEnumsView]" +
                                " WHERE [Icao] = @icao",
                            new Dictionary<string,object>() {
                                { "@icao", type },
                            }, (reader) => {
                                if(result == null) {
                                    result = new AircraftType() {
                                        Type =  Sql.GetString(reader, 0),
                                        WakeTurbulenceCategory = (WakeTurbulenceCategory)Sql.GetInt32(reader, 1),
                                        Species = (Species)Sql.GetInt32(reader, 2),
                                        EngineType = (EngineType)Sql.GetInt32(reader, 3),
                                        Engines = Sql.GetString(reader, 4),
                                    };
                                }
                                result.Models.Add(Sql.GetString(reader, 5));
                                var manufacturer = Sql.GetString(reader, 6);
                                if(!String.IsNullOrEmpty(manufacturer)) result.Manufacturers.Add(manufacturer);

                                if(_DatabaseVersion >= 5) {
                                    result.EnginePlacement = (EnginePlacement)Sql.GetInt32(reader, 7);
                                }

                                return true;
                            }, false, null);
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
                            Sql.RunSql(connection, null, String.Format("SELECT [Icao], [Iata], [Name] FROM [Operator] WHERE [{0}] = @code", code.Length == 2 ? "Iata" : "Icao"), new Dictionary<string,object>() {
                                { "@code", code },
                            }, reader => {
                                result.Add(new Airline() {
                                    IcaoCode = Sql.GetString(reader, 0),
                                    IataCode = Sql.GetString(reader, 1),
                                    Name = Sql.GetString(reader, 2),
                                });
                                return true;
                            }, false, null);
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
