// Copyright © 2014 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.IO.Ports;

namespace VirtualRadar.Interface.Feeds
{
    /// <summary>
    /// An active connector that connects to a device over a serial COM port.
    /// </summary>
    public interface ISerialConnector : IConnector
    {
        /// <summary>
        /// Gets or sets the COM port to listen to.
        /// </summary>
        string ComPort { get; set; }

        /// <summary>
        /// Gets or sets the baud rate to use.
        /// </summary>
        int BaudRate { get; set; }

        /// <summary>
        /// Gets or sets the data bits to use.
        /// </summary>
        int DataBits { get; set; }

        /// <summary>
        /// Gets or sets the stop bits to use.
        /// </summary>
        StopBits StopBits { get; set; }

        /// <summary>
        /// Gets or sets the parity to use.
        /// </summary>
        Parity Parity { get; set; }

        /// <summary>
        /// Gets or sets the handshake protocol to use.
        /// </summary>
        Handshake Handshake { get; set; }

        /// <summary>
        /// Gets or sets the text to send across the COM port on startup - a null or empty string will disable the
        /// feature. Can contain \r and \n.
        /// </summary>
        string StartupText { get; set; }

        /// <summary>
        /// Gets or sets the text to send across the COM port on shutdown - a null or empty string will disable the
        /// feature. Can contain \r and \n.
        /// </summary>
        string ShutdownText { get; set; }
    }
}
