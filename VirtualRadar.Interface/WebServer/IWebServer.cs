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
using System.Net;

namespace VirtualRadar.Interface.WebServer
{
    /// <summary>
    /// The interface for objects that can serve requests for web pages.
    /// </summary>
    /// <remarks><para>
    /// Once the properties have been set the server is controlled through the <see cref="Online"/> property. Setting it to
    /// true starts the server, setting it to false takes it offline.
    /// </para><para>
    /// When a request is received from a browser the <see cref="RequestReceived"/> event is raised. This is passed a <see cref="RequestReceivedEventArgs"/> object.
    /// If the handler(s) do not set the <see cref="RequestReceivedEventArgs.Handled"/> property of the args then the server responds with a "Content not found"
    /// error, otherwise the content as described by <see cref="RequestReceivedEventArgs.Response"/> property is sent to the browser.
    /// </para><para>
    /// The web server supports authentication. When authentication is required the <see cref="AuthenticationScheme"/> property should be set to a value that is
    /// not Anonymous. The server will raise <see cref="AuthenticationRequired"/> and pass an <see cref="AuthenticationRequiredEventArgs"/>. Setting the
    /// <see cref="AuthenticationRequiredEventArgs.IsAuthenticated"/> property will allow the request to proceed. The <see cref="CacheCredentials"/> property can
    /// be used to prevent the event being raised for credentials that the server has previously seen, and which were previously confirmed to be good by an event
    /// handler.
    /// </para><para>
    /// If the implementation of IWebServer is using the .NET Framework HttpListener then be aware that this will not run without admin privileges under Windows 7
    /// or Vista, unless you first run a NETSH command to give the application permission to listen to a particular root and port. The installer for Virtual Radar
    /// Server sets this permission up so that Virtual Radar Server can remain at normal user permissions, if you use the server for your own applications you
    /// may need to do the same or ask the user to run your application as an administrator. It is not guaranteed, however, that the implementation is using HttpListener.
    /// </para></remarks>
    public interface IWebServer : IBackgroundThreadExceptionCatcher, IDisposable
    {
        #region Properties
        /// <summary>
        /// Gets or sets the provider used to wrap bits of the .NET framework.
        /// </summary>
        IWebServerProvider Provider { get; set; }

        /// <summary>
        /// Gets or sets the root of the web site that the server will listen to.
        /// </summary>
        /// <remarks>
        /// The root is the full path from the address of the machine to the root of the site, e.g. /Foobar will be
        /// the root in the address http://127.0.0.1/Foobar. A leading slash and no trailing slash is enforced.
        /// Defaults to /.
        /// </remarks>
        string Root { get; set; }

        /// <summary>
        /// Gets or sets the port number to listen to.
        /// </summary>
        /// <remarks>
        /// Defaults to 80.
        /// </remarks>
        int Port { get; set; }

        /// <summary>
        /// Gets the prefix that describes the combination of root and port.
        /// </summary>
        /// <remarks>
        /// See http://msdn.microsoft.com/en-us/library/aa364698(VS.85).aspx for a description of prefix strings.
        /// </remarks>
        string Prefix { get; }

        /// <summary>
        /// Gets the <see cref="Port"/> number as a string suitable for use in a URL.
        /// </summary>
        /// <remarks>
        /// If <see cref="Port"/> is 80 then it returns an empty string, otherwise it returns ":nnn" where nnn
        /// is the port number.
        /// </remarks>
        string PortText { get; }

        /// <summary>
        /// Gets the local address of the web server (e.g. http://127.0.0.1:8090/Root).
        /// </summary>
        string LocalAddress { get; }

        /// <summary>
        /// Gets the address of the web server on the LAN (e.g. http://192.168.0.1:8090/Root).
        /// </summary>
        string NetworkAddress { get; }

        /// <summary>
        /// Gets the IP address of the current machine on the LAN (e.g. 192.168.0.1).
        /// </summary>
        string NetworkIPAddress { get; }

