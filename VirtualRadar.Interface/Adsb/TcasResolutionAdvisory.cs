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
    /// A class describing the content of an aircraft status message carrying a TCAS RA broadcast.
    /// </summary>
    public class TcasResolutionAdvisory
    {
        /// <summary>
        /// Gets or sets the single-threat resolution advisory in a TCAS RA broadcast.
        /// </summary>
        /// <remarks>If this is supplied then <see cref="MultipleThreatResolutionAdvisory"/> will be clear.</remarks>
        public SingleThreatResolutionAdvisory? SingleThreatResolutionAdvisory { get; set; }

        /// <summary>
        /// Gets or sets the multiple-threat resolution advisory in a TCAS RA broadcast.
        /// </summary>
        /// <remarks>If this is supplied then <see cref="SingleThreatResolutionAdvisory"/> will be null.</remarks>
        public MultipleThreatResolutionAdvisory? MultipleThreatResolutionAdvisory { get; set; }

        /// <summary>
        /// Gets or sets the RAC from a TCAS RA broadcast.
        /// </summary>
        public ResolutionAdvisoryComplement ResolutionAdvisoryComplement { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the RA has been terminated.
        /// </summary>
        public bool ResolutionAdvisoryTerminated { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that multiple threats are being processed by TCAS.
        /// </summary>
        public bool MultipleThreatEncounter { get; set; }

        private int _ThreatIcao24;
        /// <summary>
        /// Gets or sets the ICAO24 code of the most recently declared threat to the aircraft, if known.
        /// </summary>
        /// <remarks>If this is non-zero then <see cref="FormattedThreatIcao24"/> will be set and <see cref="ThreatAltitude"/>,
        /// <see cref="ThreatRange"/> and <see cref="ThreatBearing"/> will all be null.</remarks>
        public int ThreatIcao24
        {
            get { return _ThreatIcao24; }
            set { if(_ThreatIcao24 != value) { _ThreatIcao24 = value; _FormattedThreatIcao24 = null; } }
        }

        private string _FormattedThreatIcao24;
        /// <summary>
        /// Gets the <see cref="ThreatIcao24"/> value formatted as a string. If <see cref="ThreatIcao24"/> is zero then this will be null.
        /// </summary>
        public string FormattedThreatIcao24
        {
            get
            {
                if(_FormattedThreatIcao24 == null && ThreatIcao24 != 0) _FormattedThreatIcao24 = ThreatIcao24.ToString("X6");
                return _FormattedThreatIcao24;
            }
        }

        /// <summary>
        /// Gets or sets the altitude of the most recently declared threat to the aircraft.
        /// </summary>
        /// <remarks>If this is not null then <see cref="ThreatRange"/> and <see cref="ThreatBearing"/> will also be supplied (although they
        /// may be null if the given value is unknown) and <see cref="ThreatIcao24"/> will be null.</remarks>
        public int? ThreatAltitude { get; set; }

        /// <summary>
        /// Gets or sets the range from the aircraft to the threat in nautical miles.
        /// </summary>
        /// <remarks>If this is null then either the range to the threat is not known or <see cref="ThreatIcao24"/> has been supplied. If it is
        /// not null then <see cref="ThreatIcao24"/> will be null and <see cref="ThreatAltitude"/> and <see cref="ThreatBearing"/> will be supplied.</remarks>
        public float? ThreatRange { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the range to the threat in <see cref="ThreatRange"/> exceeds the maximum distance that TCAS can represent.
        /// </summary>
        /// <remarks>
        /// If this is set then the threat is further away than the value specified in <see cref="ThreatRange"/>.
        /// </remarks>
        public bool ThreatRangeExceeded { get; set; }

        /// <summary>
        /// Gets or sets the bearing, relative to the aircraft, of the threat aircraft.
        /// </summary>
        /// <remarks>
        /// If this is null then either the bearing to the threat is not known or <see cref="ThreatIcao24"/> has been supplied. If it is not null
        /// then <see cref="ThreatIcao24"/> will be null and <see cref="ThreatAltitude"/> and <see cref="ThreatRange"/> will be supplied.
        /// </remarks>
        public short? ThreatBearing { get; set; }

        /// <summary>
        /// Returns an English description of the content of the object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var result = new StringBuilder("TCAS-RA");

            if(SingleThreatResolutionAdvisory != null) result.AppendFormat(" ARA-S:0x{0:X2}", (short)SingleThreatResolutionAdvisory);
            if(MultipleThreatResolutionAdvisory != null) result.AppendFormat(" ARA-M:0x{0:X2}", (short)MultipleThreatResolutionAdvisory);
            result.AppendFormat(" RAC:0x{0:X2}", (byte)ResolutionAdvisoryComplement);
            result.AppendFormat(" RAT:{0}", ResolutionAdvisoryTerminated ? '1' : '0');
            result.AppendFormat(" MTE:{0}", MultipleThreatEncounter ? '1' : '0');
            if(ThreatIcao24 > 0) result.AppendFormat(" TID:{0}", FormattedThreatIcao24);
            if(ThreatAltitude != null) result.AppendFormat(" TID-A:{0}", ThreatAltitude);
            if(ThreatRange != null) result.AppendFormat(" TID-R:{0}{1}", ThreatRange, ThreatRangeExceeded ? "*" : "");
            if(ThreatBearing != null) result.AppendFormat(" TID-B:{0}", ThreatBearing);

            return result.ToString();
        }
    }
}
