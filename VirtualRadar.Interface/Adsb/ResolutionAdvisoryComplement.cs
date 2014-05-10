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
    /// An enumeration of flags recording resolution advisory complements that have been received from other aircraft.
    /// </summary>
    [Flags]
    public enum ResolutionAdvisoryComplement : byte
    {
        /// <summary>
        /// Do not turn right.
        /// </summary>
        /// <remarks>Bit 58 in the ICAO documentation.</remarks>
        DoNotTurnRight = 0x01,

        /// <summary>
        /// Do not turn left.
        /// </summary>
        /// <remarks>Bit 57 in the ICAO documentation.</remarks>
        DoNotTurnLeft = 0x02,

        /// <summary>
        /// Do not pass above.
        /// </summary>
        /// <remarks>Bit 56 in the ICAO documentation.</remarks>
        DoNotPassAbove = 0x04,

        /// <summary>
        /// Do not pass below.
        /// </summary>
        /// <remarks>Bit 55 in the ICAO documentation.</remarks>
        DoNotPassBelow = 0x08,
    }
}
