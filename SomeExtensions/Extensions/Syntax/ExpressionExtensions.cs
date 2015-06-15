using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Extensions.Syntax {
	public static class ExpressionExtensions {
		public static bool IsEquivalentToNull(this ExpressionSyntax expr) {
			var @null = LiteralExpression(NullLiteralExpression);
			return expr?.IsEquivalentTo(@null, true) ?? false;
		}

		public static long? ParseLong(this ExpressionSyntax expr) {
			var sign = +1;

			var unaryExpr = expr as PrefixUnaryExpressionSyntax;
			if (unaryExpr != null) {
				if (unaryExpr.IsKind(UnaryMinusExpression)) sign = -1;
				else if (unaryExpr.IsKind(UnaryPlusExpression)) sign = +1;
				else return null;
				expr = unaryExpr.Operand;
			}

			var literalExpr = expr as LiteralExpressionSyntax;
			if (literalExpr?.IsKind(NumericLiteralExpression) ?? false) {
				var number = literalExpr.Token.Text?.ParseLong();
				return number * sign;
			}

			return null;
		}
	}
}
