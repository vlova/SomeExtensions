using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Extensions.Syntax {
	public static partial class SyntaxFactoryExtensions {
		public static SyntaxToken ToToken(this SyntaxKind kind) {
			return Token(kind);
		}

		public static SyntaxToken ToToken(this SyntaxKind kind, string value) {
			return Token(default(SyntaxTriviaList), kind, value, value, default(SyntaxTriviaList));
		}
	}
}
