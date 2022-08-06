using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Interface.Listener
{
    /// <summary>
    /// An object that pulls in the feed from a single configurable receiver.
    /// </summary>
    public interface IReceiverFeed : IFeed
    {
        /// <summary>
        /// Initialises the feed with the receiver configuration settings passed across.
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="configuration"></param>
        void Initialise(Receiver receiver, Configuration configuration);

        /// <summary>
        /// Updates the listener with the new configuration options passed across.
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="configuration"></param>
        void ApplyConfiguration(Receiver receiver, Configuration configuration);
    }
}
