// Copyright © 2012 onwards, Andrew Whewell
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
using VirtualRadar.Interface.ModeS;
using VirtualRadar.Interface;

namespace VirtualRadar.Library.ModeS
{
    /// <summary>
    /// The default implementation of <see cref="IModeSParity"/>.
    /// </summary>
    class ModeSParity : IModeSParity
    {
        /// <summary>
        /// The object that's going to do all the CRC calculation work for us.
        /// </summary>
        private Crc32ModeS _CrcCalculator = new Crc32ModeS();

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void StripParity(byte[] bytes, int offset, int length)
        {
            if(bytes == null) throw new ArgumentNullException("bytes");
            if(offset < 0 || offset >= bytes.Length) throw new IndexOutOfRangeException();
            if(offset + length > bytes.Length) throw new IndexOutOfRangeException();

            if(length > 6) {
                byte[] parity = null;
                parity = _CrcCalculator.ComputeChecksumBytes(bytes, offset, length - 3, false);

                var overlayOffset = offset + length - 3;
                bytes[overlayOffset++] ^= parity[0];
                bytes[overlayOffset++] ^= parity[1];
                bytes[overlayOffset]   ^= parity[2];
            }
        }
    }
}
