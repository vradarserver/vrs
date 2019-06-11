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
using VirtualRadar.Interface.View;

namespace VirtualRadar.Plugin.TileServerCache.WebAdmin
{
    /// <summary>
    /// Carries the option view's properties to the web page.
    /// </summary>
    public class ViewModel
    {
        public long DataVersion { get; set; }

        public bool IsPluginEnabled { get; set; }

        public bool IsOfflineModeEnabled { get; set; }

        public bool UseDefaultCacheFolder { get; set; }

        public string CacheFolderOverride { get; set; }

        [ValidationModelField(ValidationField.CacheFolder)]
        public ValidationModelField CacheFolderOverrideValidation { get; set; }

        public int TileServerTimeoutSeconds { get; set; }

        public bool CacheMapTiles { get; set; }

        public bool CacheLayerTiles { get; set; }

        public ViewModel()
        {
        }

        internal ViewModel(Options options) : this()
        {
            RefreshFromSettings(options);
        }

        internal void RefreshFromSettings(Options settings)
        {
            DataVersion =               settings.DataVersion;
            IsPluginEnabled =           settings.IsPluginEnabled;
            IsOfflineModeEnabled =      settings.IsOfflineModeEnabled;
            UseDefaultCacheFolder =     settings.UseDefaultCacheFolder;
            CacheFolderOverride =       UseDefaultCacheFolder ? "" : settings.CacheFolderOverride;
            TileServerTimeoutSeconds =  settings.TileServerTimeoutSeconds;
            CacheMapTiles =             settings.CacheMapTiles;
            CacheLayerTiles =           settings.CacheLayerTiles;
        }

        internal Options CopyToSettings(Options settings)
        {
            settings.DataVersion =               DataVersion;
            settings.IsPluginEnabled =           IsPluginEnabled;
            settings.IsOfflineModeEnabled =      IsOfflineModeEnabled;
            settings.CacheFolderOverride =       UseDefaultCacheFolder ? "" : CacheFolderOverride;
            settings.TileServerTimeoutSeconds =  TileServerTimeoutSeconds;
            settings.CacheMapTiles =             CacheMapTiles;
            settings.CacheLayerTiles =           CacheLayerTiles;

            return settings;
        }
    }

    public class SaveOutcomeModel
    {
        public string Outcome { get; set; }

        public ViewModel ViewModel { get; set; }

        public SaveOutcomeModel(string outcome, ViewModel viewModel)
        {
            Outcome = outcome;
            ViewModel = viewModel;
        }
    }
}
