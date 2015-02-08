using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Semantic;
using SomeExtensions.Extensions.Syntax;
using static Microsoft.CodeAnalysis.SpecialType;

namespace SomeExtensions.Refactorings.Contracts.Providers {
	internal class CollectionIsNotEmptyProvider : IContractProvider {
		public bool CanRefactor(ContractParameter parameter) {
			if (parameter.DefaultValue.IsEquivalentToNull()) {
				return false;
			}

			if (parameter.Type.SpecialType == System_String) {
				return false;
			}

			if (parameter.Type.IsCollectionType()) {
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
