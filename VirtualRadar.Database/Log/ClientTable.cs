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
    /// A class that handles access to the Client table in the log database.
    /// </summary>
    class ClientTable : Table
    {
        /// <summary>
        /// See base class docs.
        /// </summary>
        protected override string TableName { get { return "Client"; } }

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
                "   [IpAddress] TEXT NOT NULL," +
                "   [ReverseDns] TEXT," +
                "   [ReverseDnsDate] DATETIME)",
                TableName));

            Sql.ExecuteNonQuery(connection, null, String.Format(
                "CREATE UNIQUE INDEX IF NOT EXISTS [Idx_Client_IpAddress]" +
                "  ON [{0}] ([IpAddress] ASC)", TableName));
        }
        #endregion

        #region Insert, Update, Delete
        /// <summary>
        /// Adds a new record to the database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="client"></param>
        public void Insert(IDbConnection connection, IDbTransaction transaction, TextWriter log, LogClient client)
        {
            SqlPreparedCommand command = PrepareInsert(connection, transaction, "Insert",
                "Id",
                "IpAddress", "ReverseDns", "ReverseDnsDate");
            client.Id = Sql.ExecuteInsert(command, log, client.IpAddress, client.ReverseDns, client.ReverseDnsDate);
        }

        /// <summary>
        /// Updates an existing record on the database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="client"></param>
        public void Update(IDbConnection connection, IDbTransaction transaction, TextWriter log, LogClient client)
        {
            SqlPreparedCommand command = PrepareCommand(connection, transaction, "Update",
                String.Format("UPDATE [{0}] SET [IpAddress] = ?, [ReverseDns] = ?, [ReverseDnsDate] = ?" +
                              " WHERE [Id] = ?", TableName), 4);
            Sql.SetParameters(command, client.IpAddress, client.ReverseDns, client.ReverseDnsDate, client.Id);
            Sql.LogCommand(log, command.Command);
            command.Command.ExecuteNonQuery();
        }

        /// <summary>
        /// Deletes a record from the database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="client"></param>
        public void Delete(IDbConnection connection, IDbTransaction transaction, TextWriter log, LogClient client)
        {
            SqlPreparedCommand command = PrepareCommand(connection, transaction, "Delete",
                String.Format("DELETE FROM [{0}] WHERE [Id] = ?", TableName), 1);
            Sql.SetParameters(command, client.Id);
            Sql.LogCommand(log, command.Command);
            command.Command.ExecuteNonQuery();
        }
        #endregion

        #region SelectById, SelectByIpAddress, SelectAll
        /// <summary>
        /// Fetches a client record from the database for the client ID number passed across.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public LogClient SelectById(IDbConnection connection, IDbTransaction transaction, TextWriter log, long id)
        {
            SqlPreparedCommand command = PrepareCommand(connection, transaction, "SelectById",
                String.Format("SELECT [Id], [IpAddress], [ReverseDns], [ReverseDnsDate]" +
                              "  FROM [{0}]" +
                              "  WHERE [Id] = ?",
                              TableName), 1);
            Sql.SetParameters(command, id);
            Sql.LogCommand(log, command.Command);

            return PerformSelect(command);
        }

        /// <summary>
        /// Fetches a client record from the database for the IP address passed across.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public LogClient SelectByIpAddress(IDbConnection connection, IDbTransaction transaction, TextWriter log, string ipAddress)
        {
            SqlPreparedCommand command = PrepareCommand(connection, transaction, "SelectByIpAddress",
                String.Format("SELECT [Id], [IpAddress], [ReverseDns], [ReverseDnsDate]" +
                              "  FROM [{0}]" +
                              "  WHERE [IpAddress] = ?",
                              TableName), 1);
            Sql.SetParameters(command, ipAddress);
            Sql.LogCommand(log, command.Command);

            return PerformSelect(command);
        }

        /// <summary>
        /// Fetches a collection of all clients on the database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public List<LogClient> SelectAll(IDbConnection connection, IDbTransaction transaction, TextWriter log)
        {
            List<LogClient> result = new List<LogClient>();

            SqlPreparedCommand command = PrepareCommand(connection, transaction, "SelectAll",
                String.Format("SELECT [Id], [IpAddress], [ReverseDns], [ReverseDnsDate]" +
                              "  FROM [{0}]",
                              TableName), 0);
            Sql.LogCommand(log, command.Command);
            using(IDataReader reader = command.Command.ExecuteReader()) {
                while(reader.Read()) {
                    result.Add(DecodeReader(reader));
                }
            }

            return result;
        }

        /// <summary>
        /// Executes the single record fetch command passed across and returns the client record arising from it,
        /// or null if no client matches the criteria.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private LogClient PerformSelect(SqlPreparedCommand command)
        {
            LogClient result = null;
            using(IDataReader reader = command.Command.ExecuteReader()) {
                if(reader.Read()) result = DecodeReader(reader);
            }

            return result;
        }

        /// <summary>
        /// Creates a client object and copies the content of an IDataReader into it.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static LogClient DecodeReader(IDataReader reader)
        {
            return new LogClient() {
                Id = Sql.GetInt64(reader, "Id"),
                IpAddress = Sql.GetString(reader, "IpAddress"),
                ReverseDns = Sql.GetString(reader, "ReverseDns"),
                ReverseDnsDate = Sql.GetNDateTime(reader, "ReverseDnsDate"),
            };
        }
        #endregion
    }
}
