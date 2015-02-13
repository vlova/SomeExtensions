using System;
using System.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Semantic;
using SomeExtensions.Extensions.Syntax;
using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.Contracts {
	// TODO: support of static import of contracts
	// TODO: support of properties and indexes
	[ExportCodeRefactoringProvider(nameof(ContractEnsuresResultProvider), CSharp), Shared]
	internal class ContractEnsuresResultProvider : BaseRefactoringProvider<MethodDeclarationSyntax> {
		protected override int? FindUpLimit => 3;

		protected override async Task ComputeRefactoringsAsync(RefactoringContext context, MethodDeclarationSyntax method) {
			if (method?.Body == null)
				return;

			if (IsAlreadyDefined(method.Body, GetParameterExpression(method.ReturnType))) {
				return;
			}

			var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);

			var contractParameter = new ContractParameter(
				"result",
				GetParameterExpression(method.ReturnType),
				defaultValue: null,
				type: semanticModel.GetTypeSymbol(method.ReturnType)
			);

			foreach (var provider in Helpers.Providers) {
				if (provider.CanRefactor(contractParameter)) {
					context.Register(new AddInOutContractRefactoring(
						method,
						contractParameter,
						provider,
						ContractKind.Ensure));
				}
			}
		}

		private ExpressionSyntax GetParameterExpression(TypeSyntax returnType) {
			return nameof(Contract)
				.AccessTo(nameof(Contract.Result).MakeGeneric(returnType))
				.ToInvocation();
		}

		private static bool IsAlreadyDefined(BlockSyntax body, ExpressionSyntax expression) {
			return body.Statements
				.FindContractEnsures()
				.Select(contract => contract.GetFirstArgument())
				.Any(x => x.ContainsEquivalentNode(expression));
		}
	}
}
