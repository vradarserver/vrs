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
using System.Linq;
using System.Text;
using InterfaceFactory;

namespace VirtualRadar.Interface.Database
{
    /// <summary>
    /// The interface to the log database.
    /// </summary>
    /// <remarks>
    /// The log database is an SQLite database that Virtual Radar Server maintains for itself in its configuration folder. It holds
    /// information about the connections that have been made to the web server.
    /// </remarks>
    [Singleton]
    public interface ILogDatabase : ITransactionable, IDisposable
    {
        /// <summary>
        /// Gets or sets the provider that abstracts away the environment for testing.
        /// </summary>
        ILogDatabaseProvider Provider { get; set; }

        /// <summary>
        /// Creates a new session record for the IP address passed across.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        LogSession EstablishSession(string ipAddress, string userName);

        /// <summary>
        /// Writes the session record passed across back to the database.
        /// </summary>
        /// <param name="session"></param>
        void UpdateSession(LogSession session);

        /// <summary>
        /// Writes the client back to the database.
        /// </summary>
        /// <param name="client"></param>
        void UpdateClient(LogClient client);

        /// <summary>
        /// Fetches all sessions that start and end within the date range passed across.
        /// </summary>
        /// <param name="clients"></param>
        /// <param name="sessions"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        void FetchSessions(IList<LogClient> clients, IList<LogSession> sessions, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Fetches the entire log.
        /// </summary>
        /// <param name="clients"></param>
        /// <param name="sessionsMap">A map of client ID to all sessions for the client.</param>
        void FetchAll(IList<LogClient> clients, IDictionary<long, IList<LogSession>> sessionsMap);
    }
}
