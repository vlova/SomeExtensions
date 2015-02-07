﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SomeExtensions.Extensions {
	public static class EnumerableExtensions {
		public static int IndexOf<T>(this IEnumerable<T> collection, T itemToFind) {
			int index = 0;
			foreach (var item in collection) {
				if (item.Equals(itemToFind)) {
					return index;
				} else {
					index++;
				}
			}

			return -1;
		}

		public static bool HasAtLeast<T>(this IEnumerable<T> collection, int count) {
			int inCollectionCount = 0;
			using (var enumerator = collection.GetEnumerator()) {
				while (enumerator.MoveNext()) {
					inCollectionCount++;
					if (inCollectionCount >= count) {
						return true;
					}
				}
			}

			return false;
		}

		public static IEnumerable<T> Prepend<T>(this IEnumerable<T> collection, T preItem) {
			yield return preItem;

			foreach (var item in collection) {
				yield return item;
			}
		}

		public static IEnumerable<T> Append<T>(this IEnumerable<T> collection, T afterItem) {
			foreach (var item in collection) {
				yield return item;
			}

			yield return afterItem;
		}


		public static ICollection<T> ToCollection<T>(this IEnumerable<T> collection, CancellationToken token) {
			if (collection is ICollection<T>) {
				return collection as ICollection<T>;
			}

			var result = new List<T>();

			foreach (var item in collection) {
				token.ThrowIfCancellationRequested();

				result.Add(item);
			}

			return result;
        }

		public static bool IsEmpty<T>(this IEnumerable<T> collection) {
			if (collection == null) {
				return true;
			}

			using (var enumerator = collection.GetEnumerator()) {
				return !enumerator.MoveNext();
			}
		}

		public static bool IsSingle<T>(this IEnumerable<T> collection) {
			if (collection == null) {
				return false;
			}

			using (var enumerator = collection.GetEnumerator()) {
				return !enumerator.MoveNext() || !enumerator.MoveNext();
			}
		}

		public static IEnumerable<T> WhileOk<T>(this IEnumerable<T> collection, CancellationToken token) {
			foreach (var item in collection) {
				if (token.IsCancellationRequested) {
					break;
				}

				yield return item;
			}
		}

		public static T At<T>(this IEnumerable<T> collection, int position) {
			return collection.Skip(position).FirstOrDefault();
		}
	}
}
