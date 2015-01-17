using System.Linq;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Extensions {
	public static class CompilationUnitExtensions {
		public static bool HasUsingOf(this CompilationUnitSyntax unit, string name) {
			return unit.Usings
				.Any(r => r.Name?.GetText().ToString() == name);
		}

		public static CompilationUnitSyntax AddUsingIfNotExists(
			this CompilationUnitSyntax unit,
			string name) {
			if (unit.HasUsingOf(name)) {
				return unit;
			}

			return unit.AddUsings(name.ToIdentifier().ToUsing().Nicefy());
		}
	}
}
