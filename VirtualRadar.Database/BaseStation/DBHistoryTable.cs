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
using System.IO;

namespace VirtualRadar.Database.BaseStation
{
    /// <summary>
    /// A class that handles all ADO.NET calls regarding the DBHistory table in the BaseStation database.
    /// </summary>
    class DBHistoryTable : Table
    {
        private string _GetAllRecordsCommand = "SELECT [DBHistoryID], [TimeStamp], [Description] FROM [DBHistory]";

        /// <summary>
        /// See base class.
        /// </summary>
        protected override string TableName { get { return "DBHistory"; } }

        /// <summary>
        /// See base class.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="log"></param>
        public override void CreateTable(IDbConnection connection, TextWriter log)
        {
            Sql.ExecuteNonQuery(connection, null, log, String.Format(
                "CREATE TABLE IF NOT EXISTS [{0}]" +
                "  ([DBHistoryID] integer primary key," +
                "   [TimeStamp] datetime not null," +
                "   [Description] varchar(100) not null)",
                TableName));
        }

        /// <summary>
        /// Returns every DBHistory record in the database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public List<BaseStationDBHistory> GetAllRecords(IDbConnection connection, IDbTransaction transaction, TextWriter log)
        {
            var result = new List<BaseStationDBHistory>();

            var preparedCommand = PrepareCommand(connection, transaction, "GetAll", _GetAllRecordsCommand, 0);
            Sql.LogCommand(log, preparedCommand.Command);
            using(var reader = preparedCommand.Command.ExecuteReader()) {
                while(reader.Read()) {
                    result.Add(new BaseStationDBHistory() {
                        DBHistoryID = Sql.GetInt32(reader, 0),
                        TimeStamp = Sql.GetDateTime(reader, 1),
                        Description = Sql.GetString(reader, 2),
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Inserts a new record and returns its identifier.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="dbHistory"></param>
        /// <returns></returns>
        public int Insert(IDbConnection connection, IDbTransaction transaction, TextWriter log, BaseStationDBHistory dbHistory)
        {
            var preparedCommand = PrepareInsert(connection, transaction, "Insert", "DBHistoryID", "TimeStamp", "Description");
            return (int)Sql.ExecuteInsert(preparedCommand, log, dbHistory.TimeStamp, dbHistory.Description);
        }
    }
}
