// Inject from all constructors
class Source {
	private int _field0;
	private int Field { get; }

	public Source(int field) {
		_field0 = 0;
		Field = field;
	}

	public Source(int field0, int field) {
		_field0 = field0;
		Field = field;
	}
}