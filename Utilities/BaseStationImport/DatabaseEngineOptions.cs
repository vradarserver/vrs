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
    /// Describes the options associated with a source or destination database engine.
    /// </summary>
    class DatabaseEngineOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the engine represents the source of data.
        /// </summary>
        public bool IsSource { get; set; }

        /// <summary>
        /// Gets a value indicating whether the engine represents the destination for the data.
        /// </summary>
        public bool IsTarget => !IsSource;

        /// <summary>
        /// Gets or sets the database engine in use.
        /// </summary>
        public DatabaseEngine Engine { get; set; }

        /// <summary>
        /// Gets or sets the filename or connection string to use.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the command timeout in seconds, or null if the default is to be used.
        /// </summary>
        /// <remarks>
        /// Note that depending on the plugin involved the default might be whatever the user
        /// configured in VRS and not the provider default. Also note that the SQLite version
        /// of IBaseStationDatabase does not support command timeouts.
        /// </remarks>
        public int? CommandTimeoutSeconds { get; set; }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"[{Engine}] {ConnectionString}";
        }
    }
}
