﻿// Create fluent builder
using System.Diagnostics;

class Source {
	private readonly int _field1;
	private readonly int? _field2;
	private readonly uint _field3;
	private readonly string _field4;
	private double Field5 { get; }

	public Source(int field1, int? field2, uint field3, string field4, double field5) {
		_field1 = field1;
		_field2 = field2;
		_field3 = field3;
		_field4 = field4;
		Field5 = field5;
	}

	public class Builder {
		private int _field1;
		private int? _field2;
		private uint _field3;
		private string _field4;
		private double _field5;

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
		public Builder WithField4(string field4) {
			_field4 = field4;
			return this;
		}

		[DebuggerStepThrough]
		public Builder WithField5(double field5) {
			_field5 = field5;
			return this;
		}

		[DebuggerStepThrough]
		public Source Build() {
			return new Source(_field1, _field2, _field3, _field4, _field5);
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
			builder.WithField4(argument._field4);
			builder.WithField5(argument.Field5);
			return builder;
		}
	}
}
