// Copyright © 2015 onwards, Andrew Whewell
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

namespace VirtualRadar.Library.Listener
{
    /// <summary>
    /// The default implementation of <see cref="IAircraftListJsonMessageBytesExtractor"/>.
    /// </summary>
    class AircraftListJsonMessageBytesExtractor : IAircraftListJsonMessageBytesExtractor
    {
        /// <summary>
        /// The bytes that indicate the start of the JSON.
        /// </summary>
        /// <remarks>
        /// Do not include whitespace here. We accept whitespace outside of the string but
        /// it should not be included in the start marker, we'll sort that out later.
        /// </remarks>
        static byte[] _StartMarker = Encoding.UTF8.GetBytes(@"{""acList"":[");

        /// <summary>
        /// The buffered message that's been read so far.
        /// </summary>
        byte[] _ReadBuffer;

        /// <summary>
        /// The length of read buffer that we're actually using.
        /// </summary>
        int _ReadBufferLength;

        /// <summary>
        /// The largest we'll let a read buffer grow to.
        /// </summary>
        const int _MaxReadBufferLength = 1000000;

        /// <summary>
        /// An offset that is one past the end of the last good byte read from _ReadBuffer.
        /// </summary>
        /// <remarks>
        /// The state fields (in string, in escape etc.) are based on the content of the read buffer
        /// up to, but not including, this position.
        /// </remarks>
        int _ReadBufferPosition;

        /// <summary>
        /// The braces nesting level in the read buffer.
        /// </summary>
        int _BraceNestLevel;

        /// <summary>
        /// True if the current position resides within a string.
        /// </summary>
        bool _InString;

        /// <summary>
        /// True if the next character in the read buffer is escaped.
        /// </summary>
        bool _InEscape;

