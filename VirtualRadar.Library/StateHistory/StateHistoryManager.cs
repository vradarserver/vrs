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
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StateHistory;

namespace VirtualRadar.Library.StateHistory
{
    /// <summary>
    /// Default implementation of <see cref="IStateHistoryManager"/>.
    /// </summary>
    class StateHistoryManager : IStateHistoryManager, ISharedConfigurationSubscriber
    {
        /// <summary>
        /// The database version ID for the current schema / recording methodology etc.
        /// </summary>
        private const long CurrentDatabaseVersionID = 1;

        /// <summary>
        /// The repository that we'll be using for the duration.
        /// </summary>
        private IStateHistoryRepository _Repository;

        /// <summary>
        /// The lock that ensures access to the repository is single-threaded.
        /// </summary>
        private object _DatabaseLock = new Object();

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool Enabled { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string NonStandardFolder { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler ConfigurationLoaded;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Initialise()
        {
            var sharedConfiguration = Factory.ResolveSingleton<ISharedConfiguration>();
            var config = sharedConfiguration.Get();
            Enabled = config.StateHistorySettings.Enabled;
            NonStandardFolder = config.StateHistorySettings.NonStandardFolder;

            _Repository = Factory.Resolve<IStateHistoryRepository>();
            InitialiseDatabaseWithNewConfiguration();

            sharedConfiguration.AddWeakSubscription(this);
        }

        private void InitialiseDatabaseWithNewConfiguration()
        {
            LockRepoAndCallIfEnabled(r => {
                r.Schema_Update();

                if(r.DatabaseVersion_GetLatest() == null) {
                    var databaseVersion = new DatabaseVersion() {
                        DatabaseVersionID = CurrentDatabaseVersionID,
                        CreatedUtc =        DateTime.UtcNow,
                    };
                    r.DatabaseVersion_Save(databaseVersion);
                }

                r.VrsSession_Insert(new VrsSession() {
                    DatabaseVersionID = CurrentDatabaseVersionID,
                    CreatedUtc =        DateTime.UtcNow,
                });
            });
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="sharedConfiguration"></param>
        public void SharedConfigurationChanged(ISharedConfiguration sharedConfiguration)
        {
            var config = sharedConfiguration.Get();

            var raiseConfigurationLoaded = false;

            void assignConfigValue<T>(T configValue, Func<T> readProperty, Action<T> writeProperty)
            {
                if(!Object.Equals(readProperty(), configValue)) {
                    raiseConfigurationLoaded = true;
                    writeProperty(configValue);
                }
            }

            // We need to share the repository lock here so that we can establish configuration without the repository getting
            // called mid-change
            lock(_DatabaseLock) {
                assignConfigValue(config.StateHistorySettings.Enabled,              () => Enabled,              r => Enabled = r);
                assignConfigValue(config.StateHistorySettings.NonStandardFolder,    () => NonStandardFolder,    r => NonStandardFolder = r);

                if(raiseConfigurationLoaded) {
                    InitialiseDatabaseWithNewConfiguration();
                }
            }

            if(raiseConfigurationLoaded) {
                ConfigurationLoaded?.Invoke(this, EventArgs.Empty);
            }
        }

        private void LockRepoAndCallIfEnabled(Action<IStateHistoryRepository> action)
        {
            var enabled = Enabled;
            var repository = _Repository;
            if(enabled && repository != null) {
                lock(_DatabaseLock) {
                    action(repository);
                }
            }
        }
    }
}
