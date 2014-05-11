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
using System.Text;
using System.Net;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// A class that holds the configuration of the web server.
    /// </summary>
    public class WebServerSettings
    {
        /// <summary>
        /// Gets or sets the authentication scheme that the server will employ for new connections.
        /// </summary>
        public AuthenticationSchemes AuthenticationScheme { get; set; }

        /// <summary>
        /// Gets or sets the user for basic authentication.
        /// </summary>
        /// <remarks>
        /// Last used in version 2.0.2, now superceded by <see cref="BasicAuthenticationUserIds"/>.
        /// </remarks>
        public string BasicAuthenticationUser { get; set; }

        /// <summary>
        /// Gets or sets the hash of the password for the basic authentication user.
        /// </summary>
        /// <remarks>
        /// Last used in version 2.0.2, now superceded by <see cref="BasicAuthenticationUserIds"/>.
        /// </remarks>
        public Hash BasicAuthenticationPasswordHash { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the <see cref="BasicAuthenticationUser"/> and
        /// <see cref="BasicAuthenticationPasswordHash"/> have been converted to an <see cref="IUser"/>
        /// and are now managed by the <see cref="IUserManager"/>.
        /// </summary>
        /// <remarks>
        /// The user is converted the first time version 2.0.3 is run, but only if the user manager
        /// allows new users to be created. If creation is permitted then the user is added to the
        /// <see cref="BasicAuthenticationUserIds"/> list.
        /// </remarks>
        public bool ConvertedUser { get; set; }

        private List<string> _BasicAuthenticationUserIds = new List<string>();
        /// <summary>
        /// Gets the list of users that can log onto the site with Basic authentication.
        /// </summary>
        public List<string> BasicAuthenticationUserIds
        {
            get { return _BasicAuthenticationUserIds; }
        }

        /// <summary>
        /// Gets or sets a value indicating that the server is allowed to control UPnP NAT routers.
        /// </summary>
        public bool EnableUPnp { get; set; }

        /// <summary>
        /// Gets or sets the port number that the UPnP NAT router will listen on for traffic to forward to VRS.
        /// </summary>
        public int UPnpPort { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that this is the only instance of VRS on the LAN that is allowed
        /// to respond to requests from the Internet.
        /// </summary>
        public bool IsOnlyInternetServerOnLan { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that server should automatically go onto the Internet when the program first starts up.
        /// </summary>
        public bool AutoStartUPnP { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public WebServerSettings()
        {
            AuthenticationScheme = AuthenticationSchemes.Anonymous;
            UPnpPort = 80;
            IsOnlyInternetServerOnLan = true;
        }
    }
}
