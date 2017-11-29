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
using Dapper;
using VirtualRadar.Interface.Database;

namespace VirtualRadar.Database.BaseStation
{
    /// <summary>
    /// Builds Dapper parameters from BaseStation database objects.
    /// </summary>
    public static class ParameterBuilder
    {
        /// <summary>
        /// Builds a Dapper parameters object for an aircraft.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="includeAircraftID"></param>
        /// <returns></returns>
        public static DynamicParameters FromAircraft(BaseStationAircraft aircraft, bool includeAircraftID = true)
        {
            var result = new DynamicParameters();

            if(includeAircraftID) {
                result.Add(nameof(aircraft.AircraftID),     value: aircraft.AircraftID);
            }
            result.Add(nameof(aircraft.AircraftClass),      value: aircraft.AircraftClass);
            result.Add(nameof(aircraft.CofACategory),       value: aircraft.CofACategory);
            result.Add(nameof(aircraft.CofAExpiry),         value: aircraft.CofAExpiry);
            result.Add(nameof(aircraft.Country),            value: aircraft.Country);
            result.Add(nameof(aircraft.CurrentRegDate),     value: aircraft.CurrentRegDate);
            result.Add(nameof(aircraft.DeRegDate),          value: aircraft.DeRegDate);
            result.Add(nameof(aircraft.Engines),            value: aircraft.Engines);
            result.Add(nameof(aircraft.FirstCreated),       value: aircraft.FirstCreated);
            result.Add(nameof(aircraft.FirstRegDate),       value: aircraft.FirstRegDate);
            result.Add(nameof(aircraft.GenericName),        value: aircraft.GenericName);
            result.Add(nameof(aircraft.ICAOTypeCode),       value: aircraft.ICAOTypeCode);
            result.Add(nameof(aircraft.InfoUrl),            value: aircraft.InfoUrl);
            result.Add(nameof(aircraft.Interested),         value: aircraft.Interested);
            result.Add(nameof(aircraft.LastModified),       value: aircraft.LastModified);
            result.Add(nameof(aircraft.Manufacturer),       value: aircraft.Manufacturer);
            result.Add(nameof(aircraft.ModeS),              value: aircraft.ModeS);
            result.Add(nameof(aircraft.ModeSCountry),       value: aircraft.ModeSCountry);
            result.Add(nameof(aircraft.MTOW),               value: aircraft.MTOW);
            result.Add(nameof(aircraft.OperatorFlagCode),   value: aircraft.OperatorFlagCode);
            result.Add(nameof(aircraft.OwnershipStatus),    value: aircraft.OwnershipStatus);
            result.Add(nameof(aircraft.PictureUrl1),        value: aircraft.PictureUrl1);
            result.Add(nameof(aircraft.PictureUrl2),        value: aircraft.PictureUrl2);
            result.Add(nameof(aircraft.PictureUrl3),        value: aircraft.PictureUrl3);
            result.Add(nameof(aircraft.PopularName),        value: aircraft.PopularName);
            result.Add(nameof(aircraft.PreviousID),         value: aircraft.PreviousID);
            result.Add(nameof(aircraft.Registration),       value: aircraft.Registration);
            result.Add(nameof(aircraft.RegisteredOwners),   value: aircraft.RegisteredOwners);
            result.Add(nameof(aircraft.SerialNo),           value: aircraft.SerialNo);
            result.Add(nameof(aircraft.Status),             value: aircraft.Status);
            result.Add(nameof(aircraft.TotalHours),         value: aircraft.TotalHours);
            result.Add(nameof(aircraft.Type),               value: aircraft.Type);
            result.Add(nameof(aircraft.UserNotes),          value: aircraft.UserNotes);
            result.Add(nameof(aircraft.UserTag),            value: aircraft.UserTag);
            result.Add(nameof(aircraft.YearBuilt),          value: aircraft.YearBuilt);
            result.Add(nameof(aircraft.UserString1),        value: aircraft.UserString1);
            result.Add(nameof(aircraft.UserString2),        value: aircraft.UserString2);
            result.Add(nameof(aircraft.UserString3),        value: aircraft.UserString3);
            result.Add(nameof(aircraft.UserString4),        value: aircraft.UserString4);
            result.Add(nameof(aircraft.UserString5),        value: aircraft.UserString5);
            result.Add(nameof(aircraft.UserBool1),          value: aircraft.UserBool1);
            result.Add(nameof(aircraft.UserBool2),          value: aircraft.UserBool2);
            result.Add(nameof(aircraft.UserBool3),          value: aircraft.UserBool3);
            result.Add(nameof(aircraft.UserBool4),          value: aircraft.UserBool4);
            result.Add(nameof(aircraft.UserBool5),          value: aircraft.UserBool5);
            result.Add(nameof(aircraft.UserInt1),           value: aircraft.UserInt1);
            result.Add(nameof(aircraft.UserInt2),           value: aircraft.UserInt2);
            result.Add(nameof(aircraft.UserInt3),           value: aircraft.UserInt3);
            result.Add(nameof(aircraft.UserInt4),           value: aircraft.UserInt4);
            result.Add(nameof(aircraft.UserInt5),           value: aircraft.UserInt5);

            return result;
        }

