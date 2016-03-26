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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The interface that objects that manage the downloading of air pressure readings has to implement.
    /// </summary>
    public interface IAirPressureManager : ISingleton<IAirPressureManager>
    {
        /// <summary>
        /// Gets a value indicating that air pressure downloads are enabled.
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// Gets the downloader that the manager uses to fetch air pressures.
        /// </summary>
        IAirPressureDownloader Downloader { get; }

        /// <summary>
        /// Gets the <see cref="IAirPressureLookup"/> that the manager is populating with downloaded
        /// air pressures.
        /// </summary>
        IAirPressureLookup Lookup { get; }

        /// <summary>
        /// Raised when a download of air pressures has been completed.
        /// </summary>
        event EventHandler DownloadCompleted;

        /// <summary>
        /// Starts downloading air pressures in the background.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops downloading air pressures in the background.
        /// </summary>
        void Stop();
    }
}
