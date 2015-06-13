// testing transformation to aggregate
using System.Collections.Generic;
using System.Linq;

static class Source {
	static string HashOfLowerCaseSequence(string chars) {
		int hashcode = 1;
		foreach (var @chºar in chars) {
			hashcode = hashcode * 17 + @char.GetHashCode();
		}
		return hashcode;
	}
}
