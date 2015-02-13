using System.Composition;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions.Syntax;
using SomeExtensions.Extensions.Semantic;

using static Microsoft.CodeAnalysis.LanguageNames;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace SomeExtensions.Refactorings.Contracts {
	// TODO: support of static import of contracts
	// TODO: support of properties and indexes
	[ExportCodeRefactoringProvider(nameof(ContractEnsuresOutProvider), CSharp), Shared]
	internal class ContractEnsuresOutProvider : BaseRefactoringProvider<ParameterSyntax> {
		protected override int? FindUpLimit => 3;

		protected override async Task ComputeRefactoringsAsync(RefactoringContext context, ParameterSyntax methodParameter) {
			if (!methodParameter.Modifiers.Any(OutKeyword)) return;
			var method = methodParameter?.FindUp<BaseMethodDeclarationSyntax>();

			if (method?.Body == null) return;
			var parameterName = methodParameter.Identifier.Text;

			if (IsAlreadyDefined(method.Body, parameterName)) return;

			var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);

			var contractParameter = new ContractParameter(
				$"out {parameterName}",
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
						ContractKind.Ensure));
				}
			}
		}

		private ExpressionSyntax GetParameterExpression(string parameterName) {
			return "Contract.ValueAtReturn"
				.ToInvocation(Argument(parameterName.ToIdentifierName()).WithOutKeyword());
		}

		private bool IsAlreadyDefined(BlockSyntax body, string parameterName) {
			var parameter = GetParameterExpression(parameterName);

			return body.Statements
				.FindContractRequires()
				.Select(contract => contract.GetFirstArgument())
				.Any(x => x.ContainsEquivalentNode(parameter));
		}
	}
}
