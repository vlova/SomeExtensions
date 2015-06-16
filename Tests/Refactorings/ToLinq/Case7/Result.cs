// To linq
// testing transformation to aggregate
using System.Collections.Generic;
using System.Linq;

static class Source {
	static int HashOfLowerCaseSequence(string chars) {
		int hashcode = chars.Aggregate(1, (hashcode, @char) => hashcode * 17 + @char.GetHashCode());
		return hashcode;
	}
}
