using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Extensions.Syntax {
	public static partial class SyntaxFactoryExtensions {
		public static TypeParameterConstraintClauseSyntax TypeParameterConstraint(string name, TypeSyntax baseType) {
			return TypeParameterConstraintClause(
				name.ToIdentifierName(),
				TypeConstraint(baseType).ItemToSeparatedList<TypeParameterConstraintSyntax>())
				.Nicefy();
        }
	}
}
