// Copyright © 2015 onwards, Andrew Whewell
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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Performs simple custom conversions.
    /// </summary>
    public static class CustomConvert
    {
        /// <summary>
        /// Converts a value from one type to another, as per the standard <see cref="Convert.ChangeType(object, Type)"/>,
        /// except that this version can cope with the conversion of <see cref="Nullable{T}"/> types.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static object ChangeType(object value, Type type, IFormatProvider provider)
        {
            object result = null;

            if(!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Nullable<>)) {
                if(type.IsEnum) {
                    result = Enum.Parse(type, (string)Convert.ChangeType(value, typeof(string)));
                } else if(type == typeof(Guid)) {
                    var guidText = (string)Convert.ChangeType(value, typeof(string));
                    result = new Guid(guidText);
                } else if(type == typeof(IntPtr)) {
                    var intPtrText = (string)Convert.ChangeType(value, typeof(string));
                    var intPtrInt = int.Parse(intPtrText);
                    result = new IntPtr(intPtrInt);
                } else if(type == typeof(TimeSpan)) {
                    var timeSpanText = (string)Convert.ChangeType(value, typeof(string));
                    result = TimeSpan.Parse(timeSpanText);
                } else {
                    result = Convert.ChangeType(value, type, provider);
                }
            } else if(value != null) {
                var underlyingType = type.GetGenericArguments()[0];
                result = ChangeType(value, underlyingType, provider);
            }

            return result;
        }

        /// <summary>
        /// Converts an ICAO24 string to an ICAO24 value. Returns -1 if the ICAO24 is invalid.
        /// </summary>
        /// <param name="icao24"></param>
        /// <returns></returns>
        /// <remarks>
        /// In testing the standard Convert.ToInt32() conversion call takes about 67ms for a million
        /// valid 6 digit hex codes and 20 seconds for a million invalid hex codes (it throws exceptions
        /// which need to be caught). This version takes about 50ms for a million valid or invalid
        /// 6 digt hex codes.
        /// </remarks>
        public static int Icao24(string icao24) => ParseHexSignedInt(icao24, maxLength: 6);

        /// <summary>
        /// Exactly the same as <see cref="Icao24"/> except it will accept any length of
        /// integer instead of rejecting anything over 7 digits. Note that invalid strings
        /// always return -1 so the maximum parseable is 0xfffffffe.
        /// </summary>
        /// <param name="hexText"></param>
        /// <returns></returns>
        public static int HexInt(string hexText) => ParseHexSignedInt(hexText, maxLength: 8);

        private static int ParseHexSignedInt(string icao24, int maxLength)
        {
            var result = -1;

            if(icao24 != null && icao24.Length > 0 && icao24.Length <= maxLength) {
                result = 0;
                for(var i = 0;i < icao24.Length;++i) {
                    var digit = -1;
                    switch(icao24[i]) {
                        case '0':   digit = 0; break;
                        case '1':   digit = 1; break;
                        case '2':   digit = 2; break;
                        case '3':   digit = 3; break;
                        case '4':   digit = 4; break;
                        case '5':   digit = 5; break;
                        case '6':   digit = 6; break;
                        case '7':   digit = 7; break;
                        case '8':   digit = 8; break;
                        case '9':   digit = 9; break;
                        case 'a':
                        case 'A':   digit = 10; break;
                        case 'b':
                        case 'B':   digit = 11; break;
                        case 'c':
                        case 'C':   digit = 12; break;
                        case 'd':
                        case 'D':   digit = 13; break;
                        case 'e':
                        case 'E':   digit = 14; break;
                        case 'f':
                        case 'F':   digit = 15; break;
                    }
                    if(digit == -1) {
                        result = -1;
                        break;
                    }
                    result <<= 4;
                    result |= digit;
                }
            }

            return result;
        }

        public static double DistanceUnits(double distance, DistanceUnit fromUnit, DistanceUnit toUnit)
        {
            switch(fromUnit) {
                case DistanceUnit.Kilometres:
                    switch(toUnit) {
                        case DistanceUnit.Kilometres:               return distance;
                        case DistanceUnit.Miles:                    return distance * 0.621371192;
                        case DistanceUnit.NauticalMiles:            return distance * 0.539956803;
                        default:
                            throw new NotImplementedException();
                    }
                case DistanceUnit.Miles:
                    switch(toUnit) {
                        case DistanceUnit.Kilometres:               return distance * 1.609344;
                        case DistanceUnit.Miles:                    return distance;
                        case DistanceUnit.NauticalMiles:            return distance * 0.868976;
                        default:
                            throw new NotImplementedException();
                    }
                case DistanceUnit.NauticalMiles:
                    switch(toUnit) {
                        case DistanceUnit.Kilometres:               return distance * 1.852;
                        case DistanceUnit.Miles:                    return distance * 1.15078;
                        case DistanceUnit.NauticalMiles:            return distance;
                        default:
                            throw new NotImplementedException();
                    }
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
