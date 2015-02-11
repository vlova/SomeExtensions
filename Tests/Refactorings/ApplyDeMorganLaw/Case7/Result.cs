// Apply De Morgan's law
class Source {
	private static void Ololo() {
		var a = true;
		var b = false;
		var q = !(a | true) || (b ^ false);
	}
}