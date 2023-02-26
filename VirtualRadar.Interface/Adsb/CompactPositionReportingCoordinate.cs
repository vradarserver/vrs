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
    /// Describes a Compact Position Reporting coordinate.
    /// </summary>
    public class CompactPositionReportingCoordinate
    {
        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        public int Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        public int Longitude { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the coordinate is in ODD format (true) or EVEN format (false).
        /// </summary>
        public bool OddFormat { get; set; }

        /// <summary>
        /// Gets or sets the number of bits that the coordinate was encoded in - possible values are 12, 14, 17 or 19.
        /// </summary>
        /// <remarks>
        /// This is not necessarily the number of bits used to transmit the coordinate. Surface position messages transmit
        /// a 17 bit coordinate but encode to 19 bits, for these the property should be set to 19.
        /// </remarks>
        public byte NumberOfBits { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public CompactPositionReportingCoordinate()
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="oddFormat"></param>
        /// <param name="numberOfBits"></param>
        public CompactPositionReportingCoordinate(int latitude, int longitude, bool oddFormat, byte numberOfBits)
        {
            Latitude = latitude;
            Longitude = longitude;
            OddFormat = oddFormat;
            NumberOfBits = numberOfBits;
        }

        /// <summary>
        /// Returns an English description of the coordinate.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0}/{1}{2}{3}", Latitude, Longitude, OddFormat ? 'O' : 'E', NumberOfBits);
        }
    }
}
