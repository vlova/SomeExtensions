// Move variable to inner block
using System;

class Source {
	private static void Ololo(string[] args) {
		Console.WriteLine("hello1");
		var a = int.Parse(args[0]);
		var b = int.Parse(args[1]);
		var result1 = a + b;
		if (result1 < 0) {
			return;
		}
		else {
			Console.WriteLine("hello inner");
			var c = int.Parse(args[2]);
			var d = c + int.Parse(args[3]);
			var result2 = c + d;
			var data = new int[1];
			var e = d + c;
			data[0] = result1 * result2 + e;
			Console.WriteLine(data[0]);
			Console.WriteLine("hello2");
		}
	}
}