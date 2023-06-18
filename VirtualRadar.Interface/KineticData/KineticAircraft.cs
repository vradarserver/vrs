﻿// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Interface.Types;

namespace VirtualRadar.Interface.KineticData
{
    /// <summary>
    /// A simple data transfer object that describes a record from the BaseStation database for an aircraft.
    /// </summary>
    public class KineticAircraft
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

        private DateTime _FirstCreated;
        /// <summary>
        /// Gets or sets the date and time (UTC) that the record was created. Milliseconds
        /// are stripped from the date when stored.
        /// </summary>
        public DateTime FirstCreated
        {
            get => _FirstCreated;
            set => _FirstCreated = value.TruncateMilliseconds();
        }

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

        private DateTime _LastModified;
        /// <summary>
        /// Gets or sets the date and time (UTC) that the record was last changed.
        /// </summary>
        public DateTime LastModified
        {
            get => _LastModified;
            set => _LastModified = value.TruncateMilliseconds();
        }

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
        /// Creates a new object.
        /// </summary>
        public KineticAircraft()
        {
        }

        /// <summary>
        /// Creates a new aircraft record from an existing record,
        /// </summary>
        /// <param name="donorAircraft"></param>
        /// <param name="copyAircraftId"></param>
        public KineticAircraft(KineticAircraft donorAircraft, bool copyAircraftId)
        {
            if(copyAircraftId) {
                AircraftID = donorAircraft.AircraftID;
            }

            AircraftClass =     donorAircraft.AircraftClass;
            CofACategory =      donorAircraft.CofACategory;
            CofAExpiry =        donorAircraft.CofAExpiry;
            Country =           donorAircraft.Country;
            CurrentRegDate =    donorAircraft.CurrentRegDate;
            DeRegDate =         donorAircraft.DeRegDate;
            Engines =           donorAircraft.Engines;
            FirstCreated =      donorAircraft.FirstCreated;
            FirstRegDate =      donorAircraft.FirstRegDate;
            GenericName =       donorAircraft.GenericName;
            ICAOTypeCode =      donorAircraft.ICAOTypeCode;
            InfoUrl =           donorAircraft.InfoUrl;
            Interested =        donorAircraft.Interested;
            LastModified =      donorAircraft.LastModified;
            Manufacturer =      donorAircraft.Manufacturer;
            ModeS =             donorAircraft.ModeS;
            ModeSCountry =      donorAircraft.ModeSCountry;
            MTOW =              donorAircraft.MTOW;
            OperatorFlagCode =  donorAircraft.OperatorFlagCode;
            OwnershipStatus =   donorAircraft.OwnershipStatus;
            PictureUrl1 =       donorAircraft.PictureUrl1;
            PictureUrl2 =       donorAircraft.PictureUrl2;
            PictureUrl3 =       donorAircraft.PictureUrl3;
            PopularName =       donorAircraft.PopularName;
            PreviousID =        donorAircraft.PreviousID;
            Registration =      donorAircraft.Registration;
            RegisteredOwners =  donorAircraft.RegisteredOwners;
            SerialNo =          donorAircraft.SerialNo;
            Status =            donorAircraft.Status;
            TotalHours =        donorAircraft.TotalHours;
            Type =              donorAircraft.Type;
            UserNotes =         donorAircraft.UserNotes;
            UserTag =           donorAircraft.UserTag;
            YearBuilt =         donorAircraft.YearBuilt;
            UserString1 =       donorAircraft.UserString1;
            UserString2 =       donorAircraft.UserString2;
            UserString3 =       donorAircraft.UserString3;
            UserString4 =       donorAircraft.UserString4;
            UserString5 =       donorAircraft.UserString5;
            UserBool1 =         donorAircraft.UserBool1;
            UserBool2 =         donorAircraft.UserBool2;
            UserBool3 =         donorAircraft.UserBool3;
            UserBool4 =         donorAircraft.UserBool4;
            UserBool5 =         donorAircraft.UserBool5;
            UserInt1 =          donorAircraft.UserInt1;
            UserInt2 =          donorAircraft.UserInt2;
            UserInt3 =          donorAircraft.UserInt3;
            UserInt4 =          donorAircraft.UserInt4;
            UserInt5 =          donorAircraft.UserInt5;
        }

