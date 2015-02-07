using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;

using static Microsoft.CodeAnalysis.SpecialType;
using static Microsoft.CodeAnalysis.TypeKind;
using static Microsoft.CodeAnalysis.SymbolDisplayFormat;

namespace SomeExtensions.Refactorings.Contracts.Providers {
	internal class CollectionIsNotEmptyProvider : IContractProvider {
		private static SpecialType[] _collectionTypes = new[] {
			System_Array,
			System_Collections_Generic_ICollection_T,
			System_Collections_Generic_IEnumerable_T,
			System_Collections_Generic_IList_T,
			System_Collections_Generic_IReadOnlyCollection_T,
			System_Collections_Generic_IReadOnlyList_T,
			System_Collections_IEnumerable
		};

		public bool CanRefactor(ContractParameter parameter) {
			if (parameter.DefaultValue.IsEquivalentToNull()) {
				return false;
			}

			if (parameter.Type.SpecialType == System_String) {
				return false;
			}

			if (IsCollectionType(parameter.Type)) {
				return true;
			}

            return false;
		}

		private bool IsCollectionType(ITypeSymbol type) {
			if (type.TypeKind == Array) {
				return true;
			}

			if (type.SpecialType.In(_collectionTypes)) {
				return true;
			}

			var namedType = type.As<INamedTypeSymbol>();
            if (namedType.IsGenericType &&
				namedType?.ConstructUnboundGenericType()?.ToDisplayString(FullyQualifiedFormat) == "global::System.Collections.Generic.IEnumerable<>") {
				return true;
			}

			if (type.AllInterfaces.Any(r => IsCollectionType(r))) {
				return true;
			}

			return false;
		}

		public ExpressionSyntax GetContractRequire(ContractParameter parameter) {
			var notNull = parameter.Expression.ToNotNull();
			var anyElement = parameter.Expression.AccessTo("Any").ToInvocation();

			return notNull.ToAnd(anyElement);
		}

		public string GetDescription(ContractParameter parameter) {
			return string.Format("{0}?.Any()", parameter.Name);
		}

		public IEnumerable<string> GetImportNamespaces(ContractParameter parameter) {
			yield return typeof(Enumerable).Namespace;
		}
	}
}
