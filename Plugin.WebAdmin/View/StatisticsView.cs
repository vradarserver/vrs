// Copyright © 2016 onwards, Andrew Whewell
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
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Adsb;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.ModeS;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Plugin.WebAdmin.View.Statistics;

namespace VirtualRadar.Plugin.WebAdmin.View
{
    class StatisticsView : IStatisticsView
    {
        private IStatisticsPresenter _Presenter;

        private IFeedManager _FeedManager;

        private int _FeedId = -1;

        private ViewModel _ViewModel;

        public IStatistics Statistics { get; set; }

        public long BytesReceived { get; set; }

        public TimeSpan ConnectedDuration { get; set; }

        public double ReceiverThroughput { get; set; }

        public long ReceiverBadChecksum { get; set; }

        public long CurrentBufferSize { get; set; }

        public long BaseStationMessages { get; set; }

        public long BadlyFormedBaseStationMessages { get; set; }

        public double BadlyFormedBaseStationMessagesRatio { get; set; }

        public long ModeSMessageCount { get; set; }

        public long ModeSNoAdsbPayload { get; set; }

        public double ModeSNoAdsbPayloadRatio { get; set; }

        public long ModeSShortFrame { get; set; }

        public long ModeSShortFrameUnusable { get; set; }

        public double ModeSShortFrameUnusableRatio { get; set; }

        public long ModeSLongFrame { get; set; }

        public long ModeSWithPI { get; set; }

        public long ModeSPIBadParity { get; set; }

        public double ModeSPIBadParityRatio { get; set; }

        public ModeSDFStatistics[] ModeSDFStatistics { get; private set; }

        public long AdsbMessages { get; set; }

        public long AdsbRejected { get; set; }

        public double AdsbRejectedRatio { get; set; }

        public long PositionSpeedCheckExceeded { get; set; }

        public long PositionsReset { get; set; }

        public long PositionsOutOfRange { get; set; }

        public long[] AdsbMessageTypeCount { get; private set; }

        public long[] AdsbMessageFormatCount { get; private set; }

        // Non-standard properties
        public string Name { get; set; }

        public event EventHandler CloseClicked;
        private void OnCloseClicked(EventArgs args)
        {
            EventHelper.Raise(CloseClicked, this, args);
        }

        public event EventHandler ResetCountersClicked;
        private void OnResetCountersClicked(EventArgs args)
        {
            EventHelper.Raise(ResetCountersClicked, this, args);
        }

        public StatisticsView()
        {
            _FeedManager = Factory.Singleton.Resolve<IFeedManager>().Singleton;

            AdsbMessageTypeCount = new long[256];
            ModeSDFStatistics = new ModeSDFStatistics[32];
            AdsbMessageFormatCount = new long[Enum.GetValues(typeof(MessageFormat)).OfType<MessageFormat>().Select(r => (int)r).Max() + 1];
        }

        public DialogResult ShowView()
        {
            _Presenter = Factory.Singleton.Resolve<IStatisticsPresenter>();
            _Presenter.Initialise(this);

            return DialogResult.OK;
        }

        public void Dispose()
        {
            OnCloseClicked(EventArgs.Empty);
        }

        public void UpdateCounters()
        {
            if(_FeedId > -1) {
                var feed = _FeedManager.GetByUniqueId(_FeedId, ignoreInvisibleFeeds: false);
                Name = feed == null ? "" : feed.Name;
            }

            if(_ViewModel == null) {
                _ViewModel = new ViewModel(this);
            } else {
                _ViewModel.RefreshModel(this);
            }
        }

        [WebAdminMethod]
        public void RegisterFeedId(int feedId)
        {
            var feed = _FeedManager.GetByUniqueId(feedId, ignoreInvisibleFeeds: false);
            if(feed != null && feed.Listener != null) {
                Statistics = feed.Listener.Statistics;
            }

            _FeedId = feedId;
            UpdateCounters();
        }

        [WebAdminMethod]
        public ViewModel GetState()
        {
            return _ViewModel;
        }

        [WebAdminMethod]
        public void RaiseResetCountersClicked()
        {
            OnResetCountersClicked(EventArgs.Empty);
        }
    }
}
