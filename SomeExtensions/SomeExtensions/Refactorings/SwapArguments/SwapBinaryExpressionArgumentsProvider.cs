using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions.Syntax;
using SomeExtensions.Extensions.Roslyn;

using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.SwapArguments {
	[ExportCodeRefactoringProvider(nameof(SwapBinaryExpressionArgumentsProvider), CSharp), Shared]
	internal class SwapBinaryExpressionArgumentsProvider : BaseRefactoringProvider {
		protected override void ComputeRefactorings(CodeRefactoringContext context, SyntaxNode root, SyntaxNode node) {
			var expression = node.FindUp<BinaryExpressionSyntax>();
			if (expression == null) {
				return;
			}

			context.RegisterRefactoring(root, new SwapBinaryExpressionArgumentsRefactoring(expression));
		}
	}
}
