﻿// Convert to static method call
using System;
using System.Linq;

class Source {
	private static void Ololo(string[] args) {
		Console.WriteLine(Enumerable.Concat(args, args));
	}
}