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

namespace VirtualRadar.Database
{
    /// <summary>
    /// A base class for full read-write tables in an SQLite database.
    /// </summary>
    abstract class Table : IDisposable
    {
        /// <summary>
        /// A map of command names to prepared commands.
        /// </summary>
        private Dictionary<string, SqlPreparedCommand> _Commands = new Dictionary<string,SqlPreparedCommand>();

        /// <summary>
        /// The name of the table in the database.
        /// </summary>
        protected abstract string TableName { get; }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~Table()
        {
            Dispose(false);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                CloseCommands();
            }
        }

        /// <summary>
        /// Disposes of all prepared commands.
        /// </summary>
        public void CloseCommands()
        {
            foreach(var command in _Commands.Values) {
                command.Dispose();
            }
            _Commands.Clear();
        }

        /// <summary>
        /// Creates the table if it's missing.
        /// </summary>
        public virtual void CreateTable(IDbConnection connection)
        {
        }

        /// <summary>
        /// Creates the table if it's missing.
        /// </summary>
        public virtual void CreateTable(IDbConnection connection, TextWriter log)
        {
        }

        /// <summary>
        /// Returns a prepared command by the name given by the derived class.
        /// </summary>
        /// <param name="commandName"></param>
        /// <returns></returns>
        private SqlPreparedCommand FetchExistingPreparedCommand(string commandName)
        {
            SqlPreparedCommand existing = null;
            _Commands.TryGetValue(commandName, out existing);

            return existing;
        }

        /// <summary>
        /// Saves a prepared command against the name given by the derived class.
        /// </summary>
        /// <param name="commandName"></param>
        /// <param name="existing"></param>
        /// <param name="result"></param>
        private void RecordPreparedCommand(string commandName, SqlPreparedCommand existing, SqlPreparedCommand result)
        {
            if (existing == null) _Commands.Add(commandName, result);
            else if (!Object.ReferenceEquals(existing, result)) _Commands[commandName] = result;
        }

        /// <summary>
        /// Retrieves or creates a prepared command.
        /// </summary>
        /// <returns></returns>
        protected SqlPreparedCommand PrepareCommand(IDbConnection connection, IDbTransaction transaction, string commandName, string commandText, int paramCount)
        {
            SqlPreparedCommand existing = FetchExistingPreparedCommand(commandName);
            SqlPreparedCommand result = Sql.PrepareCommand(existing, connection, transaction, commandText, paramCount);
            RecordPreparedCommand(commandName, existing, result);

            return result;
        }

        /// <summary>
        /// Prepares an insert command.
        /// </summary>
        protected SqlPreparedCommand PrepareInsert(IDbConnection connection, IDbTransaction transaction, string commandName, string uniqueIdColumnName, params string[] columnNames)
        {
            SqlPreparedCommand existing = FetchExistingPreparedCommand(commandName);
            SqlPreparedCommand result = Sql.PrepareInsert(existing, connection, transaction, TableName, uniqueIdColumnName, columnNames);
            RecordPreparedCommand(commandName, existing, result);

            return result;
        }

        /// <summary>
        /// Prepares the command and parameters simultaneously.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="commandName"></param>
        /// <param name="commandTextFormat"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected SqlPreparedCommand PrepareCommandAndParams(IDbConnection connection, IDbTransaction transaction, string commandName, string commandTextFormat, Dictionary<string, object> parameters)
        {
            var commandText = String.Format(commandTextFormat, TableName);
            var result = PrepareCommand(connection, transaction, commandName, commandText, parameters == null ? 0 : parameters.Count);
            if(parameters != null) Sql.SetNamedParameters(result.Command, parameters);

            return result;
        }

        /// <summary>
        /// Sets the named parameter at the index passed across.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="index"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected void SetNamedParameter(SqlPreparedCommand command, int index, string name, object value)
        {
            Sql.SetNamedParameter(command.Command, index, name, value);
        }

        /// <summary>
        /// Prepares and executes an insert operation.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="commandName"></param>
        /// <param name="uniqueIdColumnName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected long PrepareAndExecuteInsert(IDbConnection connection, IDbTransaction transaction, TextWriter log, string commandName, string uniqueIdColumnName, Dictionary<string, object> parameters)
        {
            var parameterCount = parameters == null ? 0 : parameters.Count;
            var columnNames = new string[parameterCount];
            var values = new object[parameterCount];
            var paramIndex = 0;
            foreach(var kvp in parameters) {
                columnNames[paramIndex] = kvp.Key;
                values[paramIndex] = kvp.Value;
                ++paramIndex;
            }

            var existing = FetchExistingPreparedCommand(commandName);
            var command = Sql.PrepareInsert(existing, connection, transaction, TableName, uniqueIdColumnName, columnNames);
            RecordPreparedCommand(commandName, existing, command);
            var result = Sql.ExecuteInsert(command, log, values);

            return result;
        }

        /// <summary>
        /// Prepares and executes a non-query operation.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="commandName"></param>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        protected void PrepareAndExecuteNonQuery(IDbConnection connection, IDbTransaction transaction, TextWriter log, string commandName, string commandText, Dictionary<string, object> parameters)
        {
            var command = PrepareCommandAndParams(connection, transaction, commandName, commandText, parameters);
            Sql.LogCommand(log, command.Command);
            command.Command.ExecuteNonQuery();
        }
    }
}
