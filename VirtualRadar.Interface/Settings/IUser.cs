// Copyright © 2014 onwards, Andrew Whewell
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
using System.ComponentModel;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// The interface that describes a user.
    /// </summary>
    public interface IUser : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the user's unique identifier.
        /// </summary>
        /// <remarks>
        /// The Unique ID should ideally be a value that you can guarantee will only
        /// be assigned to a single user and, if that user is deleted, will not be
        /// re-used for another user. However, if your user repository cannot make
        /// that guarantee then it would be acceptable to return the LoginName here,
        /// there would be side-effects but they would not be too surprising.
        /// </remarks>
        string UniqueId { get; set; }

        /// <summary>
        /// Gets a value indicating that this record has been persisted to the
        /// store of users, or has been read from the store of users.
        /// </summary>
        bool IsPersisted { get; }

        /// <summary>
        /// Gets or sets a value indicating that the user account is enabled.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the user's login name. This should be unique to a user.
        /// </summary>
        string LoginName { get; set; }

        /// <summary>
        /// Gets or sets the user's name.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the password as entered at the user interface.
        /// </summary>
        /// <remarks>
        /// This is only ever filled by the user interface - the IUserManager should not store
        /// passwords and it must never return passwords when loading users.
        /// </remarks>
        string UIPassword { get; set; }

        /// <summary>
        /// Gets or sets an object that the application can tag the user with.
        /// </summary>
        /// <remarks>
        /// This is not to be saved to the database, it's just an object that the application
        /// has assigned to the user. It should be ignored by the user manager.
        /// </remarks>
        object Tag { get; set; }
    }
}
