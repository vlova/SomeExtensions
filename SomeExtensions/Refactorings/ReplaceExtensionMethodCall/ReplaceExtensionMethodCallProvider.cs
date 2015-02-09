using System.Composition;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions.Syntax;

using static Microsoft.CodeAnalysis.LanguageNames;
using static Microsoft.CodeAnalysis.SymbolKind;

namespace SomeExtensions.Refactorings.ReplaceExtensionMethodCall {
	[ExportCodeRefactoringProvider(nameof(ReplaceExtensionMethodCallProvider), CSharp), Shared]
	internal class ReplaceExtensionMethodCallProvider : BaseRefactoringProvider<InvocationExpressionSyntax> {
		protected override int? FindUpLimit => 2;

		protected async override Task ComputeRefactoringsAsync(RefactoringContext context,  InvocationExpressionSyntax invocation) {
			var memberAccess = invocation.GetMemberAccessExpression();
			if (memberAccess == null) return;

			var model = await context.SemanticModelAsync;
			var symbol = model.GetSymbolInfo(memberAccess).Symbol as IMethodSymbol;
			if (symbol == null) return;
			if (symbol.Kind != Method) return;
			if (!symbol.IsExtensionMethod) return;

			context.Register(new ReplaceExtensionMethodCallRefactoring(model, invocation, symbol));
		}
	}
}
