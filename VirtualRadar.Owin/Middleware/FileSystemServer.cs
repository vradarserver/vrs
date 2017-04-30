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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualRadar.Interface.Owin;

namespace VirtualRadar.Owin.Middleware
{
    using System.IO;
    using System.Net;
    using InterfaceFactory;
    using VirtualRadar.Interface.WebServer;
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
                    result = Factory.Singleton.Resolve<IFileSystemProvider>();
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

        private IFileSystemConfiguration _Configuration;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public FileSystemServer()
        {
            _Configuration = Factory.Singleton.Resolve<IFileSystemConfiguration>().Singleton;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public AppFunc HandleRequest(AppFunc next)
        {
            AppFunc appFunc = async(IDictionary<string, object> environment) => {
                ServeFromFileSystem(environment);

                await next.Invoke(environment);
            };

            return appFunc;
        }

        /// <summary>
        /// Honours requests for files from the file system.
        /// </summary>
        /// <param name="environment"></param>
        private void ServeFromFileSystem(IDictionary<string, object> environment)
        {
            var context = PipelineContext.GetOrCreate(environment);
            var request = context.Request;
            var response = context.Response;

            var relativePath = ConvertRequestPathToRelativeFilePath(request.FlattenedPath);

            if(!String.IsNullOrEmpty(relativePath)) {
                foreach(var siteRoot in _Configuration.GetSiteRootFolders()) {
                    var fullPath = Path.Combine(siteRoot, relativePath);

                    if(FileSystemProvider.FileExists(fullPath)) {
                        var extension = Path.GetExtension(fullPath);
                        var mimeType = MimeType.GetForExtension(extension) ?? "application/octet-stream";

                        var content = FileSystemProvider.FileReadAllBytes(fullPath);
                        response.ContentLength = content.Length;
                        response.Body.Write(content, 0, content.Length);
                        response.StatusCode = (int)HttpStatusCode.OK;
                        response.ContentType = mimeType;
                    }
                }
            }
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
    }
}
