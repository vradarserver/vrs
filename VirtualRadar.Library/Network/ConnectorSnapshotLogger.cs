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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Network;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Library.Network
{
    /// <summary>
    /// The default implementation of <see cref="IConnectorSnapshotLogger"/>.
    /// </summary>
    class ConnectorSnapshotLogger : IConnectorSnapshotLogger
    {
        /// <summary>
        /// The number of seconds between each snapshot.
        /// </summary>
        public static readonly int SnapshotInterval = 60 * 15;

        /// <summary>
        /// The name of the file where snapshots are recorded.
        /// </summary>
        public static readonly string FileName = "ConnectorSnapshots.txt";

        /// <summary>
        /// The maximum size of the snapshot file.
        /// </summary>
        public static readonly int MaxFileSize = 100 * 1024;

        /// <summary>
        /// The lock that prevents two snapshots from being recorded simultaneously.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// The heartbeat service that we are synchronised with.
        /// </summary>
        private IHeartbeatService _HeartbeatService;

        /// <summary>
        /// The date and time at UTC of the next snapshot.
        /// </summary>
        private DateTime _NextSnapshot;

        private static ConnectorSnapshotLogger _Singleton = new ConnectorSnapshotLogger();
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IConnectorSnapshotLogger Singleton { get { return _Singleton; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string FullPath { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Initialise()
        {
            lock(_SyncLock) {
                if(_HeartbeatService == null) {
                    _NextSnapshot = DateTime.UtcNow.AddSeconds(SnapshotInterval);
                    FullPath = Path.Combine(Factory.Singleton.Resolve<IConfigurationStorage>().Singleton.Folder, FileName);

                    _HeartbeatService = Factory.Singleton.Resolve<IHeartbeatService>().Singleton;
                    _HeartbeatService.SlowTick += HeartbeatService_SlowTick;
                }
            }
        }

        /// <summary>
        /// Records a snapshot.
        /// </summary>
        public void RecordSnapshot()
        {
            lock(_SyncLock) {
                _NextSnapshot = DateTime.UtcNow.AddSeconds(SnapshotInterval);
                var snapshotBytes = Encoding.UTF8.GetBytes(BuildSnapshot());
                var existingLines = File.Exists(FullPath) ? File.ReadAllLines(FullPath) : new string[0];

                using(var fileStream = new FileStream(FullPath, FileMode.Create, FileAccess.Write)) {
                    fileStream.Write(snapshotBytes, 0, snapshotBytes.Length);
                    for(var lineNumber = 0;lineNumber < existingLines.Length;++lineNumber) {
                        var line = String.Format("{0}{1}", existingLines[lineNumber], Environment.NewLine);
                        var lineBytes = Encoding.UTF8.GetBytes(line);
                        if(lineBytes.Length + fileStream.Length > MaxFileSize) {
                            lineBytes = Encoding.UTF8.GetBytes("--- 8< ---");
                            lineNumber = existingLines.Length;
                        }
                        
                        fileStream.Write(lineBytes, 0, lineBytes.Length);
                    }
                }
            }
        }

        private string BuildSnapshot()
        {
            var buffer = new StringBuilder();
            var seperator = "===============================================================================";
                
            var connectors = Factory.Singleton.ResolveSingleton<IConnectorActivityLog>().GetActiveConnectors();
            AppendFormatLine(buffer, seperator);
            AppendFormatLine(buffer, "{0:G}   {1:N0} active connectors", DateTime.Now, connectors.Length);
            foreach(var connector in connectors) {
                var lastException = connector.LastException;
                var connections = connector.GetConnections();

                buffer.AppendLine();
                AppendFormatLine(buffer, "Name:               {0}", connector.Name ?? "");
                AppendFormatLine(buffer, "Created:            {0:G} UTC", connector.Created);
                AppendFormatLine(buffer, "Intent:             {0}", connector.Intent ?? "");
                AppendFormatLine(buffer, "Type:               {0}", connector.GetType().Name);
                AppendFormatLine(buffer, "IsPassive:          {0}", connector.IsPassive);
                AppendFormatLine(buffer, "IsSingleConnection: {0}", connector.IsSingleConnection);
                AppendFormatLine(buffer, "Closed:             {0}", !connector.EstablishingConnections);
                AppendFormatLine(buffer, "TotalExceptions:    {0:N0}", connector.CountExceptions);
                if(lastException != null) {
                    AppendFormatLine(buffer, "LastException:      {0:g} UTC {1}", lastException.TimeUtc, lastException.Exception.Message);
                }
                AppendFormatLine(buffer, "ConnectionStatus:   {0}", connector.ConnectionStatus);
                AppendFormatLine(buffer, "Connections:        {0:N0}", connections.Length);
                foreach(var connection in connections) {
                    if(connection != connections[0]) AppendFormatLine(buffer, "---------------------------------------------------------------------------");
                    AppendFormatLine(buffer, "    {0}", connection.Description);
                    AppendFormatLine(buffer, "    Connected:         {0:G} UTC", connection.Created);
                    AppendFormatLine(buffer, "    Operations queued: {0:N0}", connection.OperationQueueEntries);
                    AppendFormatLine(buffer, "    Bytes read:        {0:N0}", connection.BytesRead);
                    AppendFormatLine(buffer, "    Bytes written:     {0:N0}", connection.BytesWritten);
                    AppendFormatLine(buffer, "    Bytes buffered:    {0:N0}", connection.WriteQueueBytes);
                    AppendFormatLine(buffer, "    Bytes discarded:   {0:N0}", connection.StaleBytesDiscarded);
                    AppendFormatLine(buffer, "    Status:            {0}", connection.ConnectionStatus);
                }
            }
            AppendFormatLine(buffer, seperator);
            buffer.AppendLine();

            return buffer.ToString();
        }

        private void AppendFormatLine(StringBuilder buffer, string message)
        {
            buffer.AppendLine(message);
        }

        private void AppendFormatLine(StringBuilder buffer, string format, params object[] args)
        {
            AppendFormatLine(buffer, String.Format(format, args));
        }

        /// <summary>
        /// Called when the slow heartbeat ticks.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void HeartbeatService_SlowTick(object sender, EventArgs args)
        {
            if(DateTime.UtcNow >= _NextSnapshot) {
                ThreadPool.QueueUserWorkItem((object r) => {
                    try {
                        RecordSnapshot();
                    } catch {
                        ; // We just swallow any exceptions - we can't let them fall off the end of the thread.
                    }
                });
            }
        }
    }
}
