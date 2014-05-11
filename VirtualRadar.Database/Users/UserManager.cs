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
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;

namespace VirtualRadar.Database.Users
{
    /// <summary>
    /// The default implementation of <see cref="IUserManager"/>. Users are held
    /// in an SQLite database in the configuration folder. The database is created
    /// if required.
    /// </summary>
    class UserManager : IUserManager
    {
        #region Fields
        /// <summary>
        /// The database that is recording user information for us.
        /// </summary>
        private Database _Database;
        #endregion

        #region Properties
        private static IUserManager _Singleton;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IUserManager Singleton
        {
            get
            {
                if(_Singleton == null) _Singleton = new UserManager();
                return _Singleton;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Name { get { return Strings.UserManagerName; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool LoginNameIsCaseSensitive { get { return false; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool CanCreateUsers { get { return true; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool CanCreateUsersWithHash { get { return true; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool CanEditUsers { get { return true; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool CanChangePassword { get { return true; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool CanChangeEnabledState { get { return true; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool CanDeleteUsers { get { return true; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool CanListUsers { get { return true; } }
        #endregion

        #region Initialise, Shutdown
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Initialise()
        {
            _Database = new Database();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Shutdown()
        {
            if(_Database != null) _Database.Dispose();
            _Database = null;
        }
        #endregion

        #region Validation
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="record"></param>
        /// <param name="currentRecord"></param>
        /// <param name="allRecords"></param>
        public void ValidateUser(List<ValidationResult> results, IUser record, IUser currentRecord, List<IUser> allRecords)
        {
            if(allRecords == null) allRecords = GetUsers().ToList();

            ValidateLoginName(results, record, currentRecord, allRecords);
            ValidateName(results, record, currentRecord, allRecords);
            ValidatePassword(results, record, currentRecord, allRecords);
        }

        /// <summary>
        /// Performs the validation on a login name.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="record"></param>
        /// <param name="currentRecord"></param>
        /// <param name="allRecords"></param>
        private void ValidateLoginName(List<ValidationResult> results, IUser record, IUser currentRecord, List<IUser> allRecords)
        {
            var loginName = record.LoginName;

            if(String.IsNullOrEmpty(loginName)) results.Add(new ValidationResult(ValidationField.LoginName, Strings.LoginNameMissing));
            else {
                var existingUser = allRecords.FirstOrDefault(r => (r.LoginName ?? "").Equals(loginName ?? "", StringComparison.CurrentCultureIgnoreCase));
                if(existingUser != null && existingUser != currentRecord) {
                    results.Add(new ValidationResult(ValidationField.LoginName, Strings.LoginNameExists));
                }
            }
        }

        /// <summary>
        /// Performs validation on a name.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="record"></param>
        /// <param name="currentRecord"></param>
        /// <param name="allRecords"></param>
        private void ValidateName(List<ValidationResult> results, IUser record, IUser currentRecord, List<IUser> allRecords)
        {
            ;
        }

        /// <summary>
        /// Performs validation on a password.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="record"></param>
        /// <param name="currentRecord"></param>
        /// <param name="allRecords"></param>
        private void ValidatePassword(List<ValidationResult> results, IUser record, IUser currentRecord, List<IUser> allRecords)
        {
            var password = record.UIPassword;
            if(String.IsNullOrEmpty(password)) results.Add(new ValidationResult(ValidationField.Password, Strings.PasswordMissing));
        }
        #endregion

        #region CreateUser, UpdateUser, DeleteUser
        /// <summary>
        /// Casts a user interface to a concrete user object.
        /// </summary>
        /// <param name="currentDetails"></param>
        /// <returns></returns>
        private User CastUser(IUser currentDetails)
        {
            var result = currentDetails as User;
            if(result == null && currentDetails != null) throw new InvalidOperationException("This manager can only manage users that it created");

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="user"></param>
        public void CreateUser(IUser user)
        {
            var hash = new Hash(user.UIPassword);
            CreateUserWithHash(user, hash);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="passwordHash"></param>
        public void CreateUserWithHash(IUser user, Hash passwordHash)
        {
            var ourUser = CastUser(user);
            if(passwordHash == null) passwordHash = new Hash("");
            ourUser.PasswordHashVersion = passwordHash.Version;
            ourUser.PasswordHash = passwordHash.Buffer.ToArray();

            _Database.UserInsert(ourUser);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        public void UpdateUser(IUser user, string password)
        {
            var ourUser = CastUser(user);

            if(password != null) {
                var hash = new Hash(user.UIPassword);
                ourUser.PasswordHashVersion = hash.Version;
                ourUser.PasswordHash = hash.Buffer.ToArray();
            }

            _Database.UserUpdate(ourUser);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public void DeleteUser(IUser user)
        {
            var ourUser = CastUser(user);
            _Database.UserDelete(ourUser);
        }
        #endregion

        #region GetUser***
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="loginName"></param>
        /// <returns></returns>
        public IUser GetUserByLoginName(string loginName)
        {
            return _Database.UserGetByLoginName(loginName);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="uniqueIdentifiers"></param>
        /// <returns></returns>
        public IEnumerable<IUser> GetUsersByUniqueId(IEnumerable<string> uniqueIdentifiers)
        {
            var ids = new List<long>();
            foreach(var uniqueIdentifier in uniqueIdentifiers) {
                long id;
                if(long.TryParse(uniqueIdentifier, out id)) ids.Add(id);
            }
            ids = ids.Distinct().ToList();

            return _Database.UserGetManyByUniqueId(ids);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IUser> GetUsers()
        {
            return _Database.UserGetAll();
        }
        #endregion

        #region PasswordMatches
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool PasswordMatches(IUser user, string password)
        {
            var ourUser = CastUser(user);
            return ourUser.Hash.PasswordMatches(password);
        }
        #endregion
    }
}
