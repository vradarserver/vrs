using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualRadar.Interface;

namespace VirtualRadar.Library
{
    /// <summary>
    /// The default implementation of <see cref="IFileSystemWatcher"/>.
    /// </summary>
    sealed class FileSystemWatcherWrapper : IFileSystemWatcher
    {
        /// <summary>
        /// The file system watcher that this class wraps.
        /// </summary>
        private FileSystemWatcher _FileSystemWatcher = new FileSystemWatcher();

        /// <summary>
        /// True if the events on <see cref="_FileSystemWatcher"/> have been hooked.
        /// </summary>
        private bool _EventsHooked;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool Enabled
        {
            get {
                return _FileSystemWatcher.EnableRaisingEvents;
            }
            set {
                _FileSystemWatcher.EnableRaisingEvents = value;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Filter
        {
            get {
                return _FileSystemWatcher.Filter;
            }
            set {
                _FileSystemWatcher.Filter = value;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IncludeSubdirectories
        {
            get {
                return _FileSystemWatcher.IncludeSubdirectories;
            }
            set {
                _FileSystemWatcher.IncludeSubdirectories = value;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int InternalBufferSize
        {
            get {
                return _FileSystemWatcher.InternalBufferSize;
            }
            set {
                _FileSystemWatcher.InternalBufferSize = Math.Min(65535, Math.Max(4096, value));
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public NotifyFilters NotifyFilter
        {
            get {
                return _FileSystemWatcher.NotifyFilter;
            }

            set {
                _FileSystemWatcher.NotifyFilter = value;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Path
        {
            get {
                return _FileSystemWatcher.Path;
            }

            set {
                _FileSystemWatcher.Path = value;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event FileSystemEventHandler Changed;

        /// <summary>
        /// Raises <see cref="Changed"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnChanged(FileSystemEventArgs args)
        {
            EventHelper.Raise(Changed, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event FileSystemEventHandler Created;

        /// <summary>
        /// Raises <see cref="Created"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnCreated(FileSystemEventArgs args)
        {
            EventHelper.Raise(Created, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event FileSystemEventHandler Deleted;

        /// <summary>
        /// Raises <see cref="Deleted"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnDeleted(FileSystemEventArgs args)
        {
            EventHelper.Raise(Deleted, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event ErrorEventHandler Error;

        /// <summary>
        /// Raises <see cref="Error"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnError(ErrorEventArgs args)
        {
            EventHelper.Raise(Error, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event RenamedEventHandler Renamed;

        /// <summary>
        /// Raises <see cref="Renamed"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnRenamed(RenamedEventArgs args)
        {
            EventHelper.Raise(Renamed, this, args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public FileSystemWatcherWrapper()
        {
            HookEvents();
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~FileSystemWatcherWrapper()
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
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if(disposing) {
                if(_FileSystemWatcher != null) {
                    UnhookEvents();
                    _FileSystemWatcher.Dispose();
                }
            }
        }

        /// <summary>
        /// Wraps the underlying file system watcher's events.
        /// </summary>
        private void HookEvents()
        {
            if(!_EventsHooked) {
                _EventsHooked = true;

                _FileSystemWatcher.Changed += FileSystemWatcher_Changed;
                _FileSystemWatcher.Created += FileSystemWatcher_Created;
                _FileSystemWatcher.Deleted += FileSystemWatcher_Deleted;
                _FileSystemWatcher.Renamed += FileSystemWatcher_Renamed;
                _FileSystemWatcher.Error +=   FileSystemWatcher_Error;
            }
        }

        /// <summary>
        /// Unhooks the underlying file system watcher's events.
        /// </summary>
        private void UnhookEvents()
        {
            if(_EventsHooked) {
                _FileSystemWatcher.Changed -= FileSystemWatcher_Changed;
                _FileSystemWatcher.Created -= FileSystemWatcher_Created;
                _FileSystemWatcher.Deleted -= FileSystemWatcher_Deleted;
                _FileSystemWatcher.Renamed -= FileSystemWatcher_Renamed;
                _FileSystemWatcher.Error -=   FileSystemWatcher_Error;

                _EventsHooked = false;
            }
        }

        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs args)
        {
            OnChanged(args);
        }

        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs args)
        {
            OnCreated(args);
        }

        private void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs args)
        {
            OnDeleted(args);
        }

        private void FileSystemWatcher_Renamed(object sender, RenamedEventArgs args)
        {
            OnRenamed(args);
        }

        private void FileSystemWatcher_Error(object sender, ErrorEventArgs args)
        {
            OnError(args);
        }
    }
}
