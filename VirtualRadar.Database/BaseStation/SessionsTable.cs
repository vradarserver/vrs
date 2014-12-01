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
using System.Data;
using System.IO;
using VirtualRadar.Interface.Database;

namespace VirtualRadar.Database.BaseStation
{
    /// <summary>
    /// A class that deals with the ADO.NET calls on the Sessions table in the BaseStation database.
    /// </summary>
    class SessionsTable : Table
    {
        private string _GetAllRecordsCommandText = "SELECT [SessionID], [LocationID], [StartTime], [EndTime] FROM [Sessions]";
        private string _UpdateCommandText = "UPDATE [Sessions] SET [LocationID] = ?, [StartTime] = ?, [EndTime] = ? WHERE [SessionID] = ?";
        private string _DeleteCommandText = "DELETE FROM [Sessions] WHERE [SessionID] = ?";

        /// <summary>
        /// See base class.
        /// </summary>
        protected override string TableName { get { return "Sessions"; } }

        /// <summary>
        /// See base class.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="log"></param>
        public override void CreateTable(IDbConnection connection, TextWriter log)
        {
            Sql.ExecuteNonQuery(connection, null, log, String.Format(
                "CREATE TABLE IF NOT EXISTS [{0}]" +
                "  ([SessionID] integer primary key," +
                "   [LocationID] integer not null," +
                "   [StartTime] datetime not null," +
                "   [EndTime] datetime," +
                "   CONSTRAINT [LocationIDfk] FOREIGN KEY ([LocationID]) REFERENCES [Locations])",
                TableName));

            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [SessionsEndTime] ON [Sessions]([EndTime])");
            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [SessionsLocationID] ON [Sessions]([LocationID])");
            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [SessionsStartTime] ON [Sessions]([StartTime])");
        }

        /// <summary>
        /// Creates triggers on the table.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="log"></param>
        public void CreateTriggers(IDbConnection connection, TextWriter log)
        {
            Sql.ExecuteNonQuery(connection, null, log, "CREATE TRIGGER IF NOT EXISTS [SessionIDdeltrig] BEFORE DELETE ON [Sessions] FOR EACH ROW BEGIN DELETE FROM [Flights] WHERE [SessionID] = OLD.[SessionID]; END;");
        }

        /// <summary>
        /// Returns every session record in the database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public List<BaseStationSession> GetAllRecords(IDbConnection connection, IDbTransaction transaction, TextWriter log)
        {
            var result = new List<BaseStationSession>();

            var preparedCommand = PrepareCommand(connection, transaction, "GetAll", _GetAllRecordsCommandText, 0);
            Sql.LogCommand(log, preparedCommand.Command);
            using(var reader = Sql.Exec.ExecuteReader(preparedCommand.Command)) {
                while(Sql.Exec.Read(reader)) {
                    result.Add(new BaseStationSession() {
                        SessionID = Sql.GetInt32(reader, 0),
                        LocationID = Sql.GetInt32(reader, 1),
                        StartTime = Sql.GetDateTime(reader, 2),
                        EndTime = Sql.GetNDateTime(reader, 3),
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Inserts a new record and returns the ID.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public int Insert(IDbConnection connection, IDbTransaction transaction, TextWriter log, BaseStationSession session)
        {
            var preparedCommand = PrepareInsert(connection, transaction, "Insert", "SessionID", "LocationID", "StartTime", "EndTime");
            return (int)Sql.ExecuteInsert(preparedCommand, log, session.LocationID, session.StartTime, session.EndTime);
        }

        /// <summary>
        /// Updates an existing record.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="session"></param>
        public void Update(IDbConnection connection, IDbTransaction transaction, TextWriter log, BaseStationSession session)
        {
            var preparedCommand = PrepareCommand(connection, transaction, "Update", _UpdateCommandText, 4);
            Sql.SetParameters(preparedCommand, session.LocationID, session.StartTime, session.EndTime, session.SessionID);
            Sql.LogCommand(log, preparedCommand.Command);
            Sql.Exec.ExecuteNonQuery(preparedCommand.Command);
        }

        /// <summary>
        /// Deletes the record passed across.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="session"></param>
        public void Delete(IDbConnection connection, IDbTransaction transaction, TextWriter log, BaseStationSession session)
        {
            var preparedCommand = PrepareCommand(connection, transaction, "Delete", _DeleteCommandText, 1);
            Sql.SetParameters(preparedCommand, session.SessionID);
            Sql.LogCommand(log, preparedCommand.Command);
            Sql.Exec.ExecuteNonQuery(preparedCommand.Command);
        }
    }
}
