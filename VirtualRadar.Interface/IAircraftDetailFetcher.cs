// Copyright © 2014 onwards, Andrew Whewell
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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The interface for a singleton class that periodically fetches all aircraft
    /// details from the database.
    /// </summary>
    /// <remarks><para>
    /// Each aircraft list should call <see cref="RegisterAircraft"/> whenever it gets
    /// a message from an aircraft. The call will always return quickly, it never triggers
    /// a direct interrogation of the database. If no register calls are made within a
    /// certain period of time the aircraft is no longer monitored for detail changes.
    /// </para><para>
    /// If an event handler raises an exception then it will be logged and it will not be
    /// shown to the user. Event handlers should avoid throwing exceptions.
    /// </para></remarks>
    public interface IAircraftDetailFetcher : ISingleton<IAircraftDetailFetcher>
    {
        /// <summary>
        /// Raised when new aircraft details have been fetched for an aircraft.
        /// </summary>
        /// <remarks>
        /// Note that many aircraft lists may be registering aircraft with the fetcher and all of those
        /// aircraft can trigger events. Do not assume that only the aircraft that you registered will
        /// have events raised for it.
        /// </remarks>
        event EventHandler<EventArgs<AircraftDetail>> Fetched;

        /// <summary>
        /// Registers an aircraft as being tracked.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <returns>The aircraft details if they're already known, or null if this is the first time the aircraft
        /// has been added or the aircraft is known to have no details recorded on the database.</returns>
        /// <remarks><para>
        /// Each aircraft list should call this function to keep the aircraft monitored for database
        /// changes. If the fetcher does not receive a RegisterAircraft call within a couple of minutes then it
        /// stops monitoring the aircraft's database record.
        /// </para><para>
        /// If the aircraft has already been registered and its database details never change then objects that
        /// make further calls to RegisterAircraft for the aircraft will not see events raised for it. These objects
        /// must assume that the return value represents the correct database details for the aircraft.
        /// </para>
        /// </remarks>
        AircraftDetail RegisterAircraft(IAircraft aircraft);
    }
}
