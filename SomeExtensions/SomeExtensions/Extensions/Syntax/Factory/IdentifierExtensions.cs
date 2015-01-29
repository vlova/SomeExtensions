using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Extensions.Syntax {
	public static partial class SyntaxFactoryExtensions {
		public static SyntaxToken ToIdentifier(this string name) {
			return Identifier(name);
		}

		public static IdentifierNameSyntax ToIdentifierName(this SyntaxToken name) {
			return IdentifierName(name);
		}

		public static IdentifierNameSyntax ToIdentifierName(this string name) {
			return IdentifierName(name);
		}

		public static ExpressionSyntax ToIdentifierName(this string name, bool qualifyWithThis) {
			return qualifyWithThis
				? name.ToIdentifierName().OfThis()
				: (ExpressionSyntax)name.ToIdentifierName();
		}
	}
}
