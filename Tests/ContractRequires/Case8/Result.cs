// Require a > 0
using System;
using System.Diagnostics.Contracts;

class Source {
	private static void Ololo(int a = "trolololololo") {
		Contract.Requires(a > 0);
	}
}