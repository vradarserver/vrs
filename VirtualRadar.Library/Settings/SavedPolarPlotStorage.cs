// Copyright © 2014 onwards, Andrew Whewell
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
using Newtonsoft.Json;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Library.Settings
{
    /// <summary>
    /// The default implementation of <see cref="ISavedPolarPlotStorage"/>.
    /// </summary>
    class SavedPolarPlotStorage : ISavedPolarPlotStorage
    {
        /// <summary>
        /// The lock object that prevents concurrent saves or loads.
        /// </summary>
        private object _SyncLock = new object();

        private static ISavedPolarPlotStorage _Singleton = new SavedPolarPlotStorage();
        /// <summary>
        /// See interface docs.
        /// </summary>
        public ISavedPolarPlotStorage Singleton { get { return _Singleton; } }

        /// <summary>
        /// The sub-folder under the configuration folder where polar plots are saved and loaded.
        /// </summary>
        public static readonly string SavedPlotsFolder = "SavedPlots";

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Save()
        {
            var feedManager = Factory.Resolve<IFeedManager>().Singleton;
            var feeds = feedManager.Feeds.ToArray();

            foreach(var feed in feeds) {
                Save(feed);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="feed"></param>
        public void Save(IFeed feed)
        {
            var feedName = feed == null ? null : feed.Name;
            var savedPolarPlot = new SavedPolarPlot(feed);

            if(!String.IsNullOrEmpty(feedName) && savedPolarPlot.FeedId > 0) {
                var folder = GetFolder();

                var fileName = GetFullPath(feedName);
                var content = JsonConvert.SerializeObject(savedPolarPlot, Formatting.Indented);
                lock(_SyncLock) {
                    if(!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    File.WriteAllText(fileName, content);
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="feed"></param>
        /// <returns></returns>
        public SavedPolarPlot Load(IFeed feed)
        {
            SavedPolarPlot result = null;

            var feedName = feed == null ? null : feed.Name;
            if(!String.IsNullOrEmpty(feedName)) {
                var fileName = GetFullPath(feedName);
                string content = null;

                lock(_SyncLock) {
                    if(File.Exists(fileName)) {
                        content = File.ReadAllText(fileName);
                    }
                }
                if(content != null) result = JsonConvert.DeserializeObject<SavedPolarPlot>(content);
                if(result != null && !result.IsForSameFeed(feed)) result = null;
            }

            return result;
        }

        /// <summary>
        /// Returns the full path to the folder that holds polar plots.
        /// </summary>
        /// <returns></returns>
        private string GetFolder()
        {
            var result = Factory.ResolveSingleton<IConfigurationStorage>().Folder;
            result = Path.Combine(result, SavedPlotsFolder);

            return result;
        }

        /// <summary>
        /// Returns the full path to the saved polar plot for a feed.
        /// </summary>
        /// <param name="feedName"></param>
        /// <returns></returns>
        private string GetFullPath(string feedName)
        {
            var fileName = SanitiseFileName(feedName);
            fileName = String.Format("{0}.json", fileName);

            return Path.Combine(GetFolder(), fileName);
        }

        /// <summary>
        /// Returns a string with invalid path characters converted to underscores.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string SanitiseFileName(string fileName)
        {
            var result = new StringBuilder();

            foreach(var ch in fileName) {
                var useCh = Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch;
                result.Append(useCh);
            }

            return result.ToString();
        }
    }
}
