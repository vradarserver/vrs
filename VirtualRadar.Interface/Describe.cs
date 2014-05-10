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
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Localisation;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// A static class that can produce descriptions of objects that implement common interfaces.
    /// </summary>
    public static class Describe
    {
        /// <summary>
        /// Returns a standard description of an airport.
        /// </summary>
        /// <param name="airport"></param>
        /// <param name="preferIataAirportCode"></param>
        /// <param name="showCode"></param>
        /// <param name="showName"></param>
        /// <param name="showCountry"></param>
        /// <returns></returns>
        public static string Airport(Airport airport, bool preferIataAirportCode, bool showCode = true, bool showName = true, bool showCountry = true)
        {
            StringBuilder result = new StringBuilder();

            if(airport != null) {
                if(showCode) {
                    var firstChoice = preferIataAirportCode ? airport.IataCode : airport.IcaoCode;
                    var secondChoice = preferIataAirportCode ? airport.IcaoCode : airport.IataCode;
                    if(!String.IsNullOrEmpty(firstChoice))          result.Append(firstChoice);
                    else if(!String.IsNullOrEmpty(secondChoice))    result.Append(secondChoice);
                }
                if(showName && !String.IsNullOrEmpty(airport.Name))         AddSeparator(result, " ").Append(airport.Name);
                if(showCountry && !String.IsNullOrEmpty(airport.Country))   AddSeparator(result, ", ").Append(airport.Country);
            }

            return result.ToString();
        }

        /// <summary>
        /// Returns a single airport name that is a concatenation of the official airport name and location / city name.
        /// </summary>
        /// <param name="airportName"></param>
        /// <param name="cityName"></param>
        /// <returns></returns>
        public static string AirportName(string airportName, string cityName)
        {
            StringBuilder result = new StringBuilder(airportName ?? "");
            if(!String.IsNullOrEmpty(cityName)) {
                if(result.Length == 0 || (
                       airportName != cityName &&
                       !airportName.StartsWith(String.Format("{0} ", cityName)) &&
                       airportName.IndexOf(String.Format(" {0}", cityName)) == -1
                    )
                ) {
                    if(result.Length > 0) result.Append(", ");
                    result.Append(cityName);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Returns a number of bytes formatted as 'n B', 'n KB', 'n MB' etc.
        /// </summary>
        /// <param name="fullSize"></param>
        /// <returns></returns>
        public static string Bytes(long fullSize)
        {
            string result = "";

            double size = fullSize;
            if(fullSize < 1024) result = String.Format("{0:N0} {1}", size, Strings.AcronymByte);
            else if(fullSize < 0x100000L) result = String.Format("{0:N2} {1}", size / 1024.0, Strings.AcronymKilobyte);
            else if(fullSize < 0x40000000L) result = String.Format("{0:N2} {1}", size / 1048576.0, Strings.AcronymMegabyte);
            else if(fullSize < 0x10000000000L) result = String.Format("{0:N2} {1}", size / 1073741824.0, Strings.AcronymGigabyte);
            else result = String.Format("{0:N2} {1}", size / 1099511627776.0, Strings.AcronymTerrabyte);

            return result;
        }

        /// <summary>
        /// Returns the timespan formatted as a number of hours, minutes and seconds.
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static string TimeSpan(TimeSpan timeSpan)
        {
            return String.Format("{0:00}:{1:00}:{2:00}", (long)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);
        }

        /// <summary>
        /// Appends the separator to the string if the string isn't empty. Returns the string so that you can chain it.
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        private static StringBuilder AddSeparator(StringBuilder stringBuilder, string separator)
        {
            if(stringBuilder.Length > 0) stringBuilder.Append(separator);

            return stringBuilder;
        }

        /// <summary>
        /// Returns the registration passed across with characters that contravene the ICAO registration rules stripped out or transformed to upper-case as appropriate.
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        public static string IcaoCompliantRegistration(string registration)
        {
            string result = null;

            if(registration != null) {
                var buffer = new StringBuilder();
                foreach(var ch in registration) {
                    if(Char.IsDigit(ch) || (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || ch == '-') buffer.Append(Char.ToUpper(ch));
                }

                result = buffer.ToString();
            }

            return result;
        }

        /// <summary>
        /// Returns a description of the collection of <see cref="RebroadcastSettings"/> passed across.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static string RebroadcastSettingsCollection(List<RebroadcastSettings> settings)
        {
            var result = "";

            if(settings == null || settings.Count == 0) result = Strings.RebroadcastServersNoneConfigured;
            else {
                var enabledCount = settings.Count(r => r.Enabled);
                if(settings.Count == 1) result = String.Format(Strings.RebroadcastServersDescribeSingle, enabledCount);
                else                    result = String.Format(Strings.RebroadcastServersDescribeMany, settings.Count, enabledCount);
            }

            return result;
        }
    }
}
