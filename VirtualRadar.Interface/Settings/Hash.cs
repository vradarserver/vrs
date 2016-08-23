// Copyright © 2010 onwards, Andrew Whewell
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
using System.Text;
using System.Security.Cryptography;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// Describes a hashed password in the configuration.
    /// </summary>
    /// <remarks>
    /// Passwords are not stored in the configuration file - instead the code hashes them and stores the hash. When considering
    /// whether the password matches it hashes the attempted password and then compares hashes.
    /// </remarks>
    public class Hash
    {
        /// <summary>
        /// The version of the hashing algorithm used. If a new algorithm is implemented support should remain for hashes in older
        /// algorithms.
        /// </summary>
        public static readonly int LatestVersion = 1;

        /// <summary>
        /// Gets or sets the version of the hashing algorithm used to produce <see cref="Buffer"/>.
        /// </summary>
        public int Version { get; set; }

        private List<byte> _Buffer = new List<byte>();
        /// <summary>
        /// Gets the hash of the password. Callers can set this list to be the full byte sequence of the hash or read the hash from here.
        /// </summary>
        public List<byte> Buffer { get { return _Buffer; } }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Hash() : this(LatestVersion)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="version"></param>
        public Hash(int version)
        {
            Version = version;
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="password"></param>
        public Hash(string password) : this(LatestVersion, password)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="password"></param>
        public Hash(int version, string password) : this(version)
        {
            if(password == null) throw new ArgumentNullException("password");
            _Buffer.AddRange(HashText(version, password));
        }

        /// <summary>
        /// Returns true if the text passed across hashes into the same byte sequence held within <see cref="Buffer"/>.
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool PasswordMatches(string password)
        {
            if(password == null) throw new ArgumentNullException("password");

            List<byte> hashedText = HashText(Version, password);
            bool result = hashedText.Count == _Buffer.Count;
            if(result) {
                for(int i = 0;i < hashedText.Count;++i) {
                    if(hashedText[i] != _Buffer[i]) {
                        result = false;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the hash of the text passed across.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        private List<byte> HashText(int version, string text)
        {
            List<byte> result = new List<byte>();

            switch(version) {
                case 1:
                    SHA256Managed hashAlgorithm = new SHA256Managed();
                    result.AddRange(hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(text)));
                    break;
                default:
                    throw new InvalidOperationException($"Unknown hash version {version}");
            }

            return result;
        }
    }
}
