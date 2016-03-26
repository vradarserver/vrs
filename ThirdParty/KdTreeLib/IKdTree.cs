using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KdTree
{
	public interface IKdTree<TKey, TValue>
	{
		bool Add(TKey[] point, TValue value);

		bool TryFindValueAt(TKey[] point, out TValue value);

		TValue FindValueAt(TKey[] point);

		bool TryFindValue(TValue value, out TKey[] point);

		TKey[] FindValue(TValue value);

		KdTreeNode<TKey, TValue>[] RadialSearch(TKey[] center, TKey radius, int count);

		void RemoveAt(TKey[] point);

		void Clear();

		KdTreeNode<TKey, TValue>[] GetNearestNeighbours(TKey[] point, int count = int.MaxValue);
		
		int Count { get; }
	}
}
