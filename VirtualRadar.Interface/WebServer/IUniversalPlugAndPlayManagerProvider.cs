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
    /// The interface for objects that abstract away the Windows universal plug'n'play COM component
    /// and other aspects of the environment for <see cref="IUniversalPlugAndPlayManager"/>.
    /// </summary>
    public interface IUniversalPlugAndPlayManagerProvider
    {
        /// <summary>
        /// Creates the UPnP COM component, some aspects of which are exposed by the provider.
        /// </summary>
        void CreateUPnPComComponent();

        /// <summary>
        /// Returns a list of every port mapping exposed by the UPnP COM component. Can return null
        /// if the COM component can't expose static port mappings.
        /// </summary>
        /// <returns></returns>
        List<IPortMapping> GetPortMappings();

        /// <summary>
        /// Adds a new port mapping via the UPnP COM component.
        /// </summary>
        /// <param name="externalPort"></param>
        /// <param name="protocol"></param>
        /// <param name="internalPort"></param>
        /// <param name="internalClient"></param>
        /// <param name="startEnabled"></param>
        /// <param name="description"></param>
        void AddMapping(int externalPort, string protocol, int internalPort, string internalClient, bool startEnabled, string description);

        /// <summary>
        /// Removes a mapping via the UPnP COM component.
        /// </summary>
        /// <param name="externalPort"></param>
        /// <param name="protocol"></param>
        void RemoveMapping(int externalPort, string protocol);
    }
}
