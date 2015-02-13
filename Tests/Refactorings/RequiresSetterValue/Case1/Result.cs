// Require value > 0
using System;
using System.Diagnostics.Contracts;

class Source {
	private int A {
		get {

		}
		set {
			Contract.Requires(value > 0);
			Console.WriteLine(value);
		}
	}
}