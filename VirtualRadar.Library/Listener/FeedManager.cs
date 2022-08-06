// Copyright © 2013 onwards, Andrew Whewell
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
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Library.Listener
{
    /// <summary>
    /// The default implementation of <see cref="IFeedManager"/>.
    /// </summary>
    class FeedManager : IFeedManager
    {
        #region Fields
        /// <summary>
        /// True after <see cref="Initialise"/> has been called.
        /// </summary>
        private bool _Initialised;
        
        /// <summary>
        /// Locks the <see cref="Feeds"/> list.
        /// </summary>
        private object _SyncLock = new object();
        #endregion

        #region Properties
        private IFeed[] _Feeds;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IFeed[] Feeds
        {
            get {
                IFeed[] result;
                lock(_SyncLock) result = _Feeds;
                return result;
            }
            set {
                lock(_SyncLock) {
                    _Feeds = value;
                    _VisibleFeeds = value.Where(r => r.IsVisible).ToArray();
                }
            }
        }

        private IFeed[] _VisibleFeeds;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IFeed[] VisibleFeeds
        {
            get {
                IFeed[] result;
                lock(_SyncLock) result = _VisibleFeeds;
                return result;
            }
        }
        #endregion

        #region Events exposed
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<IFeed>> ConnectionStateChanged;

        /// <summary>
        /// Raises <see cref="ConnectionStateChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnConnectionStateChanged(EventArgs<IFeed> args)
        {
            EventHelper.Raise(ConnectionStateChanged, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<Exception>> ExceptionCaught;

        /// <summary>
        /// Raises <see cref="ExceptionCaught"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnExceptionCaught(EventArgs<Exception> args)
        {
            EventHelper.Raise(ExceptionCaught, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler FeedsChanged;

        /// <summary>
        /// Raises <see cref="FeedsChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnFeedsChanged(EventArgs args)
        {
            EventHelper.Raise(FeedsChanged, this, args);
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public FeedManager()
        {
            Feeds = new IFeed[0];
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~FeedManager()
        {
            Dispose(false);
        }
        #endregion

        #region Dispose
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
                var configurationStorage = Factory.ResolveSingleton<IConfigurationStorage>();
                configurationStorage.ConfigurationChanged -= ConfigurationStorage_ConfigurationChanged;

                foreach(var feed in Feeds) {
                    feed.ExceptionCaught -= Feed_ExceptionCaught;
                    feed.Dispose();
                }
                Feeds = new IFeed[0];
            }
        }
        #endregion

        #region Initialise, ApplyConfigurationChanges
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Initialise()
        {
            if(_Initialised) throw new InvalidOperationException("The feed manager has already been initialised");

            var configurationStorage = Factory.ResolveSingleton<IConfigurationStorage>();
            var configuration = configurationStorage.Load();
            configurationStorage.ConfigurationChanged += ConfigurationStorage_ConfigurationChanged;

            var feeds = new List<IFeed>();
            foreach(var receiver in configuration.Receivers.Where(r => r.Enabled)) {
                CreateFeedForReceiver(receiver, configuration, feeds);
            }

            var justReceiverFeeds = new List<IFeed>(feeds);
            foreach(var mergedFeed in configuration.MergedFeeds.Where(r => r.Enabled)) {
                CreateFeedForMergedFeed(mergedFeed, justReceiverFeeds, feeds);
            }

            Feeds = feeds.ToArray();
            _Initialised = true;

            OnFeedsChanged(EventArgs.Empty);
        }

        private void CreateFeedForReceiver(Receiver receiver, Configuration configuration, List<IFeed> feeds)
        {
            var feed = Factory.Resolve<IReceiverFeed>();
            feed.Initialise(receiver, configuration);
            AttachFeed(feed, feeds);
            feed.Listener.Connect();
        }

        private void CreateFeedForMergedFeed(MergedFeed mergedFeed, IEnumerable<IFeed> allReceiverPathways, List<IFeed> feeds)
        {
            var mergeFeeds = allReceiverPathways.Where(r => mergedFeed.ReceiverIds.Contains(r.UniqueId)).ToArray();
            var feed = Factory.Resolve<IMergedFeedFeed>();
            feed.Initialise(mergedFeed, mergeFeeds);
            AttachFeed(feed, feeds);
        }

        private void AttachFeed(IFeed feed, List<IFeed> feeds)
        {
            feed.ExceptionCaught += Feed_ExceptionCaught;
            feed.Listener.ConnectionStateChanged += Listener_ConnectionStateChanged;
            feeds.Add(feed);
        }

        /// <summary>
        /// Updates existing feeds, removes dead feeds and adds new feeds after a change in configuration.
        /// </summary>
        /// <param name="configurationStorage"></param>
        private void ApplyConfigurationChanges(IConfigurationStorage configurationStorage)
        {
            var configuration = configurationStorage.Load();
            var configReceiverSettings = configuration.Receivers;
            var configMergedFeedSettings = configuration.MergedFeeds;
            var existingFeeds = new List<IFeed>(Feeds);
            var feeds = new List<IFeed>(existingFeeds);

            for(var pass = 0;pass < 2;++pass) {
                var justReceiverFeeds = pass == 0 ? null : new List<IFeed>(feeds);

                if(pass == 0) {
                    foreach(var newReceiver in configReceiverSettings.Where(r => r.Enabled && !existingFeeds.Any(i => i.UniqueId == r.UniqueId))) {
                        CreateFeedForReceiver(newReceiver, configuration, feeds);
                    }
                } else {
                    foreach(var newMergedFeed in configMergedFeedSettings.Where(r => r.Enabled && !existingFeeds.Any(i => i.UniqueId == r.UniqueId))) {
                        CreateFeedForMergedFeed(newMergedFeed, justReceiverFeeds, feeds);
                    }
                }

                foreach(var feed in existingFeeds) {
                    var receiverConfig = configReceiverSettings.FirstOrDefault(r => r.UniqueId == feed.UniqueId);
                    if(receiverConfig != null && !receiverConfig.Enabled) receiverConfig = null;

                    var mergedFeedConfig = configMergedFeedSettings.FirstOrDefault(r => r.UniqueId == feed.UniqueId);
                    if(mergedFeedConfig != null && !mergedFeedConfig.Enabled) mergedFeedConfig = null;

                    if(receiverConfig != null) {
                        if(pass == 0 && feed is IReceiverFeed receiverFeed) {
                            receiverFeed.ApplyConfiguration(receiverConfig, configuration);
                        }
                    } else if(mergedFeedConfig != null) {
                        if(pass == 1 && feed is IMergedFeedFeed mergedFeedFeed) {
                            var mergeFeeds = justReceiverFeeds.Where(r => mergedFeedConfig.ReceiverIds.Contains(r.UniqueId)).ToList();
                            mergedFeedFeed.ApplyConfiguration(mergedFeedConfig, mergeFeeds);
                        }
                    } else if(pass == 0) {
                        feed.ExceptionCaught -= Feed_ExceptionCaught;
                        feed.Listener.ConnectionStateChanged -= Listener_ConnectionStateChanged;
                        feed.Dispose();
                        feeds.Remove(feed);
                    }
                }
            }

            Feeds = feeds.ToArray();
            OnFeedsChanged(EventArgs.Empty);
        }
        #endregion

        #region GetByName, GetByUniqueId
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ignoreInvisibleFeeds"></param>
        /// <returns></returns>
        public IFeed GetByName(string name, bool ignoreInvisibleFeeds)
        {
            var result = Feeds.FirstOrDefault(r => (r.Name ?? "").Equals(name, StringComparison.CurrentCultureIgnoreCase));
            if(result != null && ignoreInvisibleFeeds && !result.IsVisible) {
                result = null;
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ignoreInvisibleFeeds"></param>
        /// <returns></returns>
        public IFeed GetByUniqueId(int id, bool ignoreInvisibleFeeds)
        {
            var result = Feeds.FirstOrDefault(r => r.UniqueId == id);
            if(result != null && ignoreInvisibleFeeds && !result.IsVisible) {
                result = null;
            }

            return result;
        }
        #endregion

        #region Connect, Disconnect
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Connect()
        {
            foreach(var feed in Feeds) {
                feed.Listener.Connect();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Disconnect()
        {
            foreach(var feed in Feeds) {
                feed.Listener.Disconnect();
            }
        }
        #endregion

        #region Events subscribed
        /// <summary>
        /// Raised when a feed picks up an exception.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Feed_ExceptionCaught(object sender, EventArgs<Exception> args)
        {
            OnExceptionCaught(args);
        }

        /// <summary>
        /// Raised when the configuration has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ConfigurationStorage_ConfigurationChanged(object sender, EventArgs args)
        {
            ApplyConfigurationChanges((IConfigurationStorage)sender);
        }

        /// <summary>
        /// Raised when a listener raised ConnectionStateChanged.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Listener_ConnectionStateChanged(object sender, EventArgs args)
        {
            var feed = _Feeds.FirstOrDefault(r => r.Listener == sender);
            if(feed != null) OnConnectionStateChanged(new EventArgs<IFeed>(feed));
        }
        #endregion
    }
}
