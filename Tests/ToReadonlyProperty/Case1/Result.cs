// To readonly property with backing field
class Source {
	private readonly double _field;

	private double Field {
		get {
			return _field;
		}
	}
}
