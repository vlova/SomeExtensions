// Apply De Morgan's law
class Source {
	private static void Ololo() {
		bool? a = true;
		bool? b = false;
		var q = (!a && !b) ?? false;
	}
}