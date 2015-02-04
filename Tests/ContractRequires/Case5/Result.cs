// Require b != null
using System;
using System.Diagnostics.Contracts;

class Source {
	private static void Ololo(Source a, Source b) {
		Contract.Requires(a != null);
		Contract.Requires(b != null);
		Console.WriteLine(a);
	}
}