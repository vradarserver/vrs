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
using VirtualRadar.Interface;

namespace VirtualRadar.Plugin.FeedFilter.Json
{
    /// <summary>
    /// The JSON that is sent to the site in response for requests to fetch or save the filter configuration.
    /// </summary>
    class FilterConfigurationJson : ResponseJson
    {
        /// <summary>
        /// Gets or sets a value that is incremented every time the settings are saved.
        /// </summary>
        public long DataVersion { get; set; }

        /// <summary>
        /// Gets or sets a newline separated list of aircraft ICAO codes.
        /// </summary>
        public string Icaos { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="Icaos"/> are ICAOs that are prohibited or the
        /// only ICAOs allowed.
        /// </summary>
        public bool ProhibitIcaos { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that MLAT positions must not be allowed through the filter.
        /// </summary>
        public bool ProhibitMlat { get; set; }

        /// <summary>
        /// Creates a JSON object from a <see cref="FilterConfiguration"/> object.
        /// </summary>
        /// <param name="filterConfiguration"></param>
        /// <returns></returns>
        public void FromFilterConfiguration(FilterConfiguration filterConfiguration)
        {
            DataVersion =   filterConfiguration.DataVersion;
            ProhibitMlat =  filterConfiguration.ProhibitMlat;
            ProhibitIcaos = filterConfiguration.ProhibitIcaos;

            var icaoLines = new List<string>(filterConfiguration.Icaos);
            icaoLines.AddRange(filterConfiguration.IcaoRanges.Select(r => r.ToString()));
            icaoLines.Sort((lhs, rhs) => String.Compare(lhs, rhs));
            Icaos = String.Join("\n", icaoLines.ToArray());
        }

        /// <summary>
        /// Creates a <see cref="FilterConfiguration"/> from the object.
        /// </summary>
        /// <param name="duplicateIcaos"></param>
        /// <param name="invalidIcaos"></param>
        /// <returns></returns>
        public FilterConfiguration ToFilterConfiguration(List<string> duplicateIcaos, List<string> invalidIcaos)
        {
            var result = new FilterConfiguration() {
                DataVersion =   DataVersion,
                ProhibitMlat =  ProhibitMlat,
                ProhibitIcaos = ProhibitIcaos,
            };
            ParseIcaos(Icaos, result.Icaos, result.IcaoRanges, duplicateIcaos, invalidIcaos);

            return result;
        }

        /// <summary>
        /// Parses a single string containing multiple ICAOs into a collection of prohibited ICAOs and prohibited ICAO ranges.
        /// </summary>
        /// <param name="rawIcaos"></param>
        /// <param name="icaos"></param>
        /// <param name="icaoRanges"></param>
        /// <param name="duplicateIcaos"></param>
        /// <param name="invalidIcaos"></param>
        /// <returns></returns>
        private void ParseIcaos(string rawIcaos, List<string> icaos, List<IcaoRange> icaoRanges, List<string> duplicateIcaos, List<string> invalidIcaos)
        {
            var singleIcaos = new HashSet<string>();
            var rangeIcaos = new HashSet<Pair<string>>();

            if(!String.IsNullOrEmpty(rawIcaos)) {
                foreach(var chunk in rawIcaos.Split(new char[] { ';', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)) {
                    var candidate = chunk.ToUpper().Trim();
                    var rangeSeparatorIndex = candidate.IndexOf('-');
                    if(rangeSeparatorIndex == -1) {
                        ParseSingleIcao(candidate, singleIcaos, duplicateIcaos, invalidIcaos);
                    } else {
                        ParseIcaoRange(candidate, rangeSeparatorIndex, rangeIcaos, duplicateIcaos, invalidIcaos);
                    }
                }
            }
            icaos.AddRange(singleIcaos);
            icaoRanges.AddRange(rangeIcaos.Select(r => new IcaoRange() { Start = r.First, End = r.Second }));

            icaos.Sort((lhs, rhs) => String.Compare(lhs, rhs));
            icaoRanges.Sort((lhs, rhs) => {
                var result = String.Compare(lhs.Start, rhs.Start);
                if(result == 0) {
                    result = String.Compare(lhs.End, rhs.End);
                }
                return result;
            });
            duplicateIcaos.Sort((lhs, rhs) => String.Compare(lhs, rhs));
            invalidIcaos.Sort((lhs, rhs) => String.Compare(lhs, rhs));
        }

        private void ParseSingleIcao(string candidate, HashSet<string> singleIcaos, List<string> duplicateIcaos, List<string> invalidIcaos)
        {
            var rawCandidate = candidate;
            candidate = PadCandidate(candidate);

            if(singleIcaos.Contains(candidate)) {
                AddToErrorList(rawCandidate, duplicateIcaos);
            } else if(CandidateIsInvalid(candidate)) {
                AddToErrorList(rawCandidate, invalidIcaos);
            } else {
                singleIcaos.Add(candidate);
            }
        }

        private void ParseIcaoRange(string rangeCandidate, int separatorIndex, HashSet<Pair<string>> rangeIcaos, List<string> duplicateIcaos, List<string> invalidIcaos)
        {
            var rawCandidate = rangeCandidate;

            var startCandidate = rangeCandidate.Substring(0, separatorIndex).Trim();
            var endCandidate = rangeCandidate.Substring(separatorIndex + 1).Trim();

            if(startCandidate.Length < 1 || endCandidate.Length < 1) {
                AddToErrorList(rawCandidate, invalidIcaos);
            } else {
                startCandidate = PadCandidate(startCandidate);
                endCandidate = PadCandidate(endCandidate);
                var pair = new Pair<string>(startCandidate, endCandidate);

                if(rangeIcaos.Contains(pair)) {
                    AddToErrorList(rawCandidate, duplicateIcaos);
                } else if(CandidateIsInvalid(startCandidate) || CandidateIsInvalid(endCandidate)) {
                    AddToErrorList(rawCandidate, invalidIcaos);
                } else if(ConvertToNumber(startCandidate) > ConvertToNumber(endCandidate)) {
                    AddToErrorList(rawCandidate, invalidIcaos);
                } else {
                    rangeIcaos.Add(pair);
                }
            }
        }

        private string PadCandidate(string candidate)
        {
            if(candidate.Length < 6) {
                candidate = String.Format("{0}{1}", new String('0', 6 - candidate.Length), candidate);
            }
            return candidate;
        }

        private bool CandidateIsInvalid(string candidate)
        {
            return candidate.Length > 6 || candidate.Any(r => !((r >= 'A' && r <= 'F') || (r >= '0' && r <= '9')));
        }

        private int ConvertToNumber(string candidate)
        {
            return CustomConvert.Icao24(candidate);
        }

        private void AddToErrorList(string candidate, List<string> errorList)
        {
            if(!errorList.Contains(candidate)) {
                errorList.Add(candidate);
            }
        }
    }
}
