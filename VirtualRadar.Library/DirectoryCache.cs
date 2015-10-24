// Copyright © 2012 onwards, Andrew Whewell
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
using VirtualRadar.Interface;
using System.IO;
using InterfaceFactory;
using VirtualRadar.Interop;

namespace VirtualRadar.Library
{
    /// <summary>
    /// The default implementation of <see cref="IDirectoryCache"/>.
    /// </summary>
    /// <remarks><para>
    /// This uses a polling technique rather than FileSystemWatcher because FSW is unreliable with
    /// with Linux SMB file shares. The polling technique has some obvious drawbacks but they should
    /// not have much of an impact on the intended users of this class.
    /// </para></remarks>
    class DirectoryCache : IDirectoryCache
    {
        #region Private Class - DefaultProvider, DefaultDirectoryCacheProviderFileInfo
        /// <summary>
        /// The default implementation of <see cref="IDirectoryCacheProvider"/>.
        /// </summary>
        class DefaultProvider : IDirectoryCacheProvider
        {
            public DateTime UtcNow                      { get { return DateTime.UtcNow; } }
            public bool FolderExists(string folder)     { return Directory.Exists(folder); }

            public IDirectoryCacheProviderFileInfo GetFileInfo(string fileName)
            {
                return File.Exists(fileName) ? new DefaultDirectoryCacheProviderFileInfo(new FileInfo(fileName)) : null;
            }

            public IEnumerable<string> GetSubFoldersInFolder(string folder)
            {
                return Directory.GetDirectories(folder);
            }

            public IEnumerable<IDirectoryCacheProviderFileInfo>GetFilesInFolder(string folder)
            {
                FindFile findFile = new FindFile();  // <- interop workaround for slow DirectoryInfo.GetFiles(), we can replace this with DirectoryInfo in .NET 4
                return findFile.GetFiles(folder).Select(fi => (IDirectoryCacheProviderFileInfo)(new DefaultDirectoryCacheProviderFileInfo(fi)));
            }
        }

        /// <summary>
        /// The default implementation of <see cref="IDirectoryCacheProviderFileInfo"/>.
        /// </summary>
        class DefaultDirectoryCacheProviderFileInfo : IDirectoryCacheProviderFileInfo
        {
            public string Name                  { get; set; }
            public DateTime LastWriteTimeUtc    { get; set; }

            public DefaultDirectoryCacheProviderFileInfo(FindFileData findFileData)
            {
                Name = findFileData.Name;
                LastWriteTimeUtc = findFileData.LastWriteTimeUtc;
            }

            public DefaultDirectoryCacheProviderFileInfo(FileInfo fileInfo)
            {
                Name = fileInfo.Name;
                LastWriteTimeUtc = fileInfo.LastWriteTimeUtc;
            }
        }
        #endregion

        #region Private class - CachedFileInfo
        /// <summary>
        /// An immutable class that describes a cached file instance.
        /// </summary>
        class CachedFileInfo
        {
            public string NormalisedName { get; private set; }

            public string Name { get; private set; }

            public DateTime LastWriteTimeUtc { get; private set; }

            public CachedFileInfo(string name, DateTime lastWriteTimeUtc)
            {
                Name = name;
                NormalisedName = NormaliseFileName(name);
                LastWriteTimeUtc = lastWriteTimeUtc;
            }

            public override bool Equals(object obj)
            {
                var result = Object.ReferenceEquals(this, obj);
                if(!result) {
                    var other = obj as CachedFileInfo;
                    return other != null && other.NormalisedName == NormalisedName;
                }

                return result;
            }

            public override int GetHashCode()
            {
                return (NormalisedName ?? "").GetHashCode();
            }

            public override string ToString()
            {
                return Name ?? "<null>";
            }
        }
        #endregion

        #region Private class - CachedFolderInfo
        class CachedFolderInfo
        {
            public string FullPath { get; private set; }

            public string NormalisedFullPath { get; private set; }

            public Dictionary<string, CachedFileInfo> Files { get; private set; }

            public List<CachedFolderInfo> SubFolders { get; private set; }

