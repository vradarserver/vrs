// Copyright © 2014 onwards, Andrew Whewell
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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// An enumeration of the different types of transponder that VRS can decode messages from.
    /// </summary>
    public enum TransponderType
    {
        /// <summary>
        /// The transponder type is not known and no attempt has been made to guess it.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The transponder is sending Mode-S messages with no ADSB content.
        /// </summary>
        ModeS = 1,

        /// <summary>
        /// The transponder is sending Mode-S messages with ADSB content, but the type of ADSB
        /// content cannot be determined. Assume that it's ADSB-0.
        /// </summary>
        Adsb = 2,

        /// <summary>
        /// The transponder is sending Mode-S messages with ADSB-0 content.
        /// </summary>
        Adsb0 = 3,

        /// <summary>
        /// The transponder is sending Mode-S messages with ADSB-1 content.
        /// </summary>
        Adsb1 = 4,

        /// <summary>
        /// The transponder is sending Mode-S messages with ADSB-2 content.
        /// </summary>
        Adsb2 = 5,
    }
}
