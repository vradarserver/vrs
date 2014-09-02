using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace VirtualRadar.Interface.Network
{
    /// <summary>
    /// The interface for an <see cref="IConnection"/> exposed by <see cref="INetworkConnector"/>.
    /// </summary>
    public interface INetworkConnection : IConnection
    {
        /// <summary>
        /// Gets the local end point. Can be null if connection has been broken.
        /// </summary>
        IPEndPoint LocalEndPoint { get; }

        /// <summary>
        /// Gets the remote end point. Can be null if connection has been broken.
        /// </summary>
        IPEndPoint RemoteEndPoint { get; }
    }
}
