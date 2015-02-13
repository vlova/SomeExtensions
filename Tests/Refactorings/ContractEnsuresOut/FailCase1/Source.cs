using System;
using System.Diagnostics.Contracts;

class Source {
	private static void Ololo(Source ºa) {
		Contract.Ensures(Contract.ValueAtReturn(out a) != null);
	}
}