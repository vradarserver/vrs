using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Wraps the .NET file system watcher class so that file watchers can be used in testable classes.
    /// </summary>
    public interface IFileSystemWatcher : IDisposable
    {
        /// <summary>
        /// Gets or sets a value indicating whether the watcher is active or not.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the path of the directory to watch or the full path of the file to watch.
        /// </summary>
        /// <remarks>
        /// Wildcards are acceptable. This property may be case sensitive on mono.
        /// </remarks>
        string Path { get; set; }

        /// <summary>
        /// Gets or sets the filter string used to determine which files are monitored in a directory.
        /// </summary>
        /// <remarks>
        /// This property may be case sensitive when running under mono.
        /// </remarks>
        string Filter { get; set; }

        /// <summary>
        /// Gets or sets the flags that determine what kinds of file changes should trigger an event.
        /// </summary>
        NotifyFilters NotifyFilter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether changes in subdirectories should be monitored.
        /// </summary>
        bool IncludeSubdirectories { get; set; }

        /// <summary>
        /// Gets or sets the size of the notifications buffer.
        /// </summary>
        /// <remarks>
        /// This should be a multiple of 4KB. It cannot be less than 4KB and cannot exceed 64KB. Note that
        /// the buffer is held in non-paged memory.
        /// </remarks>
        int InternalBufferSize { get; set; }

        /// <summary>
        /// Raised when a file or directory is modified.
        /// </summary>
        event FileSystemEventHandler Changed;

        /// <summary>
        /// Raised when a file or directory in <see cref="Path"/> is created.
        /// </summary>
        event FileSystemEventHandler Created;

        /// <summary>
        /// Raised when a file or directory in <see cref="Path"/> is deleted.
        /// </summary>
        event FileSystemEventHandler Deleted;

        /// <summary>
        /// Raised when a file or directory in <see cref="Path"/> is renamed.
        /// </summary>
        event RenamedEventHandler Renamed;

        /// <summary>
        /// Raised when the connection to the <see cref="Path"/> being watched is lost or the number of
        /// changes overwhelms the notifications buffer.
        /// </summary>
        event ErrorEventHandler Error;
    }
}
