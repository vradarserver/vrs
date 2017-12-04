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
    /// Represents the BaseStationAircraftUpsert UDTT.
    /// </summary>
    class BaseStationAircraftUpsertUdtt
    {
        /// <summary>
        /// Gets the pre-generated set of UDTT properties for the class.
        /// </summary>
        public static UdttProperty<BaseStationAircraftUpsertUdtt>[] UdttProperties { get; private set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(0)]
        public string ModeS { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(1)]
        public DateTime FirstCreated { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(2)]
        public DateTime LastModified { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(3)]
        public string ModeSCountry { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(4)]
        public string Country { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(5)]
        public string Registration { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(6)]
        public string CurrentRegDate { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(7)]
        public string PreviousID { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(8)]
        public string FirstRegDate { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(9)]
        public string Status { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(10)]
        public string DeRegDate { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(11)]
        public string Manufacturer { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(12)]
        public string ICAOTypeCode { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(13)]
        public string Type { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(14)]
        public string SerialNo { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(15)]
        public string PopularName { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(16)]
        public string GenericName { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(17)]
        public string AircraftClass { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(18)]
        public string Engines { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(19)]
        public string OwnershipStatus { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(20)]
        public string RegisteredOwners { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(21)]
        public string MTOW { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(22)]
        public string TotalHours { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(23)]
        public string YearBuilt { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(24)]
        public string CofACategory { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(25)]
        public string CofAExpiry { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(26)]
        public string UserNotes { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(27)]
        public bool Interested { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(28)]
        public string UserTag { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(29)]
        public string InfoURL { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(30)]
        public string PictureURL1 { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(31)]
        public string PictureURL2 { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(32)]
        public string PictureURL3 { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(33)]
        public bool UserBool1 { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(34)]
        public bool UserBool2 { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(35)]
        public bool UserBool3 { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(36)]
        public bool UserBool4 { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(37)]
        public bool UserBool5 { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(38)]
        public string UserString1 { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(39)]
        public string UserString2 { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(40)]
        public string UserString3 { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(41)]
        public string UserString4 { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(42)]
        public string UserString5 { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(43)]
        public long UserInt1 { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(44)]
        public long UserInt2 { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(45)]
        public long UserInt3 { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(46)]
        public long UserInt4 { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(47)]
        public long UserInt5 { get; set; }

        /// <summary>
        /// See UDTT declaration
        /// </summary>
        [Ordinal(48)]
        public string OperatorFlagCode { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public BaseStationAircraftUpsertUdtt()
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="aircraft"></param>
        public BaseStationAircraftUpsertUdtt(BaseStationAircraftUpsert aircraft)
        {
            ModeS =             aircraft.ModeS;
            FirstCreated =      aircraft.FirstCreated;
            LastModified =      aircraft.LastModified;
            ModeSCountry =      aircraft.ModeSCountry;
            Country =           aircraft.Country;
            Registration =      aircraft.Registration;
            CurrentRegDate =    aircraft.CurrentRegDate;
            PreviousID =        aircraft.PreviousID;
            FirstRegDate =      aircraft.FirstRegDate;
            Status =            aircraft.Status;
            DeRegDate =         aircraft.DeRegDate;
            Manufacturer =      aircraft.Manufacturer;
            ICAOTypeCode =      aircraft.ICAOTypeCode;
            Type =              aircraft.Type;
            SerialNo =          aircraft.SerialNo;
            PopularName =       aircraft.PopularName;
            GenericName =       aircraft.GenericName;
            AircraftClass =     aircraft.AircraftClass;
            Engines =           aircraft.Engines;
            OwnershipStatus =   aircraft.OwnershipStatus;
            RegisteredOwners =  aircraft.RegisteredOwners;
            MTOW =              aircraft.MTOW;
            TotalHours =        aircraft.TotalHours;
            YearBuilt =         aircraft.YearBuilt;
            CofACategory =      aircraft.CofACategory;
            CofAExpiry =        aircraft.CofAExpiry;
            UserNotes =         aircraft.UserNotes;
            Interested =        aircraft.Interested;
            UserTag =           aircraft.UserTag;
            InfoURL =           aircraft.InfoUrl;
            PictureURL1 =       aircraft.PictureUrl1;
            PictureURL2 =       aircraft.PictureUrl2;
            PictureURL3 =       aircraft.PictureUrl3;
            UserBool1 =         aircraft.UserBool1;
            UserBool2 =         aircraft.UserBool2;
            UserBool3 =         aircraft.UserBool3;
            UserBool4 =         aircraft.UserBool4;
            UserBool5 =         aircraft.UserBool5;
            UserString1 =       aircraft.UserString1;
            UserString2 =       aircraft.UserString2;
            UserString3 =       aircraft.UserString3;
            UserString4 =       aircraft.UserString4;
            UserString5 =       aircraft.UserString5;
            UserInt1 =          aircraft.UserInt1;
            UserInt2 =          aircraft.UserInt2;
            UserInt3 =          aircraft.UserInt3;
            UserInt4 =          aircraft.UserInt4;
            UserInt5 =          aircraft.UserInt5;
            OperatorFlagCode =  aircraft.OperatorFlagCode;
        }

        /// <summary>
        /// Static constructor.
        /// </summary>
        static BaseStationAircraftUpsertUdtt()
        {
            UdttProperties = UdttProperty<BaseStationAircraftUpsertUdtt>.GetUdttProperties();
        }
    }
}
