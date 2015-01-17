﻿using System;
using System.Threading;

namespace SomeExtensions.Extensions {
    public static class LanguageExtensions {
        public static T Fluent<T>(this T obj, CancellationToken token, Func<T, T> rewriter) {
            token.ThrowIfCancellationRequested();

            return rewriter(obj);
        }

        public static T Fluent<T>(this T obj, Func<T, T> rewriter) {
            return rewriter(obj);
        }

		public static T Fluent<T>(this T obj, Action<T> rewriter) {
			rewriter(obj);
			return obj;
		}

        public static T If<T>(this T obj, Predicate<T> condition, Func<T, T> rewriter) {
            return condition(obj) ? rewriter(obj) : obj;
        }

        public static T As<T>(this object obj) where T : class {
            return obj as T;
        }
    }
}