        /// <summary>
        /// Returns parameters for a flight.
        /// </summary>
        /// <param name="flight"></param>
        /// <param name="includeFlightID"></param>
        /// <returns></returns>
        public static DynamicParameters FromFlight(BaseStationFlight flight, bool includeFlightID = true)
        {
            var result = new DynamicParameters();

            if(includeFlightID) {
                result.Add(nameof(flight.FlightID),        value: flight.FlightID);
            }
            result.Add(nameof(flight.AircraftID),          value: flight.AircraftID);
            result.Add(nameof(flight.Callsign),            value: flight.Callsign);
            result.Add(nameof(flight.EndTime),             value: flight.EndTime);
            result.Add(nameof(flight.FirstAltitude),       value: flight.FirstAltitude);
            result.Add(nameof(flight.FirstGroundSpeed),    value: flight.FirstGroundSpeed);
            result.Add(nameof(flight.FirstIsOnGround),     value: flight.FirstIsOnGround);
            result.Add(nameof(flight.FirstLat),            value: flight.FirstLat);
            result.Add(nameof(flight.FirstLon),            value: flight.FirstLon);
            result.Add(nameof(flight.FirstSquawk),         value: flight.FirstSquawk);
            result.Add(nameof(flight.FirstTrack),          value: flight.FirstTrack);
            result.Add(nameof(flight.FirstVerticalRate),   value: flight.FirstVerticalRate);
            result.Add(nameof(flight.HadAlert),            value: flight.HadAlert);
            result.Add(nameof(flight.HadEmergency),        value: flight.HadEmergency);
            result.Add(nameof(flight.HadSpi),              value: flight.HadSpi);
            result.Add(nameof(flight.LastAltitude),        value: flight.LastAltitude);
            result.Add(nameof(flight.LastGroundSpeed),     value: flight.LastGroundSpeed);
            result.Add(nameof(flight.LastIsOnGround),      value: flight.LastIsOnGround);
            result.Add(nameof(flight.LastLat),             value: flight.LastLat);
            result.Add(nameof(flight.LastLon),             value: flight.LastLon);
            result.Add(nameof(flight.LastSquawk),          value: flight.LastSquawk);
            result.Add(nameof(flight.LastTrack),           value: flight.LastTrack);
            result.Add(nameof(flight.LastVerticalRate),    value: flight.LastVerticalRate);
            result.Add(nameof(flight.NumADSBMsgRec),       value: flight.NumADSBMsgRec);
            result.Add(nameof(flight.NumModeSMsgRec),      value: flight.NumModeSMsgRec);
            result.Add(nameof(flight.NumIDMsgRec),         value: flight.NumIDMsgRec);
            result.Add(nameof(flight.NumSurPosMsgRec),     value: flight.NumSurPosMsgRec);
            result.Add(nameof(flight.NumAirPosMsgRec),     value: flight.NumAirPosMsgRec);
            result.Add(nameof(flight.NumAirVelMsgRec),     value: flight.NumAirVelMsgRec);
            result.Add(nameof(flight.NumSurAltMsgRec),     value: flight.NumSurAltMsgRec);
            result.Add(nameof(flight.NumSurIDMsgRec),      value: flight.NumSurIDMsgRec);
            result.Add(nameof(flight.NumAirToAirMsgRec),   value: flight.NumAirToAirMsgRec);
            result.Add(nameof(flight.NumAirCallRepMsgRec), value: flight.NumAirCallRepMsgRec);
            result.Add(nameof(flight.NumPosMsgRec),        value: flight.NumPosMsgRec);
            result.Add(nameof(flight.SessionID),           value: flight.SessionID);
            result.Add(nameof(flight.StartTime),           value: flight.StartTime);

            return result;
        }

