// To new task
using System;
using System.Threading.Tasks;

class Source {
	private static async Task<int> Ololo() {
		var task0 = Task.Run(() => Comp1());
		var task1 = Task.Run(() => Comp2());
		var task2 = Task.Run(async () => await task0 + await task1);
		return await task2;
	}

	private static int Comp1() {
		return 1;
	}

	private static int Comp2() {
		return 2;
	}
}