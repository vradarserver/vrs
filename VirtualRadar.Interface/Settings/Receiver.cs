// Copyright © 2013 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.IO.Ports;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// A settings object that carries information about a receiever.
    /// </summary>
    public class Receiver
    {
        /// <summary>
        /// Gets or sets a value indicating that the receiver is to be used.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a unique identifier for the receiever. This is unique across receivers and merged feeds. It cannot be zero.
        /// </summary>
        public int UniqueId { get; set; }

        /// <summary>
        /// Gets or sets the unique name of the receiver.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Gets or sets the unique ID of the IReceiverFormatProvider that will handle
        /// the feed from the receiever.
        /// </summary>
        public string DataSource { get; set; } = VirtualRadar.Interface.Settings.DataSource.Port30003;

        /// <summary>
        /// Gets or sets the mechanism to use to connect to the data source.
        /// </summary>
        public ConnectionType ConnectionType { get; set; } = ConnectionType.TCP;

        /// <summary>
        /// Obsolete since version 2.0.3. Network connections now always reconnect until you disable them.
        /// </summary>
        public bool AutoReconnectAtStartup { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating that the feed is coming off a satellite.
        /// </summary>
        /// <remarks>
        /// Aero Satcom feeds have much longer intervals between transmissions so they are subject to a
        /// separate set of display and tracking timeouts.
        /// </remarks>
        public bool IsSatcomFeed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the receiver will not connect to the source but will instead
        /// wait for the source to connect to it.
        /// </summary>
        /// <remarks>
        /// Only used when the <see cref="ConnectionType"/> is TCP. When a receiver is in passive mode the 
        /// <see cref="Address"/> is ignored. Only one source can connect to a passive receiver at a time,
        /// once a connection is established the receiver stops listening.
        /// </remarks>
        public bool IsPassive { get; set; }

        /// <summary>
        /// Gets or sets the access settings to use when the receiver is in passive mode.
        /// </summary>
        public Access Access { get; set; } = new();

        /// <summary>
        /// Gets or sets the address of the source of data to listen to.
        /// </summary>
        public string Address { get; set; } = "127.0.0.1";

        /// <summary>
        /// Gets or sets the port of the source of data to listen to.
        /// </summary>
        public int Port { get; set; } = 30003;

        /// <summary>
        /// Gets or sets a value indicating that the network connection should use KeepAlive packets.
        /// </summary>
        public bool UseKeepAlive { get; set; } = true;

        /// <summary>
        /// Gets or sets the period of time that must elapse with no received content before the network
        /// connection is reset.
        /// </summary>
        public int IdleTimeoutMilliseconds { get; set; } = 60000;

        /// <summary>
        /// Gets or sets the passphrase that the other side is expecting us to send in order to authenticate.
        /// </summary>
        /// <remarks>
        /// If this is null or empty then no passphrase is sent.
        /// </remarks>
        public string Passphrase { get; set; }

        /// <summary>
        /// Gets or sets the COM port to listen to.
        /// </summary>
        public string ComPort { get; set; }

        /// <summary>
        /// Gets or sets the baud rate to use.
        /// </summary>
        public int BaudRate { get; set; } = 115200;

        /// <summary>
        /// Gets or sets the data bits to use.
        /// </summary>
        public int DataBits { get; set; } = 8;

        /// <summary>
        /// Gets or sets the stop bits to use.
        /// </summary>
        public StopBits StopBits { get; set; } = StopBits.One;

        /// <summary>
        /// Gets or sets the parity to use.
        /// </summary>
        public Parity Parity { get; set; } = Parity.None;

        /// <summary>
        /// Gets or sets the handshake protocol to use.
        /// </summary>
        public Handshake Handshake { get; set; } = Handshake.None;

        /// <summary>
        /// Gets or sets the text to send across the COM port on startup - a null or empty string will disable the
        /// feature. Can contain \r and \n.
        /// </summary>
        public string StartupText { get; set; } = "#43-02\\r";

        /// <summary>
        /// Gets or sets the text to send across the COM port on shutdown - a null or empty string will disable the
        /// feature. Can contain \r and \n.
        /// </summary>
        public string ShutdownText { get; set; } = "#43-00\\r";

        /// <summary>
        /// Gets or sets the HTTP address to fetch data from.
        /// </summary>
        public string WebAddress { get; set; }

        /// <summary>
        /// Gets or sets the interval between fetches in milliseconds.
        /// </summary>
        public int FetchIntervalMilliseconds { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the identifier of the receiever location record associated with the receiver.
        /// </summary>
        public int ReceiverLocationId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how the receiver should be used by the system.
        /// </summary>
        public ReceiverUsage ReceiverUsage { get; set; }

        /// <summary>
        /// See base class docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Name ?? "<no name>";
    }
}
