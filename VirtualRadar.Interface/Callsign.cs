// Copyright © 2018 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Text.RegularExpressions;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// A class that helps deal with splitting callsigns into their constituent parts.
    /// </summary>
    public class Callsign
    {
        /// <summary>
        /// The regex that can split a callsign into code and number portions.
        /// </summary>
        public static readonly Regex CallsignRegex = new Regex(@"^(?<code>[A-Z]{2,3}|[A-Z][0-9]|[0-9][A-Z])(?<number>\d[A-Z0-9]*)");

        /// <summary>
        /// Gets or sets the full callsign as originally supplied.
        /// </summary>
        public string OriginalCallsign { get; }

        /// <summary>
        /// Gets or sets the code portion of the callsign.
        /// </summary>
        public string Code { get; } = "";

        /// <summary>
        /// Gets or sets the flight number portion of the callsign.
        /// </summary>
        public string Number { get; } = "";

        /// <summary>
        /// Gets the <see cref="Number"/> with leading zeros trimmed off. If the number consists
        /// solely of zeros then one zero is left behind.
        /// </summary>
        public string TrimmedNumber { get; } = "";

        /// <summary>
        /// Gets the callsign with a <see cref="TrimmedNumber"/> in place of the <see cref="Number"/>.
        /// </summary>
        public string TrimmedCallsign { get; } = "";

        /// <summary>
        /// Gets or sets a value indicating that the original callsign was valid.
        /// </summary>
        public bool IsOriginalCallsignValid { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Callsign() : this(null)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="callsign"></param>
        public Callsign(string callsign)
        {
            OriginalCallsign = callsign?.ToUpperInvariant().Trim();

            if(!String.IsNullOrEmpty(callsign)) {
                var match = CallsignRegex.Match(callsign);
                if(match.Success) {
                    Code = match.Groups["code"].Value;
                    Number = match.Groups["number"].Value;
                    TrimmedNumber = TrimNumber(Number);
                    IsOriginalCallsignValid = IsValid(Number, TrimmedNumber);
                    TrimmedCallsign = IsOriginalCallsignValid
                        ? $"{Code}{TrimmedNumber}"
                        : "";
                }
            }
        }

        private static string TrimNumber(string number)
        {
            number ??= "";
            var startIdx = 0;
            for(;startIdx < number.Length && number[startIdx] == '0';++startIdx) {
                ;
            }
            if(startIdx > 0 && (startIdx == number.Length || !Char.IsDigit(number[startIdx]))) {
                --startIdx;
            }

            return startIdx == 0
                ? number
                : number.Substring(startIdx);
        }

        private static bool IsValid(string rawNumber, string trimmedNumber)
        {
            var result = rawNumber?.Length > 0 && trimmedNumber.Length <= 4;
            if(result) {
                var bit = 1;
                var alphaBits = 0;
                for(var idx = trimmedNumber.Length -1;idx >= 0;--idx, bit <<= 1) {
                    var ch = trimmedNumber[idx];
                    if(ch < '0' || ch > '9') {
                        alphaBits += bit;
                    }
                }

                // If "n" is a digit and "A" is a letter then the alphaBits value will
                // be:
                // nnnn = 0  <-- valid
                // nnnA = 1  <-- valid
                // nnA0 = 2  <-- invalid
                // nnAA = 3  <-- valid
                // and so on. The only valid values are 0, 1 and 3, you can only have
                // up to two alphabetical characters and they need to be the last two.
                result = alphaBits < 2 || alphaBits == 3;
            }

            return result;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => OriginalCallsign ?? "";
    }
}
