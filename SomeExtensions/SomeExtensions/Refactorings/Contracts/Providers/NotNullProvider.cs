using System.Collections.Generic;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;

namespace SomeExtensions.Refactorings.Contracts.Providers {
	internal class NotNullProvider : IContractProvider {
		public bool CanRefactor(ContractParameter parameter) {
			return !(parameter.Type?.IsValueType ?? false);
		}

		public string GetDescription(ContractParameter parameter) {
			return parameter.Name + " != null";
		}

		public ExpressionSyntax GetContractRequire(ContractParameter parameter) {
			return parameter.Expression.NotNull();
		}

		public IEnumerable<string> GetImportNamespaces(ContractParameter parameter) {
			yield break;
		}
	}
}
