using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace SomeExtensions.Extensions.Syntax {
	static class ArgumentExtensions {
		public static ArgumentSyntax WithNameColon(this ArgumentSyntax argument, string nameColon) {
			return argument.WithNameColon(NameColon(nameColon));
		}

		public static ArgumentSyntax WithOutKeyword(this ArgumentSyntax argument) {
			return argument.WithRefOrOutKeyword(OutKeyword.ToToken());
		}
	}
}
