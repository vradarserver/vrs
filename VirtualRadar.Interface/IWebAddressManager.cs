// Copyright © 2022 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Handles the web addresses file in the configuration folder. Implementations are thread safe.
    /// </summary>
    public interface IWebAddressManager
    {
        /// <summary>
        /// The full path to the web addresses file.
        /// </summary>
        string AddressFileFullPath { get; }

        /// <summary>
        /// Adds or overwrites an address without overwriting existing custom addresses.
        /// </summary>
        /// <param name="name">The name of the address to add.</param>
        /// <param name="address">The address to add.</param>
        /// <param name="oldAddresses">An optional list of historical
        /// addresses that are to be overwritten with the new address. If an address
        /// already exists for <paramref name="name"/> and it is neither <paramref name="address"/>
        /// nor is it in the list of <paramref name="oldAddresses"/> then it is considered a
        /// custom address entered by the user and it is left unchanged.</param>
        /// <returns>The address actually registered against the name.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Missing name, missing or bad address</exception>
        string RegisterAddress(string name, string address, IList<string> oldAddresses = null);

        /// <summary>
        /// Returns the address associated with the case insensitive name. The program reserves
        /// names that start with 'vrs-'. Returns null if the name does not exist.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string LookupAddress(string name);
    }
}
