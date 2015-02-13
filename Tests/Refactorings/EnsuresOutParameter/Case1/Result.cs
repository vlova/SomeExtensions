// Ensure out a != null
using System;
using System.Diagnostics.Contracts;

class Source {
	private static void Ololo(out Source a) {
		Contract.Ensures(Contract.ValueAtReturn(out a) != null);
	}
}