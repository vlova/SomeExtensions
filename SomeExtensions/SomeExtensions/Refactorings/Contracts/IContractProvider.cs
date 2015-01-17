using System.Collections.Generic;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Refactorings.Contracts {
	interface IContractProvider {
		bool CanRefactor(ContractParameter parameter);

		string GetDescription(ContractParameter parameter);

		IEnumerable<string> GetImportNamespaces(ContractParameter parameter);

		ExpressionSyntax GetContractRequire(ContractParameter parameter);
	}
}
