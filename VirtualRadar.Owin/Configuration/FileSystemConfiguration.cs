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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.Owin.Configuration
{
    /// <summary>
    /// The default implementation of <see cref="IFileSystemConfiguration"/>.
    /// </summary>
    class FileSystemConfiguration : IFileSystemConfiguration
    {
        #region Private class - Root
        /// <summary>
        /// A private class that describes a root folder that we are serving files from.
        /// </summary>
        sealed class Root : IDisposable
        {
            /// <summary>
            /// A map of checksum entries keyed by normalised request paths for files.
            /// </summary>
            private Dictionary<string, ChecksumFileEntry> _RequestChecksumMap = new Dictionary<string, ChecksumFileEntry>();

            /// <summary>
            /// A map of checksum filenames and the local checksums that have been calculated for them.
            /// </summary>
            private Dictionary<string, string> _LocalFileChecksumMap = new Dictionary<string, string>();

            /// <summary>
            /// Locks write access to the fields.
            /// </summary>
            private object _SyncLock = new Object();

            /// <summary>
            /// The object that will watch for changes in checksummed files. If the root has no checksums then
            /// this will be null.
            /// </summary>
            private IFileSystemWatcher _FileSystemWatcher;

            /// <summary>
            /// Gets the <see cref="SiteRoot"/> that represents the folder.
            /// </summary>
            public SiteRoot SiteRoot { get; private set; }

            /// <summary>
            /// Gets the original folder from <see cref="SiteRoot"/>.
            /// </summary>
            public string Folder { get; private set; }

            /// <summary>
            /// Gets the original priority from <see cref="SiteRoot"/>.
            /// </summary>
            public int Priority { get; private set; }

            /// <summary>
            /// Gets a value indicating that modified files are not to be served from this folder.
            /// </summary>
            public bool IsProtectedContent
            {
                get {
                    return _RequestChecksumMap.Count > 0;
                }
            }

            /// <summary>
            /// Creates a new object.
            /// </summary>
            /// <param name="folder"></param>
            /// <param name="siteRoot"></param>
            public Root(string folder, SiteRoot siteRoot)
            {
                SiteRoot = siteRoot;
                Folder = folder;
                Priority = siteRoot.Priority;

                foreach(var checksum in siteRoot.Checksums) {
                    _RequestChecksumMap.Add(NormaliseRequestPath(checksum.FileName), checksum);
                }

                CreateFileSystemWatcher();
            }

            private void CreateFileSystemWatcher()
            {
                if(IsProtectedContent) {
                    var watcher = Factory.Singleton.Resolve<IFileSystemWatcher>();
                    watcher.Path = Folder;
                    watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Size;
                    watcher.Filter = "*";
                    watcher.IncludeSubdirectories = true;
                    watcher.Changed += FileSystemWatcher_Changed;
                    watcher.Deleted += FileSystemWatcher_Deleted;
                    watcher.Renamed += FileSystemWatcher_Renamed;
                    watcher.Error += FileSystemWatcher_Error;

                    lock(_SyncLock) {
                        _FileSystemWatcher = watcher;
                        _FileSystemWatcher.Enabled = true;
                    }
                }
            }

            private void DestroyFileSystemWatcher()
            {
                lock(_SyncLock) {
                    if(_FileSystemWatcher != null) {
                        _FileSystemWatcher.Enabled = false;
                        _FileSystemWatcher.Changed -= FileSystemWatcher_Changed;
                        _FileSystemWatcher.Deleted -= FileSystemWatcher_Deleted;
                        _FileSystemWatcher.Renamed -= FileSystemWatcher_Renamed;
                        _FileSystemWatcher.Error -= FileSystemWatcher_Error;
                        _FileSystemWatcher.Dispose();

                        _FileSystemWatcher = null;
                    }
                }
            }

            private void FileSystemWatcher_Error(object sender, ErrorEventArgs e)
            {
                DestroyFileSystemWatcher();
                ResetLocalFileChecksumMap();
                CreateFileSystemWatcher();
            }

            private void FileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
            {
                ResetLocalFileChecksumMap();
            }

            private void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
            {
                ResetLocalFileChecksumMap();
            }

            private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
            {
                ResetLocalFileChecksumMap();
            }

            /// <summary>
            /// Finalises the object.
            /// </summary>
            ~Root()
            {
                Dispose(false);
            }

            /// <summary>
            /// See interface docs.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Disposes or finalises the object.
            /// </summary>
            /// <param name="disposing"></param>
            /// <remarks>
            /// Note that it's possible for a site root to be disposed / removed while it is being used in the
            /// handling of a request.
            /// </remarks>
            private void Dispose(bool disposing)
            {
                if(disposing) {
                    DestroyFileSystemWatcher();
                    ResetLocalFileChecksumMap();
                }
            }

            /// <summary>
            /// Returns the checksum entry for the file passed across or null if no such checksum exists.
            /// </summary>
            /// <param name="requestPath"></param>
            /// <param name="requestPathIsNormalised"></param>
            /// <returns></returns>
            public ChecksumFileEntry FindChecksum(string requestPath, bool requestPathIsNormalised = false)
            {
                ChecksumFileEntry result = null;

                if(IsProtectedContent) {
                    if(!requestPathIsNormalised) {
                        requestPath = NormaliseRequestPath(requestPath);
                    }
                    _RequestChecksumMap.TryGetValue(requestPath, out result);
                }

                return result;
            }

            /// <summary>
            /// Tests whether the file described by the checksum entry has been altered.
            /// </summary>
            /// <param name="entry"></param>
            /// <param name="fileName"></param>
            /// <returns></returns>
            public bool TestChecksum(ChecksumFileEntry entry, string fileName)
            {
                var result = entry != null;
                if(result) {
                    result = File.Exists(fileName);
                    if(result) {
                        string checksum = GetOrGenerateLocalFileChecksum(entry, fileName);
                        var length = ChecksumFileEntry.GetFileSize(fileName);
                        result = checksum == entry.Checksum && length == entry.FileSize;
                    }
                }

                return result;
            }

            /// <summary>
            /// Returns the cached checksum for the file, generating it if it's not already cached.
            /// </summary>
            /// <param name="entry"></param>
            /// <param name="fileName"></param>
            /// <returns></returns>
            private string GetOrGenerateLocalFileChecksum(ChecksumFileEntry entry, string fileName)
            {
                string result = null;

                var localFileChecksumMap = _LocalFileChecksumMap;
                var watcher = _FileSystemWatcher;
                var checksumKey = entry.FileName;
                var fileSystemWatcherMissing = watcher == null || !watcher.Enabled;

                if(fileSystemWatcherMissing || !localFileChecksumMap.TryGetValue(checksumKey, out result)) {
                    result = ChecksumFileEntry.GenerateChecksum(fileName);

                    if(!fileSystemWatcherMissing) {
                        lock(_SyncLock) {
                            var newMap = CollectionHelper.ShallowCopy(_LocalFileChecksumMap);
                            if(newMap.ContainsKey(checksumKey)) {
                                newMap[checksumKey] = result;
                            } else {
                                newMap.Add(checksumKey, result);
                            }
                            _LocalFileChecksumMap = newMap;
                        }
                    }
                }

                return result;
            }

            /// <summary>
            /// Resets the local file checksum map without affecting requests that are already running.
            /// </summary>
            private void ResetLocalFileChecksumMap()
            {
                lock(_SyncLock) {
                    _LocalFileChecksumMap = new Dictionary<string, string>();
                }
            }
        }
        #endregion

        /// <summary>
        /// The list of file roots that this object serves pages from.
        /// </summary>
        private List<Root> _Roots = new List<Root>();

        private static IFileSystemConfiguration _Singleton;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IFileSystemConfiguration Singleton
        {
            get
            {
                if(_Singleton == null) {
                    _Singleton = new FileSystemConfiguration();
                }
                return _Singleton;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<TextContentEventArgs> HtmlLoadedFromFile;

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="siteRoot"></param>
        public void AddSiteRoot(SiteRoot siteRoot)
        {
            if(siteRoot == null) {
                throw new ArgumentNullException(nameof(siteRoot));
            }
            if(String.IsNullOrEmpty(siteRoot.Folder)) {
                throw new ArgumentException(nameof(siteRoot.Folder));
            }
            if(!Directory.Exists(siteRoot.Folder)) {
                throw new InvalidOperationException($"{siteRoot.Folder} does not exist");
            }
            if(!Path.IsPathRooted(siteRoot.Folder)) {
                throw new InvalidOperationException($"{siteRoot.Folder} is a relative path - only absolute paths can be site roots");
            }

            var folder = NormalisePath(siteRoot.Folder);
            if(_Roots.Any(r => r.Folder.Equals(folder, StringComparison.OrdinalIgnoreCase))) {
                throw new InvalidOperationException($"{siteRoot.Folder} is already a site root");
            }

            _Roots.Add(new Root(folder, siteRoot));
            _Roots.Sort((lhs, rhs) => { return lhs.Priority < rhs.Priority ? -1 : lhs.Priority == rhs.Priority ? 0 : 1; });
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="siteRoot"></param>
        public void RemoveSiteRoot(SiteRoot siteRoot)
        {
            var root = _Roots.SingleOrDefault(r => r.SiteRoot == siteRoot);
            if(root != null) {
                _Roots.Remove(root);
                root.Dispose();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public List<string> GetSiteRootFolders()
        {
            return _Roots.Select(r => r.Folder).ToList();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="siteRoot"></param>
        /// <param name="folderMustMatch"></param>
        /// <returns></returns>
        public bool IsSiteRootActive(SiteRoot siteRoot, bool folderMustMatch)
        {
            var result = siteRoot != null;
            if(result) {
                var normalisedSiteRootFolder = NormalisePath(siteRoot.Folder);
                result = _Roots.SingleOrDefault(r => 
                    r.SiteRoot == siteRoot && (!folderMustMatch || (r.Folder.Equals(normalisedSiteRootFolder, StringComparison.OrdinalIgnoreCase)))
                ) != null;
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="args"></param>
        public void RaiseHtmlLoadedFromFile(TextContentEventArgs args)
        {
            EventHelper.Raise(HtmlLoadedFromFile, this, () => args);
        }

        /// <summary>
        /// Returns a normalised version of a request path.
        /// </summary>
        /// <param name="requestPath"></param>
        /// <returns></returns>
        private static string NormaliseRequestPath(string requestPath)
        {
            return (requestPath ?? "").ToLower().Replace("\\", "/");
        }

        /// <summary>
        /// Flattens the path passed across and adjusts path separators so that they're treated consistently.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        private static string NormalisePath(string folder)
        {
            folder = Path.GetFullPath(folder ?? "");
            if(Path.AltDirectorySeparatorChar != Path.DirectorySeparatorChar) folder = folder.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            if(folder[folder.Length - 1] != Path.DirectorySeparatorChar) folder += Path.DirectorySeparatorChar;

            return folder;
        }
    }
}
