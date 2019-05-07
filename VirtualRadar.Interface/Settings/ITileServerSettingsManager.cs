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
using InterfaceFactory;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// The interface for singleton objects that can fetch tile server settings from the mothership
    /// and offer them up to the rest of the program.
    /// </summary>
    [Singleton]
    public interface ITileServerSettingsManager
    {
        /// <summary>
        /// Performs startup tasks.
        /// </summary>
        void Initialise();

        /// <summary>
        /// Returns true if the map provider passed across uses tile servers.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <returns></returns>
        bool MapProviderUsesTileServers(MapProvider mapProvider);

        /// <summary>
        /// Returns the tile server settings corresponding to the map provider and name passed across.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <param name="name"></param>
        /// <param name="fallbackToDefaultIfMissing"></param>
        /// <returns></returns>
        TileServerSettings GetTileServerSettings(MapProvider mapProvider, string name, bool fallbackToDefaultIfMissing);

        /// <summary>
        /// Returns the default tile server settings to use for a map provider.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <returns></returns>
        TileServerSettings GetDefaultTileServerSettings(MapProvider mapProvider);

        /// <summary>
        /// Returns all of the tile server settings for the map provider passed across.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <returns></returns>
        TileServerSettings[] GetAllTileServerSettings(MapProvider mapProvider);

        /// <summary>
        /// Returns all of the tile server layer settings for the map provider passed across.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <returns></returns>
        TileServerSettings[] GetAllTileLayerSettings(MapProvider mapProvider);

        /// <summary>
        /// Downloads the tile server settings from the mothership. Blocks until the download completes, times out or fails.
        /// </summary>
        void DownloadTileServerSettings();
    }
}