        /// <summary>
        /// Copies the object's values to another aircraft record.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="copyAircraftId"></param>
        public void ApplyTo(KineticAircraft aircraft, bool copyAircraftId)
        {
            if(copyAircraftId) {
                aircraft.AircraftID = AircraftID;
            }

            aircraft.AircraftClass =     AircraftClass;
            aircraft.CofACategory =      CofACategory;
            aircraft.CofAExpiry =        CofAExpiry;
            aircraft.Country =           Country;
            aircraft.CurrentRegDate =    CurrentRegDate;
            aircraft.DeRegDate =         DeRegDate;
            aircraft.Engines =           Engines;
            aircraft.FirstCreated =      FirstCreated;
            aircraft.FirstRegDate =      FirstRegDate;
            aircraft.GenericName =       GenericName;
            aircraft.ICAOTypeCode =      ICAOTypeCode;
            aircraft.InfoUrl =           InfoUrl;
            aircraft.Interested =        Interested;
            aircraft.LastModified =      LastModified;
            aircraft.Manufacturer =      Manufacturer;
            aircraft.ModeS =             ModeS;
            aircraft.ModeSCountry =      ModeSCountry;
            aircraft.MTOW =              MTOW;
            aircraft.OperatorFlagCode =  OperatorFlagCode;
            aircraft.OwnershipStatus =   OwnershipStatus;
            aircraft.PictureUrl1 =       PictureUrl1;
            aircraft.PictureUrl2 =       PictureUrl2;
            aircraft.PictureUrl3 =       PictureUrl3;
            aircraft.PopularName =       PopularName;
            aircraft.PreviousID =        PreviousID;
            aircraft.Registration =      Registration;
            aircraft.RegisteredOwners =  RegisteredOwners;
            aircraft.SerialNo =          SerialNo;
            aircraft.Status =            Status;
            aircraft.TotalHours =        TotalHours;
            aircraft.Type =              Type;
            aircraft.UserNotes =         UserNotes;
            aircraft.UserTag =           UserTag;
            aircraft.YearBuilt =         YearBuilt;
            aircraft.UserString1 =       UserString1;
            aircraft.UserString2 =       UserString2;
            aircraft.UserString3 =       UserString3;
            aircraft.UserString4 =       UserString4;
            aircraft.UserString5 =       UserString5;
            aircraft.UserBool1 =         UserBool1;
            aircraft.UserBool2 =         UserBool2;
            aircraft.UserBool3 =         UserBool3;
            aircraft.UserBool4 =         UserBool4;
            aircraft.UserBool5 =         UserBool5;
            aircraft.UserInt1 =          UserInt1;
            aircraft.UserInt2 =          UserInt2;
            aircraft.UserInt3 =          UserInt3;
            aircraft.UserInt4 =          UserInt4;
            aircraft.UserInt5 =          UserInt5;
        }

        /// <summary>
        /// Returns true if the other object has the same values as this one.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual bool IsSameAircraft(KineticAircraft other, bool considerAircraftId)
        {
            var result = Object.ReferenceEquals(this, other);
            if(!result) {
                result = other != null
                      && (!considerAircraftId || AircraftID == other.AircraftID)
                      && AircraftClass == other.AircraftClass
                      && CofACategory == other.CofACategory
                      && CofAExpiry == other.CofAExpiry
                      && Country == other.Country
                      && CurrentRegDate == other.CurrentRegDate
                      && DeRegDate == other.DeRegDate
                      && Engines == other.Engines
                      && FirstCreated == other.FirstCreated
                      && FirstRegDate == other.FirstRegDate
                      && GenericName == other.GenericName
                      && ICAOTypeCode == other.ICAOTypeCode
                      && InfoUrl == other.InfoUrl
                      && Interested == other.Interested
                      && LastModified == other.LastModified
                      && Manufacturer == other.Manufacturer
                      && ModeS == other.ModeS
                      && ModeSCountry == other.ModeSCountry
                      && MTOW == other.MTOW
                      && OperatorFlagCode == other.OperatorFlagCode
                      && OwnershipStatus == other.OwnershipStatus
                      && PictureUrl1 == other.PictureUrl1
                      && PictureUrl2 == other.PictureUrl2
                      && PictureUrl3 == other.PictureUrl3
                      && PopularName == other.PopularName
                      && PreviousID == other.PreviousID
                      && Registration == other.Registration
                      && RegisteredOwners == other.RegisteredOwners
                      && SerialNo == other.SerialNo
                      && Status == other.Status
                      && TotalHours == other.TotalHours
                      && Type == other.Type
                      && UserBool1 == other.UserBool1
                      && UserBool2 == other.UserBool2
                      && UserBool3 == other.UserBool3
                      && UserBool4 == other.UserBool4
                      && UserBool5 == other.UserBool5
                      && UserInt1 == other.UserInt1
                      && UserInt2 == other.UserInt2
                      && UserInt3 == other.UserInt3
                      && UserInt4 == other.UserInt4
                      && UserInt5 == other.UserInt5
                      && UserNotes == other.UserNotes
                      && UserString1 == other.UserString1
                      && UserString2 == other.UserString2
                      && UserString3 == other.UserString3
                      && UserString4 == other.UserString4
                      && UserString5 == other.UserString5
                      && UserTag == other.UserTag
                      && YearBuilt == other.YearBuilt;
            }

            return result;
        }
    }
}
