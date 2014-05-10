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

namespace VirtualRadar.Interface.Listener
{
    /// <summary>
    /// The base interface for providers that can read bytes from network streams, serial ports etc.
    /// </summary>
    /// <remarks>
    /// There is no default implentation of this interface - use one of the derived interfaces instead,
    /// such as <see cref="ITcpListenerProvider"/> or <see cref="ISerialListenerProvider"/>.
    /// </remarks>
    public interface IListenerProvider
    {
        /// <summary>
        /// Gets the read buffer that <see cref="BeginRead"/> and <see cref="EndRead"/> fill.
        /// </summary>
        byte[] ReadBuffer { get; }

        /// <summary>
        /// Creates a connection to a source of data without blocking.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        IAsyncResult BeginConnect(AsyncCallback callback);

        /// <summary>
        /// Completes the connection to a machine started with <see cref="BeginConnect"/>.
        /// </summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        bool EndConnect(IAsyncResult asyncResult);

        /// <summary>
        /// Closes the connection.
        /// </summary>
        void Close();

        /// <summary>
        /// Calls BeginRead on the stream passed across.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        IAsyncResult BeginRead(AsyncCallback callback);

        /// <summary>
        /// Calls EndRead on the network stream.
        /// </summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        int EndRead(IAsyncResult asyncResult);

        /// <summary>
        /// Pauses the thread for a given number of milliseconds.
        /// </summary>
        /// <param name="milliseconds"></param>
        void Sleep(int milliseconds);
    }
}
