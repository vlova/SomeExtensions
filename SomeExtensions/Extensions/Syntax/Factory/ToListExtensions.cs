using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace SomeExtensions.Extensions.Syntax {
	public static partial class SyntaxFactoryExtensions {
		public static ImplicitArrayCreationExpressionSyntax ToArrayCreation(this IEnumerable<ExpressionSyntax> paramsArguments) {
			return ImplicitArrayCreationExpression(InitializerExpression(ArrayInitializerExpression, paramsArguments.ToSeparatedList()));
		}

		public static SyntaxList<T> ItemToSyntaxList<T>(this T item) where T : SyntaxNode {
			return List(new[] { item });
		}

		public static SeparatedSyntaxList<T> ItemToSeparatedList<T>(this T item) where T : SyntaxNode {
			return SeparatedList(new[] { item });
		}

		public static SyntaxList<T> ToSyntaxList<T>(this IEnumerable<T> collection) where T : SyntaxNode {
			return List(collection);
		}

		public static SeparatedSyntaxList<T> ToSeparatedList<T>(this IEnumerable<T> collection) where T : SyntaxNode {
			return SeparatedList(collection);
		}

		public static SyntaxTokenList ToTokenList(this IEnumerable<SyntaxKind> collection) {
			return TokenList(collection.Select(Token));
		}

		public static ArgumentListSyntax ToArgumentList(this IEnumerable<ArgumentSyntax> collection) {
			return ArgumentList(collection.ToSeparatedList());
		}

		public static ArgumentListSyntax ToArgumentList(this IEnumerable<ExpressionSyntax> collection) {
			return collection.Select(Argument).ToArgumentList();
		}

		public static ParameterListSyntax ToParameterList(this IEnumerable<ParameterSyntax> collection) {
			return ParameterList(collection.ToSeparatedList());
		}
	}
}
