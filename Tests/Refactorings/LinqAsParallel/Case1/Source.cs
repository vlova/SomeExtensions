using System;
using System.Linq;

class Source {
	private static void Ololo(string[] args) {
		var result = argsº
			.Select(a => a.Length)
			.Select(x => Math.Pow(x, 2))
			.Where(x => x > 100)
			.Sum();
	}
}