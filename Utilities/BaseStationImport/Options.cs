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

namespace BaseStationImport
{
    /// <summary>
    /// The options passed in from the command-line.
    /// </summary>
    class Options
    {
        /// <summary>
        /// Gets or sets the command that the user wants to perform.
        /// </summary>
        public Command Command { get; set; }

        /// <summary>
        /// Gets or sets details of the source.
        /// </summary>
        public DatabaseEngineOptions Source { get; set; } = new DatabaseEngineOptions() { IsSource = true, };

        /// <summary>
        /// Gets or sets details of the destination.
        /// </summary>
        public DatabaseEngineOptions Destination { get; set; } = new DatabaseEngineOptions() { IsSource = false, };

        /// <summary>
        /// Gets or sets a value indicating that the schema should not be updated / created on the target
        /// before the import begins.
        /// </summary>
        public bool SuppressSchemaUpdate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether aircraft records are to be exported from <see cref="Source"/> to <see cref="Destination"/>.
        /// </summary>
        public bool ImportAircraft { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether session records are to be exported from <see cref="Source"/> to <see cref="Destination"/>.
        /// </summary>
        public bool ImportSessions { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether location records are to be exported from <see cref="Source"/> to <see cref="Destination"/>.
        /// </summary>
        public bool ImportLocations { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether flight records are to be exported from <see cref="Source"/> to <see cref="Destination"/>.
        /// </summary>
        public bool ImportFlights { get; set; } = true;

        /// <summary>
        /// Gets or sets the date of the earliest flight to import. Ignored if <see cref="ImportFlights"/> is false.
        /// </summary>
        public DateTime EarliestFlight { get; set; } = DateTime.MinValue.Date;

        /// <summary>
        /// Gets or sets the date of the last flight to import. Ignored if <see cref="ImportFlights"/> is false.
        /// </summary>
        public DateTime LatestFlight { get; set; } = DateTime.MaxValue.Date;
    }
}
