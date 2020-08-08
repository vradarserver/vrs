// Copyright © 2020 onwards, Andrew Whewell
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
using VirtualRadar.Interface.StateHistory;

namespace VirtualRadar.Library.StateHistory
{
    class StateHistoryDatabaseInstance : IStateHistoryDatabaseInstance
    {
        /// <summary>
        /// The database version ID for the current schema / recording methodology etc.
        /// </summary>
        private const long CurrentDatabaseVersionID = 1;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool WritesEnabled { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string NonStandardFolder { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IStateHistoryRepository Repository { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="writesEnabled"></param>
        /// <param name="nonStandardFolder"></param>
        public void Initialise(bool writesEnabled, string nonStandardFolder)
        {
            WritesEnabled =     writesEnabled;
            NonStandardFolder = nonStandardFolder;
            Repository =        Factory.Resolve<IStateHistoryRepository>();

            Repository.Initialise(this);

            DoIfWriteable(repo => {
                repo.Schema_Update();

                if(repo.DatabaseVersion_GetLatest() == null) {
                    var databaseVersion = new DatabaseVersion() {
                        DatabaseVersionID = CurrentDatabaseVersionID,
                        CreatedUtc =        DateTime.UtcNow,
                    };
                    repo.DatabaseVersion_Save(databaseVersion);
                }

                repo.VrsSession_Insert(new VrsSession() {
                    DatabaseVersionID = CurrentDatabaseVersionID,
                    CreatedUtc =        DateTime.UtcNow,
                });
            });
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool DoIfReadable(Action<IStateHistoryRepository> action)
        {
            if(!Repository.IsMissing) {
                action(Repository);
            }

            return !Repository.IsMissing;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool DoIfWriteable(Action<IStateHistoryRepository> action)
        {
            var result = Repository.WritesEnabled;
            if(result) {
                result = DoIfReadable(action);
            }

            return result;
        }
    }
}
