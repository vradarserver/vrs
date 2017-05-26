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
using VirtualRadar.Interface;

namespace Test.Framework
{
    /// <summary>
    /// Configurable mock file system provider.
    /// </summary>
    public class MockFileSystemProvider : IFileSystemProvider
    {
        class MockFileSystemEntity
        {
            protected MockFileSystemProvider _Provider;

            public MockFolder ParentFolder { get; private set; }

            public string Name { get; private set; }

            public MockFileSystemEntity(MockFileSystemProvider provider, MockFolder parentFolder, string name)
            {
                _Provider = provider;
                ParentFolder = parentFolder;
                Name = name;
            }

            public override bool Equals(object obj)
            {
                var result = Object.ReferenceEquals(this, obj);
                if(!result) {
                    if(obj is MockFileSystemEntity other) {
                        result = Equals(other.Name);
                    } else if(obj is string otherName) {
                        result = String.Equals(Name, otherName, _Provider.IsCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
                    }
                }

                return result;
            }

            public override int GetHashCode()
            {
                return (Name ?? "").GetHashCode();
            }

            public override string ToString()
            {
                var result = new StringBuilder(Name);
                for(var parent = ParentFolder;parent != null;parent = parent.ParentFolder) {
                    result.Insert(0, Path.DirectorySeparatorChar);
                    result.Insert(0, parent.Name);
                }

                return result.ToString();
            }
        }

        /// <summary>
        /// Describes a mock file and its contents.
        /// </summary>
        class MockFile : MockFileSystemEntity
        {
            public byte[] Content { get; private set; }

            public MockFile(MockFileSystemProvider owner, MockFolder parentFolder, string name) : this(owner, parentFolder, name, new byte[0])
            {
            }

            public MockFile(MockFileSystemProvider owner, MockFolder parentFolder, string name, byte[] content) : base(owner, parentFolder, name)
            {
                switch(name) {
                    case ".":
                    case "..":
                        throw new ArgumentException($"{name} is not a valid file name");
                    default:
                        if(name.Trim().Length == 0 || name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1) {
                            throw new ArgumentException($"{name} is not a valid file name");
                        }
                        break;
                }

                Content = content;
            }

            public void ChangeContent(byte[] content)
            {
                Content = content;
            }
        }

        /// <summary>
        /// Describes a mock folder and its contents.
        /// </summary>
        class MockFolder : MockFileSystemEntity
        {
            public HashSet<MockFileSystemEntity> Contents { get; private set; } = new HashSet<MockFileSystemEntity>();

            public MockFolder[] SubFolders { get { return Contents.OfType<MockFolder>().ToArray(); } }

            public MockFile[] Files { get { return Contents.OfType<MockFile>().ToArray(); } }

            public MockFolder(MockFileSystemProvider owner, MockFolder parent, string name) : base(owner, parent, name)
            {
                switch(name) {
                    case ".":
                    case "..":
                        throw new ArgumentException($"{name} is not a valid directory name");
                    case "":
                        if(parent != null) {
                            throw new ArgumentException("The folder name cannot be empty");
                        }
                        break;
                    default:
                        if(name.IndexOfAny(Path.GetInvalidPathChars()) != -1) {
                            throw new ArgumentException($"{name} contains invalid path characters");
                        }
                        break;
                }
            }

            public MockFolder AddFolder(string name)
            {
                var result = new MockFolder(_Provider, this, name);
                Contents.Add(result);

                return result;
            }

            public MockFile AddFile(string name)
            {
                return AddFile(name, new byte[0]);
            }

            public MockFile AddFile(string name, byte[] content)
            {
                var result = new MockFile(_Provider, this, name, content);
                Contents.Add(result);

                return result;
            }

            public MockFile DeleteFile(string name)
            {
                var result = FindFile(name);
                if(result != null) {
                    Contents.Remove(result);
                }

                return result;
            }

            public MockFile OverwriteFile(string name, byte[] content)
            {
                var result = FindFile(name);

                if(result != null) {
                    result.ChangeContent(content);
                }

                return result;
            }

            public MockFile AddOrOverwriteFile(string name, byte[] content)
            {
                var result = FindFile(name);

                if(result == null) {
                    result = AddFile(name, content);
                } else {
                    result.ChangeContent(content);
                }

                return result;
            }

