// To linq
// testing transformation to aggregate
using System.Collections.Generic;
using System.Linq;

static class Source {
	static string HashOfLowerCaseSequence(string chars) {
		int hashcode = sequence.Aggregate(1, (seed, @char) => seed * 17 + @char.GetHashCode());
		return hashcode;
	}
}
