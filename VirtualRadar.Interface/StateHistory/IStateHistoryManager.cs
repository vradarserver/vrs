using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;

namespace VirtualRadar.Interface.StateHistory
{
    /// <summary>
    /// The interface for a singleton that can read and write state histories.
    /// </summary>
    [Singleton]
    public interface IStateHistoryManager
    {
        /// <summary>
        /// Gets the configured Enabled value.
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// Gets the configured NonStandardFolder value.
        /// </summary>
        string NonStandardFolder { get; }

        /// <summary>
        /// Raised after configuration changes have been applied by the manager.
        /// </summary>
        event EventHandler ConfigurationLoaded;

        /// <summary>
        /// Initialises the object before use.
        /// </summary>
        void Initialise();
    }
}
