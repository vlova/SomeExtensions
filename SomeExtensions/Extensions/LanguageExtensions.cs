﻿using System;
using System.Linq;
using System.Threading;

namespace SomeExtensions.Extensions {
	public static class LanguageExtensions {

		public static T Fluent<T>(this T obj, CancellationToken token, Func<T, T> rewriter) {
            token.ThrowIfCancellationRequested();

            return rewriter(obj);
        }

		public static TResult Fluent<T, TResult>(this T obj, Func<T, TResult> rewriter) {
			return rewriter(obj);
		}

		public static T Fluent<T>(this T obj, Action<T> rewriter) {
			rewriter(obj);
			return obj;
		}

        public static T If<T>(this T obj, Predicate<T> condition, Func<T, T> rewriter) {
            return condition(obj) ? rewriter(obj) : obj;
		}

		public static T If<T>(this T obj, bool condition, Func<T, T> rewriter) {
			return condition ? rewriter(obj) : obj;
		}

		public static T Unless<T>(this T obj, Predicate<T> condition) where T : class {
			return condition(obj) ? null : obj;
		}

		public static bool In<T>(this T obj, params T[] collection) {
			return collection.Contains(obj);
		}

        public static T As<T>(this object obj) where T : class {
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
    }
}