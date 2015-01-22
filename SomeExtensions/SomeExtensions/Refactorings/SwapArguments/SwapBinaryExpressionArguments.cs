using System.Composition;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions.Syntax;
using SomeExtensions.Extensions.Roslyn;

namespace SomeExtensions.Refactorings.SwapArguments {
	[ExportCodeRefactoringProvider(RefactoringId, LanguageNames.CSharp), Shared]
	public class SwapBinaryExpressionArgumentsProvider : BaseRefactoringProvider {
		public const string RefactoringId = nameof(SwapBinaryExpressionArgumentsProvider);

		protected override void ComputeRefactorings(CodeRefactoringContext context, SyntaxNode root, SyntaxNode node) {
			var expression = node.FindUp<BinaryExpressionSyntax>();
			if (expression == null) {
				return;
			}

			context.RegisterRefactoring(root, new SwapBinaryExpressionArgumentsRefactoring(expression));
		}
	}

	public class SwapBinaryExpressionArgumentsRefactoring : IRefactoring {
		private readonly BinaryExpressionSyntax _expression;

		public SwapBinaryExpressionArgumentsRefactoring(BinaryExpressionSyntax expression) {
			_expression = expression;
		}

		public string Description { get; } = "Swap arguments";

		public SyntaxNode ComputeRoot(SyntaxNode root, CancellationToken token) {
			var newExpression = _expression
				.WithLeft(_expression.Right)
				.WithRight(_expression.Left)
				.Nicefy();

            return root.ReplaceNode(_expression, newExpression);
		}
	}
}
