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
using VirtualRadar.Interface.StandingData;

namespace VirtualRadar.Database.BasicAircraft
{
    /// <summary>
    /// Handles the interactions with the Operator table.
    /// </summary>
    class BasicOperatorTable : Table
    {
        #region Fields
        private string _DeleteUnusedCommandText;
        private string _GetByIdCommandText;
        private string _GetByNameCommandText;
        private string _UpdateCommandText;
        #endregion

        #region Properties
        /// <summary>
        /// See base class.
        /// </summary>
        protected override string TableName { get { return "Operator"; } }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public BasicOperatorTable()
        {
            _DeleteUnusedCommandText = "DELETE FROM [Operator] WHERE [OperatorID] NOT IN (SELECT [OperatorID] FROM [Aircraft])";
            _GetByIdCommandText = String.Format("SELECT {0} FROM [Operator] AS o WHERE o.[OperatorID] = ?", FieldList());
            _GetByNameCommandText = String.Format("SELECT {0} FROM [Operator] AS o WHERE o.[Name] = ?", FieldList());
            _UpdateCommandText = "UPDATE [Operator] SET" +
                    "  [Icao] = ?" +
                    ", [Name] = ?" +
                    " WHERE [OperatorID] = ?";
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
                "    [OperatorID]         INTEGER PRIMARY KEY\n" +
                "   ,[Icao]               VARCHAR(10) NULL\n" +
                "   ,[Name]               VARCHAR(250) NULL\n" +
                ")", TableName));

            Sql.ExecuteNonQuery(connection, null, "CREATE INDEX IF NOT EXISTS [IX_Operator_Name] ON [Operator]([Name])");
        }
        #endregion

        #region Transactions
        /// <summary>
        /// Inserts a new model record.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        public int Insert(IDbConnection connection, IDbTransaction transaction, BasicOperator record)
        {
            var preparedCommand = PrepareInsert(connection, transaction, "Insert", "OperatorID",
                    "Icao",
                    "Name"
            );

            return (int)Sql.ExecuteInsert(preparedCommand, null,
                    record.Icao,
                    record.Name
            );
        }

        /// <summary>
        /// Updates an existing model.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="record"></param>
        public void Update(IDbConnection connection, IDbTransaction transaction, BasicOperator record)
        {
            var preparedCommand = PrepareCommand(connection, transaction, "Update", _UpdateCommandText, 3);
            Sql.SetParameters(preparedCommand,
                    record.Icao,
                    record.Name,
                    record.OperatorID);
            preparedCommand.Command.ExecuteNonQuery();
        }

        /// <summary>
        /// Deletes all unused operators.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        public void DeleteUnused(IDbConnection connection, IDbTransaction transaction)
        {
            var preparedCommand = PrepareCommand(connection, transaction, "DeleteUnused", _DeleteUnusedCommandText, 0);
            preparedCommand.Command.ExecuteNonQuery();
        }

        /// <summary>
        /// Returns the record with the ID passed across.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public BasicOperator GetById(IDbConnection connection, IDbTransaction transaction, int id)
        {
            BasicOperator result = null;

            var preparedCommand = PrepareCommand(connection, transaction, "GetById", _GetByIdCommandText, 1);
            Sql.SetParameters(preparedCommand, id);
            using(IDataReader reader = preparedCommand.Command.ExecuteReader()) {
                int ordinal = 0;
                if(reader.Read()) result = DecodeFullOperator(reader, ref ordinal);
            }

            return result;
        }

        /// <summary>
        /// Returns the records with the name passed across.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="result"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public void GetByName(IDbConnection connection, IDbTransaction transaction, List<BasicOperator> result, string name)
        {
            var preparedCommand = PrepareCommand(connection, transaction, "GetByName", _GetByNameCommandText, 1);
            Sql.SetParameters(preparedCommand, name);
            using(IDataReader reader = preparedCommand.Command.ExecuteReader()) {
                while(reader.Read()) {
                    int ordinal = 0;
                    var record = DecodeFullOperator(reader, ref ordinal);
                    result.Add(record);
                }
            }
        }
        #endregion

        #region FieldList, DecodeFullModel
        /// <summary>
        /// Returns the list of fields required for an operator record.
        /// </summary>
        /// <returns></returns>
        public static string FieldList()
        {
            return "o.[OperatorID], " +
                   "o.[Icao], " +
                   "o.[Name] ";
        }

        /// <summary>
        /// Copies the content of an IDataReader containing an operator's information into an object.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public static BasicOperator DecodeFullOperator(IDataReader reader, ref int ordinal)
        {
            BasicOperator result = new BasicOperator();

            result.OperatorID = Sql.GetInt32(reader, ordinal++);
            result.Icao =       Sql.GetString(reader, ordinal++);
            result.Name =       Sql.GetString(reader, ordinal++);

            return result;
        }
        #endregion
    }
}
