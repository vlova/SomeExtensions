using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions {
	internal static class PropertyExtensions {
		public static AccessorDeclarationSyntax GetAccessor(this AccessorListSyntax accessorList) {
			return accessorList.DescendantNodes().OfType<AccessorDeclarationSyntax>().FirstOrDefault(a => a.CSharpKind() == SyntaxKind.GetAccessorDeclaration);
		}

		public static AccessorDeclarationSyntax SetAccessor(this AccessorListSyntax accessorList) {
			return accessorList.DescendantNodes().OfType<AccessorDeclarationSyntax>().FirstOrDefault(a => a.CSharpKind() == SyntaxKind.SetAccessorDeclaration);
		}
	}

	internal static class LanguageExtensions {
		public static T Fluent<T>(this T obj, Func<T, T> rewriter) {
			return rewriter(obj);
		}

		public static T If<T>(this T obj, Predicate<T> condition, Func<T, T> rewriter) {
			return condition(obj) ? rewriter(obj) : obj;
		}

		public static T As<T>(this object obj) where T : class {
			return obj as T;
		}
	}

	internal static class WhitespacesExtensions {
		public static SyntaxToken WithTrailingWhitespace(this SyntaxToken node) {
			return node.WithTrailingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, " ")));
		}

		public static T WithLeadingWhitespace<T>(this T node) where T : SyntaxNode {
			return node.WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, " ")));
		}

		public static T WithTrailingWhitespace<T>(this T node) where T : SyntaxNode {
			return node.WithTrailingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, " ")));
		}

		public static T WithWhitespacesAround<T>(this T node) where T : SyntaxNode {
			return node.WithLeadingWhitespace().WithTrailingWhitespace();
		}

		public static T WithTrailingEndline<T>(this T node) where T : SyntaxNode {
			return node.WithTrailingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, "\n")));
		}
	}

	internal static class Extensions {
		public static IEnumerable<ConstructorDeclarationSyntax> GetConstructors(this TypeDeclarationSyntax typeDecl) {
			return typeDecl.ChildNodes().OfType<ConstructorDeclarationSyntax>();
		}

		public static MemberAccessExpressionSyntax OfThis(this IdentifierNameSyntax identifier) {
			return SyntaxFactory.MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				SyntaxFactory.ThisExpression(),
				identifier);
		}

		public static FieldDeclarationSyntax FieldWithName(this SyntaxNode node, string identifier) {
			return node.DescendantNodes().OfType<FieldDeclarationSyntax>().FirstOrDefault(p => p.Declaration.Variables.Any(v => v.Identifier.Text == identifier));
		}

		public static TypeDeclarationSyntax TypeWithName(this SyntaxNode node, string identifier) {
			return node.DescendantNodes().OfType<TypeDeclarationSyntax>().FirstOrDefault(p => p.Identifier.Text == identifier);
		}

		public static PropertyDeclarationSyntax PropertyWithName(this SyntaxNode node, string identifier) {
			return node.DescendantNodes().OfType<PropertyDeclarationSyntax>().FirstOrDefault(p => p.Identifier.Text == identifier);
		}


		public static async Task<Document> WithReplacedNode(this Document document, SyntaxNode oldNode, SyntaxNode newNode, CancellationToken c) {
			var root = await document.GetSyntaxRootAsync(c);

			return document.WithSyntaxRoot(root.ReplaceNode(oldNode, newNode));
		}

		public static FieldDeclarationSyntax FindFieldDeclaration(this SyntaxNode node) {
			return (node as FieldDeclarationSyntax)
				?? ((node as VariableDeclarationSyntax)?.Parent as FieldDeclarationSyntax)
				?? ((node as VariableDeclaratorSyntax)?.Parent?.Parent as FieldDeclarationSyntax);
		}

		public static PropertyDeclarationSyntax FindPropertyDeclaration(this SyntaxNode node) {
			return (node as PropertyDeclarationSyntax)
				?? ((node as TypeSyntax)?.Parent as PropertyDeclarationSyntax)
				?? ((node as AccessorListSyntax)?.Parent as PropertyDeclarationSyntax)
				?? ((node as AccessorDeclarationSyntax)?.Parent?.Parent as PropertyDeclarationSyntax);
		}

		public static BaseMethodDeclarationSyntax FindMethodDeclaration(this SyntaxNode node) {
			return (node as BaseMethodDeclarationSyntax)
				?? ((node as TypeSyntax)?.Parent as BaseMethodDeclarationSyntax)
				?? ((node as ParameterListSyntax)?.Parent as BaseMethodDeclarationSyntax)
				?? (node.FindParameterSyntax()?.Parent as BaseMethodDeclarationSyntax);
		}

		public static ParameterSyntax FindParameterSyntax(this SyntaxNode node) {
			return (node as ParameterSyntax)
				?? (node.As<TypeSyntax>()?.Parent.As<ParameterSyntax>())
				?? (node.As<TypeSyntax>()?.As<TypeSyntax>()?.Parent.As<ParameterSyntax>())
				?? (node.As<IdentifierNameSyntax>()?.Parent.As<ParameterSyntax>())
				?? (node.As<SimpleNameSyntax>()?.Parent.As<ParameterSyntax>());
		}

		public static string ToFieldName(this string propertyName) {
			return "_" + (propertyName[0].ToString().ToLower()) + propertyName.Substring(1);
		}
	}
}