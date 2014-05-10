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
using System.Drawing;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The interface for an object that can deal with finding pictures of aircraft.
    /// </summary>
    public interface IAircraftPictureManager : ISingleton<IAircraftPictureManager>
    {
        /// <summary>
        /// Returns the full path and dimensions of the picture for the aircraft with the ICAO24 and registration
        /// passed across or null if no picture exists for the aircraft.
        /// </summary>
        /// <param name="directoryCache">The directory cache that will be used to look up filenames. Normally
        /// <see cref="IAutoConfigPictureFolderCache"/>'s DirectoryCache.</param>
        /// <param name="icao24">The ICAO24 code of the aircraft.</param>
        /// <param name="registration">The registration of the aircraft.</param>
        /// <returns></returns>
        PictureDetail FindPicture(IDirectoryCache directoryCache, string icao24, string registration);

        /// <summary>
        /// If existingDetail is not supplied or the file's last update or length has changed then the picture details are fetched
        /// and a new detail is returned. If existingDetail is supplied and neither the last update or length have changed then
        /// the existingDetail is returned without loading the picture.
        /// </summary>
        /// <param name="directoryCache"></param>
        /// <param name="icao24"></param>
        /// <param name="registration"></param>
        /// <param name="existingDetail"></param>
        /// <returns></returns>
        PictureDetail FindPicture(IDirectoryCache directoryCache, string icao24, string registration, PictureDetail existingDetail);

        /// <summary>
        /// Loads the aircraft picture with the ICAO24 and registration passed, returning null if the aircraft has
        /// no picture. It is the callers responsibility to dispose of the image returned. Note that the image may
        /// or may not have been loaded from a file.
        /// </summary>
        /// <param name="directoryCache"></param>
        /// <param name="icao24"></param>
        /// <param name="registration"></param>
        /// <returns></returns>
        Image LoadPicture(IDirectoryCache directoryCache, string icao24, string registration);
    }
}
