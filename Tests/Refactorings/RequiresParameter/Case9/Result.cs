// Require a > 0
using System;
using System.Diagnostics.Contracts;

class Source {
	private int this[int a] {
		get {
			Contract.Requires(a > 0);
			return a;
		}
	}
}