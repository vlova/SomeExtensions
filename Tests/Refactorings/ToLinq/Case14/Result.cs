// To linq
// testing transformation to intersect
using System.Collections.Generic;
using System.Linq;

static class Source {
	static string HashOfLowerCaseSequence(string chars, string chars2) {
		foreach (var @char in chars.Intersect(chars2))
		{
		}
	}
}