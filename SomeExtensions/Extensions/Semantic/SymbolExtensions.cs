using Microsoft.CodeAnalysis;

namespace SomeExtensions.Extensions.Semantic {
	public static class SymbolExtensions {
		public static string GetFullName(this ITypeSymbol _symbol) {
			var @namespace = _symbol
				.ContainingNamespace
				.ToDisplayString();

			return @namespace + "." + _symbol.Name;
		}
	}
}
