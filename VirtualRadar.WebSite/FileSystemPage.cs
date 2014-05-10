// Copyright © 2013 onwards, Andrew Whewell
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
using System.Net;
using System.Text;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// A class that can serve pages from the file system.
    /// </summary>
    class FileSystemPage : Page
    {
        #region Private class - RequestFile
        /// <summary>
        /// Describes a file that is going to be served by the page.
        /// </summary>
        class RequestFile
        {
            /// <summary>
            /// Gets or sets the root folder that the file was found in.
            /// </summary>
            public Root Root { get; set; }

            /// <summary>
            /// Gets or sets the full path to the file to serve.
            /// </summary>
            public string FileName { get; set; }

            /// <summary>
            /// Creates a new object.
            /// </summary>
            /// <param name="root"></param>
            /// <param name="fileName"></param>
            public RequestFile(Root root, string fileName)
            {
                Root = root;
                FileName = fileName;
            }

            /// <summary>
            /// See base docs.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return String.Format("{0} in {1}", FileName, Root.Folder);
            }
        }

        class SiteRootDetail : SiteRoot
        {
            public Dictionary<string, ChecksumFileEntry> RequestPathMap { get; private set; }
        }
        #endregion

        #region Private class - Root
        /// <summary>
        /// A private class that describes a root folder that we are serving files from.
        /// </summary>
        class Root
        {
            /// <summary>
            /// Gets or sets the <see cref="SiteRoot"/> that represents the folder.
            /// </summary>
            public SiteRoot SiteRoot { get; set; }

            /// <summary>
            /// Gets the original folder from <see cref="SiteRoot"/>.
            /// </summary>
            public string Folder { get; set; }

            /// <summary>
            /// Gets the original priority from <see cref="SiteRoot"/>.
            /// </summary>
            public int Priority { get; set; }

            /// <summary>
            /// Gets a map of checksum entries keyed by normalised request paths for files.
            /// </summary>
            public Dictionary<string, ChecksumFileEntry> RequestChecksumMap { get; private set; }

            /// <summary>
            /// Gets a value indicating that modified files are not to be served from this folder.
            /// </summary>
            public bool IsProtectedContent { get { return RequestChecksumMap.Count > 0; } }

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

                RequestChecksumMap = new Dictionary<string,ChecksumFileEntry>();
                foreach(var checksum in siteRoot.Checksums) {
                    RequestChecksumMap.Add(NormaliseRequestPath(checksum.FileName), checksum);
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
                    if(!requestPathIsNormalised) requestPath = NormaliseRequestPath(requestPath);
                    RequestChecksumMap.TryGetValue(requestPath, out result);
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
                        var checksum = ChecksumFileEntry.GenerateChecksum(fileName);
                        var length = ChecksumFileEntry.GetFileSize(fileName);
                        result = checksum == entry.Checksum && length == entry.FileSize;
                    }
                }

                return result;
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The list of file roots that this object serves pages from.
        /// </summary>
        private List<Root> _Roots = new List<Root>();
        #endregion

        #region Properties
        /// <summary>
        /// The root folder of the site laid down by the installer. The site here cannot be modified.
        /// </summary>
        private string _DefaultRootFolder;
        private string DefaultRootFolder
        {
            get
            {
                if(_DefaultRootFolder == null) {
                    var runtime = Factory.Singleton.Resolve<IRuntimeEnvironment>().Singleton;
                    _DefaultRootFolder = Path.Combine(runtime.ExecutablePath, "Web");
                }
                return _DefaultRootFolder;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public FileSystemPage(WebSite webSite) : base(webSite)
        {
            var runtime = Factory.Singleton.Resolve<IRuntimeEnvironment>().Singleton;
            var defaultSiteRoot = new SiteRoot() {
                Folder = String.Format("{0}{1}", Path.Combine(runtime.ExecutablePath, "Web"), Path.DirectorySeparatorChar),
                Priority = 0,
            };
            defaultSiteRoot.Checksums.AddRange(ChecksumFile.Load(SiteResources.Checksums));
            AddSiteRoot(defaultSiteRoot);
        }
        #endregion

        #region NormaliseRequestPath
        /// <summary>
        /// Returns a normalised version of a request path.
        /// </summary>
        /// <param name="requestPath"></param>
        /// <returns></returns>
        private static string NormaliseRequestPath(string requestPath)
        {
            return (requestPath ?? "").ToLower().Replace("\\", "/");
        }
        #endregion

        #region AddSiteRoot, RemoveSiteRoot
        /// <summary>
        /// See interface docs for <see cref="IWebSite"/>.
        /// </summary>
        /// <param name="siteRoot"></param>
        public void AddSiteRoot(SiteRoot siteRoot)
        {
            if(siteRoot == null) throw new ArgumentNullException("siteRoot");
            if(String.IsNullOrEmpty(siteRoot.Folder)) throw new ArgumentException("siteRoot.Folder");
            if(!Directory.Exists(siteRoot.Folder)) throw new InvalidOperationException(String.Format("{0} does not exist", siteRoot.Folder));
            if(!Path.IsPathRooted(siteRoot.Folder)) throw new InvalidOperationException(String.Format("{0} is a relative path - only absolute paths can be site roots", siteRoot.Folder));

            var folder = NormalisePath(siteRoot.Folder);
            if(_Roots.Any(r => r.Folder.Equals(folder, StringComparison.OrdinalIgnoreCase))) throw new InvalidOperationException(String.Format("{0} is already a site root", siteRoot.Folder));

            _Roots.Add(new Root(folder, siteRoot));
            _Roots.Sort((lhs, rhs) => { return lhs.Priority < rhs.Priority ? -1 : lhs.Priority == rhs.Priority ? 0 : 1; });
        }

        /// <summary>
        /// Flattens the path passed across and adjusts path separators so that they're treated consistently.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        private string NormalisePath(string folder)
        {
            folder = Path.GetFullPath(folder ?? "");
            if(Path.AltDirectorySeparatorChar != Path.DirectorySeparatorChar) folder = folder.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            if(folder[folder.Length - 1] != Path.DirectorySeparatorChar) folder += Path.DirectorySeparatorChar;

            return folder;
        }

        /// <summary>
        /// See interface docs for <see cref="IWebSite"/>.
        /// </summary>
        /// <param name="siteRoot"></param>
        public void RemoveSiteRoot(SiteRoot siteRoot)
        {
            var root = _Roots.SingleOrDefault(r => r.SiteRoot == siteRoot);
            if(root != null) _Roots.Remove(root);
        }

        /// <summary>
        /// See interface docs for <see cref="IWebSite"/>.
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
        /// See interface docs for <see cref="IWebSite"/>.
        /// </summary>
        /// <returns></returns>
        public List<string> GetSiteRootFolders()
        {
            return _Roots.Select(r => r.Folder).ToList();
        }
        #endregion

        #region DoHandleRequest
        /// <summary>
        /// See base class docs.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected override bool DoHandleRequest(IWebServer server, RequestReceivedEventArgs args)
        {
            var requestFile = GetRequestFile(args.PathParts, args.File);

            var result = requestFile != null && !String.IsNullOrEmpty(requestFile.FileName) && File.Exists(requestFile.FileName);
            if(result) {
                var normalisedRequestPath = NormaliseRequestPath(args.PathAndFile);
                var extension = Path.GetExtension(requestFile.FileName);
                var isProtected = requestFile.Root.IsProtectedContent;

                var checksumEntry = isProtected ? requestFile.Root.FindChecksum(normalisedRequestPath, requestPathIsNormalised: true) : null;
                result = !isProtected || checksumEntry != null;
                if(result) {
                    var isHtml = ".html".Equals(extension, StringComparison.OrdinalIgnoreCase) || ".htm".Equals(extension, StringComparison.OrdinalIgnoreCase);

                    if(isProtected && !requestFile.Root.TestChecksum(checksumEntry, requestFile.FileName)) {
                        Factory.Singleton.Resolve<ILog>().Singleton.WriteLine("Will not serve {0}, it has failed the checksum test", args.PathAndFile);
                        if(!isHtml) {
                            args.Response.StatusCode = HttpStatusCode.BadRequest;
                        } else {
                            Responder.SendText(args.Request, args.Response, "<HTML><HEAD><TITLE>No</TITLE></HEAD><BODY>VRS will not serve content that has been tampered with. Install the custom content plugin if you want to alter the site's files.</BODY></HTML>", Encoding.UTF8, MimeType.Html);
                            args.Classification = ContentClassification.Html;
                        }
                    } else {
                        if(isHtml) {
                            ModifyAndSendContent(args, requestFile.FileName, extension, r => {
                                _WebSite.InjectHtmlContent(args.PathAndFile, r);
                                _WebSite.BundleHtml(args.PathAndFile, r);
                            });
                        } else if(".js".Equals(extension, StringComparison.OrdinalIgnoreCase)) {
                            ModifyAndSendContent(args, requestFile.FileName, extension, r => {
                                _WebSite.MinifyJavaScript(r);
                            });
                        } else if(".css".Equals(extension, StringComparison.OrdinalIgnoreCase)) {
                            ModifyAndSendContent(args, requestFile.FileName, extension, r => {
                                _WebSite.MinifyCss(r);
                            });
                        } else {
                            args.Response.MimeType = MimeType.GetForExtension(extension);

                            var enableCompression = true;
                            if(args.Response.MimeType == MimeType.IconImage) enableCompression = false;
                            else if(args.Response.MimeType.StartsWith("image/")) enableCompression = false;

                            if(enableCompression) args.Response.EnableCompression(args.Request);
                            args.Response.ContentLength = new FileInfo(requestFile.FileName).Length;
                            args.Classification = MimeType.GetContentClassification(args.Response.MimeType);
                            args.Response.StatusCode = HttpStatusCode.OK;
                            using(var fileStream = new FileStream(requestFile.FileName, FileMode.Open, FileAccess.Read)) {
                                StreamHelper.CopyStream(fileStream, args.Response.OutputStream, 4096);
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Reads the content of the text file passed across, modifies it and then sends it back to the browser.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="fileName"></param>
        /// <param name="extension"></param>
        /// <param name="modifyContent"></param>
        private void ModifyAndSendContent(RequestReceivedEventArgs args, string fileName, string extension, Action<TextContent> modifyContent)
        {
            var textContent = LoadTextFileWithBom(fileName);
            modifyContent(textContent);

            var bytes = textContent.GetBytes();
            args.Response.EnableCompression(args.Request);
            args.Response.ContentLength = bytes.Length;
            args.Response.MimeType = MimeType.GetForExtension(extension);
            args.Classification = MimeType.GetContentClassification(args.Response.MimeType);
            args.Response.StatusCode = HttpStatusCode.OK;
            using(var stream = new MemoryStream(bytes)) {
                StreamHelper.CopyStream(stream, args.Response.OutputStream);
            }
        }

        /// <summary>
        /// Loads the text content of a file while recording information about any byte order marks in the file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <remarks>File.ReadAllText strips off BOM preambles which will cause us problems - we want those sent to the browser.
        /// This doesn't try to be exhaustive, for our purposes that isn't necessary. Most (all?) files served by VRS will be
        /// UTF-8, it's just user files that might be in some other encoding.</remarks>
        private TextContent LoadTextFileWithBom(string fileName)
        {
            var result = new TextContent() {
                Encoding = Encoding.Default,
            };

            using(var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read)) {
                using(var streamReader = new StreamReader(fileStream, detectEncodingFromByteOrderMarks: true)) {
                    var currentStreamPosition = fileStream.Position;
                    var preamble = streamReader.CurrentEncoding.GetPreamble();
                    if(preamble.Length > 0 && fileStream.Length >= preamble.Length) {
                        fileStream.Position = 0;
                        var filePreamble = new byte[preamble.Length];
                        var bytesRead = fileStream.Read(filePreamble, 0, filePreamble.Length);
                        result.HadPreamble = bytesRead == preamble.Length && preamble.SequenceEqual(filePreamble);
                        fileStream.Position = currentStreamPosition;
                    }
                    result.Encoding = streamReader.CurrentEncoding;
                    result.Content = streamReader.ReadToEnd();
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the full path to the file corresponding to the path parts and filename passed across.
        /// </summary>
        /// <param name="pathParts"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private RequestFile GetRequestFile(List<string> pathParts, string fileName)
        {
            RequestFile result = null;

            var fileNameHasInvalidCharacters = fileName.Intersect(Path.GetInvalidFileNameChars()).Any();
            var pathHasInvalidCharacters = false;
            foreach(var pathPart in pathParts) {
                pathHasInvalidCharacters = pathPart.Intersect(Path.GetInvalidPathChars()).Any();
                if(pathHasInvalidCharacters) break;
            }

            if(!fileNameHasInvalidCharacters && !pathHasInvalidCharacters) {
                foreach(var root in _Roots) {
                    var folder = root.Folder;
                    foreach(var pathPart in pathParts) {
                        folder = Path.Combine(folder, pathPart);
                    }
                    if(Directory.Exists(folder)) {
                        folder = Path.GetFullPath(folder);
                        var isSiteFolder = folder.ToUpper().StartsWith(root.Folder.ToUpper());
                        if(isSiteFolder) {
                            var fullPath = Path.Combine(folder, fileName);
                            if(File.Exists(fullPath)) {
                                result = new RequestFile(root, fullPath);
                                break;
                            }
                        }
                    }
                }
            }

            return result;
        }
        #endregion
    }
}
