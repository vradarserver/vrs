// Copyright © 2017 onwards, Andrew Whewell
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using VirtualRadar.Interface.Database;

namespace VirtualRadar.Database.BaseStation
{
    /// <summary>
    /// Default implementation of <see cref="IBaseStationDatabaseImport"/>.
    /// </summary>
    class BaseStationDatabaseImport : IBaseStationDatabaseImport
    {
        /// <summary>
        /// The log stream to write to.
        /// </summary>
        private StreamWriter _Log;

        /// <summary>
        /// A map of source location IDs to destination location IDs.
        /// </summary>
        private Dictionary<int, int> _LocationMap = new Dictionary<int, int>();

        /// <summary>
        /// A map of source session IDs to destintion session IDs.
        /// </summary>
        private Dictionary<int, int> _SessionMap = new Dictionary<int, int>();

        /// <summary>
        /// A map of source aircraft IDs to destination aircraft IDs.
        /// </summary>
        private Dictionary<int, int> _AircraftMap = new Dictionary<int, int>();

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string BaseStationFileName { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string LogFileName { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool AppendLog { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool ImportAircraft { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool ImportSessions { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool ImportLocations { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool ImportFlights { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Import()
        {
            if(String.IsNullOrEmpty(BaseStationFileName)) {
                throw new InvalidOperationException("You must supply a BaseStation filename");
            }
            if(!File.Exists(BaseStationFileName)) {
                throw new FileNotFoundException($"{BaseStationFileName} does not exist");
            }

            var source = Factory.Singleton.Resolve<IBaseStationDatabaseSQLite>();
            var dest =  Factory.Singleton.ResolveSingleton<IAutoConfigBaseStationDatabase>().Database;

            if(source.Engine == dest.Engine) {
                throw new InvalidOperationException("This is not intended for copying between SQLite databases");
            }

            _Log = OpenLogFile();
            try {
                WriteLog($"===============================================================================");
                WriteLog($"BaseStation import started. All timestamps are UTC.");
                WriteLog($"Importing from {BaseStationFileName}");

                source.FileName = BaseStationFileName;
                source.WriteSupportEnabled = false;
                if(!source.TestConnection()) {
                    WriteLog($"Could not open a connection to {source.FileName}");
                }

                ProcessLocations(source, dest);
                ProcessSessions(source, dest);
                ProcessAircraft(source, dest);
                ProcessFlights(source, dest);

                WriteLog("Finished import");
                WriteLog($"===============================================================================");
            } finally {
                _Log?.Dispose();
            }
        }

        /// <summary>
        /// Imports Location records.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        private void ProcessLocations(IBaseStationDatabase source, IBaseStationDatabase dest)
        {
            _LocationMap.Clear();

            if(!ImportLocations) {
                WriteLog("Location import skipped");
            } else {
                WriteLog("Importing Location records");

                var allSource = source.GetLocations();
                var allDest =   dest.GetLocations();

                foreach(var rec in allSource) {
                    var sourceID = rec.LocationID;

                    var existing = allDest.FirstOrDefault(r => r.LocationID > 0 && String.Equals(r.LocationName, rec.LocationName));
                    if(existing == null) {
                        rec.LocationID = 0;
                        dest.InsertLocation(rec);
                    } else {
                        rec.LocationID = existing.LocationID;
                        existing.LocationID = -1;
                        dest.UpdateLocation(rec);
                    }

                    _LocationMap.Add(sourceID, rec.LocationID);
                }

                WriteLog($"    Imported {allSource.Count:N0} locations");
            }
        }

        /// <summary>
        /// Imports Session records.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        private void ProcessSessions(IBaseStationDatabaseSQLite source, IBaseStationDatabase dest)
        {
            _SessionMap.Clear();

            if(!ImportSessions) {
                WriteLog("Session import skipped");
            } else if(!ImportLocations) {
                WriteLog("Session import skipped because location import was skipped");
            } else {
                WriteLog("Importing Session records");

                var allSource = source.GetSessions();
                var allDest = dest.GetSessions();

                foreach(var rec in allSource) {
                    if(_LocationMap.TryGetValue(rec.LocationID, out var destLocationID)) {
                        var sourceID = rec.SessionID;
                        rec.LocationID = destLocationID;

                        var existing = allDest.FirstOrDefault(r => r.SessionID > 0 && r.StartTime == rec.StartTime);
                        if(existing == null) {
                            rec.SessionID = 0;
                            dest.InsertSession(rec);
                        } else {
                            rec.SessionID = existing.SessionID;
                            dest.UpdateSession(rec);
                        }

                        _SessionMap.Add(sourceID, rec.SessionID);
                    }
                }

                WriteLog($"    Imported {allSource.Count:N0} sessions");
            }
        }

        /// <summary>
        /// Imports aircraft records.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        private void ProcessAircraft(IBaseStationDatabaseSQLite source, IBaseStationDatabase dest)
        {
            _AircraftMap.Clear();

            if(!ImportAircraft) {
                WriteLog("Aircraft import skipped");
            } else {
                WriteLog("Importing Aircraft records");

                var allSource = source.GetAllAircraft().ToDictionary(r => r.AircraftID, r => r);
                var upsertCandidates = new List<BaseStationAircraftUpsert>();
                var upsertKeys = new HashSet<string>();
                foreach(var kvp in allSource) {
                    if(!upsertKeys.Contains(kvp.Value.ModeS)) {
                        upsertCandidates.Add(new BaseStationAircraftUpsert(kvp.Value));
                        upsertKeys.Add(kvp.Value.ModeS);
                    }
                }
                upsertKeys.Clear();

                var upserted = dest.UpsertManyAircraft(upsertCandidates).ToDictionary(r => r.ModeS, r => r);
                upsertCandidates.Clear();

                foreach(var sourceKvp in allSource) {
                    var sourceID = sourceKvp.Key;
                    if(upserted.TryGetValue(sourceKvp.Value.ModeS, out var rec)) {
                        _AircraftMap.Add(sourceID, rec.AircraftID);
                    }
                }

                WriteLog($"    Imported {upserted.Count:N0} / {allSource.Count:N0} aircraft");
            }
        }

        /// <summary>
        /// Imports flight records.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        private void ProcessFlights(IBaseStationDatabaseSQLite source, IBaseStationDatabase dest)
        {
            if(!ImportFlights) {
                WriteLog("Flight import skipped");
            } else if(!ImportSessions || !ImportLocations) {
                WriteLog("Flight import skipped because session import was skipped");
            } else if(!ImportAircraft) {
                WriteLog("Flight import skipped because aircraft import was skipped");
            } else {
                WriteLog("Importing flight records");

                var criteria = new SearchBaseStationCriteria();
                var countFlights = source.GetCountOfFlights(new SearchBaseStationCriteria());
                var countSource = 0;
                var countDest = 0;
                var startRow = 0;
                var pageSize = 30000;

                while(startRow < countFlights) {
                    var allSource = source.GetFlights(criteria, startRow, startRow + (pageSize - 1), "DATE", true, null, false);
                    countSource += allSource.Count;

                    var upsertCandidates = new List<BaseStationFlightUpsert>();
                    var upsertKeys = new HashSet<string>();
                    foreach(var candidate in allSource) {
                        var key = $"{candidate.AircraftID}-{candidate.StartTime}";
                        if(!upsertKeys.Contains(key) && _AircraftMap.TryGetValue(candidate.AircraftID, out var aircraftID) && _SessionMap.TryGetValue(candidate.SessionID, out var sessionID)) {
                            upsertCandidates.Add(new BaseStationFlightUpsert(candidate) {
                                AircraftID = aircraftID,
                                SessionID =  sessionID,
                            });
                            upsertKeys.Add(key);
                        }
                    }

                    var upserted = dest.UpsertManyFlights(upsertCandidates);
                    countDest += upserted.Length;
                    startRow += pageSize;
                }

                WriteLog($"    Imported {countDest:N0} / {countSource:N0} flights");
            }
        }

        /// <summary>
        /// Returns the log stream if applicable.
        /// </summary>
        /// <returns></returns>
        private StreamWriter OpenLogFile()
        {
            StreamWriter result = null;

            if(!String.IsNullOrEmpty(LogFileName)) {
                var directory = Path.GetDirectoryName(LogFileName);
                if(!Directory.Exists(directory)) {
                    Directory.CreateDirectory(directory);
                }

                result = new StreamWriter(LogFileName, append: AppendLog);
            }

            return result;
        }

        /// <summary>
        /// Writes a message to the log file.
        /// </summary>
        /// <param name="message"></param>
        private void WriteLog(string message)
        {
            _Log?.WriteLine($"[{DateTime.UtcNow}] {message}");
        }
    }
}
