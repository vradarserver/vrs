// Copyright © 2010 onwards, Andrew Whewell
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
using System.Text;

namespace VirtualRadar.Interface.BaseStation
{
    /// <summary>
    /// Arguments to an event that carries a message from a BaseStation instance.
    /// </summary>
    public class BaseStationMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the message that was transmitted by BaseStation.
        /// </summary>
        public BaseStationMessage Message { get; private set; }

        /// <summary>
        /// Gets a value indicating that the message was received on a feed that is not the nominated feed for
        /// the aircraft in a merged feed.
        /// </summary>
        /// <remarks>
        /// At the time of writing this is only set on merged feeds when the message was received from an MLAT source
        /// and the aircraft is currently being serviced by a different receiver.
        /// </remarks>
        public bool IsOutOfBand { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="message"></param>
        public BaseStationMessageEventArgs(BaseStationMessage message)
        {
            Message = message;
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="isOutOfBand"></param>
        public BaseStationMessageEventArgs(BaseStationMessage message, bool isOutOfBand) : this(message)
        {
            IsOutOfBand = isOutOfBand;
        }
    }
}
