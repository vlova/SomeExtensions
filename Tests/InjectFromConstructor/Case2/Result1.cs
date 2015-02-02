// Inject from constructor
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.InjectFromConstructor.Case2 {
	class Source {
		private int _field0;
		private int _field;

		public Source(int field0, int field) {
			_field0 = field0;
			_field = field;
		}
	}
}
