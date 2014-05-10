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

namespace VirtualRadar.Interface.ModeS
{
    /// <summary>
    /// An enumeration of the different values contained within the CF field of DF18 replies.
    /// </summary>
    public enum ControlField : byte
    {
        /// <summary>
        /// Non-transponder ADS-B device transmitting ICAO24 code in the AA field.
        /// </summary>
        AdsbDeviceTransmittingIcao24 = 0,

        /// <summary>
        /// Non-transponder ADS-B device transmitting a non-ICAO24 address in the AA field.
        /// </summary>
        AdsbDeviceNotTransmittingIcao24 = 1,

        /// <summary>
        /// Fine-format TIS-B message.
        /// </summary>
        FineFormatTisb = 2,

        /// <summary>
        /// Coarse-format TIS-B message.
        /// </summary>
        CoarseFormatTisb = 3,

        /// <summary>
        /// Reserved for TIS-B management messages.
        /// </summary>
        TisbManagement = 4,

        /// <summary>
        /// TIS-B relay of ADS-B messages that have a non-ICAO24 address in the AA field.
        /// </summary>
        TisbRelayNotTransmittingIcao24 = 5,

        /// <summary>
        /// ADS-B rebroadcast using same type codes and message formats as defined for Extended Squitter (DF17) messages.
        /// </summary>
        AdsbRebroadcastOfExtendedSquitter = 6,

        /// <summary>
        /// Reserved.
        /// </summary>
        CF7 = 7,
    }
}
