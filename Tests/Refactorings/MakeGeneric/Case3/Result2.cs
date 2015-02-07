// Make method generic using base constraint
using System.Linq;
using System.Collections.Generic;

class Base {
}

class Source {
	private static TBase Ololo<TBase>(List<TBase> collection) where TBase : Base {
		return collection.FirstOrDefault();
	}
}