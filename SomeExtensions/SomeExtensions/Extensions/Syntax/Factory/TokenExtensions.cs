using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SomeExtensions.Extensions.Syntax {
	public static partial class SyntaxFactoryExtensions {
		public static SyntaxToken ToToken(this SyntaxKind kind) {
			return SyntaxFactory.Token(kind);
		}

		public static SyntaxToken ToToken(this SyntaxKind kind, string value) {
			return SyntaxFactory.Token(default(SyntaxTriviaList), kind, value, value, default(SyntaxTriviaList));
		}
	}
}
