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
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace VirtualRadar.Plugin.Vatsim
{
    /// <summary>
    /// Decodes a Remarks field on a pilot record.
    /// </summary>
    /// <remarks>
    /// As of time of writing the link to the specification on VATSIM's github wiki page is broken
    /// so I'm guessing at the format here :) Don't @ me.
    /// </remarks>
    class VatsimRemarks
    {
        private static readonly Regex _KeyValuePairRegex = new Regex(@"(?<key>[A-Z/]+)/(?<value>(.(?![A-Z/]+/))*)", RegexOptions.Compiled);

        private static readonly Regex _WordRegex = new Regex(@"(?<word>\b[A-Z0-9]+\b)", RegexOptions.Compiled);

        /// <summary>
        /// Gets the full remarks.
        /// </summary>
        public string Remarks { get; }

        /// <summary>
        /// Gets a collection of key-value properties extracted from <see cref="Remarks"/>.
        /// </summary>
        public NameValueCollection Properties { get; } = new NameValueCollection();

        /// <summary>
        /// Gets the registration as extracted from the <see cref="Remarks"/>. Will be an
        /// empty string if missing or invalid, will never be null.
        /// </summary>
        public string Registration => OnlyWordOf("REG", maxLength: 10);

        /// <summary>
        /// Gets the operator ICAO as extracted from the <see cref="Remarks"/>. Will be an
        /// empty string if missing or invalid, will never be null.
        /// </summary>
        public string OperatorIcao => OnlyWordOf("OPR", maxLength: 3);

        /// <summary>
        /// Gets the Mode-S ICAO24 as extracted from the <see cref="Remarks"/>. Will be an
        /// empty string if missing or invalid, will never be null.
        /// </summary>
        public string ModeSCode => OnlyWordOf("CODE", maxLength: 6);

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="remarks">The remarks to parse into <see cref="Properties"/>.</param>
        public VatsimRemarks(string remarks)
        {
            Remarks = remarks;
            DecodeProperties();
        }

        private void DecodeProperties()
        {
            var normalisedRemarks = (Remarks ?? "").Trim();
            if(normalisedRemarks != "") {
                // OK - no documentation, all guesswork. Looks like it's free-form uppercase ASCII
                // but there is a convention that allows for optional key-value pairs to be encoded
                // into the string.
                //
                // The key and value are separated by a slash. The value and following key are
                // separated by a space. Neither key nor value have a fixed length. The value can
                // contain spaces.
                //
                // "/V/" is often seen as a terminator, but I've also seen key-values following it.
                // I don't know if it should be translated as "/" = "V" and "/" = "" or "/V" = "".
                // Latter seems more logical and avoids two values with the same key.
                //
                // Occasionally there is free-form text with the /V/ terminator, e.g.:
                //   "VERY NEW PILOT /V/"

                // We're only intrested in the key-value pairs for the properties collection
                foreach(Match match in _KeyValuePairRegex.Matches(normalisedRemarks)) {
                    var key = match.Groups["key"].Value;
                    var value = match.Groups["value"].Value;

                    if(!String.IsNullOrEmpty(key)) {
                        Properties.Add(key, value?.Trim());
                    }
                }
            }
        }

        /// <summary>
        /// See base class.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Remarks;

        /// <summary>
        /// Returns the <see cref="Properties"/> key passed across as long as (1) it
        /// is an entire word and (2) it is not longer than <paramref name="maxLength"/>.
        /// If both conditions are not met then an empty string is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public string OnlyWordOf(string key, int maxLength = int.MaxValue)
        {
            var result = (Properties[key] ?? "").Trim();

            if(result != "") {
                if(result.Length > maxLength) {
                    result = "";
                } else {
                    var match = _WordRegex.Match(result);
                    if(!match.Success) {
                        result = "";
                    } else {
                        if(match.Groups["word"].Value.Length != result.Length) {
                            result = "";
                        }
                    }
                }
            }

            return result;
        }
    }
}
