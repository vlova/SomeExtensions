// do not add contract ensures if method already has it
using System;
using System.Diagnostics.Contracts;

class Source {
	private static Source Ololo(Source ºa) {
		Contract.Ensures(Contract.Result<Source>() != null);
	}
}