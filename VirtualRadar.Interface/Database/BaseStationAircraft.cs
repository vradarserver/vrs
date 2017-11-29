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

namespace VirtualRadar.Interface.Database
{
    #pragma warning disable 0659 // Equals overridden while GetHashCode is not - see notes against Equals
    /// <summary>
    /// A simple data transfer object that describes a record from the BaseStation database for an aircraft.
    /// </summary>
    public class BaseStationAircraft
    {
        /// <summary>
        /// Gets or sets the unique identifier of the aircraft in the database.
        /// </summary>
        public int AircraftID { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's 'class' - can't find out what this field actually is, guessing it's the species?
        /// We derive the species from the model ICAO, this is just for the sake of completeness.
        /// </summary>
        public string AircraftClass { get; set; }

        /// <summary>
        /// Gets or sets the certificate of airworthiness category.
        /// </summary>
        public string CofACategory { get; set; }

        /// <summary>
        /// Gets or sets the date that the certificate of airworthiness expires.
        /// </summary>
        public string CofAExpiry { get; set; }

        /// <summary>
        /// Gets or sets the country of the operator.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the date that the aircraft was registered.
        /// </summary>
        public string CurrentRegDate { get; set; }

        /// <summary>
        /// Gets or sets the date that the plane was taken off the register.
        /// </summary>
        public string DeRegDate { get; set; }

        /// <summary>
        /// Gets or sets a description of the engines on the aircraft.
        /// </summary>
        public string Engines { get; set; }

        /// <summary>
        /// Gets or sets the date and time (UTC) that the record was created.
        /// </summary>
        public DateTime FirstCreated { get; set; }

        /// <summary>
        /// Gets or sets the date that the aircraft was first placed onto the register.
        /// </summary>
        public string FirstRegDate { get; set; }

        /// <summary>
        /// Gets or sets the generic name for the type of aircraft.
        /// </summary>
        public string GenericName { get; set; }

        /// <summary>
        /// Gets or sets the ICAO 8643 type code for the aircraft.
        /// </summary>
        public string ICAOTypeCode { get; set; }

        /// <summary>
        /// Gets or sets the URL of a web page showing more information about the aircraft.
        /// </summary>
        public string InfoUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the BaseStation operator wants to be alerted when this aircraft appears.
        /// </summary>
        public bool Interested { get; set; }

        /// <summary>
        /// Gets or sets the date and time (UTC) that the record was last changed.
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Gets or sets the full name of the aircraft's manufacturer.
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// Gets or sets the 24-bit Mode-S identifier assigned to the aircraft.
        /// </summary>
        public string ModeS { get; set; }

        /// <summary>
        /// Gets or sets the country of registration.
        /// </summary>
        public string ModeSCountry { get; set; }

        /// <summary>
        /// Gets or sets the maximum takeoff weight of the aircraft.
        /// </summary>
        public string MTOW { get; set; }

        /// <summary>
        /// Gets or sets the ICAO code for the aircraft's operator.
        /// </summary>
        public string OperatorFlagCode { get; set; }

        /// <summary>
        /// Gets or sets a value describing whether the aircraft is owned outright, leased etc.
        /// </summary>
        public string OwnershipStatus { get; set; }

        /// <summary>
        /// Gets or sets the URL of a picture of the aircraft.
        /// </summary>
        public string PictureUrl1 { get; set; }

        /// <summary>
        /// Gets or sets the URL of a picture of the aircraft.
        /// </summary>
        public string PictureUrl2 { get; set; }

        /// <summary>
        /// Gets or sets the URL of a picture of the aircraft.
        /// </summary>
        public string PictureUrl3 { get; set; }

        /// <summary>
        /// Gets or sets the popular name for the type of aircraft.
        /// </summary>
        public string PopularName { get; set; }

        /// <summary>
        /// Gets or sets the previous Mode-S ID of the aircraft (I think? not sure, never seen this set).
        /// </summary>
        public string PreviousID { get; set; }

        /// <summary>
        /// Gets or sets the registration of the aircraft.
        /// </summary>
        public string Registration { get; set; }

        /// <summary>
        /// Gets or sets the operator of the aircraft.
        /// </summary>
        public string RegisteredOwners { get; set; }

        /// <summary>
        /// Gets or sets the manufacturers serial number assigned to the aircraft.
        /// </summary>
        public string SerialNo { get; set; }

        /// <summary>
        /// Gets or sets a value describing whether the aircraft is in service, mothballed etc.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the total hours flown by the aircraft.
        /// </summary>
        public string TotalHours { get; set; }

        /// <summary>
        /// Gets or sets a description of the model of the aircraft.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the user's notes on the aircraft.
        /// </summary>
        public string UserNotes { get; set; }

        /// <summary>
        /// Gets or sets the user tag on the aircraft.
        /// </summary>
        public string UserTag { get; set; }

        /// <summary>
        /// Gets or sets the year the aircraft was manufactured.
        /// </summary>
        public string YearBuilt { get; set; }

        /// <summary>
        /// Gets or sets the user string 1 field.
        /// </summary>
        public string UserString1 { get; set; }

        /// <summary>
        /// Gets or sets the user string 2 field.
        /// </summary>
        public string UserString2 { get; set; }

        /// <summary>
        /// Gets or sets the user string 3 field.
        /// </summary>
        public string UserString3 { get; set; }

        /// <summary>
        /// Gets or sets the user string 4 field.
        /// </summary>
        public string UserString4 { get; set; }

        /// <summary>
        /// Gets or sets the user string 5 field.
        /// </summary>
        public string UserString5 { get; set; }

        /// <summary>
        /// Gets or sets the user bool 1 field.
        /// </summary>
        public bool UserBool1 { get; set; }

        /// <summary>
        /// Gets or sets the user bool 2 field.
        /// </summary>
        public bool UserBool2 { get; set; }

        /// <summary>
        /// Gets or sets the user bool 3 field.
        /// </summary>
        public bool UserBool3 { get; set; }

        /// <summary>
        /// Gets or sets the user bool 4 field.
        /// </summary>
        public bool UserBool4 { get; set; }

        /// <summary>
        /// Gets or sets the user bool 5 field.
        /// </summary>
        public bool UserBool5 { get; set; }

        /// <summary>
        /// Gets or sets the user int 1 field.
        /// </summary>
        public long UserInt1 { get; set; }

        /// <summary>
        /// Gets or sets the user int 2 field.
        /// </summary>
        public long UserInt2 { get; set; }

        /// <summary>
        /// Gets or sets the user int 3 field.
        /// </summary>
        public long UserInt3 { get; set; }

        /// <summary>
        /// Gets or sets the user int 4 field.
        /// </summary>
        public long UserInt4 { get; set; }

        /// <summary>
        /// Gets or sets the user int 5 field.
        /// </summary>
        public long UserInt5 { get; set; }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var result = Object.ReferenceEquals(this, obj);
            if(!result) {
                var other = obj as BaseStationAircraft;
                result = other != null &&
                         AircraftID == other.AircraftID &&
                         AircraftClass == other.AircraftClass &&
                         CofACategory == other.CofACategory &&
                         CofAExpiry == other.CofAExpiry &&
                         Country == other.Country &&
                         CurrentRegDate == other.CurrentRegDate &&
                         DeRegDate == other.DeRegDate &&
                         Engines == other.Engines &&
                         FirstCreated == other.FirstCreated &&
                         FirstRegDate == other.FirstRegDate &&
                         GenericName == other.GenericName &&
                         ICAOTypeCode == other.ICAOTypeCode &&
                         InfoUrl == other.InfoUrl &&
                         Interested == other.Interested &&
                         LastModified == other.LastModified &&
                         Manufacturer == other.Manufacturer &&
                         ModeS == other.ModeS &&
                         ModeSCountry == other.ModeSCountry &&
                         MTOW == other.MTOW &&
                         OperatorFlagCode == other.OperatorFlagCode &&
                         OwnershipStatus == other.OwnershipStatus &&
                         PictureUrl1 == other.PictureUrl1 &&
                         PictureUrl2 == other.PictureUrl2 &&
                         PictureUrl3 == other.PictureUrl3 &&
                         PopularName == other.PopularName &&
                         PreviousID == other.PreviousID &&
                         Registration == other.Registration &&
                         RegisteredOwners == other.RegisteredOwners &&
                         SerialNo == other.SerialNo &&
                         Status == other.Status &&
                         TotalHours == other.TotalHours &&
                         Type == other.Type &&
                         UserBool1 == other.UserBool1 &&
                         UserBool2 == other.UserBool2 &&
                         UserBool3 == other.UserBool3 &&
                         UserBool4 == other.UserBool4 &&
                         UserBool5 == other.UserBool5 &&
                         UserInt1 == other.UserInt1 &&
                         UserInt2 == other.UserInt2 &&
                         UserInt3 == other.UserInt3 &&
                         UserInt4 == other.UserInt4 &&
                         UserInt5 == other.UserInt5 &&
                         UserNotes == other.UserNotes &&
                         UserString1 == other.UserString1 &&
                         UserString2 == other.UserString2 &&
                         UserString3 == other.UserString3 &&
                         UserString4 == other.UserString4 &&
                         UserString5 == other.UserString5 &&
                         UserTag == other.UserTag &&
                         YearBuilt == other.YearBuilt;
            }

            return result;
        }
    }
}
