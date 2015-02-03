// Use type ICollection<T>
using System;
using System.Collections.Generic;

static class Source {
	private static void Ololo<T>(this ICollection<T> collection) {
	}
}