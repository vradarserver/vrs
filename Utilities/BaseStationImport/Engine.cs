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
using VirtualRadar.Interface.Database;

namespace BaseStationImport
{
    abstract class Engine
    {
        /// <summary>
        /// Gets a value indicating that the engine is file-based rather than engine based.
        /// </summary>
        public virtual bool UsesFileName => false;

        /// <summary>
        /// Gets a value indicating that the engine uses connection strings.
        /// </summary>
        public bool UsesConnectionString => !UsesFileName;

        /// <summary>
        /// Returns a list of issues with the options passed across. If there are no obvious problems then an empty array is returned.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public abstract string[] ValidateOptions(DatabaseEngineOptions options);

        /// <summary>
        /// Returns an implementation of <see cref="IBaseStationDatabase"/> for the engine.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public abstract IBaseStationDatabase CreateRepository(DatabaseEngineOptions options);

        /// <summary>
        /// Creates a database engine.
        /// </summary>
        /// <param name="engineOptions"></param>
        /// <returns></returns>
        public static Engine Build(DatabaseEngineOptions engineOptions)
        {
            Engine result = null;

            switch(engineOptions.Engine) {
                case DatabaseEngine.None:       result = null; break;
                case DatabaseEngine.SQLite:     result = new SQLiteEngine(); break;
                case DatabaseEngine.SqlServer:  result = new SqlServerEngine(); break;
                default:                        throw new NotImplementedException();
            }

            return result;
        }
    }
}
