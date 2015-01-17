using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;

namespace SomeExtensions.Refactorings.Contracts.Providers {
	internal class EnumIsDefinedProvider : IContractProvider {
		public bool CanRefactor(ContractParameter parameter) {
			return parameter.Type.TypeKind == TypeKind.Enum;
		}

		public ExpressionSyntax GetContractRequire(ContractParameter parameter) {
			return "Enum.IsDefined"
				.ToInvocation(
					SyntaxFactory.TypeOfExpression(parameter.Type.ToTypeSyntax()),
					parameter.Expression)
				.ToLogicalNot();
		}

		public string GetDescription(ContractParameter parameter) {
			return "Enum.IsDefined";
		}

		public IEnumerable<string> GetImportNamespaces(ContractParameter parameter) {
			yield return nameof(System);
		}
	}
}
