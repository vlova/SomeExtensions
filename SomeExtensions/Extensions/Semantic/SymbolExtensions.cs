using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.SpecialType;
using static Microsoft.CodeAnalysis.SymbolDisplayFormat;

namespace SomeExtensions.Extensions.Semantic {
	public static class TypeSymbolExtensions {
		public static bool IsCollectionType(this ITypeSymbol type) {
			return type.IsCollectionTypeOf(predicate: el => true);
		}

		public static bool IsCollectionTypeOf(this ITypeSymbol type, ITypeSymbol elementType) {
			return type.IsCollectionTypeOf(el => elementType == el);
		}

		public static bool IsCollectionTypeOf(this ITypeSymbol type, Predicate<ITypeSymbol> predicate) {
			var arrayType = type.As<IArrayTypeSymbol>();
			if (arrayType != null) {
				return predicate(arrayType.ElementType);
			}

			var namedType = type.As<INamedTypeSymbol>();
			if (IsIEnumerableOf(namedType, predicate)) {
				return true;
			}

			if (type.AllInterfaces.Any(r => IsCollectionTypeOf(r, predicate))) {
				return true;
			}

			return false;
		}

		private static bool IsIEnumerableOf(INamedTypeSymbol namedType, Predicate<ITypeSymbol> predicate) {
			if (!namedType.IsGenericType) return false;

			var constructedType = namedType
				.ConstructUnboundGenericType()
				.ToDisplayString(FullyQualifiedFormat);

			if (constructedType == "global::System.Collections.Generic.IEnumerable<>") {
				var elementType = namedType.TypeArguments.First();
				return predicate(elementType);
            }
			else {
				return false;
			}
		}

		public static string GetFullName(this ITypeSymbol symbol) {
			var @namespace = symbol
				.ContainingNamespace
				.ToDisplayString();

			return @namespace + "." + symbol.Name;
		}

		public static bool IsNullable(this ITypeSymbol type) {
			return type.OriginalDefinition.SpecialType == System_Nullable_T;
		}
	}
}
