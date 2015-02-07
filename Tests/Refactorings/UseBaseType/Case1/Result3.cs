// Use type IReadOnlyList<T>
using System;
using System.Collections.Generic;

static class Source {
	private static void Ololo<T>(this IReadOnlyList<T> collection) {
	}
}