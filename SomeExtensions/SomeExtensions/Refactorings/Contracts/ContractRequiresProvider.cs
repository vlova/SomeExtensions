using System.Composition;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions.Roslyn;
using SomeExtensions.Extensions.Syntax;
using SomeExtensions.Extensions.Semantic;

using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.Contracts {
	// TODO: support of properties and indexes
	// TODO: support of Ensures
	[ExportCodeRefactoringProvider(nameof(ContractRequiresProvider), CSharp), Shared]
	public class ContractRequiresProvider : BaseRefactoringProvider<ParameterSyntax> {
		protected override int? FindUpLimit => 3;

		protected override async Task ComputeRefactoringsAsync(CodeRefactoringContext context, SyntaxNode root, ParameterSyntax methodParameter) {
			var method = methodParameter?.FindUp<BaseMethodDeclarationSyntax>();

			if (method?.Body == null)
				return;

			var parameterName = methodParameter.Identifier.Text;

			if (IsAlreadyDefined(method, parameterName)) {
				return;
			}

			var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);

			var contractParameter = new ContractParameter(
				parameterName,
				parameterName.ToIdentifierName(),
				methodParameter?.Default?.Value,
                semanticModel.GetTypeSymbol(methodParameter.Type)
			);

			foreach (var provider in Helpers.Providers) {
				if (provider.CanRefactor(contractParameter)) {
						context.RegisterRefactoring(root, new ContractRequiresRefactoring(method, contractParameter, provider));
				}
			}
		}

		private static bool IsAlreadyDefined(BaseMethodDeclarationSyntax method, string parameterName) {
			var parameterIdentifier = parameterName.ToIdentifierName();

			return method.Body.Statements
				.FindContractRequires()
				.Select(contract => contract.ArgumentList.Arguments.FirstOrDefault())
				.Any(x => x
					?.DescendantNodes()
					?.Any(r => r.IsEquivalentTo(parameterIdentifier, topLevel: false))
					?? false);
		}
	}
}
