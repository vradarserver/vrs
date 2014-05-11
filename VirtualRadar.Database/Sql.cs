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
using System.Data;
using System.IO;
using System.Text;

namespace VirtualRadar.Database
{
    /// <summary>
    /// Wraps ADO.NET SQL operations for the SQLite library.
    /// </summary>
    static class Sql
    {
        #region PrepareCommand, SetParameters, AddParameters
        /// <summary>
        /// Prepares a command for future use.
        /// </summary>
        /// <param name="oldPreparedCommand"></param>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="commandText"></param>
        /// <param name="paramCount"></param>
        /// <returns></returns>
        public static SqlPreparedCommand PrepareCommand(SqlPreparedCommand oldPreparedCommand, IDbConnection connection, IDbTransaction transaction, string commandText, int paramCount)
        {
            var result = oldPreparedCommand;

            if(oldPreparedCommand == null || connection != oldPreparedCommand.Connection || transaction != oldPreparedCommand.Transaction) {
                if (oldPreparedCommand != null) oldPreparedCommand.Dispose();

                result = new SqlPreparedCommand() {
                    Command = connection.CreateCommand(),
                    Connection = connection,
                    Transaction = transaction
                };

                result.Command.Transaction = transaction;
                result.Command.CommandText = commandText;

                for(var c = 0;c < paramCount;c++) {
                    IDbDataParameter sqlParameter = result.Command.CreateParameter();
                    result.Command.Parameters.Add(sqlParameter);
                }
            }

            return result;
        }

        /// <summary>
        /// Writes values into the parameters on a prepared command. The number of parameters being written must match the
        /// number of parameters on the command.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="paramValues"></param>
        public static void SetParameters(IDbCommand command, object[] paramValues)
        {
            if(paramValues.Length != command.Parameters.Count) throw new InvalidOperationException(String.Format("{0} parameters were passed to SetParameters for a command that is expecting {1}", paramValues.Length, command.Parameters.Count));

            for(var c = 0;c < paramValues.Length;c++) {
                IDbDataParameter parameter = (IDbDataParameter)command.Parameters[c];
                parameter.Value = paramValues[c];
            }
        }

        /// <summary>
        /// Writes values into the parameters on a prepared command. The number of parameters being written must match the
        /// number of parameters on the command.
        /// </summary>
        /// <param name="preparedCommand"></param>
        /// <param name="paramValues"></param>
        public static void SetParameters(SqlPreparedCommand preparedCommand, params object[] paramValues)
        {
            SetParameters(preparedCommand.Command, paramValues);
        }

        /// <summary>
        /// Writes values into the parameters on a prepared command that is expecting named parameters.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="namedValues"></param>
        public static void SetNamedParameters(IDbCommand command, Dictionary<string, object> namedValues)
        {
            if(namedValues.Count != command.Parameters.Count) throw new InvalidOperationException(String.Format("{0} parameters were passed to SetNamedParameters for a command that is expecting {1}", namedValues.Count, command.Parameters.Count));
            var paramPosn = 0;
            foreach(var namedValue in namedValues) {
                SetNamedParameter(command, paramPosn++, namedValue.Key, namedValue.Value);
            }
        }

        /// <summary>
        /// Sets a single named parameter.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="index"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetNamedParameter(IDbCommand command, int index, string name, object value)
        {
            if(index > command.Parameters.Count) throw new InvalidOperationException(String.Format("Could not set the named parameter at index {0}, there are only {1} parameters on the command", index, command.Parameters.Count));
            var parameter = (IDbDataParameter)command.Parameters[index];
            parameter.ParameterName = name;
            parameter.Value = value;
        }


        /// <summary>
        /// Adds many parameters to the command passed across.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        public static void AddParameters(IDbCommand command, params object[] parameters)
        {
            foreach(var value in parameters) {
                AddParameter(command, value);
            }
        }

        /// <summary>
        /// Adds a single parameter to the command passed across.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="value"></param>
        public static void AddParameter(IDbCommand command, object value)
        {
            var parameter = command.CreateParameter();
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }
        #endregion

        #region ExecuteNonQuery, ExecuteScalarWithParametersList
        /// <summary>
        /// Executes the command passed across on the connection.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="commandText"></param>
        public static void ExecuteNonQuery(IDbConnection connection, IDbTransaction transaction, string commandText)
        {
            ExecuteNonQuery(connection, transaction, null, commandText);
        }

