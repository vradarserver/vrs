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
        /// The object that persists data to and from the client table.
        /// </summary>
        private ClientTable _ClientTable;

        /// <summary>
        /// The object that persists data to and from the session table.
        /// </summary>
        private SessionTable _SessionTable;

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

        private static readonly ILogDatabase _Singleton = new Database();
        /// <summary>
        /// See interface docs.
        /// </summary>
        public ILogDatabase Singleton { get { return _Singleton; } }
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
                if(_ClientTable != null) _ClientTable.Dispose();
                if(_SessionTable != null) _SessionTable.Dispose();
                _TransactionHelper.Abandon();
                if(_Connection != null) _Connection.Dispose();
                _Connection = null;
                _ClientTable = null;
                _SessionTable = null;
            }
        }
        #endregion

        #region StartTransaction, EndTransaction, RollbackTransaction
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void StartTransaction()
        {
            lock(_SyncLock) {
                CreateConnection();
                _TransactionHelper.StartTransaction(_Connection);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void EndTransaction()
        {
            lock(_SyncLock) {
                CreateConnection();
                _TransactionHelper.EndTransaction();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void RollbackTransaction()
        {
            lock(_SyncLock) {
                _TransactionHelper.RollbackTransaction();
            }
        }
        #endregion

        #region CreateConnection
        /// <summary>
        /// Opens a connect to the database file, can be called while a connection is already open
        /// </summary>
        private void CreateConnection()
        {
            if(_Connection == null) {
                IConfigurationStorage configurationStorage = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton;

                var builder = Factory.Singleton.Resolve<ISQLiteConnectionStringBuilder>().Initialise();
                builder.DataSource = Path.Combine(configurationStorage.Folder, "ConnectionLog.sqb");
                builder.DateTimeFormat = SQLiteDateFormats.JulianDay;
                builder.ReadOnly = false;
                builder.FailIfMissing = false;
                builder.JournalMode = SQLiteJournalModeEnum.Persist;
                var connection = Factory.Singleton.Resolve<ISQLiteConnectionProvider>().Create(builder.ConnectionString);
                _Connection = connection;
                _Connection.Open();

                _ClientTable = new ClientTable();
                _SessionTable = new SessionTable();

                _ClientTable.CreateTable(_Connection);
                _SessionTable.CreateTable(_Connection);
            }
        }
        #endregion

        #region EstablishSession, UpdateClient, UpdateSession
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public LogSession EstablishSession(string ipAddress)
        {
            LogSession result = null;

            lock(_SyncLock) {
                CreateConnection();

                var client = _ClientTable.SelectByIpAddress(_Connection, _TransactionHelper.Transaction, null, ipAddress);
                if(client == null) {
                    client = new LogClient() { IpAddress = ipAddress };
                    _ClientTable.Insert(_Connection, _TransactionHelper.Transaction, null, client);
                }

                result = new LogSession() {
                    ClientId = client.Id,
                    StartTime = Provider.UtcNow,
                    EndTime = Provider.UtcNow,
                };
                _SessionTable.Insert(_Connection, _TransactionHelper.Transaction, null, result);
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
                _ClientTable.Update(_Connection, _TransactionHelper.Transaction, null, client);
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
                _SessionTable.Update(_Connection, _TransactionHelper.Transaction, null, session);
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
                foreach(var session in _SessionTable.SelectByStartDate(_Connection, _TransactionHelper.Transaction, null, startTime, endTime)) {
                    sessions.Add(session);
                    if(clients != null && !clientMap.ContainsKey(session.ClientId)) {
                        clients.Add(_ClientTable.SelectById(_Connection, _TransactionHelper.Transaction, null, session.ClientId));
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

                foreach(var client in _ClientTable.SelectAll(_Connection, _TransactionHelper.Transaction, null)) {
                    clients.Add(client);
                }

                if(sessionsMap != null) {
                    foreach(var session in _SessionTable.SelectAll(_Connection, _TransactionHelper.Transaction, null)) {
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
