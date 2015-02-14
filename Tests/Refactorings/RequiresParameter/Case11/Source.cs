// just check that static import handled correctly
using System;
using static System.Diagnostics.Contracts.Contract;

class Source {
	private int this[int ºa] {
		get {
			return a;
		}
		set {
		}
	}
}