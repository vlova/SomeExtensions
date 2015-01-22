using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Extensions.Syntax {
	public static partial class SyntaxFactoryExtensions {
		public static PrefixUnaryExpressionSyntax ToLogicalNot(this ExpressionSyntax expression) {
			return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, expression);
		}

		public static BinaryExpressionSyntax ToNotNull(this ExpressionSyntax left) {
			return left.ToNotEquals(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression));
		}

		public static BinaryExpressionSyntax ToNotEquals(this ExpressionSyntax left, ExpressionSyntax right) {
			return SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression, left, right);
		}

		public static BinaryExpressionSyntax ToAnd(this ExpressionSyntax left, ExpressionSyntax right) {
			return SyntaxFactory.BinaryExpression(SyntaxKind.LogicalAndExpression, left, right);
		}

		public static BinaryExpressionSyntax ToGreaterThan(this ExpressionSyntax left, ExpressionSyntax right) {
			return SyntaxFactory.BinaryExpression(SyntaxKind.GreaterThanExpression, left, right);
		}
	}
}
