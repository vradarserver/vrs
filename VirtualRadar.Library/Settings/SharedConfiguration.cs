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
using System.Linq;
using System.Text;
using System.Threading;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Library.Settings
{
    /// <summary>
    /// The default implementation of <see cref="ISharedConfiguration"/>.
    /// </summary>
    class SharedConfiguration : ISharedConfiguration
    {
        #region Fields
        /// <summary>
        /// The lock that stops two threads from simultaenously loading configurations.
        /// </summary>
        private object _SyncLock = new Object();

        /// <summary>
        /// The current configuration.
        /// </summary>
        private Configuration _Configuration;

        /// <summary>
        /// The object that loads the configuration.
        /// </summary>
        private IConfigurationStorage _ConfigurationStorage;

        /// <summary>
        /// The object that listens to the configuration for illegal modifications.
        /// </summary>
        private IConfigurationListener _ConfigurationListener;
        #endregion

        #region Properties
        private static ISharedConfiguration _Singleton = new SharedConfiguration();
        /// <summary>
        /// See interface docs.
        /// </summary>
        public ISharedConfiguration Singleton
        {
            get { return _Singleton; }
        }
        #endregion

        #region Events exposed
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler ConfigurationChanged;

        /// <summary>
        /// Raises <see cref="ConfigurationChanged"/>. Exceptions in the event handlers are logged
        /// but they are not allowed to prevent any other handlers from running.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnConfigurationChanged(EventArgs args)
        {
            EventHelper.Raise(ConfigurationChanged, this, args, ex => {
                var log = Factory.ResolveSingleton<ILog>();
                log.WriteLine("Caught exception in shared ConfigurationChanged handler: {0}", ex.ToString());
            });
        }
        #endregion

        #region Get, LoadConfiguration
        /// <summary>
        /// Gets the current configuration.
        /// </summary>
        /// <returns></returns>
        public Configuration Get()
        {
            var result = _Configuration;
            if(result == null) {
                LoadConfiguration(forceLoad: false);
                result = _Configuration;
            }

            return result;
        }

        /// <summary>
        /// Loads the current configuration. This is forced onto a single thread.
        /// </summary>
        /// <param name="forceLoad"></param>
        private void LoadConfiguration(bool forceLoad)
        {
            lock(_SyncLock) {
                if(forceLoad || _Configuration == null) {
                    if(_ConfigurationStorage == null) {
                        _ConfigurationListener = Factory.Resolve<IConfigurationListener>();
                        _ConfigurationListener.PropertyChanged += ConfigurationListener_PropertyChanged;

                        _ConfigurationStorage = Factory.Resolve<IConfigurationStorage>().Singleton;
                        _ConfigurationStorage.ConfigurationChanged += ConfigurationStorage_ConfigurationChanged;
                    }

                    if(_Configuration != null) _ConfigurationListener.Release();

                    var configuration = _ConfigurationStorage.Load();
                    _ConfigurationListener.Initialise(configuration);

                    _Configuration = configuration;
                }
            }
        }
        #endregion

        #region Event handlers
        /// <summary>
        /// Called when the configuration changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConfigurationStorage_ConfigurationChanged(object sender, EventArgs e)
        {
            LoadConfiguration(forceLoad: true);
            OnConfigurationChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Called when the configuration is modified by something. This is not allowed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConfigurationListener_PropertyChanged(object sender, ConfigurationListenerEventArgs e)
        {
            throw new InvalidOperationException(String.Format("Modification of shared configuration detected: {0}{1}",
                e.Record == null ? "" : String.Format("{0}.", e.Record.GetType().Name),
                e.PropertyName
            ));
        }
        #endregion
    }
}
