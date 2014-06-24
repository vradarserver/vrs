using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.WinForms.OptionPage
{
    /// <summary>
    /// Describes a feed item, which is derived from either a receiver or a merged feed.
    /// </summary>
    public class CombinedFeed
    {
        public Receiver Receiver { get; private set; }

        public MergedFeed MergedFeed { get; private set; }

        public int UniqueId { get { return Receiver != null ? Receiver.UniqueId : MergedFeed != null ? MergedFeed.UniqueId : 0; } }

        public string Name { get { return Receiver != null ? Receiver.Name : MergedFeed != null ? MergedFeed.Name : null; } }

        public CombinedFeed(Receiver receiver)
        {
            Receiver = receiver;
        }

        public CombinedFeed(MergedFeed mergedFeed)
        {
            MergedFeed = mergedFeed;
        }
    }
}
