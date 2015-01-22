using System.Collections.Generic;
using System.Threading;

namespace SomeExtensions.Extensions {
	public static class EnumerableExtensions {
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
	}
}
