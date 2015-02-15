using System;
using System.Diagnostics.Contracts;

class Source {
	private static void Ololo(out Source ºa) {
		Contract.Ensures(Contract.ValueAtReturn(out a) != null);
	}
}