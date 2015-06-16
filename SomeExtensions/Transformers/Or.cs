using System;

namespace SomeExtensions.Transformers {
	public struct Or<T1, T2> {
		private readonly int _kind;
		private readonly T1 _item1;
		private readonly T2 _item2;

		public Or(T1 item1) {
			_kind = 1;
			_item1 = item1;
			_item2 = default(T2);
		}

		// If T1 == T2, then item1 = value, item2 = null
		public Or(T2 item2) {
			if (typeof(T1) == typeof(T2)) {
				_kind = 1;
				_item2 = default(T2);
				_item1 = (T1)(object)item2;
			}
			else {
				_item1 = default(T1);
				_item2 = item2;
				_kind = 2;
			}
		}

		public TResult Match<TResult>(Func<T1, TResult> func1, Func<T2, TResult> func2) {
			return _kind == 1
				? func1(_item1)
				: func2(_item2);
		}

		public bool IsNull() {
			return _kind == 1 ? _item1 == null : _item2 == null;
		}

		public static implicit operator Or<T1, T2>(T1 item) {
			return new Or<T1, T2>(item);
		}

		public static implicit operator Or<T1, T2>(T2 item) {
			return new Or<T1, T2>(item);
		}
	}
}