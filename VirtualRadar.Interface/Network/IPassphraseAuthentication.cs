using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Interface.Network
{
    /// <summary>
    /// The interface for objects that implement passphrase authentication on
    /// rebroadcast servers.
    /// </summary>
    public interface IPassphraseAuthentication : IConnectorAuthentication
    {
        /// <summary>
        /// Gets or sets the passphrase that the class will accept.
        /// </summary>
        string Passphrase { get; set; }
    }
}
