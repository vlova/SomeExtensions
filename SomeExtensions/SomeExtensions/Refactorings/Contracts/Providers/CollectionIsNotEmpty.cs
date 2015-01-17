using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;

namespace SomeExtensions.Refactorings.Contracts.Providers {
	internal class CollectionIsNotEmptyProvider : IContractProvider {
		private static SpecialType[] _collectionTypes = new[] {
			SpecialType.System_Array,
			SpecialType.System_Collections_Generic_ICollection_T,
			SpecialType.System_Collections_Generic_IEnumerable_T,
			SpecialType.System_Collections_Generic_IList_T,
			SpecialType.System_Collections_Generic_IReadOnlyCollection_T,
			SpecialType.System_Collections_Generic_IReadOnlyList_T,
			SpecialType.System_Collections_IEnumerable
		};

		public bool CanRefactor(ContractParameter parameter) {
			if (parameter.Type.SpecialType == SpecialType.System_String) {
				return false;
			}

			if (parameter.Type.TypeKind == TypeKind.Array) {
				return true;
			}

			if (parameter.Type.SpecialType.In(_collectionTypes)) {
				return true;
			}

			return parameter.Type
				.AllInterfaces
				.Any(r => r.SpecialType.In(_collectionTypes));
		}

		// TODO: special cases for arrays/collections & c#
		// like: array?.Length > 0
		public ExpressionSyntax GetContractRequire(ContractParameter parameter) {
			var notNull = parameter.Expression.NotNull();
			var anyElement = parameter.Expression.AccessTo("Any").ToInvocation();

			return notNull.And(anyElement);
		}

		public string GetDescription(ContractParameter parameter) {
			return string.Format("{0}?.Any()", parameter.Name);
		}

		public IEnumerable<string> GetImportNamespaces(ContractParameter parameter) {
			yield return typeof(Enumerable).Namespace;
		}
	}
}