        /// <summary>
        /// Executes the command passed across on the connection.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="commandText"></param>
        public static void ExecuteNonQuery(IDbConnection connection, IDbTransaction transaction, TextWriter log, string commandText)
        {
            using(var command = connection.CreateCommand()) {
                command.CommandText = commandText;
                command.Transaction = transaction;

                LogCommand(log, command);

                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Executes the command passed across on the connection with the parameters passed across.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object ExecuteScalarWithParametersList(IDbConnection connection, IDbTransaction transaction, string commandText, object[] parameters)
        {
            object result = null;

            using(var command = connection.CreateCommand()) {
                command.CommandText = commandText;
                foreach(var parameter in parameters) {
                    IDbDataParameter sqlParameter = command.CreateParameter();
                    sqlParameter.Value = parameter;
                    command.Parameters.Add(sqlParameter);
                }

                command.Transaction = transaction;
                result = command.ExecuteScalar();
            }

            return result;
        }
        #endregion

        #region RunSql
        /// <summary>
        /// A jack-of-all-trades method that runs a SQL command as a reader (if readerFunc is supplied), scalar (if isScalar is set) or a non-query (if neither of the other two are supplied).
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <param name="readerFunc"></param>
        /// <param name="isScalar"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static object RunSql(IDbConnection connection, IDbTransaction transaction, string commandText, Dictionary<string, object> parameters, Func<IDataReader, bool> readerFunc, bool isScalar, TextWriter log)
        {
            object result = null;

            using(var command = connection.CreateCommand()) {
                command.Connection = connection;
                command.Transaction = transaction;
                command.CommandText = commandText;

                if(parameters != null) {
                    foreach(var kvp in parameters) {
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = kvp.Key;
                        parameter.Value = kvp.Value;
                        command.Parameters.Add(parameter);
                    }
                }

                if(log != null) LogCommand(log, command);

                if(readerFunc == null) {
                    if(isScalar) result = command.ExecuteScalar();
                    else         command.ExecuteNonQuery();
                } else {
                    using(var reader = command.ExecuteReader()) {
                        while(reader.Read()) {
                            if(!readerFunc(reader)) break;
                        }
                    }
                }
            }

            return result;
        }
        #endregion

        #region LogCommand
        /// <summary>
        /// Writes the content of a command to the log.
        /// </summary>
        /// <param name="log"></param>
        /// <param name="command"></param>
        public static void LogCommand(TextWriter log, IDbCommand command)
        {
            if(log != null) {
                log.WriteLine(command.CommandText);
                if(command.Parameters.Count > 0) {
                    int counter = 0;
                    foreach(IDataParameter param in command.Parameters) {
                        log.Write("-- {0} = [{1}]", param.ParameterName ?? String.Format("?{0}", counter), param.Value);
                        counter++;
                    }
                }
            }
        }
        #endregion

        #region Insert - PrepareInsert, ExecuteInsert
        /// <summary>
        /// Prepares an insert command for future execution with an <see cref="ExecuteInsert"/>.
        /// </summary>
        /// <param name="preparedCommand"></param>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="tableName"></param>
        /// <param name="uniqueIdColumn"></param>
        /// <param name="columnNames"></param>
        /// <returns></returns>
        public static SqlPreparedCommand PrepareInsert(SqlPreparedCommand preparedCommand, IDbConnection connection, IDbTransaction transaction, string tableName, string uniqueIdColumn, params object[] columnNames)
        {
            var result = preparedCommand;

            if(result == null || result.Connection != connection || result.Transaction != transaction) {
                // Start the command off
                var commandText = new StringBuilder();
                commandText.AppendFormat("INSERT INTO [{0}](", tableName);

                // Add all of the column names
                for(var c = 0;c < columnNames.Length;c++) {
                    if(c > 0) commandText.Append(", ");
                    commandText.AppendFormat("[{0}]", (string)columnNames[c]);
                }

                // Add the values - just add parameter placeholders
                commandText.Append(") VALUES (");
                for(var c = 0;c < columnNames.Length;c++) {
                    if(c > 0) commandText.Append(", ");
                    commandText.Append('?');
                }

                commandText.Append(')');
                if(uniqueIdColumn != null) commandText.AppendFormat("; SELECT [{0}] FROM [{1}] WHERE _ROWID_ = last_insert_rowid()", uniqueIdColumn, tableName);

                result = Sql.PrepareCommand(result, connection, transaction, commandText.ToString(), columnNames.Length);
            }

            return result;
        }

        /// <summary>
        /// Executes the insert statement passed across and return the ID number of the new record.
        /// </summary>
        /// <param name="preparedCommand"></param>
        /// <param name="log"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static long ExecuteInsert(SqlPreparedCommand preparedCommand, TextWriter log, params object[] parameters)
        {
            SetParameters(preparedCommand.Command, parameters);
            LogCommand(log, preparedCommand.Command);

            var result = preparedCommand.Command.ExecuteScalar();
            return result == null ? 0L : (long)result;
        }
        #endregion

        #region Get*** field reader methods
        /// <summary>
        /// Returns the column content as a string.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static string GetString(IDataReader reader, string columnName)
        {
            return reader[columnName] as String;
        }

        /// <summary>
        /// Returns the column content as a string.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public static string GetString(IDataReader reader, int ordinal)
        {
            return reader[ordinal] as string;
        }

        /// <summary>
        /// Returns the column content as a bool.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static bool GetBool(IDataReader reader, string columnName)
        {
            object result = reader[columnName];
            return result is DBNull ? false : Convert.ToBoolean(result);
        }

        /// <summary>
        /// Returns the column content as a bool.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        internal static bool GetBool(IDataReader reader, int ordinal)
        {
            object result = reader[ordinal];
            return result is DBNull ? false : Convert.ToBoolean(result);
        }

        /// <summary>
        /// Returns the column content as a nullable bool.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static bool? GetNBool(IDataReader reader, string columnName)
        {
            object result = reader[columnName];
            return result is DBNull ? (bool?)null : Convert.ToBoolean(result);
        }

        /// <summary>
        /// Returns the column content as a nullable bool.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public static bool? GetNBool(IDataReader reader, int ordinal)
        {
            object result = reader[ordinal];
            return result is DBNull ? (bool?)null : Convert.ToBoolean(result);
        }

        /// <summary>
        /// Returns the column content as a DateTime.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static DateTime GetDateTime(IDataReader reader, string columnName)
        {
            object result = reader[columnName];
            return result is DBNull ? DateTime.MinValue : (DateTime)result;
        }

        /// <summary>
        /// Returns the column content as a DateTime.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <param name="dateTimeKind"></param>
        /// <returns></returns>
        public static DateTime GetDateTime(IDataReader reader, string columnName, DateTimeKind dateTimeKind)
        {
            object value = reader[columnName];
            var result = value is DBNull ? DateTime.MinValue : (DateTime)value;
            if(result.Kind != dateTimeKind) result = DateTime.SpecifyKind(result, dateTimeKind);

            return result;
        }

        /// <summary>
        /// Returns the column content as a DateTime.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public static DateTime GetDateTime(IDataReader reader, int ordinal)
        {
            object result = reader[ordinal];
            return result is DBNull ? DateTime.MinValue : (DateTime)result;
        }

        /// <summary>
        /// Returns the column content as a DateTime.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        /// <param name="dateTimeKind"></param>
        /// <returns></returns>
        public static DateTime GetDateTime(IDataReader reader, int ordinal, DateTimeKind dateTimeKind)
        {
            object value = reader[ordinal];
            var result = value is DBNull ? DateTime.MinValue : (DateTime)value;
            if(result.Kind != dateTimeKind) result = DateTime.SpecifyKind(result, dateTimeKind);

            return result;
        }

        /// <summary>
        /// Returns the column content as a nullable DateTime.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static DateTime? GetNDateTime(IDataReader reader, string columnName)
        {
            object result = reader[columnName];
            return result is DBNull ? (DateTime?)null : (DateTime)result;
        }

        /// <summary>
        /// Returns the column content as a nullable DateTime.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public static DateTime? GetNDateTime(IDataReader reader, int ordinal)
        {
            object result = reader[ordinal];
            return result is DBNull ? (DateTime?)null : (DateTime)result;
        }

        /// <summary>
        /// Returns the column content as a 32-bit integer.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static Int32 GetInt32(IDataReader reader, string columnName)
        {
            object result = reader[columnName];
            return result is DBNull ? 0 : Convert.ToInt32(result);
        }

        /// <summary>
        /// Returns the column content as a 32-bit integer.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        internal static int GetInt32(IDataReader reader, int ordinal)
        {
            object result = reader[ordinal];
            return result is DBNull ? 0 : Convert.ToInt32(result);
        }

        /// <summary>
        /// Returns the column content as a nullable 32-bit integer.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static Int32? GetNInt32(IDataReader reader, string columnName)
        {
            object result = reader[columnName];
            return result is DBNull ? (Int32?)null : Convert.ToInt32(result);
        }

        /// <summary>
        /// Returns the column content as a nullable 32-bit integer.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public static Int32? GetNInt32(IDataReader reader, int ordinal)
        {
            object result = reader[ordinal];
            return result is DBNull ? (Int32?)null : Convert.ToInt32(result);
        }

        /// <summary>
        /// Returns the column content as a long.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static Int64 GetInt64(IDataReader reader, string columnName)
        {
            object result = reader[columnName];
            return result is DBNull ? 0L : Convert.ToInt64(result);
        }

        /// <summary>
        /// Returns the column content as a long.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public static Int64 GetInt64(IDataReader reader, int ordinal)
        {
            object result = reader[ordinal];
            return result is DBNull ? 0L : Convert.ToInt64(result);
        }

        /// <summary>
        /// Returns the column content as a nullable long.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static Int64? GetNInt64(IDataReader reader, string columnName)
        {
            object result = reader[columnName];
            return result is DBNull ? (Int64?)null : Convert.ToInt64(result);
        }

        /// <summary>
        /// Returns the column content as a nullable long.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public static Int64? GetNInt64(IDataReader reader, int ordinal)
        {
            object result = reader[ordinal];
            return result is DBNull ? (Int64?)null : Convert.ToInt64(result);
        }

        /// <summary>
        /// Returns the column content as a double.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static double GetDouble(IDataReader reader, string columnName)
        {
            object result = reader[columnName];
            return result is DBNull ? 0 : Convert.ToDouble(result);
        }

        /// <summary>
        /// Returns the column content as a double.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public static double GetDouble(IDataReader reader, int ordinal)
        {
            object result = reader[ordinal];
            return result is DBNull ? 0 : Convert.ToDouble(result);
        }

        /// <summary>
        /// Returns the column content as a nullable double.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static double? GetNDouble(IDataReader reader, string columnName)
        {
            object result = reader[columnName];
            return result is DBNull ? (double?)null : Convert.ToDouble(result);
        }

        /// <summary>
        /// Returns the column content as a nullable double.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public static double? GetNDouble(IDataReader reader, int ordinal)
        {
            object result = reader[ordinal];
            return result is DBNull ? (double?)null : Convert.ToDouble(result);
        }

        /// <summary>
        /// Returns the column content as a float.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static float GetFloat(IDataReader reader, string columnName)
        {
            object result = reader[columnName];
            return result is DBNull ? 0f : Convert.ToSingle(result);
        }

        /// <summary>
        /// Returns the column content as a float.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public static float GetFloat(IDataReader reader, int ordinal)
        {
            object result = reader[ordinal];
            return result is DBNull ? 0f : Convert.ToSingle(result);
        }

        /// <summary>
        /// Returns the column content as a nullable float.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static float? GetNFloat(IDataReader reader, string columnName)
        {
            object result = reader[columnName];
            return result is DBNull ? (float?)null : Convert.ToSingle(result);
        }

        /// <summary>
        /// Returns the column content as a nullable float.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public static float? GetNFloat(IDataReader reader, int ordinal)
        {
            object result = reader[ordinal];
            return result is DBNull ? (float?)null : Convert.ToSingle(result);
        }

        /// <summary>
        /// Returns the column content as an array of bytes.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static byte[] GetBlob(IDataReader reader, string columnName)
        {
            object result = reader[columnName];
            return result is DBNull ? new byte[] { } : (byte[])result;
        }

        /// <summary>
        /// Returns the column content as an array of bytes.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public static byte[] GetBlob(IDataReader reader, int ordinal)
        {
            object result = reader[ordinal];
            return result is DBNull ? new byte[] { } : (byte[])result;
        }
        #endregion
    }
}
