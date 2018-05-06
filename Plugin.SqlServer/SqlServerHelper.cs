// Copyright © 2017 onwards, Andrew Whewell
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
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using VirtualRadar.Plugin.SqlServer.Models;

namespace VirtualRadar.Plugin.SqlServer
{
    /// <summary>
    /// Helper methods for working with SQL Server databases.
    /// </summary>
    public static class SqlServerHelper
    {
        /// <summary>
        /// Protects against multi-threaded access to the fields.
        /// </summary>
        private static object _SyncLock = new object();

        /// <summary>
        /// A map of connection references to script output lines.
        /// </summary>
        private static Dictionary<SqlConnection, List<string>> _ConnectionPrintLinesMap = new Dictionary<SqlConnection, List<string>>();

        /// <summary>
        /// Runs the script passed across on the connection passed across.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="script"></param>
        /// <param name="commandTimeoutSeconds"></param>
        /// <returns>Print output from script</returns>
        public static string[] RunScript(IDbConnection connection, string script, int? commandTimeoutSeconds = null)
        {
            var printLines = new List<string>();

            var sqlConnection = connection as SqlConnection;
            if(sqlConnection != null) {
                lock(_SyncLock) {
                    _ConnectionPrintLinesMap.Add(sqlConnection, printLines);
                }
                sqlConnection.InfoMessage += SqlConnection_InfoMessage;
            }

            try {
                var scriptLines = new List<string>();
                using(var reader = new StringReader(script)) {
                    string line;
                    while((line = reader.ReadLine()) != null) {
                        if(line.Trim().ToUpper() != "GO") {
                            scriptLines.Add(line);
                        } else {
                            RunScriptChunk(connection, scriptLines, commandTimeoutSeconds);
                            scriptLines.Clear();
                        }
                    }
                }
                RunScriptChunk(connection, scriptLines, commandTimeoutSeconds);
            } finally {
                if(sqlConnection != null) {
                    sqlConnection.InfoMessage -= SqlConnection_InfoMessage;
                    lock(_SyncLock) {
                        _ConnectionPrintLinesMap.Remove(sqlConnection);
                    }
                }
            }

            return printLines.ToArray();
        }

        private static void SqlConnection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            if(sender is SqlConnection sqlConnection) {
                lock(_SyncLock) {
                    if(_ConnectionPrintLinesMap.TryGetValue(sqlConnection, out var printLines)) {
                        using(var reader = new StringReader(e.Message ?? "")) {
                            string line;
                            while((line = reader.ReadLine()) != null) {
                                printLines.Add(line);
                            }
                        }
                    }
                }
            }
        }

        private static void RunScriptChunk(IDbConnection connection, IEnumerable<string> scriptLines, int? commandTimeoutSeconds = null)
        {
            var sql = String.Join(Environment.NewLine, scriptLines);
            if(!String.IsNullOrEmpty(sql)) {
                using(var command = connection.CreateCommand()) {
                    command.Connection = connection;
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    if(commandTimeoutSeconds != null) {
                        command.CommandTimeout = commandTimeoutSeconds.Value;
                    }

                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Generates a UDTT data table parameter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="orderedProperties"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static DataTable UdttParameter<T>(IEnumerable<UdttProperty<T>> orderedProperties, IEnumerable<T> collection)
        {
            return CreateDataTableForUdtt<T>(
                collection,
                orderedProperties.Select(r => r.Property.Name).ToArray(),
                obj => orderedProperties.Select(r => r.GetFunc(obj)).ToArray(),
                typeof(T).Name
            );
        }

        private static DataTable CreateDataTableForUdtt<T>(IEnumerable<T> collection, IEnumerable<string> columnNames, Func<T, object[]> getValues, string tableName = null)
        {
            var result = new DataTable(tableName ?? typeof(T).Name);

            foreach(var columnName in columnNames) {
                var column = result.Columns.Add(columnName);
                var dataType = typeof(T).GetProperty(columnName).PropertyType;
                if(dataType.IsGenericType && dataType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    column.DataType = dataType.GetGenericArguments()[0];
                    column.AllowDBNull = true;
                } else if(!dataType.IsEnum) {
                    column.DataType = dataType;
                }
            }

            if(collection != null) {
                foreach(var element in collection) {
                    var row = result.Rows.Add(getValues(element));
                }
            }

            return result;
        }
    }
}
