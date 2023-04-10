// Copyright © 2017 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.ComponentModel.DataAnnotations;

namespace VirtualRadar.Interface.KineticData
{
    /// <summary>
    /// A cut-down version of <see cref="KineticAircraftKeyless"/> used when upserting
    /// aircraft lookup results.
    /// </summary>
    public class KineticAircraftLookup
    {
        /// <summary>
        /// Gets or sets the ModeS code of the aircraft.
        /// </summary>
        [Required]
        public string ModeS { get; set; }

        /// <summary>
        /// Gets or sets the date and time (UTC) that the record was last changed.
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Gets or sets the registration of the aircraft.
        /// </summary>
        public string Registration { get; set; }

        /// <summary>
        /// Gets or sets the country of the operator.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the country of registration.
        /// </summary>
        public string ModeSCountry { get; set; }

        /// <summary>
        /// Gets or sets the full name of the aircraft's manufacturer.
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// Gets or sets a description of the model of the aircraft.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the ICAO 8643 type code for the aircraft.
        /// </summary>
        public string ICAOTypeCode { get; set; }

        /// <summary>
        /// Gets or sets the operator of the aircraft.
        /// </summary>
        public string RegisteredOwners { get; set; }

        /// <summary>
        /// Gets or sets the ICAO code for the aircraft's operator.
        /// </summary>
        public string OperatorFlagCode { get; set; }

        /// <summary>
        /// Gets or sets the manufacturers serial number assigned to the aircraft.
        /// </summary>
        public string SerialNo { get; set; }

        /// <summary>
        /// Gets or sets the year the aircraft was manufactured.
        /// </summary>
        public string YearBuilt { get; set; }
    }
}
