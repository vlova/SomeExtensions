using System;
using static System.Console;
using System.Linq;

class Source {
	private static void Ololo() {
		WriteLine(string.Join(", ",
			Enumerable.Concat(
				Enumerable.Rºange(0, 100)),
				Enumerable.Range(0, 100)));
	}
}