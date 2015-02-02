// Create new public constructor
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.InjectFromConstructor.Case1 {
	class Source {
		private int _field;

		public Source(int field) {
			_field = field;
		}
	}
}
