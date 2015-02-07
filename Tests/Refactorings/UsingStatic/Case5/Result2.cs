// Add using static directive (fix all)
using System;
using static System.Console;
using System.Linq;
using static System.Linq.Enumerable;

class Source {
	private static void Ololo() {
		WriteLine(string.Join(", ",
			Concat(
				Range(0, 100)),
				Range(0, 100)));
	}
}