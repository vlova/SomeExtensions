// Require a?.Any()
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

class Source {
	private static void Ololo(IEnumerable<char> a) {
		Contract.Requires(a != null && a.Any());
	}
}