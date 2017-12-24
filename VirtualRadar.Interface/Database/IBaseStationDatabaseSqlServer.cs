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
    /// The SQL Server implementation of <see cref="IBaseStationDatabase"/>.
    /// </summary>
    /// <remarks>
    /// This is only implementated if the SQL Server plugin has been installed and loaded.
    /// </remarks>
    public interface IBaseStationDatabaseSqlServer : IBaseStationDatabase
    {
        /// <summary>
        /// Gets or sets the connection string to use.
        /// </summary>
        string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we are allowed to update the schema.
        /// </summary>
        bool CanUpdateSchema { get; set; }

        /// <summary>
        /// Gets or sets the duration that commands will run for before they time out.
        /// </summary>
        /// <remarks>
        /// A value of 0 indicates that commands will never time out. Note that there is no code support
        /// for connection timeouts because these can already be specified in the connection string.
        /// </remarks>
        int CommandTimeoutSeconds { get; set; }

        /// <summary>
        /// Applies the schema (if <see cref="CanUpdateSchema"/> is true) and returns the output from the schema update script.
        /// </summary>
        /// <returns></returns>
        string[] UpdateSchema();
    }
}
