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
using System.Text;

namespace VirtualRadar.Interface.KineticData
{
    /// <summary>
    /// The interface for objects that can deal with the BaseStation database file for us.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The BaseStation database is an SQLite file that Kinetic's BaseStation application creates and
    /// maintains. By default the object implementing the interface is in read-only mode, it will not make any
    /// changes to the database. In this mode attempts to use the insert / update or delete methods should
    /// throw an InvalidOperation exception. If the program sets <see cref="WriteSupportEnabled"/> then the
    /// insert / update and delete methods should allow writes to the database.
    /// </para>
    /// <para>
    /// Virtual Radar Server never sets <see cref="WriteSupportEnabled"/>, it will never write to the
    /// database. The write methods are only there for the use of plugins.
    /// </para>
    /// </remarks>
    public interface IBaseStationDatabase : ITransactionable, IDisposable
    {
        /// <summary>
        /// Gets the name of the database engine or library that the implementation is using.
        /// </summary>
        string Engine { get; }

        /// <summary>
        /// Gets a value indicating that there is an open connection to the database.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets or sets a flag indicating that methods that can create or modify the database are enabled. By default
        /// this setting is disabled. Changing this setting closes the current connection, the next call to access the
        /// database will reopen it.
        /// </summary>
        bool WriteSupportEnabled { get; set; }

        /// <summary>
        /// Gets the maximum number of parameters that can be passed to the underlying database engine. Note that calls that
        /// accept a variable number of parameters will automatically handle splitting the call into multiple calls on the
        /// database unless otherwise noted.
        /// </summary>
        /// <remarks>
        /// Ideally this would be read from the database engine but, in the case of SQLite, it's not so easy to get at the
        /// maximum number of parameters using just the ADO.NET provider. So this could be a bit arbitrary.
        /// </remarks>
        int MaxParameters { get; }

        /// <summary>
        /// Raised after an aircraft has been updated.
        /// </summary>
        event EventHandler<EventArgs<KineticAircraft>> AircraftUpdated;

        /// <summary>
        /// Takes an exception that was thrown from TestConnection and attempts to correct the error that
        /// was encountered. If it's likely to have succeeded then returns true.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        bool AttemptAutoFix(Exception ex);

        /// <summary>
        /// If the database file is missing or entirely empty then this method creates the file and pre-populates the
        /// tables with roughly the same records that BaseStation prepopulates a new database with.
        /// </summary>
        /// <param name="fileName">The name of the database file to create. This need not be the same as <see cref="FileName"/>.</param>
        /// <remarks>
        /// This does nothing if the database file exists and is not zero-length or if the database file is not set.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="WriteSupportEnabled"/> is false.</exception>
        void CreateDatabaseIfMissing(string fileName);

        /// <summary>
        /// Returns the first aircraft with the registration passed across.
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        KineticAircraft GetAircraftByRegistration(string registration);

        /// <summary>
        /// Returns the first aircraft with the ICAO24 code passed across.
        /// </summary>
        /// <param name="icao24"></param>
        /// <returns></returns>
        KineticAircraft GetAircraftByCode(string icao24);

        /// <summary>
        /// Returns aircraft records and counts of flights for many ICAO24 codes simultaneously.
        /// </summary>
        /// <param name="icao24s"></param>
        /// <returns></returns>
        Dictionary<string, KineticAircraftAndFlightsCount> GetManyAircraftAndFlightsCountByCode(IEnumerable<string> icao24s);

        /// <summary>
        /// Returns aircraft records for many ICAO24 codes simultaneously.
        /// </summary>
        /// <param name="icao24s"></param>
        /// <returns></returns>
        Dictionary<string, KineticAircraft> GetManyAircraftByCode(IEnumerable<string> icao24s);

        /// <summary>
        /// Returns every aircraft record in the database.
        /// </summary>
        /// <returns></returns>
        List<KineticAircraft> GetAllAircraft();

        /// <summary>
        /// Returns a list of every flight, or a subset of every flight, that matches the criteria passed across.
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
        List<KineticFlight> GetFlightsForAircraft(
            KineticAircraft aircraft,
            SearchBaseStationCriteria criteria,
            int fromRow,
            int toRow,
            string sort1,
            bool sort1Ascending,
            string sort2,
            bool sort2Ascending
        );

