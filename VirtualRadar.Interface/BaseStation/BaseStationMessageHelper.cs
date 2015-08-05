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
using System.Text;
using VirtualRadar.Interface;

namespace VirtualRadar.Interface.BaseStation
{
    /// <summary>
    /// A static class containing methods that may be of use when handling BaseStation messages.
    /// </summary>
    /// <remarks>
    /// When <see cref="BaseStationMessage"/> was an interface this was used to help with writing implementations
    /// of the interface. Now that the interface has gone there is an argument for rolling this stuff into
    /// <see cref="BaseStationMessage"/>.
    /// </remarks>
    public static class BaseStationMessageHelper
    {
        /// <summary>
        /// Converts from BaseStation text to a message type.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="isMlat"></param>
        /// <returns></returns>
        public static BaseStationMessageType ConvertToBaseStationMessageType(string text, ref bool isMlat)
        {
            switch(text) {
                case "SEL":     return BaseStationMessageType.UserClicked;
                case "ID":      return BaseStationMessageType.NewIdentifier;
                case "AIR":     return BaseStationMessageType.NewAircraft;
                case "STA":     return BaseStationMessageType.StatusChange;
                case "CLK":     return BaseStationMessageType.UserDoubleClicked;
                case "MSG":     return BaseStationMessageType.Transmission;
                case "MLAT":    isMlat = true; goto case "MSG";
                default:        return BaseStationMessageType.Unknown;
            }
        }

        /// <summary>
        /// Converts from a message type to the expected BaseStation text.
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="isMlatSourced"></param>
        /// <param name="useExtendedFormat"></param>
        /// <returns></returns>
        public static string ConvertToString(BaseStationMessageType messageType, bool isMlatSourced = false, bool useExtendedFormat = false)
        {
            switch(messageType) {
                case BaseStationMessageType.NewAircraft:                return "AIR";
                case BaseStationMessageType.NewIdentifier:              return "ID";
                case BaseStationMessageType.StatusChange:               return "STA";
                case BaseStationMessageType.Unknown:                    return "";
                case BaseStationMessageType.UserClicked:                return "SEL";
                case BaseStationMessageType.UserDoubleClicked:          return "CLK";
                case BaseStationMessageType.Transmission:               return !isMlatSourced || !useExtendedFormat ? "MSG" : "MLAT";
                default:                                                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Converts strings into <see cref="BaseStationTransmissionType"/> enumeration values.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static BaseStationTransmissionType ConvertToBaseStationTransmissionType(string text)
        {
            switch(text) {
                case "1":   return BaseStationTransmissionType.IdentificationAndCategory;
                case "2":   return BaseStationTransmissionType.SurfacePosition;
                case "3":   return BaseStationTransmissionType.AirbornePosition;
                case "4":   return BaseStationTransmissionType.AirborneVelocity;
                case "5":   return BaseStationTransmissionType.SurveillanceAlt;
                case "6":   return BaseStationTransmissionType.SurveillanceId;
                case "7":   return BaseStationTransmissionType.AirToAir;
                case "8":   return BaseStationTransmissionType.AllCallReply;
                default:    return BaseStationTransmissionType.None;
            }
        }

        /// <summary>
        /// Converts <see cref="BaseStationTransmissionType"/> values into strings.
        /// </summary>
        /// <param name="transmissionType"></param>
        /// <returns></returns>
        public static string ConvertToString(BaseStationTransmissionType transmissionType)
        {
            switch(transmissionType) {
                case BaseStationTransmissionType.AirbornePosition:              return "3";
                case BaseStationTransmissionType.AirborneVelocity:              return "4";
                case BaseStationTransmissionType.AirToAir:                      return "7";
                case BaseStationTransmissionType.AllCallReply:                  return "8";
                case BaseStationTransmissionType.IdentificationAndCategory:     return "1";
                case BaseStationTransmissionType.None:                          return "";
                case BaseStationTransmissionType.SurfacePosition:               return "2";
                case BaseStationTransmissionType.SurveillanceAlt:               return "5";
                case BaseStationTransmissionType.SurveillanceId:                return "6";
                default:                                                        throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Converts a string to a <see cref="BaseStationStatusCode"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static BaseStationStatusCode ConvertToBaseStationStatusCode(string text)
        {
            switch(text) {
                case "PL":      return BaseStationStatusCode.PositionLost;
                case "SL":      return BaseStationStatusCode.SignalLost;
                case "RM":      return BaseStationStatusCode.Remove;
                case "AD":      return BaseStationStatusCode.Delete;
                case "OK":      return BaseStationStatusCode.OK;
                default:        return BaseStationStatusCode.None;
            }
        }

        /// <summary>
        /// Converts a <see cref="BaseStationStatusCode"/> to a string.
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public static string ConvertToString(BaseStationStatusCode statusCode)
        {
            switch(statusCode) {
                case BaseStationStatusCode.Delete:          return "AD";
                case BaseStationStatusCode.None:            return "";
                case BaseStationStatusCode.OK:              return "OK";
                case BaseStationStatusCode.PositionLost:    return "PL";
                case BaseStationStatusCode.Remove:          return "RM";
                case BaseStationStatusCode.SignalLost:      return "SL";
                default:                                    throw new NotImplementedException();
            }
        }
    }
}
