using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace SomeExtensions.Extensions.Syntax {
	public static partial class SyntaxFactoryExtensions {
		private static readonly Dictionary<SyntaxKind, SyntaxKind> _opposites = new Dictionary<SyntaxKind, SyntaxKind>() {
			{ EqualsExpression, NotEqualsExpression },
			{ LessThanExpression, GreaterThanOrEqualExpression },
			{ LessThanOrEqualExpression, GreaterThanExpression },


			{ NotEqualsExpression ,EqualsExpression },
			{ GreaterThanOrEqualExpression , LessThanExpression},
			{ GreaterThanExpression , LessThanOrEqualExpression}
		};

		public static ExpressionSyntax ToLogicalNot(this ExpressionSyntax expression, bool simplify = false) {
			if (simplify) {
				var newExpression = RemoveLogicalNot(expression);
				if (newExpression != null) return newExpression;
			}

			return PrefixUnaryExpression(LogicalNotExpression, expression);
		}

		private static ExpressionSyntax RemoveLogicalNot(ExpressionSyntax expression) {
			if (expression is ParenthesizedExpressionSyntax) {
				return RemoveLogicalNot(expression.As<ParenthesizedExpressionSyntax>()?.Expression)?.ToParenthesized();
			}

			var unaryExpression = expression as PrefixUnaryExpressionSyntax;
			if (unaryExpression?.IsKind(LogicalNotExpression) ?? false)
				return unaryExpression.Operand;

			var oppositeBinaryExpression = ToOpposite(expression as BinaryExpressionSyntax);
			if (oppositeBinaryExpression != null)
				return oppositeBinaryExpression;

			return null;
		}

		public static BinaryExpressionSyntax ToOpposite(this BinaryExpressionSyntax expression) {
			if (expression == null) return null;

			var kind = expression.CSharpKind();
			if (_opposites.ContainsKey(kind)) {
				return BinaryExpression(_opposites[kind], expression.Left, expression.Right);
			}

			return null;
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
