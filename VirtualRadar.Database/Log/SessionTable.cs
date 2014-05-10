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
using VirtualRadar.Interface.Database;
using System.Data;
using InterfaceFactory;
using System.IO;

namespace VirtualRadar.Database.Log
{
    /// <summary>
    /// The class that deals with accessing the Sessions table in the log database.
    /// </summary>
    class SessionTable : Table
    {
        #region Fields
        /// <summary>
        /// The fields that are fetched from the Session table.
        /// </summary>
        private const string AllFields = "[Id], [ClientId], [StartTime], [EndTime], [CountRequests], [OtherBytesSent], " +
                                         "[HtmlBytesSent], [JsonBytesSent], [ImageBytesSent], [AudioBytesSent]";
        #endregion

        #region Properties
        /// <summary>
        /// See base class docs.
        /// </summary>
        protected override string TableName { get { return "Session"; } }
        #endregion

        #region CreateTable
        /// <summary>
        /// See base class docs.
        /// </summary>
        /// <param name="connection"></param>
        public override void CreateTable(IDbConnection connection)
        {
            Sql.ExecuteNonQuery(connection, null, String.Format(
                "CREATE TABLE IF NOT EXISTS [{0}]" +
                "  ([Id] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL," +
                "   [ClientId] INTEGER NOT NULL," +
                "   [StartTime] DATETIME," +
                "   [EndTime] DATETIME," +
                "   [CountRequests] INTEGER," +
                "   [OtherBytesSent] INTEGER," +
                "   [HtmlBytesSent] INTEGER," +
                "   [JsonBytesSent] INTEGER," +
                "   [ImageBytesSent] INTEGER," +
                "   [AudioBytesSent] INTEGER)",
                TableName));

            Sql.ExecuteNonQuery(connection, null, String.Format(
                "CREATE INDEX IF NOT EXISTS [Idx_Session_ClientId]" +
                "  ON [{0}] ([ClientId] ASC)", TableName));

            Sql.ExecuteNonQuery(connection, null, String.Format(
                "CREATE INDEX IF NOT EXISTS [Idx_Session_StartTime]" +
                "  ON [{0}] ([StartTime] ASC)", TableName));

            Sql.ExecuteNonQuery(connection, null, String.Format(
                "CREATE INDEX IF NOT EXISTS [Idx_Session_EndTime]" +
                " ON [{0}] ([EndTime] ASC)", TableName));
        }
        #endregion

        #region Insert, Update, Delete
        /// <summary>
        /// Adds a new record to the table.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="session"></param>
        public void Insert(IDbConnection connection, IDbTransaction transaction, TextWriter log, LogSession session)
        {
            SqlPreparedCommand command = PrepareInsert(connection, transaction, "Insert",
                "Id",
                "ClientId", "StartTime", "EndTime", "CountRequests", "OtherBytesSent",
                "HtmlBytesSent", "JsonBytesSent", "ImageBytesSent", "AudioBytesSent");
            session.Id = Sql.ExecuteInsert(command, log,
                session.ClientId, session.StartTime, session.EndTime, session.CountRequests, session.OtherBytesSent,
                session.HtmlBytesSent, session.JsonBytesSent, session.ImageBytesSent, session.AudioBytesSent);
        }

        /// <summary>
        /// Updates an existing record.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="session"></param>
        public void Update(IDbConnection connection, IDbTransaction transaction, TextWriter log, LogSession session)
        {
            SqlPreparedCommand command = PrepareCommand(connection, transaction, "Update",
                String.Format("UPDATE [{0}] SET" +
                              " [ClientId] = ?, [StartTime] = ?, [EndTime] = ?, [CountRequests] = ?, " +
                              " [OtherBytesSent] = ?, [HtmlBytesSent] = ?, [JsonBytesSent] = ?, " +
                              " [ImageBytesSent] = ?, [AudioBytesSent] = ?" +
                              " WHERE [Id] = ?", TableName), 10);
            Sql.SetParameters(command, session.ClientId, session.StartTime, session.EndTime, session.CountRequests,
                              session.OtherBytesSent, session.HtmlBytesSent, session.JsonBytesSent,
                              session.ImageBytesSent, session.AudioBytesSent,
                              session.Id);
            Sql.LogCommand(log, command.Command);
            command.Command.ExecuteNonQuery();
        }

        /// <summary>
        /// Removes an existing record from the database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="session"></param>
        public void Delete(IDbConnection connection, IDbTransaction transaction, TextWriter log, LogSession session)
        {
            SqlPreparedCommand command = PrepareCommand(connection, transaction, "Delete",
                String.Format("DELETE FROM [{0}] WHERE [Id] = ?", TableName), 1);
            Sql.SetParameters(command, session.Id);
            Sql.LogCommand(log, command.Command);
            command.Command.ExecuteNonQuery();
        }
        #endregion

        #region SelectByStartDate, SelectAll
        /// <summary>
        /// Fetches a collection of records from the database whose start or end time was within the two times passed across.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="earliest"></param>
        /// <param name="latest"></param>
        /// <returns></returns>
        public List<LogSession> SelectByStartDate(IDbConnection connection, IDbTransaction transaction, TextWriter log, DateTime earliest, DateTime latest)
        {
            earliest = earliest.ToUniversalTime();
            latest = latest.ToUniversalTime();

            SqlPreparedCommand command = PrepareCommand(connection, transaction, "SelectByStartDate",
                String.Format("SELECT {0}" +
                              "  FROM [{1}]" +
                              "  WHERE (([StartTime] >= ? AND [StartTime] <= ?)" +
                              "         OR ([EndTime] >= ? AND [EndTime] <= ?)" +
                              "         OR ([StartTime] < ? AND [EndTime] > ?)" +
                              ")",
                              AllFields, TableName), 6);
            Sql.SetParameters(command, earliest, latest, earliest, latest, earliest, latest);
            Sql.LogCommand(log, command.Command);

            return PerformSelect(command);
        }

        /// <summary>
        /// Fetches a collection of every record from the database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public List<LogSession> SelectAll(IDbConnection connection, IDbTransaction transaction, TextWriter log)
        {
            SqlPreparedCommand command = PrepareCommand(connection, transaction, "SelectAll",
                String.Format("SELECT {0}" +
                              "  FROM [{1}]",
                              AllFields, TableName), 0);
            Sql.LogCommand(log, command.Command);

            return PerformSelect(command);
        }

        /// <summary>
        /// Executes a select command that returns a collection of records.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private List<LogSession> PerformSelect(SqlPreparedCommand command)
        {
            var result = new List<LogSession>();

            using(IDataReader reader = command.Command.ExecuteReader()) {
                while(reader.Read()) {
                    var session = new LogSession() {
                        Id = Sql.GetInt64(reader, "Id"),
                        ClientId = Sql.GetInt64(reader, "ClientId"),
                        StartTime = Sql.GetDateTime(reader, "StartTime"),
                        EndTime = Sql.GetDateTime(reader, "EndTime"),
                        CountRequests = Sql.GetInt64(reader, "CountRequests"),
                        OtherBytesSent = Sql.GetInt64(reader, "OtherBytesSent"),
                        HtmlBytesSent = Sql.GetInt64(reader, "HtmlBytesSent"),
                        JsonBytesSent = Sql.GetInt64(reader, "JsonBytesSent"),
                        ImageBytesSent = Sql.GetInt64(reader, "ImageBytesSent"),
                        AudioBytesSent = Sql.GetInt64(reader, "AudioBytesSent"),
                    };

                    result.Add(session);
                }
            }

            return result;
        }
        #endregion
    }
}
