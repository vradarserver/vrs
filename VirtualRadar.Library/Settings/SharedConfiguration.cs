// Copyright © 2014 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Library.Settings
{
    /// <summary>
    /// The default implementation of <see cref="ISharedConfiguration"/>.
    /// </summary>
    class SharedConfiguration : ISharedConfiguration
    {
        private readonly IConfigurationStorage _ConfigurationStorage;
        private readonly IHeartbeatService _HeartbeatService;
        private readonly ILog _Log;

        /// <summary>
        /// The lock that stops two threads from simultaenously loading configurations.
        /// </summary>
        private readonly object _SyncLock = new();

        /// <summary>
        /// The current configuration.
        /// </summary>
        private Configuration _Configuration;

        /// <summary>
        /// The date and time (at UTC) when the configuration was loaded.
        /// </summary>
        private DateTime _ConfigurationLoadedUtc;

        /// <summary>
        /// A list of weak references to subscribers of configuration changed events.
        /// </summary>
        private List<WeakReference<ISharedConfigurationSubscriber>> _Subscribers = new();

        /// <inheritdoc/>
        public event EventHandler ConfigurationChanged;

        /// <summary>
        /// Raises <see cref="ConfigurationChanged"/>. Exceptions in the event handlers are logged
        /// but they are not allowed to prevent any other handlers from running.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnConfigurationChanged(EventArgs args)
        {
            EventHelper.Raise(ConfigurationChanged, this, args, ex => {
                _Log.WriteLine($"Caught exception in shared ConfigurationChanged handler: {ex}");
            });
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="configurationStorage"></param>
        /// <param name="heartbeatService"></param>
        /// <param name="log"></param>
        public SharedConfiguration(
            IConfigurationStorage configurationStorage,
            IHeartbeatService heartbeatService,
            ILog log
        )
        {
            _ConfigurationStorage = configurationStorage;
            _HeartbeatService = heartbeatService;
            _Log = log;

            _ConfigurationStorage.ConfigurationChanged += ConfigurationStorage_ConfigurationChanged;
            _HeartbeatService.SlowTick += HeartBeat_SlowTick;
        }

        /// <inheritdoc/>
        public Configuration Get()
        {
            var result = _Configuration;
            if(result == null) {
                LoadConfiguration(forceLoad: false);
                result = _Configuration;
            }

            return result;
        }

        /// <inheritdoc/>
        public DateTime GetConfigurationChangedUtc()
        {
            if(_ConfigurationLoadedUtc == default) {
                LoadConfiguration(forceLoad: false);
            }

            return _ConfigurationLoadedUtc;
        }

        /// <summary>
        /// Loads the current configuration. This is forced onto a single thread.
        /// </summary>
        /// <param name="forceLoad"></param>
        private void LoadConfiguration(bool forceLoad)
        {
            lock(_SyncLock) {
                if(forceLoad || _Configuration == null) {
                    var configuration = _ConfigurationStorage.Load();
                    _ConfigurationLoadedUtc = DateTime.UtcNow;
                    _Configuration = configuration;
                }
            }
        }

        /// <inheritdoc/>
        public void AddWeakSubscription(ISharedConfigurationSubscriber subscriber)
        {
            if(_ConfigurationLoadedUtc == default) {
                LoadConfiguration(forceLoad: false);
            }

            lock(_SyncLock) {
                var subscribers = CollectionHelper.ShallowCopy(_Subscribers);
                subscribers.Add(new WeakReference<ISharedConfigurationSubscriber>(subscriber));
                _Subscribers = subscribers;
            }
        }

        /// <summary>
        /// Calls subscribers to tell them about the configuration change.
        /// </summary>
        private void CallSubscribers()
        {
            var weakReferences = _Subscribers;

            foreach(var weakReference in weakReferences) {
                if(weakReference.TryGetTarget(out var subscriber)) {
                    try {
                        subscriber.SharedConfigurationChanged(this);
                    } catch(Exception ex) {
                        _Log.WriteLine($"Caught exception while calling ISharedConfiguration subscriber: {ex}");
                    }
                }
            }
        }

        /// <summary>
        /// Removes weak references that no longer point at active objects.
        /// </summary>
        private void RemoveDeadSubscribers()
        {
            lock(_SyncLock) {
                var removeList = new List<WeakReference<ISharedConfigurationSubscriber>>();
                foreach(var weakReference in _Subscribers) {
                    if(!weakReference.TryGetTarget(out var subscriber)) {
                        removeList.Add(weakReference);
                    }
                }

                foreach(var removeEntry in removeList) {
                    _Subscribers.Remove(removeEntry);
                }
            }
        }

        /// <summary>
        /// Called when the configuration changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConfigurationStorage_ConfigurationChanged(object sender, EventArgs e)
        {
            LoadConfiguration(forceLoad: true);
            OnConfigurationChanged(EventArgs.Empty);
            CallSubscribers();
        }

        /// <summary>
        /// Called every few seconds by the heartbeat timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HeartBeat_SlowTick(object sender, EventArgs e) => RemoveDeadSubscribers();
    }
}
