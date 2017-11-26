// Copyright © 2010 onwards, Andrew Whewell
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
using InterfaceFactory;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.SQLite;
using Dapper;

namespace VirtualRadar.Database.Log
{
    /// <summary>
    /// The default implementation of <see cref="ILogDatabase"/>.
    /// </summary>
    sealed class Database : ILogDatabase
    {
        #region Private class - DefaultProvider
        /// <summary>
        /// The default implementation of the provider.
        /// </summary>
        class DefaultProvider : ILogDatabaseProvider
        {
            /// <summary>
            /// See interface docs.
            /// </summary>
            public DateTime UtcNow { get { return DateTime.UtcNow; } }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The object that all threads synchronise to 
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// The connection to the database file.
        /// </summary>
        private IDbConnection _Connection;

        /// <summary>
        /// The transaction currently in force.
        /// </summary>
        private IDbTransaction _Transaction;

        /// <summary>
        /// The object that manages nested transactions for us.
        /// </summary>
        private TransactionHelper _TransactionHelper = new TransactionHelper();
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public ILogDatabaseProvider Provider { get; set; }
        #endregion

        #region Constructor and Finaliser
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Database()
        {
            Provider = new DefaultProvider();
        }

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
        /// Finalises or disposes of the object.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if(disposing) {
                if(_Connection != null) {
                    _Connection.Dispose();
                }
                _Connection = null;
            }
        }
        #endregion

        #region StartTransaction, EndTransaction, RollbackTransaction
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool PerformInTransaction(Func<bool> action)
        {
            var result = false;

            lock(_SyncLock) {
                CreateConnection();
                result = _TransactionHelper.PerformInTransaction(_Connection, _Transaction != null, false, r => _Transaction = r, action);
            }

            return result;
        }
        #endregion

        #region CreateConnection
        /// <summary>
        /// Opens a connect to the database file, can be called while a connection is already open
        /// </summary>
        private void CreateConnection()
        {
            if(_Connection == null) {
                IConfigurationStorage configurationStorage = Factory.Singleton.ResolveSingleton<IConfigurationStorage>();

                var builder = Factory.Singleton.Resolve<ISQLiteConnectionStringBuilder>().Initialise();
                builder.DataSource = Path.Combine(configurationStorage.Folder, "ConnectionLog.sqb");
                builder.DateTimeFormat = SQLiteDateFormats.JulianDay;
                builder.ReadOnly = false;
                builder.FailIfMissing = false;
                builder.JournalMode = SQLiteJournalModeEnum.Persist;
                var connection = Factory.Singleton.Resolve<ISQLiteConnectionProvider>().Create(builder.ConnectionString);
                _Connection = connection;
                _Connection.Open();

                _Connection.Execute(Commands.UpdateSchema);

                if(!Sql.ColumnExists(_Connection, null, "Session", "UserName")) {
                    _Connection.Execute("ALTER TABLE [Session] ADD COLUMN [UserName] TEXT NULL;");
                }
            }
        }
        #endregion

        #region Client Operations
        private LogClient[] Client_GetAll()
        {
            return _Connection.Query<LogClient>("SELECT * FROM [Client]", transaction: _Transaction).ToArray();
        }

        private LogClient Client_GetById(long id)
        {
            return _Connection.Query<LogClient>("SELECT * FROM [Client] WHERE [Id] = @id", new {
                id = id,
            }, transaction: _Transaction).FirstOrDefault();
        }

        private LogClient Client_GetByIpAddress(string ipAddress)
        {
            return _Connection.Query<LogClient>("SELECT * FROM [Client] WHERE [IpAddress] = @ipAddress", new {
                @ipAddress = ipAddress,
            }, transaction: _Transaction).FirstOrDefault();
        }

        private void Client_Insert(LogClient client)
        {
            client.Id = _Connection.ExecuteScalar<long>(Commands.Client_Insert, new {
                @ipAddress =        client.IpAddress,
                @reverseDns =       client.ReverseDns,
                @reverseDnsDate =   client.ReverseDnsDate,
            }, transaction: _Transaction);
        }

        private void Client_Update(LogClient client)
        {
            _Connection.Execute(Commands.Client_Update, new {
                @ipAddress          = client.IpAddress,
                @reverseDns         = client.ReverseDns,
                @reverseDnsDate     = client.ReverseDnsDate,
                @id                 = client.Id,
            }, transaction: _Transaction);
        }
        #endregion

        #region Session Operations
        private LogSession[] Session_GetAll()
        {
            return _Connection.Query<LogSession>("SELECT * FROM [Session]", transaction: _Transaction).ToArray();
        }

