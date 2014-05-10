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
    /// An enumeration of flags that indicate the meaning of a TCAS resolution advisory when
    /// there is a single threat or the resolution advisory is intended to provide separation
    /// in the same direction for all threats.
    /// </summary>
    [Flags]
    public enum SingleThreatResolutionAdvisory : short
    {
        /// <summary>
        /// Reserved.
        /// </summary>
        /// <remarks>Bit 54 in the ICAO documentation.</remarks>
        Reserved1 = 0x0001,

        /// <summary>
        /// Reserved.
        /// </summary>
        /// <remarks>Bit 53 in the ICAO documentation.</remarks>
        Reserved2 = 0x0002,

        /// <summary>
        /// Reserved.
        /// </summary>
        /// <remarks>Bit 52 in the ICAO documentation.</remarks>
        Reserved3 = 0x0004,

        /// <summary>
        /// Reserved.
        /// </summary>
        /// <remarks>Bit 51 in the ICAO documentation.</remarks>
        Reserved4 = 0x0008,

        /// <summary>
        /// Reserved.
        /// </summary>
        /// <remarks>Bit 50 in the ICAO documentation.</remarks>
        Reserved5 = 0x0010,

        /// <summary>
        /// Reserved.
        /// </summary>
        /// <remarks>Bit 49 in the ICAO documentation.</remarks>
        Reserved6 = 0x0020,

        /// <summary>
        /// Reserved.
        /// </summary>
        /// <remarks>Bit 48 in the ICAO documentation.</remarks>
        Reserved7 = 0x0040,

        /// <summary>
        /// RA is positive when set, is vertical speed limit when clear.
        /// </summary>
        /// <remarks>Bit 47 in the ICAO documentation.</remarks>
        IsPositive = 0x0080,

        /// <summary>
        /// RA is altitude crossing when set, is not altitude crossing when clear.
        /// </summary>
        /// <remarks>Bit 46 in the ICAO documentation.</remarks>
        IsAltitudeCrossing = 0x0100,

        /// <summary>
        /// RA is a sense reversal when set, is not a sense reversal when clear.
        /// </summary>
        /// <remarks>Bit 45 in the ICAO documentation.</remarks>
        IsSenseReversal = 0x0200,

        /// <summary>
        /// RA is increased rate when set, is not increased rate when clear.
        /// </summary>
        /// <remarks>Bit 44 in the ICAO documentation.</remarks>
        IsIncreasedRate = 0x0400,

        /// <summary>
        /// Downward sense RA has been generated when set, upward sense RA has been generated when clear.
        /// </summary>
        /// <remarks>Bit 43 in the ICAO documentation.</remarks>
        DownwardSense = 0x0800,

        /// <summary>
        /// RA is corrective when set, is preventive when clear.
        /// </summary>
        /// <remarks>Bit 42 in the ICAO documentation.</remarks>
        IsCorrective = 0x1000,
    }
}
