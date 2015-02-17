// testing transformation to where->takewhile->select
using System.Collections.Generic;
using System.Linq;

static class Source {
	static string HashOfLowerCaseSequence(string chars) {
		var sequence = new List<int>();
		foreach (var @chaºr in chars) {
			if (!char.IsLower(@char)) continue;
			if (char.IsUpper(@char)) break;

			sequence.Add(@char.GetHashCode());
		}

		return sequence.Aggregate(0, (s, h) => s * 17 + h);
	}
}
