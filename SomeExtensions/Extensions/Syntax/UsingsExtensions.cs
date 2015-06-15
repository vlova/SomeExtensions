using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace SomeExtensions.Extensions.Syntax {
	public static class UsingsExtensions {
		public static bool HasStaticUsingOf(this CompilationUnitSyntax unit, string name) {
			return unit.Usings
				.Any(r => r.StaticKeyword.IsKind(StaticKeyword)
					&& r.Name?.GetText().ToString() == name);
		}

		public static bool HasUsingOf(this CompilationUnitSyntax unit, string name) {
			return unit.Usings
				.Any(r => r.Name?.GetText().ToString() == name);
		}

		public static CompilationUnitSyntax AddUsingIfNotExists(
			this CompilationUnitSyntax unit,
			string name,
			bool @static = false) {
			if (unit.HasUsingOf(name)) {
				return unit;
			}

			var @using = name.ToIdentifierName()
				.ToUsingDirective(@static)
				.Formattify();

			var newUsings = unit.Usings.Add(@using);
			return unit.WithUsings(newUsings);
		}
	}
}
