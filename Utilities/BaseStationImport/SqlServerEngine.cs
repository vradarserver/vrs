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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using VirtualRadar.Interface.Database;

namespace BaseStationImport
{
    /// <summary>
    /// The specialisation of <see cref="Engine"/> for SQL Server.
    /// </summary>
    class SqlServerEngine : Engine
    {
        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public override string[] ValidateOptions(DatabaseEngineOptions options)
        {
            var result = new List<string>();
            var direction = options.IsSource ? "source" : "target";

            if(!Factory.Singleton.HasImplementation<IBaseStationDatabaseSqlServer>()) {
                result.Add("The SQL Server plugin has not been installed or could not be loaded");
            } else {
                if(String.IsNullOrEmpty(options.ConnectionString)) {
                    result.Add($"You must specify the connection string for the {direction} SQL Server connection");
                } else {
                    try {
                        using(var repository = Factory.Singleton.Resolve<IBaseStationDatabaseSqlServer>()) {
                            repository.ConnectionString = options.ConnectionString;

                            if(!repository.TestConnection()) {
                                result.Add($"A connection to the {direction} SQL Server could not be established");
                            }
                        }
                    } catch(Exception ex) {
                        result.Add($"Could not connect to the {direction} SQL Server: {ex.Message}");
                    }
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public override IBaseStationDatabase CreateRepository(DatabaseEngineOptions options)
        {
            var result = Factory.Singleton.Resolve<IBaseStationDatabaseSqlServer>();
            result.ConnectionString = options.ConnectionString;
            result.WriteSupportEnabled = options.IsTarget;
            result.CanUpdateSchema = options.IsTarget;

            return result;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public override string[] UpdateSchema(DatabaseEngineOptions options)
        {
            using(var repository = (IBaseStationDatabaseSqlServer)CreateRepository(options)) {
                return repository.UpdateSchema();
            }
        }
    }
}
