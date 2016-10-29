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
using System.ComponentModel;
using System.Linq.Expressions;
using System.Collections.ObjectModel;
using VirtualRadar.Interface.PortableBinding;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// A class that holds the configuration of the web server.
    /// </summary>
    public class WebServerSettings : INotifyPropertyChanged
    {
        private AuthenticationSchemes _AuthenticationScheme;
        /// <summary>
        /// Gets or sets the authentication scheme that the server will employ for new connections.
        /// </summary>
        public AuthenticationSchemes AuthenticationScheme
        {
            get { return _AuthenticationScheme; }
            set { SetField(ref _AuthenticationScheme, value, nameof(AuthenticationScheme)); }
        }

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

        private bool _ConvertedUser;
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
        public bool ConvertedUser
        {
            get { return _ConvertedUser; }
            set { SetField(ref _ConvertedUser, value, nameof(ConvertedUser)); }
        }

        private NotifyList<string> _BasicAuthenticationUserIds = new NotifyList<string>();
        /// <summary>
        /// Gets the list of users that can log onto the site when Basic Authentication is
        /// switched on.
        /// </summary>
        /// <remarks>
        /// The site will also allow any users listed in <see cref="AdministratorUserIds"/>
        /// to log in, even if they are not present in this list.
        /// </remarks>
        public NotifyList<string> BasicAuthenticationUserIds
        {
            get { return _BasicAuthenticationUserIds; }
        }

        private NotifyList<string> _AdministratorUserIds = new NotifyList<string>();
        /// <summary>
        /// Gets the list of users that are considered to be administrators of the site.
        /// </summary>
        /// <remarks>
        /// Administrators can automatically do anything a user can do, they do not need to be listed
        /// both here and <see cref="BasicAuthenticationUserIds"/>.
        /// </remarks>
        public NotifyList<string> AdministratorUserIds
        {
            get { return _AdministratorUserIds; }
        }

        private bool _EnableUPnp;
        /// <summary>
        /// Gets or sets a value indicating that the server is allowed to control UPnP NAT routers.
        /// </summary>
        public bool EnableUPnp
        {
            get { return _EnableUPnp; }
            set { SetField(ref _EnableUPnp, value, nameof(EnableUPnp)); }
        }

        private int _UPnpPort;
        /// <summary>
        /// Gets or sets the port number that the UPnP NAT router will listen on for traffic to forward to VRS.
        /// </summary>
        public int UPnpPort
        {
            get { return _UPnpPort; }
            set { SetField(ref _UPnpPort, value, nameof(UPnpPort)); }
        }

        private bool _IsOnlyInternetServerOnLan;
        /// <summary>
        /// Gets or sets a value indicating that this is the only instance of VRS on the LAN that is allowed
        /// to respond to requests from the Internet.
        /// </summary>
        public bool IsOnlyInternetServerOnLan
        {
            get { return _IsOnlyInternetServerOnLan; }
            set { SetField(ref _IsOnlyInternetServerOnLan, value, nameof(IsOnlyInternetServerOnLan)); }
        }

        private bool _AutoStartUPnP;
        /// <summary>
        /// Gets or sets a value indicating that server should automatically go onto the Internet when the program first starts up.
        /// </summary>
        public bool AutoStartUPnP
        {
            get { return _AutoStartUPnP; }
            set { SetField(ref _AutoStartUPnP, value, nameof(AutoStartUPnP)); }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            var handler = PropertyChanged;
            if(handler != null) {
                handler(this, args);
            }
        }

        /// <summary>
        /// Sets the field's value and raises <see cref="PropertyChanged"/>, but only when the value has changed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="fieldName"></param>
        /// <returns>True if the value was set because it had changed, false if the value did not change and the event was not raised.</returns>
        protected bool SetField<T>(ref T field, T value, string fieldName)
        {
            var result = !EqualityComparer<T>.Default.Equals(field, value);
            if(result) {
                field = value;
                OnPropertyChanged(new PropertyChangedEventArgs(fieldName));
            }

            return result;
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public WebServerSettings()
        {
            AuthenticationScheme = AuthenticationSchemes.Anonymous;
            UPnpPort = 80;
            IsOnlyInternetServerOnLan = true;

            BasicAuthenticationUserIds.ListChanged += BasicAuthenticationUserIds_ListChanged;
            AdministratorUserIds.ListChanged += AdministratorUserIds_ListChanged;
        }

        private void BasicAuthenticationUserIds_ListChanged(object sender, ListChangedEventArgs args)
        {
            if(args.ListChangedType != ListChangedType.ItemChanged) {
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(BasicAuthenticationUserIds)));
            }
        }

        void AdministratorUserIds_ListChanged(object sender, ListChangedEventArgs args)
        {
            if(args.ListChangedType != ListChangedType.ItemChanged) {
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(AdministratorUserIds)));
            }
        }
    }
}
