// Make method generic
using System.Linq;
using System.Collections.Generic;

class Source {
	private static TInt Ololo<TInt>(List<TInt> collection) {
		return collection.Sum();
	}
}