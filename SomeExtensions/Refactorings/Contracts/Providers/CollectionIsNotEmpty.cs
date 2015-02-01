using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;

using static Microsoft.CodeAnalysis.SpecialType;
using static Microsoft.CodeAnalysis.TypeKind;

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

			if (parameter.Type.TypeKind == Array) {
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
