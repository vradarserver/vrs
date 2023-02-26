// Copyright © 2023 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace VirtualRadar.Interface.Options
{
    public class TcpPullConnectionOptions
    {
        /// <summary>
        /// The address to connect to.
        /// </summary>
        public string Address { get; set; } = "127.0.0.1";

        /// <summary>
        /// The port to connect to.
        /// </summary>
        public int Port { get; set; } = 30003;

        /// <summary>
        /// True if keep-alive packets should be enabled on the connection.
        /// </summary>
        public bool UseKeepAlive { get; set; } = true;

        /// <summary>
        /// Only used if <see cref="UseKeepAlive"/> is false - the period of inactivity that
        /// must elapse before the connection is dropped and re-established. If this is zero
        /// the the feature is disabled.
        /// </summary>
        public int IdleTimeoutSeconds { get; set; } = 60;

        /// <summary>
        /// Used by System.Text.Json to preserve properties that it does not know how to deserialise.
        /// Allows users to regress to earlier versions of the program without losing options that
        /// were added in a later version.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement> PreservedForwardCompatibleProperties { get; set; }
    }
}
