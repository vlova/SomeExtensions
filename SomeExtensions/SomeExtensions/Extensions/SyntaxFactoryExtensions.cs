using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Extensions {
    public static class SyntaxFactoryExtensions {
        public static MemberAccessExpressionSyntax OfThis(this IdentifierNameSyntax identifier) {
            return SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression(), identifier);
        }

        public static IdentifierNameSyntax ToIdentifier(this string name) {
            return SyntaxFactory.IdentifierName(name);
        }

        public static ExpressionSyntax ToIdentifier(this string name, bool qualifyWithThis) {
            return qualifyWithThis
                ? name.ToIdentifier().OfThis()
                : (ExpressionSyntax)name.ToIdentifier();
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

		public static UsingDirectiveSyntax ToUsing(this NameSyntax name) {
			return SyntaxFactory.UsingDirective(name);
		}

		public static SyntaxList<T> ToSyntaxList<T>(this IEnumerable<T> collection) where T : SyntaxNode {
			return SyntaxFactory.List<T>(collection);
		}

		public static AssignmentExpressionSyntax AssignWith(this ExpressionSyntax syntax, ExpressionSyntax what) {
            return SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, syntax, what);
        }
    }
}
