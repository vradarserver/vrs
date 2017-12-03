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
    public class BaseStationAircraft : BaseStationAircraftUpsert
    {
        /// <summary>
        /// Gets or sets the unique identifier of the aircraft in the database.
        /// </summary>
        public int AircraftID { get; set; }

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
