// Copyright © 2018 onwards, Andrew Whewell
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
using VirtualRadar.Interface.StandingData;

namespace VirtualRadar.Interface.Database
{
    /// <summary>
    /// The interface for objects that can record full track histories to a database. Do not resolve
    /// direct references to this in the application, use <see cref="ITrackHistoryDatabaseSingleton"/>
    /// instead.
    /// </summary>
    /// <remarks>
    /// Implementations of the interface should fail gracefully if they have not yet been fully initialised.
    /// </remarks>
    public interface ITrackHistoryDatabase : ITransactionable
    {
        #region Properties
        /// <summary>
        /// True if the <see cref="FileName"/> or <see cref="ConnectionString"/> is taken from some other source and the
        /// implementation will ignore both properties. The configuration UI should not ask for a data source if this is set.
        /// </summary>
        bool IsDataSourceReadOnly { get; }

        /// <summary>
        /// True if the database is stored in a single file whose name is expected to be in <see cref="FileName"/>, false if it
        /// uses a connection string in <see cref="ConnectionString"/>.
        /// </summary>
        bool FileNameRequired { get; }

        /// <summary>
        /// Gets or sets the full path to the database file. Only used if <see cref="FileNameRequired"/> is true.
        /// </summary>
        string FileName { get; set; }

        /// <summary>
        /// Gets or sets the connection string to the database. Only used if <see cref="FileNameRequired"/> is false.
        /// </summary>
        string ConnectionString { get; set; }
        #endregion

        #region Schema
        /// <summary>
        /// Creates a new instance of a database. The meaning of <paramref name="dataSource"/> depends on whether
        /// <see cref="FileNameRequired"/> is true or false.
        /// </summary>
        /// <param name="dataSource"></param>
        void Create(string dataSource);
        #endregion

        #region Aircraft
        /// <summary>
        /// Returns the aircraft record for the ID passed across or null if no such aircraft exists.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TrackHistoryAircraft Aircraft_GetByID(long id);

        /// <summary>
        /// Returns the aircraft record for the ICAO passed across or null if no such aircraft exists.
        /// </summary>
        /// <param name="icao"></param>
        /// <returns></returns>
        TrackHistoryAircraft Aircraft_GetByIcao(string icao);

        /// <summary>
        /// Creates or updates the aircraft record passed across.
        /// </summary>
        /// <param name="aircraft"></param>
        void Aircraft_Save(TrackHistoryAircraft aircraft);

        /// <summary>
        /// Deletes the aircraft passed across.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <remarks>
        /// This automatically deletes all <see cref="TrackHistory"/> records attached to the aircraft.
        /// </remarks>
        void Aircraft_Delete(TrackHistoryAircraft aircraft);
        #endregion

        #region AircraftType
        /// <summary>
        /// Returns the aircraft type for the ID passed across or null if no such record exists.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TrackHistoryAircraftType AircraftType_GetByID(int id);

        /// <summary>
        /// Saves an aircraft type record.
        /// </summary>
        /// <param name="aircraftType"></param>
        void AircraftType_Save(TrackHistoryAircraftType aircraftType);

        /// <summary>
        /// Deletes an existing aircraft type and nulls out references to it.
        /// </summary>
        /// <param name="aircraftType"></param>
        void AircraftType_Delete(TrackHistoryAircraftType aircraftType);
        #endregion

        #region Country
        /// <summary>
        /// Returns the country record for the ID passed across or null if no such country exists.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TrackHistoryCountry Country_GetByID(int id);

        /// <summary>
        /// Returns the country for the case insensitive name passed across or null if no such country exists.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        TrackHistoryCountry Country_GetByName(string name);

        /// <summary>
        /// Returns the country for the case insensitive name passed across. If no such country exists then a new one is created
        /// and returned.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        TrackHistoryCountry Country_GetOrCreateByName(string name);

        /// <summary>
        /// Creates or updates the country passed across.
        /// </summary>
        /// <param name="country"></param>
        void Country_Save(TrackHistoryCountry country);

        /// <summary>
        /// Deletes the country passed across.
        /// </summary>
        /// <param name="country"></param>
        void Country_Delete(TrackHistoryCountry country);
        #endregion

        #region EnginePlacement
        /// <summary>
        /// Returns the engine placement record for the standing data EnginePlacement passed across or null if no such record exists.
        /// </summary>
        /// <param name="enginePlacement"></param>
        /// <returns></returns>
        TrackHistoryEnginePlacement EnginePlacement_GetByID(EnginePlacement enginePlacement);

        /// <summary>
        /// Returns all of the engine placement records on the database.
        /// </summary>
        /// <returns></returns>
        IEnumerable<TrackHistoryEnginePlacement> EnginePlacement_GetAll();
        #endregion

