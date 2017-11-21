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
        private object _SyncLock = new object();

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
                var configurationStorage = Factory.Singleton.ResolveSingleton<IConfigurationStorage>();

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
            lock(_SyncLock) {
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
            lock(_SyncLock) {
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
            lock(_SyncLock) {
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
            lock(_SyncLock) {
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
            lock(_SyncLock) {
                OpenConnection();
                return _UserTable.GetAll(_Connection, _TransactionHelper.Transaction, Log);
            }
        }

        /// <summary>
        /// Returns a collection of users that match the IDs passed across.
        /// </summary>
        /// <param name="uniqueIdentifiers"></param>
        /// <returns></returns>
        public List<IUser> UserGetManyByUniqueId(List<long> uniqueIdentifiers)
        {
            lock(_SyncLock) {
                OpenConnection();
                return _UserTable.GetManyById(_Connection, _TransactionHelper.Transaction, Log, uniqueIdentifiers);
            }
        }
        #endregion
    }
}
