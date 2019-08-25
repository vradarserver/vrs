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
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.Database;
using InterfaceFactory;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using VirtualRadar.Interface;
using VirtualRadar.Localisation;
using System.Diagnostics;

namespace VirtualRadar.Library.Presenter
{
    /// <summary>
    /// The default implementation of the object that can control <see cref="IConnectionClientLogView"/> views.
    /// </summary>
    class ConnectionClientLogPresenter : IConnectionClientLogPresenter
    {
        /// <summary>
        /// The default implementation of the provider.
        /// </summary>
        class DefaultProvider : IConnectionClientLogPresenterProvider
        {
            class BackgroundState
            {
                public Action<IList<LogClient>> CallbackMethod;
                public IList<LogClient> LookupClients;
            }

            public void InvokeOnBackgroundThread(Action<IList<LogClient>> callbackMethod, IList<LogClient> lookupClients)
            {
                ThreadPool.QueueUserWorkItem(BackgroundThreadWorker, new BackgroundState() { CallbackMethod = callbackMethod, LookupClients = lookupClients } );
            }

            private void BackgroundThreadWorker(object state)
            {
                var backgroundState = (BackgroundState)state;
                backgroundState.CallbackMethod(backgroundState.LookupClients);
            }

            public string LookupReverseDns(IPAddress ipAddress)
            {
                return Dns.GetHostEntry(ipAddress).HostName;
            }
        }

        /// <summary>
        /// The view that this presenter is controlling.
        /// </summary>
        private IConnectionClientLogView _View;

        /// <summary>
        /// The object that manages the clock for us.
        /// </summary>
        private IClock _Clock;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IConnectionClientLogPresenterProvider Provider { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ConnectionClientLogPresenter()
        {
            Provider = new DefaultProvider();
            _Clock = Factory.Resolve<IClock>();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="view"></param>
        public void Initialise(IConnectionClientLogView view)
        {
            _View = view;

            var clients = new List<LogClient>();
            Dictionary<long, IList<LogSession>> sessionMap = new Dictionary<long,IList<LogSession>>();
            var logDatabase = Factory.Resolve<ILogDatabase>().Singleton;
            logDatabase.FetchAll(clients, sessionMap);
            _View.ShowClientsAndSessions(clients, sessionMap);

            var lookupClients = clients.Where(c => (c.ReverseDns == null || c.ReverseDnsDate == null) && (c.IpAddress != null && c.Address != null)).ToList();
            if(lookupClients.Count > 0) Provider.InvokeOnBackgroundThread(LookupReverseDNS, lookupClients);
        }

        /// <summary>
        /// Runs on the background thread, looks up clients that have never had a reverse DNS lookup performed on
        /// them and updates the database and the view with the results.
        /// </summary>
        /// <param name="lookupClients"></param>
        private void LookupReverseDNS(IList<LogClient> lookupClients)
        {
            var database = Factory.Resolve<ILogDatabase>().Singleton;

            foreach(var client in lookupClients) {
                string reverseDns = null;
                bool canSave = true;

                try {
                    reverseDns = Provider.LookupReverseDns(client.Address);
                } catch(ArgumentException ex) {
                    Debug.WriteLine(String.Format("ConnectionClientLogPresenter.LookupReverseDNS caught exception: {0}", ex.ToString()));
                    reverseDns = client.IpAddress;
                } catch(SocketException ex) {
                    Debug.WriteLine(String.Format("ConnectionClientLogPresenter.LookupReverseDNS caught exception: {0}", ex.ToString()));
                    canSave = false;
                } catch(Exception ex) {
                    Debug.WriteLine(String.Format("ConnectionClientLogPresenter.LookupReverseDNS caught exception: {0}", ex.ToString()));
                    var log = Factory.Resolve<ILog>().Singleton;
                    log.WriteLine("{0}: {1}", Strings.CaughtExceptionInReverseDNS, ex.ToString());
                    canSave = false;
                }

                if(canSave) {
                    client.ReverseDns = reverseDns;
                    client.ReverseDnsDate = _Clock.UtcNow;
                    database.UpdateClient(client);

                    _View.RefreshClientReverseDnsDetails(client);
                }
            }
        }
    }
}