        /// <summary>
        /// Gets the address of the web server on the public Internet (e.g. http://12.13.14.15:8080/Root).
        /// </summary>
        /// <remarks>
        /// This is derived from <see cref="ExternalIPAddress"/> and <see cref="ExternalPort"/>.
        /// </remarks>
        string ExternalAddress { get; }

        /// <summary>
        /// Gets or sets the IP address of the local machine on the public internet. Optional, used to build <see cref="ExternalAddress"/>.
        /// </summary>
        string ExternalIPAddress { get; set; }

        /// <summary>
        /// Gets or sets the port number that public Internet clients can connect to in order to reach this machine. Optional, used to
        /// build <see cref="ExternalAddress"/>.
        /// </summary>
        int ExternalPort { get; set; }

        /// <summary>
        /// Gets or sets the authentication scheme to use.
        /// </summary>
        /// <remarks>
        /// Defaults to AuthenticationSchemes.None. Only None, Anonymous and Basic are currently supported.
        /// </remarks>
        AuthenticationSchemes AuthenticationScheme { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that authentication credentials can be cached by the server.
        /// </summary>
        /// <remarks>
        /// If this is set then <see cref="AuthenticationRequired"/> will only be raised if <see cref="AuthenticationScheme"/>
        /// is not 'None' and the combination of username and password sent by the browser has never previously been confirmed
        /// as valid. Invalid combinations are not cached.
        /// </remarks>
        bool CacheCredentials { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the server is online.
        /// </summary>
        bool Online { get; set; }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the <see cref="AuthenticationScheme"/> is not 'None' and the browser has supplied credentials that need testing.
        /// </summary>
        event EventHandler<AuthenticationRequiredEventArgs> AuthenticationRequired;

        /// <summary>
        /// Raised when the <see cref="ExternalAddress"/> property changes.
        /// </summary>
        event EventHandler ExternalAddressChanged;

        /// <summary>
        /// Raised when the <see cref="Online"/> property changes state.
        /// </summary>
        event EventHandler OnlineChanged;

        /// <summary>
        /// Raised on a background thread when the web server receives a request from a browser. Called
        /// before <see cref="RequestReceived"/> is raised.
        /// </summary>
        /// <remarks>
        /// This event allows plugins to intercept web requests before the main web site sees them. The
        /// main web site only hooks <see cref="RequestReceived"/>, if the plugin hooks this request and
        /// then sets <see cref="RequestReceivedEventArgs.Handled"/> to true it will prevent the main web
        /// site from attempting to honour that request.
        /// </remarks>
        event EventHandler<RequestReceivedEventArgs> BeforeRequestReceived;

        /// <summary>
        /// Raised on a background thread when the web server receives a request from a browser.
        /// </summary>
        /// <remarks>
        /// This is raised even if an event handler for <see cref="BeforeRequestReceived"/> flagged the
        /// request as having been handled. However the default web site will ignore requests that have
        /// been handled.
        /// </remarks>
        event EventHandler<RequestReceivedEventArgs> RequestReceived;

        /// <summary>
        /// Raised on a background thread when the web server receives a request from a browser. Called
        /// after <see cref="RequestReceived"/> is raised.
        /// </summary>
        /// <remarks>
        /// This event allows plugins to fulfil web requests that they are sure have not been handled by
        /// the main web site.
        /// </remarks>
        event EventHandler<RequestReceivedEventArgs> AfterRequestReceived;

        /// <summary>
        /// Raised on a background thread when the web server responds to a request.
        /// </summary>
        event EventHandler<ResponseSentEventArgs> ResponseSent;
        #endregion

        #region ResetCredentialCache
        /// <summary>
        /// Clears the cache of credentials held by the server - see <see cref="CacheCredentials"/>.
        /// </summary>
        void ResetCredentialCache();
        #endregion
    }
}
