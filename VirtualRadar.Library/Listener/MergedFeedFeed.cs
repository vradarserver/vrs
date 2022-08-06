using System;
using System.Collections.Generic;
using System.Linq;
using InterfaceFactory;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Library.Listener
{
    class MergedFeedFeed : Feed, IMergedFeedFeed
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="mergedFeed"></param>
        /// <param name="mergeFeeds"></param>
        public void Initialise(MergedFeed mergedFeed, IEnumerable<IFeed> mergeFeeds)
        {
            if(_Initialised) throw new InvalidOperationException("A feed can only be initialised once");
            if(mergedFeed == null) throw new ArgumentNullException("receiver");
            if(mergeFeeds == null) throw new ArgumentNullException("mergePathways");
            if(!mergedFeed.Enabled) throw new InvalidOperationException($"The {mergedFeed.Name} merged feed has not been enabled");

            var mergedListeners = GetListenersFromMergeFeeds(mergedFeed, mergeFeeds);

            var mergedFeedListener = Factory.Resolve<IMergedFeedListener>();
            Listener = mergedFeedListener;
            Listener.ExceptionCaught += Listener_ExceptionCaught;
            Listener.IgnoreBadMessages = true;
            Listener.AssumeDF18CF1IsIcao = false;
            mergedFeedListener.IcaoTimeout = mergedFeed.IcaoTimeout;
            mergedFeedListener.IgnoreAircraftWithNoPosition = mergedFeed.IgnoreAircraftWithNoPosition;
            mergedFeedListener.SetListeners(mergedListeners);

            DoCommonInitialise(mergedFeed.UniqueId, mergedFeed.Name, false, mergedFeed.ReceiverUsage, startAircraftList: true);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="mergedFeed"></param>
        /// <param name="mergeFeeds"></param>
        public void ApplyConfiguration(MergedFeed mergedFeed, IEnumerable<IFeed> mergeFeeds)
        {
            if(!_Initialised) throw new InvalidOperationException("ApplyConfiguration cannot be called on an uninitialised feed");
            if(mergedFeed == null) throw new ArgumentNullException("mergedFeed");
            if(mergeFeeds == null) throw new ArgumentNullException("mergePathways");
            if(mergedFeed.UniqueId != UniqueId) throw new InvalidOperationException($"Cannot apply configuration for merged feed #{mergedFeed.UniqueId} to feed for merged feed for #{UniqueId}");

            var mergedFeedListener = Listener as IMergedFeedListener;
            if(mergedFeedListener == null) throw new InvalidOperationException($"ReceiverPathway {UniqueId} was initialised with a receiver but updated with a merged feed");

            Name = mergedFeed.Name;
            SetIsVisible(mergedFeed.ReceiverUsage);

            var listeners = GetListenersFromMergeFeeds(mergedFeed, mergeFeeds);
            mergedFeedListener.IcaoTimeout = mergedFeed.IcaoTimeout;
            mergedFeedListener.IgnoreAircraftWithNoPosition = mergedFeed.IgnoreAircraftWithNoPosition;
            mergedFeedListener.SetListeners(listeners);
        }

        private static List<IMergedFeedComponentListener> GetListenersFromMergeFeeds(MergedFeed mergedFeed, IEnumerable<IFeed> mergeFeeds)
        {
            var result = new List<IMergedFeedComponentListener>();
            foreach(var receiverId in mergedFeed.ReceiverIds) {
                var feed = mergeFeeds.FirstOrDefault(r => r.UniqueId == receiverId);
                var listener = feed == null ? null : feed.Listener;
                if(listener != null) {
                    var mergedFeedReceiver = mergedFeed.ReceiverFlags.FirstOrDefault(r => r.UniqueId == receiverId);
                    var isMlatFeed = mergedFeedReceiver == null ? false : mergedFeedReceiver.IsMlatFeed;

                    var mergedFeedComponent = Factory.Resolve<IMergedFeedComponentListener>();
                    mergedFeedComponent.SetListener(listener, isMlatFeed);
                    result.Add(mergedFeedComponent);
                }
            }

            return result;
        }
    }
}
