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
using System.Linq;
using System.Text;
using VirtualRadar.Interface;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.Database;
using InterfaceFactory;
using System.Diagnostics;

namespace VirtualRadar.Library
{
    /// <summary>
    /// The default implementation of <see cref="IConnectionLogger"/>.
    /// </summary>
    sealed class ConnectionLogger : IConnectionLogger
    {
        #region Fields
        /// <summary>
        /// The duration over which no request is seen before a session is considered to be stale and removed from the cache.
        /// </summary>
        private const int StaleSessionMinutes = 10;

        /// <summary>
        /// The number of seconds that needs to elapse between exceptions that are allowed to bubble up to the GUI. Exceptions
        /// caught before this period expires are discarded.
        /// </summary>
        private const int AntiGuiSpamSeconds = 30;

        /// <summary>
        /// The number of seconds that need to have elapsed before the session cache is flushed to disk - prevents manual triggers
        /// of the heartbeat timer causing excessive disk activity.
        /// </summary>
        private const int CacheFlushTimeoutSeconds = 60;

        /// <summary>
        /// The object that synchronises access to the fields across threads.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// The object that manages the clock for us.
        /// </summary>
        private IClock _Clock;

        /// <summary>
        /// The dictionary of established sessions.
        /// </summary>
        private Dictionary<string, LogSession> _Sessions = new Dictionary<string,LogSession>();

        /// <summary>
        /// The last time the database was updated.
        /// </summary>
        private DateTime _LastDatabaseUpdate;

        /// <summary>
        /// The last time a database exception was raised during ResponseSent processing.
        /// </summary>
        private DateTime _LastExceptionRaised;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IWebServer WebServer { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public ILogDatabase LogDatabase { get; set; }
        #endregion

        #region Events
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<Exception>> ExceptionCaught;

        /// <summary>
        /// Raises <see cref="ExceptionCaught"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnExceptionCaught(EventArgs<Exception> args)
        {
            EventHelper.Raise(ExceptionCaught, this, args);
        }
        #endregion

        #region Constructors and finaliser
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ConnectionLogger()
        {
            _Clock = Factory.Singleton.Resolve<IClock>();
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~ConnectionLogger()
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
        private void Dispose(bool disposing)
        {
            if(disposing) {
                FlushSessionsToDatabase();
            }
        }
        #endregion

        #region Start
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Start()
        {
            if(WebServer == null) throw new InvalidOperationException("The web server must be supplied before the connection logger can be used");
            if(LogDatabase == null) throw new InvalidOperationException("The database must be supplied before the connection logger can be used");

            WebServer.ResponseSent += WebServer_ResponseSent;
            Factory.Singleton.ResolveSingleton<IHeartbeatService>().SlowTick += Heartbeat_SlowTick;
        }
        #endregion

        #region Utility methods - FormKey
        /// <summary>
        /// Forms a key from the IP address and the user name.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        private static string FormKey(string ipAddress, string userName)
        {
            userName = (userName ?? "").Trim().ToUpper();

            return String.Format("{0}{1}{2}",
                ipAddress,
                userName == "" ? "-" : "",
                userName);
        }
        #endregion

        #region FlushSessionsToDatabase, RemoveOldSessions
        /// <summary>
        /// Writes the sessions in the cache out to the database.
        /// </summary>
        private void FlushSessionsToDatabase()
        {
            Exception caughtException = null;

            lock(_SyncLock) {
                if(_Sessions.Count > 0) {
                    try {
                        LogDatabase.PerformInTransaction(() => {
                            foreach(var session in _Sessions.Values) {
                                LogDatabase.UpdateSession(session);
                            }
                            return true;
                        });
                    } catch(Exception ex) {
                        Debug.WriteLine(String.Format("ConnectionLogger.FlushSessionsToDatabase caught exception: {0}", ex.ToString()));
                        caughtException = ex;
                    }
                }
            }

            // We want to show the exception without locking out ResponseSent updates. Note that calls
            // to this method are usually AT LEAST one minute apart so there is no need to prevent the
            // UI from being spammed with exception messages.
            if(caughtException != null) OnExceptionCaught(new EventArgs<Exception>(caughtException));
        }

        /// <summary>
        /// Removes expired sessions from the cache.
        /// </summary>
        private void RemoveOldSessions()
        {
            lock(_SyncLock) {
                var threshold = _Clock.UtcNow.AddMinutes(-StaleSessionMinutes);
                var expiredSessions = _Sessions.Where(s => s.Value.EndTime <= threshold).ToList();
                foreach(var expiredSession in expiredSessions) {
                    _Sessions.Remove(expiredSession.Key);
                }
            }
        }
        #endregion

        #region Events subscribed
        /// <summary>
        /// Called when the webServer responds to a request. Usually called on some random non-GUI thread.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void WebServer_ResponseSent(object sender, ResponseSentEventArgs args)
        {
            Exception caughtException = null;

            if(!String.IsNullOrEmpty(args.UserAddress) && args.BytesSent > 0L) {
                lock(_SyncLock) {
                    try {
                        var key = FormKey(args.UserAddress, args.UserName);

                        LogSession session;
                        if(!_Sessions.TryGetValue(key, out session)) {
                            session = LogDatabase.EstablishSession(args.UserAddress, args.UserName);
                            _Sessions.Add(key, session);
                        }

                        ++session.CountRequests;
                        session.EndTime = _Clock.UtcNow;
                        switch(args.Classification) {
                            case ContentClassification.Audio:    session.AudioBytesSent += args.BytesSent; break;
                            case ContentClassification.Html:     session.HtmlBytesSent += args.BytesSent; break;
                            case ContentClassification.Image:    session.ImageBytesSent += args.BytesSent; break;
                            case ContentClassification.Json:     session.JsonBytesSent += args.BytesSent; break;
                            case ContentClassification.Other:    session.OtherBytesSent += args.BytesSent; break;
                            default:                             throw new NotImplementedException();
                        }
                    } catch(Exception ex) {
                        Debug.WriteLine(String.Format("ConnectionLogger.WebServer_ResponseSent caught exception: {0}", ex.ToString()));

                        // We limit the rate at which exceptions are bubbled up to the GUI to prevent the GUI from
                        // being spammed by them.
                        if(_LastExceptionRaised.AddSeconds(AntiGuiSpamSeconds) <= _Clock.UtcNow) {
                            _LastExceptionRaised = _Clock.UtcNow;
                            caughtException = ex;
                        }
                    }
                }
            }

            if(caughtException != null) OnExceptionCaught(new EventArgs<Exception>(caughtException));
        }

        /// <summary>
        /// Called when the heartbeat timer elapses (usually once every few minutes but can be within microseconds
        /// of the last tick). Usually called on some random non-GUI thread.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Heartbeat_SlowTick(object sender, EventArgs args)
        {
            if(_LastDatabaseUpdate.AddSeconds(CacheFlushTimeoutSeconds) <= _Clock.UtcNow) {
                _LastDatabaseUpdate = _Clock.UtcNow;
                FlushSessionsToDatabase();
            }
            RemoveOldSessions();
        }
        #endregion
    }
}
