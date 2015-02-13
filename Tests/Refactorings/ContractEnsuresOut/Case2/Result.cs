// Ensure out a != null
using System;
using System.Diagnostics.Contracts;

class Source {
	private static int Ololo(out Source a) {
		Contract.Ensures(Contract.ValueAtReturn(out a) != null);
		Contract.Ensures(Contract.Result<int>() > 0);
		return 2;
	}
}