// Copyright © 2020 onwards, Andrew Whewell
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
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Interface.StateHistory
{
    /// <summary>
    /// A static class that can be used to create and work with SHA-1 fingerprints.
    /// </summary>
    public static class Sha1Fingerprint
    {
        /// <summary>
        /// Creates an SHA1 fingerprint from a string.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static byte[] CreateFingerprintFromText(string text)
        {
            byte[] result = null;

            if(text == "") {
                result = new byte[0];
            } else if(text != null) {
                using(var hasher = SHA1.Create()) {
                    using(var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(text))) {
                        result = hasher.ComputeHash(memoryStream);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a fingerprint from a list of objects.
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        /// <remarks>
        /// The parameterless ToString() is called for each object, different regions might resolve
        /// to different fingerprints for the same set as a result.
        /// </remarks>
        public static byte[] CreateFingerprintFromObjects(params object[] objects)
        {
            byte[] result = null;

            if(objects != null) {
                var buffer = new StringBuilder();
                foreach(var obj in objects) {
                    if(obj == null) {
                        buffer.Append('\0');
                    } else {
                        buffer.Append(obj.ToString());
                    }
                    buffer.Append('\n');
                }

                result = CreateFingerprintFromText(buffer.ToString());
            }

            return result;
        }

        /// <summary>
        /// Converts a fingerprint to a string.
        /// </summary>
        /// <param name="fingerprint"></param>
        /// <returns></returns>
        public static string ConvertToString(byte[] fingerprint)
        {
            var buffer = new StringBuilder(fingerprint?.Length >= 1 ? fingerprint.Length * 2 : 2);
            for(var i = 0;i < fingerprint?.Length;++i) {
                buffer.Append(fingerprint[i].ToString("x2"));
            }

            return fingerprint == null ? null : buffer.ToString();
        }

        /// <summary>
        /// Converts a fingerprint hex string into a byte array fingerprint.
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] ConvertFromString(string hexString)
        {
            var result = hexString == null
                ? null
                : hexString.Length == 0
                    ? new byte[0]
                    : hexString.Length == 40
                        ? new byte[20]
                        : throw new ArgumentOutOfRangeException($"{nameof(hexString)} must either be null, empty or 40 characters long");

            if(result?.Length == 20) {
                var resultIdx = 0;
                int hexDigit(char ch)
                {
                    if(ch >= '0' && ch <= '9') return ch - '0';
                    if(ch >= 'a' && ch <= 'f') return 10 + (ch - 'a');
                    if(ch >= 'A' && ch <= 'F') return 10 + (ch - 'A');
                    throw new InvalidOperationException($"{ch} is not a valid hex character");
                };
                for(var stringIdx = 0;stringIdx < hexString.Length;stringIdx += 2, resultIdx += 1) {
                    result[resultIdx] = (byte)((hexDigit(hexString[stringIdx]) << 4) | hexDigit(hexString[stringIdx + 1]));
                }
            }

            return result;
        }
    }
}
