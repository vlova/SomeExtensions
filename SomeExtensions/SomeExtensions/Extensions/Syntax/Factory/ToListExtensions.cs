using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Extensions.Syntax {
	public static partial class SyntaxFactoryExtensions {
		public static SyntaxList<T> ItemToSyntaxList<T>(this T item) where T : SyntaxNode {
			return SyntaxFactory.List(new[] { item });
		}

		public static SeparatedSyntaxList<T> ItemToSeparatedList<T>(this T item) where T : SyntaxNode {
			return SyntaxFactory.SeparatedList(new[] { item });
		}

		public static SyntaxList<T> ToSyntaxList<T>(this IEnumerable<T> collection) where T : SyntaxNode {
			return SyntaxFactory.List(collection);
		}

		public static SeparatedSyntaxList<T> ToSeparatedList<T>(this IEnumerable<T> collection) where T : SyntaxNode {
			return SyntaxFactory.SeparatedList(collection);
		}

		public static SyntaxTokenList ToTokenList(this IEnumerable<SyntaxKind> collection) {
			return SyntaxFactory.TokenList(collection.Select(SyntaxFactory.Token));
		}

		public static ArgumentListSyntax ToArgumentList(this IEnumerable<ArgumentSyntax> collection) {
			return SyntaxFactory.ArgumentList(collection.ToSeparatedList());
		}

		public static ArgumentListSyntax ToArgumentList(this IEnumerable<ExpressionSyntax> collection) {
			return collection.Select(SyntaxFactory.Argument).ToArgumentList();
		}

		public static ParameterListSyntax ToParameterList(this IEnumerable<ParameterSyntax> collection) {
			return SyntaxFactory.ParameterList(collection.ToSeparatedList());
		}
	}
}
