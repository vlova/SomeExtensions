// To new task
using System;
using System.Threading.Tasks;

class Source {
	private static async Task<int> Ololo() {
		var task0 = Task.Run(() => Comp1());
		var task1 = Task.Run(() => Comp2());
		return await task0 + (await task1);
	}

	private static int Comp1() {
		return 1;
	}

	private static int Comp2() {
		return 2;
	}
}