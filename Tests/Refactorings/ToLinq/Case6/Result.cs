// To linq
// testing transformation to where->takewhile->select
using System.Collections.Generic;
using System.Linq;

static class Source {
	static string HashOfLowerCaseSequence(string chars) {
		var sequence = chars.Where(@char => char.IsLower(@char)).TakeWhile(@char => !char.IsUpper(@char)).Select(@char => @char.GetHashCode()).ToList();

		return sequence.Aggregate(0, (s, h) => s * 17 + h);
	}
}
