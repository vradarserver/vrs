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
using VirtualRadar.Interface.Listener;
using Moq;

namespace Test.VirtualRadar.Library.Listener
{
    /// <summary>
    /// A mock message bytes extractor.
    /// </summary>
    class MockMessageBytesExtractor : Mock<IMessageBytesExtractor>
    {
        /// <summary>
        /// A list of every byte array sent to the extractor for message extraction.
        /// </summary>
        public List<byte[]> SourceByteArrays { get; private set; }

        /// <summary>
        /// The list of extracted bytes that are returned to the caller of ExtractMessageBytes.
        /// </summary>
        public List<ExtractedBytes> ExtractedBytes { get; private set; }

        /// <summary>
        /// Gets or sets a flag indicating that <see cref="SourceByteArrays"/> should be filled.
        /// </summary>
        public bool RecordSourceByteArrays { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public MockMessageBytesExtractor()
        {
            SourceByteArrays = new List<byte[]>();
            ExtractedBytes = new List<ExtractedBytes>();
            RecordSourceByteArrays = true;

            Setup(r => r.ExtractMessageBytes(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                 .Callback((byte[] bytes, int offset, int length) => {
                    ExtractMessageBytesCallback(bytes, offset, length);
                 })
                 .Returns(CreateExtractMessageBytesReturnValue);
        }

        /// <summary>
        /// Called on each invocation of ExtractMessageBytes.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        private void ExtractMessageBytesCallback(byte[] bytes, int offset, int length)
        {
            if(RecordSourceByteArrays) {
                var sourceByteArray = new byte[length];
                Array.Copy(bytes, offset, sourceByteArray, 0, length);
                SourceByteArrays.Add(sourceByteArray);
            }
        }

        /// <summary>
        /// Provides the return value for ExtractMessageBytes.
        /// </summary>
        /// <returns></returns>
        private List<ExtractedBytes> CreateExtractMessageBytesReturnValue()
        {
            return ExtractedBytes;
        }

        /// <summary>
        /// Adds an <see cref="ExtractedBytes"/> with a simple payload and the supplied format.
        /// </summary>
        /// <param name="format"></param>
        public ExtractedBytes AddExtractedBytes(ExtractedBytesFormat format)
        {
            return AddExtractedBytes(format, new byte[] { 0x31 }, 0, 1, false, false, null);
        }

        /// <summary>
        /// Adds an <see cref="ExtractedBytes"/> with a simple payload of the supplied length and format.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="length"></param>
        public ExtractedBytes AddExtractedBytes(ExtractedBytesFormat format, int length)
        {
            var bytes = new byte[length];
            return AddExtractedBytes(format, bytes, 0, length, false, false, null);
        }

        /// <summary>
        /// Adds an <see cref="ExtractedBytes"/> with a simple payload of the supplied length and format.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="length"></param>
        /// <param name="checksumFailed"></param>
        /// <param name="hasParity"></param>
        public ExtractedBytes AddExtractedBytes(ExtractedBytesFormat format, int length, bool checksumFailed, bool hasParity)
        {
            var bytes = new byte[length];
            return AddExtractedBytes(format, bytes, 0, length, checksumFailed, hasParity, null);
        }

        /// <summary>
        /// Adds an <see cref="ExtractedBytes"/> with a given payload expressed as an ASCII string.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="asciiPayload"></param>
        public ExtractedBytes AddExtractedBytes(ExtractedBytesFormat format, string asciiPayload)
        {
            var bytes = Encoding.ASCII.GetBytes(asciiPayload);
            return AddExtractedBytes(format, bytes, 0, bytes.Length, false, false, null);
        }

        /// <summary>
        /// Adds an <see cref="ExtractedBytes"/> with a given payload expressed as an ASCII string.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="asciiPayload"></param>
        /// <param name="checksumFailed"></param>
        /// <param name="hasParity"></param>
        public ExtractedBytes AddExtractedBytes(ExtractedBytesFormat format, string asciiPayload, bool checksumFailed, bool hasParity)
        {
            var bytes = Encoding.ASCII.GetBytes(asciiPayload);
            return AddExtractedBytes(format, bytes, 0, bytes.Length, checksumFailed, hasParity, null);
        }

        /// <summary>
        /// Adds an <see cref="ExtractedBytes"/> with a given payload expressed as an ASCII string.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="asciiPayload"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="checksumFailed"></param>
        /// <param name="hasParity"></param>
        public ExtractedBytes AddExtractedBytes(ExtractedBytesFormat format, string asciiPayload, int offset, int length, bool checksumFailed, bool hasParity)
        {
            var bytes = Encoding.ASCII.GetBytes(asciiPayload);
            return AddExtractedBytes(format, bytes, offset, length, checksumFailed, hasParity, null);
        }

        /// <summary>
        /// Adds an <see cref="ExtractedBytes"/> with a given payload expressed as an ASCII string.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="asciiPayload"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="checksumFailed"></param>
        /// <param name="hasParity"></param>
        /// <param name="signalLevel"></param>
        public ExtractedBytes AddExtractedBytes(ExtractedBytesFormat format, string asciiPayload, int offset, int length, bool checksumFailed, bool hasParity, int? signalLevel)
        {
            var bytes = Encoding.ASCII.GetBytes(asciiPayload);
            return AddExtractedBytes(format, bytes, offset, length, checksumFailed, hasParity, signalLevel);
        }

        /// <summary>
        /// Adds an <see cref="ExtractedBytes"/> with the supplied parameters.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="payload"></param>
        public ExtractedBytes AddExtractedBytes(ExtractedBytesFormat format, byte[] payload)
        {
            return AddExtractedBytes(format, payload, 0, payload.Length, false, false, null);
        }

        /// <summary>
        /// Adds an <see cref="ExtractedBytes"/> with the supplied parameters.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="payload"></param>
        /// <param name="checksumFailed"></param>
        /// <param name="hasParity"></param>
        public ExtractedBytes AddExtractedBytes(ExtractedBytesFormat format, byte[] payload, bool checksumFailed, bool hasParity)
        {
            return AddExtractedBytes(format, payload, 0, payload.Length, checksumFailed, hasParity, null);
        }

        /// <summary>
        /// Adds an <see cref="ExtractedBytes"/> with the supplied parameters.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="payload"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="checksumFailed"></param>
        /// <param name="hasParity"></param>
        /// <returns></returns>
        public ExtractedBytes AddExtractedBytes(ExtractedBytesFormat format, byte[] payload, int offset, int length, bool checksumFailed, bool hasParity)
        {
            return AddExtractedBytes(format, payload, offset, length, checksumFailed, hasParity, null);
        }

        /// <summary>
        /// Adds an <see cref="ExtractedBytes"/> with the supplied parameters.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="payload"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="checksumFailed"></param>
        /// <param name="hasParity"></param>
        /// <param name="signalLevel"></param>
        public ExtractedBytes AddExtractedBytes(ExtractedBytesFormat format, byte[] payload, int offset, int length, bool checksumFailed, bool hasParity, int? signalLevel)
        {
            var result = new ExtractedBytes() {
                Bytes = payload,
                Offset = offset,
                Length = length,
                Format = format,
                ChecksumFailed = checksumFailed,
                HasParity = hasParity,
                SignalLevel = signalLevel,
            };

            ExtractedBytes.Add(result);

            return result;
        }
    }
}
