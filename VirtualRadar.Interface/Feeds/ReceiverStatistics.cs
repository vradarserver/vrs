// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Interface.Adsb;
using VirtualRadar.Interface.ModeS;

namespace VirtualRadar.Interface.Feeds
{
    /// <summary>
    /// A class that records various statistics gleaned from messages interpreted by a receiver.
    /// </summary>
    public class ReceiverStatistics
    {
        /// <summary>
        /// The lock that controls updates of the values.
        /// </summary>
        private object _SyncLock = new();

        /// <summary>
        /// Gets or sets the date and time at UTC when the connection to the listener was first established.
        /// </summary>
        public DateTime? ConnectionTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the number of bytes received by the listener.
        /// </summary>
        public long BytesReceived { get; set; }

        /// <summary>
        /// Gets or sets the size in bytes of the receiver buffers maintained by the listener, either directly or indirectly.
        /// </summary>
        public long CurrentBufferSize { get; set; }

        /// <summary>
        /// Gets or sets a count of BaseStation format messages received.
        /// </summary>
        public long BaseStationMessagesReceived { get; set; }

        /// <summary>
        /// Gets or sets a count of badly formatted BaseStation messages received.
        /// </summary>
        public long BaseStationBadFormatMessagesReceived { get; set; }

        /// <summary>
        /// Gets or sets the number of Mode-S messages received from a source of Mode-S data.
        /// </summary>
        public long ModeSMessagesReceived { get; set; }

        /// <summary>
        /// Gets or sets the number of long-frame Mode-S messages seen on the data feed.
        /// </summary>
        public long ModeSLongFrameMessagesReceived { get; set; }

        /// <summary>
        /// Gets or sets the count of Mode-S messages with a PI field.
        /// </summary>
        public long ModeSWithPIField { get; set; }

        /// <summary>
        /// Gets or sets the number of Mode-S messages with a PI field that was not zero.
        /// </summary>
        public long ModeSWithBadParityPIField { get; set; }

        /// <summary>
        /// Gets or sets the number of short-frame Mode-S messages seen on the data feed.
        /// </summary>
        public long ModeSShortFrameMessagesReceived { get; set; }

        /// <summary>
        /// Gets or sets the number of short-frame Mode-S messages that were discarded because no prior long-frame message had been received.
        /// </summary>
        /// <remarks>
        /// It will be normal for this to go up continuously, even on a good feed.
        /// </remarks>
        public long ModeSShortFrameWithoutLongFrameMessagesReceived { get; set; }

        /// <summary>
        /// Gets or sets the number of messages that had a bad checksum.
        /// </summary>
        /// <remarks>
        /// This will only be of interest with radios that apply a checksum to their messages, e.g. the SBS-3.
        /// </remarks>
        public long FailedChecksumMessages { get; set; }

        /// <summary>
        /// Gets an array, indexable by (int)<see cref="VirtualRadar.Interface.ModeS.DownlinkFormat"/>, recording statistics for each Mode-S message type received.
        /// </summary>
        /// <remarks>
        /// Writes to the elements of the array should be protected by the same locking mechanism that protects
        /// writes to the statistics object.
        /// </remarks>
        public ModeSDFStatistics[] ModeSDFStatistics { get; } = new ModeSDFStatistics[32];

        /// <summary>
        /// Gets or sets the count of ADS-B messages decoded.
        /// </summary>
        public long AdsbCount { get; set; }

        /// <summary>
        /// Gets or sets the count of ADS-B messages that were rejected because of bad parity.
        /// </summary>
        public long AdsbRejected { get; set; }

        /// <summary>
        /// Gets or sets the count of aircraft transmitting ADS-B messages that are being tracked;
        /// </summary>
        public long AdsbAircraftTracked { get; set; }

        /// <summary>
        /// Gets or sets the count of position resets that have taken place because position fixes for aircraft failed a reasonableness test.
        /// </summary>
        public long AdsbPositionsReset { get; set; }

        /// <summary>
        /// Gets or sets the count of positions that were rejected because they lay outside the range of the receiver.
        /// </summary>
        public long AdsbPositionsOutsideRange { get; set; }

        /// <summary>
        /// Gets or sets the count of positions that were rejected because the vehicle would have to have been travelling unrealistically quickly to reach the position.
        /// </summary>
        public long AdsbPositionsExceededSpeedCheck { get; set; }

        /// <summary>
        /// Gets or sets the count of Mode-S messages that were not carrying an ADS-B payload.
        /// </summary>
        public long ModeSNotAdsbCount { get; set; }

        /// <summary>
        /// Gets an array, indexable by (int)<see cref="VirtualRadar.Interface.Adsb.AdsbMessage"/>.Type, recording the count of each ADS-B message type received.
        /// </summary>
        public long[] AdsbTypeCount { get; } = new long[256];

        /// <summary>
        /// Gets an array, indexable by (int)<see cref="VirtualRadar.Interface.Adsb.MessageFormat"/>, recording the count of each ADS-B message format received.
        /// </summary>
        public long[] AdsbMessageFormatCount { get; } = new long[
            Enum.GetValues<MessageFormat>()
                .Select(r => (int)r)
                .Max() + 1
        ];

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ReceiverStatistics()
        {
            ResetModeSDFStatistics();
        }

