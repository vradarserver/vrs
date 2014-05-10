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
        /// <returns></returns>
        public static BaseStationMessageType ConvertToBaseStationMessageType(string text)
        {
            BaseStationMessageType result = BaseStationMessageType.Unknown;

            if(!String.IsNullOrEmpty(text)) {
                switch(text) {
                    case "SEL":     result = BaseStationMessageType.UserClicked; break;
                    case "ID":      result = BaseStationMessageType.NewIdentifier; break;
                    case "AIR":     result = BaseStationMessageType.NewAircraft; break;
                    case "STA":     result = BaseStationMessageType.StatusChange; break;
                    case "CLK":     result = BaseStationMessageType.UserDoubleClicked; break;
                    case "MSG":     result = BaseStationMessageType.Transmission; break;
                }
            }

            return result;
        }

        /// <summary>
        /// Converts from a message type to the expected BaseStation text.
        /// </summary>
        /// <param name="messageType"></param>
        /// <returns></returns>
        public static string ConvertToString(BaseStationMessageType messageType)
        {
            string result = null;
            switch(messageType) {
                case BaseStationMessageType.NewAircraft:        result = "AIR"; break;
                case BaseStationMessageType.NewIdentifier:      result = "ID"; break;
                case BaseStationMessageType.StatusChange:       result = "STA"; break;
                case BaseStationMessageType.Transmission:       result = "MSG"; break;
                case BaseStationMessageType.Unknown:            result = ""; break;
                case BaseStationMessageType.UserClicked:        result = "SEL"; break;
                case BaseStationMessageType.UserDoubleClicked:  result = "CLK"; break;
                default:                                        throw new NotImplementedException();
            }

            return result;
        }

        /// <summary>
        /// Converts strings into <see cref="BaseStationTransmissionType"/> enumeration values.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static BaseStationTransmissionType ConvertToBaseStationTransmissionType(string text)
        {
            BaseStationTransmissionType result = BaseStationTransmissionType.None;

            if(!String.IsNullOrEmpty(text)) {
                switch(text) {
                    case "1":   result = BaseStationTransmissionType.IdentificationAndCategory; break;
                    case "2":   result = BaseStationTransmissionType.SurfacePosition; break;
                    case "3":   result = BaseStationTransmissionType.AirbornePosition; break;
                    case "4":   result = BaseStationTransmissionType.AirborneVelocity; break;
                    case "5":   result = BaseStationTransmissionType.SurveillanceAlt; break;
                    case "6":   result = BaseStationTransmissionType.SurveillanceId; break;
                    case "7":   result = BaseStationTransmissionType.AirToAir; break;
                    case "8":   result = BaseStationTransmissionType.AllCallReply; break;
                }
            }

            return result;
        }

        /// <summary>
        /// Converts <see cref="BaseStationTransmissionType"/> values into strings.
        /// </summary>
        /// <param name="transmissionType"></param>
        /// <returns></returns>
        public static string ConvertToString(BaseStationTransmissionType transmissionType)
        {
            string result = "";

            switch(transmissionType) {
                case BaseStationTransmissionType.AirbornePosition:              result = "3"; break;
                case BaseStationTransmissionType.AirborneVelocity:              result = "4"; break;
                case BaseStationTransmissionType.AirToAir:                      result = "7"; break;
                case BaseStationTransmissionType.AllCallReply:                  result = "8"; break;
                case BaseStationTransmissionType.IdentificationAndCategory:     result = "1"; break;
                case BaseStationTransmissionType.None:                          break;
                case BaseStationTransmissionType.SurfacePosition:               result = "2"; break;
                case BaseStationTransmissionType.SurveillanceAlt:               result = "5"; break;
                case BaseStationTransmissionType.SurveillanceId:                result = "6"; break;
                default:                                                        throw new NotImplementedException();
            }

            return result;
        }

        /// <summary>
        /// Converts a string to a <see cref="BaseStationStatusCode"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static BaseStationStatusCode ConvertToBaseStationStatusCode(string text)
        {
            BaseStationStatusCode result = BaseStationStatusCode.None;

            if(!String.IsNullOrEmpty(text)) {
                switch(text) {
                    case "PL":      result = BaseStationStatusCode.PositionLost; break;
                    case "SL":      result = BaseStationStatusCode.SignalLost; break;
                    case "RM":      result = BaseStationStatusCode.Remove; break;
                    case "AD":      result = BaseStationStatusCode.Delete; break;
                    case "OK":      result = BaseStationStatusCode.OK; break;
                }
            }

            return result;
        }

        /// <summary>
        /// Converts a <see cref="BaseStationStatusCode"/> to a string.
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public static string ConvertToString(BaseStationStatusCode statusCode)
        {
            string result = "";

            switch(statusCode) {
                case BaseStationStatusCode.Delete:          result = "AD"; break;
                case BaseStationStatusCode.None:            break;
                case BaseStationStatusCode.OK:              result = "OK"; break;
                case BaseStationStatusCode.PositionLost:    result = "PL"; break;
                case BaseStationStatusCode.Remove:          result = "RM"; break;
                case BaseStationStatusCode.SignalLost:      result = "SL"; break;
                default:                                    throw new NotImplementedException();
            }

            return result;
        }
    }
}
