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
using VirtualRadar.Interface.Adsb;

namespace VirtualRadar.Interface.View
{
    /// <summary>
    /// The interface for the view that displays the statistics and refreshes the display on a timer.
    /// </summary>
    public interface IStatisticsView : IView
    {
        /// <summary>
        /// Gets or sets the statistics object that the view will display.
        /// </summary>
        IStatistics Statistics { get; set; }

        /// <summary>
        /// Gets or sets the number of bytes received.
        /// </summary>
        long BytesReceived { get; set; }

        /// <summary>
        /// Gets or sets the span of time that has elapsed since the user first connected.
        /// </summary>
        TimeSpan ConnectedDuration { get; set; }

        /// <summary>
        /// Gets or sets the throughput from the receiver in KB/sec.
        /// </summary>
        double ReceiverThroughput { get; set; }

        /// <summary>
        /// Gets or sets the number of messages from a receiver that adds a checksum during transmission which had a bad checksum.
        /// </summary>
        long ReceiverBadChecksum { get; set; }

        /// <summary>
        /// Gets or sets the current buffer size.
        /// </summary>
        long CurrentBufferSize { get; set; }

        /// <summary>
        /// Gets or sets the count of exceptions thrown by the connector.
        /// </summary>
        long ConnectorExceptionCount { get; set; }

        /// <summary>
        /// Gets or sets the last exception thrown by the connector.
        /// </summary>
        Exception ConnectorLastException { get; set; }

        /// <summary>
        /// Gets or sets the count of BaseStation messages received.
        /// </summary>
        long BaseStationMessages { get; set; }

        /// <summary>
        /// Gets or sets the count of BaseStation messages that were badly formed.
        /// </summary>
        long BadlyFormedBaseStationMessages { get; set; }

        /// <summary>
        /// Gets or sets the %age of BaseStation messages that were badly formed.
        /// </summary>
        double BadlyFormedBaseStationMessagesRatio { get; set; }

        /// <summary>
        /// Gets or sets the number of Mode-S messages seen.
        /// </summary>
        long ModeSMessageCount { get; set; }

        /// <summary>
        /// Gets or sets the number of Mode-S messages that were not DF17.
        /// </summary>
        long ModeSNoAdsbPayload { get; set; }

        /// <summary>
        /// Gets or sets the %age of Mode-S messages that were not DF17.
        /// </summary>
        double ModeSNoAdsbPayloadRatio { get; set; }

        /// <summary>
        /// Gets or sets the number of short-frame Mode-S messages.
        /// </summary>
        long ModeSShortFrame { get; set; }

        /// <summary>
        /// Gets or sets the number of short-frame Mode-S messages that VRS couldn't use.
        /// </summary>
        long ModeSShortFrameUnusable { get; set; }

        /// <summary>
        /// Gets or sets the %age of short-frame Mode-S messages that VRS couldn't use.
        /// </summary>
        double ModeSShortFrameUnusableRatio { get; set; }

        /// <summary>
        /// Gets or sets the number of long-frame Mode-S messages.
        /// </summary>
        long ModeSLongFrame { get; set; }

        /// <summary>
        /// Gets or sets the number of Mode-S messages that have a PI field, both long and short frame.
        /// </summary>
        long ModeSWithPI { get; set; }

        /// <summary>
        /// Gets or sets the number of Mode-S messages with a PI field that indicated bad parity.
        /// </summary>
        long ModeSPIBadParity { get; set; }

        /// <summary>
        /// Gets or sets the %age of Mode-S messages with a PI field that had bad parity.
        /// </summary>
        double ModeSPIBadParityRatio { get; set; }

        /// <summary>
        /// Gets an array of counters of Mode-S messages indexed by the message's DF.
        /// </summary>
        long[] ModeSDFCount { get; }

        /// <summary>
        /// Gets or sets the count of ADS-B messages seen.
        /// </summary>
        long AdsbMessages { get; set; }

        /// <summary>
        /// Gets or sets the count of ADS-B messages that were not used.
        /// </summary>
        long AdsbRejected { get; set; }

        /// <summary>
        /// Gets or sets the %age of ADS-B messages that were rejected, either because of bad parity or settings.
        /// </summary>
        double AdsbRejectedRatio { get; set; }

        /// <summary>
        /// Gets or sets the number of ADS-B position messages that exceeded the ICAO speed check.
        /// </summary>
        long PositionSpeedCheckExceeded { get; set; }

        /// <summary>
        /// Gets or sets the number of ADS-B position messages that triggered a position reset.
        /// </summary>
        long PositionsReset { get; set; }

        /// <summary>
        /// Gets or sets the number of ADS-B position messages that were rejected because the position was out of range of the receiver.
        /// </summary>
        long PositionsOutOfRange { get; set; }

        /// <summary>
        /// Gets an array of counters of ADS-B messages against their message type.
        /// </summary>
        long[] AdsbMessageTypeCount { get; }

        /// <summary>
        /// Gets an array of counters of ADS-B messages against their message format.
        /// </summary>
        long[] AdsbMessageFormatCount { get; }

        /// <summary>
        /// Raised when the user closes the view.
        /// </summary>
        event EventHandler CloseClicked;

        /// <summary>
        /// Raised when the user wants to reset the counters.
        /// </summary>
        event EventHandler ResetCountersClicked;

        /// <summary>
        /// Refreshes the display of counters.
        /// </summary>
        void UpdateCounters();
    }
}
