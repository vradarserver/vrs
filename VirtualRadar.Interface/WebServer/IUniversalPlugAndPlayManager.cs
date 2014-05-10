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
using VirtualRadar.Interface.WebServer;

namespace VirtualRadar.Interface.WebServer
{
    /// <summary>
    /// The interface for the object that deals with UPnP routers for the application.
    /// </summary>
    public interface IUniversalPlugAndPlayManager : IDisposable
    {
        /// <summary>
        /// Gets or sets the provider that abstracts away the environment for the tests.
        /// </summary>
        IUniversalPlugAndPlayManagerProvider Provider { get; set; }

        /// <summary>
        /// Gets or sets the web server that wants packets sent to it from the outside world via the UPnP router.
        /// </summary>
        IWebServer WebServer { get; set; }

        /// <summary>
        /// Gets a value mirroring the configuration option that enables or disables the use of the UPnP router.
        /// </summary>
        /// <remarks>
        /// This is provided here for the sake of convenience.
        /// </remarks>
        bool IsEnabled { get; }

        /// <summary>
        /// Gets a value indicating that a UPnP router is present on the network.
        /// </summary>
        bool IsRouterPresent { get; }

        /// <summary>
        /// Gets a value indicating that there is a port forwarding on the router that redirects packets from the Internet on
        /// the UPnP port indicated by the configuration to this machine and to the port that <see cref="WebServer"/> is listening to.
        /// </summary>
        /// <remarks>
        /// This may be true even if the user has disabled the UPnP features of the program. It could be true because another
        /// instance of the program on another machine on the network has added the mapping to the router, or the user may have
        /// disabled UPnP control in the configuration while the router was turned off, or the user or another application has added
        /// their own port forwarding that the router is reporting back to us.
        /// </remarks>
        bool PortForwardingPresent { get; }

        /// <summary>
        /// Raised when any of the properties that describe the state of the manager changes.
        /// </summary>
        event EventHandler StateChanged;

        /// <summary>
        /// Initialises the manager once all of the properties have been built up.
        /// </summary>
        void Initialise();

        /// <summary>
        /// Adds the necessary port mappings to the UPnP router to put the <see cref="WebServer"/> onto the Internet.
        /// </summary>
        void PutServerOntoInternet();

        /// <summary>
        /// Reverses the port mappings added to the UPnP router by <see cref="PutServerOntoInternet"/> with the intention
        /// of taking the <see cref="WebServer"/> off the Internet.
        /// </summary>
        void TakeServerOffInternet();
    }
}
