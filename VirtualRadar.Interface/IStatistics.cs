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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// A class that records various statistics.
    /// </summary>
    public interface IStatistics
    {
        /// <summary>
        /// Gets or sets the date and time at UTC when the connection to the listener was first established.
        /// </summary>
        DateTime? ConnectionTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the number of bytes received by the listener.
        /// </summary>
        long BytesReceived { get; set; }

        /// <summary>
        /// Gets or sets the size in bytes of the receiver buffers maintained by the listener, either directly or indirectly.
        /// </summary>
        long CurrentBufferSize { get; set; }

        /// <summary>
        /// Gets or sets a count of BaseStation format messages received.
        /// </summary>
        long BaseStationMessagesReceived { get; set; }

        /// <summary>
        /// Gets or sets a count of badly formatted BaseStation messages received.
        /// </summary>
        long BaseStationBadFormatMessagesReceived { get; set; }

        /// <summary>
        /// Gets or sets the number of Mode-S messages received from a source of Mode-S data.
        /// </summary>
        long ModeSMessagesReceived { get; set; }

        /// <summary>
        /// Gets or sets the number of long-frame Mode-S messages seen on the data feed.
        /// </summary>
        long ModeSLongFrameMessagesReceived { get; set; }

        /// <summary>
        /// Gets or sets the count of Mode-S messages with a PI field.
        /// </summary>
        long ModeSWithPIField { get; set; }

        /// <summary>
        /// Gets or sets the number of Mode-S messages with a PI field that was not zero.
        /// </summary>
        long ModeSWithBadParityPIField { get; set; }

        /// <summary>
        /// Gets or sets the number of short-frame Mode-S messages seen on the data feed.
        /// </summary>
        long ModeSShortFrameMessagesReceived { get; set; }

        /// <summary>
        /// Gets or sets the number of short-frame Mode-S messages that were discarded because no prior long-frame message had been received.
        /// </summary>
        /// <remarks>
        /// It will be normal for this to go up continuously, even on a good feed.
        /// </remarks>
        long ModeSShortFrameWithoutLongFrameMessagesReceived { get; set; }

        /// <summary>
        /// Gets or sets the number of messages that had a bad checksum.
        /// </summary>
        /// <remarks>
        /// This will only be of interest with radios that apply a checksum to their messages, e.g. the SBS-3.
        /// </remarks>
        long FailedChecksumMessages { get; set; }

        /// <summary>
        /// Gets an array, indexable by (int)<see cref="VirtualRadar.Interface.ModeS.DownlinkFormat"/>, recording the count of each Mode-S message type received.
        /// </summary>
        long[] ModeSDFCount { get; }

        /// <summary>
        /// Gets or sets the count of ADS-B messages decoded.
        /// </summary>
        long AdsbCount { get; set; }

        /// <summary>
        /// Gets or sets the count of ADS-B messages that were rejected because of bad parity.
        /// </summary>
        long AdsbRejected { get; set; }

        /// <summary>
        /// Gets or sets the count of aircraft transmitting ADS-B messages that are being tracked;
        /// </summary>
        long AdsbAircraftTracked { get; set; }

        /// <summary>
        /// Gets or sets the count of position resets that have taken place because position fixes for aircraft failed a reasonableness test.
        /// </summary>
        long AdsbPositionsReset { get; set; }

        /// <summary>
        /// Gets or sets the count of positions that were rejected because they lay outside the range of the receiver.
        /// </summary>
        long AdsbPositionsOutsideRange { get; set; }

        /// <summary>
        /// Gets or sets the count of positions that were rejected because the vehicle would have to have been travelling unrealistically quickly to reach the position.
        /// </summary>
        long AdsbPositionsExceededSpeedCheck { get; set; }

        /// <summary>
        /// Gets or sets the count of Mode-S messages that were not carrying an ADS-B payload.
        /// </summary>
        long ModeSNotAdsbCount { get; set; }

        /// <summary>
        /// Gets an array, indexable by (int)<see cref="VirtualRadar.Interface.Adsb.AdsbMessage"/>.Type, recording the count of each ADS-B message type received.
        /// </summary>
        long[] AdsbTypeCount { get; }

        /// <summary>
        /// Gets an array, indexable by (int)<see cref="VirtualRadar.Interface.Adsb.MessageFormat"/>, recording the count of each ADS-B message format received.
        /// </summary>
        long[] AdsbMessageFormatCount { get; }

        /// <summary>
        /// Prepares the statistics for first use.
        /// </summary>
        void Initialise();

        /// <summary>
        /// Calls the delegate within a lock. The delegate must not, under any circumstances,
        /// call <see cref="Lock "/> or do anything that might call <see cref="Lock"/>.
        /// A deadlock may occur if it does. Does nothing if the statistics have not been
        /// initialised.
        /// </summary>
        /// <param name="lockedDelegate"></param>
        void Lock(Action<IStatistics> lockedDelegate);

        /// <summary>
        /// Resets all counters associated with listening to a source of messages. Does not reset connection statistics.
        /// </summary>
        void ResetMessageCounters();

        /// <summary>
        /// Resets connection statistics.
        /// </summary>
        void ResetConnectionStatistics();
    }
}