            public MockFileSystemEntity FindEntity(string name)
            {
                return Contents.FirstOrDefault(r => r.Equals(name));
            }

            public MockFile FindFile(string name)
            {
                return FindEntity(name) as MockFile;
            }

            public MockFolder FindFolder(string name)
            {
                switch(name ?? "") {
                    case ".":   return this;
                    case "..":  return ParentFolder;
                    default:    return FindEntity(name) as MockFolder;
                }
            }

            public MockFolder CreateOrFindFolder(string name)
            {
                var result = FindFolder(name);
                if(result == null) {
                    result = AddFolder(name);
                }

                return result;
            }
        }

        /// <summary>
        /// The root folder of the mock file system.
        /// </summary>
        private MockFolder _Root;

        /// <summary>
        /// Gets or sets a value indicating whether file and folder names are case sensitive.
        /// </summary>
        public bool IsCaseSensitive { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public MockFileSystemProvider()
        {
            _Root = new MockFolder(this, null, "");
        }

        /// <summary>
        /// Adds a fully pathed folder. The directory separators can be forward-slashes or backslashes. Drive specs are ignored.
        /// </summary>
        /// <param name="path"></param>
        public void AddFolder(string path)
        {
            var folder = _Root;
            foreach(var pathPart in DecomposePath(path)) {
                folder = folder.CreateOrFindFolder(pathPart);
            }
        }

        /// <summary>
        /// Adds a fully pathed file. The directory separators can be forward-slashes or backslashes. Drive specs are ignored.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        public void AddFile(string path, byte[] content)
        {
            CreatePathAndPerformActionOnFile(path, (folder, fileName) => folder.AddFile(fileName, content));
        }

        /// <summary>
        /// Overwrites a fully pathed file.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        public void OverwriteFile(string path, byte[] content)
        {
            FollowPathAndPerformActionOnFile(path, (folder, fileName) => folder.OverwriteFile(fileName, content), throwIfNotFound: true);
        }

        /// <summary>
        /// Adds or overwrites a fully pathed file.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        public void AddOrOverwriteFile(string path, byte[] content)
        {
            CreatePathAndPerformActionOnFile(path, (folder, fileName) => folder.AddOrOverwriteFile(fileName, content));
        }

        /// <summary>
        /// FOR TEST USE. Combines the paths passed in and strips off drive specs etc.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        public string CombineInternalPath(string path, params string[] paths)
        {
            var result = new StringBuilder(ConvertToLocalDirectorySeparator(path));
            var slash = Path.DirectorySeparatorChar;

            foreach(var rawPathPart in paths) {
                var pathPart = ConvertToLocalDirectorySeparator(rawPathPart);
                var pathHasSlash = result.Length > 0 && result[result.Length - 1] == slash;
                var partHasSlash = pathPart.Length > 0 && pathPart[0] == slash;

                if(pathHasSlash && partHasSlash) {
                    result.Append(pathPart.Substring(1));
                } else if(pathHasSlash || partHasSlash) {
                    result.Append(pathPart);
                } else {
                    result.Append(slash);
                    result.Append(pathPart);
                }
            }

            return result.ToString();
        }

        private string ConvertToLocalDirectorySeparator(string pathPart)
        {
            if(Path.DirectorySeparatorChar == '\\') {
                return (pathPart ?? "").Replace('/', '\\');
            } else {
                return (pathPart ?? "").Replace('\\', '/');
            }
        }

        /// <summary>
        /// Decomposes a path into its parts. Ignores DOS drive specifiers. Accepts either forward-slashes or backslashes.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private List<string> DecomposePath(string path)
        {
            var result = new List<string>();

            if(!String.IsNullOrEmpty(path)) {
                var driveSpecIdx = path.IndexOf(':');
                if(driveSpecIdx != -1) {
                    path = path.Substring(driveSpecIdx + 1);
                }

                if(path.Length > 0 && (path[0] == '\\' || path[0] == '/')) {
                    path = path.Substring(1);
                }

                result.AddRange(path.Split('\\', '/'));
            }

            return result;
        }

        /// <summary>
        /// Creates the folders leading up to the filename part of the path and then passes the
        /// <see cref="MockFolder"/> and filename to the <paramref name="fileAction"/> passed across.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileAction"></param>
        private void CreatePathAndPerformActionOnFile(string path, Action<MockFolder, string> fileAction)
        {
            var pathParts = DecomposePath(path);
            if(pathParts.Count > 0) {
                var folder = _Root;
                foreach(var pathPart in pathParts.Take(pathParts.Count - 1)) {
                    folder = folder.CreateOrFindFolder(pathPart);
                }

                fileAction(folder, pathParts[pathParts.Count - 1]);
            }
        }

        /// <summary>
        /// Follows the folders leading up to the filename part of the path and then passes the
        /// <see cref="MockFolder"/> and filename to the <paramref name="fileAction"/> passed across.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileAction"></param>
        /// <param name="throwIfNotFound"></param>
        private void FollowPathAndPerformActionOnFile(string path, Action<MockFolder, string> fileAction, bool throwIfNotFound = false)
        {
            var pathParts = DecomposePath(path);
            if(pathParts.Count > 0) {
                var folder = _Root;
                var currentFolder = new StringBuilder();

                foreach(var pathPart in pathParts.Take(pathParts.Count - 1)) {
                    currentFolder.Append($"{Path.DirectorySeparatorChar}{pathPart}");

                    folder = folder.FindFolder(pathPart);
                    if(folder == null) {
                        if(throwIfNotFound) {
                            throw new DirectoryNotFoundException($"{currentFolder} cannot be found");
                        } else {
                            break;
                        }
                    }
                }

                if(folder != null) {
                    fileAction(folder, pathParts[pathParts.Count - 1]);
                }
            }
        }

        /// <summary>
        /// Follows the folders for the entire path and then passes the <see cref="MockFolder"/> to the
        /// <paramref name="folderAction"/> passed across.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="folderAction"></param>
        /// <param name="throwIfNotFound"></param>
        private void FollowPathAndPerformActionOnFolder(string path, Action<MockFolder> folderAction, bool throwIfNotFound = false)
        {
            var pathParts = DecomposePath(path);
            if(pathParts.Count > 0) {
                var folder = _Root;
                var currentFolder = new StringBuilder();

                foreach(var pathPart in pathParts) {
                    currentFolder.Append($"{Path.DirectorySeparatorChar}{pathPart}");

                    folder = folder.FindFolder(pathPart);
                    if(folder == null) {
                        if(throwIfNotFound) {
                            throw new DirectoryNotFoundException($"{currentFolder} cannot be found");
                        } else {
                            break;
                        }
                    }
                }

                if(folder != null) {
                    folderAction(folder);
                }
            }
        }

        /// <summary>
        /// Returns the file at the path specified.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="throwIfNotFound"></param>
        /// <returns></returns>
        private MockFile FindFile(string path, bool throwIfNotFound = false)
        {
            MockFile result = null;

            FollowPathAndPerformActionOnFile(path, (folder, fileName) => {
                result = folder.FindFile(fileName);
            }, throwIfNotFound);

            if(result == null && throwIfNotFound) {
                throw new FileNotFoundException($"{path} cannot be found");
            }

            return result;
        }

        /// <summary>
        /// Returns the folder at the path specified.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="throwIfNotFound"></param>
        /// <returns></returns>
        private MockFolder FindFolder(string path, bool throwIfNotFound = false)
        {
            MockFolder result = null;

            FollowPathAndPerformActionOnFolder(path, folder => {
                result = folder;
            }, throwIfNotFound);

            if(result == null && throwIfNotFound) {
                throw new DirectoryNotFoundException($"{path} cannot be found");
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool DirectoryExists(string path)
        {
            return FindFolder(path, throwIfNotFound: false) != null;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool FileExists(string fileName)
        {
            return FindFile(fileName, throwIfNotFound: false) != null;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public byte[] FileReadAllBytes(string fileName)
        {
            var mockFile = FindFile(fileName, throwIfNotFound: true);
            return mockFile.Content;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public long FileSize(string fileName)
        {
            var mockFile = FindFile(fileName, throwIfNotFound: true);
            return mockFile.Content.LongLength;
        }
    }
}
