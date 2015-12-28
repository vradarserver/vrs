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
            Icaos =         String.Join("\n", filterConfiguration.Icaos.ToArray());
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
            result.Icaos.AddRange(ParseIcaos(Icaos, duplicateIcaos, invalidIcaos));

            return result;
        }

        /// <summary>
        /// Parses a single string containing multiple ICAOs into a collection of prohibited ICAOs.
        /// </summary>
        /// <param name="rawIcaos"></param>
        /// <param name="duplicateIcaos"></param>
        /// <param name="invalidIcaos"></param>
        /// <returns></returns>
        private string[] ParseIcaos(string rawIcaos, List<string> duplicateIcaos, List<string> invalidIcaos)
        {
            var result = new HashSet<string>();

            if(!String.IsNullOrEmpty(rawIcaos)) {
                foreach(var chunk in rawIcaos.Split(new char[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)) {
                    var candidate = chunk.ToUpper().Trim();
                    if(candidate.Length < 6) candidate = String.Format("{0}{1}", new String('0', 6 - candidate.Length), candidate);
                    if(result.Contains(candidate)) {
                        if(!duplicateIcaos.Contains(candidate)) duplicateIcaos.Add(candidate);
                    } else if(candidate.Length > 6 || candidate.Any(r => !((r >= 'A' && r <= 'F') || (r >= '0' && r <= '9')))) {
                        if(!invalidIcaos.Contains(candidate)) invalidIcaos.Add(candidate);
                    } else {
                        result.Add(candidate);
                    }
                }
            }
            duplicateIcaos.Sort((lhs, rhs) => String.Compare(lhs, rhs));
            invalidIcaos.Sort((lhs, rhs) => String.Compare(lhs, rhs));

            return result.OrderBy(r => r).ToArray();
        }
    }
}
