// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Interface.ModeS
{
    /// <summary>
    /// An enumeration of the different downlink formats transmitted by transponders.
    /// </summary>
    public enum DownlinkFormat : byte
    {
        /// <summary>
        /// Short ACAS reply (DF = 0).
        /// </summary>
        ShortAirToAirSurveillance = 0,

        /// <summary>
        /// Unused.
        /// </summary>
        DF1 = 1,

        /// <summary>
        /// Unused.
        /// </summary>
        DF2 = 2,

        /// <summary>
        /// Unused.
        /// </summary>
        DF3 = 3,

        /// <summary>
        /// Surveillance altitude reply (DF = 4).
        /// </summary>
        SurveillanceAltitudeReply = 4,

        /// <summary>
        /// Surveillance identity reply (DF = 5).
        /// </summary>
        SurveillanceIdentityReply = 5,

        /// <summary>
        /// Unused.
        /// </summary>
        DF6 = 6,

        /// <summary>
        /// Unused.
        /// </summary>
        DF7 = 7,

        /// <summary>
        /// Unused.
        /// </summary>
        DF8 = 8,

        /// <summary>
        /// Unused.
        /// </summary>
        DF9 = 9,

        /// <summary>
        /// Unused.
        /// </summary>
        DF10 = 10,

        /// <summary>
        /// Reply to an all-call interrogation (DF = 11).
        /// </summary>
        AllCallReply = 11,

        /// <summary>
        /// Unused.
        /// </summary>
        DF12 = 12,

        /// <summary>
        /// Unused.
        /// </summary>
        DF13 = 13,

        /// <summary>
        /// Unused.
        /// </summary>
        DF14 = 14,

        /// <summary>
        /// Unused.
        /// </summary>
        DF15 = 15,

        /// <summary>
        /// Long ACAS reply (DF = 16).
        /// </summary>
        LongAirToAirSurveillance = 16,

        /// <summary>
        /// Extended squitter (DF = 17).
        /// </summary>
        ExtendedSquitter = 17,

        /// <summary>
        /// Extended squitter from a non-transponder reply (DF = 18).
        /// </summary>
        ExtendedSquitterNonTransponder = 18,

        /// <summary>
        /// Military extended squitter reply (DF = 19).
        /// </summary>
        MilitaryExtendedSquitter = 19,

        /// <summary>
        /// Comm-B altitude reply (DF = 20).
        /// </summary>
        CommBAltitudeReply = 20,

        /// <summary>
        /// Comm-B identity reply (DF = 21).
        /// </summary>
        CommBIdentityReply = 21,

        /// <summary>
        /// Unused.
        /// </summary>
        DF22 = 22,

        /// <summary>
        /// Unused.
        /// </summary>
        DF23 = 23,

        /// <summary>
        /// Comm-D reply (DF = 24).
        /// </summary>
        CommD = 24
    }
}
