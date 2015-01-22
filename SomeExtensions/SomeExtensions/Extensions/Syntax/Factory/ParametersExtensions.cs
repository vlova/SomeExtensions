using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Extensions.Syntax {
	public static partial class SyntaxFactoryExtensions {
		public static EqualsValueClauseSyntax ToInitializer(this ExpressionSyntax value) {
			if (value != null) {
				return SyntaxFactory
					.EqualsValueClause(value)
					.WithEqualsToken(SyntaxFactory.ParseToken("="));
			}

			return null;
		}

		public static VariableDeclaratorSyntax ToVariableDeclarator(this string name, ExpressionSyntax value = null) {
			return SyntaxFactory.VariableDeclarator(name).WithInitializer(value.ToInitializer());
		}

		public static VariableDeclarationSyntax ToVariableDeclaration(this VariableDeclaratorSyntax name, TypeSyntax type) {
			return SyntaxFactory.VariableDeclaration(type, name.ItemToSeparatedList());
		}

		public static VariableDeclarationSyntax ToVariableDeclaration(this string name, TypeSyntax type, ExpressionSyntax value = null) {
			return SyntaxFactory.VariableDeclaration(type, name.ToVariableDeclarator(value).ItemToSeparatedList());
		}

		public static FieldDeclarationSyntax ToFieldDeclaration(this VariableDeclarationSyntax variable) {
			return SyntaxFactory.FieldDeclaration(variable);
		}

		public static FieldDeclarationSyntax ToFieldDeclaration(this string name, TypeSyntax type) {
			return name
				.ToVariableDeclaration(type)
				.ToFieldDeclaration();
		}

		public static ParameterSyntax ToParameter(this string name, TypeSyntax type, ExpressionSyntax defaultValue = null) {
			return SyntaxFactory.Parameter(
						SyntaxFactory.List<AttributeListSyntax>(),
						SyntaxFactory.TokenList(),
						type,
						SyntaxFactory.Identifier(name),
						defaultValue.ToInitializer());
		}

		public static LocalDeclarationStatementSyntax ToLocalDeclaration(this VariableDeclarationSyntax expr) {
			return SyntaxFactory.LocalDeclarationStatement(expr);
		}
	}
}
