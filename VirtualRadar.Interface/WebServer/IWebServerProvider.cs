// Copyright © 2010 onwards, Andrew Whewell
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
using System.Net;

namespace VirtualRadar.Interface.WebServer
{
    /// <summary>
    /// The provider that implementations of <see cref="IWebServer"/> should use to access the .NET framework.
    /// </summary>
    public interface IWebServerProvider : IDisposable
    {
        /// <summary>
        /// Gets the time now at UTC.
        /// </summary>
        DateTime UtcNow { get; }

        /// <summary>
        /// Gets or sets the prefix that the webserver will listen to.
        /// </summary>
        string ListenerPrefix { get; set; }

        /// <summary>
        /// Gets or sets the authentication schemes that will be accepted by the server.
        /// </summary>
        AuthenticationSchemes AuthenticationSchemes { get; set; }

        /// <summary>
        /// Gets the realm that the listener is responding to.
        /// </summary>
        string ListenerRealm { get; }

        /// <summary>
        /// Gets a value indicating that the listener is listening.
        /// </summary>
        bool IsListening { get; }

        /// <summary>
        /// Gets or sets a value indication that compression has been enabled by the user.
        /// </summary>
        bool EnableCompression { get; set; }

        /// <summary>
        /// Starts the web server.
        /// </summary>
        void StartListener();

        /// <summary>
        /// Stops the web server.
        /// </summary>
        void StopListener();

        /// <summary>
        /// Starts the process of listening for and retrieving the next <see cref="IContext"/> from the user on a background thread.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        IAsyncResult BeginGetContext(AsyncCallback callback);

        /// <summary>
        /// Retrieves the <see cref="IContext"/> that represents the request from the user.
        /// </summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        IContext EndGetContext(IAsyncResult asyncResult);

        /// <summary>
        /// Returns the IP addresses for the local machine.
        /// </summary>
        /// <returns></returns>
        IPAddress[] GetHostAddresses();
    }
}