        /// <summary>
        /// The object that is repeatedly returned from <see cref="ExtractMessageBytes"/>. The bytes component
        /// of this is always the _ReadBuffer.
        /// </summary>
        ExtractedBytes _ExtractedBytes = new ExtractedBytes() { Format = ExtractedBytesFormat.AircraftListJson };

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long BufferSize
        {
            get { return _ReadBuffer == null ? 0 : _ReadBuffer.Length; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public IEnumerable<ExtractedBytes> ExtractMessageBytes(byte[] bytes, int offset, int length)
        {
            var resetBufferIfNoStartFound = _ReadBufferLength != 0;
            if(!AppendBytesToReadBuffer(bytes, offset, length)) {
                ResetState();
                _ReadBufferLength = 0;
            } else {
                while(_ReadBufferPosition < _ReadBufferLength) {
                    if(_ReadBufferPosition == 0) {
                        // Find the start of the JSON and move it to the start of the buffer. If everything is working
                        // as it should be the start will normally be on position 0 and no shift should be required.
                        var startOffset = GetJsonFirstBraceOffset();
                        if(startOffset == -1) {
                            if(resetBufferIfNoStartFound) {
                                // We only append one packet - if we still can't find the start then give up and wait
                                // for the next one. We don't want the buffer to keep growing indefinitely.
                                _ReadBufferLength = 0;
                            }
                            break;
                        }
                        if(startOffset > 0) {
                            ShiftReadBufferLeft(startOffset);
                        }

                        ResetState();
                        _ReadBufferPosition = 1;
                        _BraceNestLevel = 1;
                    }

                    var finalBraceOffset = GetJsonLastBraceOffset();

                    if(_BraceNestLevel > 0) {
                        _ReadBufferPosition = _ReadBufferLength;
                    } else {
                        var jsonLength = finalBraceOffset + 1;

                        _ExtractedBytes.Bytes = _ReadBuffer;
                        _ExtractedBytes.Offset = 0;
                        _ExtractedBytes.Length = jsonLength;
                        _ExtractedBytes.HasParity = false;
                        _ExtractedBytes.SignalLevel = null;

                        yield return _ExtractedBytes;

                        ResetState();

                        // I expect most of the time we get one complete JSON and then nothing else for a while,
                        // but just in case we've got a bunch of them we need to shift what remains to the start
                        // of the buffer and repeat the process.
                        if(jsonLength >= _ReadBufferLength) {
                            _ReadBufferLength = 0;
                        } else {
                            ShiftReadBufferLeft(jsonLength);
                            resetBufferIfNoStartFound = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Appends the bytes passed across to the end of the buffer and sets <see cref="_ReadBufferLength"/>.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        private bool AppendBytesToReadBuffer(byte[] bytes, int offset, int length)
        {
            var newLength = 0;

            if(_ReadBuffer == null) {
                if(length < _MaxReadBufferLength) {
                    _ReadBuffer = new byte[length];
                    Array.Copy(bytes, offset, _ReadBuffer, 0, length);
                }
                newLength = length;
            } else {
                newLength = _ReadBufferLength + length;
                if(newLength < _MaxReadBufferLength) {
                    if(newLength > _ReadBuffer.Length) {
                        var newReadBuffer = new byte[newLength];
                        if(_ReadBuffer != null) _ReadBuffer.CopyTo(newReadBuffer, 0);
                        _ReadBuffer = newReadBuffer;
                    }
                    Array.Copy(bytes, offset, _ReadBuffer, _ReadBufferLength, length);
                }
            }

            var result = newLength < _MaxReadBufferLength;
            if(result) {
                _ReadBufferLength = newLength;
            }

            return result;
        }

        /// <summary>
        /// Moves the bytes in _ReadBuffer to the left by so-many positions and adjusts
        /// <see cref="_ReadBufferLength"/> accordingly.
        /// </summary>
        /// <param name="count"></param>
        private void ShiftReadBufferLeft(int count)
        {
            var newLength = _ReadBufferLength - count;
            Array.Copy(_ReadBuffer, count, _ReadBuffer, 0, newLength);
            _ReadBufferLength = newLength;
        }

        /// <summary>
        /// Resets the parser state back to default values.
        /// </summary>
        private void ResetState()
        {
            _ReadBufferPosition = 0;
            _BraceNestLevel = 0;
            _InString = false;
            _InEscape = false;
        }

        /// <summary>
        /// Returns the offset of the first brace in the JSON or -1 if it could not be found.
        /// </summary>
        /// <returns></returns>
        private int GetJsonFirstBraceOffset()
        {
            var result = -1;

            var seqIndex = 0;
            var firstMatchByte = _StartMarker[seqIndex];
            var matchByte = firstMatchByte;
            var startIndex = -1;
            var inString = false;

            for(var i = 0;i < _ReadBufferLength;++i) {
                var ch = _ReadBuffer[i];
                if(ch == 0x22) inString = !inString;                                     // "
                else if(!inString && (ch == 0x20 || ch == 0x0A || ch == 0x0D)) continue; // whitespace

                if(ch != matchByte) {
                    if(startIndex != -1) {
                        seqIndex = 0;
                        matchByte = firstMatchByte;
                        startIndex = -1;
                        inString = false;
                    }
                } else {
                    if(seqIndex == 0) startIndex = i;
                    if(++seqIndex != _StartMarker.Length) {
                        matchByte = _StartMarker[seqIndex];
                    } else {
                        result = startIndex;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the offset within <see cref="_ReadBuffer"/> of the final brace in the JSON,
        /// or -1 if the brace could not be found.
        /// </summary>
        /// <returns></returns>
        private int GetJsonLastBraceOffset()
        {
            var result = -1;

            for(var i = _ReadBufferPosition;i < _ReadBufferLength && _BraceNestLevel > 0;++i) {
                var ch = _ReadBuffer[i];
                if(_InEscape) _InEscape = false;
                else {
                    switch(ch) {
                        case 0x22: _InString = !_InString; break;              // "
                        case 0x5C: if(_InString) _InEscape = true; break;      // \
                        case 0x7B: if(!_InString) ++_BraceNestLevel; break;    // {
                        case 0x7D:                                              // }
                            if(!_InString) {
                                if(--_BraceNestLevel == 0) {
                                    result = i;
                                }
                            }
                            break;
                    }
                }
            }

            return result;
        }
    }
}
