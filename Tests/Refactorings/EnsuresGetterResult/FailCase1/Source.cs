// do not add contract ensures if property already has it
using System;
using System.Diagnostics.Contracts;

class Source {
	private Source ºOlolo {
		get {
			Contract.Ensures(Contract.Result<Source>() != null);
			return null;
		}
	}
}