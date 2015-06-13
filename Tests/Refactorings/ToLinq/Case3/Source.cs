// testing transformation to takeWhile-where
using System.Collections.Generic;
using System.Linq;

static class Source {
	static string HashOfLowerCaseSequence(string chars) {
		foreach (var @ºchar in chars) {
			if (char.IsUpper(@char)) break;
			if (char.IsLower(@char)) continue;
		}
	}
}
