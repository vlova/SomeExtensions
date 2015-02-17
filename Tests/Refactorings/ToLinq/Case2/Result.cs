// To linq
// testing transformation to where
using System.Collections.Generic;
using System.Linq;

static class Source {
	static string HashOfLowerCaseSequence(string chars) {
		foreach (var @char in chars.Where(@char => !char.IsLower(@char))) {
		}
	}
}