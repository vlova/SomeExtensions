using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Extensions {
	public static class ParseSyntaxExtensions {
		public static TypeSyntax ParseTypeName(this string text) {
			return SyntaxFactory.ParseTypeName(text);
		}

		public static ExpressionSyntax ParseExpression(this string text) {
			return SyntaxFactory.ParseExpression(text);
		}
	}
}
