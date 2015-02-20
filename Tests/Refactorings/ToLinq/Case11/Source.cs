// testing transformation to cast
using System.Collections.Generic;
using System.Linq;

static class Source {
	static string HashOfLowerCaseSequence(string chars) {
		foreach (var @ºchar in chars) {
			var @castedChar = (string)(object)@char;
		}
	}
}
