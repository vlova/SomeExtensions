using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions.Roslyn;

using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.SwapArguments {
	[ExportCodeRefactoringProvider(nameof(SwapBinaryExpressionArgumentsProvider), CSharp), Shared]
	internal class SwapBinaryExpressionArgumentsProvider : BaseRefactoringProvider<BinaryExpressionSyntax> {
		protected override int? FindUpLimit => 2;

		protected override void ComputeRefactorings(CodeRefactoringContext context, SyntaxNode root, BinaryExpressionSyntax expression) {
			context.RegisterRefactoring(root, new SwapBinaryExpressionArgumentsRefactoring(expression));
		}
	}
}
