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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Interface.StateHistory
{
    /// <summary>
    /// Holds a snapshot of an engine type enum value.
    /// </summary>
    public class EnginePlacementSnapshot : SnapshotRecord
    {
        /// <summary>
        /// Gets or sets the unique ID of the snapshot in the database.
        /// </summary>
        public long EnginePlacementSnapshotID { get; set; }

        /// <summary>
        /// Gets or sets the enum value.
        /// </summary>
        public int EnumValue { get; set; }

        /// <summary>
        /// Gets or sets the enum name.
        /// </summary>
        public string EnginePlacementName { get; set; }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override byte[] FingerprintProperties()
        {
            return TakeFingerprint(
                EnumValue,
                EnginePlacementName
            );
        }

        /// <summary>
        /// Returns the fingerprint derived from component parts.
        /// </summary>
        /// <param name="enumValue"></param>
        /// <param name="enumName"></param>
        /// <returns></returns>
        public static byte[] TakeFingerprint(int enumValue, string enumName) => Sha1Fingerprint.CreateFingerprintFromObjects(
            enumValue,
            enumName
        );

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"[{FingerprintHex}] {EnginePlacementName} ({EnumValue})";
    }
}
