﻿// Move variable to inner block
using System;

class Source {
	private static void Ololo(string[] args) {
		Console.WriteLine("hello1");
		var a = int.Parse(args[0]);
		var b = int.Parse(args[1]);
		var result1 = a + b;
		var c = int.Parse(args[2]);
		if (result1 < 0) {
			Console.WriteLine(c);
			return;
		}
		else {
			Console.WriteLine("hello inner");
			var d = c + int.Parse(args[3]);
			var result2 = c + d;
			var data = new int[1];
			data[0] = result1 * result2;
			Console.WriteLine(data[0]);
			Console.WriteLine("hello2");
		}
	}
}