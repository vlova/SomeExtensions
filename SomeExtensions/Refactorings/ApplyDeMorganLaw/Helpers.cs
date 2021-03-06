﻿using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using static System.Diagnostics.Contracts.Contract;

namespace SomeExtensions.Refactorings.ApplyDeMorganLaw {
	static class Helpers {
		private static Dictionary<SyntaxKind, SyntaxKind> _conversionDictionary
			= new Dictionary<SyntaxKind, SyntaxKind>();

		static Helpers() {
			RegisterConversion(LogicalOrExpression, LogicalAndExpression);
			RegisterConversion(BitwiseOrExpression, BitwiseAndExpression);
		}

		private static void RegisterConversion(SyntaxKind first, SyntaxKind second) {
			_conversionDictionary[first] = second;
			_conversionDictionary[second] = first;
		}

		public static SyntaxKind Convert(SyntaxKind kind) {
			Assume(_conversionDictionary.ContainsKey(kind));
			return _conversionDictionary[kind];
		}

		public static bool CanConvert(SyntaxKind kind) {
			return _conversionDictionary.ContainsKey(kind);
		}
	}
}
