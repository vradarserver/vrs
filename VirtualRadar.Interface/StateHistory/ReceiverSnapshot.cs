﻿// Copyright © 2020 onwards, Andrew Whewell
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
    /// Holds a snapshot of a receiver's details.
    /// </summary>
    public class ReceiverSnapshot : SnapshotRecord
    {
        /// <summary>
        /// Gets or sets the unique ID of the snapshot in the database.
        /// </summary>
        public long ReceiverSnapshotID { get; set; }

        /// <summary>
        /// Gets or sets the receiver's ID. This is not unique across all of state history,
        /// it is only guaranteed unique within a single VRS session.
        /// </summary>
        public int ReceiverID { get; set; }

        /// <summary>
        /// Gets or sets the receiver's unique ID. This is unique across all of state history.
        /// </summary>
        public Guid Key { get; set; }

        /// <summary>
        /// Gets or sets the receiver's name.
        /// </summary>
        public string ReceiverName { get; set; }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override byte[] FingerprintProperties()
        {
            return TakeFingerprint(
                ReceiverID,
                Key,
                ReceiverName
            );
        }

        /// <summary>
        /// Returns the fingerprint derived from component parts.
        /// </summary>
        /// <param name="countryName"></param>
        /// <returns></returns>
        public static byte[] TakeFingerprint(
            int receiverID,
            Guid key,
            string receiverName
        ) => Sha1Fingerprint.CreateFingerprintFromObjects(
            receiverID,
            key.ToString().ToUpper(),
            receiverName
        );

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"[{FingerprintHex}] {ReceiverID} {ReceiverName} ({Key})";
    }
}
