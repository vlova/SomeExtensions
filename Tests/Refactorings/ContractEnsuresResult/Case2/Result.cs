// Ensure result != null
using System;
using System.Diagnostics.Contracts;

class Source {
	private static Source Ololo(Source a) {
		Contract.Requires(a != null);
		Contract.Ensures(Contract.Result<Source>() != null);
	}
}