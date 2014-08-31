using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualRadar.Interface.Network
{
    /// <summary>
    /// The delegate that is called when an <see cref="IConnection"/> reads bytes off the connection.
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="length"></param>
    /// <param name="bytesRead"></param>
    public delegate void ConnectionReadDelegate(IConnection connection, byte[] buffer, int offset, int length, int bytesRead);
}
