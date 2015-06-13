// testing transformation to where->takewhile
using System.Collections.Generic;
using System.Linq;

static class Source {
	static string HashOfLowerCaseSequence(string chars) {
		var sequence = new List<char>();
		foreach (var @chaºr in chars) {
			if (!char.IsLower(@char)) continue;
			if (char.IsUpper(@char)) break;

			sequence.Add(@char);
		}

		return sequence.Aggregate(0, (s, h) => s * 17 + h);
	}
}
