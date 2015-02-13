// Ensure result != null
using System;
using System.Diagnostics.Contracts;

class Source {
	private Source Ololo {
		get {
			Contract.Ensures(Contract.Result<Source>() != null);
			return null;
		}
	}
}