            public CachedFolderInfo(string folder)
            {
                FullPath = folder;
                NormalisedFullPath = NormaliseFolder(folder);
                Files = new Dictionary<string,CachedFileInfo>();
                SubFolders = new List<CachedFolderInfo>();
            }

            public override bool Equals(object obj)
            {
                var result = Object.ReferenceEquals(this, obj);
                if(!result) {
                    var other = obj as CachedFolderInfo;
                    result = other != null && other.NormalisedFullPath == NormalisedFullPath;
                }

                return result;
            }

            public override int GetHashCode()
            {
                return (NormalisedFullPath ?? "").GetHashCode();
            }

            public override string ToString()
            {
                return String.Format("{0} ({1} files)", FullPath, Files.Count);
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The object that is used to restrict access to every other field.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// The object that manages the clock for us.
        /// </summary>
        private IClock _Clock;

        /// <summary>
        /// A map of fully-pathed normalised folder names to a map of filenames within the folder and information about the files.
        /// </summary>
        private Dictionary<string, CachedFolderInfo> _Cache = new Dictionary<string, CachedFolderInfo>();

        /// <summary>
        /// True if sub-folders have been cached, false if they have not.
        /// </summary>
        private bool _CachedSubFolders;

        /// <summary>
        /// The date and time of the last fetch as at UTC.
        /// </summary>
        private DateTime _LastFetchTime;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IDirectoryCacheProvider Provider { get; set; }

        private volatile string _Folder;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Folder
        {
            get { return _Folder; }
            set
            {
                lock(_SyncLock) {
                    if(_Folder != value) {
                        _Folder = value;
                        BeginRefresh();
                    }
                }
            }
        }

        private volatile bool _CacheSubFolders;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool CacheSubFolders
        {
            get { return _CacheSubFolders; }
            set
            {
                lock(_SyncLock) {
                    if(_CacheSubFolders != value) {
                        _CacheSubFolders = value;
                        if(!String.IsNullOrEmpty(Folder)) BeginRefresh();
                    }
                }
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler CacheChanged;

        /// <summary>
        /// Raises <see cref="CacheChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnCacheChanged(EventArgs args)
        {
            EventHelper.Raise(CacheChanged, this, args);
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public DirectoryCache()
        {
            Provider = new DefaultProvider();
            _Clock = Factory.Singleton.Resolve<IClock>();
            Factory.Singleton.Resolve<IHeartbeatService>().Singleton.SlowTick += HeartbeatService_SlowTick;
        }
        #endregion

        #region SetConfiguration
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="cacheSubFolders"></param>
        /// <returns></returns>
        public bool SetConfiguration(string folder, bool cacheSubFolders)
        {
            var result = _Folder != folder || _CacheSubFolders != cacheSubFolders;
            if(result) {
                lock(_SyncLock) {
                    _Folder = folder;
                    _CacheSubFolders = cacheSubFolders;
                    BeginRefresh();
                }
            }

            return result;
        }
        #endregion

        #region GetFullPath, Add, Remove, BeginRefresh
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool FileExists(string fileName)
        {
            return false; // deprecated, will be getting rid of this soon.
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string GetFullPath(string fileName)
        {
            string result = null;

            if(!String.IsNullOrEmpty(fileName)) {
                var normalisedFileName = NormaliseFileName(fileName);
                lock(_SyncLock) {
                    foreach(var kvp in _Cache) {
                        var folderInfo = kvp.Value;
                        CachedFileInfo fileInfo;
                        if(folderInfo.Files.TryGetValue(normalisedFileName, out fileInfo)) {
                            result = Path.Combine(folderInfo.FullPath, fileInfo.Name);
                            break;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="fileName"></param>
        public void Add(string fileName)
        {
            AddRemoveFile(fileName, true);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="fileName"></param>
        public void Remove(string fileName)
        {
            AddRemoveFile(fileName, false);
        }

        /// <summary>
        /// Does the work of adding or removing a file from the cache.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="adding"></param>
        private void AddRemoveFile(string fileName, bool adding)
        {
            lock(_SyncLock) {
                if(!String.IsNullOrEmpty(fileName)) {
                    var folder = Path.GetDirectoryName(fileName);
                    if(IsChildOfRoot(folder) && Provider.FolderExists(folder)) {
                        var normalisedFolder = NormaliseFolder(folder);
                        CachedFolderInfo folderInfo;
                        if(!_Cache.TryGetValue(normalisedFolder, out folderInfo)) {
                            folderInfo = new CachedFolderInfo(folder);
                            _Cache.Add(normalisedFolder, folderInfo);
                        }

                        var normalisedFileName = NormaliseFileName(Path.GetFileName(fileName));
                        var fileWasCached = folderInfo.Files.ContainsKey(normalisedFileName);

                        var raiseCacheChanged = false;
                        if(adding && !fileWasCached) {
                            raiseCacheChanged = RefreshFilesInFolder(folderInfo, raiseCacheChanged);
                        } else if(!adding && fileWasCached) {
                            raiseCacheChanged = RefreshFilesInFolder(folderInfo, raiseCacheChanged);
                        }

                        if(raiseCacheChanged) OnCacheChanged(EventArgs.Empty);
                    }
                }
            }
        }

        private bool IsChildOfRoot(string folder)
        {
            var result = false;
            if(!String.IsNullOrEmpty(folder) && !String.IsNullOrEmpty(Folder)) {
                var rootAndSlash = NormaliseFolder(Folder, true);
                var folderAndSlash = NormaliseFolder(folder, true);

                if(!CacheSubFolders) result = folderAndSlash == rootAndSlash;
                else                 result = folderAndSlash.StartsWith(rootAndSlash);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void BeginRefresh()
        {
            string folder;
            lock(_SyncLock) folder = _Folder;

            var backgroundWorker = Factory.Singleton.Resolve<IBackgroundWorker>();
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.StartWork(folder);
        }
        #endregion

        #region LoadFiles
        /// <summary>
        /// Reloads the cache. Assumes that the caller has locked <see cref="_SyncLock"/>.
        /// </summary>
        private void LoadFiles(string folder)
        {
            try {
                bool raiseCacheChanged = false;

                if(folder == null || !Provider.FolderExists(folder)) {
                    raiseCacheChanged = _Cache.Count > 0;
                    _Cache.Clear();
                } else {
                    if(_CachedSubFolders != CacheSubFolders) {
                        raiseCacheChanged = _Cache.Count > 0;
                        _Cache.Clear();
                    }

                    raiseCacheChanged = RefreshFolderCache(folder, raiseCacheChanged);
                    _CachedSubFolders = CacheSubFolders;
                }

                if(raiseCacheChanged) OnCacheChanged(EventArgs.Empty);

                _LastFetchTime = _Clock.UtcNow;
            } catch(Exception ex) {
                Factory.Singleton.Resolve<ILog>().Singleton.WriteLine("Caught exception while reading filenames from {0}: {1}", folder, ex.ToString());
            }
        }

        private bool RefreshFolderCache(string folder, bool raiseCacheChanged)
        {
            var normalisedFolder = NormaliseFolder(folder);

            CachedFolderInfo folderInfo;
            if(!_Cache.TryGetValue(normalisedFolder, out folderInfo)) {
                folderInfo = new CachedFolderInfo(folder);
                _Cache.Add(normalisedFolder, folderInfo);
            }

            raiseCacheChanged = RefreshFilesInFolder(folderInfo, raiseCacheChanged);
            if(CacheSubFolders) raiseCacheChanged = RefreshSubFolders(folderInfo, raiseCacheChanged);

            return raiseCacheChanged;
        }

        private bool RefreshFilesInFolder(CachedFolderInfo folderInfo, bool raiseCacheChanged)
        {
            Dictionary<string, CachedFileInfo> files = folderInfo.Files;

            Dictionary<string, CachedFileInfo> oldFiles = null;
            if(!raiseCacheChanged) {
                oldFiles = new Dictionary<string, CachedFileInfo>();
                foreach(var oldFile in files) {
                    oldFiles.Add(oldFile.Key, oldFile.Value);
                }
            }

            files.Clear();
            var directoryEntries = Provider.GetFilesInFolder(folderInfo.FullPath).ToList();
            foreach(var file in directoryEntries) {
                var fileInfo = new CachedFileInfo(file.Name, file.LastWriteTimeUtc);
                files.Add(fileInfo.NormalisedName, fileInfo);
                if(oldFiles != null) {
                    CachedFileInfo oldFile;
                    if(!oldFiles.TryGetValue(fileInfo.NormalisedName, out oldFile)) raiseCacheChanged = true;
                    else {
                        if(oldFile.LastWriteTimeUtc != fileInfo.LastWriteTimeUtc) raiseCacheChanged = true;
                        oldFiles.Remove(fileInfo.NormalisedName);
                    }
                }
            }

            if(!raiseCacheChanged && oldFiles != null && oldFiles.Count != 0) raiseCacheChanged = true;

            return raiseCacheChanged;
        }

        private bool RefreshSubFolders(CachedFolderInfo folderInfo, bool raiseCacheChanged)
        {
            var cachedSubFolders = folderInfo.SubFolders;
            var subFolders = new List<CachedFolderInfo>();

            var deletedSubFolders = new Dictionary<string, string>();
            foreach(var name in cachedSubFolders.Select(r => r.NormalisedFullPath)) {
                deletedSubFolders.Add(name, name);
            }

            var subFolderNames = Provider.GetSubFoldersInFolder(folderInfo.FullPath).ToList();
            foreach(var subFolderName in subFolderNames) {
                var fullPath = Path.Combine(folderInfo.FullPath, subFolderName);
                var normalisedSubFolder = NormaliseFolder(fullPath);
                if(deletedSubFolders.ContainsKey(normalisedSubFolder)) deletedSubFolders.Remove(normalisedSubFolder);

                raiseCacheChanged = RefreshFolderCache(fullPath, raiseCacheChanged);

                var subFolderInfo = _Cache[normalisedSubFolder];
                subFolders.Add(subFolderInfo);
            }

            foreach(var deletedSubFolder in deletedSubFolders.Keys) {
                var deletedEntry = _Cache[deletedSubFolder];
                _Cache.Remove(deletedSubFolder);
                if(deletedEntry.Files.Count > 0) raiseCacheChanged = true;
            }

            folderInfo.SubFolders.Clear();
            folderInfo.SubFolders.AddRange(subFolders);

            return raiseCacheChanged;
        }
        #endregion

        #region NormaliseFolder
        /// <summary>
        /// Normalises a folder name.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="trailingSlash"></param>
        /// <returns></returns>
        private static string NormaliseFolder(string folder, bool trailingSlash = false)
        {
            var result = folder;
            if(!String.IsNullOrEmpty(result)) {
                var trailing = result[result.Length - 1];
                var hasTrailingSlash = trailing == Path.DirectorySeparatorChar || trailing == Path.AltDirectorySeparatorChar;
                if(trailingSlash && !hasTrailingSlash)      result += Path.DirectorySeparatorChar;
                else if(!trailingSlash && hasTrailingSlash) result = result.Substring(0, result.Length - 1);

                result = result.ToUpper();
            }

            return result;
        }

        /// <summary>
        /// Normalises a filename.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string NormaliseFileName(string fileName)
        {
            return fileName == null ? null : fileName.ToUpper();
        }
        #endregion

        #region Events consumed
        /// <summary>
        /// Called on a background thread by the background worker service.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void BackgroundWorker_DoWork(object sender, EventArgs args)
        {
            var folder = (string)((IBackgroundWorker)sender).State;
            lock(_SyncLock) {
                if(_Folder == folder) LoadFiles(folder);
            }
        }

        /// <summary>
        /// Called on a background thread by the heartbeat service.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HeartbeatService_SlowTick(object sender, EventArgs args)
        {
            if(_LastFetchTime.AddSeconds(60) <= _Clock.UtcNow) {
                lock(_SyncLock) LoadFiles(Folder);
            }
        }
        #endregion
    }
}
