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
    /// Manages a self-expiring dictionary of items.
    /// </summary>
    /// <remarks>
    /// When an item is added to this dictionary it is given a timestamp. The items in the dictionary are
    /// periodically checked, if they have been in the map for longer than <see cref="ExpireMilliseconds"/>
    /// then they are removed. This class is thread-safe.
    /// </remarks>
    /// <typeparam name="TItem">The type of item being held by the collection.</typeparam>
    public class ExpiringDictionary<TKey, TValue> : ExpiringCollection<TValue>
        where TValue: class
    {
        #region Fields
        /// <summary>
        /// The underlying dictionary.
        /// </summary>
        private Dictionary<TKey, ExpiringCollection<TValue>.CollectionItem<TValue>> _Dictionary = new Dictionary<TKey,CollectionItem<TValue>>();
        #endregion

        #region Properties
        /// <summary>
        /// Gets the current count of items in the dictionary.
        /// </summary>
        public override long Count
        {
            get { lock(_SyncLock) return _Dictionary.Count; }
        }

        /// <summary>
        /// Gets or sets an optional delegate that returns the collection of the keys of expired items. If this delegate is present
        /// then it is always used in preference to the timestamps attached to the item and the Refresh methods will have no effect.
        /// </summary>
        public Func<ExpiringDictionary<TKey, TValue>, IEnumerable<TKey>> GetExpiredItemsDelegate { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="expireMilliseconds"></param>
        /// <param name="millisecondsBetweenChecks"></param>
        public ExpiringDictionary(int expireMilliseconds, int millisecondsBetweenChecks) : base(expireMilliseconds, millisecondsBetweenChecks)
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
            if(_Dictionary.Count > 0) {
                var getExpiredItemsDelegate = GetExpiredItemsDelegate;
                var removeKeys = getExpiredItemsDelegate == null ? new List<TKey>() : getExpiredItemsDelegate(this).ToList();
                if(getExpiredItemsDelegate == null) {
                    foreach(var kvp in _Dictionary) {
                        if(kvp.Value.Timestamp <= threshold) {
                            removeKeys.Add(kvp.Key);
                        }
                    }
                }
                foreach(var removeKey in removeKeys) {
                    _Dictionary.Remove(removeKey);
                }

                if(_Dictionary.Count == 0) {
                    UnhookHeartbeat();
                }
                if(removeKeys.Count > 0) {
                    OnCountChanged(_Dictionary.Count);
                }
            }
        }
        #endregion

        #region Utility methods - DoAdd, DoFind, DoFindAll, DoGetForKey, DoUpsert
        /// <summary>
        /// Does the work of adding a new item to the dictionary. The key must not already exist.
        /// Must be called from within a lock.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="raiseCountChanged"></param>
        private void DoAdd(TKey key, TValue value, bool raiseCountChanged)
        {
            var collectionItem = new ExpiringCollection<TValue>.CollectionItem<TValue>(value, _Clock.UtcNow);
            _Dictionary.Add(key, collectionItem);

            if(_Dictionary.Count == 1) {
                HookHeartbeat();
            }

            if(raiseCountChanged) {
                OnCountChanged(_Dictionary.Count);
            }
        }

        /// <summary>
        /// Does the work for the Find methods. Must be called from within a lock.
        /// </summary>
        /// <param name="match"></param>
        /// <param name="refreshTimestamp"></param>
        /// <returns></returns>
        private TValue DoFind(Predicate<TValue> match, bool refreshTimestamp)
        {
            TValue result = null;

            var collectionItem = _Dictionary.Values.FirstOrDefault(r => match(r.Item));
            if(collectionItem != null) {
                result = collectionItem.Item;
                if(refreshTimestamp) {
                    collectionItem.Timestamp = _Clock.UtcNow;
                }
            }

            return result;
        }

        /// <summary>
        /// Does the work for the FindAll methods. Must be called from within a lock.
        /// </summary>
        /// <param name="match"></param>
        /// <param name="refreshTimestamp"></param>
        /// <returns></returns>
        private List<TValue> DoFindAll(Predicate<TValue> match, bool refreshTimestamp)
        {
            var result = new List<TValue>();

            var now = _Clock.UtcNow;
            var collectionItems = _Dictionary.Values.Where(r => match(r.Item));
            foreach(var collectionItem in collectionItems) {
                result.Add(collectionItem.Item);
                if(refreshTimestamp) {
                    collectionItem.Timestamp = now;
                }
            }

            return result;
        }

        /// <summary>
        /// Does the work for the GetForKey methods. Must be called from within a lock.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="refreshTimestamp"></param>
        /// <returns></returns>
        private TValue DoGetForKey(TKey key, bool refreshTimestamp)
        {
            TValue result = null;

            ExpiringCollection<TValue>.CollectionItem<TValue> collectionItem;
            if(_Dictionary.TryGetValue(key, out collectionItem)) {
                result = collectionItem.Item;
                if(refreshTimestamp) {
                    collectionItem.Timestamp = _Clock.UtcNow;
                }
            }

            return result;
        }

        /// <summary>
        /// Does the work of upserting an item in the dictionary. Must be called from within a lock.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="now"></param>
        /// <param name="refreshTimestamp"></param>
        /// <param name="raiseCountChanged"></param>
        /// <returns>True if the item existed, false if it did not.</returns>
        private bool DoUpsert(TKey key, TValue value, DateTime now, bool refreshTimestamp, bool raiseCountChanged)
        {
            var result = false;

            ExpiringCollection<TValue>.CollectionItem<TValue> existingCollectionItem;
            if(!(result = _Dictionary.TryGetValue(key, out existingCollectionItem))) {
                DoAdd(key, value, raiseCountChanged);
            } else {
                existingCollectionItem.Item = value;
                if(refreshTimestamp) {
                    existingCollectionItem.Timestamp = now;
                }
            }

            return result;
        }
        #endregion

        #region Public methods - Add, Clear, Find, FindAll, FindAndRefresh, FindAllAndRefresh, GetForKey, GetForKeyAndRefresh, RefreshAll, RemoveIfExists, Snapshot, SnapshotAndRefresh, Upsert, UpsertOrRefresh
        /// <summary>
        /// Adds an item to the dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TKey key, TValue value)
        {
            lock(_SyncLock) {
                DoAdd(key, value, raiseCountChanged: true);
            }
        }

        /// <summary>
        /// Empties the dictionary.
        /// </summary>
        public override void Clear()
        {
            lock(_SyncLock) {
                _Dictionary.Clear();
                UnhookHeartbeat();
                OnCountChanged(_Dictionary.Count);
            }
        }

        // I HAVE NOT WRITTEN A ContainsKey ON PURPOSE. IT IS MEANINGLESS, THE ITEM COULD EXIST WHEN
        // ContainsKey RETURNS TRUE BUT BE REMOVED BY THE TIME THE CALLER ATTEMPTS A RETRIEVAL. USE
        // GetForKey() INSTEAD.
        private bool ContainsKey(TKey key)
        {
            // Just in case I forget in the future why I did this and add a ContainsKey() out of alphabetical order :)
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the first value that matches the predicate passed across or null if no
        /// such value could be found.
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public TValue Find(Predicate<TValue> match)
        {
            lock(_SyncLock) {
                return DoFind(match, refreshTimestamp: false);
            }
        }

        /// <summary>
        /// Returns a list of all values that match the predicate passed across.
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public List<TValue> FindAll(Predicate<TValue> match)
        {
            lock(_SyncLock) {
                return DoFindAll(match, refreshTimestamp: false);
            }
        }

        /// <summary>
        /// Returns a list of all values that match the predicate passed across. Refreshes the
        /// timestamps for all items returned.
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public List<TValue> FindAllAndRefresh(Predicate<TValue> match)
        {
            lock(_SyncLock) {
                return DoFindAll(match, refreshTimestamp: true);
            }
        }

        /// <summary>
        /// Finds the first value that matches the predicate passed across or null if no
        /// such value could be found. Refreshes the timestamp on the value if it could be found.
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public TValue FindAndRefresh(Predicate<TValue> match)
        {
            lock(_SyncLock) {
                return DoFind(match, refreshTimestamp: true);
            }
        }

        /// <summary>
        /// Returns the item for the key passed across. Returns null if the key is no longer in the dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue GetForKey(TKey key)
        {
            lock(_SyncLock) {
                return DoGetForKey(key, refreshTimestamp: false);
            }
        }

        /// <summary>
        /// Returns the item for the key passed across. Returns null if the key is no longer in the dictionary.
        /// Refreshes the item's timestamp.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue GetForKeyAndRefresh(TKey key)
        {
            lock(_SyncLock) {
                return DoGetForKey(key, refreshTimestamp: true);
            }
        }

        /// <summary>
        /// Returns the item for the key passed across or calls the create method to create a new item and add it to the dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="create"></param>
        /// <returns></returns>
        public TValue GetOrCreate(TKey key, Func<TKey, TValue> create)
        {
            lock(_SyncLock) {
                var result = DoGetForKey(key, refreshTimestamp: false);
                if(result == null) {
                    result = create(key);
                    DoAdd(key, result, raiseCountChanged: true);
                }

                return result;
            }
        }

        /// <summary>
        /// Returns the item for the key passed across or calls the create method to create a new item and add it to the dictionary.
        /// If the item already exists then its timestamp is refreshed.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="create"></param>
        /// <returns></returns>
        public TValue GetAndRefreshOrCreate(TKey key, Func<TKey, TValue> create)
        {
            lock(_SyncLock) {
                var result = DoGetForKey(key, refreshTimestamp: true);
                if(result == null) {
                    result = create(key);
                    DoAdd(key, result, raiseCountChanged: true);
                }

                return result;
            }
        }

        /// <summary>
        /// Refreshes all of the timestamps in the dictionary.
        /// </summary>
        public void RefreshAll()
        {
            lock(_SyncLock) {
                var now = _Clock.UtcNow;
                foreach(var value in _Dictionary.Values) {
                    value.Timestamp = now;
                }
            }
        }

        /// <summary>
        /// Removes the item for the key passed across. Does nothing if the key does not exist.
        /// </summary>
        /// <param name="key"></param>
        public void RemoveIfExists(TKey key)
        {
            lock(_SyncLock) {
                if(_Dictionary.ContainsKey(key)) {
                    _Dictionary.Remove(key);
                    if(_Dictionary.Count == 0) {
                        UnhookHeartbeat();
                    }
                    OnCountChanged(_Dictionary.Count);
                }
            }
        }

        /// <summary>
        /// Returns a snapshot of all of the key-value pairs in the dictionary.
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<TKey, TValue>[] Snapshot()
        {
            lock(_SyncLock) {
                return _Dictionary.Select(r => new KeyValuePair<TKey, TValue>(r.Key, r.Value.Item)).ToArray();
            }
        }

        /// <summary>
        /// Returns a snapshot of all of the key-value pairs in the dictionary and refreshes the timestamps on all of them.
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<TKey, TValue>[] SnapshotAndRefresh()
        {
            lock(_SyncLock) {
                RefreshAll();
                return Snapshot();
            }
        }

        /// <summary>
        /// Adds or updates an item in the dictionary. If the key is already in use then the value is overwritten
        /// but the timestamp is not refreshed.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>True if the item existed and was updated, false if it had to be added.</returns>
        public bool Upsert(TKey key, TValue value)
        {
            lock(_SyncLock) {
                return DoUpsert(key, value, DateTime.MinValue, refreshTimestamp: false, raiseCountChanged: true);
            }
        }

        /// <summary>
        /// Adds or updates an item in the dictionary. If the key is already in use then the value is overwritten and
        /// the timestamp is refreshed.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>True if the item existed and was updated / refreshed, false if it had to be added.</returns>
        public bool UpsertAndRefresh(TKey key, TValue value)
        {
            lock(_SyncLock) {
                return DoUpsert(key, value, _Clock.UtcNow, refreshTimestamp: true, raiseCountChanged: true);
            }
        }

        /// <summary>
        /// Adds or updates a range of items. If an item exists then it is updated but its timestamp is not refreshed.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="extractKey"></param>
        public void UpsertRange(IEnumerable<TValue> values, Func<TValue, TKey> extractKey)
        {
            lock(_SyncLock) {
                var countChanged = false;
                foreach(var value in values) {
                    var key = extractKey(value);
                    DoUpsert(key, value, DateTime.MinValue, refreshTimestamp: false, raiseCountChanged: false);
                    countChanged = true;
                }
                if(countChanged) {
                    OnCountChanged(_Dictionary.Count);
                }
            }
        }

        /// <summary>
        /// Adds or updates a range of items. If an item exists then it is updated and its timestamp is refreshed.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="extractKey"></param>
        public void UpsertRangeAndRefresh(IEnumerable<TValue> values, Func<TValue, TKey> extractKey)
        {
            lock(_SyncLock) {
                var countChanged = false;
                var now = _Clock.UtcNow;
                foreach(var value in values) {
                    var key = extractKey(value);
                    DoUpsert(key, value, now, refreshTimestamp: true, raiseCountChanged: false);
                    countChanged = true;
                }
                if(countChanged) {
                    OnCountChanged(_Dictionary.Count);
                }
            }
        }
        #endregion
    }
}
