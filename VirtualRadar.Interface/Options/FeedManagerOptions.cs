namespace VirtualRadar.Interface.Options
{
    /// <summary>
    /// The options for the code that manages incoming aircraft feeds.
    /// </summary>
    public class FeedManagerOptions
    {
        /// <summary>
        /// Settings for every receiver.
        /// </summary>
        public List<ReceiverOptions> Receivers { get; } = new();
    }
}
