using System;
using System.Diagnostics.Contracts;

class Source {
	private static void Ololo(out Source ºa, int b) {
		Contract.Requires(b > 0);
	}
}