﻿// Add using static directive
using System;
using static System.Console;
using System.Linq;
using static System.Linq.Enumerable;

class Source {
	private static void Ololo() {
		WriteLine(string.Join(", ", Range(0, 100)));
	}
}