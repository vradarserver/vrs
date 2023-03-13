// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Library.ModeS
{
    /// <summary>
    /// A static class that can decode Mode A squawk codes.
    /// </summary>
    static class ModeATranslator
    {
        /// <summary>
        /// Decodes a set of 13 bits into a squawk code.
        /// </summary>
        /// <param name="bits">The 13 Mode A bits in C1, A1, C2, A2, A4, zero, B1, D1, B2, D2, B4, D4 order.</param>
        /// <returns></returns>
        public static short DecodeModeA(short bits)
        {
            var a = ((bits & 0x0800) >> 11) + ((bits & 0x200) >> 8) + ((bits & 0x080) >> 5);
            var b = ((bits & 0x0020) >> 5)  + ((bits & 0x008) >> 2) + ((bits & 0x002) << 1);
            var c = ((bits & 0x1000) >> 12) + ((bits & 0x400) >> 9) + ((bits & 0x100) >> 6);
            var d = ((bits & 0x0010) >> 4)  + ((bits & 0x004) >> 1) + ((bits & 0x001) << 2);

            return (short)(
                  (a * 1000)
                + (b * 100)
                + (c * 10)
                + d
            );
        }
    }
}
