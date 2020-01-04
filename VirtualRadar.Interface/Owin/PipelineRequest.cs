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
using System.Net;
using Microsoft.Owin;

namespace VirtualRadar.Interface.Owin
{
    /// <summary>
    /// Extends an OwinRequest object.
    /// </summary>
    [Obsolete("Use AWhewell.Owin.Utility.OwinContext")]
    public class PipelineRequest
    {
        private AWhewell.Owin.Utility.OwinContext _Context;

        /// <summary>
        /// As per the OwinRequest.Path except an empty string path is returned as /.
        /// </summary>
        public PathString PathNormalised => new PathString(_Context.RequestPathNormalised);

        /// <summary>
        /// As per OwinRequest.Path except a string is returned, directory traversal characters are parsed out and
        /// an empty or null path is returned as /.
        /// </summary>
        public string FlattenedPath => _Context.RequestPathFlattened;

        /// <summary>
        /// The filename portion of the request URL. If no filename has been specified then an empty string is returned.
        /// </summary>
        public string FileName
        {
            get {
                var path = FlattenedPath;
                var lastFolderIndex = path.LastIndexOf('/');
                return lastFolderIndex == -1 ? "" : path.Substring(lastFolderIndex + 1);
            }
        }

        /// <summary>
        /// All of the portions of the request URL that were separated by forward-slash. If the URL was not terminated with
        /// a forward-slash then the final portion (which is assumed to be the filename) is not included.
        /// </summary>
        /// <remarks>
        /// For example, if the URL is '/folder/filename' then this will return an array of one string containing 'folder'. However,
        /// if the URL is '/folder/subfolder/' then you will get two strings back - 'folder' and 'subfolder'.
        /// </remarks>
        public string[] PathParts => _Context.RequestPathParts;

        /// <summary>
        /// Gets a value derived from the user-agent which indicates that the request MIGHT be from a mobile device.
        /// </summary>
        public bool IsMobileUserAgentString => _Context.RequestHeadersDictionary.UserAgentValue.IsMobileUserAgentString;

        /// <summary>
        /// Gets a value derived from the user-agent which indicates that the request MIGHT be from a tablet.
        /// </summary>
        public bool IsTabletUserAgentString => _Context.RequestHeadersDictionary.UserAgentValue.IsTabletUserAgentString;

        /// <summary>
        /// Gets a value indicating that the request came from the LAN or was local.
        /// </summary>
        public bool IsLocalOrLan => _Context.IsLocalOrLan;

        /// <summary>
        /// Gets a value indicating that the request came from the Internet. Shorthand for !IsLocalOrLan.
        /// </summary>
        public bool IsInternet => _Context.IsInternet;

        /// <summary>
        /// Gets the IP address of the machine that made the request on the server. If the request
        /// came from a proxy server on the LAN then it is the address of the machine that accesssed
        /// the proxy server, as opposed to RequestIpAddress which would be the address of the proxy
        /// server.
        /// </summary>
        public string ClientIpAddress => _Context.ClientIpAddress;

        /// <summary>
        /// Gets the <see cref="ClientIpAddress"/> parsed into a System.Net IPAddress.
        /// </summary>
        public IPAddress ClientIpAddressParsed => _Context.ClientIpAddressParsed;

        /// <summary>
        /// Gets the address of the reverse proxy that the request came through, if known.
        /// </summary>
        public string ProxyIpAddress => _Context.ProxyIpAddress;

        /// <summary>
        /// Gets or sets the user agent from the request.
        /// </summary>
        public string UserAgent => _Context.RequestHeadersDictionary.UserAgent;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PipelineRequest() : base()
        {
            _Context = new AWhewell.Owin.Utility.OwinContext();
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="environment"></param>
        public PipelineRequest(IDictionary<string, object> environment)
        {
            _Context = AWhewell.Owin.Utility.OwinContext.Create(environment);
        }
    }
}
