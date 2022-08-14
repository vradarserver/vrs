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
using System.Text;
using System.Threading.Tasks;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Network;
using VirtualRadar.Plugin.Vatsim.VatsimApiModels;

namespace VirtualRadar.Plugin.Vatsim
{
    class VatsimFeed : ICustomFeed
    {
        private bool _FeedEnabled;

        internal bool FeedEnabled
        {
            get => _FeedEnabled;
            private set {
                if(FeedEnabled != value) {
                    _FeedEnabled = value;
                    OnConnectionStateChanged(EventArgs.Empty);
                }
            }
        }

        public int UniqueId { get; private set; }

        public string Name { get; internal set; } = "VATSIM Feed";

        internal VatsimAircraftList VatsimAircraftList { get; } = new VatsimAircraftList();

        public IAircraftList AircraftList => VatsimAircraftList;

        public bool IsVisible => true;

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

        public event EventHandler ConnectionStateChanged;

        protected virtual void OnConnectionStateChanged(EventArgs args) => ConnectionStateChanged?.Invoke(this, args);

        public event EventHandler<EventArgs<Exception>> ExceptionCaught;

        protected virtual void OnExceptionCaught(EventArgs<Exception> args) => ExceptionCaught?.Invoke(this, args);

        public void Dispose()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                Disconnect();
            }
        }

        public void SetUniqueId(int uniqueId)
        {
            UniqueId = uniqueId;
        }

        public void Connect()
        {
            if(!FeedEnabled) {
                VatsimDownloader.DataDownloaded += VatsimDownloader_DataDownloaded;
                FeedEnabled = true;
                AircraftList.Start();
            }
        }

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
            VatsimAircraftList.ApplyPilotStates(args.Value.pilots);
        }

        private void VatsimDownloader_DataDownloaded(object sender, EventArgs<VatsimDataV3> args)
        {
            ProcessDownload(args);
        }
    }
}