        #region EngineType
        /// <summary>
        /// Returns the engine type record for the standing data EngineType passed across or null if no such record exists.
        /// </summary>
        /// <param name="engineType"></param>
        /// <returns></returns>
        TrackHistoryEngineType EngineType_GetByID(EngineType engineType);

        /// <summary>
        /// Returns all of the engine type records on the database.
        /// </summary>
        /// <returns></returns>
        IEnumerable<TrackHistoryEngineType> EngineType_GetAll();
        #endregion

        #region Manufacturer
        /// <summary>
        /// Returns the manufacturer record for the ID passed across or null if no such manufacturer exists.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TrackHistoryManufacturer Manufacturer_GetByID(int id);

        /// <summary>
        /// Returns the manufacturer for the case insensitive name passed across or null if no such manufacturer exists.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        TrackHistoryManufacturer Manufacturer_GetByName(string name);

        /// <summary>
        /// Returns the manufacturer for the case insensitive name passed across. If no such manufacturer exists then a new one is created
        /// and returned.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        TrackHistoryManufacturer Manufacturer_GetOrCreateByName(string name);

        /// <summary>
        /// Creates or updates the manufacturer passed across.
        /// </summary>
        /// <param name="manufacturer"></param>
        void Manufacturer_Save(TrackHistoryManufacturer manufacturer);

        /// <summary>
        /// Deletes the manufacturer passed across.
        /// </summary>
        /// <param name="manufacturer"></param>
        void Manufacturer_Delete(TrackHistoryManufacturer manufacturer);
        #endregion

        #region Model
        /// <summary>
        /// Returns the model record for the ID passed across or null if no such model exists.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TrackHistoryModel Model_GetByID(int id);

        /// <summary>
        /// Returns the model for the case insensitive name passed across or null if no such model exists.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        TrackHistoryModel Model_GetByName(string name);

        /// <summary>
        /// Returns the model for the case insensitive name passed across. If no such model exists then a new one is created
        /// and returned.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        TrackHistoryModel Model_GetOrCreateByName(string name);

        /// <summary>
        /// Creates or updates the model passed across.
        /// </summary>
        /// <param name="model"></param>
        void Model_Save(TrackHistoryModel model);

        /// <summary>
        /// Deletes the model passed across.
        /// </summary>
        /// <param name="model"></param>
        void Model_Delete(TrackHistoryModel model);
        #endregion

        #region Operator
        /// <summary>
        /// Returns the operator for the ID passed across or null if no such operator exists.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TrackHistoryOperator Operator_GetByID(int id);

        /// <summary>
        /// Returns the operator matching the unique key passed across or null if no such operator exists.
        /// </summary>
        /// <param name="icao"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        TrackHistoryOperator Operator_GetByKey(string icao, string name);

        /// <summary>
        /// Returns the operator matching the unique key passed across or creates and returns the operator
        /// if it does not already exist.
        /// </summary>
        /// <param name="icao"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        TrackHistoryOperator Operator_GetOrCreateByKey(string icao, string name);

        /// <summary>
        /// Creates or updates the operator passed across.
        /// </summary>
        /// <param name="op"></param>
        void Operator_Save(TrackHistoryOperator acOperator);

        /// <summary>
        /// Deletes the operator passed across. References to the operator are nulled out.
        /// </summary>
        /// <param name="acOperator"></param>
        void Operator_Delete(TrackHistoryOperator acOperator);
        #endregion

        #region Receiver
        /// <summary>
        /// Returns the receiver for the ID passed across or null if no such receiver exists.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TrackHistoryReceiver Receiver_GetByID(int id);

        /// <summary>
        /// Returns the receiver for the case insensitive name passed across or null if no such receiver exists.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        TrackHistoryReceiver Receiver_GetByName(string name);

        /// <summary>
        /// Returns the receiver for the case insensitive name passed across. If no such receiver exists then a new one is created
        /// and returned.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        TrackHistoryReceiver Receiver_GetOrCreateByName(string name);

        /// <summary>
        /// Creates or updates the receiver passed across.
        /// </summary>
        /// <param name="receiver"></param>
        void Receiver_Save(TrackHistoryReceiver receiver);

        /// <summary>
        /// Deletes the receiver passed across.
        /// </summary>
        /// <param name="receiver"></param>
        /// <remarks>
        /// References to the receiver in track history states will be nulled out. If a track involves more than one receiver then
        /// this could make it appear that parts of the track were attributable to the wrong receiver. For example if you have two
        /// states, the first refers to receiver A and the second to receiver B, and then you delete receiver B then the receiver
        /// entry in the second state will be nulled out, making it appear that the last non-null receiver in the state history (A)
        /// continues to be valid for the state.
        /// </remarks>
        void Receiver_Delete(TrackHistoryReceiver receiver);
        #endregion

