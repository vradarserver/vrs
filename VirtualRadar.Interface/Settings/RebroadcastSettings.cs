// Copyright © 2012 onwards, Andrew Whewell
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

namespace VirtualRadar.Interface.Settings
{
    #pragma warning disable 0659
    /// <summary>
    /// The settings for the rebroadcast server.
    /// </summary>
    /// <remarks>
    /// Equals is overridden on this object but GetHashCode is not - the object is mutable and
    /// is not safe for use as a key, there was no requirement to implement GetHashCode. Do
    /// not use as a key.
    /// </remarks>
    [Serializable]
    public class RebroadcastSettings
    {
        /// <summary>
        /// Gets or sets the unique identifier for the server. This cannot be zero.
        /// </summary>
        public int UniqueId { get; set; }

        /// <summary>
        /// Gets or sets the unique name for the server.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the rebroadcast server is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the receiver that the rebroadcast server will rebroadcast.
        /// </summary>
        public int ReceiverId { get; set; }

        /// <summary>
        /// Gets or sets the format in which to rebroadcast the receiver's messages.
        /// </summary>
        public RebroadcastFormat Format { get; set; }

        /// <summary>
        /// Gets or sets the port number to rebroadcast the receiver's messages on.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the threshold for discarding buffered messages that are too old.
        /// </summary>
        public int StaleSeconds { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public RebroadcastSettings()
        {
            StaleSeconds = 3;
        }
    }
    #pragma warning restore 0659
}
