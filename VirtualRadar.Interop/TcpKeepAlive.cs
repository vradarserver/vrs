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
using System.Runtime.InteropServices;

namespace VirtualRadar.Interop
{
    /// <summary>
    /// A struct suitable for use with interop calls that require a tcp_keepalive block.
    /// </summary>
    /// <remarks><para>
    /// See http://msdn.microsoft.com/en-us/library/dd877220(VS.85).aspx, C version is tcp_keepalive and
    /// is associated with SIO_KEEPALIVE_VALS control code.
    /// </para><para>
    /// By default BeginRead on a socket will wait forever with no heartbeat. If the connection breaks you get no
    /// indication. By specifying a hearbeat check with SIO_KEEPALIVE_VALS you can force it to throw a SocketException
    /// after a connection has remained broken for a given period of time.
    /// </para></remarks>
    public struct TcpKeepAlive
    {
        /// <summary>
        /// Indicates whether TCP keep-alive is enabled or disabled.
        /// </summary>
        public int OnOff;

        /// <summary>
        /// The timeout in milliseconds.
        /// </summary>
        public int KeepAliveTime;

        /// <summary>
        /// The interval in milliseconds between successive keep-alive packets.
        /// </summary>
        public int KeepAliveInterval;

        /// <summary>
        /// Returns a managed array of bytes representing the struct content.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This is intended for use with the Socket.IOControl method, which takes a managed array of bytes. We
        /// don't need to keep this struct hanging around in unmanaged memory when it's used.
        /// </remarks>
        public byte[] BuildBuffer()
        {
            var blockSize = Marshal.SizeOf(this);
            var result = new byte[blockSize];

            var nativeBlock = Marshal.AllocHGlobal(blockSize);
            try {
                Marshal.StructureToPtr(this, nativeBlock, false);
                Marshal.Copy(nativeBlock, result, 0, blockSize);
            } finally {
                Marshal.FreeHGlobal(nativeBlock);
            }

            return result;
        }
    }
}
