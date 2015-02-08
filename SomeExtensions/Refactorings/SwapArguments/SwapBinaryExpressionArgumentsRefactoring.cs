using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions.Syntax;
using System.Diagnostics.Contracts;

namespace SomeExtensions.Refactorings.SwapArguments {
	internal class SwapBinaryExpressionArgumentsRefactoring : IRefactoring {
		private readonly BinaryExpressionSyntax _expression;

		public SwapBinaryExpressionArgumentsRefactoring(BinaryExpressionSyntax expression) {
			Contract.Requires(expression != null);
			_expression = expression;
		}

		public string Description => "Swap arguments";

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token) {
			var newExpression = _expression
				.WithLeft(_expression.Right)
				.WithRight(_expression.Left)
				.Nicefy();

			return root.ReplaceNode(_expression, newExpression);
		}
	}
}
