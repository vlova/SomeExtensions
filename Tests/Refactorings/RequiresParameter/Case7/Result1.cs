// Require a?.Any()
using System;
using System.Diagnostics.Contracts;
using System.Linq;

class Source {
	private static void Ololo(char[] a) {
		Contract.Requires(a != null && a.Any());
	}
}