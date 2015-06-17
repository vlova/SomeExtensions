// To new task #2
using System;
using System.Threading.Tasks;

class Source {
	private static async Task<int> Ololo() {
		var task0 = Task.Run(() => Comp1() + Comp2());
		return await task0;
	}

	private static int Comp1() {
		return 1;
	}

	private static int Comp2() {
		return 2;
	}
}