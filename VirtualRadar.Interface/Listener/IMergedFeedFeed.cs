using System.Collections.Generic;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Interface.Listener
{
    /// <summary>
    /// The interface for objects that expose a single feed built from the merging of
    /// two or more feeds.
    /// </summary>
    public interface IMergedFeedFeed : IFeed
    {
        /// <summary>
        /// Initialises the feed with the merged feed configuration settings passed across.
        /// </summary>
        /// <param name="mergedFeed"></param>
        /// <param name="mergePathways"></param>
        void Initialise(MergedFeed mergedFeed, IEnumerable<IFeed> mergePathways);

        /// <summary>
        /// Updates the listener with the new configuration options passed across.
        /// </summary>
        /// <param name="mergedFeed"></param>
        /// <param name="mergePathways"></param>
        void ApplyConfiguration(MergedFeed mergedFeed, IEnumerable<IFeed> mergePathways);
    }
}
