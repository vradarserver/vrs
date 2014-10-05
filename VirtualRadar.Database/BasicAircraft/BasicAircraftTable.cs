// Copyright © 2014 onwards, Andrew Whewell
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

namespace VirtualRadar.Database.BasicAircraft
{
    /// <summary>
    /// Deals with the interactions with the Aircraft table.
    /// </summary>
    class BasicAircraftTable : Table
    {
        #region Fields
        private string _GetByIcaoCommandText;
        private string _UpdateCommandText;
        #endregion

        #region Properties
        /// <summary>
        /// See base class.
        /// </summary>
        protected override string TableName { get { return "Aircraft"; } }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public BasicAircraftTable()
        {
            _GetByIcaoCommandText = String.Format("SELECT {0} FROM [Aircraft] AS a WHERE a.[Icao] = ?", FieldList());
            _UpdateCommandText = "UPDATE [Aircraft] SET" +
                    "  [Icao] = ?" +
                    ", [Registration] = ?" +
                    ", [ModelID] = ?" +
                    ", [OperatorID] = ?" +
                    ", [BaseStationUpdated] = ?" +
                    " WHERE [AircraftID] = ?";
        }
        #endregion

        #region CreateTable
        /// <summary>
        /// See base class.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="log"></param>
        public override void CreateTable(IDbConnection connection)
        {
            Sql.ExecuteNonQuery(connection, null, null, String.Format(
                "CREATE TABLE IF NOT EXISTS [{0}]\n" +
                "(\n" +
                "    [AircraftID]         INTEGER PRIMARY KEY\n" +
                "   ,[Icao]               VARCHAR(6) NOT NULL\n" +
                "   ,[Registration]       VARCHAR(20) NOT NULL\n" +
                "   ,[ModelID]            INTEGER REFERENCES [Model]([ModelID]) ON DELETE SET NULL\n" +
                "   ,[OperatorID]         INTEGER REFERENCES [Operator]([OperatorID]) ON DELETE SET NULL\n" +
                "   ,[BaseStationUpdated] DATETIME NOT NULL\n" +
                ")", TableName));

            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [IX_Aircraft_Icao] ON [Aircraft]([Icao])");
            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [IX_Aircraft_ModelId] ON [Aircraft]([ModelId])");
            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [IX_Aircraft_OperatorId] ON [Aircraft]([OperatorId])");
        }
        #endregion

        #region Transactions
        /// <summary>
        /// Inserts a new model record.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="aircraft"></param>
        /// <returns></returns>
        public int Insert(IDbConnection connection, IDbTransaction transaction, Interface.StandingData.BasicAircraft aircraft)
        {
            var preparedCommand = PrepareInsert(connection, transaction, "Insert", "AircraftID",
                    "Icao",
                    "Registration",
                    "ModelID",
                    "OperatorID",
                    "BaseStationUpdated"
            );

            return (int)Sql.ExecuteInsert(preparedCommand, null,
                    aircraft.Icao,
                    aircraft.Registration,
                    aircraft.BasicModelID,
                    aircraft.BasicOperatorID,
                    aircraft.BaseStationUpdated
            );
        }

        /// <summary>
        /// Updates an existing model.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="aircraft"></param>
        public void Update(IDbConnection connection, IDbTransaction transaction, Interface.StandingData.BasicAircraft aircraft)
        {
            var preparedCommand = PrepareCommand(connection, transaction, "Update", _UpdateCommandText, 3);
            Sql.SetParameters(preparedCommand,
                    aircraft.Icao,
                    aircraft.Registration,
                    aircraft.BasicModelID,
                    aircraft.BasicOperatorID,
                    aircraft.BaseStationUpdated,
                    aircraft.AircraftID);
            preparedCommand.Command.ExecuteNonQuery();
        }

        /// <summary>
        /// Gets a single aircraft by its ICAO code.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="icao"></param>
        /// <returns></returns>
        public Interface.StandingData.BasicAircraft GetByIcao(IDbConnection connection, IDbTransaction transaction, string icao)
        {
            Interface.StandingData.BasicAircraft result = null;

            var preparedCommand = PrepareCommand(connection, transaction, "GetByIcao", _GetByIcaoCommandText, 1);
            Sql.SetParameters(preparedCommand, icao);
            using(IDataReader reader = preparedCommand.Command.ExecuteReader()) {
                int ordinal = 0;
                if(reader.Read()) result = DecodeFullAircraft(reader, ref ordinal);
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
            return "a.[AircraftID], " +
                   "a.[Icao], " +
                   "a.[Registration], " +
                   "a.[ModelID], " +
                   "a.[OperatorID], " +
                   "a.[BaseStationUpdated] ";
        }

        /// <summary>
        /// Copies the content of an IDataReader containing an aircraft's information into an object.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public static Interface.StandingData.BasicAircraft DecodeFullAircraft(IDataReader reader, ref int ordinal)
        {
            Interface.StandingData.BasicAircraft result = new Interface.StandingData.BasicAircraft();

            result.AircraftID =         Sql.GetInt32(reader, ordinal++);
            result.Icao =               Sql.GetString(reader, ordinal++);
            result.Registration =       Sql.GetString(reader, ordinal++);
            result.BasicModelID =       Sql.GetNInt32(reader, ordinal++);
            result.BasicOperatorID =    Sql.GetNInt32(reader, ordinal++);
            result.BaseStationUpdated = Sql.GetDateTime(reader, ordinal++);

            return result;
        }
        #endregion
    }
}
