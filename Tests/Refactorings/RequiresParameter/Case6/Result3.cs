﻿// Require a != null
using System;
using System.Diagnostics.Contracts;

class Source {
	private static void Ololo(string a) {
		Contract.Requires(a != null);
	}
}