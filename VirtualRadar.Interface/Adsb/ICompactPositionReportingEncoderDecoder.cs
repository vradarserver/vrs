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
    /// The interface for classes that can encode and decode Compact Position Reporting coordinates.
    /// </summary>
    public interface ICompactPositionReportingEncoderDecoder
    {
        /// <summary>
        /// Encodes a latitude and longitude to CPR coordinates.
        /// </summary>
        /// <param name="globalCoordinate">The latitude and longitude to encode.</param>
        /// <param name="isOddFormat">True if ODD formatting is required, false if EVEN formatting is required.</param>
        /// <param name="numberOfBits">The number of bits to encode to - this can be one of 12, 14, 17 or 19.</param>
        /// <returns></returns>
        /// <remarks>
        /// Note that the standard only allows for even encoding when the number of bits is 14. Implementations do not have to police this.
        /// VRS does not use this method, implementations only need to supply a full version of this method if they want to pass the unit
        /// tests.
        /// </remarks>
        CompactPositionReportingCoordinate Encode(GlobalCoordinate globalCoordinate, bool isOddFormat, byte numberOfBits);

        /// <summary>
        /// Decodes a CPR latitude and longitude to an unambiguous latitude and longitude with reference to a known latitude and longitude.
        /// </summary>
        /// <param name="cprCoordinate">The CPR coordinate to decode.</param>
        /// <param name="referenceCoordinate">The global coordinate that is within 180NM (for messages up to 17 bits) or 45NM (for messages of 19 bits) of the vehicle's location.</param>
        /// <returns></returns>
        GlobalCoordinate LocalDecode(CompactPositionReportingCoordinate cprCoordinate, GlobalCoordinate referenceCoordinate);

        /// <summary>
        /// Decodes an early and later CPR coordinate into an unambiguous location on the surface of the globe. May return null if the location cannot be decoded.
        /// </summary>
        /// <param name="earlyCpr">The earlier of the two transmissions.</param>
        /// <param name="laterCpr">The later of the two transmissions. The OddFormat parameter must not be the same as the ealier coordinate's and the number of bits must be identical.</param>
        /// <param name="receiverLocation">The location of the receiver on the globe. If this is null then 19-bit CPR coordinates will not be decoded, but 17-bit and below still will.</param>
        /// <returns>An object describing the location on the surface of the globe or null if the location could not be decoded.</returns>
        /// <remarks><para>
        /// Surface position coordinates (19-bit format) always decode to two possible latitudes and four possible longitudes. It is impossible to unambiguously determine
        /// which of those eight locations is the correct one, so the one closest to 'receiverLocation' is always chosen. If the receiver location is unknown then null is
        /// returned for 19-bit formats. The receiver location is ignored for all other formats.
        /// </para><para>
        /// The spec suggests that two messages are only globally decoded if they arrive within a certain time frame (10 seconds for airborne position messages,
        /// 25 to 50 seconds for surface position messages). This method does not police those time frames, it's up to the caller to determine whether two messages are
        /// suitable for global decoding. This is important to note. The algorithm does NOT give the right result if the distance between the two locations at which the
        /// messages were transmitted is larger than 3 nautical miles (about 5.5km) for 17 bit messages or 0.25 nmi (about 1.38km) for 19 bit messages. You should follow
        /// the rules for testing the reasonableness of a globally decoded location as laid out in the ICAO documentation.
        /// </para></remarks>
        GlobalCoordinate GlobalDecode(CompactPositionReportingCoordinate earlyCpr, CompactPositionReportingCoordinate laterCpr, GlobalCoordinate receiverLocation);
    }
}
