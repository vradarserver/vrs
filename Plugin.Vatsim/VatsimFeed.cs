// Copyright © 2022 onwards, Andrew Whewell
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
using VirtualRadar.Interface;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Network;
using VirtualRadar.Plugin.Vatsim.VatsimApiModels;

namespace VirtualRadar.Plugin.Vatsim
{
    /// <summary>
    /// A custom feed that exposes VATSIM pilot states as an aircraft feed.
    /// </summary>
    class VatsimFeed : ICustomFeed
    {
        private bool _FeedEnabled;
        private GeofenceCWH _Geofence;

        /// <summary>
        /// True if the feed is enabled.
        /// </summary>
        public bool FeedEnabled
        {
            get => _FeedEnabled;
            private set {
                if(FeedEnabled != value) {
                    _FeedEnabled = value;
                    OnConnectionStateChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int UniqueId { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets or sets the feed options. This will be null for the master feed.
        /// </summary>
        public GeofenceFeedOption GeofenceFeedOption { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether this is the master (i.e. unfiltered) feed.
        /// </summary>
        public bool IsMasterFeed => GeofenceFeedOption == null;

        /// <summary>
        /// Gets the <see cref="VatsimAircraftList"/> associated with this feed.
        /// </summary>
        internal VatsimAircraftList VatsimAircraftList { get; } = new VatsimAircraftList();

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IAircraftList AircraftList => VatsimAircraftList;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsVisible => true;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public ConnectionStatus ConnectionStatus
        {
            get {
                return FeedEnabled
                    ? VatsimDownloader.Started
                        ? ConnectionStatus.Connected
                        : ConnectionStatus.Connecting
                    : ConnectionStatus.Disconnected;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler ConnectionStateChanged;

        /// <summary>
        /// Raises <see cref="ConnectionStateChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnConnectionStateChanged(EventArgs args) => ConnectionStateChanged?.Invoke(this, args);

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<Exception>> ExceptionCaught;

        /// <summary>
        /// Raises <see cref="ExceptionCaught"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnExceptionCaught(EventArgs<Exception> args) => ExceptionCaught?.Invoke(this, args);

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="feedName"></param>
        public VatsimFeed(string feedName = null)
        {
            SetName(feedName);
        }

        private void SetName(string feedName)
        {
            Name = $"VATSIM: {feedName}";
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="option"></param>
        public VatsimFeed(GeofenceFeedOption option) : this(option.FeedName)
        {
            GeofenceFeedOption = option;
            _Geofence = option.CreateGeofence();
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~VatsimFeed()
        {
            Dispose(false);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                Disconnect();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="uniqueId"></param>
        public void SetUniqueId(int uniqueId)
        {
            UniqueId = uniqueId;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Connect()
        {
            if(!FeedEnabled) {
                VatsimDownloader.DataDownloaded += VatsimDownloader_DataDownloaded;
                FeedEnabled = true;
                AircraftList.Start();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Disconnect()
        {
            if(FeedEnabled) {
                VatsimDownloader.DataDownloaded -= VatsimDownloader_DataDownloaded;
                FeedEnabled = false;
                AircraftList.Stop();
            }
        }

        private void ProcessDownload(EventArgs<VatsimDataV3> args)
        {
            var filteredPilots = args.Value.pilots;

            var feedOption = GeofenceFeedOption;
            if(feedOption != null) {
                filteredPilots = FilterPilots(filteredPilots, feedOption);
            }

            VatsimAircraftList.ApplyPilotStates(filteredPilots);
        }

        /// <summary>
        /// Updates the feed with changed options.
        /// </summary>
        /// <param name="geofencedFeedOption"></param>
        internal void RefreshOptions(GeofenceFeedOption geofencedFeedOption)
        {
            SetName(geofencedFeedOption.FeedName);
            GeofenceFeedOption = geofencedFeedOption;
            _Geofence = GeofenceFeedOption.CreateGeofence();
        }

        private List<VatsimDataV3Pilot> FilterPilots(List<VatsimDataV3Pilot> allPilots, GeofenceFeedOption feedOption)
        {
            var result = new List<VatsimDataV3Pilot>();

            var geofence = _Geofence;
            switch(feedOption.CentreOn) {
                case GeofenceCentreOn.PilotCid:
                    var pilot = allPilots.FirstOrDefault(r => r.cid == feedOption.PilotCid);
                    geofence = feedOption.CreateGeofence(pilot?.latitude, pilot?.longitude);
                    break;
            }

            foreach(var candidate in allPilots) {
                if(geofence.IsWithinBounds(candidate.latitude, candidate.longitude)) {
                    result.Add(candidate);
                }
            }

            return result;
        }

        private void VatsimDownloader_DataDownloaded(object sender, EventArgs<VatsimDataV3> args)
        {
            ProcessDownload(args);
        }
    }
}
