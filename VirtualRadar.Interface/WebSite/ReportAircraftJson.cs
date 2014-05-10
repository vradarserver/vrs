// Copyright © 2010 onwards, Andrew Whewell
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
using System.Runtime.Serialization;

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// Describes an aircraft in a report.
    /// </summary>
    [DataContract]
    public class ReportAircraftJson
    {
        /// <summary>
        /// Gets or sets a value that indicates that the registraton does not correspond to any aircraft in the database.
        /// </summary>
        [DataMember(Name="isUnknown", EmitDefaultValue=false)]
        public bool IsUnknown { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the aircraft in the database.
        /// </summary>
        [DataMember(Name="acID")]
        public int AircraftId { get; set; }

        /// <summary>
        /// Gets or sets the registration of the aircraft.
        /// </summary>
        [DataMember(Name="reg", EmitDefaultValue=false)]
        public string Registration { get; set; }

        /// <summary>
        /// Gets or sets the 24-bit Mode-S identifier assigned to the aircraft.
        /// </summary>
        [DataMember(Name="icao", EmitDefaultValue=false)]
        public string Icao { get; set; }

        /// <summary>
        /// Gets or sets the registration as formatted for Airframes.org
        /// </summary>
        [DataMember(Name="afr", EmitDefaultValue=false)]
        [Obsolete("Have the javascript do this instead")]
        public string AirframeReg { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's 'class' - can't find out what this actually is! Still report it though.
        /// </summary>
        [DataMember(Name="acClass", EmitDefaultValue=false)]
        public string AircraftClass { get; set; }

        /// <summary>
        /// Gets or sets the country of the operator.
        /// </summary>
        [DataMember(Name="country", EmitDefaultValue=false)]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the country of registration.
        /// </summary>
        [DataMember(Name="modeSCountry", EmitDefaultValue=false)]
        public string ModeSCountry { get; set; }

        /// <summary>
        /// Gets or sets the ATC wake turbulence category for the aircraft (see <see cref="WakeTurbulenceCategory"/>) cast to an int.
        /// </summary>
        [DataMember(Name="wtc", EmitDefaultValue=false)]
        public int? WakeTurbulenceCategory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how many engines there are, or whether the engines are coupled.
        /// </summary>
        [DataMember(Name="engines", EmitDefaultValue=false)]
        public string Engines { get; set; }

        /// <summary>
        /// Gets or sets the engine type used by the aircraft cast to an int (see <see cref="EngineType"/>).
        /// </summary>
        [DataMember(Name="engType", EmitDefaultValue=false)]
        public int? EngineType { get; set; }

        /// <summary>
        /// Gets or sets the species of the aircraft cast to an int (see <see cref="Species"/>).
        /// </summary>
        [DataMember(Name="species", EmitDefaultValue=false)]
        public int? Species { get; set; }

        /// <summary>
        /// Gets or sets the generic name for the type of aircraft.
        /// </summary>
        [DataMember(Name="genericName", EmitDefaultValue=false)]
        public string GenericName { get; set; }

        /// <summary>
        /// Gets or sets the ICAO 8643 type code for the aircraft.
        /// </summary>
        [DataMember(Name="icaoType", EmitDefaultValue=false)]
        public string IcaoTypeCode { get; set; }

        /// <summary>
        /// Gets or sets the full name of the aircraft's manufacturer.
        /// </summary>
        [DataMember(Name="manufacturer", EmitDefaultValue=false)]
        public string Manufacturer { get; set; }

        /// <summary>
        /// Gets or sets the ICAO code for the aircraft's operator.
        /// </summary>
        [DataMember(Name="opFlag", EmitDefaultValue=false)]
        public string OperatorFlagCode { get; set; }

        /// <summary>
        /// Gets or sets a value describing whether the aircraft is owned outright, leased etc.
        /// </summary>
        [DataMember(Name="ownerStatus", EmitDefaultValue=false)]
        public string OwnershipStatus { get; set; }

        /// <summary>
        /// Gets or sets the popular name for the type of aircraft.
        /// </summary>
        [DataMember(Name="popularName", EmitDefaultValue=false)]
        public string PopularName { get; set; }

        /// <summary>
        /// Gets or sets the previous Mode-S ID of the aircraft (I think? doesn't make a great deal of sense, would be better off as an int back to another aircraft record - but mine is not to question why...)
        /// </summary>
        [DataMember(Name="previousId", EmitDefaultValue=false)]
        public string PreviousId { get; set; }

        /// <summary>
        /// Gets or sets the operator of the aircraft.
        /// </summary>
        [DataMember(Name="owner", EmitDefaultValue=false)]
        public string RegisteredOwners { get; set; }

        /// <summary>
        /// Gets or sets the manufacturers serial number assigned to the aircraft.
        /// </summary>
        [DataMember(Name="serial", EmitDefaultValue=false)]
        public string SerialNumber { get; set; }

        /// <summary>
        /// Gets or sets a value describing whether the aircraft is in service, mothballed etc.
        /// </summary>
        [DataMember(Name="status", EmitDefaultValue=false)]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets a description of the model of the aircraft.
        /// </summary>
        [DataMember(Name="typ", EmitDefaultValue=false)]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the date that the plane was taken off the register.
        /// </summary>
        [DataMember(Name="deregDate", EmitDefaultValue=false)]
        public string DeRegDate { get; set; }

        /// <summary>
        /// Gets or sets the certificate of airworthiness category.
        /// </summary>
        [DataMember(Name="cofACategory", EmitDefaultValue=false)]
        public string CofACategory { get; set; }

        /// <summary>
        /// Gets or sets the date that the certificate of airworthiness expires.
        /// </summary>
        [DataMember(Name="cofAExpiry", EmitDefaultValue=false)]
        public string CofAExpiry { get; set; }

        /// <summary>
        /// Gets or sets the date that the aircraft was registered.
        /// </summary>
        [DataMember(Name="curRegDate", EmitDefaultValue=false)]
        public string CurrentRegDate { get; set; }

        /// <summary>
        /// Gets or sets the date that the aircraft was first placed onto the register.
        /// </summary>
        [DataMember(Name="firstRegDate", EmitDefaultValue=false)]
        public string FirstRegDate { get; set; }

        /// <summary>
        /// Gets or sets the URL of a web page showing more information about the aircraft.
        /// </summary>
        [DataMember(Name="infoUrl", EmitDefaultValue=false)]
        public string InfoUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the BaseStation operator wants to be alerted when this aircraft appears.
        /// </summary>
        [DataMember(Name="interested", EmitDefaultValue=false)]
        public bool Interested { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the aircraft is operated by a military force.
        /// </summary>
        [DataMember(Name="military", EmitDefaultValue=false)]
        public bool Military { get; set; }

        /// <summary>
        /// Gets or sets the maximum takeoff weight of the aircraft.
        /// </summary>
        [DataMember(Name="mtow", EmitDefaultValue=false)]
        public string MTOW { get; set; }

        /// <summary>
        /// Gets or sets the URL of a picture of the aircraft.
        /// </summary>
        [DataMember(Name="pictureUrl1", EmitDefaultValue=false)]
        public string PictureUrl1 { get; set; }

        /// <summary>
        /// Gets or sets the URL of a picture of the aircraft.
        /// </summary>
        [DataMember(Name="pictureUrl2", EmitDefaultValue=false)]
        public string PictureUrl2 { get; set; }

        /// <summary>
        /// Gets or sets the URL of a picture of the aircraft.
        /// </summary>
        [DataMember(Name="pictureUrl3", EmitDefaultValue=false)]
        public string PictureUrl3 { get; set; }

        /// <summary>
        /// Gets or sets the total hours flown by the aircraft.
        /// </summary>
        [DataMember(Name="totalHours", EmitDefaultValue=false)]
        public string TotalHours { get; set; }

        /// <summary>
        /// Gets or sets the user's notes on the aircraft.
        /// </summary>
        [DataMember(Name="notes", EmitDefaultValue=false)]
        public string Notes { get; set; }

        /// <summary>
        /// Gets or sets the year the aircraft was manufactured.
        /// </summary>
        [DataMember(Name="yearBuilt", EmitDefaultValue=false)]
        public string YearBuilt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the user has a local picture of the aircraft.
        /// </summary>
        [DataMember(Name="hasPic", EmitDefaultValue=false)]
        public bool HasPicture { get; set; }

        /// <summary>
        /// Gets or sets the width of the picture in pixels.
        /// </summary>
        [DataMember(Name="picX", EmitDefaultValue=false)]
        public int PictureWidth { get; set; }

        /// <summary>
        /// Gets or sets the height of the picture in pixels.
        /// </summary>
        [DataMember(Name="picY", EmitDefaultValue=false)]
        public int PictureHeight { get; set; }
    }
}
