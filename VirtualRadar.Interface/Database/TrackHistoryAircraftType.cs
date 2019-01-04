// Copyright © 2019 onwards, Andrew Whewell
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
    /// Describes an aircraft type record in the track history database.
    /// </summary>
    public class TrackHistoryAircraftType
    {
        /// <summary>
        /// Gets or sets the unique ID for the aircraft type record.
        /// </summary>
        public int AircraftTypeID { get; set; }

        /// <summary>
        /// Gets or sets the ICAO code for the aircraft type.
        /// </summary>
        public string Icao { get; set; }

        /// <summary>
        /// Gets or sets the ID of the <see cref="TrackHistoryManufacturer"/> record associated with the aircraft type.
        /// </summary>
        public int? ManufacturerID { get; set; }

        /// <summary>
        /// Gets or sets the ID of the <see cref="TrackHistoryModel"/> record associated with the aircraft type.
        /// </summary>
        public int? ModelID { get; set; }

        /// <summary>
        /// Gets or sets the ID of the <see cref="TrackHistoryEngineType"/> record associated with the aircraft type.
        /// </summary>
        public EngineType? EngineTypeID { get; set; }

        /// <summary>
        /// Gets or sets the ID of the <see cref="TrackHistoryEnginePlacement"/> record associated with the aircraft type.
        /// </summary>
        public EnginePlacement? EnginePlacementID { get; set; }

        /// <summary>
        /// Gets or sets the ID of the <see cref="TrackHistoryWakeTurbulenceCategory"/> record associated with the aircraft type.
        /// </summary>
        public WakeTurbulenceCategory? WakeTurbulenceCategoryID { get; set; }

        /// <summary>
        /// Gets or sets the number of engines (note that this is a string, one of the offical engine counts is 'C').
        /// </summary>
        public string EngineCount { get; set; }

        /// <summary>
        /// Gets or sets the time that the record was created.
        /// </summary>
        public DateTime CreatedUtc { get; set; }

        /// <summary>
        /// Gets or sets the time that the record was last updated.
        /// </summary>
        public DateTime UpdatedUtc { get; set; }
    }
}
