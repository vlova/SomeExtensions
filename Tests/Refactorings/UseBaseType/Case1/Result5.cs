// Use type IEnumerable<T>
using System;
using System.Collections.Generic;

static class Source {
	private static void Ololo<T>(this IEnumerable<T> collection) {
	}
}