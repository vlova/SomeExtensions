using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

namespace SomeExtensions.Extensions {
	public static class SyntaxNodeExtensions {
        public static TRoot InsertAfter<TRoot>(this TRoot root, SyntaxNode node, SyntaxNode newNode) where TRoot : SyntaxNode {
            return root.InsertNodesAfter(node, new[] { newNode });
        }

        public static TRoot InsertBefore<TRoot>(this TRoot root, SyntaxNode node, SyntaxNode newNode) where TRoot : SyntaxNode {
            return root.InsertNodesBefore(node, new[] { newNode });
        }

        public static IEnumerable<SyntaxNode> GetThisAndParents(this SyntaxNode node) {
            while (node != null) {
                yield return node;
                node = node.Parent;
            }
        }

        public static IEnumerable<SyntaxNode> GetParents(this SyntaxNode node) {
            node = node?.Parent;
            while (node != null) {
                yield return node;
                node = node.Parent;
            }
        }

        public static T FindUp<T>(this SyntaxNode node)
            where T : class {
            return node.GetThisAndParents()
                .Select(n => n as T)
                .Where(n => n != null)
                .FirstOrDefault();
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
