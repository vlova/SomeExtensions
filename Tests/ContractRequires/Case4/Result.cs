// Require Enum.IsDefined
using System;
using System.Diagnostics.Contracts;

enum EnumA {

}

class Source {
	private static void Ololo(EnumA a) {
		Contract.Requires(Enum.IsDefined(typeof(EnumA), a));
	}
}