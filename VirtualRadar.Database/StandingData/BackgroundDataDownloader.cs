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
using System.Diagnostics;
using System.Linq;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;

namespace VirtualRadar.Database.StandingData
{
    /// <summary>
    /// The default implementation of <see cref="IBackgroundDataDownloader"/>.
    /// </summary>
    class BackgroundDataDownloader : IBackgroundDataDownloader
    {
        /// <summary>
        /// The default implementation of the provider.
        /// </summary>
        class DefaultProvider : IBackgroundDataDownloaderProvider
        {
            public DateTime UtcNow { get { return DateTime.UtcNow; } }
        }

        /// <summary>
        /// The time of the last update.
        /// </summary>
        private DateTime _LastUpdateTime;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IBackgroundDataDownloaderProvider Provider { get; set; }

        private static readonly IBackgroundDataDownloader _Singleton = new BackgroundDataDownloader();
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IBackgroundDataDownloader Singleton { get { return _Singleton; } }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public BackgroundDataDownloader()
        {
            Provider = new DefaultProvider();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Start()
        {
            Factory.ResolveSingleton<IHeartbeatService>().SlowTick += Heartbeat_SlowTick;
        }

        /// <summary>
        /// Called every time the heartbeat timer ticks.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Heartbeat_SlowTick(object sender, EventArgs args)
        {
            if(Factory.ResolveSingleton<IConfigurationStorage>().Load().FlightRouteSettings.AutoUpdateEnabled) {
                if(_LastUpdateTime.AddHours(1) <= Provider.UtcNow) {
                    try {
                        _LastUpdateTime = Provider.UtcNow;
                        var updater = Factory.Resolve<IStandingDataUpdater>();
                        updater.Update();
                    } catch(Exception ex) {
                        Debug.WriteLine(String.Format("BackgroundDataDownloader.Heartbeat_SlowTick caught exception: {0}", ex.ToString()));
                        Factory.ResolveSingleton<ILog>().WriteLine("Exception caught during data download: {0}", ex.ToString());
                    }
                }
            }
        }
    }
}
