// Copyright © 2018 onwards, Andrew Whewell
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
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Owin.Middleware
{
    /// <summary>
    /// A helper class that lets us share code between objects that need to handle basic authentication.
    /// </summary>
    class BasicAuthenticationHelper
    {
        /// <summary>
        /// Extra information stored against cached users.
        /// </summary>
        public class CachedUserTag
        {
            /// <summary>
            /// Gets or sets the last known good password for the user.
            /// </summary>
            /// <remarks>
            /// This will be null when the user is loaded from the database. The object using the cache can
            /// store a known good password here to save having to always compare hashes.
            /// </remarks>
            public string LastKnownGoodPassword { get; set; }

            /// <summary>
            /// Gets or sets the cached principal object for the user.
            /// </summary>
            public GenericPrincipal Principal { get; set; }

            /// <summary>
            /// Returns true if the password either matches <see cref="LastKnownGoodPassword"/> or is good according
            /// to the user manager. Updates <see cref="LastKnownGoodPassword"/>.
            /// </summary>
            /// <param name="userManager"></param>
            /// <param name="user"></param>
            /// <param name="password"></param>
            /// <returns></returns>
            public bool CheckPassword(IUserManager userManager, IUser user, string password)
            {
                var result = LastKnownGoodPassword != null && LastKnownGoodPassword == password;
                if(!result) {
                    result = userManager.PasswordMatches(user, password);

                    if(result) {
                        LastKnownGoodPassword = password;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// The manager that will authenticate users for us.
        /// </summary>
        private IUserManager _UserManager;

        /// <summary>
        /// A cache of all web content users and administrators. Automatically refreshes when the
        /// configuration changes.
        /// </summary>
        private IUserCache _UserCache;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public BasicAuthenticationHelper()
        {
            _UserManager = Factory.ResolveSingleton<IUserManager>();

            _UserCache = Factory.Resolve<IUserCache>();
            _UserCache.LoadAllUsers = false;
            _UserCache.TagAction = (cachedUser) => cachedUser.Tag = new CachedUserTag();
            _UserCache.Refresh();
        }

        /// <summary>
        /// Extracts the username and password from a basic Authorize header. Returns false if the header
        /// is missing or badly formatted.
        /// </summary>
        /// <param name="authorizationMime64"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool ExtractCredentials(string authorizationMime64, ref string userName, ref string password)
        {
            var result = false;

            if(!String.IsNullOrEmpty(authorizationMime64)) {
                const string basicScheme = "Basic ";
                if(authorizationMime64.StartsWith(basicScheme, StringComparison.OrdinalIgnoreCase)) {
                    var usernamePassword = authorizationMime64.Substring(basicScheme.Length).Trim();
                    var authorizationBytes = Convert.FromBase64String(usernamePassword);
                    var authorization = Encoding.UTF8.GetString(authorizationBytes);

                    var separatorPosn = authorization.IndexOf(':');
                    if(separatorPosn != -1) {
                        userName = authorization.Substring(0, separatorPosn);
                        password = authorization.Substring(separatorPosn + 1);

                        result = userName.Length > 0;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the cached user associated with the username passed across.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public CachedUser GetCachedUser(string userName)
        {
            return _UserCache.GetWebContentUser(userName);
        }

        /// <summary>
        /// Returns the cached user tab for the cached user passed across.
        /// </summary>
        /// <param name="cachedUser"></param>
        /// <returns></returns>
        public CachedUserTag GetCachedUserTag(CachedUser cachedUser)
        {
            return (CachedUserTag)cachedUser?.Tag;
        }

        /// <summary>
        /// Returns true if the username and password are valid.
        /// </summary>
        /// <param name="cachedUser"></param>
        /// <param name="cachedUserTag"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool IsPasswordValid(CachedUser cachedUser, CachedUserTag cachedUserTag, string password)
        {
            return cachedUser != null && cachedUserTag.CheckPassword(_UserManager, cachedUser.User, password);
        }

        /// <summary>
        /// Returns the principal cached against the user or creates and returns a new principal if the
        /// user has not got a cached principal.
        /// </summary>
        /// <param name="cachedUser"></param>
        /// <param name="cachedUserTag"></param>
        /// <returns></returns>
        public IPrincipal CreatePrincipal(CachedUser cachedUser, CachedUserTag cachedUserTag)
        {
            var result = cachedUserTag.Principal;

            if(result == null) {
                var roles = new List<string>();
                roles.Add(Roles.User);
                if(cachedUser.IsAdministrator) {
                    roles.Add(Roles.Admin);
                }

                result = new GenericPrincipal(
                    new GenericIdentity(cachedUser.User.LoginName, "basic"),
                    roles.ToArray()
                );

                cachedUserTag.Principal = result;
            }

            return result;
        }
    }
}