        private void ResetModeSDFStatistics()
        {
            for(var i = 0;i < ModeSDFStatistics.Length;++i) {
                ModeSDFStatistics[i] = new ModeSDFStatistics() {
                    DF = (DownlinkFormat)i
                };
            }
        }

        /// <summary>
        /// Calls the delegate within a lock. The delegate must not, under any circumstances,
        /// call <see cref="Lock "/> or do anything that might call <see cref="Lock"/>.
        /// A deadlock may occur if it does. Does nothing if the statistics have not been
        /// initialised.
        /// </summary>
        /// <param name="lockedDelegate"></param>
        public void Lock(Action<ReceiverStatistics> lockedDelegate)
        {
            lock(_SyncLock) {
                lockedDelegate(this);
            }
        }

        /// <summary>
        /// Resets all counters associated with listening to a source of messages. Does not reset connection statistics.
        /// </summary>
        public void ResetMessageCounters()
        {
            Lock(_ => {
                Array.Clear(AdsbTypeCount, 0, AdsbTypeCount.Length);
                Array.Clear(AdsbMessageFormatCount, 0, AdsbMessageFormatCount.Length);
                ResetModeSDFStatistics();
                AdsbAircraftTracked = 0;
                AdsbCount = 0;
                AdsbPositionsExceededSpeedCheck = 0;
                AdsbPositionsOutsideRange = 0;
                AdsbPositionsReset = 0;
                AdsbRejected = 0;
                BaseStationBadFormatMessagesReceived = 0;
                BaseStationMessagesReceived = 0;
                CurrentBufferSize = 0;
                FailedChecksumMessages = 0;
                ModeSWithBadParityPIField = 0;
                ModeSLongFrameMessagesReceived = 0;
                ModeSMessagesReceived = 0;
                ModeSNotAdsbCount = 0;
                ModeSShortFrameMessagesReceived = 0;
                ModeSShortFrameWithoutLongFrameMessagesReceived = 0;
                ModeSWithPIField = 0;
            });
        }

        /// <summary>
        /// Resets connection statistics.
        /// </summary>
        public void ResetConnectionStatistics()
        {
            Lock(_ => {
                ConnectionTimeUtc = null;
                BytesReceived = 0;
            });
        }

        /// <summary>
        /// Returns a deep clone of this statstics object. The object is copied from within a
        /// call to <see cref="Lock"/> to try to keep all of the counters consistent.
        /// </summary>
        /// <returns></returns>
        public virtual ReceiverStatistics Clone()
        {
            var target = (ReceiverStatistics)Activator.CreateInstance(GetType());

            Lock(_ => {
                Array.Copy(AdsbTypeCount,           target.AdsbTypeCount,           AdsbTypeCount.Length);
                Array.Copy(AdsbMessageFormatCount,  target.AdsbMessageFormatCount,  AdsbMessageFormatCount.Length);

                for(var idx = 0;idx < ModeSDFStatistics.Length;++idx) {
                    var sourceModeS = ModeSDFStatistics[idx];
                    var targetModeS = target.ModeSDFStatistics[idx];
                    targetModeS.BadParityPI =       sourceModeS.BadParityPI;
                    targetModeS.DF =                sourceModeS.DF;
                    targetModeS.MessagesReceived =  sourceModeS.MessagesReceived;
                }

                target.AdsbAircraftTracked =                                AdsbAircraftTracked;
                target.AdsbCount =                                          AdsbCount;
                target.AdsbPositionsExceededSpeedCheck =                    AdsbPositionsExceededSpeedCheck;
                target.AdsbPositionsOutsideRange =                          AdsbPositionsOutsideRange;
                target.AdsbPositionsReset =                                 AdsbPositionsReset;
                target.AdsbRejected =                                       AdsbRejected;
                target.BaseStationBadFormatMessagesReceived =               BaseStationBadFormatMessagesReceived;
                target.BaseStationMessagesReceived =                        BaseStationMessagesReceived;
                target.BytesReceived =                                      BytesReceived;
                target.ConnectionTimeUtc =                                  ConnectionTimeUtc;
                target.CurrentBufferSize =                                  CurrentBufferSize;
                target.FailedChecksumMessages =                             FailedChecksumMessages;
                target.ModeSWithBadParityPIField =                          ModeSWithBadParityPIField;
                target.ModeSLongFrameMessagesReceived =                     ModeSLongFrameMessagesReceived;
                target.ModeSMessagesReceived =                              ModeSMessagesReceived;
                target.ModeSNotAdsbCount =                                  ModeSNotAdsbCount;
                target.ModeSShortFrameMessagesReceived =                    ModeSShortFrameMessagesReceived;
                target.ModeSShortFrameWithoutLongFrameMessagesReceived =    ModeSShortFrameWithoutLongFrameMessagesReceived;
                target.ModeSWithPIField =                                   ModeSWithPIField;
            });

            return target;
        }
    }
}
