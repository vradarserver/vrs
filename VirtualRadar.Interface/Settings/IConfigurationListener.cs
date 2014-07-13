using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// The interface for a class that can listen to a <see cref="Configuration"/> object and raise
    /// a single event when one of its properties (or the properties of a child object) are changed.
    /// </summary>
    /// <remarks>
    /// Note that at the time of writing there is no one singleton instance of <see cref="Configuration"/>
    /// that you can listen to. This is currently only really of interest to GUI objects that want to
    /// observe a configuration as it is changed by the user.
    /// </remarks>
    public interface IConfigurationListener : IDisposable
    {
        /// <summary>
        /// Raised when a configuration value changes.
        /// </summary>
        event EventHandler<ConfigurationListenerEventArgs> PropertyChanged;

        /// <summary>
        /// Starts observing a configuration for changes.
        /// </summary>
        /// <param name="configuration"></param>
        void Initialise(Configuration configuration);

        /// <summary>
        /// Stops observing a configuration for changes.
        /// </summary>
        void Release();
    }
}
