using System;
using System.Diagnostics.Contracts;

class Source {
	private static int Ololo(out Source ºa) {
		Contract.Ensures(Contract.Result<int>() > 0);
		return 2;
	}
}