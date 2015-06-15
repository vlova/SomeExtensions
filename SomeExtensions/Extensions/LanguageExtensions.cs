using System;
using System.Linq;

namespace SomeExtensions.Extensions {
	public static class LanguageExtensions {
		public static TResult F<T, TResult>(this T obj, Func<T, TResult> rewriter) {
			CancellationTokenExtensions.ThrowOnCancellation();
			return rewriter(obj);
		}

		public static T F<T>(this T obj, Action<T> rewriter) {
			CancellationTokenExtensions.ThrowOnCancellation();
			rewriter(obj);
			return obj;
		}

        public static T If<T>(this T obj, Predicate<T> condition, Func<T, T> rewriter) {
			CancellationTokenExtensions.ThrowOnCancellation();
			return condition(obj) ? rewriter(obj) : obj;
		}

		public static T If<T>(this T obj, bool condition, Func<T, T> rewriter) {
			CancellationTokenExtensions.ThrowOnCancellation();
			return condition ? rewriter(obj) : obj;
		}

		public static T Unless<T>(this T obj, Predicate<T> condition) where T : class {
			CancellationTokenExtensions.ThrowOnCancellation();
			return condition(obj) ? null : obj;
		}

		public static bool In<T>(this T obj, params T[] collection) {
			CancellationTokenExtensions.ThrowOnCancellation();
			return collection.Contains(obj);
		}

        public static T As<T>(this object obj) where T : class {
			CancellationTokenExtensions.ThrowOnCancellation();
			return obj as T;
        }

		public static int? ParseInteger(this string parameter) {
			int result = 0;
			if (int.TryParse(parameter, out result)) {
				return result;
			} else {
				return null;
			}
		}

		public static long? ParseLong(this string parameter) {
			long result = 0;
			if (long.TryParse(parameter, out result)) {
				return result;
			}
			else {
				return null;
			}
		}
	}
}
