// To ternary operator
static class Source {
	static int Max(int a, int b) {
		int result;
		int result2;
		result = a > b ? a : b;
		result2 = a > b ? b : a;
		return result;
	}
}