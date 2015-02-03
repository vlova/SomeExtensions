using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;
using static Microsoft.CodeAnalysis.TypeKind;

namespace SomeExtensions.Refactorings.Contracts.Providers {
	internal class EnumIsDefinedProvider : IContractProvider {
		public bool CanRefactor(ContractParameter parameter) {
			return parameter.Type.TypeKind == Enum;
		}

		public ExpressionSyntax GetContractRequire(ContractParameter parameter) {
			return "Enum.IsDefined"
				.ToInvocation(
					SyntaxFactory.TypeOfExpression(parameter.Type.ToTypeSyntax()),
					parameter.Expression);
		}

		public string GetDescription(ContractParameter parameter) {
			return "Enum.IsDefined";
		}

		public IEnumerable<string> GetImportNamespaces(ContractParameter parameter) {
			yield return nameof(System);
		}
	}
}
