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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.SQLite;
using VirtualRadar.Interface.StandingData;

namespace VirtualRadar.Database.BasicAircraft
{
    /// <summary>
    /// The default implementation of <see cref="IBaseicAircraftLookupDatabase"/>.
    /// </summary>
    class BasicAircraftLookupDatabase : IBasicAircraftLookupDatabase
    {
        #region Fields
        // Objects that represent each table.
        private BasicAircraftTable _AircraftTable = new BasicAircraftTable();
        private BasicModelTable    _ModelTable = new BasicModelTable();
        private BasicOperatorTable _OperatorTable = new BasicOperatorTable();

        /// <summary>
        /// The maximum number of parameters that SQLite allows.
        /// </summary>
        private const int MaxParameters = 900;

        /// <summary>
        /// The object that we synchronise threads on.
        /// </summary>
        private object _ConnectionLock = new Object();

        /// <summary>
        /// The connection to the database.
        /// </summary>
        private IDbConnection _Connection;

        /// <summary>
        /// The object that manages nestable transactions for us.
        /// </summary>
        private TransactionHelper _TransactionHelper = new TransactionHelper();

        /// <summary>
        /// True if the object has been disposed.
        /// </summary>
        private bool _Disposed;
        #endregion

        #region Properties
        private static BasicAircraftLookupDatabase _Singleton = new BasicAircraftLookupDatabase();
        /// <summary>
        /// Returns the singleton instance.
        /// </summary>
        public IBasicAircraftLookupDatabase Singleton { get { return _Singleton; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string FileName { get { return "BasicAircraftLookup.sqb"; } }

        private string _FullPath;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string FullPath
        {
            get { return _FullPath; }
            set { if(_FullPath != value) { _FullPath = value; CloseConnection(); } }
        }

        private bool _WriteSupportEnabled;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool WriteSupportEnabled
        {
            get { return _WriteSupportEnabled; }
            set { if(_WriteSupportEnabled != value) { _WriteSupportEnabled = value; CloseConnection(); } }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public object Lock { get { return _ConnectionLock; } }
        #endregion

        #region Events
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler ContentUpdated;

        /// <summary>
        /// Raises <see cref="ContentUpdated"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnContentUpdated(EventArgs args)
        {
            if(ContentUpdated != null) ContentUpdated(this, args);
        }
        #endregion

        #region Ctor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public BasicAircraftLookupDatabase()
        {
            var configurationStorage = Factory.Singleton.Resolve<IConfigurationStorage>();
            FullPath = Path.Combine(configurationStorage.Folder, FileName);
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~BasicAircraftLookupDatabase()
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
            if(disposing && !_Disposed) {
                lock(_ConnectionLock) {
                    _Disposed = true;

                    _TransactionHelper.Abandon();
                    CloseConnection();

                    if(_AircraftTable != null) {
                        _AircraftTable.Dispose();
                        _AircraftTable = null;
                    }

                    if(_ModelTable != null) {
                        _ModelTable.Dispose();
                        _ModelTable = null;
                    }

                    if(_OperatorTable != null) {
                        _OperatorTable.Dispose();
                        _OperatorTable = null;
                    }
                }
            }
        }
        #endregion

        #region OpenConnection, CloseConnection, TestConnection
        /// <summary>
        /// Closes the connection and disposes of it if open.
        /// </summary>
        private void CloseConnection()
        {
            lock(_ConnectionLock) {
                if(_Connection != null) {
                    _TransactionHelper.Abandon();

                    if(_AircraftTable != null)  _AircraftTable.CloseCommands();
                    if(_ModelTable != null)     _ModelTable.CloseCommands();
                    if(_OperatorTable != null)  _OperatorTable.CloseCommands();

                    _Connection.Close();
                    _Connection.Dispose();
                    _Connection = null;
                }
            }
        }

        /// <summary>
        /// Opens the connection if not already open.
        /// </summary>
        private void OpenConnection(string fullPath = null, bool? writeSupportEnabled = null)
        {
            lock(_ConnectionLock) {
                if(_Connection == null && !_Disposed) {
                    bool inCreateMode = fullPath != null && writeSupportEnabled.GetValueOrDefault();

                    fullPath = fullPath ?? FullPath;
                    writeSupportEnabled = writeSupportEnabled ?? WriteSupportEnabled;

                    bool fileExists = File.Exists(fullPath);
                    bool zeroLength = fileExists && new FileInfo(fullPath).Length == 0;

                    if(!String.IsNullOrEmpty(fullPath) && fileExists && (inCreateMode || !zeroLength)) {
                        var builder = Factory.Singleton.Resolve<ISQLiteConnectionStringBuilder>().Initialise();
                        builder.DataSource = fullPath;
                        builder.ReadOnly = !writeSupportEnabled.Value;
                        builder.FailIfMissing = true;
                        builder.DateTimeFormat = SQLiteDateFormats.ISO8601;
                        builder.JournalMode = SQLiteJournalModeEnum.Default;
                        var connection = Factory.Singleton.Resolve<ISQLiteConnectionProvider>().Create(builder.ConnectionString);
                        _Connection = connection;
                        _Connection.Open();
                    }
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public bool TestConnection()
        {
            bool result = false;
            lock(_ConnectionLock) {
                OpenConnection();
                result = _Connection != null;
            }

            return result;
        }
        #endregion

        #region CreateDatabaseIfMissing
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void CreateDatabaseIfMissing()
        {
            if(!String.IsNullOrEmpty(FullPath)) {
                bool fileMissing = !File.Exists(FullPath);
                bool fileEmpty = fileMissing || new FileInfo(FullPath).Length == 0;
                if(fileMissing || fileEmpty) {
                    File.Create(FullPath).Close();

                    CloseConnection();
                    OpenConnection(FullPath, true);
                    try {
                        if(_Connection != null) {
                            var transaction = _Connection.BeginTransaction();
                            try {
                                _ModelTable.CreateTable(_Connection);
                                _OperatorTable.CreateTable(_Connection);
                                _AircraftTable.CreateTable(_Connection);

                                transaction.Commit();
                            } catch(Exception ex) {
                                Debug.WriteLine(String.Format("Database.CreateDatabaseIfMissing caught exception {0}", ex.ToString()));
                                transaction.Rollback();
                                throw;
                            }
                        }
                    } finally {
                        CloseConnection();
                    }
                }
            }
        }
        #endregion

        #region StartTransaction, EndTransaction, RollbackTransaction
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void StartTransaction()
        {
            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) _TransactionHelper.StartTransaction(_Connection);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void EndTransaction()
        {
            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) _TransactionHelper.EndTransaction();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void RollbackTransaction()
        {
            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) _TransactionHelper.RollbackTransaction();
            }
        }
        #endregion

        #region Compact, PrepareForUpdate, FinishedUpdate
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Compact()
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot compress the database when write support is disabled");
            if(_TransactionHelper.Transaction != null) throw new InvalidOperationException("You cannot compress the database within a transaction");

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) {
                    Sql.ExecuteNonQuery(_Connection, _TransactionHelper.Transaction, "VACUUM");
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public void PrepareForUpdate()
        {
            // This should already be locked with a lock on Lock. We apply a second lock here
            // on the understanding that:
            //   1. If the caller forgot to call lock(Lock) then we would be doing unprotected modifications to
            //      _Connection, which would be bad.
            //   2. If the caller called lock(Lock) then they need to do so on the same thread as us, and if they
            //      have then our lock has no ill-effect. If they called on a separate thread then it'll deadlock
            //      and they need to fix their code.
            lock(_ConnectionLock) {
                CloseConnection();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void FinishedUpdate()
        {
            OnContentUpdated(EventArgs.Empty);
        }
        #endregion

        #region Aircraft transactions
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraft"></param>
        public void InsertAircraft(Interface.StandingData.BasicAircraft aircraft)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot insert aircraft when write support is disabled");

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) aircraft.AircraftID = _AircraftTable.Insert(_Connection, _TransactionHelper.Transaction, aircraft);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraft"></param>
        public void UpdateAircraft(Interface.StandingData.BasicAircraft aircraft)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot update aircraft when write support is disabled");

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) _AircraftTable.Update(_Connection, _TransactionHelper.Transaction, aircraft);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao"></param>
        /// <returns></returns>
        public Interface.StandingData.BasicAircraft GetAircraftByIcao(string icao)
        {
            VirtualRadar.Interface.StandingData.BasicAircraft result = null;

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) result = _AircraftTable.GetByIcao(_Connection, _TransactionHelper.Transaction, icao);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao"></param>
        /// <returns></returns>
        public BasicAircraftAndChildren GetAircraftAndChildrenByIcao(string icao)
        {
            BasicAircraftAndChildren result = null;

            var aircraft = GetAircraftByIcao(icao);
            if(aircraft != null) {
                result = ConvertBasicAircraftToBasicAircraftAndChildren(aircraft, autoLoadChildren: true);
            }

            return result;
        }

        /// <summary>
        /// Converts from a <see cref="BasicAircraft"/> to a <see cref="BasicAircraftAndChildren"/>.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="autoLoadChildren"></param>
        /// <returns></returns>
        private BasicAircraftAndChildren ConvertBasicAircraftToBasicAircraftAndChildren(Interface.StandingData.BasicAircraft aircraft, bool autoLoadChildren = true)
        {
            var result = new BasicAircraftAndChildren() {
                AircraftID = aircraft.AircraftID,
                BaseStationUpdated = aircraft.BaseStationUpdated,
                Icao = aircraft.Icao,
                Registration = aircraft.Registration,
            };
            if(autoLoadChildren) {
                result.Model = GetModelById(aircraft.BasicModelID);
                result.Operator = GetOperatorById(aircraft.BasicOperatorID);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="existingIcaos"></param>
        /// <returns></returns>
        public Dictionary<string, BasicAircraftAndChildren> GetManyAircraftAndChildrenByCode(string[] existingIcaos)
        {
            var result = new Dictionary<string, BasicAircraftAndChildren>();

            if(existingIcaos != null && existingIcaos.Any()) {
                lock(_ConnectionLock) {
                    OpenConnection();
                    if(_Connection != null) {
                        var maxParameters = MaxParameters;
                        var offset = 0;
                        var countIcao24s = existingIcaos.Length;

                        do {
                            var chunk = existingIcaos.Skip(offset).Take(maxParameters).ToArray();
                            foreach(var aircraft in _AircraftTable.GetManyByIcao(_Connection, _TransactionHelper.Transaction, chunk)) {
                                if(aircraft.Icao != null && !result.ContainsKey(aircraft.Icao)) {
                                    var fullRecord = ConvertBasicAircraftToBasicAircraftAndChildren(aircraft, autoLoadChildren: true);
                                    result.Add(aircraft.Icao, fullRecord);
                                }
                            }
                            offset += maxParameters;
                        } while(offset < countIcao24s);
                    }
                }
            }

            return result;
        }
        #endregion

        #region Model transactions
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="model"></param>
        public void InsertModel(BasicModel model)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot insert models when write support is disabled");

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) model.ModelID = _ModelTable.Insert(_Connection, _TransactionHelper.Transaction, model);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="model"></param>
        public void UpdateModel(BasicModel model)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot update models when write support is disabled");

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) _ModelTable.Update(_Connection, _TransactionHelper.Transaction, model);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void DeleteUnusedModels()
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot delete unused models when write support is disabled");

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) _ModelTable.DeleteUnused(_Connection, _TransactionHelper.Transaction);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public BasicModel GetModelById(int? id)
        {
            BasicModel result = null;

            if(id != null) {
                lock(_ConnectionLock) {
                    OpenConnection();
                    if(_Connection != null) result = _ModelTable.GetById(_Connection, _TransactionHelper.Transaction, id.Value);
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<BasicModel> GetModelByName(string name)
        {
            List<BasicModel> result = new List<BasicModel>();

            if(!String.IsNullOrEmpty(name)) {
                lock(_ConnectionLock) {
                    OpenConnection();
                    if(_Connection != null) _ModelTable.GetByName(_Connection, _TransactionHelper.Transaction, result, name);
                }
            }

            return result;
        }
        #endregion

        #region Operator transactions
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="record"></param>
        public void InsertOperator(BasicOperator record)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot insert operators when write support is disabled");

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) record.OperatorID = _OperatorTable.Insert(_Connection, _TransactionHelper.Transaction, record);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="record"></param>
        public void UpdateOperator(BasicOperator record)
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot update operators when write support is disabled");

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) _OperatorTable.Update(_Connection, _TransactionHelper.Transaction, record);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void DeleteUnusedOperators()
        {
            if(!WriteSupportEnabled) throw new InvalidOperationException("You cannot delete unused operators when write support is disabled");

            lock(_ConnectionLock) {
                OpenConnection();
                if(_Connection != null) _OperatorTable.DeleteUnused(_Connection, _TransactionHelper.Transaction);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public BasicOperator GetOperatorById(int? id)
        {
            BasicOperator result = null;

            if(id != null) {
                lock(_ConnectionLock) {
                    OpenConnection();
                    if(_Connection != null) result = _OperatorTable.GetById(_Connection, _TransactionHelper.Transaction, id.Value);
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<BasicOperator> GetOperatorByName(string name)
        {
            var result = new List<BasicOperator>();

            if(!String.IsNullOrEmpty(name)) {
                lock(_ConnectionLock) {
                    OpenConnection();
                    if(_Connection != null) _OperatorTable.GetByName(_Connection, _TransactionHelper.Transaction, result, name);
                }
            }

            return result;
        }
        #endregion
    }
}
