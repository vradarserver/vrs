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
using VirtualRadar.Interface;
using VirtualRadar.Interface.Adsb;
using VirtualRadar.Interface.ModeS;

namespace VirtualRadar.Plugin.WebAdmin.View.Statistics
{
    public class ViewModel
    {
        public string Name { get; set; }

        public long BytesReceived { get; set; }

        public string ConnectedDuration { get; set; }

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

        public ModeSDFStatisticsModel[] ModeSDFStatistics { get; private set; }

        public long AdsbMessages { get; set; }

        public long AdsbRejected { get; set; }

        public double AdsbRejectedRatio { get; set; }

        public long PositionSpeedCheckExceeded { get; set; }

        public long PositionsReset { get; set; }

        public long PositionsOutOfRange { get; set; }

        public AdsbMessageTypeCountModel[] AdsbMessageTypeCount { get; private set; }

        public AdsbMessageFormatCountModel[] AdsbMessageFormatCount { get; private set; }

        internal ViewModel(StatisticsView view)
        {
            RefreshModel(view);
        }

        internal void RefreshModel(StatisticsView view)
        {
            this.AdsbMessages =                         view.AdsbMessages;
            this.AdsbRejected =                         view.AdsbRejected;
            this.AdsbRejectedRatio =                    view.AdsbRejectedRatio;
            this.BadlyFormedBaseStationMessages =       view.BadlyFormedBaseStationMessages;
            this.BadlyFormedBaseStationMessagesRatio =  view.BadlyFormedBaseStationMessagesRatio;
            this.BaseStationMessages =                  view.BaseStationMessages;
            this.BytesReceived =                        view.BytesReceived;
            this.ConnectedDuration =                    String.Format("{0:00}:{1:00}:{2:00}", (long)view.ConnectedDuration.TotalHours, view.ConnectedDuration.Minutes, view.ConnectedDuration.Seconds);
            this.CurrentBufferSize =                    view.CurrentBufferSize;
            this.ModeSLongFrame =                       view.ModeSLongFrame;
            this.ModeSMessageCount =                    view.ModeSMessageCount;
            this.ModeSNoAdsbPayload =                   view.ModeSNoAdsbPayload;
            this.ModeSNoAdsbPayloadRatio =              view.ModeSNoAdsbPayloadRatio;
            this.ModeSPIBadParity =                     view.ModeSPIBadParity;
            this.ModeSPIBadParityRatio =                view.ModeSPIBadParityRatio;
            this.ModeSShortFrame =                      view.ModeSShortFrame;
            this.ModeSShortFrameUnusable =              view.ModeSShortFrameUnusable;
            this.ModeSShortFrameUnusableRatio =         view.ModeSShortFrameUnusableRatio;
            this.ModeSWithPI =                          view.ModeSWithPI;
            this.Name =                                 view.Name;
            this.PositionsOutOfRange =                  view.PositionsOutOfRange;
            this.PositionSpeedCheckExceeded =           view.PositionSpeedCheckExceeded;
            this.PositionsReset =                       view.PositionsReset;
            this.ReceiverBadChecksum =                  view.ReceiverBadChecksum;
            this.ReceiverThroughput =                   view.ReceiverThroughput;

            this.AdsbMessageTypeCount =     SetArray(view.AdsbMessageTypeCount, value => value != 0L, (idx, value) => new AdsbMessageTypeCountModel(idx, value));
            this.AdsbMessageFormatCount =   SetArray(view.AdsbMessageFormatCount, value => value != 0L, (idx, value) => new AdsbMessageFormatCountModel((MessageFormat)idx, value));
            this.ModeSDFStatistics =        SetArray(
                view.ModeSDFStatistics,
                value => value != null && (value.MessagesReceived != 0 || value.BadParityPI != 0),
                (idx, value) => new ModeSDFStatisticsModel(value)
            );
        }

        private TModel[] SetArray<TModel, TSource>(TSource[] sourceArray, Func<TSource, bool> showSourceValue, Func<int, TSource, TModel> createModel)
        {
            var result = new List<TModel>();

            for(var i = 0;i < sourceArray.Length;++i) {
                var value = sourceArray[i];
                if(showSourceValue(value)) {
                    result.Add(createModel(i, value));
                }
            }

            return result.ToArray();
        }
    }

    public class AdsbMessageTypeCountModel
    {
        public int N { get; set; }

        public long Val { get; set; }

        public AdsbMessageTypeCountModel(int idx, long value)
        {
            N = idx;
            Val = value;
        }
    }

    public class AdsbMessageFormatCountModel
    {
        public string Fmt { get; set; }

        public long Val { get; set; }

        public AdsbMessageFormatCountModel(MessageFormat format, long value)
        {
            Fmt = format.ToString();
            Val = value;
        }
    }

    public class ModeSDFStatisticsModel
    {
        public int DF { get; set; }

        public string DFName { get; set; }

        public long MessagesReceived { get; set; }

        public long BadParityPI { get; set; }

        public ModeSDFStatisticsModel(ModeSDFStatistics value)
        {
            DF =                (int)value.DF;
            DFName =            Enum.GetName(typeof(DownlinkFormat), value.DF);
            MessagesReceived =  value.MessagesReceived;
            BadParityPI =       value.BadParityPI;
        }
    }
}
