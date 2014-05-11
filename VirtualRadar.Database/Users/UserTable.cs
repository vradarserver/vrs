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
using System.IO;
using System.Linq;
using System.Text;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Database.Users
{
    /// <summary>
    /// The class that deals with user records in the user database.
    /// </summary>
    class UserTable : Table
    {
        #region Properties
        /// <summary>
        /// See base class docs.
        /// </summary>
        protected override string TableName { get { return "User"; } }
        #endregion

        #region CreateTable
        /// <summary>
        /// See base class docs.
        /// </summary>
        /// <param name="connection"></param>
        public override void CreateTable(IDbConnection connection)
        {
            Sql.ExecuteNonQuery(connection, null, String.Format(
                "CREATE TABLE IF NOT EXISTS [{0}]" +
                "  ([Id] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL" +
                "  ,[Enabled] BIT NOT NULL" +
                "  ,[LoginName] TEXT NOT NULL COLLATE NOCASE" +
                "  ,[Name] TEXT NOT NULL" +
                "  ,[PasswordHashVersion] INTEGER NOT NULL" +
                "  ,[PasswordHash] BLOB NOT NULL" +
                "  ,[Created] DATETIME NOT NULL" +
                "  ,[Updated] DATETIME NOT NULL" +
                "  )", TableName));

            Sql.ExecuteNonQuery(connection, null, String.Format(
                "CREATE UNIQUE INDEX IF NOT EXISTS [idx_User_LoginName]" +
                "  ON [{0}] ([LoginName] ASC)", TableName));
        }
        #endregion

        #region Insert, Update, Delete
        /// <summary>
        /// Adds a new record to the table.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="user"></param>
        public void Insert(IDbConnection connection, IDbTransaction transaction, TextWriter log, User user)
        {
            var now = DateTime.UtcNow;
            user.Id = PrepareAndExecuteInsert(connection, transaction, log, "Insert", "Id", new Dictionary<string, object>() {
                { "Enabled",                user.Enabled },
                { "LoginName",              user.LoginName },
                { "Name",                   user.Name },
                { "PasswordHashVersion",    user.PasswordHashVersion },
                { "PasswordHash",           user.PasswordHash },
                { "Created",                now },
                { "Updated",                now },
            });
            user.CreatedUtc = now;
            user.UpdatedUtc = now;
        }

        /// <summary>
        /// Updates an existing record.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="user"></param>
        public void Update(IDbConnection connection, IDbTransaction transaction, TextWriter log, User user)
        {
            var now = DateTime.UtcNow;
            PrepareAndExecuteNonQuery(connection, transaction, log, "Update",
                "UPDATE [{0}]" +
                "   SET [Enabled] = @Enabled" +
                "      ,[LoginName] = @LoginName" +
                "      ,[Name] = @Name" +
                "      ,[PasswordHashVersion] = @PasswordHashVersion" +
                "      ,[PasswordHash] = @PasswordHash" +
                "      ,[Updated] = @Updated" +
                " WHERE [Id] = @Id",
                new Dictionary<string, object>() {
                    { "Id",                     user.Id },
                    { "Enabled",                user.Enabled },
                    { "LoginName",              user.LoginName },
                    { "Name",                   user.Name },
                    { "PasswordHashVersion",    user.PasswordHashVersion },
                    { "PasswordHash",           user.PasswordHash },
                    { "Updated",                now },
                }
            );
            user.UpdatedUtc = now;
        }

        /// <summary>
        /// Removes an existing record from the database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="user"></param>
        public void Delete(IDbConnection connection, IDbTransaction transaction, TextWriter log, User user)
        {
            PrepareAndExecuteNonQuery(connection, transaction, log, "Delete",
                "DELETE FROM [{0}] WHERE [Id] = @Id",
                new Dictionary<string,object>() {
                    { "Id", user.Id },
                }
            );
        }
        #endregion

        #region BuildFromReader
        /// <summary>
        /// Creates a user object from the content of a reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private User BuildFromReader(IDataReader reader, User user = null)
        {
            if(user == null) user = new User();

            user.Id =                   Sql.GetInt64(reader, "Id");
            user.Enabled =              Sql.GetBool(reader, "Enabled");
            user.LoginName =            Sql.GetString(reader, "LoginName");
            user.Name =                 Sql.GetString(reader, "Name");
            user.PasswordHashVersion =  Sql.GetInt32(reader, "PasswordHashVersion");
            user.PasswordHash =         Sql.GetBlob(reader, "PasswordHash");
            user.CreatedUtc =           Sql.GetDateTime(reader, "Created", DateTimeKind.Utc);
            user.UpdatedUtc =           Sql.GetDateTime(reader, "Updated", DateTimeKind.Utc);

            return user;
        }
        #endregion

        #region GetByLoginName, GetAll
        /// <summary>
        /// Returns the user for the ID passed across or null if no such user exists.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public User GetById(IDbConnection connection, IDbTransaction transaction, TextWriter log, long id)
        {
            User result = null;

            var command = PrepareGetById(connection, transaction);
            SetNamedParameter(command, 0, "Id", id);

            using(var reader = command.Command.ExecuteReader()) {
                if(reader.Read()) {
                    result = BuildFromReader(reader);
                }
            }

            return result;
        }

        /// <summary>
        /// Prepares the command to fetch a single record by ID. This is used by more than one public Get method.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private SqlPreparedCommand PrepareGetById(IDbConnection connection, IDbTransaction transaction)
        {
            return PrepareCommand(connection, transaction, "GetById", String.Format("SELECT * FROM [{0}] WHERE [Id] = @Id", TableName), 1);
        }

        /// <summary>
        /// Returns the user record for a login name or null if no such user record exists.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="loginName"></param>
        /// <returns></returns>
        public User GetByLoginName(IDbConnection connection, IDbTransaction transaction, TextWriter log, string loginName)
        {
            User result = null;

            if(!String.IsNullOrEmpty(loginName)) {
                var command = PrepareCommandAndParams(connection, transaction, "GetByLoginName",
                    "SELECT * FROM [{0}] WHERE [LoginName] = @LoginName", new Dictionary<string, object>() {
                        { "LoginName", loginName },
                    }
                );

                using(var reader = command.Command.ExecuteReader()) {
                    if(reader.Read()) result = BuildFromReader(reader);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a collection of every user recorded on the database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public List<IUser> GetAll(IDbConnection connection, IDbTransaction transaction, TextWriter log)
        {
            var result = new List<IUser>();

            var command = PrepareCommandAndParams(connection, transaction, "GetAll",
                "SELECT * FROM [{0}] ORDER BY [LoginName]",
                null
            );

            using(var reader = command.Command.ExecuteReader()) {
                while(reader.Read()) {
                    result.Add(BuildFromReader(reader));
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a collection of every user matching the ID passed across.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="log"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<IUser> GetManyById(IDbConnection connection, IDbTransaction transaction, TextWriter log, List<long> ids)
        {
            var result = new List<IUser>();

            var command = PrepareGetById(connection, transaction);
            foreach(var id in ids) {
                SetNamedParameter(command, 0, "Id", id);
                using(var reader = command.Command.ExecuteReader()) {
                    if(reader.Read()) result.Add(BuildFromReader(reader));
                }
            }

            return result;
        }
        #endregion
    }
}
