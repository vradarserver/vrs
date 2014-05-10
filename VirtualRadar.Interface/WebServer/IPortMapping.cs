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

namespace VirtualRadar.Interface.WebServer
{
    /// <summary>
    /// The interface for objects that describe a static port mapping on a UPnP router.
    /// </summary>
    public interface IPortMapping
    {
        /// <summary>
        /// Gets the description that the router holds for this port mapping.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the Internet-facing port number that the router will allow from the Internet onto the LAN.
        /// </summary>
        int ExternalPort { get; }

        /// <summary>
        /// Gets the address of the machine on the LAN that the UPnP router will forward packets to.
        /// </summary>
        string InternalClient { get; }

        /// <summary>
        /// Gets the port number of the machine at <see cref="InternalClient"/> that the UPnP router will forward packets to.
        /// </summary>
        int InternalPort { get; }

        /// <summary>
        /// Gets the protocol of the packets that this port mapping is for.
        /// </summary>
        string Protocol { get; }
    }
}
