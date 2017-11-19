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
    /// A user record that has been cached by an instance of <see cref="IUserCache{T}"/>.
    /// </summary>
    public class CachedUser
    {
        /// <summary>
        /// Gets the user's record as loaded from the database.
        /// </summary>
        public IUser User { get; private set; }

        /// <summary>
        /// Gets a value indicating that the user is flagged as a web content user.
        /// </summary>
        public bool IsWebContentUser { get; private set; }

        /// <summary>
        /// Gets a value indicating that the user is flagged as an administrator.
        /// </summary>
        public bool IsAdministrator { get; private set; }

        /// <summary>
        /// Gets or sets extra information attached to the user when it was cached.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="isWebContentUser"></param>
        /// <param name="isAdministrator"></param>
        public CachedUser(IUser user, bool isWebContentUser, bool isAdministrator)
        {
            User = user;
            IsWebContentUser = isWebContentUser;
            IsAdministrator = isAdministrator;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return User?.LoginName ?? "";
        }
    }
}
