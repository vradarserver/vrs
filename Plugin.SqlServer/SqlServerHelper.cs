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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Plugin.SqlServer
{
    /// <summary>
    /// Helper methods for working with SQL Server databases.
    /// </summary>
    public static class SqlServerHelper
    {
        /// <summary>
        /// Runs the script passed across on the connection passed across.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="script"></param>
        public static void RunScript(IDbConnection connection, string script)
        {
            var lines = new List<string>();
            using(var reader = new StringReader(script)) {
                string line;
                while((line = reader.ReadLine()) != null) {
                    if(line.Trim().ToUpper() != "GO") {
                        lines.Add(line);
                    } else {
                        RunScriptChunk(connection, lines);
                        lines.Clear();
                    }
                }
            }

            RunScriptChunk(connection, lines);
        }

        private static void RunScriptChunk(IDbConnection connection, IEnumerable<string> lines)
        {
            var sql = String.Join(Environment.NewLine, lines);
            if(!String.IsNullOrEmpty(sql)) {
                using(var command = connection.CreateCommand()) {
                    command.Connection = connection;
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    command.ExecuteNonQuery();
                }
            }
        }

        public static DataTable GenerateDataTable<T>(IEnumerable<T> rows, string[] columnNames, Type[] columnTypes, Func<T, int, object> getColumnValue)
        {
            var result = new DataTable();
            for(var i = 0;i < columnNames.Length;++i) {
                result.Columns.Add(columnNames[i], columnTypes[i]);
            }
            foreach(var row in rows) {
                var dataTableRow = result.NewRow();
                for(var i = 0;i < columnNames.Length;++i) {
                    dataTableRow[i] = getColumnValue(row, i);
                }
            }
            result.EndLoadData();

            return result;
        }
    }
}