        private LogSession[] Session_GetByDateRange(DateTime startDate, DateTime endDate)
        {
            startDate = startDate.ToUniversalTime();
            endDate = endDate.ToUniversalTime();

            return _Connection.Query<LogSession>(Commands.Session_GetByDateRange, new {
                @startDate =    startDate,
                @endDate =      endDate,
            }, transaction: _Transaction).ToArray();
        }

        private void Session_Insert(LogSession session)
        {
            session.Id = _Connection.ExecuteScalar<long>(Commands.Session_Insert, new {
                @clientId           = session.ClientId,
                @userName           = session.UserName,
                @startTime          = session.StartTime,
                @endTime            = session.EndTime,
                @countRequests      = session.CountRequests,
                @otherBytesSent     = session.OtherBytesSent,
                @htmlBytesSent      = session.HtmlBytesSent,
                @jsonBytesSent      = session.JsonBytesSent,
                @imageBytesSent     = session.ImageBytesSent,
                @audioBytesSent     = session.AudioBytesSent,
            }, transaction: _Transaction);
        }

        private void Session_Update(LogSession session)
        {
            _Connection.Execute(Commands.Session_Update, new {
                @clientId           = session.ClientId,
                @userName           = session.UserName,
                @startTime          = session.StartTime,
                @endTime            = session.EndTime,
                @countRequests      = session.CountRequests,
                @otherBytesSent     = session.OtherBytesSent,
                @htmlBytesSent      = session.HtmlBytesSent,
                @jsonBytesSent      = session.JsonBytesSent,
                @imageBytesSent     = session.ImageBytesSent,
                @audioBytesSent     = session.AudioBytesSent,
                @id                 = session.Id,
            }, transaction: _Transaction);
        }
        #endregion

        #region EstablishSession, UpdateClient, UpdateSession
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public LogSession EstablishSession(string ipAddress, string userName)
        {
            LogSession result = null;

            lock(_SyncLock) {
                CreateConnection();

                var client = Client_GetByIpAddress(ipAddress);
                if(client == null) {
                    client = new LogClient() { IpAddress = ipAddress };
                    Client_Insert(client);
                }

                result = new LogSession() {
                    ClientId = client.Id,
                    UserName = userName,
                    StartTime = Provider.UtcNow,
                    EndTime = Provider.UtcNow,
                };
                Session_Insert(result);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="client"></param>
        public void UpdateClient(LogClient client)
        {
            if(client == null) throw new ArgumentNullException("client");
            if(_Connection == null) throw new InvalidOperationException("The connection must be opened before the client can be updated");

            lock(_SyncLock) {
                Client_Update(client);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="session"></param>
        public void UpdateSession(LogSession session)
        {
            if(session == null) throw new ArgumentNullException("session");
            if(_Connection == null) throw new InvalidOperationException("The connection must be opened before the session can be updated");

            lock(_SyncLock) {
                Session_Update(session);
            }
        }
        #endregion

        #region FetchSessions, FetchAll
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="clients"></param>
        /// <param name="sessions"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        public void FetchSessions(IList<LogClient> clients, IList<LogSession> sessions, DateTime startTime, DateTime endTime)
        {
            if(sessions == null) throw new ArgumentNullException("sessions");

            lock(_SyncLock) {
                CreateConnection();

                Dictionary<long, long> clientMap = new Dictionary<long,long>();
                foreach(var session in Session_GetByDateRange(startTime, endTime)) {
                    sessions.Add(session);
                    if(clients != null && !clientMap.ContainsKey(session.ClientId)) {
                        clients.Add(Client_GetById(session.ClientId));
                        clientMap.Add(session.ClientId, session.ClientId);
                    }
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="clients"></param>
        /// <param name="sessionsMap"></param>
        public void FetchAll(IList<LogClient> clients, IDictionary<long, IList<LogSession>> sessionsMap)
        {
            if(clients == null) throw new ArgumentNullException("clients");

            clients.Clear();
            if(sessionsMap != null) sessionsMap.Clear();

            lock(_SyncLock) {
                CreateConnection();

                foreach(var client in Client_GetAll()) {
                    clients.Add(client);
                }

                if(sessionsMap != null) {
                    foreach(var session in Session_GetAll()) {
                        IList<LogSession> sessions;
                        if(!sessionsMap.TryGetValue(session.ClientId, out sessions)) {
                            sessions = new List<LogSession>();
                            sessionsMap.Add(session.ClientId, sessions);
                        }
                        sessions.Add(session);
                    }
                }
            }
        }
        #endregion
    }
}
