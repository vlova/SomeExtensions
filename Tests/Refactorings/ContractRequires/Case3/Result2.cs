// Require a != null
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

class Source {
	private static void Ololo(IEnumerable<char> a) {
		Contract.Requires(a != null);
	}
}