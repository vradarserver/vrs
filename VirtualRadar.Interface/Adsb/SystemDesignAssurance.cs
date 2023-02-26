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
    /// An enumeration of the different System Design Assurance values extracted from operational message
    /// fields of version 2 ADS-B messages.
    /// </summary>
    public enum SystemDesignAssurance : byte
    {
        /// <summary>
        /// The probability of an undetected fault causing the transmission of false or misleading position information is &gt; 1x10^3 per flight hour.
        /// </summary>
        /// <remarks>Does not support any failure condition.</remarks>
        None = 0,

        /// <summary>
        /// The probability of an undetected fault causing the transmission of false or misleading position information is &lt;= 1x10^-3 per flight hour.
        /// </summary>
        /// <remarks>Supports failure conditions classified as Minor.</remarks>
        AssuranceLevelD = 1,

        /// <summary>
        /// The probability of an undetected fault causing the transmission of false or misleading position information is &lt;= 1x10^-5 per flight hour.
        /// </summary>
        /// <remarks>Supports failure conditions classified as Major.</remarks>
        AssuranceLevelC = 2,

        /// <summary>
        /// The probability of an undetected fault causing the transmission of false or misleading position information is &lt;= 1x10^-7 per flight hour.
        /// </summary>
        /// <remarks>Supports failure conditions classified as Hazardous.</remarks>
        AssuranceLevelB = 3,
    }
}
