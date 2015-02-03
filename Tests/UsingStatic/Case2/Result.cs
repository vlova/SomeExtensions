// Add using static directive
using System;
using static System.Console;
using static System.String;

class Source {
	private static void Ololo() {
		WriteLine(Join(", ", new[] { 1, 2, 3, 4 }));
	}
}