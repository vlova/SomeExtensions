using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using static Microsoft.CodeAnalysis.SymbolDisplayFormat;

namespace SomeExtensions.Extensions.Syntax {
	public static partial class SyntaxFactoryExtensions {
		public class ArgumentWrapper {
			public ArgumentSyntax Argument { get; }

			public ArgumentWrapper(ArgumentSyntax argument) {
				Argument = argument;
			}

			public ArgumentWrapper(ExpressionSyntax expression) {
				Argument = Argument(expression);
			}

			public static implicit operator ArgumentWrapper(ArgumentSyntax argument) {
				return new ArgumentWrapper(argument);
			}

			public static implicit operator ArgumentWrapper(ExpressionSyntax expression) {
				return new ArgumentWrapper(expression);
			}

			public static implicit operator ArgumentSyntax(ArgumentWrapper wrapper) {
				return wrapper.Argument;
			}
		}

        public static UsingDirectiveSyntax ToUsingDirective(this NameSyntax name, bool @static = false) {
			var directive = UsingDirective(name);

			return @static
				? directive.WithStaticKeyword(StaticKeyword.ToToken())
				: directive;
		}

		public static InvocationExpressionSyntax ToInvocation(this ExpressionSyntax expression, params ArgumentWrapper[] arguments) {
			return InvocationExpression(expression, arguments.Select(r => r.Argument).ToArgumentList());
		}

		public static InvocationExpressionSyntax ToInvocation(this string expression, params ArgumentWrapper[] arguments) {
			return InvocationExpression(expression.ToMemberAccess(), arguments.Select(r => r.Argument).ToArgumentList());
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
