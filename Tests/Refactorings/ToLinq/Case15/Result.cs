// To linq
// testing transformation to sum
using System.Collections.Generic;
using System.Linq;

static class Source {
	static int HashOfLowerCaseSequence(string chars) {
		int hashcode = chars.Select(@char => @char.GetHashCode()).Sum();
		return hashcode;
	}
}
