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

        #region GetUser, GetUsers
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
        /// <returns></returns>
        public IEnumerable<IUser> GetUsers()
        {
            return _Database.UserGetAll();
        }
        #endregion

        #region LoginWebsiteUser, GenerateServiceUserHash, LoginServiceUser
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="loginName"></param>
        /// <param name="passwordHash"></param>
        /// <param name="hashVersion"></param>
        /// <returns></returns>
        public IUser LoginWebsiteUser(string loginName, byte[] passwordHash, int hashVersion)
        {
            return DoLoginUser(loginName, passwordHash, hashVersion);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="loginName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public Hash GenerateServiceUserHash(string loginName, string password)
        {
            return new Hash(password);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="loginName"></param>
        /// <param name="passwordHash"></param>
        /// <param name="hashVersion"></param>
        /// <returns></returns>
        public IUser LoginServiceUser(string loginName, byte[] passwordHash, int hashVersion)
        {
            return DoLoginUser(loginName, passwordHash, hashVersion);
        }

        /// <summary>
        /// Performs the work for the two login methods. In the default manager we don't
        /// need to distinguish between website users and service users.
        /// </summary>
        /// <param name="loginName"></param>
        /// <param name="passwordHash"></param>
        /// <param name="hashVersion"></param>
        /// <returns></returns>
        private IUser DoLoginUser(string loginName, byte[] passwordHash, int hashVersion)
        {
            IUser result = null;

            if(!String.IsNullOrEmpty(loginName) && passwordHash != null) {
                var existingUser = _Database.UserGetByLoginName(loginName);
                if(hashVersion == existingUser.PasswordHashVersion) {
                    if(passwordHash.SequenceEqual(existingUser.PasswordHash)) {
                        result = existingUser;
                    }
                }
            }

            return result;
        }
        #endregion
    }
}
