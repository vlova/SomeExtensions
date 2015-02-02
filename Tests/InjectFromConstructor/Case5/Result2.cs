// Inject from constructor #2
class Source {
	private int _field0;
	private int Field { get; }

	public Source() {
		_field0 = 0;
	}

	public Source(int field0, int field) {
		_field0 = field0;
		Field = field;
	}
}