using System.Collections;
using System.Collections.Generic;

namespace TypeLite.ReadOnlyDictionary {
	/// <summary>
	/// Implements read-only wrapper of a dictionary.
	/// </summary>
	/// <typeparam name="TKey">The key of the dictionary entry.</typeparam>
	/// <typeparam name="TValue">The value at the dictionary entry.</typeparam>
	public class ReadOnlyDictionaryWrapper<TKey, TValue> : IReadOnlyDictionary<TKey, TValue> {
		private readonly IDictionary<TKey, TValue> _dictionary;

		/// <summary>
		/// Gets the number of items in the collection. 
		/// </summary>
		public int Count {
			get {
				return _dictionary.Count;
			}
		}

		/// <summary>
		/// Gets the item that exists at a specified key.
		/// </summary>
		/// <param name="key">The specified key.></param>
		/// <returns>The item stored with the specific key.</returns>
		public TValue this[TKey key] {
			get { return _dictionary[key]; }
		}

		/// <summary>
		/// Gets the collection of keys in the dictionary.
		/// </summary>
		public IEnumerable<TKey> Keys {
			get {
				return _dictionary.Keys;
			}
		}

		/// <summary>
		/// Gets the collection of values in the dictionary.
		/// </summary>
		public IEnumerable<TValue> Values {
			get {
				return _dictionary.Values;
			}
		}

		/// <summary>
		/// Initializes a new instance of the ReadOnlyDictionaryWrapper class with the specific source dictionary.
		/// </summary>
		/// <param name="dictionary">The source dictionary</param>
		public ReadOnlyDictionaryWrapper(IDictionary<TKey, TValue> dictionary) {
			_dictionary = dictionary;
		}

		/// <summary>
		/// Determines whether the dictionary contains a specific key.
		/// </summary>
		/// <param name="key">The specified key to search for.</param>
		/// <returns>true if the key is found; otherwise, false.</returns>
		public bool ContainsKey(TKey key) {
			return _dictionary.ContainsKey(key);
		}

		/// <summary>
		/// Determines whether a specified value is stored at a specified key in the dictionary.
		/// </summary>
		/// <param name="key">The specified key.</param>
		/// <param name="value">The value to search for at the specified key.</param>
		/// <returns>true if the value is found; otherwise, false.</returns>
		public bool TryGetValue(TKey key, out TValue value) {
			return _dictionary.TryGetValue(key, out value);
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>A IEnumerator that can be used to iterate through the collection.</returns>
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
			return _dictionary.GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>A IEnumerator that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}
