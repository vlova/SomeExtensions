// Add argument name
using System;
using System.Linq;

class Source {
	private static void Ololo(string[] args) {
		Console.WriteLine(args.Concat(second: args));
	}
}