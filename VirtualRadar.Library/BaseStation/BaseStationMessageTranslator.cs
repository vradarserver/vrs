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
using VirtualRadar.Interface.BaseStation;
using System.Globalization;
using System.Diagnostics;
using VirtualRadar.Interface;

namespace VirtualRadar.Library.BaseStation
{
    /// <summary>
    /// Default implementation of <see cref="IBaseStationMessageTranslator"/>.
    /// </summary>
    class BaseStationMessageTranslator : IBaseStationMessageTranslator
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="signalLevel"></param>
        /// <returns></returns>
        public BaseStationMessage Translate(string text, int? signalLevel)
        {
            var result = new BaseStationMessage() {
                SignalLevel = signalLevel
            };

            if(!String.IsNullOrEmpty(text)) {
                var isMlat = false;
                var parts = text.Split(',');
                for(var c = 0;c < parts.Length;c++) {
                    var chunk = parts[c];
                    try {
                        if(!String.IsNullOrEmpty(chunk)) {
                            switch(c) {
                                case 0:     result.MessageType = BaseStationMessageHelper.ConvertToBaseStationMessageType(chunk, ref isMlat); break;
                                case 1:     result.TransmissionType = result.MessageType == BaseStationMessageType.Transmission ? BaseStationMessageHelper.ConvertToBaseStationTransmissionType(chunk) : BaseStationTransmissionType.None; break;
                                case 2:     result.SessionId = ParseInt(chunk); break;
                                case 3:     result.AircraftId = ParseInt(chunk); break;
                                case 4:     result.Icao24 = chunk; break;
                                case 5:     result.FlightId = ParseInt(chunk); break;
                                case 6:     result.MessageGenerated = ParseDate(chunk); break;
                                case 7:     result.MessageGenerated = ParseTime(result.MessageGenerated, chunk); break;
                                case 8:     result.MessageLogged = ParseDate(chunk); break;
                                case 9:     result.MessageLogged = ParseTime(result.MessageLogged, chunk); break;
                                case 10:    if(result.MessageType == BaseStationMessageType.StatusChange) result.StatusCode = BaseStationMessageHelper.ConvertToBaseStationStatusCode(chunk); else result.Callsign = chunk.Replace("@", "").Trim(); break;
                                case 11:    result.Altitude = ParseInt(chunk); break;
                                case 12:    result.GroundSpeed = ParseFloat(chunk); break;
                                case 13:    result.Track = ParseFloat(chunk); break;
                                case 14:    result.Latitude = ParseDouble(chunk); break;
                                case 15:    result.Longitude = ParseDouble(chunk); break;
                                case 16:    result.VerticalRate = ParseInt(chunk); break;
                                case 17:    result.Squawk = ParseInt(chunk); if(result.Squawk == 0) result.Squawk = null; break;
                                case 18:    result.SquawkHasChanged = ParseBool(chunk); break;
                                case 19:    result.Emergency = ParseBool(chunk); break;
                                case 20:    result.IdentActive = ParseBool(chunk); break;
                                case 21:    result.OnGround = ParseBool(chunk); break;
                            }
                        }
                    } catch(Exception ex) {
                        Debug.WriteLine(String.Format("BaseStationMessageTranslator.Translate caught exception: {0}", ex.ToString()));

                        // I would prefer to pass ex as the inner exception to this. However Microsoft's Application.ThreadException unhandled exception handler
                        // strips off all outer exceptions and only shows the bottom-most exception - i.e., in our case, the exception from a Parse method. This
                        // is not useful in isolation, we need to know what was being translated, the context in which the exception was thrown. So I have ended
                        // up with this, which is not very nice but shows enough information in the unhandled exception handler to allow diagnosis of the problem.
                        throw new BaseStationTranslatorException($"{ex.Message} while translating \"{chunk}\" (chunk {c}) in \"{text}\"");
                    }
                }

                var icaoNumber = CustomConvert.Icao24(result.Icao24);
                if(icaoNumber > -1) {
                    result.Icao24 = icaoNumber.ToString("X6");
                }

                result.IsMlat = isMlat;
            }

            return result;
        }

        private int ParseInt(string chunk)          { return int.Parse(chunk, CultureInfo.InvariantCulture); }

        private float ParseFloat(string chunk)      { return float.Parse(chunk, CultureInfo.InvariantCulture); }

        private double ParseDouble(string chunk)    { return double.Parse(chunk, CultureInfo.InvariantCulture); }

        private bool ParseBool(string chunk)        { return chunk == "0" ? false : true; }

        // Note that locale settings can play havoc with delimiters sent from BaseStation. I've given up using DateTime.Parse and
        // I'm just plucking out the numbers by hand...
        private DateTime ParseDate(string chunk)
        {
            if(chunk.Length != 10) throw new InvalidOperationException($"{chunk} doesn't look like a valid date");
            int year = int.Parse(chunk.Substring(0, 4));
            int month = int.Parse(chunk.Substring(5,2));
            int day = int.Parse(chunk.Substring(8, 2));

            return new DateTime(year, month, day);
        }

        // See notes against ParseDate for explanation of parser
        private DateTime ParseTime(DateTime date, string chunk)
        {
            if(chunk.Length != 12) throw new InvalidOperationException($"{chunk} doesn't look like a valid time");
            int hour = int.Parse(chunk.Substring(0, 2));
            int minute = int.Parse(chunk.Substring(3, 2));
            int second = int.Parse(chunk.Substring(6, 2));
            int millisecond = int.Parse(chunk.Substring(9, 3));

            return new DateTime(date.Year, date.Month, date.Day, hour, minute, second, millisecond);
        }
    }
}
