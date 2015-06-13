// To linq
// testing transformation to ofType
using System.Collections.Generic;
using System.Linq;

static class Source {
	static string HashOfLowerCaseSequence(string chars) {
		foreach (var @castedChar in chars.OfType<string>())
		{
		}
	}
}