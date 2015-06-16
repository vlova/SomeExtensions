// testing transformation to sum
using System.Collections.Generic;
using System.Linq;

static class Source {
	static int HashOfLowerCaseSequence(string chars) {
		int hashcode = 0;
		foreach (var @chºar in chars) {
			var charHash = @char.GetHashCode();
			hashcode = hashcode + charHash;
		}
		return hashcode;
	}
}
