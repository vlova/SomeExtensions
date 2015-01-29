using Microsoft.CodeAnalysis;

using static Microsoft.CodeAnalysis.SymbolDisplayFormat;

namespace SomeExtensions.Extensions.Semantic {
	public static class SymbolExtensions {
		public static string GetFullName(this ITypeSymbol _symbol) {
			var @namespace = _symbol
				.ContainingNamespace
				.ToDisplayString(MinimallyQualifiedFormat);

			return @namespace + "." + _symbol.Name;
		}
	}
}
