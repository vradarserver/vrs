// Copyright © 2017 onwards, Andrew Whewell
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
using System.Threading.Tasks;
using VirtualRadar.Interface.Database;

namespace VirtualRadar.Plugin.SqlServer.Models
{
    /// <summary>
    /// Represents the BaseStationAircraftUpsertLookup UDTT.
    /// </summary>
    class BaseStationAircraftUpsertLookupUdtt
    {
        /// <summary>
        /// Gets the pre-generated set of UDTT properties for the class.
        /// </summary>
        public static UdttProperty<BaseStationAircraftUpsertLookupUdtt>[] UdttProperties { get; private set; }

        /// <summary>
        /// Gets or sets the ModeS code of the aircraft. This is required, it is the key used to drive the upsert.
        /// </summary>
        [Ordinal(0)]
        public string ModeS { get; set; }

        /// <summary>
        /// Gets or sets the date and time (UTC) that the record was last changed.
        /// </summary>
        [Ordinal(1)]
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Gets or sets the registration of the aircraft.
        /// </summary>
        [Ordinal(2)]
        public string Registration { get; set; }

        /// <summary>
        /// Gets or sets the country of the operator.
        /// </summary>
        [Ordinal(3)]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the country of registration.
        /// </summary>
        [Ordinal(4)]
        public string ModeSCountry { get; set; }

        /// <summary>
        /// Gets or sets the full name of the aircraft's manufacturer.
        /// </summary>
        [Ordinal(5)]
        public string Manufacturer { get; set; }

        /// <summary>
        /// Gets or sets a description of the model of the aircraft.
        /// </summary>
        [Ordinal(6)]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the ICAO 8643 type code for the aircraft.
        /// </summary>
        [Ordinal(7)]
        public string ICAOTypeCode { get; set; }

        /// <summary>
        /// Gets or sets the operator of the aircraft.
        /// </summary>
        [Ordinal(8)]
        public string RegisteredOwners { get; set; }

        /// <summary>
        /// Gets or sets the ICAO code for the aircraft's operator.
        /// </summary>
        [Ordinal(9)]
        public string OperatorFlagCode { get; set; }

        /// <summary>
        /// Gets or sets the manufacturers serial number assigned to the aircraft.
        /// </summary>
        [Ordinal(10)]
        public string SerialNo { get; set; }

        /// <summary>
        /// Gets or sets the year the aircraft was manufactured.
        /// </summary>
        [Ordinal(11)]
        public string YearBuilt { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public BaseStationAircraftUpsertLookupUdtt()
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="lookup"></param>
        public BaseStationAircraftUpsertLookupUdtt(BaseStationAircraftUpsertLookup lookup)
        {
            ModeS =             lookup.ModeS;
            LastModified =      lookup.LastModified;
            Registration =      lookup.Registration;
            Country =           lookup.Country;
            ModeSCountry =      lookup.ModeSCountry;
            Manufacturer =      lookup.Manufacturer;
            Type =              lookup.Type;
            ICAOTypeCode =      lookup.ICAOTypeCode;
            RegisteredOwners =  lookup.RegisteredOwners;
            OperatorFlagCode =  lookup.OperatorFlagCode;
            SerialNo =          lookup.SerialNo;
            YearBuilt =         lookup.YearBuilt;
        }

        /// <summary>
        /// Static constructor.
        /// </summary>
        static BaseStationAircraftUpsertLookupUdtt()
        {
            UdttProperties = UdttProperty<BaseStationAircraftUpsertLookupUdtt>.GetUdttProperties();
        }
    }
}
