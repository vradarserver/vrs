// Copyright © 2013 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Interface;
using VirtualRadar.Interface.StandingData;

namespace VirtualRadar.Library
{
    /// <summary>
    /// The default implementation of <see cref="ICallsignParser"/>
    /// </summary>
    public class CallsignParser : ICallsignParser
    {
        /// <summary>
        /// The standing data manager that we can use to fetch airline information.
        /// </summary>
        private IStandingDataManager _StandingDataManager;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="standingDataManager"></param>
        public CallsignParser(IStandingDataManager standingDataManager)
        {
            _StandingDataManager = standingDataManager;
        }

        /// <inheritdoc/>
        public IReadOnlyList<string> GetAllAlternateCallsigns(string callsign)
        {
            var result = new List<string>();

            if(!String.IsNullOrEmpty(callsign)) {
                result.Add(callsign);

                var parsed = new Callsign(callsign);
                if(parsed.IsOriginalCallsignValid) {
                    var processedCodes = new List<string>();
                    foreach(var airline in _StandingDataManager.FindAirlinesForCode(parsed.Code)) {
                        ProcessAlternateAirlineCode(callsign, result, parsed.TrimmedNumber, processedCodes, airline.IcaoCode);
                        ProcessAlternateAirlineCode(callsign, result, parsed.TrimmedNumber, processedCodes, airline.IataCode);
                    }
                }
            }

            return result;
        }

        private static void ProcessAlternateAirlineCode(string callsign, List<string> result, string number, List<string> processedCodes, string airlineCode)
        {
            if(!String.IsNullOrEmpty(airlineCode) && !processedCodes.Contains(airlineCode)) {
                processedCodes.Add(airlineCode);

                for(var padLength = 7 - (airlineCode.Length + number.Length);padLength >= 0;--padLength) {
                    var padding = new String('0', padLength);
                    var altCallsign = $"{airlineCode}{padding}{number}";
                    if(altCallsign != callsign) {
                        result.Add(altCallsign);
                    }
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="callsign"></param>
        /// <param name="operatorIcao"></param>
        /// <returns></returns>
        public IReadOnlyList<string> GetAllRouteCallsigns(string callsign, string operatorIcao)
        {
            var result = new List<string>();

            if(!String.IsNullOrEmpty(callsign)) {
                result.Add(callsign);

                if(callsign.Length >= 1) {
                    if(Char.IsDigit(callsign[0]) && (callsign.Length == 1 || Char.IsDigit(callsign[1]))) {
                        if(!String.IsNullOrEmpty(operatorIcao) && callsign.Length + operatorIcao.Length <= 8) {
                            result.Add($"{operatorIcao}{callsign}");
                        }
                    }
                }

                var parsed = new Callsign(callsign);
                if(parsed.IsOriginalCallsignValid) {
                    var code = parsed.Code;
                    var number = parsed.Number;

                    if(!String.IsNullOrEmpty(number)) {
                        if(String.IsNullOrEmpty(code) && !String.IsNullOrEmpty(operatorIcao)) {
                            code = operatorIcao;
                        }

                        var trimmedNumber = parsed.TrimmedNumber;
                        if(trimmedNumber != number) {
                            result.Add($"{code}{trimmedNumber}");
                        }

                        if(code.Length == 2) {
                            foreach(var airline in _StandingDataManager.FindAirlinesForCode(code).Where(r => !String.IsNullOrEmpty(r.IcaoCode)).OrderBy(r => r.IcaoCode)) {
                                result.Add($"{airline.IcaoCode}{number}");
                                if(number != trimmedNumber) {
                                    result.Add($"{airline.IcaoCode}{trimmedNumber}");
                                }
                            }
                        } else if(code.Length == 3) {
                            var isNumeric = number.Count(r => Char.IsDigit(r)) == number.Length;
                            if(isNumeric) {
                                var airlines = _StandingDataManager
                                    .FindAirlinesForCode(code)
                                    .Where(r => r.IcaoCode == code && !String.IsNullOrEmpty(r.IataCode))
                                    .ToArray();
                                if(airlines.Length == 1) {
                                    var icaoCodes = _StandingDataManager
                                        .FindAirlinesForCode(airlines[0].IataCode)
                                        .Where(r => !String.IsNullOrEmpty(r.IcaoCode) && r.IcaoCode != code)
                                        .OrderBy(r => r.IcaoCode)
                                        .Select(r => r.IcaoCode)
                                        .Distinct();
                                    foreach(var icaoCode in icaoCodes) {
                                        result.Add($"{icaoCode}{number}");
                                        if(number != trimmedNumber) {
                                            result.Add($"{icaoCode}{trimmedNumber}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}
