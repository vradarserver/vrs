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
using VirtualRadar.Interface;
using VirtualRadar.Interface.ModeS;
using VirtualRadar.Interface.Adsb;

namespace VirtualRadar.Library
{
    class Statistics : IStatistics
    {
        /// <summary>
        /// Set when the object has been initialised.
        /// </summary>
        private bool _Initialised;

        /// <summary>
        /// The spin lock that controls updates of the values.
        /// </summary>
        private SpinLock _Lock = new SpinLock();

        /// <summary>
        /// See interface docs.
        /// </summary>
        public DateTime? ConnectionTimeUtc { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long BytesReceived { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long CurrentBufferSize { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long BaseStationMessagesReceived { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long BaseStationBadFormatMessagesReceived { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long ModeSMessagesReceived { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long ModeSLongFrameMessagesReceived { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long ModeSWithPIField { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long ModeSWithBadParityPIField { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long ModeSShortFrameMessagesReceived { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long ModeSShortFrameWithoutLongFrameMessagesReceived { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long FailedChecksumMessages { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long[] ModeSDFCount { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long AdsbCount { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long AdsbRejected { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long AdsbAircraftTracked { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long AdsbPositionsReset { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long AdsbPositionsOutsideRange { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long AdsbPositionsExceededSpeedCheck { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long ModeSNotAdsbCount { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long[] AdsbTypeCount { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long[] AdsbMessageFormatCount { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long ConnectorExceptionCount { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<List<TimestampedException>>> ConnectorExceptionsRequired;

        /// <summary>
        /// Raises <see cref="ConnectorExceptionsRequired"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnConnectorExceptionsRequired(EventArgs<List<TimestampedException>> args)
        {
            if(ConnectorExceptionsRequired != null) ConnectorExceptionsRequired(this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Initialise()
        {
            if(!_Initialised) {
                AdsbTypeCount = new long[256];
                ModeSDFCount = new long[Enum.GetValues(typeof(DownlinkFormat)).OfType<DownlinkFormat>().Select(r => (int)r).Max() + 1];
                AdsbMessageFormatCount = new long[Enum.GetValues(typeof(MessageFormat)).OfType<MessageFormat>().Select(r => (int)r).Max() + 1];
                _Initialised = true;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="lockedDelegate"></param>
        public void Lock(Action<IStatistics> lockedDelegate)
        {
            if(_Initialised) {
                _Lock.Lock();
                try {
                    lockedDelegate(this);
                } finally {
                    _Lock.Unlock();
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void ResetMessageCounters()
        {
            if(_Initialised) {
                Lock(r => {
                    Array.Clear(AdsbTypeCount, 0, AdsbTypeCount.Length);
                    Array.Clear(ModeSDFCount, 0, ModeSDFCount.Length);
                    Array.Clear(AdsbMessageFormatCount, 0, AdsbMessageFormatCount.Length);
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
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void ResetConnectionStatistics()
        {
            if(_Initialised) {
                Lock(r => {
                    ConnectionTimeUtc = null;
                    BytesReceived = 0;
                });
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public List<TimestampedException> GetConnectorExceptions()
        {
            var result = new List<TimestampedException>();
            OnConnectorExceptionsRequired(new EventArgs<List<TimestampedException>>(result));

            return result;
        }
    }
}
