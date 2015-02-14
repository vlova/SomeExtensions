// Require a > 0
// just check that static import handled correctly
using System;
using static System.Diagnostics.Contracts.Contract;

class Source {
	private int this[int a] {
		get {
			Requires(a > 0);
			return a;
		}
		set {
			Requires(a > 0);
		}
	}
}