// Copyright © 2018 onwards, Andrew Whewell
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
using System.Threading.Tasks;
using InterfaceFactory;

namespace VirtualRadar.Interface.Database
{
    /// <summary>
    /// A singleton object that exposes a common <see cref="ITrackHistoryDatabase"/>
    /// for everyone to share and handles changes to the configuration of it.
    /// </summary>
    [Singleton]
    public interface ITrackHistoryDatabaseSingleton : IDisposable
    {
        /// <summary>
        /// Gets the <see cref="ITrackHistoryDatabase"/> instance that everyone can use.
        /// This can be null.
        /// </summary>
        /// <remarks>
        /// This will be null if <see cref="Initialise"/> has not been called or if the
        /// object has been disposed. The object itself is not guaranteed to have the same
        /// lifetime of the singleton, DO NOT take a reference to <see cref="Database"/>
        /// and then keep using it. Always get the latest object from here and use that.
        /// </remarks>
        ITrackHistoryDatabase Database { get; }

        /// <summary>
        /// Initialises the properties, hooks events etc.
        /// </summary>
        void Initialise();
    }
}
