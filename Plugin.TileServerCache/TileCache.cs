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
using System.IO;
using System.Linq;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Plugin.TileServerCache
{
    /// <summary>
    /// A thread-safe object that can load and save tile images to a set of folders.
    /// </summary>
    static class TileCache
    {
        // Used to stop multiple threads creating the same folder at the same time or the same file at the same time.
        private static object _SyncLock = new object();

        // The VRS configuration folder. This should never change over the lifetime of the program.
        private static string _ConfigurationFolder;

        // The default cache root folder. This is always under _ConfigurationFolder.
        private static string _DefaultCacheRootFolder;

        /// <summary>
        /// Initialises the static object.
        /// </summary>
        public static void Initialise()
        {
            var configStorage = Factory.Resolve<IConfigurationStorage>().Singleton;
            _ConfigurationFolder = configStorage.Folder;
            _DefaultCacheRootFolder = Path.Combine(_ConfigurationFolder, "TileServerCache");
        }

        /// <summary>
        /// Returns the cached tile image or null if no such image exists.
        /// </summary>
        /// <param name="urlValues"></param>
        /// <returns></returns>
        public static CachedTile GetCachedTile(FakeUrlEncodedValues urlValues)
        {
            return GetCachedTile(
                urlValues.MapProvider,
                urlValues.Name,
                urlValues.Zoom,
                urlValues.X,
                urlValues.Y,
                urlValues.Retina,
                urlValues.TileImageExtension
            );
        }

        /// <summary>
        /// Returns the cached tile image or null if no such image exists.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <param name="tileServerName"></param>
        /// <param name="zoom"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="retina"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static CachedTile GetCachedTile(MapProvider mapProvider, string tileServerName, string zoom, string x, string y, string retina, string extension)
        {
            CachedTile result = null;

            var options = Plugin.Singleton?.Options;
            if(options != null) {
                var fileName = BuildFileName(options, mapProvider, tileServerName, zoom, x, y, retina, extension);
                if(fileName != null && File.Exists(fileName)) {
                    lock(_SyncLock) {
                        result = new CachedTile() {
                            FileName = fileName,
                            Content =  File.ReadAllBytes(fileName),
                        };
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Saves the image bytes.
        /// </summary>
        /// <param name="urlValues"></param>
        /// <param name="content"></param>
        public static void SaveTile(FakeUrlEncodedValues urlValues, byte[] content)
        {
            SaveTile(
                urlValues.MapProvider,
                urlValues.Name,
                urlValues.Zoom,
                urlValues.X,
                urlValues.Y,
                urlValues.Retina,
                urlValues.TileImageExtension,
                content
            );
        }

        /// <summary>
        /// Saves the image bytes.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <param name="tileServerName"></param>
        /// <param name="zoom"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="retina"></param>
        /// <param name="extension"></param>
        /// <param name="content"></param>
        public static void SaveTile(MapProvider mapProvider, string tileServerName, string zoom, string x, string y, string retina, string extension, byte[] content)
        {
            var options = Plugin.Singleton?.Options;
            if(options != null) {
                var fileName = BuildFileName(options, mapProvider, tileServerName, zoom, x, y, retina, extension);
                if(fileName != null) {
                    var folder = Path.GetDirectoryName(fileName);
                    lock(_SyncLock) {
                        if(!Directory.Exists(folder)) {
                            Directory.CreateDirectory(folder);
                        }

                        File.WriteAllBytes(fileName, content ?? new byte[0]);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the name of the cached image that corresponds with the parameters passed across.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <param name="tileServerName"></param>
        /// <param name="zoom"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="retina"></param>
        /// <returns></returns>
        private static string BuildFileName(Options options, MapProvider mapProvider, string tileServerName, string zoom, string x, string y, string retina, string extension)
        {
            var rootFolder = GetRootFolder(options);

            var result = Path.Combine(rootFolder, SanitisePathPart(mapProvider.ToString()));
            result =     Path.Combine(result,     SanitisePathPart(tileServerName));
            result =     Path.Combine(result,     SanitisePathPart(zoom));
            result =     Path.Combine(result,     SanitisePathPart(x));
            result =     Path.Combine(result,     SanitisePathPart($"{y}{retina}{extension}"));

            result = Path.GetFullPath(result);
            if(result.Length < rootFolder.Length) {
                result = null;
            } else if(!String.Equals(rootFolder, result.Substring(0, rootFolder.Length), StringComparison.OrdinalIgnoreCase)) {
                result = null;
            }

            return result;
        }

        /// <summary>
        /// Returns the root folder for the cache. All cached files must be below this folder in the hierarchy.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private static string GetRootFolder(Options options)
        {
            var result = options.CacheFolderOverride;
            if(String.IsNullOrEmpty(result)) {
                result = _DefaultCacheRootFolder;
            }

            return result;
        }

        /// <summary>
        /// Removes invalid characters etc. from path parts.
        /// </summary>
        /// <param name="pathPart"></param>
        /// <returns></returns>
        private static string SanitisePathPart(string pathPart)
        {
            var result = new StringBuilder();

            var invalidPathChars = Path.GetInvalidPathChars();
            var invalidFileNameChars = Path.GetInvalidFileNameChars();

            foreach(var ch in pathPart) {
                if(ch == '\\' || ch == '/' || invalidPathChars.Contains(ch) || invalidFileNameChars.Contains(ch)) {
                    continue;
                }
                result.Append(ch);
            }

            var text = result.ToString();
            if(text == "." || text == "..") {
                text = "";
            }

            return text;
        }
    }
}
