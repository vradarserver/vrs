// Copyright © 2017 onwards, Andrew Whewell
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
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.Interface.Owin
{
    /// <summary>
    /// The interface for a singleton object that holds configuration information for
    /// <see cref="IImageServer"/> middleware.
    /// </summary>
    public interface IImageServerConfiguration : ISingleton<IImageServerConfiguration>
    {
        /// <summary>
        /// Gets the operator flag folder from current configuration or null if the configured folder
        /// does not exist or is not accessible.
        /// </summary>
        string OperatorFolder { get; }

        /// <summary>
        /// Gets the silhouette flag folder from current configuration or null if the configured folder
        /// does not exist or is not accessible.
        /// </summary>
        string SilhouettesFolder { get; }

        /// <summary>
        /// Gets the instance of <see cref="IAircraftPictureManager"/> that all instances of image server
        /// middleware should share.
        /// </summary>
        IAircraftPictureManager AircraftPictureManager { get; }

        /// <summary>
        /// Gets the instance of <see cref="IDirectoryCache"/> that all instances of image server middleware
        /// should share.
        /// </summary>
        IDirectoryCache AircraftPictureCache { get; }
    }
}
