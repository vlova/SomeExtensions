// Require a != null
using System;
using System.Diagnostics.Contracts;

class Source {
	private static void Ololo(char[] a) {
		Contract.Requires(a != null);
	}
}