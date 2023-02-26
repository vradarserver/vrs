// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Interface.Adsb
{
    /// <summary>
    /// An enumeration of the different ADS-B message formats.
    /// </summary>
    public enum MessageFormat : byte
    {
        /// <summary>
        /// Unknown / unrecognised message format.
        /// </summary>
        None,

        /// <summary>
        /// Type 0 - airborne or surface vehicle with no position message.
        /// </summary>
        NoPositionInformation,

        /// <summary>
        /// Types 1-4 - vehicle identification and category messages.
        /// </summary>
        IdentificationAndCategory,

        /// <summary>
        /// Types 5-8 - surface position message.
        /// </summary>
        SurfacePosition,

        /// <summary>
        /// Types 9-18 and 20-22 - airborne position message.
        /// </summary>
        AirbornePosition,

        /// <summary>
        /// Type 19 - airborne velocity message, ground speed or airspeed.
        /// </summary>
        AirborneVelocity,

        /// <summary>
        /// Type 28 - aircraft status message.
        /// </summary>
        AircraftStatus,

        /// <summary>
        /// Type 29 - target state and status message (ADS-B versions 1 and 2).
        /// </summary>
        TargetStateAndStatus,

        /// <summary>
        /// Type 31 - aircraft operational status message.
        /// </summary>
        AircraftOperationalStatus,

        /// <summary>
        /// DF18 CA3 - coarse TIS-B airborne position message.
        /// </summary>
        CoarseTisbAirbornePosition,
    }
}
