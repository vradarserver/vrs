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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The interface for objects that can collect together information from the messages transmitted by
    /// aircraft, and lookups in various data sources, to maintain a list of aircraft and their current state.
    /// </summary>
    public interface IAircraftList : IBackgroundThreadExceptionCatcher, IDisposable
    {
        /// <summary>
        /// Gets a value indicating where the aircraft messages are coming from.
        /// </summary>
        AircraftListSource Source { get; }

        /// <summary>
        /// Gets the total number of aircraft currently being tracked. 
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets a value indicating that the aircraft list is tracking aircraft.
        /// </summary>
        bool IsTracking { get; }

        /// <summary>
        /// Raised when the count of aircraft has changed. This could be raised from any thread, it needn't be raised on a GUI thread.
        /// </summary>
        event EventHandler CountChanged;

        /// <summary>
        /// Raised when the aircraft list is started or stopped. Could be raised from any thread.
        /// </summary>
        event EventHandler TrackingStateChanged;

        /// <summary>
        /// Starts the tracking of aircraft.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops tracking aircraft.
        /// </summary>
        void Stop();

        /// <summary>
        /// Returns details of the aircraft with the unique ID passed across or null if no such aircraft exists. The aircraft
        /// returned is a clone of the orignal, it will not change as further messages are received from the aircraft.
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        /// <remarks>
        /// Implementations should return a clone of the aircraft object whose properties will remain constant even if
        /// the original <see cref="IAircraft"/> continues to be updated to keep track with the source of
        /// aircraft data.
        /// </remarks>
        IAircraft FindAircraft(int uniqueId);

        /// <summary>
        /// Returns a list of all of the aircraft currently being tracked. The aircraft objects are clones of the
        /// originals held by the list, they will not change as further messages are received from the aircraft.
        /// </summary>
        /// <param name="snapshotTimeStamp"></param>
        /// <param name="snapshotDataVersion"></param>
        /// <returns></returns>
        /// <remarks>
        /// Implementations should return clones of the aircraft objects whose properties will remain constant even if
        /// the original <see cref="IAircraft"/> continues to be updated to keep track with the source of
        /// aircraft data.
        /// </remarks>
        List<IAircraft> TakeSnapshot(out long snapshotTimeStamp, out long snapshotDataVersion);
    }
}
