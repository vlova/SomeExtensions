using System;
using System.Diagnostics.Contracts;

class Source {
	private static void Ololo(Source a, Source ºb) {
		Contract.Requires(a != null);
		Console.WriteLine(a);
	}
}