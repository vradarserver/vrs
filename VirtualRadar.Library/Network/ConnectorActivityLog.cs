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
using System.Linq;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Network;

namespace VirtualRadar.Library.Network
{
    /// <summary>
    /// The default implementation of <see cref="IConnectorActivityLog"/>.
    /// </summary>
    public class ConnectorActivityLog : IConnectorActivityLog
    {
        #region Static fields
        /// <summary>
        /// The largest number of activities recorded by the log.
        /// </summary>
        public static readonly int MaximumActivities = 250;
        #endregion

        #region Fields
        /// <summary>
        /// The lock that protects the list from multithreaded access.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// The list of active connectors.
        /// </summary>
        private List<IConnector> _Connectors = new List<IConnector>();

        /// <summary>
        /// True if the system's snapshot logger has been initialised.
        /// </summary>
        private bool _InitialisedSnapshotLogger;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs. Retained for backwards compatability.
        /// </summary>
        public IConnectorActivityLog Singleton => Factory.ResolveSingleton<IConnectorActivityLog>();
        #endregion

        #region Events
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<ConnectorActivityEvent>> ActivityRecorded;

        /// <summary>
        /// Raises <see cref="ActivityRecorded"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnActivityRecorded(EventArgs<ConnectorActivityEvent> args)
        {
            EventHelper.Raise(ActivityRecorded, this, args);
        }
        #endregion

        #region RecordConnectorCreated
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="connector"></param>
        public void RecordConnectorCreated(IConnector connector)
        {
            lock(_SyncLock) {
                if(!_InitialisedSnapshotLogger) {
                    _InitialisedSnapshotLogger = true;
                    Factory.ResolveSingleton<IConnectorSnapshotLogger>().Initialise();
                }

                if(!_Connectors.Contains(connector)) {
                    _Connectors.Add(connector);
                    connector.ActivityRecorded += Connector_ActivityRecorded;
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="connector"></param>
        public void RecordConnectorDestroyed(IConnector connector)
        {
            lock(_SyncLock) {
                if(_Connectors.Contains(connector)) {
                    connector.ActivityRecorded -= Connector_ActivityRecorded;
                    _Connectors.Remove(connector);
                }
            }
        }
        #endregion

        #region GetActivityHistory, GetActiveConnectors
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public ConnectorActivityEvent[] GetActivityHistory()
        {
            var result = new List<ConnectorActivityEvent>();

            lock(_SyncLock) {
                foreach(var connector in _Connectors) {
                    result.AddRange(connector.GetActivityHistory());
                }
            }
            result.Sort((lhs, rhs) => lhs.Time.CompareTo(rhs.Time));

            return result.ToArray();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IConnector[] GetActiveConnectors()
        {
            lock(_SyncLock) {
                return _Connectors.ToArray();
            }
        }
        #endregion

        #region Subscribed events
        /// <summary>
        /// Called when a connector indicates that an activity has been recorded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Connector_ActivityRecorded(object sender, EventArgs<ConnectorActivityEvent> args)
        {
            OnActivityRecorded(args);
        }
        #endregion
    }
}
