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

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// A collection of strings describing the UniqueId values for the rebroadcast server formats
    /// that ship with VRS.
    /// </summary>
    public static class RebroadcastFormat
    {
        /// <summary>
        /// The original messages from the receiver are rebroadcast without change.
        /// </summary>
        public static readonly string Passthrough = "Passthrough";

        /// <summary>
        /// The Port30003 messages derived from the receiver's transmissions are rebroadcast.
        /// </summary>
        public static readonly string Port30003 = "Port30003";

        /// <summary>
        /// The Mode-S messages are rebroadcast in either * or : AVR format, depending on
        /// whether the original Mode-S message source had parity stripped or not.
        /// </summary>
        public static readonly string Avr = "Avr";

        /// <summary>
        /// The Port30003 messages are compressed before broadcast.
        /// </summary>
        public static readonly string CompressedVRS = "CompressedVRS";

        /// <summary>
        /// Changes to the aircraft list are periodically sent as JSON. On the receiving side the
        /// JSON is translated into a set of BaseStation messages, one per aircraft.
        /// </summary>
        public static readonly string AircraftListJson = "AircraftListJson";

        /// <summary>
        /// Same as BaseStation except if the message is an MLAT generated message then MLAT is sent
        /// instead of MSG.
        /// </summary>
        public static readonly string ExtendedBaseStation = "ExtendedBaseStation";

        private static string[] _AllInternalFormats = new string[] {
            Passthrough,
            Port30003,
            Avr,
            CompressedVRS,
            AircraftListJson,
            ExtendedBaseStation,
        };
        /// <summary>
        /// A collection of all rebroadcast server formats that ship with VRS. The server doesn't use this,
        /// it's to help make life easier with unit testing.
        /// </summary>
        public static string[] AllInternalFormats
        {
            get { return _AllInternalFormats; }
        }
    }
}
