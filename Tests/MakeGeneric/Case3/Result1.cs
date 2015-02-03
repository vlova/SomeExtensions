// Make method generic
using System.Linq;
using System.Collections.Generic;

class Base {
}

class Source {
	private static TBase Ololo<TBase>(List<TBase> collection) {
		return collection.FirstOrDefault();
	}
}