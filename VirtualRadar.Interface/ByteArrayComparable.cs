// Copyright © 2014 onwards, Andrew Whewell
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
using System.Net;
using System.Text;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// An implementation of IComparable that returns the comparative order
    /// of a byte array.
    /// </summary>
    public class ByteArrayComparable : IComparable, IComparable<byte[]>, IComparable<ByteArrayComparable>
    {
        /// <summary>
        /// An empty byte array.
        /// </summary>
        private static byte[] _Empty = new byte[0];

        /// <summary>
        /// Gets the byte array that is being compared.
        /// </summary>
        public byte[] Array { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ByteArrayComparable() : this(_Empty)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="address"></param>
        public ByteArrayComparable(IPAddress address) : this(address == null ? null : address.GetAddressBytes())
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="endPoint"></param>
        public ByteArrayComparable(IPEndPoint endPoint)
        {
            if(endPoint == null || endPoint.Address == null) Array = _Empty;
            else {
                var addressBytes = endPoint.Address.GetAddressBytes();
                Array = new byte[addressBytes.Length + 2];
                addressBytes.CopyTo(Array, 0);
                Array[Array.Length - 2] = (byte)((endPoint.Port & 0xFF00) >> 8);
                Array[Array.Length - 1] = (byte)(endPoint.Port & 0xFF);
            }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="cidr"></param>
        public ByteArrayComparable(Cidr cidr)
        {
            if(cidr == null) Array = _Empty;
            else {
                var addressBytes = cidr.MaskedAddress.GetAddressBytes();
                Array = new byte[addressBytes.Length + 1];
                addressBytes.CopyTo(Array, 0);
                Array[Array.Length - 1] = (byte)cidr.BitmaskBits;
            }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="array"></param>
        public ByteArrayComparable(byte[] array)
        {
            Array = array ?? _Empty;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(byte[] other)
        {
            if(other == null) other = _Empty;
            int result = Array.Length - other.Length;
            if(result == 0) {
                for(var i = 0;result == 0 && i < Array.Length;++i) {
                    result = (int)Array[i] - (int)other[i];
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        int IComparable<ByteArrayComparable>.CompareTo(ByteArrayComparable other)
        {
            var array = other == null ? _Empty : other.Array;
            return CompareTo(array);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        int IComparable.CompareTo(object obj)
        {
            int result;

            var byteArrayComparable = obj as ByteArrayComparable;
            if(byteArrayComparable != null) result = ((IComparable<ByteArrayComparable>)this).CompareTo(byteArrayComparable);
            else {
                var array = obj as byte[];
                result = CompareTo((byte[])array);
            }

            return result;
        }
    }
}
