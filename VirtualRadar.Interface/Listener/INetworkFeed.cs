namespace VirtualRadar.Interface.Listener
{
    /// <summary>
    /// The interface for feeds that take messages from a network source.
    /// </summary>
    public interface INetworkFeed : IFeed
    {
        /// <summary>
        /// Gets the listener that <see cref="AircraftList"/> is listening to.
        /// </summary>
        IListener Listener { get; }
    }
}
