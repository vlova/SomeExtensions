using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;

using static Microsoft.CodeAnalysis.SpecialType;

namespace SomeExtensions.Refactorings.Contracts.Providers {
	internal class StringNotWhitespaceProvider : IContractProvider {
		public bool CanRefactor(ContractParameter parameter) {
			if (parameter.DefaultValue.IsEquivalentToNull()) {
				return false;
			}

			return parameter.Type.SpecialType == System_String;
		}

		public ExpressionSyntax GetContractRequire(ContractParameter parameter) {
			return "string.IsNullOrWhiteSpace"
				.ToInvocation(parameter.Expression)
				.ToLogicalNot();
		}

		public string GetDescription(ContractParameter parameter) {
			return "! NullOrWhiteSpace";
		}

		public IEnumerable<string> GetImportNamespaces(ContractParameter parameter) {
			yield break;
		}
	}
}
