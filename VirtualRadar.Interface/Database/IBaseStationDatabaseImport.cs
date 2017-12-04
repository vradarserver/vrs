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

namespace VirtualRadar.Interface.Database
{
    /// <summary>
    /// The interface for objects that can import from SQLite BaseStation.sqb files to a
    /// non-SQLite implementation of <see cref="IBaseStationDatabase"/>.
    /// </summary>
    public interface IBaseStationDatabaseImport
    {
        /// <summary>
        /// Gets or sets the full path and filename of the BaseStation.sqb to import from.
        /// </summary>
        string BaseStationFileName { get; set; }

        /// <summary>
        /// Gets or sets the full path and filename of the optional log file to write to.
        /// </summary>
        string LogFileName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the log should be appended to rather than overwritten.
        /// </summary>
        bool AppendLog { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the aircraft should be imported.
        /// </summary>
        bool ImportAircraft { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that sessions should be imported.
        /// </summary>
        /// <remarks>
        /// Sessions rely on locations, if locations are not imported then sessions will
        /// not be imported either.
        /// </remarks>
        bool ImportSessions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that locations should be imported.
        /// </summary>
        bool ImportLocations { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that flights should be imported.
        /// </summary>
        /// <remarks>
        /// Flights rely on sessions and aircraft, if either of those are not imported then
        /// flights will not be imported either.
        /// </remarks>
        bool ImportFlights { get; set; }

        /// <summary>
        /// Performs the import.
        /// </summary>
        void Import();
    }
}
