using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Extensions.Syntax {
	public static partial class SyntaxFactoryExtensions {

		public static UsingDirectiveSyntax ToUsingDirective(this NameSyntax name) {
			return UsingDirective(name);
		}

		public static InvocationExpressionSyntax ToInvocation(this ExpressionSyntax expression, params ExpressionSyntax[] arguments) {
			return InvocationExpression(expression, arguments.ToArgumentList());
		}

		public static InvocationExpressionSyntax ToInvocation(this string expression, params ExpressionSyntax[] arguments) {
			return InvocationExpression(expression.ToMemberAccess(), arguments.ToArgumentList());
		}

		public static ExpressionStatementSyntax AssignWith(this ExpressionSyntax syntax, ExpressionSyntax what) {
			return AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, syntax, what)
				.ToStatement();
		}

		public static TypeSyntax ToTypeSyntax(this ITypeSymbol type) {
			return ParseTypeName(type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
		}

		public static LiteralExpressionSyntax ToLiteral(this int number) {
			return LiteralExpression(
				SyntaxKind.NumericLiteralExpression,
				Literal(number));
		}
	}
}
