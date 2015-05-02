// Copyright © 2015 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InterfaceFactory;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Manages a self-expiring collection of items.
    /// </summary>
    /// <remarks><para>
    /// When an item is added to this collection it is given a timestamp. The items in the collection are
    /// periodically checked, if they have been in the collection for longer than <see cref="ExpireMilliseconds"/>
    /// then they are removed.
    /// </para><para>
    /// This class is disposable. However, Dispose() just clears the collection. Clearing the collection releases
    /// all hooks, so it is kind-of permissible to not dispose of these objects - once the last item in the collection
    /// expires the class will automatically unhook itself and become eligible for finalising (if there are no strong
    /// references to it). However it is better if you can dispose of it when you are finished with it.
    /// </para></remarks>
    /// <typeparam name="TItem">The type of item being held by the collection.</typeparam>
    public abstract class ExpiringCollection<TItem> : IDisposable
        where TItem: class
    {
        #region Private class - CollectionItem
        /// <summary>
        /// An instance of a collection item and its associated timestamp.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        protected class CollectionItem<T>
            where T: class
        {
            /// <summary>
            /// Gets or sets the item in the collection.
            /// </summary>
            public T Item { get; set; }

            /// <summary>
            /// Gets or sets the timestamp associated with the item.
            /// </summary>
            public DateTime Timestamp { get; set; }

            /// <summary>
            /// Creates a new object.
            /// </summary>
            /// <param name="item"></param>
            /// <param name="utcNow"></param>
            public CollectionItem(T item, DateTime utcNow)
            {
                Item = item;
                Timestamp = utcNow;
            }

            /// <summary>
            /// Calls ToString on the item.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return Item == null ? "" : Item.ToString();
            }

            /// <summary>
            /// Calls Equals on the item.
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override bool Equals(object obj)
            {
                return Object.Equals(obj, Item);
            }

            /// <summary>
            /// Calls GetHashCode on the item.
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return Item == null ? -1 : Item.GetHashCode();
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The lock object that controls access to the other fields.
        /// </summary>
        protected object _SyncLock = new object();

        /// <summary>
        /// The clock that provides timestamps.
        /// </summary>
        protected IClock _Clock;

        /// <summary>
        /// The heartbeat service to use.
        /// </summary>
        private IHeartbeatService _HeartbeatService;

        /// <summary>
        /// True if the slow heartbeat event has been hooked.
        /// </summary>
        private bool _SlowHeartbeatHooked;

        /// <summary>
        /// True if the fast heartbeat event has been hooked.
        /// </summary>
        private bool _FastHeartbeatHooked;

        /// <summary>
        /// The time that the list was last checked for expired items.
        /// </summary>
        private DateTime _LastCheckUtc;

        /// <summary>
        /// True while the <see cref="CountChangedDelegate"/> is running.
        /// </summary>
        private bool _CountChangedDelegateRunning;
        #endregion

        #region Properties
        private int _ExpireMilliseconds;
        /// <summary>
        /// Gets or sets the minimum number of milliseconds that an item can remain within the list.
        /// </summary>
        public int ExpireMilliseconds
        {
            get { return _ExpireMilliseconds; }
            set { DoChangeIntervals(value, MillisecondsBetweenChecks); }
        }

        private int _MillisecondsBetweenChecks;
        /// <summary>
        /// Gets or sets the minimum number of milliseconds to wait between checks for expiring items.
        /// </summary>
        public int MillisecondsBetweenChecks
        {
            get { return _MillisecondsBetweenChecks; }
            set { DoChangeIntervals(ExpireMilliseconds, value); }
        }

        /// <summary>
        /// Gets or sets an optional delegate that is called when the count changes. The new count is
        /// passed to the delegate. The list is locked to the calling thread while the delegate is running.
        /// The delegate will occasionally be called on a background thread.
        /// </summary>
        /// <remarks><para>
        /// If you set this property to null just before the list calls it then the list may still go through
        /// with the call. Your delegate can be called while this property is null.
        /// </para><para>
        /// The intention here is to provide a way for owners of the list to quickly update a counter somewhere
        /// when the list count changes. It is not intended for anything more than that. If you modify the list
        /// from within the delegate then you will not get recursive calls to the delegate.
        /// </para>
        /// </remarks>
        public Action<int> CountChangedDelegate { get; set; }

        /// <summary>
        /// Gets or sets an optional delegate that is called before the check is made for expired items.
        /// </summary>
        /// <remarks>
        /// The delegate will be called on a background thread. The list is not locked at the time that the delegate
        /// is called.
        /// </remarks>
        public Action BeforeCheckForExpiredItemsDelegate { get; set; }

        /// <summary>
        /// Gets or sets an optional delegate that is called after the check for expired items is made.
        /// </summary>
        public Action AfterCheckForExpiredItemsDelegate { get; set; }

        /// <summary>
        /// When overridden by the derivee this returns the current count of items in the collection.
        /// </summary>
        public abstract long Count
        {
            get;
        }
        #endregion

        #region Ctors, finaliser
        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="expireMilliseconds"></param>
        /// <param name="millisecondsBetweenChecks"></param>
        public ExpiringCollection(int expireMilliseconds, int millisecondsBetweenChecks)
        {
            ExpireMilliseconds = expireMilliseconds;
            MillisecondsBetweenChecks = millisecondsBetweenChecks;

            _Clock = Factory.Singleton.Resolve<IClock>();
            _HeartbeatService = Factory.Singleton.Resolve<IHeartbeatService>().Singleton;
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~ExpiringCollection()
        {
            Dispose(false);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                Clear();
                UnhookHeartbeat();      // Shouldn't be necessary, but just in case a derivee isn't unhooking on clear...
            }
        }
        #endregion

        #region Heartbeat handling - HookHeartbeat, UnhookHeartbeat, ChangeIntervals
        /// <summary>
        /// Hooks the heartbeat event. Must be called from within a lock.
        /// </summary>
        /// <remarks>
        /// The derivee should call this whenever they add the first item to the collection.
        /// </remarks>
        protected void HookHeartbeat()
        {
            if(!_SlowHeartbeatHooked && !_FastHeartbeatHooked) {
                var useSlowHeartbeat = MillisecondsBetweenChecks >= 10000;
                if(useSlowHeartbeat) {
                    _SlowHeartbeatHooked = true;
                    _HeartbeatService.SlowTick += HeartbeatService_Tick;
                } else {
                    _FastHeartbeatHooked = true;
                    _HeartbeatService.FastTick += HeartbeatService_Tick;
                }
            }
        }

        /// <summary>
        /// Unhooks the heartbeat service if hooked. Must be called from within a lock.
        /// </summary>
        /// <returns>True if the heartbeat was hooked when the function was called, false otherwise.</returns>
        /// <remarks>
        /// The derivee should call this whenever they remove the last item from the collection.
        /// </remarks>
        protected bool UnhookHeartbeat()
        {
            var result = _SlowHeartbeatHooked || _FastHeartbeatHooked;

            if(result) {
                if(_SlowHeartbeatHooked)        _HeartbeatService.SlowTick -= HeartbeatService_Tick;
                else if(_FastHeartbeatHooked)   _HeartbeatService.FastTick -= HeartbeatService_Tick;

                _SlowHeartbeatHooked = _FastHeartbeatHooked = false;
            }

            return result;
        }

        /// <summary>
        /// Changes the intervals for the expiration of items and the interval between checks.
        /// </summary>
        /// <param name="expireMilliseconds"></param>
        /// <param name="millisecondsBetweenChecks"></param>
        public void ChangeIntervals(int expireMilliseconds, int millisecondsBetweenChecks)
        {
            DoChangeIntervals(expireMilliseconds, millisecondsBetweenChecks);
        }

        /// <summary>
        /// Changes the intervals, unhooking and re-hooking the heartbeat timer if appropriate.
        /// </summary>
        /// <param name="expireMilliseconds"></param>
        /// <param name="millisecondsBetweenChecks"></param>
        private void DoChangeIntervals(int expireMilliseconds, int millisecondsBetweenChecks)
        {
            lock(_SyncLock) {
                if(_ExpireMilliseconds != expireMilliseconds || _MillisecondsBetweenChecks != millisecondsBetweenChecks) {
                    var wasHooked = _MillisecondsBetweenChecks != millisecondsBetweenChecks && UnhookHeartbeat();

                    _ExpireMilliseconds = expireMilliseconds;
                    _MillisecondsBetweenChecks = millisecondsBetweenChecks;

                    if(wasHooked) {
                        HookHeartbeat();
                    }
                }
            }
        }
        #endregion

        #region RemoveExpiredItems, OnCountChanged
        /// <summary>
        /// Removes expired items.
        /// </summary>
        /// <remarks>
        /// This is automatically called on a timer. If you call this manually then it will reset the timer, the
        /// next check for expired items will not take place until at least <see cref="MillisecondsBetweenChecks"/>
        /// milliseconds have passed.
        /// </remarks>
        public void RemoveExpiredItems()
        {
            lock(_SyncLock) {
                var now = _Clock.UtcNow;
                var threshold = now.AddMilliseconds(-ExpireMilliseconds);
                DoRemoveExpiredItems(threshold);

                _LastCheckUtc = now;
            }
        }

        /// <summary>
        /// Called by the derivee to inform the owner of the collection that the count has changed. Always call this
        /// from within a lock.
        /// </summary>
        /// <param name="newCount"></param>
        protected virtual void OnCountChanged(int newCount)
        {
            var countChangedDelegate = CountChangedDelegate;
            if(countChangedDelegate != null && !_CountChangedDelegateRunning) {
                _CountChangedDelegateRunning = true;
                try {
                    countChangedDelegate(newCount);
                } finally {
                    _CountChangedDelegateRunning = false;
                }
            }
        }
        #endregion

        #region Lock
        /// <summary>
        /// Calls the action passed across with the list locked. Do not perform an action that will attempt
        /// to lock the list from another thread, it will block.
        /// </summary>
        /// <param name="action"></param>
        public void Lock(Action action)
        {
            lock(_SyncLock) {
                action();
            }
        }
        #endregion

        #region Stubs - Clear, DoRemoveExpiredItems
        /// <summary>
        /// When overridden by a derivee this clears the collection and unhooks all events.
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// When overridden by the derivee this should remove all items that have a timestamp that is less than or
        /// equal to the threshold passed across.
        /// </summary>
        /// <param name="threshold"></param>
        protected abstract void DoRemoveExpiredItems(DateTime threshold);
        #endregion

        #region Events consumed
        /// <summary>
        /// Called when the heartbeat service raises a tick event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HeartbeatService_Tick(object sender, EventArgs args)
        {
            var threshold = _Clock.UtcNow.AddMilliseconds(-MillisecondsBetweenChecks);
            if(_LastCheckUtc <= threshold) {
                var beforeCheckForExpiredItems = BeforeCheckForExpiredItemsDelegate;
                if(beforeCheckForExpiredItems != null) {
                    beforeCheckForExpiredItems();
                }

                RemoveExpiredItems();

                var afterCheckForExpiredItems = AfterCheckForExpiredItemsDelegate;
                if(afterCheckForExpiredItems != null) {
                    afterCheckForExpiredItems();
                }
            }
        }
        #endregion
    }
}
