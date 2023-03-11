// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Text;

namespace VirtualRadar.Interface.ModeS
{
    /// <summary>
    /// A static class that can extract characters encoded as per the ICAO specs from a bit stream.
    /// </summary>
    public static class ModeSCharacterTranslator
    {
        /// <summary>
        /// Returns a string of count length containing characters extracted from the bit stream.
        /// </summary>
        /// <param name="bitStream"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string ExtractCharacters(BitStream bitStream, int count)
        {
            var result = new StringBuilder();

            for(var i = 0;i < count;++i) {
                var ch = '\0';
                var encodedCh = bitStream.ReadByte(6);
                if(encodedCh > 0 && encodedCh < 27) {
                    ch = (char)('A' + (encodedCh - 1));
                } else if(encodedCh == 32) {
                    ch = ' ';
                } else if(encodedCh > 47 && encodedCh < 58) {
                    ch = (char)('0' + (encodedCh - 48));
                }
                if(ch != '\0') {
                    result.Append(ch);
                }
            }

            return result.ToString();
        }
    }
}
