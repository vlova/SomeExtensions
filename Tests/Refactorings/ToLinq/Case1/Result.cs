// To linq
// testing transformation to takeWhile
using System.Collections.Generic;
using System.Linq;

static class Source {
	static string HashOfLowerCaseSequence(string chars) {
		foreach (var @char in chars.TakeWhile(@char => !char.IsUpper(@char))) {
		}
	}
}