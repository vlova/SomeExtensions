using System.Composition;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Refactorings.Contracts.Providers;

namespace SomeExtensions.Refactorings.Contracts {
	// TODO: support of properties and indexes
	// TODO: support of Ensures
	[ExportCodeRefactoringProvider(RefactoringId, LanguageNames.CSharp), Shared]
	public class ContractRequiresProvider : BaseRefactoringProvider {
		public const string RefactoringId = "ContractRequires";

		private static IContractProvider[] _providers = new IContractProvider[] {
			new NotNullProvider(),
			new StringNotEmptyProvider(),
			new StringNotWhitespaceProvider(),
			new IsPositiveProvider(),
			new EnumIsDefinedProvider(),
			new CollectionIsNotEmptyProvider(),
			// TODO: custom contracts
		};

		protected override async Task ComputeRefactoringsAsync(CodeRefactoringContext context, SyntaxNode root, SyntaxNode node) {
			var methodParameter = node.FindUp<ParameterSyntax>();
			var method = methodParameter?.FindUp<BaseMethodDeclarationSyntax>();

			if (method?.Body == null)
				return;

			var parameterName = methodParameter.Identifier.Text;

			if (IsAlreadyDefined(method, parameterName)) {
				return;
			}

			var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);

			var typeInfo = semanticModel.GetSpeculativeTypeSymbol(methodParameter.Type);
			var contractParameter = new ContractParameter(
				parameterName,
				parameterName.ToIdentifierName(),
				typeInfo
			);

			foreach (var provider in _providers) {
				if (provider.CanRefactor(contractParameter)) {
						context.RegisterRefactoring(new ContractRequiresRefactoring(method, contractParameter, provider));
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
