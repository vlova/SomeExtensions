using System.Collections.Generic;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;

namespace SomeExtensions.Refactorings.Contracts.Providers {
	internal class NotNullProvider : IContractProvider {
		public bool CanRefactor(ContractParameter parameter) {
			if (parameter.DefaultValue.IsEquivalentToNull()) {
				return false;
			}

			return !(parameter.Type?.IsValueType ?? false);
		}

		public string GetDescription(ContractParameter parameter) {
			return parameter.Name + " != null";
		}

		public ExpressionSyntax GetContractRequire(ContractParameter parameter) {
			return parameter.Expression.ToNotNull();
		}

		public IEnumerable<string> GetImportNamespaces(ContractParameter parameter) {
			yield break;
		}
	}
}