        /// <summary>
        /// Returns the number of flight records that match the criteria passed across.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        int GetCountOfFlightsForAircraft(KineticAircraft aircraft, SearchBaseStationCriteria criteria);

        /// <summary>
        /// Returns all flights, or a subset of all flights, that match the criteria passed across.
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="fromRow"></param>
        /// <param name="toRow"></param>
        /// <param name="sortField1"></param>
        /// <param name="sortField1Ascending"></param>
        /// <param name="sortField2"></param>
        /// <param name="sortField2Ascending"></param>
        /// <returns></returns>
        List<KineticFlight> GetFlights(
            SearchBaseStationCriteria criteria,
            int fromRow,
            int toRow,
            string sortField1,
            bool sortField1Ascending,
            string sortField2,
            bool sortField2Ascending
        );

        /// <summary>
        /// Returns the number of flights that match the criteria passed across.
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        int GetCountOfFlights(SearchBaseStationCriteria criteria);

/*
        /// <summary>
        /// Returns all of the records from BaseStation's DBHistory table.
        /// </summary>
        /// <returns></returns>
        IList<KineticDBHistory> GetDatabaseHistory();

        /// <summary>
        /// Returns the single DBInfo record in BaseStation's DBInfo table. Note that this has no key.
        /// </summary>
        /// <returns></returns>
        KineticDBInfo GetDatabaseVersion();

        /// <summary>
        /// Returns the entire content of the SystemEvents table.
        /// </summary>
        /// <returns></returns>
        IList<KineticSystemEvent> GetSystemEvents();

        /// <summary>
        /// Inserts a new SystemEvents record and sets the SystemEventsID to the identifier of the new record.
        /// </summary>
        /// <param name="systemEvent"></param>
        void InsertSystemEvent(KineticSystemEvent systemEvent);

        /// <summary>
        /// Updates an existing SystemEvents record.
        /// </summary>
        /// <param name="systemEvent"></param>
        void UpdateSystemEvent(KineticSystemEvent systemEvent);

        /// <summary>
        /// Deletes an existing SystemEvents record.
        /// </summary>
        /// <param name="systemEvent"></param>
        void DeleteSystemEvent(KineticSystemEvent systemEvent);

        /// <summary>
        /// Returns all of the locations from BaseStation's Locations table.
        /// </summary>
        /// <returns></returns>
        IList<KineticLocation> GetLocations();

        /// <summary>
        /// Inserts a new record in the database for the location passed across and sets the LocationID to the
        /// identifier of the new record.
        /// </summary>
        /// <param name="location"></param>
        void InsertLocation(KineticLocation location);

        /// <summary>
        /// Updates an existing location record.
        /// </summary>
        /// <param name="location"></param>
        void UpdateLocation(KineticLocation location);

        /// <summary>
        /// Deletes an existing location record.
        /// </summary>
        /// <param name="location"></param>
        void DeleteLocation(KineticLocation location);

        /// <summary>
        /// Returns all of the sessions from BaseStation's Sessions table.
        /// </summary>
        /// <returns></returns>
        IList<KineticSession> GetSessions();

        /// <summary>
        /// Inserts a record in the Sessions table, setting SessionID to the identifier of the new record.
        /// </summary>
        /// <param name="session"></param>
        void InsertSession(KineticSession session);

        /// <summary>
        /// Updates the record for a session.
        /// </summary>
        /// <param name="session"></param>
        void UpdateSession(KineticSession session);

        /// <summary>
        /// Deletes the record for a session. This automatically deletes all flights associated with the session.
        /// </summary>
        /// <param name="session"></param>
        void DeleteSession(KineticSession session);
*/

        /// <summary>
        /// Retrieves an aircraft record by its identifier.
        /// </summary>
        /// <param name="id"></param>
        KineticAircraft GetAircraftById(int id);

        /// <summary>
        /// Inserts a new aircraft record and fills AircraftID with the identifier of the record.
        /// </summary>
        /// <param name="aircraft"></param>
        void InsertAircraft(KineticAircraft aircraft);

