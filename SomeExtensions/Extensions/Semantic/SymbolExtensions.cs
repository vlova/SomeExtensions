using Microsoft.CodeAnalysis;

using static Microsoft.CodeAnalysis.SpecialType;

namespace SomeExtensions.Extensions.Semantic {
	public static class SymbolExtensions {
		public static string GetFullName(this ITypeSymbol _symbol) {
			var @namespace = _symbol
				.ContainingNamespace
				.ToDisplayString();

			return @namespace + "." + _symbol.Name;
		}

		private static bool IsNullable(ITypeSymbol type) {
			return type.OriginalDefinition.SpecialType == System_Nullable_T;
		}
	}
}
