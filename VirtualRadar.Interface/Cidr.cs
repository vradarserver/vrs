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
using System.Net.Sockets;
using System.Text;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Represents a Classless Inter-Domain Routing value.
    /// </summary>
    /// <remarks><para>
    /// A CIDR is an IP address and a mask, expressed as a number of bits. The bits indicate the number of 1
    /// bits measured from the high bit down to the low bit - a bitmask of /1 is the high bit (bit 32) set,
    /// 2 would be bits 31 &amp; 32 set and so on. If the number of bits isn't specified then 32 is assumed.
    /// </para><para>
    /// The address is masked off to produce a set of high bits from the address. When we want to compare another
    /// address we apply the bitmask to the other address as well. If all of the bits match - i.e. an xor of
    /// both addresses after masking is 0 - then we have a match. Otherwise the addresses don't match.
    /// </para><para>
    /// It follows that if the number of bits is zero then every address will match the CIDR whereas if the
    /// bits are 32 then only one address can match.
    /// </para></remarks>
    public class Cidr
    {
        /// <summary>
        /// The IPAddress that is created by a zero bitmask.
        /// </summary>
        private static readonly IPAddress _ZeroMask = new IPAddress(new byte[] { 0, 0, 0, 0 });

        /// <summary>
        /// The bytes associated with a zero bitmask.
        /// </summary>
        private static readonly byte[] _ZeroMaskBytes = new byte[] { 0, 0, 0, 0 };

        /// <summary>
        /// The bytes formed by bitmasking the address.
        /// </summary>
        private byte[] _MaskedAddressBytes;

        /// <summary>
        /// Gets the IP address.
        /// </summary>
        public IPAddress Address { get; private set; }

        /// <summary>
        /// Gets the <see cref="Address"/> with the bitmask applied.
        /// </summary>
        public IPAddress MaskedAddress { get; private set; }

        /// <summary>
        /// Gets the number of bits used to construct the bitmask.
        /// </summary>
        public int BitmaskBits { get; private set; }

        /// <summary>
        /// Gets the bitmask to use when <see cref="Address"/> is IPv4.
        /// </summary>
        public uint IPv4Bitmask { get; private set; }

        /// <summary>
        /// Gets the first matching address.
        /// </summary>
        public IPAddress FirstMatchingAddress
        {
            get { return BuildFirstMatchingAddress(); }
        }

        /// <summary>
        /// Gets the last matching address.
        /// </summary>
        public IPAddress LastMatchingAddress
        {
            get { return BuildLastMatchingAddress(); }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Cidr()
        {
            Address = IPAddress.None;
            MaskedAddress = _ZeroMask;
            _MaskedAddressBytes = _ZeroMaskBytes;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0}/{1} [0x{2:X8}]", Address, BitmaskBits, IPv4Bitmask);
        }

        /// <summary>
        /// Returns true if the other object is a CIDR with the same properties as this object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var result = Object.ReferenceEquals(this, obj);
            if(!result) {
                var other = obj as Cidr;
                result = other != null && other.BitmaskBits == BitmaskBits && Address.Equals(other.Address);
            }

            return result;
        }

        /// <summary>
        /// Returns a hashcode for the CIDR. Guaranteed to return the same hashcode for two CIDRs that
        /// are equal.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked {
                return (Address.GetHashCode() << 5) | BitmaskBits;
            }
        }

        /// <summary>
        /// Returns true if the address passed across matches the CIDR.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public bool Matches(IPAddress address)
        {
            var result = false;

            if(address != null && address.AddressFamily == AddressFamily.InterNetwork) {
                var maskedAddress = ApplyBitmask(address, IPv4Bitmask);

                result = maskedAddress.Length == _MaskedAddressBytes.Length;
                for(var i = 0;result && i < _MaskedAddressBytes.Length;++i) {
                    result = _MaskedAddressBytes[i] == maskedAddress[i];
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the first address that matches the CIDR.
        /// </summary>
        /// <returns></returns>
        private IPAddress BuildFirstMatchingAddress()
        {
            return MaskedAddress;
        }

        /// <summary>
        /// Returns the last address that matches the CIDR.
        /// </summary>
        /// <returns></returns>
        private IPAddress BuildLastMatchingAddress()
        {
            var bytes = ApplyBitmask(Address, IPv4Bitmask, getLastMatchingAddress: true);
            return new IPAddress(bytes);
        }

        /// <summary>
        /// Parses an address. Throws exceptions if the parse fails.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static Cidr Parse(string address)
        {
            var result = new Cidr();

            if(!String.IsNullOrEmpty(address)) {
                // IPAddress.Parse is quite happy with addresses like "1" - this just makes sure that we
                // have something that resembles a dotted-quad
                if(address.Count(r => r == '.') != 3) throw new ArgumentException("Invalid IPv4 address", "address");

                var ipAddress = address;
                var bitmask = 32;

                var slashPosn = address.IndexOf('/');
                if(slashPosn != -1) {
                    ipAddress = address.Substring(0, slashPosn).Trim();
                    bitmask = int.Parse(address.Substring(slashPosn + 1));
                }

                if(bitmask < 0 || bitmask > 32) throw new ArgumentOutOfRangeException("The number of bits must be between 0 and 32 inclusive");
                result.BitmaskBits = bitmask;
                var clearBits = 32 - bitmask;
                while(bitmask-- != 0) result.IPv4Bitmask = (result.IPv4Bitmask << 1) | 1;
                result.IPv4Bitmask = result.IPv4Bitmask << clearBits;

                result.Address = IPAddress.Parse(ipAddress);
                if(result.Address.AddressFamily != AddressFamily.InterNetwork) throw new InvalidOperationException("Only IPv4 addresses are currently supported");

                result._MaskedAddressBytes = ApplyBitmask(result.Address, result.IPv4Bitmask);
                result.MaskedAddress = new IPAddress(result._MaskedAddressBytes);
            }

            return result;
        }

        /// <summary>
        /// Parses an address. Returns false and sets the <paramref name="cidr"/> to null if the parse fails.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="cidr"></param>
        /// <returns></returns>
        public static bool TryParse(string address, out Cidr cidr)
        {
            var result = true;

            try {
                cidr = Parse(address);
            } catch {
                cidr = null;
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Returns an IPAddress has been converted to a byte array and then had the bitmask applied to it.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="bitmask"></param>
        /// <param name="getLastMatchingAddress">If true then the address is masked, the bitmask inverted and or'd to produce the last matching address.</param>
        /// <returns></returns>
        private static byte[] ApplyBitmask(IPAddress address, uint bitmask, bool getLastMatchingAddress = false)
        {
            var bytes = address.GetAddressBytes();

            var addressValue = (uint)(bytes[0] << 24) | (uint)(bytes[1] << 16) | (uint)(bytes[2] << 8) | bytes[3];
            var bitmasked = addressValue & bitmask;

            if(getLastMatchingAddress) bitmasked |= ~bitmask;

            bytes[0] = (byte)((uint)(bitmasked & 0xff000000) >> 24);
            bytes[1] = (byte)((uint)(bitmasked & 0x00ff0000) >> 16);
            bytes[2] = (byte)((uint)(bitmasked & 0x0000ff00) >> 8);
            bytes[3] = (byte)(bitmasked & 0x000000ff);

            return bytes;
        }
    }
}
