using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Extensions.Syntax {
	public static partial class SyntaxFactoryExtensions {
		public static SyntaxToken ToIdentifier(this string name) {
			return SyntaxFactory.Identifier(name);
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
	}
}
