using System;
using System.Diagnostics.Contracts;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace SomeExtensions.Extensions.Syntax {
	public static class NodeExtensions {
        public static TRoot InsertAfter<TRoot>(this TRoot root, SyntaxNode node, SyntaxNode newNode) where TRoot : SyntaxNode {
            return root.InsertNodesAfter(node, new[] { newNode });
        }

        public static TRoot InsertBefore<TRoot>(this TRoot root, SyntaxNode node, SyntaxNode newNode) where TRoot : SyntaxNode {
            return root.InsertNodesBefore(node, new[] { newNode });
        }

		public static TNode RemoveNodeAt<TNode>(this TNode node, int position, SyntaxRemoveOptions options = SyntaxRemoveOptions.KeepNoTrivia)
			where TNode : SyntaxNode {
			return node.RemoveNode(node.ChildNodes().At(position), options);
		}

		public static int IndexOf(this SyntaxNode node, SyntaxNode ofNode) {
			return node.ChildNodes().IndexOf(ofNode);
		}

        public static IEnumerable<SyntaxNode> GetThisAndParents(this SyntaxNode node, int? limit = null) {
            while (node != null) {
				CancellationTokenExtensions.ThrowOnCancellation();

				if (limit < 0) {
					break;
				}

                yield return node;
                node = node.Parent;
				limit = limit - 1;
			}
        }

        public static IEnumerable<SyntaxNode> GetParents(this SyntaxNode node, int? limit = null) {
            return node?.Parent.GetThisAndParents() ?? Enumerable.Empty<SyntaxNode>();
		}

		public static T FindUp<T>(this SyntaxNode node, int? limit = null, bool skipThis = false)
			where T : SyntaxNode {
			var nodes = skipThis ? node.GetParents(limit) : node.GetThisAndParents(limit);
			return nodes
				.OfType<T>()
				.FirstOrDefault();
		}

		public static T FindUp<T>(this SyntaxNode node, Func<T, bool> isGood, int? limit = null)
            where T : SyntaxNode {
            return node.GetThisAndParents(limit)
                .OfType<T>()
                .FirstOrDefault(isGood);
        }

		public static T WithLeadingEndLine<T>(this T node, int count = 1) where T : SyntaxNode {
			if (count == 0) return node;
			var trivia = SyntaxFactory.SyntaxTrivia(SyntaxKind.EndOfLineTrivia, "\n");
			var triviaList = SyntaxFactory.TriviaList(Enumerable.Repeat(trivia, count));

			return node.WithLeadingTrivia(triviaList);
		}

		public static SyntaxToken WithLeadingEndLine(this SyntaxToken node, int count = 1) {
			if (count == 0) return node;
			var trivia = SyntaxFactory.SyntaxTrivia(SyntaxKind.EndOfLineTrivia, "\n");
			var triviaList = SyntaxFactory.TriviaList(Enumerable.Repeat(trivia, count));

			return node.WithLeadingTrivia(triviaList);
		}

		public static T WithTrailingEndLine<T>(this T node, int count = 1) where T : SyntaxNode {
			if (count == 0) return node;
			var trivia = SyntaxFactory.SyntaxTrivia(SyntaxKind.EndOfLineTrivia, "\n");
            var triviaList = SyntaxFactory.TriviaList(Enumerable.Repeat(trivia, count));

			return node.WithTrailingTrivia(triviaList);
		}

		public static IEnumerable<T> DescendantNodes<T>(this SyntaxNode node, Func<SyntaxNode, bool> descendIntoChildren = null, bool descendIntoTrivia = false) where T : SyntaxNode {
            return node.DescendantNodes(descendIntoChildren, descendIntoTrivia).OfType<T>();
        }

		public static bool ContainsEquivalentNode(this SyntaxNode node, SyntaxNode comparand) {
			return node
				?.DescendantNodesAndSelf()
				?.Any(n => n.IsEquivalentTo(comparand, topLevel: false))
				?? false;
		}

        public static T Nicefy<T>(this T node) where T : SyntaxNode {
            return node.WithAdditionalAnnotations(
                Formatter.Annotation,
                Simplifier.Annotation);
		}

		public static SyntaxToken Formattify(this SyntaxToken node) {
			return node.WithAdditionalAnnotations(Formatter.Annotation);
		}

		public static T Formattify<T>(this T node) where T : SyntaxNode {
            return node.WithAdditionalAnnotations(Formatter.Annotation);
		}

		public static SyntaxToken WithUserRename(this SyntaxToken token) {
            return token.WithAdditionalAnnotations(RenameAnnotation.Create());
		}

		public static T WithUserRename<T>(this T node) where T : SyntaxNode {
			var token = node.DescendantTokens().Cast<SyntaxToken?>().FirstOrDefault();

			Contract.Assume(token != null);

			return node.ReplaceToken(token.Value, token.Value.WithUserRename());
        }

        public static bool IsBreakable(this SyntaxNode p) {
            return p is ReturnStatementSyntax
                || p is ContinueStatementSyntax
                || p is BreakStatementSyntax
                || p is YieldStatementSyntax
                || p is ThrowStatementSyntax;
        }
    }
}
