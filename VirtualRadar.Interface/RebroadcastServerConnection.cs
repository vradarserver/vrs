using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Describes a connection to a rebroadcast server.
    /// </summary>
    public class RebroadcastServerConnection
    {
        /// <summary>
        /// Gets or sets the identifier of the rebroadcast server.
        /// </summary>
        public int RebroadcastServerId { get; set; }

        /// <summary>
        /// Gets or sets the name of the rebroadcast server.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the local port that the rebroadcast server listens to.
        /// </summary>
        public int LocalPort { get; set; }

        /// <summary>
        /// Gets or sets the address that the client connected from.
        /// </summary>
        public string EndpointAddress { get; set; }

        /// <summary>
        /// Gets or sets the port that the client connected from.
        /// </summary>
        public int EndpointPort { get; set; }

        /// <summary>
        /// Gets or sets a count of bytes currently buffered and awaiting transmission.
        /// </summary>
        public long BytesBuffered { get; set; }

        /// <summary>
        /// Gets or sets a count of bytes sent to the client.
        /// </summary>
        public long BytesWritten { get; set; }

        /// <summary>
        /// Gets or sets a count of bytes that were discarded from the buffer because they took
        /// too long to send.
        /// </summary>
        public long StaleBytesDiscarded { get; set; }
    }
}
