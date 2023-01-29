// Copyright © 2012 onwards, Andrew Whewell
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
    /// <summary>
    /// The settings for the rebroadcast server.
    /// </summary>
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
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the rebroadcast server is active (it
        /// transmits the feed to another machine) or passive (it accepts multiple connections
        /// from other machines).
        /// </summary>
        public bool IsTransmitter { get; set; }

        /// <summary>
        /// Gets or sets the address of the machine to send the feed to in Transmitter mode.
        /// </summary>
        public string TransmitAddress { get; set; }

        /// <summary>
        /// Gets or sets the port number to rebroadcast the receiver's messages on or the
        /// port to transmit the feed to.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the network connection should use KeepAlive packets.
        /// </summary>
        public bool UseKeepAlive { get; set; }

        /// <summary>
        /// Gets or sets the period of time that the receiving side has to accept a message within before
        /// the connection is dropped.
        /// </summary>
        public int IdleTimeoutMilliseconds { get; set; } = 30000;

        /// <summary>
        /// Gets or sets the threshold for discarding buffered messages that are too old.
        /// </summary>
        public int StaleSeconds { get; set; } = 3;

        /// <summary>
        /// Gets or sets the access settings for the rebroadcast server.
        /// </summary>
        public Access Access { get; set; } = new();

        /// <summary>
        /// Gets or sets the passphrase that the connecting side has to send before the connection is accepted.
        /// </summary>
        /// <remarks>
        /// If this is null or empty then no passphrase is required.
        /// </remarks>
        public string Passphrase { get; set; }

        /// <summary>
        /// Gets or sets the delay between sends for formats that lend themselves to message batching.
        /// </summary>
        /// <remarks>
        /// At the time of writing the only format that uses this is <see cref="RebroadcastFormat.AircraftListJson"/>.
        /// </remarks>
        public int SendIntervalMilliseconds { get; set; } = 1000;
    }
}
