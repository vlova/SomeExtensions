// Require a != null
using System;
using System.Diagnostics.Contracts;

class Source {
	private static void Ololo(Source a) {
		Contract.Requires(a != null);
	}
}