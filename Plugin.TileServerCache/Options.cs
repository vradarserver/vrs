// Copyright © 2019 onwards, Andrew Whewell
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

namespace VirtualRadar.Plugin.TileServerCache
{
    /// <summary>
    /// The options for the Tile Server Cache plugin.
    /// </summary>
    class Options : ICloneable
    {
        /// <summary>
        /// Gets or sets the version of the options being saved.
        /// </summary>
        public long DataVersion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the plugin is enabled.
        /// </summary>
        public bool IsPluginEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that tiles should always be served from the cache and no attempt
        /// should be made to fetch expired or new tiles from the tile server. Anything not in the cache returns
        /// a status 404 response.
        /// </summary>
        public bool IsOfflineModeEnabled { get; set; }

        /// <summary>
        /// Gets or sets the folder to load cached tiles from and save cached tiles to. If empty then
        /// a folder under the VRS configuration folder is used.
        /// </summary>
        public string CacheFolderOverride { get; set; }

        /// <summary>
        /// Gets a value indicating that the default cache folder should be used.
        /// </summary>
        public bool UseDefaultCacheFolder => String.IsNullOrEmpty(CacheFolderOverride);

        /// <summary>
        /// Gets or sets the number of seconds to wait for a tile server to respond before assuming that it is
        /// offline and either returning an expired cached tile or a 404 response.
        /// </summary>
        public int TileServerTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets a value indicating that map tiles should be cached.
        /// </summary>
        public bool CacheMapTiles { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating that layer tiles should be cached.
        /// </summary>
        public bool CacheLayerTiles { get; set; } = true;

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
