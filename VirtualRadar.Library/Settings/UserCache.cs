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
using InterfaceFactory;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Library.Settings
{
    /// <summary>
    /// The default implementation of <see cref="IUserCache"/>.
    /// </summary>
    class UserCache : IUserCache
    {
        /// <summary>
        /// The object that protects writes to the fields.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// A dictionary of loaded users indexed by a (possibly) normalised version of their login name.
        /// </summary>
        private Dictionary<string, CachedUser> _Cache = new Dictionary<string, CachedUser>();

        /// <summary>
        /// The shared configuration singleton.
        /// </summary>
        private ISharedConfiguration _SharedConfiguration;

        /// <summary>
        /// The user manager singleton.
        /// </summary>
        private IUserManager _UserManager;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool LoadAllUsers { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public Action<CachedUser> TagAction { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public UserCache()
        {
            _SharedConfiguration = Factory.Singleton.Resolve<ISharedConfiguration>().Singleton;
            _SharedConfiguration.AddWeakSubscription(this);
            _UserManager = Factory.Singleton.Resolve<IUserManager>().Singleton;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="sharedConfiguration"></param>
        public void SharedConfigurationChanged(ISharedConfiguration sharedConfiguration)
        {
            Refresh();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Refresh()
        {
            var users = new Dictionary<string, CachedUser>();
            var config = _SharedConfiguration.Get();

            foreach(var user in _UserManager.GetUsers().Where(r => r.Enabled)) {
                var isAdministrator = config.WebServerSettings.AdministratorUserIds.Contains(user.UniqueId);
                var isWebContentUser = isAdministrator || config.WebServerSettings.BasicAuthenticationUserIds.Contains(user.UniqueId);

                if(LoadAllUsers || isWebContentUser) {
                    var key = NormaliseLoginName(user.LoginName);
                    if(!users.ContainsKey(key)) {
                        var cachedUser = new CachedUser(user, isWebContentUser, isAdministrator);
                        if(TagAction != null) {
                            TagAction(cachedUser);
                        }

                        users.Add(key, cachedUser);
                    }
                }
            }

            lock(_SyncLock) {
                _Cache = users;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="loginName"></param>
        /// <returns></returns>
        public CachedUser GetUser(string loginName)
        {
            var cache = _Cache;
            var key = NormaliseLoginName(loginName);

            CachedUser result;
            cache.TryGetValue(key, out result);

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="loginName"></param>
        /// <returns></returns>
        public CachedUser GetAdministrator(string loginName)
        {
            var result = GetUser(loginName);
            return result != null && result.IsAdministrator ? result : null;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="loginName"></param>
        /// <returns></returns>
        public CachedUser GetWebContentUser(string loginName)
        {
            var result = GetUser(loginName);
            return result != null && result.IsWebContentUser ? result : null;
        }

        /// <summary>
        /// Normalises login names for the dictionary keys.
        /// </summary>
        /// <param name="loginName"></param>
        /// <returns></returns>
        private string NormaliseLoginName(string loginName)
        {
            loginName = loginName ?? "";
            return _UserManager.LoginNameIsCaseSensitive ? loginName : loginName.ToLowerInvariant();
        }
    }
}
