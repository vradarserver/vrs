// Copyright © 2023 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.IO;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// A stream that can expose a list of bytes like MemoryStream but also allows that
    /// list to be written to, a bit like FileStream. An optional callback can be supplied
    /// that will be called after every write.
    /// </summary>
    /// <remarks>
    /// This is mostly intended for use by the tests but it might be of general use so I've
    /// put it in the interface library.
    /// </remarks>
    public class ByteStream : Stream
    {
        private List<byte> _Content = new();

        private long _Position;

        private Action<IReadOnlyList<byte>> WriteCallback { get; }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override bool CanWrite => true;

        /// <inheritdoc/>
        public override long Length => _Content.Count;

        private int LengthRemaining => _Content.Count - (int)_Position;

        /// <inheritdoc/>
        public override long Position
        {
            get => _Position < _Content.Count ? _Position : -1L;
            set {
                if(value < 0 || value > _Content.Count) {
                    throw new IOException($"Cannot seek to {value}, the stream is only {Length} bytes");
                }
                _Position = value;
            }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="initialContent"></param>
        /// <param name="writeCallback">Called when the stream content is changed.</param>
        public ByteStream(IEnumerable<byte> initialContent, Action<IReadOnlyList<byte>> writeCallback = null)
        {
            if(initialContent != null) {
                _Content.AddRange(initialContent);
            }
            WriteCallback = writeCallback;
        }

        /// <inheritdoc/>
        public override void Flush()
        {
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            var lengthRemaining = LengthRemaining;
            var result = lengthRemaining < count
                ? lengthRemaining
                : count;

            _Content.CopyTo((int)_Position, buffer, offset, result);

            return result;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            var newPosition = _Position;

            switch(origin) {
                case SeekOrigin.Begin:      newPosition = 0; break;
                case SeekOrigin.Current:    newPosition += offset; break;
                case SeekOrigin.End:        newPosition = _Content.Count - offset; break;
            }

            Position = newPosition;

            return Position;
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            var intValue = (int)value;

            var contentChanged = true;
            if(intValue < _Content.Count) {
                _Content.RemoveRange(intValue, _Content.Count - intValue);
            } else if(intValue > _Content.Count) {
                _Content.AddRange(new byte[intValue - _Content.Count]);
            } else {
                contentChanged = false;
            }

            if(contentChanged) {
                WriteCallback?.Invoke(_Content);
            }
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            for(int idx = offset, length = count;length > 0;++idx, --length) {
                var b = buffer[idx];
                if(_Position == _Content.Count) {
                    _Content.Add(b);
                } else {
                    _Content[(int)_Position] = b;
                }
                ++_Position;
            }

            if(count > 0) {
                WriteCallback?.Invoke(_Content);
            }
        }
    }
}
