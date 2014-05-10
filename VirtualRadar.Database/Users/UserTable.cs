using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

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
        public List<User> GetAll(IDbConnection connection, IDbTransaction transaction, TextWriter log)
        {
            var result = new List<User>();

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
        #endregion
    }
}
