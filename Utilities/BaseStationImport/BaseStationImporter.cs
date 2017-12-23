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
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;

namespace BaseStationImport
{
    /// <summary>
    /// Handles the importing of BaseStation databases.
    /// </summary>
    class BaseStationImporter
    {
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
        /// Gets or sets the source BaseStation database.
        /// </summary>
        public IBaseStationDatabase Source { get; set; }

        /// <summary>
        /// Gets or sets the destination BaseStation database.
        /// </summary>
        public IBaseStationDatabase Destination { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the schema on the target should not be updated before import begins.
        /// </summary>
        public bool SuppressSchemaUpdate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that aircraft should be imported.
        /// </summary>
        public bool ImportAircraft { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that sessions should be imported.
        /// </summary>
        public bool ImportSessions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that locations should be imported.
        /// </summary>
        public bool ImportLocations { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that flights should be imported.
        /// </summary>
        public bool ImportFlights { get; set; }

        /// <summary>
        /// Gets or sets the date of the earliest flight to import.
        /// </summary>
        public DateTime EarliestFlight { get; set; }

        /// <summary>
        /// Gets the <see cref="EarliestFlight"/> as a nullable date, where null indicates that the min value was supplied.
        /// </summary>
        private DateTime? EarliestFlightCriteria => EarliestFlight.Date == DateTime.MinValue.Date ? (DateTime?)null : EarliestFlight.Date;

        /// <summary>
        /// Gets or sets the date of the latest flight to import.
        /// </summary>
        public DateTime LatestFlight { get; set; }

        /// <summary>
        /// Gets the <see cref="LatestFlight"/> as a nullable date, where null indicates that the max value was supplied.
        /// </summary>
        private DateTime? LatestFlightCriteria => LatestFlight.Date == DateTime.MaxValue.Date ? (DateTime?)null : LatestFlight.Date.AddDays(1).AddTicks(-1);

        /// <summary>
        /// The number of flight records to copy at a time.
        /// </summary>
        public int FlightPageSize { get; set; } = 1000;

        /// <summary>
        /// Raised whenever the importer starts importing or loading a new table.
        /// </summary>
        public EventHandler<EventArgs<string>> TableChanged;

        /// <summary>
        /// Raises <see cref="TableChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnTableChanged(EventArgs<string> args)
        {
            TableChanged?.Invoke(this, args);
        }

        /// <summary>
        /// Raised whenever there is some progress to report.
        /// </summary>
        public EventHandler<ProgressEventArgs> ProgressChanged;

        /// <summary>
        /// Raises <see cref="ProgressChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnProgressChanged(ProgressEventArgs args)
        {
            ProgressChanged?.Invoke(this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Import()
        {
            UpdateSchema();

            ProcessLocations();
            ProcessSessions();
            ProcessAircraft();
            ProcessFlights();
        }

        private void UpdateSchema()
        {
            if(!SuppressSchemaUpdate) {
                OnTableChanged(new EventArgs<string>("Schema"));
                var progress = new ProgressEventArgs() {
                    Caption = "Applying schema",
                    TotalItems = 1,
                };
                OnProgressChanged(progress);

                Destination.CreateDatabaseIfMissing(Destination.FileName);

                progress.CurrentItem = 1;
                OnProgressChanged(progress);
            }
        }

        /// <summary>
        /// Imports Location records.
        /// </summary>
        private void ProcessLocations()
        {
            if(ImportLocations || ImportSessions || ImportFlights) {
                LoadOrImportLocations();
            }
        }

        private void LoadOrImportLocations()
        {
            OnTableChanged(new EventArgs<string>("Locations"));
            var progress = new ProgressEventArgs() {
                Caption =       ImportLocations ? "Importing locations" : "Loading locations",
                TotalItems =    -1,
            };
            OnProgressChanged(progress);

            _LocationMap.Clear();
            var allSource = Source.GetLocations();
            var allDest =   Destination.GetLocations();

            progress.TotalItems = allSource.Count;
            progress.CurrentItem = 0;
            OnProgressChanged(progress);

            foreach(var rec in allSource) {
                var sourceID = rec.LocationID;

                var existing = allDest.FirstOrDefault(r => r.LocationID > 0 && String.Equals(r.LocationName, rec.LocationName));
                if(existing == null) {
                    rec.LocationID = 0;
                    if(ImportLocations) {
                        Destination.InsertLocation(rec);
                    }
                } else {
                    rec.LocationID = existing.LocationID;
                    existing.LocationID = -1;
                    if(ImportLocations) {
                        Destination.UpdateLocation(rec);
                    }
                }

                if(rec.LocationID != 0) {
                    _LocationMap.Add(sourceID, rec.LocationID);
                }

                ++progress.CurrentItem;
                OnProgressChanged(progress);
            }

            progress.CurrentItem = progress.TotalItems;
            OnProgressChanged(progress);
        }

        /// <summary>
        /// Imports Session records.
        /// </summary>
        private void ProcessSessions()
        {
            if(ImportSessions || ImportFlights) {
                LoadOrImportSessions();
            }
        }

        private void LoadOrImportSessions()
        {
            OnTableChanged(new EventArgs<string>("Sessions"));
            var progress = new ProgressEventArgs() {
                Caption =       ImportSessions ? "Importing sessions" : "Loading sessions",
                TotalItems =    -1,
            };
            OnProgressChanged(progress);

            _SessionMap.Clear();
            var allSource = Source.GetSessions();
            var allDest = Destination.GetSessions();

            progress.TotalItems = allSource.Count;
            progress.CurrentItem = 0;
            OnProgressChanged(progress);

            foreach(var rec in allSource) {
                if(_LocationMap.TryGetValue(rec.LocationID, out var destLocationID)) {
                    var sourceID = rec.SessionID;
                    rec.LocationID = destLocationID;

                    var existing = allDest.FirstOrDefault(r => r.SessionID > 0 && r.StartTime == rec.StartTime);
                    if(existing == null) {
                        rec.SessionID = 0;
                        if(ImportSessions) {
                            Destination.InsertSession(rec);
                        }
                    } else {
                        rec.SessionID = existing.SessionID;
                        if(ImportSessions) {
                            Destination.UpdateSession(rec);
                        }
                    }

                    if(rec.SessionID != 0) {
                        _SessionMap.Add(sourceID, rec.SessionID);
                    }
                }

                ++progress.CurrentItem;
                OnProgressChanged(progress);
            }

            progress.CurrentItem = progress.TotalItems;
            OnProgressChanged(progress);
        }

        /// <summary>
        /// Imports aircraft records.
        /// </summary>
        private void ProcessAircraft()
        {
            if(ImportAircraft || ImportFlights) {
                LoadOrImportAircraft();
            }
        }

        private void LoadOrImportAircraft()
        {
            OnTableChanged(new EventArgs<string>("Aircraft"));
            var progress = new ProgressEventArgs() {
                Caption =       ImportAircraft ? "Importing aircraft" : "Loading aircraft",
                TotalItems =    -1,
            };
            OnProgressChanged(progress);

            _AircraftMap.Clear();
            var allSource = Source.GetAllAircraft().ToDictionary(r => r.AircraftID, r => r);

            progress.TotalItems = allSource.Count;
            progress.CurrentItem = 0;
            OnProgressChanged(progress);

            if(!ImportAircraft) {
                var allDest = Destination.GetAllAircraft().ToDictionary(r => r.ModeS, r => r);
                foreach(var src in allSource) {
                    if(allDest.TryGetValue(src.Value.ModeS, out var dest)) {
                        _AircraftMap.Add(src.Value.AircraftID, dest.AircraftID);
                    }
                    ++progress.CurrentItem;
                    OnProgressChanged(progress);
                }
            } else {
                var upsertCandidates = new List<BaseStationAircraftUpsert>();
                var upsertKeys = new HashSet<string>();
                foreach(var kvp in allSource) {
                    if(!upsertKeys.Contains(kvp.Value.ModeS)) {
                        upsertCandidates.Add(new BaseStationAircraftUpsert(kvp.Value));
                        upsertKeys.Add(kvp.Value.ModeS);
                    }
                }
                upsertKeys.Clear();

                var upserted = Destination.UpsertManyAircraft(upsertCandidates).ToDictionary(r => r.ModeS, r => r);
                upsertCandidates.Clear();

                foreach(var sourceKvp in allSource) {
                    var sourceID = sourceKvp.Key;
                    if(upserted.TryGetValue(sourceKvp.Value.ModeS, out var rec)) {
                        _AircraftMap.Add(sourceID, rec.AircraftID);
                    }

                    ++progress.CurrentItem;
                    OnProgressChanged(progress);
                }
            }

            progress.CurrentItem = progress.TotalItems;
            OnProgressChanged(progress);
        }

        /// <summary>
        /// Imports flight records.
        /// </summary>
        private void ProcessFlights()
        {
            if(ImportFlights) {
                OnTableChanged(new EventArgs<string>("Flights"));
                var progress = new ProgressEventArgs() {
                    Caption =       "Importing flights",
                    TotalItems =    -1,
                };
                OnProgressChanged(progress);

                var criteria = new SearchBaseStationCriteria();
                if(EarliestFlightCriteria != null || LatestFlightCriteria != null) {
                    criteria.Date = new FilterRange<DateTime>() {
                        Condition =     FilterCondition.Between,
                        LowerValue =    EarliestFlightCriteria,
                        UpperValue =    LatestFlightCriteria,
                    };
                }

                var countFlights = Source.GetCountOfFlights(criteria);
                var countSource = 0;
                var countDest = 0;
                var startRow = 0;

                progress.TotalItems = countFlights;
                progress.CurrentItem = 0;
                OnProgressChanged(progress);

                while(startRow < countFlights) {
                    var allSource = Source.GetFlights(criteria, startRow, startRow + (FlightPageSize - 1), "DATE", true, null, false);
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

                    var upserted = Destination.UpsertManyFlights(upsertCandidates);
                    countDest += upserted.Length;
                    startRow += FlightPageSize;

                    progress.CurrentItem += allSource.Count;
                    OnProgressChanged(progress);
                }

                progress.CurrentItem = progress.TotalItems;
                OnProgressChanged(progress);
            }
        }
    }
}
