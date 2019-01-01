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
        /// This will not be initialised until <see cref="Initialise"/> is called.
        /// </summary>
        /// <remarks>
        /// The object will always have the same lifetime as the singleton.
        /// </remarks>
        ITrackHistoryDatabase Database { get; }

        /// <summary>
        /// Raised before the configuration is changed.
        /// </summary>
        /// <remarks>
        /// If the configuration is about to be changed then objects that are maintaining
        /// track history sessions should close those sessions and ensure that no new
        /// sessions will be created until a corresponding <see cref="ConfigurationChanged"/>
        /// event is raised.
        /// </remarks>
        event EventHandler ConfigurationChanging;

        /// <summary>
        /// Raised after the configuration has changed or <see cref="Database"/> has been
        /// fully initialised.
        /// </summary>
        event EventHandler ConfigurationChanged;

        /// <summary>
        /// Initialises the properties, hooks events etc.
        /// </summary>
        void Initialise();
    }
}
