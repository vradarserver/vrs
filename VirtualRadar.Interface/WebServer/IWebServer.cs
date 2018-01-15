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
using System.Collections.ObjectModel;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Interface.WebServer
{
    /// <summary>
    /// The interface for objects that can serve requests for web pages.
    /// </summary>
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
        /// Gets or sets a value indicating that the server is online.
        /// </summary>
        bool Online { get; set; }
        #endregion

        #region Events
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

        /// <summary>
        /// Raised after a response has been sent and the request completely closed down. The parameter is
        /// the UniqueId of the <see cref="ResponseSentEventArgs"/> original created for the request.
        /// </summary>
        event EventHandler<EventArgs<long>> RequestFinished;
        #endregion

        #region GetAdministratorPaths, AddAdministratorPath, RemoveAdministratorPath, GetRestrictedPaths, AddRestrictedPath
        /// <summary>
        /// Returns the paths that have been marked as requiring authentication.
        /// </summary>
        /// <returns></returns>
        string[] GetAdministratorPaths();

        /// <summary>
        /// Tells the server that all access to this path must be authenticated and that the user must be configured as
        /// an administrator.
        /// </summary>
        /// <param name="pathFromRoot"></param>
        void AddAdministratorPath(string pathFromRoot);

        /// <summary>
        /// Tells the server that anyone can now access this path.
        /// </summary>
        /// <param name="pathFromRoot"></param>
        void RemoveAdministratorPath(string pathFromRoot);

        /// <summary>
        /// Returns a map of restricted paths to the <see cref="Access"/> describing which IPAddresses can access the path.
        /// </summary>
        /// <returns></returns>
        IDictionary<string, Access> GetRestrictedPathsMap();

        /// <summary>
        /// Sets access on a restricted path. If <paramref name="access"/> is null then the restrictions are removed
        /// from the path.
        /// </summary>
        /// <param name="pathFromRoot"></param>
        /// <param name="access"></param>
        void SetRestrictedPath(string pathFromRoot, Access access);
        #endregion
    }
}
