﻿// testing transformation to where-takeWhile
using System.Collections.Generic;
using System.Linq;

static class Source {
	static string HashOfLowerCaseSequence(string chars) {
		foreach (var @ºchar in chars) {
			if (!char.IsLower(@char)) continue;
			if (!char.IsUpper(@char)) break;
		}
	}
}