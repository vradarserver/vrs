// Copyright © 2014 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Interface.Settings
{
    #pragma warning disable 0659        // Implemented Equals but not GetHashCode. These aren't keys, they aren't immutable, would be wrong to implement GetHashCode
    /// <summary>
    /// A class that holds access lists to Internet resources.
    /// </summary>
    public class Access
    {
        /// <summary>
        /// Gets or sets a value indicating the default access to support for the resource.
        /// This also indicates how <see cref="Addresses"/> is to be interpreted.
        /// </summary>
        public DefaultAccess DefaultAccess { get; set; } = DefaultAccess.Unrestricted;

        /// <summary>
        /// Gets a list of domain or IPv4 CIDR notation addresses that are either explicitly
        /// denied or allowed access to the resource.
        /// </summary>
        public IList<string> Addresses { get; } = new List<string>();

        /// <summary>
        /// Returns an Access object that denies all access aside from the local machine.
        /// </summary>
        /// <returns></returns>
        public static Access CreateDenyAllButLocalMachine()
        {
            return new Access() {
                DefaultAccess = DefaultAccess.Deny,
                Addresses = { "127.0.0.1/32" },
            };
        }

        /// <summary>
        /// Returns an Access object that denies all access aside from the local machine and the local network.
        /// </summary>
        /// <returns></returns>
        public static Access CreateDenyAllButLocalMachineAndNetwork()
        {
            return new Access() {
                DefaultAccess = DefaultAccess.Deny,
                Addresses = {
                    "127.0.0.1/32",
                    "10.0.0.0/8",
                    "169.254.0.0/16",
                    "172.16.0.0/12",
                    "192.168.0.0/16",
                },
            };
        }

        /// <summary>
        /// Returns true if the object passed across is an <see cref="Access"/> object with the
        /// same properties as this one.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var result = Object.ReferenceEquals(this, obj);
            if(!result && obj is Access other) {
                result = other.DefaultAccess == DefaultAccess &&
                         other.Addresses.SequenceEqual(Addresses);
            }

            return result;
        }
    }
}
