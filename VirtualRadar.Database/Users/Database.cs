using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.SQLite;
using System.IO;

namespace VirtualRadar.Database.Users
{
    /// <summary>
    /// The object that controls the user database.
    /// </summary>
    class Database : IDisposable
    {
        #region Fields
        /// <summary>
        /// The lock that forces single-threaded access to the database.
        /// </summary>
        private SpinLock _SpinLock = new SpinLock();

        /// <summary>
        /// The connection to the database
        /// </summary>
        private IDbConnection _Connection;

        /// <summary>
        /// The object that can help with nested transactions.
        /// </summary>
        private TransactionHelper _TransactionHelper;

        /// <summary>
        /// The object controlling access to the user table.
        /// </summary>
        private UserTable _UserTable;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the log of all SQL commands.
        /// </summary>
        public TextWriter Log { get; set; }
        #endregion

        #region Ctor and finaliser
        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~Database()
        {
            Dispose(false);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                if(_TransactionHelper != null) _TransactionHelper.Abandon();
                _TransactionHelper = null;

                if(_Connection != null) _Connection.Dispose();
                _Connection = null;

                if(_UserTable != null) _UserTable.Dispose();
                _UserTable = null;
            }
        }
        #endregion

        #region OpenConnection
        /// <summary>
        /// Creates and opens the connection if one has not already been established.
        /// </summary>
        private void OpenConnection()
        {
            if(_Connection == null) {
                var configurationStorage = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton;

                var builder = Factory.Singleton.Resolve<ISQLiteConnectionStringBuilder>().Initialise();
                builder.DataSource = Path.Combine(configurationStorage.Folder, "Users.sqb");
                builder.DateTimeFormat = SQLiteDateFormats.JulianDay;
                builder.ReadOnly = false;
                builder.FailIfMissing = false;
                builder.JournalMode = SQLiteJournalModeEnum.Persist;
                _Connection = Factory.Singleton.Resolve<ISQLiteConnectionProvider>().Create(builder.ConnectionString);
                _Connection.Open();

                _TransactionHelper = new TransactionHelper();

                _UserTable = new UserTable();
                _UserTable.CreateTable(_Connection);
            }
        }
        #endregion

        #region User operations
        /// <summary>
        /// Inserts a new user.
        /// </summary>
        /// <param name="user"></param>
        public void UserInsert(User user)
        {
            using(_SpinLock.AcquireLock()) {
                OpenConnection();
                _UserTable.Insert(_Connection, _TransactionHelper.Transaction, Log, user);
            }
        }

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        /// <param name="user"></param>
        public void UserUpdate(User user)
        {
            using(_SpinLock.AcquireLock()) {
                OpenConnection();
                _UserTable.Update(_Connection, _TransactionHelper.Transaction, Log, user);
            }
        }

        /// <summary>
        /// Deletes an existing user.
        /// </summary>
        /// <param name="user"></param>
        public void UserDelete(User user)
        {
            using(_SpinLock.AcquireLock()) {
                OpenConnection();
                _UserTable.Delete(_Connection, _TransactionHelper.Transaction, Log, user);
            }
        }

        /// <summary>
        /// Returns the user associated with the login name passed across.
        /// </summary>
        /// <param name="loginName"></param>
        /// <returns></returns>
        public User UserGetByLoginName(string loginName)
        {
            using(_SpinLock.AcquireLock()) {
                OpenConnection();
                return _UserTable.GetByLoginName(_Connection, _TransactionHelper.Transaction, Log, loginName);
            }
        }

        /// <summary>
        /// Returns a collection of every user in the database.
        /// </summary>
        /// <returns></returns>
        public List<IUser> UserGetAll()
        {
            using(_SpinLock.AcquireLock()) {
                OpenConnection();
                var userList = _UserTable.GetAll(_Connection, _TransactionHelper.Transaction, Log);
                var genericList = userList.Select(r => (IUser)r).ToList();

                return genericList;
            }
        }
        #endregion
    }
}
