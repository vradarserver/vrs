// Copyright © 2022 onwards, Andrew Whewell
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

namespace VirtualRadar.Interface.ModeS
{
    /// <summary>
    /// Converts between a Mode-S code and a registration. Only a handful of
    /// countries use an algorithm to convert between registration and Mode-S
    /// ICAO code, and even fewer publish the algorithm, so this cannot be
    /// used to figure out registrations for all aircraft.
    /// </summary>
    public static class RegistrationConverter
    {
        /// <summary>
        /// Returns null if the registration cannot be converted
        /// to an ICAO, otherwise returns the ICAO formatted as
        /// a six digit hex string.
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        public static string RegistrationToModeS(string registration)
        {
            string result = null;

            if(registration?.Length > 1 && registration[0] == 'N') {
                result = IcaoFromUSRegistration(registration);
            }

            return result;
        }

        /// <summary>
        /// Returns the registration decoded from the ICAO or null
        /// if the ICAO is not associated with a civilian aviation registry
        /// that imposes a calculatable relationship between ICAO and
        /// registration.
        /// </summary>
        /// <param name="modeSIcao"></param>
        /// <returns></returns>
        public static string ModeSToRegistration(string modeSIcao)
        {
            var icao = CustomConvert.Icao24(modeSIcao);

            var result = new StringBuilder("N");

            icao -= 0xa00000;

            for(var numIdx = 0;icao > 0 && numIdx < 6;++numIdx) {
                switch(numIdx) {
                    case 0:
                        var zeroMod = icao / 101711;
                        icao -= (zeroMod * 101711) + 1;
                        result.Append(ATo9Base34(25 + zeroMod + 1));
                        break;
                    default:
                        if(IsAsciiCharBetween(result[numIdx], 'A', 'Z')) {
                            if(icao < 35) {
                                result.Append(ATo9Base34(icao));
                                icao = 0;
                            }
                        } else {
                            if(icao > 600) {
                                var mod = icao / 601;
                                icao -= mod * 601;
                                result.Append(ATo9Base34(mod + 24));
                            } else {
                                --icao;
                                var mod = icao / 25;
                                icao -= mod * 25;
                                result.Append(ATo9Base34(mod + 1));
                            }
                        }
                        break;
                }
            }

            return result.ToString();
        }

        private static string IcaoFromUSRegistration(string registration)
        {
            var icao = 0;
            var thisCharacterFollowsAlphaCharacter = false;
            for(var idx = 1;idx < registration.Length;++idx) {
                var ch = registration[idx];
                var digit = Base34ATo9(ch);

                var base24Portion = digit - 1;
                var base34Portion = 0;
                var bigBaseMultiplier = 0;

                switch(idx - 1) {
                    case 0:
                        icao = (digit - 26 /* '1' */) * 101711;
                        icao += 1;
                        break;
                    case 1:
                        if(digit > 25 /* '0' */) {
                            bigBaseMultiplier = 10076;      // 11 * 916
                        }
                        goto default;
                    case 2:
                        if(digit > 25) {
                            bigBaseMultiplier = 916;        // A-Z + A-Z = (24 * 24 = 576), plus 0-9 + A-Z = (10 * 34 = 340)
                        }
                        goto default;
                    case 3:
                        if(digit > 25) {
                            base34Portion = digit - 25;
                        }
                        goto default;
                    default:
                        icao += digit;
                        if(bigBaseMultiplier > 0) {
                            base34Portion = digit - 25;
                            icao += base34Portion * bigBaseMultiplier;
                        }
                        if(base34Portion > 0) {
                            base24Portion = 24;
                        }
                        if(!thisCharacterFollowsAlphaCharacter && idx != 5) {
                            icao += 24 * base24Portion;
                            icao += 34 * base34Portion;
                        }
                        thisCharacterFollowsAlphaCharacter = digit < 25;
                        break;
                }
            }

            return (icao + (0xA00000)).ToString("X6");
        }

        static int Base34ATo9(char ch)
        {
            // Values are:
            // A = 1
            // B = 2 (etc.)
            // H = 8
            // (skip I)
            // J = 9 (etc.)
            // N = 13
            // ... (skip O)
            // P = 14 (etc.)
            // Z = 24
            // 0 = 25 (etc.)
            // 9 = 34
            //
            // Unknown characters return 0, charset is always ASCII

            return (ch >= '0' && ch <= '9')
                ? (ch - '0') + 25
                : (ch < 'A' || ch > 'Z')
                    ? 0
                    : (ch - 'A') + (ch < 'I' ? 1 : ch < 'O' ? 0 : -1);
        }

        // See Base34ATo9's comment
        static char ATo9Base34(int oneBasedValue) => "ABCDEFGHJKLMNPQRSTUVWXYZ0123456789"[oneBasedValue - 1];

        private static bool IsAsciiCharBetween(char character, char lowerInclusive, char upperInclusive) => character >= lowerInclusive && character <= upperInclusive;
    }
}
