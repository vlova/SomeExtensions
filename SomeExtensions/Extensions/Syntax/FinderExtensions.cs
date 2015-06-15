using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace SomeExtensions.Extensions.Syntax {
	public static class FinderExtensions {
		public struct Finder {
			private readonly SyntaxNode _node;

			public Finder(SyntaxNode node) {
				_node = node;
			}

			public SyntaxNode Node {
				get {
					return _node;
				}
			}
		}

		public static Finder Find(this SyntaxNode node) {
			return new Finder(node);
		}

		public static IEnumerable<ConstructorDeclarationSyntax> FindConstructors(this TypeDeclarationSyntax type) {
			return type.ChildNodes().OfType<ConstructorDeclarationSyntax>();
		}

		// TODO: optimize finding by skiping obviously bad descedants
		public static FieldDeclarationSyntax Field(this Finder finder, string identifier) {
			return finder.Node.DescendantNodes<FieldDeclarationSyntax>()
				.FirstOrDefault(p => p.Declaration.Variables.Any(v => v.Identifier.Text == identifier));
		}

		internal static T DescedantWithName<T>(this Finder finder, string name) where T : SyntaxNode {
			return finder.Node
				.DescendantNodes<T>()
				.FirstOrDefault(p => p.As<dynamic>().Identifier.Text == name);
		}

		public static TypeDeclarationSyntax Type(this Finder finder, string identifier) {
			return finder.DescedantWithName<TypeDeclarationSyntax>(identifier);
		}

		public static TypeDeclarationSyntax Type(this Finder finder, TypeDeclarationSyntax type) {
			return finder.DescedantWithName<TypeDeclarationSyntax>(type.Identifier.Text);
		}

		public static PropertyDeclarationSyntax Property(this Finder finder, string identifier) {
			return finder.DescedantWithName<PropertyDeclarationSyntax>(identifier);
		}

		public static PrefixUnaryExpressionSyntax FindUpLogicalNot(this ExpressionSyntax expression) {
			expression = expression?.Parent.As<ParenthesizedExpressionSyntax>() ?? expression;
			var unaryExpression = expression?.Parent.As<PrefixUnaryExpressionSyntax>();
			if (unaryExpression?.IsKind(LogicalNotExpression) ?? false) {
				return unaryExpression;
			}
			else {
				return null;
			}
		}
	}
}
