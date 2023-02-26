// Copyright © 2013 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Interface.Feeds
{
    /// <summary>
    /// The interface for objects that can collect together a <see cref="IFeed"/> for each receiver and merged feed configured in settings.
    /// </summary>
    public interface IFeedManager : IDisposable
    {
        /// <summary>
        /// Gets a collection of active and enabled feeds.
        /// </summary>
        /// <remarks>
        /// If a feed has been disabled in the configuration then it is not included in this collection.
        /// </remarks>
        IFeed[] Feeds { get; }

        /// <summary>
        /// Gets a collection of feeds that the web site can see.
        /// </summary>
        /// <remarks>
        /// This is a subset of <see cref="Feeds"/>.
        /// </remarks>
        IFeed[] VisibleFeeds { get; }

        /// <summary>
        /// Raised when the collection of feeds managed by the object is changed.
        /// </summary>
        event EventHandler FeedsChanged;

        /// <summary>
        /// Raised when the listener attached to a feed changes its connection state.
        /// </summary>
        event EventHandler<EventArgs<IFeed>> ConnectionStateChanged;

        /// <summary>
        /// Initialises the manager.
        /// </summary>
        void Initialise();

        /// <summary>
        /// Adds a custom feed.
        /// </summary>
        /// <param name="customFeed"></param>
        /// <remarks><para>
        /// This can be called before or after <see cref="Initialise"/> is called. Any feeds that are
        /// added before the manager is initialised will not appear in feed lists until after the
        /// <see cref="Initialise"/> call. Events raised on the custom feed before <see cref="Initialise"/>
        /// is called will not raise feed manager events.
        /// </para><para>
        /// If the custom feed's unique ID is zero then an ID will be allocated from a pool. If the pool
        /// is exhausted then the program will throw a <see cref="FeedUniqueIdException"/> exception.
        /// At time of writing the pool's range is 1000000 to 1999999 inclusive. Allocated IDs are not
        /// returned to the pool after a custom feed has been removed from the manager.
        /// </para><para>
        /// If the custom feed's unique ID is not zero then it is used unchanged. If this would lead to
        /// a clash of unique IDs, either between other custom feeds or with a normal feed, then the
        /// program will throw a <see cref="FeedUniqueIdException"/> exception.
        /// </para><para>
        /// Custom feeds are disconnected when <see cref="Dispose"/> is called but they are not themselves
        /// disposed. The code that adds custom feeds is solely responsible for their disposal.
        /// </para></remarks>
        void AddCustomFeed(ICustomFeed customFeed);

        /// <summary>
        /// Removes a custom feed.
        /// </summary>
        /// <param name="customFeed"></param>
        void RemoveCustomFeed(ICustomFeed customFeed);

        /// <summary>
        /// Causes every feed to establish a connection.
        /// </summary>
        void Connect();

        /// <summary>
        /// Causes every feed to disconnect from the data source.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Returns the feed with a given name or null if no such feed exists. Only enabled feed are returned.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ignoreInvisibleFeeds"></param>
        /// <returns></returns>
        IFeed GetByName(string name, bool ignoreInvisibleFeeds);

        /// <summary>
        /// Returns the feed with the given identifier or null if no such feed exists. Only enabled feeds are
        /// returned.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ignoreInvisibleFeeds"></param>
        /// <returns></returns>
        IFeed GetByUniqueId(int id, bool ignoreInvisibleFeeds);
    }
}
