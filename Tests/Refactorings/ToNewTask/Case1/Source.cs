using System;
using System.Threading.Tasks;

class Source {
	private static async Task<int> Ololo() {
		return Coºmp1() + Comp2();
    }

	private static int Comp1() {
		return 1;
	}

	private static int Comp2() {
		return 2;
	}
}