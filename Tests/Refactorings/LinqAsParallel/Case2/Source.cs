using System;
using System.Linq;

class Source {
	private static void Ololo(string[] args) {
		var res = Enumerableº
			.Range(0, 100)
			.Select(x => Math.Pow(x, 2))
			.Where(x => x > 100)
			.Sum();
	}
}