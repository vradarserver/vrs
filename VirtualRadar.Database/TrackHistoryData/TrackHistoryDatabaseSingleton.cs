// Copyright © 2018 onwards, Andrew Whewell
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
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Database.TrackHistoryData
{
    /// <summary>
    /// Default implementation of <see cref="ITrackHistoryDatabaseSingleton"/>.
    /// </summary>
    class TrackHistoryDatabaseSingleton : ITrackHistoryDatabaseSingleton
    {
        /// <summary>
        /// True if the object has been initialised.
        /// </summary>
        private bool _Initialised;

        /// <summary>
        /// The object that tracking the configuration for us.
        /// </summary>
        private ISharedConfiguration _SharedConfiguration;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public ITrackHistoryDatabase Database { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsRecordingEnabled { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler ConfigurationChanging;

        /// <summary>
        /// Raises <see cref="ConfigurationChanging"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnConfigurationChanging(EventArgs args)
        {
            EventHelper.Raise(ConfigurationChanging, this, () => args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler ConfigurationChanged;

        /// <summary>
        /// Raises <see cref="ConfigurationChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnConfigurationChanged(EventArgs args)
        {
            EventHelper.Raise(ConfigurationChanged, this, () => args);
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~TrackHistoryDatabaseSingleton()
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
                Database = null;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Initialise()
        {
            if(!_Initialised) {
                _Initialised = true;

                Database = Factory.Resolve<ITrackHistoryDatabase>();

                _SharedConfiguration = Factory.ResolveSingleton<ISharedConfiguration>();
                ApplyConfiguration(_SharedConfiguration.Get(), raiseEvents: false);
                _SharedConfiguration.ConfigurationChanged += SharedConfiguration_ConfigurationChanged;
            }
        }

        /// <summary>
        /// Applies the configuration passed across.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="raiseEvents"></param>
        private void ApplyConfiguration(Configuration configuration, bool raiseEvents)
        {
            var database = Database;
            if(database != null) {
                var configFileName = configuration.BaseStationSettings.TrackHistoryDatabaseFileName;
                var configConnectionString = configuration.BaseStationSettings.TrackHistoryDatabaseConnectionString;

                var hasConfigChanged =
                       (!database.IsDataSourceReadOnly && database.FileNameRequired && !String.Equals(database.FileName, configFileName))
                    || (!database.IsDataSourceReadOnly && !database.FileNameRequired && !String.Equals(database.ConnectionString, configConnectionString))
                    || IsRecordingEnabled != configuration.BaseStationSettings.TrackHistoryRecordFlights
                ;

                if(hasConfigChanged) {
                    if(raiseEvents) {
                        OnConfigurationChanging(EventArgs.Empty);
                    }

                    IsRecordingEnabled = configuration.BaseStationSettings.TrackHistoryRecordFlights;
                    if(database.FileNameRequired) {
                        database.FileName = configuration.BaseStationSettings.TrackHistoryDatabaseFileName;
                    } else {
                        database.ConnectionString = configuration.BaseStationSettings.TrackHistoryDatabaseConnectionString;
                    }

                    if(raiseEvents) {
                        OnConfigurationChanged(EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Raised when the configuration changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SharedConfiguration_ConfigurationChanged(object sender, EventArgs e)
        {
            ApplyConfiguration(_SharedConfiguration.Get(), raiseEvents: true);
        }
    }
}
