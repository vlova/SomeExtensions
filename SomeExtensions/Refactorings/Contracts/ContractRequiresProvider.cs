﻿using System.Composition;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions.Syntax;
using SomeExtensions.Extensions.Semantic;

using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.Contracts {
	// TODO: support of static import of contracts
	// TODO: support of properties and indexes
	[ExportCodeRefactoringProvider(nameof(ContractRequiresProvider), CSharp), Shared]
	internal class ContractRequiresProvider : BaseRefactoringProvider<ParameterSyntax> {
		protected override int? FindUpLimit => 3;

		protected override async Task ComputeRefactoringsAsync(RefactoringContext context, ParameterSyntax methodParameter) {
			var method = methodParameter?.FindUp<BaseMethodDeclarationSyntax>();

			if (method?.Body == null) return;

			var parameterName = methodParameter.Identifier.Text;

			if (IsAlreadyDefined(method.Body, parameterName)) return;

			var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);

			var contractParameter = new ContractParameter(
				parameterName,
				GetParameterExpression(parameterName),
				methodParameter?.Default?.Value,
				semanticModel.GetTypeSymbol(methodParameter.Type)
			);

			foreach (var provider in Helpers.Providers) {
				if (provider.CanRefactor(contractParameter)) {
					context.Register(new AddInOutContractRefactoring(
						method,
						contractParameter,
						provider,
						ContractKind.Require));
				}
			}
		}

		private static IdentifierNameSyntax GetParameterExpression(string parameterName) {
			return parameterName.ToIdentifierName();
		}

		private static bool IsAlreadyDefined(BlockSyntax body, string parameterName) {
			return body.Statements
				.FindContractRequires()
				.Select(contract => contract.GetFirstArgument())
				.Any(x => x.ContainsEquivalentNode(GetParameterExpression(parameterName)));
		}
	}
}
