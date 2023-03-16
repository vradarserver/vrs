// Copyright © 2023 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VirtualRadar.Interface.Options
{
    public class ReceiverOptions
    {
        /// <summary>
        /// Gets or sets the unique identifier of the receiver.
        /// </summary>
        public Guid ReceiverId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets a value indicating that the receiver is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the receiver's name that is shown to the user.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// The format of the feed that this receiver is expecting to to be sent.
        /// </summary>
        public string FeedFormat { get; set; } = "Port30003";

        /// <summary>
        /// Gets or sets a value indicating how to connect to the feed.
        /// </summary>
        public string ConnectionType { get; set; } = "TCP-PULL";

        /// <summary>
        /// The passphrase that the other side is expecting us to send when a connection is
        /// established. If this is null or empty then no passphrase is required.
        /// </summary>
        public string Passphrase { get; set; }

        /// <summary>
        /// The latitude of the receiver.
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// The longitude of the receiver.
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// Used by Newtonsoft.Json to preserve properties that it does not know how to deserialise.
        /// Allows users to regress to earlier versions of the program without losing options that
        /// were added in a later version.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JToken> PreservedForwardCompatibleProperties { get; set; }
    }
}
