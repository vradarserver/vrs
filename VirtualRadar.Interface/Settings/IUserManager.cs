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
using VirtualRadar.Interface.View;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// The interface for the object that manages lists of users.
    /// </summary>
    public interface IUserManager : ISingleton<IUserManager>
    {
        /// <summary>
        /// Gets the name of the manager.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating that the manager can create new user accounts.
        /// </summary>
        bool CanCreateUsers { get; }

        /// <summary>
        /// Gets a value indicating that the manager can create a user and make use
        /// of a password hash supplied by VRS.
        /// </summary>
        /// <remarks>
        /// This is only used to port the old Basic Authentication user from legacy
        /// versions of VRS into IUserManager - it is expected that most 3rd party
        /// user repositories can't support this, in which case they should return
        /// false here.
        /// </remarks>
        bool CanCreateUsersWithHash { get; }

        /// <summary>
        /// Gets a value indicating that the manager can modify user accounts.
        /// </summary>
        bool CanEditUsers { get; }

        /// <summary>
        /// Gets a value indicating that the manager can change passwords on user accounts.
        /// </summary>
        bool CanChangePassword { get; }

        /// <summary>
        /// Gets a value indicating that the manager can enable or disable user accounts.
        /// </summary>
        bool CanChangeEnabledState { get; }

        /// <summary>
        /// Gets a value indicating that the manager can delete user accounts.
        /// </summary>
        bool CanDeleteUsers { get; }

        /// <summary>
        /// Gets a value indicating that the manager can retrieve a list of users.
        /// </summary>
        bool CanListUsers { get; }

        /// <summary>
        /// Gets a value indicating that the login name is case sensitive.
        /// </summary>
        bool LoginNameIsCaseSensitive { get; }

        /// <summary>
        /// Initialises the manager.
        /// </summary>
        void Initialise();

        /// <summary>
        /// Shuts the manager down.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Validates the record passed across.
        /// </summary>
        /// <param name="results">A list of every problem with the record. If there are no problems then leave this alone.</param>
        /// <param name="record">The record to validate.</param>
        /// <param name="currentRecord">If the record is new then this will be null - otherwise it is the original record that is now being modified.</param>
        /// <param name="allRecords">If this is null then validate against the source of users - otherwise it is the entire list of users
        /// to validate against (the assumption being that this was originally returned by GetAll and may contain new, modified or deleted objects
        /// that have not yet been saved).</param>
        /// <returns></returns>
        /// <remarks>
        /// If the user manager does not support a particular feature, such as the changing of the enabled state or
        /// the changing of the password, then return an appropriate validation message. However the UI should have
        /// taken care not to let such modifications get this far.
        /// </remarks>
        void ValidateUser(List<ValidationResult> results, IUser record, IUser currentRecord, IList<IUser> allRecords);

        /// <summary>
        /// Creates a new user. If <see cref="CanCreateUsers"/> is false then this should throw an exception when called.
        /// The user manager is expected to modify the record passed in to set IsPersisted to true and to fill in the unique ID.
        /// </summary>
        /// <param name="user"></param>
        void CreateUser(IUser user);

        /// <summary>
        /// Creates a new user. If <see cref="CanCreateUsersWithHash"/> is false then this should throw an exception when called.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="passwordHash"></param>
        void CreateUserWithHash(IUser user, Hash passwordHash);

        /// <summary>
        /// Edits an existing user. Throw an exception if the change is not permitted, otherwise modify the backing store to
        /// reflect the change in details for the user with the appropriate UniqueID.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="newPassword">This will be null if the password is to remain unchanged, otherwise it is the new password.</param>
        void UpdateUser(IUser user, string newPassword);

        /// <summary>
        /// Delete the user passed across. Throw an exception if the deletion is not permitted, otherwise remove the user
        /// from the backing store.
        /// </summary>
        /// <param name="user"></param>
        void DeleteUser(IUser user);

        /// <summary>
        /// Returns the user with the login name specified or null if no such user exists.
        /// </summary>
        /// <param name="loginName"></param>
        /// <returns>The user record or null.</returns>
        IUser GetUserByLoginName(string loginName);

        /// <summary>
        /// Returns a collection of users that have the unique identifiers passed across. 
        /// </summary>
        /// <param name="uniqueIdentifiers"></param>
        /// <returns></returns>
        /// <remarks>
        /// It is very likely that you will be passed identifiers for users that do not exist,
        /// or had existed at one point but were since deleted. This is because the UI to maintain
        /// users is separate from the configuration UI. When you are passed a unique ID that no
        /// longer exists you should omit it entirely from the result - do not return null
        /// elements for them. If every unique ID passed across has no user associated with it
        /// then you would return an empty list.
        /// </remarks>
        IEnumerable<IUser> GetUsersByUniqueId(IEnumerable<string> uniqueIdentifiers);

        /// <summary>
        /// Returns a list of all users. Throw an exception if <see cref="CanListUsers"/> is false.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Do not return deleted users in this list. Do not consider the Enabled flag - this must
        /// return both enabled and disabled users.
        /// </remarks>
        IEnumerable<IUser> GetUsers();

        /// <summary>
        /// Returns true if the password is a match for the user's password.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        bool PasswordMatches(IUser user, string password);
    }
}
