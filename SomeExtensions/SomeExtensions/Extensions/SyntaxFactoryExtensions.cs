using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Extensions {
	public static class SyntaxFactoryExtensions {
		public static MemberAccessExpressionSyntax OfThis(this IdentifierNameSyntax identifier) {
			return SyntaxFactory.MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression(), identifier);
		}

		public static SyntaxToken ToToken(this SyntaxKind kind) {
			return SyntaxFactory.Token(kind);
		}

		public static IdentifierNameSyntax ToIdentifierName(this SyntaxToken name) {
			return SyntaxFactory.IdentifierName(name);
		}

		public static IdentifierNameSyntax ToIdentifierName(this string name) {
			return SyntaxFactory.IdentifierName(name);
		}

		public static ExpressionSyntax ToIdentifierName(this string name, bool qualifyWithThis) {
			return qualifyWithThis
				? name.ToIdentifierName().OfThis()
				: (ExpressionSyntax)name.ToIdentifierName();
		}

		public static ExpressionSyntax AccessTo(this ExpressionSyntax to, string what) {
			if (to == null) {
				return what.ToIdentifierName();
			}

			return SyntaxFactory.MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				to,
				what.ToIdentifierName());
		}

		public static MemberAccessExpressionSyntax AccessTo(this string name, string what) {
			return SyntaxFactory.MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				name.ToIdentifierName(),
				what.ToIdentifierName());
		}

		public static ExpressionSyntax ToMemberAccess(this string names) {
			var nameArray = names.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
			return nameArray.Aggregate((ExpressionSyntax)null, (to, what) => to.AccessTo(what));
		}

		public static VariableDeclaratorSyntax ToVariableDeclarator(this string name) {
			return SyntaxFactory.VariableDeclarator(name);
		}

		public static VariableDeclarationSyntax ToVariableDeclaration(this string name, TypeSyntax type) {
			return SyntaxFactory.VariableDeclaration(type,
				SyntaxFactory.SeparatedList(new[] {
					name.ToVariableDeclarator()
			}));
		}

		public static FieldDeclarationSyntax ToFieldDeclaration(this VariableDeclarationSyntax variable) {
			return SyntaxFactory.FieldDeclaration(variable);
		}

		public static FieldDeclarationSyntax ToFieldDeclaration(this string name, TypeSyntax type) {
			return name
				.ToVariableDeclaration(type)
				.ToFieldDeclaration();
		}

		public static ParameterSyntax ToParameter(this string name, TypeSyntax type) {
			return SyntaxFactory.Parameter(
						SyntaxFactory.List<AttributeListSyntax>(),
						SyntaxFactory.TokenList(),
						type,
						SyntaxFactory.Identifier(name),
						null);
		}

		public static ReturnStatementSyntax ToReturnStatement(this ExpressionSyntax expression) {
			return SyntaxFactory.ReturnStatement(expression);
		}

		public static BlockSyntax ToBlock(this StatementSyntax statement) {
			return SyntaxFactory.Block(statement);
		}

		public static ExpressionStatementSyntax ToStatement(this ExpressionSyntax expr) {
			return SyntaxFactory.ExpressionStatement(expr);
		}

		public static LocalDeclarationStatementSyntax ToLocalDeclaration(this VariableDeclarationSyntax expr) {
			return SyntaxFactory.LocalDeclarationStatement(expr);
		}

		public static UsingDirectiveSyntax ToUsingDirective(this NameSyntax name) {
			return SyntaxFactory.UsingDirective(name);
		}

		public static SyntaxList<T> ToSyntaxList<T>(this IEnumerable<T> collection) where T : SyntaxNode {
			return SyntaxFactory.List<T>(collection);
		}

		public static SyntaxList<T> ItemToSyntaxList<T>(this T item) where T : SyntaxNode {
			return SyntaxFactory.List<T>(new[] { item });
		}

		public static SeparatedSyntaxList<T> ToSeparatedList<T>(this IEnumerable<T> collection) where T : SyntaxNode {
			return SyntaxFactory.SeparatedList<T>(collection);
		}

		public static SeparatedSyntaxList<T> ItemToSeparatedList<T>(this T item) where T : SyntaxNode {
			return SyntaxFactory.SeparatedList<T>(new[] { item });
		}

		public static ArgumentListSyntax ToArgumentList(this SeparatedSyntaxList<ArgumentSyntax> collection) {
			return SyntaxFactory.ArgumentList(collection);
		}

		public static ArgumentListSyntax ToArgumentList(this IEnumerable<ExpressionSyntax> collection) {
			return collection
				.Select(r => SyntaxFactory.Argument(r))
				.ToSeparatedList()
				.ToArgumentList();
		}

		public static InvocationExpressionSyntax ToInvocation(this ExpressionSyntax expression, params ExpressionSyntax[] arguments) {
			return SyntaxFactory.InvocationExpression(expression, arguments.ToArgumentList());
		}

		public static InvocationExpressionSyntax ToInvocation(this string expression, params ExpressionSyntax[] arguments) {
			return SyntaxFactory.InvocationExpression(expression.ToMemberAccess(), arguments.ToArgumentList());
		}

		public static PrefixUnaryExpressionSyntax ToLogicalNot(this ExpressionSyntax expression) {
			return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, expression);
		}

		public static BinaryExpressionSyntax And(this ExpressionSyntax left, ExpressionSyntax right) {
			return SyntaxFactory.BinaryExpression(SyntaxKind.LogicalAndExpression, left, right);
		}

		public static BinaryExpressionSyntax NotEquals(this ExpressionSyntax left, ExpressionSyntax right) {
			return SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression, left, right);
		}

		public static BinaryExpressionSyntax GreaterThan(this ExpressionSyntax left, ExpressionSyntax right) {
			return SyntaxFactory.BinaryExpression(SyntaxKind.GreaterThanExpression, left, right);
		}

		public static BinaryExpressionSyntax NotNull(this ExpressionSyntax left) {
			return left.NotEquals(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression));
        }

		public static AssignmentExpressionSyntax AssignWith(this ExpressionSyntax syntax, ExpressionSyntax what) {
			return SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, syntax, what);
		}

		public static TypeSyntax ToTypeSyntax(this ITypeSymbol type) {
			return SyntaxFactory.ParseTypeName(type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
		}

		public static LiteralExpressionSyntax ToLiteral(this int number) {
			return SyntaxFactory.LiteralExpression(
				SyntaxKind.NumericLiteralExpression,
				SyntaxFactory.Literal(number));
        }
	}
}
