// Ensure out a != null
using System;
using System.Diagnostics.Contracts;

class Source {
	private static void Ololo(out Source a, int b) {
		Contract.Requires(b > 0);
		Contract.Ensures(Contract.ValueAtReturn(out a) != null);
	}
}