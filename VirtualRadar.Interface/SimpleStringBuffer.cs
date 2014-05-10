// Copyright © 2013 onwards, Andrew Whewell
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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// A very lightweight string buffer.
    /// </summary>
    /// <remarks>
    /// JsonSerialiser was using StringBuilder.Append to build up an escaped
    /// string, which worked just fine but in profiling it turns out that the
    /// Append call is slow. This is just a quicker implementation using an
    /// array of chars.
    /// </remarks>
    class SimpleStringBuffer
    {
        private const int _ExpandCount = 32;
        private char[] _Buffer;
        private int _Length = 0;

        public SimpleStringBuffer(int initialLength)
        {
            if(initialLength < _ExpandCount) initialLength = _ExpandCount;
            _Buffer = new char[initialLength];
        }

        public void Append(char ch)
        {
            ExpandBuffer(1);
            _Buffer[_Length++] = ch;
        }

        public void Append(string text)
        {
            ExpandBuffer(text.Length);
            for(var i = 0;i < text.Length;++i) {
                _Buffer[_Length++] = text[i];
            }
        }

        public override string ToString()
        {
            return new String(_Buffer, 0, _Length);
        }

        private void ExpandBuffer(int count)
        {
            var newSize = _Length + count;
            if(newSize >= _Buffer.Length) {
                newSize = newSize + (newSize % _ExpandCount);
                var newBuffer = new char[newSize];
                Array.Copy(_Buffer, newBuffer, _Length);
                _Buffer = newBuffer;
            }
        }
    }
}
