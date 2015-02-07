// To readonly property with backing field
class Source {
	private readonly static double _field;

	private static double Field {
		get {
			return _field;
		}
	}
}
