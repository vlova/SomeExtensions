using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SomeExtensions.Extensions {
	public static class EnumerableExtensions {
		public static int IndexOf<T>(this IEnumerable<T> collection, T itemToFind) {
			int index = 0;
			foreach (var item in collection) {
				if (item.Equals(itemToFind)) {
					return index;
				}
				else {
					index++;
				}
			}

			return -1;
		}

		public static bool HasNotMore<T>(this IEnumerable<T> collection, int count) {
			int inCollectionCount = 0;
			using (var enumerator = collection.GetEnumerator()) {
				while (enumerator.MoveNext()) {
					inCollectionCount++;
					if (count > inCollectionCount) {
						return false;
					}
				}
			}

			return true;
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

		public static IEnumerable<IEnumerable<T>> GetPatternMatches<T>(this IEnumerable<T> collection, params Predicate<T>[] predicates) {
			var list = collection.AsIList();
			for (int start = 0; start < list.Count - predicates.Length + 1; start++) {
				if (IsMatch(list, predicates, start)) {
					yield return list.Subset(start, predicates.Length);
				}
			}
		}

		public static IEnumerable<TRes> SelectLast<T, TRes>(this IEnumerable<IEnumerable<T>> collection, Func<T, TRes> selector) {
			return collection
				.Select(Enumerable.Last)
				.Select(selector);
		}

		private static bool IsMatch<T>(IList<T> list, Predicate<T>[] predicates, int start) {
			bool matchPattern = true;
			for (int matchIndex = 0; matchIndex < predicates.Length; matchIndex++) {
				var element = list[start + matchIndex];
				var predicate = predicates[matchIndex];
				if (!predicate(element)) {
					matchPattern = false;
				}
			}

			return matchPattern;
		}

		public static IEnumerable<T> Subset<T>(this IList<T> list, int start, int length) {
			var endPosition = Math.Min(list.Count, start + length);
			for (int i = start; i < endPosition; i++) {
				yield return list[i];
			}
		}

		public static IList<T> AsIList<T>(this IEnumerable<T> collection) {
			if (collection is IList<T>) {
				return collection as IList<T>;
			}
			else {
				return collection.ToList();
			}
		}
	}
}
