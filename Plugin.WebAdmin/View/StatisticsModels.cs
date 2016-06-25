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

        public ModeSDFCountModel[] ModeSDFCount { get; private set; }

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

            this.AdsbMessageTypeCount =     SetArray(view.AdsbMessageTypeCount, (idx, value) => new AdsbMessageTypeCountModel(idx, value));
            this.AdsbMessageFormatCount =   SetArray(view.AdsbMessageFormatCount, (idx, value) => new AdsbMessageFormatCountModel((MessageFormat)idx, value));
            this.ModeSDFCount =             SetArray(view.ModeSDFCount, (idx, value) => new ModeSDFCountModel((DownlinkFormat)idx, value));
        }

        private T[] SetArray<T>(long[] sourceArray, Func<int, long, T> createModel)
        {
            var result = new List<T>();

            for(var i = 0;i < sourceArray.Length;++i) {
                var value = sourceArray[i];
                if(value != 0) {
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

    public class ModeSDFCountModel
    {
        public int DF { get; set; }

        public long Val { get; set; }

        public ModeSDFCountModel(DownlinkFormat format, long value)
        {
            DF = (int)format;
            Val = value;
        }
    }
}
