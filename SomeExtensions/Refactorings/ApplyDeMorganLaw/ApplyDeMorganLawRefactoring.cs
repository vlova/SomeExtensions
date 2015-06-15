using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;
using static System.Diagnostics.Contracts.Contract;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Refactorings.ApplyDeMorganLaw {
	internal class ApplyDeMorganLawRefactoring : IRefactoring {
		private readonly BinaryExpressionSyntax _operation;

		public ApplyDeMorganLawRefactoring(BinaryExpressionSyntax operation) {
			Requires(operation != null);
			_operation = operation;
		}

		public string Description => "Apply De Morgan's law";

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root) {
			var newBinaryOperation = BinaryExpression(
					kind: Helpers.Convert(_operation.Kind()),
					left: _operation.Left.ToLogicalNot(simplify: true),
					right: _operation.Right.ToLogicalNot(simplify: true))
				.Nicefy();

			// just prevent cases like !(!(a || b))
			var operationToReplace = _operation.FindUpLogicalNot() ?? (ExpressionSyntax)_operation;
			ExpressionSyntax newOperation;
			if (operationToReplace == _operation) {
				newOperation = newBinaryOperation.ToParenthesized().ToLogicalNot().Nicefy();
			}
			else {
				// do not add parenthesizes unless necessary
				newOperation = (operationToReplace.Parent is ExpressionSyntax)
					? (ExpressionSyntax)newBinaryOperation.ToParenthesized()
					: newBinaryOperation;
			}

			return root.ReplaceNode(operationToReplace, newOperation);
		}
	}
}