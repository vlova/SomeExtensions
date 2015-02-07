// To ternary operator
using System;

static class Source {
	static void WriteMax(int a, int b) {
		return a > b
			? a * 2 - 5
			: b * 3 + 10;
	}
}