        /// <summary>
        /// Fetches an aircraft by its ICAO code. If there is no record for the aircraft then a new bare-bones aircraft record is created.
        /// </summary>
        /// <param name="icao24"></param>
        /// <returns></returns>
        /// <remarks>
        /// A lock is held over both the fetch and the insert, this is an atomic operation. The bare-bones aircraft record has the ModeS,
        /// create and update times and ModeSCountry fields filled in.
        /// </remarks>
        KineticAircraft GetOrInsertAircraftByCode(string icao24, out bool created);

        /// <summary>
        /// Updates an existing aircraft record.
        /// </summary>
        /// <param name="aircraft"></param>
        void UpdateAircraft(KineticAircraft aircraft);

        /// <summary>
        /// Updates the Mode-S country for an aircraft.
        /// </summary>
        /// <param name="aircraftId"></param>
        /// <param name="modeSCountry"></param>
        void UpdateAircraftModeSCountry(int aircraftId, string modeSCountry);

        /// <summary>
        /// Records a missing aircraft record.
        /// </summary>
        /// <param name="icao"></param>
        /// <remarks>
        /// An aircraft is missing if it has no registration, no manufacturer, no model, no operator and the UserString1 is set to 'Missing'.
        /// If the aircraft record does not exist then it is created with the created and updated times set, along with the UserString1. If
        /// it exists and has the correct values for a missing aircraft (except for UserString1, that can be anything) then this method updates
        /// the time and forces UserString1 to Missing. Otherwise the method does nothing.
        /// </remarks>
        void RecordMissingAircraft(string icao);

        /// <summary>
        /// Does the same as <see cref="RecordMissingAircraft"/> but for many ICAOs simultaneously.
        /// </summary>
        /// <param name="icaos"></param>
        void RecordManyMissingAircraft(IEnumerable<string> icaos);

        /// <summary>
        /// Creates or updates an aircraft record, populating the details retrieved by online lookups.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="onlyUpdateIfMarkedAsMissing"></param>
        /// <returns></returns>
        KineticAircraft UpsertAircraftLookup(KineticAircraftLookup aircraft, bool onlyUpdateIfMarkedAsMissing);

        /// <summary>
        /// Does the same as <see cref="UpsertAircraft"/> but for many aircraft.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="onlyUpdateIfMarkedAsMissing"></param>
        /// <returns></returns>
        KineticAircraft[] UpsertManyAircraftLookup(IEnumerable<KineticAircraftLookup> aircraft, bool onlyUpdateIfMarkedAsMissing);

        /// <summary>
        /// Creates or updates full-size aircraft records.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <returns></returns>
        KineticAircraft[] UpsertManyAircraft(IEnumerable<KineticAircraftKeyless> aircraft);

        /// <summary>
        /// Deletes an existing aircraft record.
        /// </summary>
        /// <param name="aircraft"></param>
        void DeleteAircraft(KineticAircraft aircraft);

        /// <summary>
        /// Retrieves a flight record from the database by its ID number. This does not read the associated aircraft record.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        KineticFlight GetFlightById(int id);

/*
        /// <summary>
        /// Inserts a new flight record and assigns the unique identifier of the new record to the FlightID property. The AircraftID
        /// property must be filled with the identifier of an existing aircraft record.
        /// </summary>
        /// <param name="flight"></param>
        void InsertFlight(KineticFlight flight);

        /// <summary>
        /// Updates an existing flight record. Ignores the aircraft record attached to the flight (if any).
        /// </summary>
        /// <param name="flight"></param>
        void UpdateFlight(KineticFlight flight);

        /// <summary>
        /// Deletes an existing flight record. Ignores the aircraft record attached to the flight (if any).
        /// </summary>
        /// <param name="flight"></param>
        void DeleteFlight(KineticFlight flight);

        /// <summary>
        /// Inserts or updates many flights in one go.
        /// </summary>
        /// <param name="flights"></param>
        /// <returns></returns>
        KineticFlight[] UpsertManyFlights(IEnumerable<KineticFlightKeyless> flights);
*/
    }
}
