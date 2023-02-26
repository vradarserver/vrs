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
    /// An enumeration of flags that indicate the meaning of a TCAS resolution advisory when
    /// there are multiple threats and the resolution advisory provides separation below some
    /// threats and above other threats.
    /// </summary>
    [Flags]
    public enum MultipleThreatResolutionAdvisory : short
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
        /// RA is a sense reversal when set, is not a sense reversal when clear.
        /// </summary>
        /// <remarks>Bit 47 in the ICAO documentation.</remarks>
        IsSenseReversal = 0x0080,

        /// <summary>
        /// RA requires a crossing when set, does not require a crossing when clear.
        /// </summary>
        /// <remarks>Bit 46 in the ICAO documentation.</remarks>
        RequiresCrossing = 0x0100,

        /// <summary>
        /// RA requires a positive descend when set, does not require a positive descend when clear.
        /// </summary>
        /// <remarks>Bit 45 in the ICAO documentation.</remarks>
        RequiresPositiveDescend = 0x0200,

        /// <summary>
        /// RA requires a correction in the downward sense when set, does not require a correction in the downward sense when clear.
        /// </summary>
        /// <remarks>Bit 44 in the ICAO documentation.</remarks>
        RequiresCorrectionInDownwardSense = 0x0400,

        /// <summary>
        /// RA requires a positive climb when set, does not require a positive climb when clear.
        /// </summary>
        /// <remarks>Bit 43 in the ICAO documentation.</remarks>
        RequiresPositiveClimb = 0x0800,

        /// <summary>
        /// RA requires a correction in the upward sense when set, does not require a correction in the upward sense when clear.
        /// </summary>
        /// <remarks>Bit 42 in the ICAO documentation.</remarks>
        RequiresCorrectionInUpwardSense = 0x1000,
    }
}
