// Copyright © 2014 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VirtualRadar.Interface.Options;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.Validation;
using VirtualRadar.Localisation;

namespace VirtualRadar.Database.SQLite.Users
{
    /// <summary>
    /// The default implementation of <see cref="IUserManager"/>. Users are held
    /// in an SQLite database in the configuration folder. The database is created
    /// if required.
    /// </summary>
    class UserManager : IUserManager
    {
        private UserContext _UserContext;
        private UserManagerOptions _Options;

        /// <summary>
        /// The last temporary unique ID assigned by the manager.
        /// </summary>
        private long _TemporaryUniqueId;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Name => Strings.UserManagerName;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool LoginNameIsCaseSensitive => false;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool CanCreateUsers => true;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool CanCreateUsersWithHash => true;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool CanEditUsers => true;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool CanChangePassword => true;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool CanChangeEnabledState => true;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool CanDeleteUsers => true;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool CanListUsers => true;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public UserManager(UserContext userContext, IOptions<UserManagerOptions> options)
        {
            _UserContext = userContext;
            _Options = options.Value;

            _UserContext.Database.EnsureCreated();
            _UserContext.Database.ExecuteSql(FormattableStringFactory.Create(
                UserSqbScripts.UpdateSchema
            ));
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Initialise()
        {
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Shutdown()
        {
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="record"></param>
        /// <param name="currentRecord"></param>
        /// <param name="allRecords"></param>
        public void ValidateUser(IList<ValidationResult> results, IUser record, IUser currentRecord, IList<IUser> allRecords)
        {
            if(allRecords == null) {
                allRecords = _UserContext.Users.ToArray();
            }

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
        private void ValidateLoginName(IList<ValidationResult> results, IUser record, IUser currentRecord, IList<IUser> allRecords)
        {
            var loginName = record.LoginName;

            if(String.IsNullOrEmpty(loginName)) {
                results.Add(new ValidationResult(record, ValidationField.LoginName, Strings.LoginNameMissing));
            } else {
                var existingUser = allRecords
                    .Where(r => r != currentRecord)
                    .Any(r => (r.LoginName ?? "").Equals(loginName ?? "", StringComparison.CurrentCultureIgnoreCase));
                if(existingUser) {
                    results.Add(new ValidationResult(record, ValidationField.LoginName, Strings.LoginNameExists));
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
        private void ValidateName(IList<ValidationResult> results, IUser record, IUser currentRecord, IList<IUser> allRecords)
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
        private void ValidatePassword(IList<ValidationResult> results, IUser record, IUser currentRecord, IList<IUser> allRecords)
        {
            var password = record.UIPassword;
            if(String.IsNullOrEmpty(password)) {
                results.Add(new ValidationResult(record, ValidationField.Password, Strings.PasswordMissing));
            }
        }

        /// <summary>
        /// Casts a user interface to a concrete user object.
        /// </summary>
        /// <param name="currentDetails"></param>
        /// <returns></returns>
        private User CastUser(IUser currentDetails)
        {
            var result = currentDetails as User;
            if(result == null && currentDetails != null) {
                throw new InvalidOperationException("This manager can only manage users that it created");
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IUser NewUser() => new User();

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        public void SaveUser(IUser user, string password)
        {
            var ourUser = CastUser(user);

            if(!_UserContext.Users.Contains(ourUser)) {
                _UserContext.Users.Add(ourUser);
            }

            password ??= user.UIPassword;
            if(password != null) {
                var hash = new Hash(password);
                ourUser.PasswordHashVersion = hash.Version;
                ourUser.PasswordHash = hash.Buffer.ToArray();
            }

            _UserContext.SaveChanges();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public void DeleteUser(IUser user)
        {
            var ourUser = CastUser(user);
            _UserContext.Users.Remove(ourUser);
            _UserContext.SaveChanges();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="loginName"></param>
        /// <returns></returns>
        public IUser GetUserByLoginName(string loginName) => _UserContext.Users.FirstOrDefault(usr => usr.LoginName == loginName);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="uniqueIdentifiers"></param>
        /// <returns></returns>
        public IEnumerable<IUser> GetUsersByUniqueId(IEnumerable<string> uniqueIdentifiers)
        {
            var allUsers = GetUsers()
                .ToDictionary(u => u.UniqueId, u => u);

            var result = uniqueIdentifiers.Select(id => {
                if(allUsers.TryGetValue(id, out var user)) {
                    allUsers[id] = null;
                }
                return user;
            })
            .Where(usr => usr != null)
            .ToArray();

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IUser> GetUsers() => _UserContext.Users.ToArray();

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool PasswordMatches(IUser user, string password)
        {
            var ourUser = CastUser(user);
            return ourUser
                .Hash
                .PasswordMatches(password);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public string GenerateTemporaryUniqueId(IUser user)
        {
            var result = _TemporaryUniqueId - 1;
            _TemporaryUniqueId = result;

            return result.ToString(CultureInfo.InvariantCulture);
        }
    }
}
