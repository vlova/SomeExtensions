// Add using static directive
using System;
using static System.Console;
using System.Linq;
using static System.Linq.Enumerable;

class Source {
	private static void Ololo() {
		WriteLine(string.Join(", ",
			Enumerable.Concat(
				Range(0, 100)),
				Enumerable.Range(0, 100)));
	}
}