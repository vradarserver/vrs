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

namespace VirtualRadar.Interface.ModeS
{
    /// <summary>
    /// The event that is raised when a listener to a source of raw Mode-S messages from an aircraft receives a message.
    /// </summary>
    public class ModeSMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the date and time at UTC when the message was received.
        /// </summary>
        public DateTime ReceivedUtc { get; private set; }

        /// <summary>
        /// Gets the decoded Mode-S message from the aircraft.
        /// </summary>
        public ModeSMessage ModeSMessage { get; private set; }

        /// <summary>
        /// Gets the decoded ADS-B message from extended squitter Mode-S messages. This will be null if the Mode-S message did not contain an ADS-B payload.
        /// </summary>
        public AdsbMessage AdsbMessage { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="utcNow"></param>
        /// <param name="modeSMessage"></param>
        /// <param name="adsbMessage"></param>
        public ModeSMessageEventArgs(DateTime utcNow, ModeSMessage modeSMessage, AdsbMessage adsbMessage)
        {
            ReceivedUtc = utcNow;
            ModeSMessage = modeSMessage;
            AdsbMessage = adsbMessage;
        }
    }
}
