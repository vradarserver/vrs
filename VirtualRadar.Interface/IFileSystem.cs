// Copyright © 2017 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.IO;
using System.Text;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The interface for services that abstract away the file system.
    /// </summary>
    public interface IFileSystem
    {
        /// <summary>
        /// A wrapper around Path.Combine - only exists so that code that wants to avoid accidentally using System.IO.File
        /// or System.IO.Directory doesn't have to add a using System.IO to do trivial calls to Path.Combine.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        string Combine(params string[] paths);

        /// <summary>
        /// Returns the folder portion of a full path.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        string GetDirectory(string fullPath);

        /// <summary>
        /// Returns true if the directory exists.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool DirectoryExists(string path);

        /// <summary>
        /// Creates the path and returns true if the path does not already exist, otherwise returns false.
        /// </summary>
        /// <param name="path"></param>
        bool CreateDirectoryIfNotExists(string path);

        /// <summary>
        /// Returns true if the file exists.
        /// </summary>
        /// <param name="fileName">The name of the file. Case sensitivity depends on the underlying operating system.</param>
        /// <returns></returns>
        bool FileExists(string fileName);

        /// <summary>
        /// Returns the size of a file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        long FileSize(string fileName);

        /// <summary>
        /// Copies from the source filename to the destination filename.
        /// </summary>
        /// <param name="sourceFileName"></param>
        /// <param name="destFileName"></param>
        /// <param name="overwrite"></param>
        void CopyFile(string sourceFileName, string destFileName, bool overwrite);

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="fileName">The name of the file. Case sensitivity depends on the underlying operating system.</param>
        void DeleteFile(string fileName);

        /// <summary>
        /// Moves a file from source to destination.
        /// </summary>
        /// <param name="sourceFileName"></param>
        /// <param name="newFileName"></param>
        /// <param name="overwrite"></param>
        void MoveFile(string sourceFileName, string newFileName, bool overwrite);

        /// <summary>
        /// Opens a stream on a file. The caller has to dispose of the stream.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileMode"></param>
        /// <param name="fileAccess"></param>
        /// <param name="fileShare"></param>
        /// <returns></returns>
        Stream OpenFileStream(string fileName, FileMode fileMode, FileAccess fileAccess, FileShare fileShare);

        /// <summary>
        /// Returns the byte content of a file.
        /// </summary>
        /// <param name="fileName">The name of the file. Case sensitivity depends on the underlying operating system.</param>
        /// <returns></returns>
        byte[] ReadAllBytes(string fileName);

        /// <summary>
        /// Returns the byte content of a file as read from a background thread.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<byte[]> ReadAllBytesAsync(string fileName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns all lines from a text file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        string[] ReadAllLines(string fileName);

        /// <summary>
        /// Returns all lines from a text file as read from a background thread.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string[]> ReadAllLinesAsync(string fileName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns all text from the file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="encoding">Defaults to UTF8.</param>
        /// <returns></returns>
        string ReadAllText(string fileName, Encoding encoding = null);

        /// <summary>
        /// Returns all text from the file as read from a background thread.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="encoding">Defaults to UTF8.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string> ReadAllTextAsync(string fileName, Encoding encoding = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Overwrites the content of the file with the byte array passed across.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bytes"></param>
        void WriteAllBytes(string fileName, byte[] bytes);

        /// <summary>
        /// Overwrites the content of the file on a background thread with the byte array passed across.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bytes"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task WriteAllBytesAsync(string fileName, byte[] bytes, CancellationToken cancellationToken = default);

        /// <summary>
        /// Overwrites the content of the file with the lines of text passed across.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="contents"></param>
        void WriteAllLines(string fileName, IEnumerable<string> contents);

        /// <summary>
        /// Overwrites the content of the file on a background thread with the lines of text passed across.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="contents"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task WriteAllLinesAsync(string fileName, IEnumerable<string> contents, CancellationToken cancellationToken = default);

        /// <summary>
        /// Overwrites the content of the file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="text"></param>
        /// <param name="encoding">Defaults to UTF8.</param>
        void WriteAllText(string fileName, string text, Encoding encoding = null);

        /// <summary>
        /// Overwrites the content of the file on a background thread.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="text"></param>
        /// <param name="encoding"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task WriteAllTextAsync(string fileName, string text, Encoding encoding = null, CancellationToken cancellationToken = default);
    }
}
