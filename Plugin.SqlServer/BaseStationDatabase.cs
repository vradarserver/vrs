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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;

namespace VirtualRadar.Plugin.SqlServer
{
    /// <summary>
    /// The SQL Server implementation of <see cref="IBaseStationDatabase"/>.
    /// </summary>
    class BaseStationDatabase : IBaseStationDatabase
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IBaseStationDatabaseProvider Provider { get; set; } = new BaseStationDatabaseProvider();

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string FileName
        {
            get => SqlServerStrings.SeeSqlServerPluginOptions;
            set {;}
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string LogFileName
        {
            get => SqlServerStrings.SeeSqlServerPluginOptions;
            set {;}
        }

        public bool IsConnected => throw new NotImplementedException();

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool WriteSupportEnabled { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int MaxParameters => 2000;

        /// <summary>
        /// Unused.
        /// </summary>
        public event EventHandler FileNameChanging;

        /// <summary>
        /// Unused.
        /// </summary>
        public event EventHandler FileNameChanged;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<BaseStationAircraft>> AircraftUpdated;

        /// <summary>
        /// Raises <see cref="AircraftUpdated"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnAircraftUpdated(EventArgs<BaseStationAircraft> args)
        {
            EventHelper.Raise(AircraftUpdated, this, () => args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public bool AttemptAutoFix(Exception ex) => false;

        public void CreateDatabaseIfMissing(string fileName)
        {
            throw new NotImplementedException();
        }

        public void DeleteAircraft(BaseStationAircraft aircraft)
        {
            throw new NotImplementedException();
        }

        public void DeleteFlight(BaseStationFlight flight)
        {
            throw new NotImplementedException();
        }

        public void DeleteLocation(BaseStationLocation location)
        {
            throw new NotImplementedException();
        }

        public void DeleteSession(BaseStationSession session)
        {
            throw new NotImplementedException();
        }

        public void DeleteSystemEvent(BaseStationSystemEvents systemEvent)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void EndTransaction()
        {
            throw new NotImplementedException();
        }

        public BaseStationAircraft GetAircraftByCode(string icao24)
        {
            throw new NotImplementedException();
        }

        public BaseStationAircraft GetAircraftById(int id)
        {
            throw new NotImplementedException();
        }

        public BaseStationAircraft GetAircraftByRegistration(string registration)
        {
            throw new NotImplementedException();
        }

        public List<BaseStationAircraft> GetAllAircraft()
        {
            throw new NotImplementedException();
        }

        public int GetCountOfFlights(SearchBaseStationCriteria criteria)
        {
            throw new NotImplementedException();
        }

        public int GetCountOfFlightsForAircraft(BaseStationAircraft aircraft, SearchBaseStationCriteria criteria)
        {
            throw new NotImplementedException();
        }

        public IList<BaseStationDBHistory> GetDatabaseHistory()
        {
            throw new NotImplementedException();
        }

        public BaseStationDBInfo GetDatabaseVersion()
        {
            throw new NotImplementedException();
        }

        public BaseStationFlight GetFlightById(int id)
        {
            throw new NotImplementedException();
        }

        public List<BaseStationFlight> GetFlights(SearchBaseStationCriteria criteria, int fromRow, int toRow, string sortField1, bool sortField1Ascending, string sortField2, bool sortField2Ascending)
        {
            throw new NotImplementedException();
        }

        public List<BaseStationFlight> GetFlightsForAircraft(BaseStationAircraft aircraft, SearchBaseStationCriteria criteria, int fromRow, int toRow, string sort1, bool sort1Ascending, string sort2, bool sort2Ascending)
        {
            throw new NotImplementedException();
        }

        public IList<BaseStationLocation> GetLocations()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, BaseStationAircraftAndFlightsCount> GetManyAircraftAndFlightsCountByCode(IEnumerable<string> icao24s)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, BaseStationAircraft> GetManyAircraftByCode(IEnumerable<string> icao24s)
        {
            throw new NotImplementedException();
        }

        public BaseStationAircraft GetOrInsertAircraftByCode(string icao24, Func<string, BaseStationAircraft> createNewAircraftFunc)
        {
            throw new NotImplementedException();
        }

        public IList<BaseStationSession> GetSessions()
        {
            throw new NotImplementedException();
        }

        public IList<BaseStationSystemEvents> GetSystemEvents()
        {
            throw new NotImplementedException();
        }

        public void InsertAircraft(BaseStationAircraft aircraft)
        {
            throw new NotImplementedException();
        }

        public void InsertFlight(BaseStationFlight flight)
        {
            throw new NotImplementedException();
        }

        public void InsertLocation(BaseStationLocation location)
        {
            throw new NotImplementedException();
        }

        public void InsertSession(BaseStationSession session)
        {
            throw new NotImplementedException();
        }

        public void InsertSystemEvent(BaseStationSystemEvents systemEvent)
        {
            throw new NotImplementedException();
        }

        public void RecordManyMissingAircraft(IEnumerable<string> icaos)
        {
            throw new NotImplementedException();
        }

        public void RecordMissingAircraft(string icao)
        {
            throw new NotImplementedException();
        }

        public void RollbackTransaction()
        {
            throw new NotImplementedException();
        }

        public void StartTransaction()
        {
            throw new NotImplementedException();
        }

        public bool TestConnection()
        {
            throw new NotImplementedException();
        }

        public void UpdateAircraft(BaseStationAircraft aircraft)
        {
            throw new NotImplementedException();
        }

        public void UpdateAircraftModeSCountry(int aircraftId, string modeSCountry)
        {
            throw new NotImplementedException();
        }

        public void UpdateFlight(BaseStationFlight flight)
        {
            throw new NotImplementedException();
        }

        public void UpdateLocation(BaseStationLocation location)
        {
            throw new NotImplementedException();
        }

        public void UpdateSession(BaseStationSession session)
        {
            throw new NotImplementedException();
        }

        public void UpdateSystemEvent(BaseStationSystemEvents systemEvent)
        {
            throw new NotImplementedException();
        }

        public BaseStationAircraft UpsertAircraftByCode(string icao, Func<BaseStationAircraft, BaseStationAircraft> fillAircraft)
        {
            throw new NotImplementedException();
        }

        public BaseStationAircraft[] UpsertManyAircraftByCodes(IEnumerable<string> icaos, Func<BaseStationAircraft, BaseStationAircraft> fillAircraft)
        {
            throw new NotImplementedException();
        }
    }
}
