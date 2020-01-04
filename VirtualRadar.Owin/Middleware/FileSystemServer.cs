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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AWhewell.Owin.Utility;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.Owin.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// The default implementation of <see cref="IFileSystemServer"/>.
    /// </summary>
    class FileSystemServer : IFileSystemServer
    {
        private IFileSystemProvider _FileSystemProvider;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IFileSystemProvider FileSystemProvider
        {
            get
            {
                var result = _FileSystemProvider;
                if(result == null) {
                    result = Factory.Resolve<IFileSystemProvider>();
                    _FileSystemProvider = result;
                }

                return result;
            }

            set
            {
                if(value != null) {
                    _FileSystemProvider = value;
                }
            }
        }

        private IFileSystemServerConfiguration _Configuration;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public FileSystemServer()
        {
            _Configuration = Factory.ResolveSingleton<IFileSystemServerConfiguration>();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public AppFunc HandleRequest(AppFunc next)
        {
            AppFunc appFunc = async(IDictionary<string, object> environment) => {
                var context = OwinContext.Create(environment);
                if(!ServeFromFileSystem(environment)) {
                    await next.Invoke(environment);
                }
            };

            return appFunc;
        }

        /// <summary>
        /// Honours requests for files from the file system. Returns false if no file matched the request.
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        private bool ServeFromFileSystem(IDictionary<string, object> environment)
        {
            var result = false;

            var context = OwinContext.Create(environment);

            var relativePath = ConvertRequestPathToRelativeFilePath(context.RequestPathFlattened);
            relativePath = RejectInvalidCharacters(relativePath);
            if(!String.IsNullOrEmpty(relativePath)) {
                foreach(var siteRoot in _Configuration.GetSiteRootFolders()) {
                    var fullPath = Path.Combine(siteRoot, relativePath);

                    if(FileSystemProvider.FileExists(fullPath)) {
                        var extension = Path.GetExtension(fullPath);
                        var mimeType = MimeType.GetForExtension(extension) ?? "application/octet-stream";
                        var content = FileSystemProvider.FileReadAllBytes(fullPath);

                        if(_Configuration.IsFileUnmodified(siteRoot, context.RequestPathFlattened, content)) {
                            content = RaiseTextLoadedFromFile(context, content, mimeType);
                            SendContent(context, content, mimeType);
                        } else {
                            if(mimeType != MimeType.Html) {
                                context.ResponseHttpStatusCode = HttpStatusCode.BadRequest;
                            } else {
                                content = Encoding.UTF8.GetBytes("<HTML><HEAD><TITLE>No</TITLE></HEAD><BODY>VRS will not serve content that has been tampered with. Install the custom content plugin if you want to alter the site's files.</BODY></HTML>");
                                SendContent(context, content, mimeType);
                            }
                        }

                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Raises the TextLoadedFromFile event on the singleton configuration object if the mime type indicates
        /// that it's a recognised text file.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="content"></param>
        /// <param name="mimeType"></param>
        /// <returns></returns>
        private byte[] RaiseTextLoadedFromFile(OwinContext context, byte[] content, string mimeType)
        {
            if(mimeType == MimeType.Css ||
               mimeType == MimeType.Html ||
               mimeType == MimeType.Javascript ||
               mimeType == MimeType.Text) {
                var textContent = TextContent.Load(content);
                var args = new TextContentEventArgs(
                    context.RequestPathFlattened,
                    textContent.Content,
                    textContent.Encoding,
                    mimeType
                );
                _Configuration.RaiseTextLoadedFromFile(args);

                textContent.Content = args.Content;
                content = textContent.GetBytes(includePreamble: true);
            }

            return content;
        }

        /// <summary>
        /// Sends content back to the caller.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="content"></param>
        /// <param name="mimeType"></param>
        private static void SendContent(OwinContext context, byte[] content, string mimeType)
        {
            context.ResponseHttpStatusCode = HttpStatusCode.OK;
            context.ReturnBytes(
                mimeType,
                content
            );
        }

        /// <summary>
        /// Converts a request path to a relative file path.
        /// </summary>
        /// <param name="requestPath"></param>
        /// <returns></returns>
        private string ConvertRequestPathToRelativeFilePath(string requestPath)
        {
            var result = requestPath;

            if(!String.IsNullOrEmpty(result)) {
                if(result[0] == '/') {
                    result = result.Substring(1);
                }

                result = result.Replace('/', Path.DirectorySeparatorChar);
            }

            return result;
        }

        /// <summary>
        /// Returns null if the path contains invalid file system characters.
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        private string RejectInvalidCharacters(string relativePath)
        {
            var result = relativePath;

            if(!String.IsNullOrEmpty(relativePath)) {
                var chunks = relativePath.Split(Path.DirectorySeparatorChar);
                for(var i = 0;result != null && i < chunks.Length;++i) {
                    var badCharacters = i + 1 < chunks.Length ? Path.GetInvalidPathChars() : Path.GetInvalidFileNameChars();
                    if(chunks[i].Any(r => badCharacters.Contains(r))) {
                        result = null;
                    }
                }
            }

            return result;
        }
    }
}
