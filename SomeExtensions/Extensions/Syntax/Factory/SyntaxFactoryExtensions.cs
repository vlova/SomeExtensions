using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using static Microsoft.CodeAnalysis.SymbolDisplayFormat;

namespace SomeExtensions.Extensions.Syntax {
	public static partial class SyntaxFactoryExtensions {
		public static UsingDirectiveSyntax ToUsingDirective(this NameSyntax name, bool @static = false) {
			var directive = UsingDirective(name);

			return @static
				? directive.WithStaticKeyword(StaticKeyword.ToToken())
				: directive;
		}

		public static InvocationExpressionSyntax ToInvocation(this ExpressionSyntax expression, params ExpressionSyntax[] arguments) {
			return InvocationExpression(expression, arguments.ToArgumentList());
		}

		public static InvocationExpressionSyntax ToInvocation(this string expression, params ExpressionSyntax[] arguments) {
			return InvocationExpression(expression.ToMemberAccess(), arguments.ToArgumentList());
		}

		public static ExpressionStatementSyntax AssignWith(this ExpressionSyntax syntax, ExpressionSyntax what) {
			return AssignmentExpression(SimpleAssignmentExpression, syntax, what)
				.ToStatement();
		}

		public static TypeSyntax ToTypeSyntax(this ITypeSymbol type) {
			return ParseTypeName(type.ToDisplayString(MinimallyQualifiedFormat));
		}

		public static LiteralExpressionSyntax ToLiteral(this int number) {
			return LiteralExpression(
				NumericLiteralExpression,
				Literal(number));
		}

		public static ParenthesizedExpressionSyntax ToParenthesized(this ExpressionSyntax expression) {
			return ParenthesizedExpression(expression);
		}
	}
}
