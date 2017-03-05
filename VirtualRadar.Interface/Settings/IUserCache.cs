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

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// The interface for objects that can cache user records.
    /// </summary>
    /// <remarks>
    /// The cache is automatically reloaded when the configuration changes.
    /// </remarks>
    public interface IUserCache : ISharedConfigurationSubscriber
    {
        /// <summary>
        /// Gets or sets a flag indicating that all user records should be loaded.
        /// </summary>
        /// <remarks>
        /// If this flag is false then only users that are flagged for web content or administrator rights are loaded
        /// by the cache. Setting this flag to true causes all enabled user records to be loaded.
        /// </remarks>
        bool LoadAllUsers { get; set; }

        /// <summary>
        /// Gets or sets an action that is called whenever a cached user is created.
        /// </summary>
        /// <remarks>
        /// This is a kludge to get around the fact that the interface factory doesn't yet support generic interfaces.
        /// </remarks>
        Action<CachedUser> TagAction { get; set; }

        /// <summary>
        /// Clears the cache and then loads web content and administrator user lists using the current configuration.
        /// </summary>
        void Refresh();

        /// <summary>
        /// Retrieves a web content user from the cache by login name.
        /// </summary>
        /// <param name="loginName"></param>
        /// <returns></returns>
        CachedUser GetWebContentUser(string loginName);

        /// <summary>
        /// Retrieves an administrator user from the cache by login name.
        /// </summary>
        /// <param name="loginName"></param>
        /// <returns></returns>
        CachedUser GetAdministrator(string loginName);

        /// <summary>
        /// Retrieves a user record by name.
        /// </summary>
        /// <param name="loginName"></param>
        /// <returns></returns>
        CachedUser GetUser(string loginName);
    }
}
