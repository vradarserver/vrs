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
    /// A class that handles the ADO.NET for the Locations table in the BaseStation database.
    /// </summary>
    class LocationsTable : Table
    {
        private string _GetAllRecordsCommandText = "SELECT [LocationID], [LocationName], [Latitude], [Longitude], [Altitude] FROM [Locations]";
        private string _UpdateCommandText = "UPDATE [Locations] SET [LocationName] = ?, [Latitude] = ?, [Longitude] = ?, [Altitude] = ? WHERE [LocationID] = ?";
        private string _DeleteCommandText = "DELETE FROM [Locations] WHERE [LocationID] = ?";

        /// <summary>
        /// See base class.
        /// </summary>
        protected override string TableName { get { return "Locations"; } }

        /// <summary>
        /// See base class.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="log"></param>
        public override void CreateTable(IDbConnection connection, TextWriter log)
        {
            Sql.ExecuteNonQuery(connection, null, log, String.Format(
                "CREATE TABLE IF NOT EXISTS [{0}]" +
                "  ([LocationID] integer primary key," +
                "   [LocationName] varchar(20) not null," +
                "   [Latitude] real not null," +
                "   [Longitude] real not null," +
                "   [Altitude] real not null)",
                TableName));

            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [LocationsLocationName] ON [Locations] ([LocationName]);");
        }

        /// <summary>
        /// Returns every session record in the database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public List<BaseStationLocation> GetAllRecords(IDbConnection connection, IDbTransaction transaction, TextWriter log)
        {
            var result = new List<BaseStationLocation>();

            var preparedCommand = PrepareCommand(connection, transaction, "GetAll", _GetAllRecordsCommandText, 0);
            Sql.LogCommand(log, preparedCommand.Command);
            using(var reader = Sql.Exec.ExecuteReader(preparedCommand.Command)) {
                while(Sql.Exec.Read(reader)) {
                    result.Add(new BaseStationLocation() {
                        LocationID = Sql.GetInt32(reader, 0),
                        LocationName = Sql.GetString(reader, 1),
                        Latitude = Sql.GetDouble(reader, 2),
                        Longitude = Sql.GetDouble(reader, 3),
                        Altitude = Sql.GetDouble(reader, 4),
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
        /// <param name="location"></param>
        /// <returns></returns>
        public int Insert(IDbConnection connection, IDbTransaction transaction, TextWriter log, BaseStationLocation location)
        {
            var preparedCommand = PrepareInsert(connection, transaction, "Insert", "LocationID", "LocationName", "Latitude", "Longitude", "Altitude");
            return (int)Sql.ExecuteInsert(preparedCommand, log, location.LocationName, location.Latitude, location.Longitude, location.Altitude);
        }

        /// <summary>
        /// Updates an existing record.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="location"></param>
        public void Update(IDbConnection connection, IDbTransaction transaction, TextWriter log, BaseStationLocation location)
        {
            var preparedCommand = PrepareCommand(connection, transaction, "Update", _UpdateCommandText, 5);
            Sql.SetParameters(preparedCommand, location.LocationName, location.Latitude, location.Longitude, location.Altitude, location.LocationID);
            Sql.LogCommand(log, preparedCommand.Command);
            Sql.Exec.ExecuteNonQuery(preparedCommand.Command);
        }

        /// <summary>
        /// Deletes the record passed across.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="location"></param>
        public void Delete(IDbConnection connection, IDbTransaction transaction, TextWriter log, BaseStationLocation location)
        {
            var preparedCommand = PrepareCommand(connection, transaction, "Delete", _DeleteCommandText, 1);
            Sql.SetParameters(preparedCommand, location.LocationID);
            Sql.LogCommand(log, preparedCommand.Command);
            Sql.Exec.ExecuteNonQuery(preparedCommand.Command);
        }
    }
}
