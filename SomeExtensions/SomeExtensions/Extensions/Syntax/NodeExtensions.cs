using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

namespace SomeExtensions.Extensions.Syntax {
	public static class NodeExtensions {
        public static TRoot InsertAfter<TRoot>(this TRoot root, SyntaxNode node, SyntaxNode newNode) where TRoot : SyntaxNode {
            return root.InsertNodesAfter(node, new[] { newNode });
        }

        public static TRoot InsertBefore<TRoot>(this TRoot root, SyntaxNode node, SyntaxNode newNode) where TRoot : SyntaxNode {
            return root.InsertNodesBefore(node, new[] { newNode });
        }

        public static IEnumerable<SyntaxNode> GetThisAndParents(this SyntaxNode node, int? limit = null) {
            while (node != null) {
				if (limit < 0) {
					break;
				}

                yield return node;
                node = node.Parent;
				limit = limit - 1;
			}
        }

        public static IEnumerable<SyntaxNode> GetParents(this SyntaxNode node) {
            node = node?.Parent;
            while (node != null) {
                yield return node;
                node = node.Parent;
            }
        }

        public static T FindUp<T>(this SyntaxNode node, int? limit = null)
            where T : SyntaxNode {
            return node.GetThisAndParents(limit)
                .OfType<T>()
                .FirstOrDefault();
        }

		public static T WithLeadingEndLine<T>(this T node) where T : SyntaxNode {
			var triviaList =
				SyntaxFactory.TriviaList(
					SyntaxFactory.SyntaxTrivia(
						SyntaxKind.EndOfLineTrivia, "\n"));

            return node.WithLeadingTrivia(triviaList);
		}

		public static IEnumerable<T> DescendantNodes<T>(this SyntaxNode node, Func<SyntaxNode, bool> descendIntoChildren = null, bool descendIntoTrivia = false) where T : SyntaxNode {
            return node.DescendantNodes(descendIntoChildren, descendIntoTrivia).OfType<T>();
        }

        public static T Nicefy<T>(this T node) where T : SyntaxNode {
            return node.WithAdditionalAnnotations(
                Formatter.Annotation,
                Simplifier.Annotation);
        }
    }
}
