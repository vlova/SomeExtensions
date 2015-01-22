using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Extensions.Syntax {
	public static class ExpressionExtensions {
		public static bool IsEquivalentToNull(this ExpressionSyntax expr) {
			var @null = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
            return expr?.IsEquivalentTo(@null, true) ?? false;
        }
	}
}
