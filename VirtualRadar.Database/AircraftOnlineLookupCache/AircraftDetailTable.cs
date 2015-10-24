// Copyright © 2015 onwards, Andrew Whewell
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
using System.Data;
using System.Linq;
using System.Text;
using VirtualRadar.Interface;

namespace VirtualRadar.Database.AircraftOnlineLookupCache
{
    /// <summary>
    /// Deals with the interactions with the AircraftDetail table.
    /// </summary>
    class AircraftDetailTable : Table
    {
        #region Fields
        private string _GetByIcaoCommandText;
        private string _GetManyByIcaoCommandText;
        private string _UpdateCommandText;
        #endregion

        #region Properties
        /// <summary>
        /// See base class.
        /// </summary>
        protected override string TableName { get { return "AircraftDetail"; } }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public AircraftDetailTable()
        {
            _GetByIcaoCommandText = String.Format("SELECT {0} FROM [AircraftDetail] AS [aircraft] WHERE [aircraft].[Icao] = ?", FieldList());
            _GetManyByIcaoCommandText = String.Format("SELECT {0} FROM [AircraftDetail] AS [aircraft] ", FieldList());
            _UpdateCommandText = "UPDATE [AircraftDetail] SET" +
                    "  [Icao] = ?" +
                    ", [Registration] = ?" +
                    ", [Country] = ?" +
                    ", [Manufacturer] = ?" +
                    ", [Model] = ?" +
                    ", [ModelIcao] = ?" +
                    ", [Operator] = ?" +
                    ", [OperatorIcao] = ?" +
                    ", [Serial] = ?" +
                    ", [YearBuilt] = ?" +
                    ", [CreatedUtc] = ?" +
                    ", [UpdatedUtc] = ?" +
                    " WHERE [AircraftDetailID] = ?";
        }
        #endregion

        #region CreateTable
        /// <summary>
        /// See base class.
        /// </summary>
        /// <param name="connection"></param>
        public override void CreateTable(IDbConnection connection)
        {
            Sql.ExecuteNonQuery(connection, null, null, String.Format(
                "CREATE TABLE IF NOT EXISTS [{0}]\n" +
                "(\n" +
                "    [AircraftDetailID]   INTEGER PRIMARY KEY\n" +
                "   ,[Icao]               VARCHAR(6) NOT NULL\n" +
                "   ,[Registration]       VARCHAR(20) NULL\n" +
                "   ,[Country]            NVARCHAR(200) NULL\n" +
                "   ,[Manufacturer]       NVARCHAR(200) NULL\n" +
                "   ,[Model]              NVARCHAR(200) NULL\n" +
                "   ,[ModelIcao]          VARCHAR(10) NULL\n" +
                "   ,[Operator]           NVARCHAR(200) NULL\n" +
                "   ,[OperatorIcao]       VARCHAR(3) NULL\n" +
                "   ,[Serial]             VARCHAR(80) NULL\n" +
                "   ,[YearBuilt]          INTEGER NULL\n" +
                "   ,[CreatedUtc]         DATETIME NOT NULL\n" +
                "   ,[UpdatedUtc]         DATETIME NOT NULL\n" +
                ")", TableName));

            Sql.ExecuteNonQuery(connection, null, "CREATE UNIQUE INDEX IF NOT EXISTS [IX_Aircraft_Icao] ON [Aircraft]([Icao])");
        }
        #endregion

        #region Transactions
        /// <summary>
        /// Inserts a new record.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="aircraft"></param>
        /// <returns></returns>
        public void Insert(IDbConnection connection, IDbTransaction transaction, AircraftOnlineLookupDetail aircraft)
        {
            var preparedCommand = PrepareInsert(connection, transaction, "Insert", "AircraftDetailID",
                    "Icao",
                    "Registration",
                    "Country",
                    "Manufacturer",
                    "Model",
                    "ModelIcao",
                    "Operator",
                    "OperatorIcao",
                    "Serial",
                    "YearBuilt",
                    "CreatedUtc",
                    "UpdatedUtc"
            );

            aircraft.CreatedUtc = aircraft.CreatedUtc ?? DateTime.UtcNow;
            aircraft.UpdatedUtc = aircraft.UpdatedUtc ?? aircraft.CreatedUtc;

            aircraft.AircraftDetailId = Sql.ExecuteInsert(preparedCommand, null,
                    aircraft.Icao,
                    aircraft.Registration,
                    aircraft.Country,
                    aircraft.Manufacturer,
                    aircraft.Model,
                    aircraft.ModelIcao,
                    aircraft.Operator,
                    aircraft.OperatorIcao,
                    aircraft.Serial,
                    aircraft.YearBuilt,
                    aircraft.CreatedUtc,
                    aircraft.UpdatedUtc
            );
        }

