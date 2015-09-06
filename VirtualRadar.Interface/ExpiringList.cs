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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Manages a self-expiring list of items.
    /// </summary>
    /// <remarks>
    /// When an item is added to this list it is given a timestamp. The items in the list are
    /// periodically checked, if they have been in the list for longer than ExpireMilliseconds
    /// then they are removed. This class is thread-safe.
    /// </remarks>
    public class ExpiringList<T> : ExpiringCollection<T>
        where T: class
    {
        #region Fields
        /// <summary>
        /// The underlying list of collection items.
        /// </summary>
        private List<CollectionItem<T>> _List = new List<CollectionItem<T>>();
        #endregion

        #region Properties
        /// <summary>
        /// Gets the current count of items in the list.
        /// </summary>
        public override long Count
        {
            get { lock(_SyncLock) return _List.Count; }
        }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="expireMilliseconds"></param>
        /// <param name="millisecondsBetweenChecks"></param>
        public ExpiringList(int expireMilliseconds, int millisecondsBetweenChecks) : base(expireMilliseconds, millisecondsBetweenChecks)
        {
            ;
        }
        #endregion

        #region Abstract implementations - DoRemoveExpiredItems
        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="threshold"></param>
        protected override void DoRemoveExpiredItems(DateTime threshold)
        {
            var startCount = _List.Count;

            if(startCount > 0) {
                for(var i = _List.Count - 1;i >= 0;--i) {
                    if(_List[i].Timestamp <= threshold) {
                        _List.RemoveAt(i);
                    }
                }

                if(_List.Count == 0) {
                    UnhookHeartbeat();
                }

                if(startCount != _List.Count) {
                    OnCountChanged(_List.Count);
                }
            }
        }
        #endregion

        #region Utility methods - FindCollectionItem, DoAdd, DoAddOrRefresh, DoRemove
        /// <summary>
        /// Finds the collection item for the item passed across.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <remarks>
        /// This must be called from within a lock.
        /// </remarks>
        protected ExpiringCollection<T>.CollectionItem<T> FindCollectionItem(T item)
        {
            var result = _List.FirstOrDefault(r => r.Equals(item));
            return result;
        }

        /// <summary>
        /// Does the work of adding an item to the list. Must be called from within a lock.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="raiseCountChanged"></param>
        private void DoAdd(T item, bool raiseCountChanged)
        {
            var collectionItem = new ExpiringCollection<T>.CollectionItem<T>(item, _Clock.UtcNow);
            _List.Add(collectionItem);

            if(_List.Count == 1) {
                HookHeartbeat();
            }

            if(raiseCountChanged) {
                OnCountChanged(_List.Count);
            }
        }

        /// <summary>
        /// Does the work of the AddOrRefresh methods. Must be called from within a lock.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="now"></param>
        /// <param name="raiseCountChanged"></param>
        /// <returns></returns>
        private bool DoAddOrRefresh(T item, DateTime now, bool raiseCountChanged)
        {
            var collectionItem = FindCollectionItem(item);
            if(collectionItem != null) {
                collectionItem.Timestamp = now;
            } else {
                DoAdd(item, raiseCountChanged);
            }

            return collectionItem != null;
        }

        /// <summary>
        /// Does the work of removing an item from the list. Must be called from within a lock.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="raiseCountChanged"></param>
        /// <returns></returns>
        private bool DoRemove(T item, bool raiseCountChanged)
        {
            var collectionItem = FindCollectionItem(item);
            if(collectionItem != null) {
                _List.Remove(collectionItem);
                if(raiseCountChanged) {
                    OnCountChanged(_List.Count);
                }
            }

            return collectionItem != null;
        }
        #endregion

        #region Public methods - Add, AddOrRefresh, AddRange, AddRangeOrRefresh, Clear, Find, FindAndRefresh, FindAll, FindAllAndRefresh, RefreshAll, Remove, RemoveRange, Snapshot, SnapshotAndRefresh
        /// <summary>
        /// Adds the item to the list.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void Add(T item)
        {
            lock(_SyncLock) {
                DoAdd(item, raiseCountChanged: true);
            }
        }

        /// <summary>
        /// Adds the item if it is not already in the list. If it is in the list then its
        /// timestamp is refreshed to the current time.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>True if the item was already in the list and was just refreshed, false if it had to be added.</returns>
        public bool AddOrRefresh(T item)
        {
            lock(_SyncLock) {
                return DoAddOrRefresh(item, _Clock.UtcNow, raiseCountChanged: true);
            }
        }

        /// <summary>
        /// Adds a range of items to the list.
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(IEnumerable<T> items)
        {
            lock(_SyncLock) {
                var countChanged = false;
                foreach(var item in items) {
                    DoAdd(item, raiseCountChanged: false);
                    countChanged = true;
                }
                if(countChanged) {
                    OnCountChanged(_List.Count);
                }
            }
        }

        /// <summary>
        /// Adds a range of items to the list. Items that already exist in the list have their timestamps refreshed.
        /// </summary>
        /// <param name="items"></param>
        public void AddRangeOrRefresh(IEnumerable<T> items)
        {
            lock(_SyncLock) {
                var countChanged = false;
                var now = _Clock.UtcNow;
                foreach(var item in items) {
                    DoAddOrRefresh(item, now, raiseCountChanged: false);
                    countChanged = true;
                }
                if(countChanged) {
                    OnCountChanged(_List.Count);
                }
            }
        }

        /// <summary>
        /// Empties the list.
        /// </summary>
        public override void Clear()
        {
            lock(_SyncLock) {
                _List.Clear();
                UnhookHeartbeat();
                OnCountChanged(_List.Count);
            }
        }

        /// <summary>
        /// Returns the item that matches the predicate or null if no such item exists.
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public T Find(Predicate<T> match)
        {
            lock(_SyncLock) {
                var result = _List.Find((CollectionItem<T> item) => { return match(item.Item); });
                return result == null ? null : result.Item;
            }
        }

        /// <summary>
        /// Returns the item that matches the predicate or null if no such item exists. Refreshes
        /// the timestamp for the item.
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public T FindAndRefresh(Predicate<T> match)
        {
            lock(_SyncLock) {
                var result = _List.Find((CollectionItem<T> item) => { return match(item.Item); });
                if(result != null) result.Timestamp = _Clock.UtcNow;

                return result == null ? null : result.Item;
            }
        }

        /// <summary>
        /// Returns all of the items that match the predicate or an empty list if no such items exist.
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public List<T> FindAll(Predicate<T> match)
        {
            lock(_SyncLock) {
                var result = _List.FindAll((CollectionItem<T> item) => { return match(item.Item); });
                return result == null ? new List<T>() : result.Select(r => r.Item).ToList();
            }
        }

        /// <summary>
        /// Returns all of the items that match the predicate or an empty list if no such items exist.
        /// Refreshes the timestamps for all of the items returned.
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public List<T> FindAllAndRefresh(Predicate<T> match)
        {
            lock(_SyncLock) {
                var result = _List.FindAll((CollectionItem<T> item) => { return match(item.Item); });
                var now = _Clock.UtcNow;
                foreach(var collectionItem in result) {
                    collectionItem.Timestamp = now;
                }

                return result.Select(r => r.Item).ToList();
            }
        }

        /// <summary>
        /// Refreshes the timestamps on every item in the list.
        /// </summary>
        public void RefreshAll()
        {
            lock(_SyncLock) {
                var now = _Clock.UtcNow;
                foreach(var item in _List) {
                    item.Timestamp = now;
                }
            }
        }

        /// <summary>
        /// Removes the item from the list.
        /// </summary>
        /// <param name="item"></param>
        public void Remove(T item)
        {
            lock(_SyncLock) {
                DoRemove(item, raiseCountChanged: true);
                if(_List.Count == 0) {
                    UnhookHeartbeat();
                }
            }
        }

        /// <summary>
        /// Removes a range of items from the list.
        /// </summary>
        /// <param name="items"></param>
        public void RemoveRange(IEnumerable<T> items)
        {
            lock(_SyncLock) {
                var countChanged = false;
                foreach(var item in items) {
                    if(DoRemove(item, raiseCountChanged: false)) countChanged = true;
                }
                if(_List.Count == 0) {
                    UnhookHeartbeat();
                }
                if(countChanged) {
                    OnCountChanged(_List.Count);
                }
            }
        }

        /// <summary>
        /// Returns a clone of the contents of the list.
        /// </summary>
        /// <returns></returns>
        public T[] Snapshot()
        {
            lock(_SyncLock) {
                return _List.Select(r => r.Item).ToArray();
            }
        }

        /// <summary>
        /// Returns a clone of every item in the list. Refreshes the timestamps on every item.
        /// </summary>
        /// <returns></returns>
        public T[] SnapshotAndRefresh()
        {
            lock(_SyncLock) {
                RefreshAll();
                return Snapshot();
            }
        }
        #endregion
    }
}