        /// <summary>
        /// Generates parameters for a DB history object.
        /// </summary>
        /// <param name="dbHistory"></param>
        /// <param name="includeHistoryID"></param>
        /// <returns></returns>
        public static DynamicParameters FromDBHistory(BaseStationDBHistory dbHistory, bool includeHistoryID = true)
        {
            var result = new DynamicParameters();

            if(includeHistoryID) {
                result.Add(nameof(dbHistory.DBHistoryID),   value: dbHistory.DBHistoryID);
            }
            result.Add(nameof(dbHistory.TimeStamp),         value: dbHistory.TimeStamp);
            result.Add(nameof(dbHistory.Description),       value: dbHistory.Description);

            return result;
        }

        /// <summary>
        /// Generates parameters for a DB info object.
        /// </summary>
        /// <param name="dbInfo"></param>
        /// <returns></returns>
        public static DynamicParameters FromDBInfo(BaseStationDBInfo dbInfo)
        {
            var result = new DynamicParameters();

            result.Add(nameof(dbInfo.OriginalVersion), value: dbInfo.OriginalVersion);
            result.Add(nameof(dbInfo.CurrentVersion),  value: dbInfo.CurrentVersion);

            return result;
        }

        /// <summary>
        /// Generates parameters for a location object.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="includeLocationID"></param>
        /// <returns></returns>
        public static DynamicParameters FromLocation(BaseStationLocation location, bool includeLocationID = true)
        {
            var result = new DynamicParameters();

            if(includeLocationID) {
                result.Add(nameof(location.LocationID), value: location.LocationID);
            }
            result.Add(nameof(location.LocationName),   value: location.LocationName);
            result.Add(nameof(location.Latitude),       value: location.Latitude);
            result.Add(nameof(location.Longitude),      value: location.Longitude);
            result.Add(nameof(location.Altitude),       value: location.Altitude);

            return result;
        }

        /// <summary>
        /// Generates parameters for a session object.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="includeSessionID"></param>
        /// <returns></returns>
        public static DynamicParameters FromSession(BaseStationSession session, bool includeSessionID = true)
        {
            var result = new DynamicParameters();

            if(includeSessionID) {
                result.Add(nameof(session.SessionID),   value: session.SessionID);
            }
            result.Add(nameof(session.LocationID),      value: session.LocationID);
            result.Add(nameof(session.StartTime),       value: session.StartTime);
            result.Add(nameof(session.EndTime),         value: session.EndTime);

            return result;
        }

        /// <summary>
        /// Generates parameters for a system event object.
        /// </summary>
        /// <param name="systemEvent"></param>
        /// <param name="includeSystemEventID"></param>
        /// <returns></returns>
        public static DynamicParameters FromSystemEvent(BaseStationSystemEvents systemEvent, bool includeSystemEventID = true)
        {
            var result = new DynamicParameters();

            if(includeSystemEventID) {
                result.Add(nameof(systemEvent.SystemEventsID),  value: systemEvent.SystemEventsID);
            }
            result.Add(nameof(systemEvent.TimeStamp),           value: systemEvent.TimeStamp);
            result.Add(nameof(systemEvent.App),                 value: systemEvent.App);
            result.Add(nameof(systemEvent.Msg),                 value: systemEvent.Msg);

            return result;
        }
    }
}
