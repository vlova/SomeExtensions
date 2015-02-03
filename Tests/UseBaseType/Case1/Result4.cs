// Use type IReadOnlyCollection<T>
using System;
using System.Collections.Generic;

static class Source {
	private static void Ololo<T>(this IReadOnlyCollection<T> collection) {
	}
}