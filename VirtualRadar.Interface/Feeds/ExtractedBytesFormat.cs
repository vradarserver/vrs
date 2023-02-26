﻿// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Interface.Feeds
{
    /// <summary>
    /// An enumeration of the different formats that the payload of <see cref="ExtractedBytes"/> can be in.
    /// </summary>
    public enum ExtractedBytesFormat
    {
        /// <summary>
        /// The format of the payload is unknown.
        /// </summary>
        None,

        /// <summary>
        /// The format of the payload corresponds with Kinetic's de-facto standard port 30003 text.
        /// </summary>
        Port30003,

        /// <summary>
        /// The format of the payload corresponds with ICAO's specification for Mode-S messages.
        /// </summary>
        ModeS,

        /// <summary>
        /// The format of the payload is a compressed Port30003 object.
        /// </summary>
        Compressed,

        /// <summary>
        /// The format of the payload is an aircraft list formatted in UTF-8 JSON.
        /// </summary>
        AircraftListJson,

        /// <summary>
        /// The format of the payload is an Airnav XRange JSON object.
        /// </summary>
        AirnavXRange,
    }
}
