using System.Linq;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Extensions.Syntax {
	public static class UsingsExtensions {
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
				.Nicefy();

			return unit.AddUsings(@using);
		}
	}
}