        #region Species
        /// <summary>
        /// Returns the species record for the standing data species passed across or null if no such record exists.
        /// </summary>
        /// <param name="species"></param>
        /// <returns></returns>
        TrackHistorySpecies Species_GetByID(Species species);

        /// <summary>
        /// Returns all of the species records on the database.
        /// </summary>
        /// <returns></returns>
        IEnumerable<TrackHistorySpecies> Species_GetAll();
        #endregion

        #region TrackHistory
        /// <summary>
        /// Returns all track histories within a date / time range.
        /// </summary>
        /// <param name="startTimeInclusive">The optional start time. If null then the search runs from the beginning of time.</param>
        /// <param name="endTimeInclusive">The optional end time. If null then the search runs to the end of time.</param>
        /// <returns></returns>
        IEnumerable<TrackHistory> TrackHistory_GetByDateRange(DateTime? startTimeInclusive, DateTime? endTimeInclusive);

        /// <summary>
        /// Returns a single track history for the ID passed across or null if no such record exists.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TrackHistory TrackHistory_GetByID(long id);

        /// <summary>
        /// Returns all track histories for an aircraft, optionally constraining them to a date / time range.
        /// </summary>
        /// <param name="aircraftID">The aircraft ID to search for.</param>
        /// <param name="startTimeInclusive">The optional start time. If null then the search runs from the beginning of time.</param>
        /// <param name="endTimeInclusive">The optional end time. If null then the search runs to the end of time.</param>
        /// <returns></returns>
        IEnumerable<TrackHistory> TrackHistory_GetByAircraftID(long aircraftID, DateTime? startTimeInclusive, DateTime? endTimeInclusive);

        /// <summary>
        /// Creates or updates a track history record.
        /// </summary>
        /// <param name="trackHistory"></param>
        void TrackHistory_Save(TrackHistory trackHistory);

        /// <summary>
        /// Deletes the track history passed across. This will delete the history even if it is marked as preserved.
        /// </summary>
        /// <param name="trackHistory"></param>
        /// <returns></returns>
        TrackHistoryTruncateResult TrackHistory_Delete(TrackHistory trackHistory);

        /// <summary>
        /// Removes all track histories older than or equal to <paramref name="deleteUpToUtc"/> unless they are marked as preserved.
        /// </summary>
        /// <param name="deleteUpToUtc"></param>
        /// <returns></returns>
        TrackHistoryTruncateResult TrackHistory_DeleteExpired(DateTime deleteUpToUtc);

        /// <summary>
        /// Truncates states from the track history passed across unless it is marked as preserved.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newUpdatedUtc"></param>
        /// <returns></returns>
        TrackHistoryTruncateResult TrackHistory_Truncate(TrackHistory trackHistory, DateTime newUpdatedUtc);

        /// <summary>
        /// Removes all state except for the first and last records for all track histories older than or equal to <see cref="truncateUpToUtc"/>
        /// unless they are marked as preserved.
        /// </summary>
        /// <param name="truncateUpToUtc"></param>
        /// <param name="newUpdatedUtc"></param>
        /// <returns></returns>
        TrackHistoryTruncateResult TrackHistory_TruncateExpired(DateTime truncateUpToUtc, DateTime newUpdatedUtc);
        #endregion

        #region TrackHistoryState
        /// <summary>
        /// Returns the state record for the ID passed across or null if it does not exist.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TrackHistoryState TrackHistoryState_GetByID(long id);

        /// <summary>
        /// Returns all of the state records for a track history in ascending date/time order.
        /// </summary>
        /// <param name="trackHistory"></param>
        /// <returns></returns>
        IEnumerable<TrackHistoryState> TrackHistoryState_GetByTrackHistory(TrackHistory trackHistory);

        /// <summary>
        /// Creates or updates a track history state record.
        /// </summary>
        /// <param name="trackHistoryState"></param>
        void TrackHistoryState_Save(TrackHistoryState trackHistoryState);

        /// <summary>
        /// Creates or updates many track history states at once.
        /// </summary>
        /// <param name="trackHistoryStates"></param>
        void TrackHistoryState_SaveMany(IEnumerable<TrackHistoryState> trackHistoryStates);
        #endregion

        #region WakeTurbulenceCategory
        /// <summary>
        /// Returns the wake turbulence category record for the standing data WakeTurbulenceCategory passed across
        /// or null if no such record exists.
        /// </summary>
        /// <param name="wakeTurbulenceCategory"></param>
        /// <returns></returns>
        TrackHistoryWakeTurbulenceCategory WakeTurbulenceCategory_GetByID(WakeTurbulenceCategory wakeTurbulenceCategory);

        /// <summary>
        /// Returns all of the wake turbulence category records on the database.
        /// </summary>
        /// <returns></returns>
        IEnumerable<TrackHistoryWakeTurbulenceCategory> WakeTurbulenceCategory_GetAll();
        #endregion
    }
}
