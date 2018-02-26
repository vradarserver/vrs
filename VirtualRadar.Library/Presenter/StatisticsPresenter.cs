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
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.View;

namespace VirtualRadar.Library.Presenter
{
    /// <summary>
    /// See interface docs.
    /// </summary>
    class StatisticsPresenter : IStatisticsPresenter
    {
        /// <summary>
        /// The view that we're controlling.
        /// </summary>
        private IStatisticsView _View;

        /// <summary>
        /// The object that manages the clock.
        /// </summary>
        private IClock _Clock;

        /// <summary>
        /// The date and time at UTC of the last update of statistics.
        /// </summary>
        private DateTime _LastUpdate;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public StatisticsPresenter()
        {
            _Clock = Factory.Resolve<IClock>();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="view"></param>
        public void Initialise(IStatisticsView view)
        {
            _View = view;
            _View.ResetCountersClicked += View_ResetCountersClicked;
            _View.CloseClicked += View_CloseClicked;
            _View.UpdateCounters();

            Factory.ResolveSingleton<IHeartbeatService>().FastTick += HeartbeatService_FastTick;
        }

        /// <summary>
        /// Updates the view with the latest statistics.
        /// </summary>
        private void DoRefreshView()
        {
            var statistics = _View.Statistics;
            if(statistics != null) {
                statistics.Lock(r => {
                    _View.BytesReceived = r.BytesReceived;
                    _View.ConnectedDuration = r.ConnectionTimeUtc == null ? TimeSpan.Zero : _Clock.UtcNow - r.ConnectionTimeUtc.Value;
                    _View.ReceiverBadChecksum = r.FailedChecksumMessages;
                    _View.CurrentBufferSize = r.CurrentBufferSize;
                    _View.BaseStationMessages = r.BaseStationMessagesReceived;
                    _View.BadlyFormedBaseStationMessages = r.BaseStationBadFormatMessagesReceived;
                    _View.ModeSMessageCount = r.ModeSMessagesReceived;
                    _View.ModeSNoAdsbPayload = r.ModeSNotAdsbCount;
                    _View.ModeSShortFrame = r.ModeSShortFrameMessagesReceived;
                    _View.ModeSShortFrameUnusable = r.ModeSShortFrameWithoutLongFrameMessagesReceived;
                    _View.ModeSLongFrame = r.ModeSLongFrameMessagesReceived;
                    _View.ModeSWithPI = r.ModeSWithPIField;
                    _View.ModeSPIBadParity = r.ModeSWithBadParityPIField;
                    _View.AdsbMessages = r.AdsbCount;
                    _View.AdsbRejected = r.AdsbRejected;
                    _View.PositionSpeedCheckExceeded = r.AdsbPositionsExceededSpeedCheck;
                    _View.PositionsReset = r.AdsbPositionsReset;
                    _View.PositionsOutOfRange = r.AdsbPositionsOutsideRange;
                    Array.Copy(r.AdsbMessageFormatCount, _View.AdsbMessageFormatCount, statistics.AdsbMessageFormatCount.Length);
                    Array.Copy(r.AdsbTypeCount, _View.AdsbMessageTypeCount, statistics.AdsbTypeCount.Length);

                    for(var i = 0;i < r.ModeSDFStatistics.Length;++i) {
                        _View.ModeSDFStatistics[i] = r.ModeSDFStatistics[i].Clone();
                    }
                });

                _View.ReceiverThroughput = CalculateRatio(_View.BytesReceived / 1024.0, _View.ConnectedDuration.TotalSeconds);
                _View.BadlyFormedBaseStationMessagesRatio = CalculateRatio(_View.BadlyFormedBaseStationMessages, _View.BaseStationMessages);
                _View.ModeSNoAdsbPayloadRatio = CalculateRatio(_View.ModeSNoAdsbPayload, _View.ModeSMessageCount);
                _View.ModeSShortFrameUnusableRatio = CalculateRatio(_View.ModeSShortFrameUnusable, _View.ModeSShortFrame);
                _View.ModeSPIBadParityRatio = CalculateRatio(_View.ModeSPIBadParity, _View.ModeSWithPI);
                _View.AdsbRejectedRatio = CalculateRatio(_View.AdsbRejected, _View.AdsbMessages);

                _View.UpdateCounters();
            }
        }

        private double CalculateRatio(double numerator, double denominator)
        {
            return denominator == 0.0 ? 0.0 : numerator / denominator;
        }

        /// <summary>
        /// Resets the counters.
        /// </summary>
        private void DoResetCounters()
        {
            var statistics = _View.Statistics;
            if(statistics != null) {
                statistics.ResetMessageCounters();
                DoRefreshView();
            }
        }

        /// <summary>
        /// Unhooks events.
        /// </summary>
        private void DoShutdown()
        {
            _View.CloseClicked -= View_CloseClicked;
            _View.ResetCountersClicked -= View_ResetCountersClicked;
            Factory.ResolveSingleton<IHeartbeatService>().FastTick -= HeartbeatService_FastTick;
        }

        /// <summary>
        /// Called when the heartbeat fast timer ticks.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HeartbeatService_FastTick(object sender, EventArgs args)
        {
            if((_Clock.UtcNow - _LastUpdate).TotalMilliseconds >= 900) {
                _LastUpdate = _Clock.UtcNow;
                DoRefreshView();
            }
        }

        /// <summary>
        /// Called when the user is closing the form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void View_CloseClicked(object sender, EventArgs args)
        {
            DoShutdown();
        }

        /// <summary>
        /// Called when the user wants to reset the counters.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void View_ResetCountersClicked(object sender, EventArgs args)
        {
            DoResetCounters();
        }
    }
}
