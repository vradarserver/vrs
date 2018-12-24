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

namespace VirtualRadar.Interface.Database
{
    /// <summary>
    /// The interface for objects that can record full track histories to a database.
    /// </summary>
    public interface ITrackHistoryDatabase : ITransactionable
    {
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
        /// Returns all track histories for an ICAO, optionally constraining them to a date / time range.
        /// </summary>
        /// <param name="icao">The ICAO to search for.</param>
        /// <param name="startTimeInclusive">The optional start time. If null then the search runs from the beginning of time.</param>
        /// <param name="endTimeInclusive">The optional end time. If null then the search runs to the end of time.</param>
        /// <returns></returns>
        IEnumerable<TrackHistory> TrackHistory_GetByIcao(string icao, DateTime? startTimeInclusive, DateTime? endTimeInclusive);

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
    }
}