        /// <summary>
        /// Updates an existing record.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="aircraft"></param>
        public void Update(IDbConnection connection, IDbTransaction transaction, AircraftOnlineLookupDetail aircraft)
        {
            aircraft.UpdatedUtc = DateTime.UtcNow;
            var preparedCommand = PrepareCommand(connection, transaction, "Update", _UpdateCommandText, 3);
            Sql.SetParameters(preparedCommand,
                    aircraft.Icao,
                    aircraft.Registration,
                    aircraft.Country,
                    aircraft.Manufacturer,
                    aircraft.Model,
                    aircraft.ModelIcao,
                    aircraft.Operator,
                    aircraft.OperatorIcao,
                    aircraft.Serial,
                    aircraft.CreatedUtc,
                    aircraft.UpdatedUtc,
                    aircraft.AircraftDetailId
            );
            preparedCommand.Command.ExecuteNonQuery();
        }

        /// <summary>
        /// Gets a single aircraft by its ICAO code.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="icao"></param>
        /// <returns></returns>
        public AircraftOnlineLookupDetail GetByIcao(IDbConnection connection, IDbTransaction transaction, string icao)
        {
            AircraftOnlineLookupDetail result = null;

            var preparedCommand = PrepareCommand(connection, transaction, "GetByIcao", _GetByIcaoCommandText, 1);
            Sql.SetParameters(preparedCommand, icao);
            using(IDataReader reader = preparedCommand.Command.ExecuteReader()) {
                int ordinal = 0;
                if(reader.Read()) result = DecodeFullAircraft(reader, ref ordinal);
            }

            return result;
        }

        /// <summary>
        /// Gets many aircraft simultaneously by their ICAO codes.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="icaos"></param>
        /// <returns></returns>
        public List<AircraftOnlineLookupDetail> GetManyByIcao(IDbConnection connection, IDbTransaction transaction, string[] icaos)
        {
            var result = new List<AircraftOnlineLookupDetail>();

            var parameters = icaos.ToArray();
            var parameterString = new StringBuilder();
            for(var i = 0;i < parameters.Length;++i) {
                if(i != 0) parameterString.Append(',');
                parameterString.Append('?');
            }

            using(var command = connection.CreateCommand()) {
                var whereClause = String.Format(" WHERE a.[Icao] IN ({0})", parameterString);
                var commandText = String.Format("{0} {1}", _GetManyByIcaoCommandText, whereClause);
                command.Connection = connection;
                command.Transaction = transaction;
                command.CommandText = commandText;

                Sql.AddParameters(command, parameters);
                using(var reader = command.ExecuteReader()) {
                    while(reader.Read()) {
                        int ordinal = 0;
                        var aircraft = DecodeFullAircraft(reader, ref ordinal);
                        result.Add(aircraft);
                    }
                }
            }

            return result;
        }
        #endregion

        #region FieldList, DecodeFullAircraft
        /// <summary>
        /// Returns the list of fields required for an aircraft record.
        /// </summary>
        /// <returns></returns>
        public static string FieldList()
        {
            return "[aircraft].[AircraftDetailID], " +
                   "[aircraft].[Icao], " +
                   "[aircraft].[Registration], " +
                   "[aircraft].[Country], " +
                   "[aircraft].[Manufacturer], " +
                   "[aircraft].[Model] " +
                   "[aircraft].[ModelIcao] " +
                   "[aircraft].[Operator] " +
                   "[aircraft].[OperatorIcao] " +
                   "[aircraft].[Serial] " +
                   "[aircraft].[YearBuilt] " +
                   "[aircraft].[CreatedUtc] " +
                   "[aircraft].[UpdatedUtc] ";
        }

        /// <summary>
        /// Copies the content of an IDataReader containing an aircraft's information into an object.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public static AircraftOnlineLookupDetail DecodeFullAircraft(IDataReader reader, ref int ordinal)
        {
            var result = new Interface.AircraftOnlineLookupDetail();

            result.AircraftDetailId =   Sql.GetInt64(reader, ordinal++);
            result.Icao =               Sql.GetString(reader, ordinal++);
            result.Registration =       Sql.GetString(reader, ordinal++);
            result.Country =            Sql.GetString(reader, ordinal++);
            result.Manufacturer =       Sql.GetString(reader, ordinal++);
            result.Model =              Sql.GetString(reader, ordinal++);
            result.ModelIcao =          Sql.GetString(reader, ordinal++);
            result.Operator =           Sql.GetString(reader, ordinal++);
            result.OperatorIcao =       Sql.GetString(reader, ordinal++);
            result.Serial =             Sql.GetString(reader, ordinal++);
            result.YearBuilt =          Sql.GetNInt32(reader, ordinal++);
            result.CreatedUtc =         Sql.GetDateTime(reader, ordinal++, DateTimeKind.Utc);
            result.UpdatedUtc =         Sql.GetDateTime(reader, ordinal++, DateTimeKind.Utc);

            return result;
        }
        #endregion
    }
}
