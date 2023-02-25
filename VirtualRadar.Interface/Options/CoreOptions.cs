namespace VirtualRadar.Interface.Options
{
    /// <summary>
    /// The configuration of the .NET core version of the program.
    /// </summary>
    public class CoreOptions
    {
        /// <summary>
        /// Incremented on every save, used for optimistic locking.
        /// </summary>
        public long SaveCounter { get; }

        /// <summary>
        /// Settings for the feed manager.
        /// </summary>
        public FeedManagerOptions FeedManagerOptions { get; } = new();

        /// <summary>
        /// Settings for the user manager.
        /// </summary>
        public UserManagerOptions UserManagerOptions { get; } = new();
    }
}
