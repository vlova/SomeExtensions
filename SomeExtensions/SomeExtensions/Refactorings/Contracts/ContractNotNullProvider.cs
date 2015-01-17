using System.Composition;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;

namespace SomeExtensions.Refactorings.Contracts {
	[ExportCodeRefactoringProvider(RefactoringId, LanguageNames.CSharp), Shared]
    public class ContractNotNullProvider : BaseRefactoringProvider {
        public const string RefactoringId = "ContractNotNull";

        protected override async Task ComputeRefactoringsAsync(CodeRefactoringContext context, SyntaxNode root, SyntaxNode node) {
			var parameter = node.FindUp<ParameterSyntax>();
            var method = parameter?.FindUp<BaseMethodDeclarationSyntax>();

            if (method?.Body == null)
                return;

            if (method.ContainsRequiresNotNull(parameter.Identifier.Text)) {
                return;
            }

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);

            var typeInfo = semanticModel.GetSpeculativeTypeSymbol(parameter.Type);

			if (typeInfo?.IsValueType ?? false) {
                return;
            }

            context.RegisterRefactoring(new ContractNotNullRefactoring(parameter, method));
        }
    }
}
