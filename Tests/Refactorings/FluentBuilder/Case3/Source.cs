using System.Diagnostics;

class Source {
	private readonly int _field1;
	private readonly int? _field2;
	private readonly uint _field3;
	private readonly string _field4;
	private readonly double Field5 { get; }

	public class Builder {
		private int _field1;
		private int? _field2;
		private uint _field3;

		[DebuggerStepThrough]
		public Builder WithField1(int field1) {
			_field1 = field1;
			return this;
		}

		[DebuggerStepThrough]
		public Builder WithField2(int? field2) {
			_field2 = field2;
			return this;
		}

		[DebuggerStepThrough]
		public Builder WithField3(uint field3) {
			_field3 = field3;
			return this;
		}

		[DebuggerStepThrough]
		public Source Build() {
			return new Source(_field1, _field2, _field3);
		}

		[DebuggerStepThrough]
		public static implicit operator Source(Builder argument) {
			return argument.Build();
		}

		[DebuggerStepThrough]
		public static implicit operator Builder(Source argument) {
			Builder builder = new Builder();
			builder.WithField1(argument._field1);
			builder.WithField2(argument._field2);
			builder.WithField3(argument._field3);
			return builder;
		}
	}

	public Sourcºe(int field1 = 1, int? field2 = null, uint field3 = 0, string field4 = "abc", double field5 = 5.0) {
		_field1 = field1;
		_field2 = field2;
		_field3 = field3;
		_field4 = field4;
		Field5 = field5;
    }
}
