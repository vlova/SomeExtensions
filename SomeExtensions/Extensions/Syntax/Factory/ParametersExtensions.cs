using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Extensions.Syntax {
	public static partial class SyntaxFactoryExtensions {
		public static EqualsValueClauseSyntax ToInitializer(this ExpressionSyntax value) {
			if (value != null) {
				return EqualsValueClause(value)
					.WithEqualsToken(ParseToken("="));
			}

			return null;
		}

		public static VariableDeclaratorSyntax ToVariableDeclarator(this string name, ExpressionSyntax value = null) {
			return VariableDeclarator(name).WithInitializer(value.ToInitializer());
		}

		public static VariableDeclarationSyntax ToVariableDeclaration(this VariableDeclaratorSyntax name, TypeSyntax type) {
			return VariableDeclaration(type, name.ItemToSeparatedList());
		}

		public static VariableDeclarationSyntax ToVariableDeclaration(this string name, TypeSyntax type, ExpressionSyntax value = null) {
			return VariableDeclaration(type, name.ToVariableDeclarator(value).ItemToSeparatedList());
		}

		public static FieldDeclarationSyntax ToFieldDeclaration(this VariableDeclarationSyntax variable) {
			return FieldDeclaration(variable);
		}

		public static FieldDeclarationSyntax ToFieldDeclaration(this string name, TypeSyntax type) {
			return name
				.ToVariableDeclaration(type)
				.ToFieldDeclaration();
		}

		public static ParameterSyntax ToParameter(this string name, TypeSyntax type, ExpressionSyntax defaultValue = null) {
			return Parameter(
						List<AttributeListSyntax>(),
						TokenList(),
						type,
						name.ToIdentifier(),
						defaultValue.ToInitializer());
		}

		public static LocalDeclarationStatementSyntax ToLocalDeclaration(this VariableDeclarationSyntax expr) {
			return LocalDeclarationStatement(expr);
		}
	}
}
