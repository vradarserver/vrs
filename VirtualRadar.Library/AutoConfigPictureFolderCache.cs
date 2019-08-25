// Copyright © 2012 onwards, Andrew Whewell
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

namespace VirtualRadar.Library
{
    /// <summary>
    /// The default implementation of <see cref="IAutoConfigPictureFolderCache"/>.
    /// </summary>
    sealed class AutoConfigPictureFolderCache : IAutoConfigPictureFolderCache
    {
        private readonly static IAutoConfigPictureFolderCache _Singleton = new AutoConfigPictureFolderCache();
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IAutoConfigPictureFolderCache Singleton
        {
            get { return _Singleton; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IDirectoryCache DirectoryCache { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler CacheConfigurationChanged;

        /// <summary>
        /// Raises <see cref="CacheConfigurationChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnCacheConfigurationChanged(EventArgs args)
        {
            EventHelper.Raise(CacheConfigurationChanged, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Initialise()
        {
            if(DirectoryCache == null) {
                DirectoryCache = Factory.Resolve<IDirectoryCache>();

                LoadConfiguration();

                var configStorage = Factory.Resolve<IConfigurationStorage>().Singleton;
                configStorage.ConfigurationChanged += ConfigurationStorage_ConfigurationChanged;
            }
        }

        /// <summary>
        /// Loads the configuration into the directory cache.
        /// </summary>
        private void LoadConfiguration()
        {
            var configStorage = Factory.Resolve<IConfigurationStorage>().Singleton;
            var config = configStorage.Load();

            // On testing over a slow VPN link it was found that this would cause the options screen to appear to hang -
            // DirectoryCache properties block if there is a background thread running that's using the values. In an
            // ideal world this would be fixed by aborting any caching that's in progress and starting a new caching-up
            // of filenames when one of the properties is set, but unfortunately I'm very near to release date and I don't
            // want to mess about with anything too much. So. For now I'll set the properties on a background thread.
            // TODO: Change DirectoryCache so that if a property is changed while a background thread is caching up then
            // the background process is aborted and a new process started straight away, rather than blocking until the
            // background thread has completed.
            if(!ThreadPool.QueueUserWorkItem((object state) => {
                try {
                    var refreshTriggered = DirectoryCache.SetConfiguration(config.BaseStationSettings.PicturesFolder, config.BaseStationSettings.SearchPictureSubFolders);
                    if(refreshTriggered) OnCacheConfigurationChanged(EventArgs.Empty);
                } catch(Exception ex) {
                    var log = Factory.Resolve<ILog>().Singleton;
                    log.WriteLine("Caught exception while trying to set the directory cache properties: {0}", ex.ToString());
                }
            })) {
                var refreshTriggered = DirectoryCache.SetConfiguration(config.BaseStationSettings.PicturesFolder, config.BaseStationSettings.SearchPictureSubFolders);
                if(refreshTriggered) OnCacheConfigurationChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called when the configuration changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ConfigurationStorage_ConfigurationChanged(object sender, EventArgs args)
        {
            LoadConfiguration();
        }
    }
}
