using System.Composition;

using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.SwapArguments {
	[ExportCodeRefactoringProvider(nameof(SwapBinaryExpressionArgumentsProvider), CSharp), Shared]
	internal class SwapBinaryExpressionArgumentsProvider : BaseRefactoringProvider<BinaryExpressionSyntax> {
		protected override int? FindUpLimit => 2;

		protected override void ComputeRefactorings(RefactoringContext context, BinaryExpressionSyntax expression) {
			context.Register(new SwapBinaryExpressionArgumentsRefactoring(expression));
		}
	}
}
