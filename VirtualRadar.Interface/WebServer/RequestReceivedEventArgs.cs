// Copyright © 2010 onwards, Andrew Whewell
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
using System.Web;
using System.Collections.Specialized;
using System.Net;
using System.Threading;

namespace VirtualRadar.Interface.WebServer
{
    /// <summary>
    /// The arguments passed for the RequestReceived event on <see cref="IWebServer"/>.
    /// </summary>
    public class RequestReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// The counter that is used to generate values for <see cref="UniqueId"/>.
        /// </summary>
        private static long _NextUniqueId;

        /// <summary>
        /// Gets a value that uniquely identifies the request.
        /// </summary>
        public long UniqueId { get; private set; }

        /// <summary>
        /// Gets details of the page requested by the user.
        /// </summary>
        public IRequest Request { get; private set; }

        /// <summary>
        /// Gets details of the response to send back to the user.
        /// </summary>
        public IResponse Response { get; private set; }

        /// <summary>
        /// Gets the root of the website.
        /// </summary>
        public string Root { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the request originated from the Internet.
        /// </summary>
        /// <remarks>
        /// This takes into account reverse proxies when determining whether the request came from the Internet.
        /// </remarks>
        public bool IsInternetRequest { get; private set; }

        /// <summary>
        /// Gets the address of the reverse proxy that forwarded this request to the server.
        /// </summary>
        public string ProxyAddress { get; private set; }

        /// <summary>
        /// Gets the address of the machine that is accessing the server. If the request is going through a reverse
        /// proxy then it is the address of the machine that accessed the reverse proxy.
        /// </summary>
        public string ClientAddress { get; private set; }

        /// <summary>
        /// Gets the website address, as seen by the browser.
        /// </summary>
        /// <remarks>
        /// If the request URL is 'http://192.168.0.1/MyRoot/MyPage.html' then this would be
        /// 'http://192.168.0.1/MyRoot'.
        /// </remarks>
        public string WebSite { get; private set; }

        /// <summary>
        /// Gets the requested path from the root of the site.
        /// </summary>
        /// <remarks>
        /// If the server root is /Root then for the RawUrl of '/Root' this would return '/', for the RawUrl of
        /// '/Root/' it would also return '/', for '/Root/Page.htm' it would return '/Page.htm', '/Root/Folder/' it
        /// would return '/Folder/' and so on. Query strings are stripped off and escaped characters are unescaped.
        /// </remarks>
        public string PathAndFile { get; private set; }

        /// <summary>
        /// Gets the path portion of <see cref="PathAndFile"/> or '/' if the request is for a file in the root.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Gets the file portion of <see cref="PathAndFile"/> or an empty string if there is no file portion.
        /// </summary>
        public string File { get; private set; }

        /// <summary>
        /// Gets a list of the folders from the root to the file. An empty list indicates that the file is in root.
        /// </summary>
        public List<string> PathParts { get; private set; }

        /// <summary>
        /// Gets the query string portions sent with the request.
        /// </summary>
        /// <remarks>
        /// For the URL 'http://127.0.0.1/Root/File.txt?val1=10&amp;val2=20' there would be two name-value pairs in the container
        /// for val1 = 10 and val2 = 20.
        /// </remarks>
        public NameValueCollection QueryString { get; private set; }

        private bool? _IsAndroid;
        /// <summary>
        /// Gets a value indicating that the request probably came from an Android device.
        /// </summary>
        public bool IsAndroid
        {
            get
            {
                if(_IsAndroid == null) _IsAndroid = String.IsNullOrEmpty(Request.UserAgent) ? false : Request.UserAgent.Contains(" Android ");
                return _IsAndroid.Value;
            }
        }

        private bool? _IsIPad;
        /// <summary>
        /// Gets a value indicating that the request probably came from an iPad.
        /// </summary>
        public bool IsIPad
        {
            get
            {
                if(_IsIPad == null) _IsIPad = String.IsNullOrEmpty(Request.UserAgent) ? false : Request.UserAgent.Contains("(iPad;");
                return _IsIPad.Value;
            }
        }

        private bool? _IsIPhone;
        /// <summary>
        /// Gets a value indicating that the request probably came from an iPhone.
        /// </summary>
        public bool IsIPhone
        {
            get
            {
                if(_IsIPhone == null) _IsIPhone = String.IsNullOrEmpty(Request.UserAgent) ? false : Request.UserAgent.Contains("(iPhone;");
                return _IsIPhone.Value;
            }
        }

        private bool? _IsIPod;
        /// <summary>
        /// Gets a value indicating that the request probably came from an iPod.
        /// </summary>
        public bool IsIPod
        {
            get
            {
                if(_IsIPod == null) _IsIPod = String.IsNullOrEmpty(Request.UserAgent) ? false : Request.UserAgent.Contains("(iPod;");
                return _IsIPod.Value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating that an event handler somewhere has dealt with the request and produced an appropriate response.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the classification that has been assigned to the response.
        /// </summary>
        public ContentClassification Classification { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <param name="root"></param>
        public RequestReceivedEventArgs(IRequest request, IResponse response, string root)
        {
            UniqueId = Interlocked.Increment(ref _NextUniqueId);

            PathParts = new List<string>();
            PathAndFile = File = "";

            Request = request;
            Response = response;
            Root = root;
            if(String.IsNullOrEmpty(Root)) Root = "/";

            ClientAddress = request.RemoteEndPoint.Address.ToString();
            DecomposeRequestUrl(request);
            DetermineReverseProxyAndIsInternetRequest(request);

            if(QueryString == null) QueryString = HttpUtility.ParseQueryString("");
        }

        /// <summary>
        /// Breaks down the URL used to request the information into chunks that the event handlers can use to decide what to serve.
        /// </summary>
        /// <param name="request"></param>
        /// <remarks><para>
        /// .NET unescapes percent-escaped characters before it picks out query strings etc. Virtual Radar WebServer uses the URL to
        /// carry character strings, in particular they could contain encoded reserved characters such as ?, / and =. If you try
        /// to decode those strings with .NET the escaped reserved characters cause problems, although RFC1738 does say:
        /// </para><para>
        /// "On the other hand, characters that are not required to be encoded (including alphanumerics) may be encoded within
        /// the scheme-specific part of a URL, as long as they are not being used for a reserved purpose".
        /// </para><para>
        /// This would imply that .NET's default behaviour is wrong. It should break the URL down using the unescaped string and then
        /// unescape each part of the string. "As long as they are not being used for a reserved purpose" implies that reserved characters
        /// should be treated as normal characters and not for their reserved purpose if they have been escaped.  In particular a URL such
        /// as 'http://127.0.0.1/MyRoot/Folder%3FName%3DValue/Hello.txt' should be interpreted as a path of '/MyRoot/Folder?Name=Value'
        /// and no query strings - the default .NET behaviour is to resolve that as a path of '/MyRoot/Folder' and a name-value of
        /// Name = Value.
        /// </para></remarks>
        private void DecomposeRequestUrl(IRequest request)
        {
            WebSite = "";
            PathAndFile = "";
            Path = "";

            if(!String.IsNullOrEmpty(request.RawUrl) && request.RawUrl.StartsWith(Root, StringComparison.OrdinalIgnoreCase)) {
                WebSite = String.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, Root);

                var rawUrl = request.RawUrl.Substring(Root.Length);

                var querySplitPosn = rawUrl.IndexOf('?');
                var path = querySplitPosn == -1 ? rawUrl : rawUrl.Substring(0, querySplitPosn);
                var query = querySplitPosn == -1 ? "" : rawUrl.Substring(querySplitPosn + 1);

                PathAndFile = HttpUtility.UrlDecode(path);
                if(PathAndFile.Length == 0) PathAndFile = "/";

                var pathBuffer = new StringBuilder();
                var pathParts = path.Split('/');
                for(var i = 0;i < pathParts.Length;++i) {
                    var chunk = HttpUtility.UrlDecode(pathParts[i]);
                    if(i + 1 == pathParts.Length) File = chunk;
                    else if(chunk.Length > 0) {
                        PathParts.Add(chunk);
                        pathBuffer.Append('/');
                        pathBuffer.Append(chunk);
                    }
                }
                Path = pathBuffer.Length == 0 ? "/" : pathBuffer.ToString();

                QueryString = HttpUtility.ParseQueryString(query);
            }
        }

        /// <summary>
        /// Tries to work out whether the request came through a reverse proxy and, related to that, whether the request came from the Internet.
        /// </summary>
        /// <param name="request"></param>
        /// <remarks>
        /// Only reverse proxies that have a local address can influence this, if an Internet request comes with XFF headers they're just ignored.
        /// The code only checks the address of the machine that used the proxy, so if the request came through two reverse proxies on the LAN
        /// then it will always look like a local machine made the request.
        /// </remarks>
        private void DetermineReverseProxyAndIsInternetRequest(IRequest request)
        {
            IsInternetRequest = !IPEndPointHelper.IsLocalOrLanAddress(request.RemoteEndPoint);

            if(!IsInternetRequest) {
                var xForwardedFor = Request.Headers["X-Forwarded-For"];
                if(xForwardedFor != null) {
                    ProxyAddress = request.RemoteEndPoint.Address.ToString();
                    ClientAddress = xForwardedFor.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Last().Trim();

                    IPAddress clientIPAddress;
                    if(IPAddress.TryParse(ClientAddress, out clientIPAddress)) {
                        IsInternetRequest = !IPEndPointHelper.IsLocalOrLanAddress(new IPEndPoint(clientIPAddress, request.RemoteEndPoint.Port));
                    } else {
                        ClientAddress = request.RemoteEndPoint.Address.ToString();
                        IsInternetRequest = true;
                    }
                }
            }
        }
    }
}
