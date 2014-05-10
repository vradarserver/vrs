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

namespace VirtualRadar.Interface.Adsb
{
    /// <summary>
    /// An enumeration of the different emergency states transmitted in aircraft status and ADS-B
    /// version 1 target state and status messages.
    /// </summary>
    public enum EmergencyState
    {
        /// <summary>
        /// No emergency state is being transmitted.
        /// </summary>
        None = 0,

        /// <summary>
        /// General emergency (corresponds with squawk 7700).
        /// </summary>
        GeneralEmergency = 1,

        /// <summary>
        /// Lifeguard duty or medical emergency.
        /// </summary>
        Lifeguard = 2,

        /// <summary>
        /// Minimum fuel emergency.
        /// </summary>
        MinimumFuel = 3,

        /// <summary>
        /// No radio communications (corresponds with squawk 7600).
        /// </summary>
        NoCommunications = 4,

        /// <summary>
        /// Unlawful interference (corresponds with squawk 7500).
        /// </summary>
        UnlawfulInterference = 5,

        /// <summary>
        /// Downed aircraft (transmitted only in ADS-B messages compliant with version 1 and above).
        /// </summary>
        DownedAircraft = 6,

        /// <summary>
        /// Reserved.
        /// </summary>
        EmergencyState7 = 7,
    }
}
