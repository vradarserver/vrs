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
    /// Contains the ADO.NET code for interaction with the SystemEvents table.
    /// </summary>
    class SystemEventsTable : Table
    {
        private string _GetAllRecordsCommandText = "SELECT [SystemEventsID], [TimeStamp], [App], [Msg] FROM [SystemEvents]";
        private string _UpdateCommandText = "UPDATE [SystemEvents] SET [TimeStamp] = ?, [App] = ?, [Msg] = ? WHERE [SystemEventsID] = ?";
        private string _DeleteCommandText = "DELETE FROM [SystemEvents] WHERE [SystemEventsID] = ?";

        /// <summary>
        /// See base class.
        /// </summary>
        protected override string TableName { get { return "SystemEvents"; } }

        /// <summary>
        /// See base class.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="log"></param>
        public override void CreateTable(IDbConnection connection, TextWriter log)
        {
            Sql.ExecuteNonQuery(connection, null, log, String.Format(
                "CREATE TABLE IF NOT EXISTS [{0}]" +
                "  ([SystemEventsID] integer primary key," +
                "   [TimeStamp] datetime not null," +
                "   [App] varchar(15) not null," +
                "   [Msg] varchar(100) not null)",
                TableName));

            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX [SystemEventsApp] ON [SystemEvents]([App])");
            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX [SystemEventsTimeStamp] ON [SystemEvents]([TimeStamp])");
        }

        /// <summary>
        /// Returns every system event record in the database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public List<BaseStationSystemEvents> GetAllRecords(IDbConnection connection, IDbTransaction transaction, TextWriter log)
        {
            var result = new List<BaseStationSystemEvents>();

            var preparedCommand = PrepareCommand(connection, transaction, "GetAll", _GetAllRecordsCommandText, 0);
            Sql.LogCommand(log, preparedCommand.Command);
            using(var reader = Sql.Exec.ExecuteReader(preparedCommand.Command)) {
                while(Sql.Exec.Read(reader)) {
                    result.Add(new BaseStationSystemEvents() {
                        SystemEventsID = Sql.GetInt32(reader, 0),
                        TimeStamp = Sql.GetDateTime(reader, 1),
                        App = Sql.GetString(reader, 2),
                        Msg = Sql.GetString(reader, 3),
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
        /// <param name="systemEvent"></param>
        /// <returns></returns>
        public int Insert(IDbConnection connection, IDbTransaction transaction, TextWriter log, BaseStationSystemEvents systemEvent)
        {
            var preparedCommand = PrepareInsert(connection, transaction, "Insert", "SystemEventsID", "TimeStamp", "App", "Msg");
            return (int)Sql.ExecuteInsert(preparedCommand, log, systemEvent.TimeStamp, systemEvent.App, systemEvent.Msg);
        }

        /// <summary>
        /// Updates an existing record.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="systemEvent"></param>
        public void Update(IDbConnection connection, IDbTransaction transaction, TextWriter log, BaseStationSystemEvents systemEvent)
        {
            var preparedCommand = PrepareCommand(connection, transaction, "Update", _UpdateCommandText, 4);
            Sql.SetParameters(preparedCommand, systemEvent.TimeStamp, systemEvent.App, systemEvent.Msg, systemEvent.SystemEventsID);
            Sql.LogCommand(log, preparedCommand.Command);
            Sql.Exec.ExecuteNonQuery(preparedCommand.Command);
        }

        /// <summary>
        /// Deletes the record passed across.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="systemEvent"></param>
        public void Delete(IDbConnection connection, IDbTransaction transaction, TextWriter log, BaseStationSystemEvents systemEvent)
        {
            var preparedCommand = PrepareCommand(connection, transaction, "Delete", _DeleteCommandText, 1);
            Sql.SetParameters(preparedCommand, systemEvent.SystemEventsID);
            Sql.LogCommand(log, preparedCommand.Command);
            Sql.Exec.ExecuteNonQuery(preparedCommand.Command);
        }
    }
}
