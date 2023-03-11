﻿// Copyright © 2023 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace Test.Framework
{
    public static class TestDataParser
    {
        /// <summary>
        /// Converts a value to a binary representation of a given length.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="countDigits"></param>
        /// <returns>The value, expressed as a sequence of binary digits, within a string of countDigits length.</returns>
        public static string ConvertToBitString(long value, int countDigits)
        {
            var bits = Convert.ToString(value, 2);
            if(bits.Length < countDigits) {
                bits = String.Format("{0}{1}", new String('0', countDigits - bits.Length), bits);  // must be a better way to do this :)
            }
            return bits;
        }

        /// <summary>
        /// Converts a value to a binary representation of a given length.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="countDigits"></param>
        /// <returns>The value, expressed as a sequence of binary digits, within a string of countDigits length.</returns>
        public static string ConvertToBitString(int value, int countDigits) => ConvertToBitString((long)value, countDigits);

        /// <summary>
        /// Converts a string of binary digits (with optional whitespace) to a collection of bytes.
        /// </summary>
        /// <param name="bits"></param>
        /// <returns></returns>
        public static List<byte> ConvertBitStringToBytes(string bits)
        {
            bits = bits
                .Trim()
                .Replace(" ", "");
            if(bits.Length % 8 != 0) {
                Assert.Fail("The number of bits must be divisible by 8");
            }

            var result = new List<byte>();
            for(var idx = 0;idx < bits.Length;idx += 8) {
                result.Add(Convert.ToByte(bits.Substring(idx, 8), 2));
            }

            return result;
        }
    }
}
