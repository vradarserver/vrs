// Copyright © 2016 onwards, Andrew Whewell
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

namespace VirtualRadar.Interface.Network
{
    /// <summary>
    /// The singleton manager that keeps track of all registered rebroadcast server formats.
    /// </summary>
    /// <remarks>
    /// Plugins cannot provide their own implementation of this interface, the singleton is established
    /// before RegisterImplementations is called.
    /// </remarks>
    public interface IRebroadcastFormatManager : ISingleton<IRebroadcastFormatManager>
    {
        /// <summary>
        /// Initialises the format manager.
        /// </summary>
        void Initialise();

        /// <summary>
        /// Registers a provider.
        /// </summary>
        /// <param name="provider"></param>
        /// <remarks>
        /// Plugins should register their providers in RegisterImplementations, which should be
        /// called before VRS tries to connect to any feeds.
        /// </remarks>
        void RegisterProvider(IRebroadcastFormatProvider provider);

        /// <summary>
        /// Returns a collection of all registered rebroadcast server formats in a summarised form.
        /// </summary>
        /// <returns></returns>
        RebroadcastFormatName[] GetRegisteredFormats();

        /// <summary>
        /// Returns the registered provider with the identifier supplied or null if no provider has been
        /// registered for this ID.
        /// </summary>
        /// <param name="providerId"></param>
        /// <returns></returns>
        IRebroadcastFormatProvider GetProvider(string providerId);

        /// <summary>
        /// Creates a new instance of the provider with the unique ID passed across. Returns null if no provider
        /// is registered for the ID passed across.
        /// </summary>
        /// <param name="providerId"></param>
        /// <returns></returns>
        IRebroadcastFormatProvider CreateProvider(string providerId);

        /// <summary>
        /// Returns the short name associated with the unique ID passed across. If there is no
        /// provider registered with the unique ID then a string along the lines of &quot;Unknown&quot;
        /// is returned.
        /// </summary>
        /// <param name="providerId"></param>
        /// <returns></returns>
        string ShortName(string providerId);
    }
}
