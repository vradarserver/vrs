using System.Collections.Generic;

namespace TypeLite.ReadOnlyDictionary {
	/// <summary>
	/// Defines methods and properties that handle read-only dictionary collections.
	/// </summary>
	/// <typeparam name="TKey">The key of the dictionary entry.</typeparam>
	/// <typeparam name="TValue">The value at the dictionary entry.</typeparam>
	public interface IReadOnlyDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> {
		/// <summary>
		/// Gets the number of items in the collection. 
		/// </summary>
		int Count { get; }

		/// <summary>
		/// Gets the item that exists at a specified key.
		/// </summary>
		/// <param name="key">The specified key.></param>
		/// <returns>The item stored with the specific key.</returns>
		TValue this[TKey key] { get; }

		/// <summary>
		/// Gets the collection of keys in the dictionary.
		/// </summary>
		IEnumerable<TKey> Keys { get; }

		/// <summary>
		/// Gets the collection of values in the dictionary.
		/// </summary>
		IEnumerable<TValue> Values { get; }

		/// <summary>
		/// Determines whether the dictionary contains a specific key.
		/// </summary>
		/// <param name="key">The specified key to search for.</param>
		/// <returns>true if the key is found; otherwise, false.</returns>
		bool ContainsKey(TKey key);

		/// <summary>
		/// Determines whether a specified value is stored at a specified key in the dictionary.
		/// </summary>
		/// <param name="key">The specified key.</param>
		/// <param name="value">The value to search for at the specified key.</param>
		/// <returns>true if the value is found; otherwise, false.</returns>
		bool TryGetValue(TKey key, out TValue value);
	}
}
