using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace SomeExtensions.Extensions.Syntax {
	public static partial class SyntaxFactoryExtensions {
		public static PrefixUnaryExpressionSyntax ToLogicalNot(this ExpressionSyntax expression) {
			return PrefixUnaryExpression(LogicalNotExpression, expression);
		}

		public static BinaryExpressionSyntax ToNotNull(this ExpressionSyntax left) {
			return left.ToNotEquals(LiteralExpression(NullLiteralExpression));
		}

		public static BinaryExpressionSyntax ToNotEquals(this ExpressionSyntax left, ExpressionSyntax right) {
			return BinaryExpression(NotEqualsExpression, left, right);
		}

		public static BinaryExpressionSyntax ToAnd(this ExpressionSyntax left, ExpressionSyntax right) {
			return BinaryExpression(LogicalAndExpression, left, right);
		}

		public static BinaryExpressionSyntax ToGreaterThan(this ExpressionSyntax left, ExpressionSyntax right) {
			return BinaryExpression(GreaterThanExpression, left, right);
		}
	}
}
