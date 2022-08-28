// Copyright © 2022 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Text.RegularExpressions;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Holds information about an aircraft registration prefix.
    /// </summary>
    public class RegistrationPrefixDetail
    {
        /// <summary>
        /// Gets the prefix code, e.g. G for UK registrations.
        /// </summary>
        public string Prefix { get; internal set; }

        /// <summary>
        /// Gets the ISO2 code of the country that the prefix is for, e.g GB
        /// for UK prefixes.
        /// </summary>
        public string CountryISO2 { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the registration is formatted with
        /// a dash, e.g. true for GB and false for US.
        /// </summary>
        public bool HasHyphen { get; internal set; }

        /// <summary>
        /// Gets the text of a regular expression that will extract the code
        /// portion of the full registration (including dash, if appropriate)
        /// into a group called code.
        /// </summary>
        public string DecodeFullRegexText { get; internal set; }

        /// <summary>
        /// The compiled regular expression based on <see cref="DecodeFullRegexText"/>
        /// but only good for strings that are *just* the registration.
        /// </summary>
        public Regex DecodeFullRegex { get; internal set; }

        /// <summary>
        /// Gets the text of a regular expression that will extract the code
        /// portion of the full registration (without the dash) into a group
        /// called code.
        /// </summary>
        public string DecodeNoHyphenRegexText { get; internal set; }

        /// <summary>
        /// The compiled regular expression based on <see cref="DecodeNoHyphenRegexText"/>
        /// but only good for strings that are *just* the registration.
        /// </summary>
        public Regex DecodeNoHyphenRegex { get; internal set; }

        /// <summary>
        /// A string that will return the correctly formatted registration when the aircraft
        /// code is substituted in place of the string '{code}'.
        /// </summary>
        public string FormatTemplate { get; internal set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="countryISO2"></param>
        /// <param name="hasHyphen"></param>
        /// <param name="decodeFullRegex"></param>
        /// <param name="decodeNoHyphenRegex"></param>
        /// <param name="formatTemplate"></param>
        public RegistrationPrefixDetail(
            string prefix,
            string countryISO2,
            bool hasHyphen,
            string decodeFullRegex,
            string decodeNoHyphenRegex,
            string formatTemplate
        )
        {
            Prefix =                    prefix;
            CountryISO2 =               countryISO2;
            HasHyphen =                 hasHyphen;
            DecodeFullRegexText =       decodeFullRegex;
            DecodeNoHyphenRegexText =   decodeNoHyphenRegex;
            FormatTemplate =            formatTemplate;

            DecodeFullRegex =       new Regex($"^{DecodeFullRegexText}$", RegexOptions.Compiled);
            DecodeNoHyphenRegex =   new Regex($"^{DecodeNoHyphenRegexText}$", RegexOptions.Compiled);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{Prefix}-{CountryISO2}-{DecodeFullRegex}";

        /// <summary>
        /// Returns a fully formed registration from <see cref="FormatTemplate"/> and the
        /// aircraft code passed across.
        /// </summary>
        /// <param name="aircraftCode"></param>
        /// <returns></returns>
        public string FormatCode(string aircraftCode) => FormatTemplate.Replace("{code}", aircraftCode);
    }
}
