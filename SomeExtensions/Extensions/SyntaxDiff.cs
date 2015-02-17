using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;

namespace SomeExtensions.Extensions {
	public struct NodeDiff<TNode> where TNode : SyntaxNode {
		public TNode First { get; }
		public TNode Second { get; }
		public bool AreEquivalent { get; }

		public NodeDiff(TNode first, TNode second) {
			First = first;
			Second = second;
			AreEquivalent = SyntaxFactory.AreEquivalent(first, second, topLevel: false);
		}
	}

	static class SyntaxDiff {
		public static IEnumerable<NodeDiff<TNode>> FindDiffNodes<TNode>(IEnumerable<SyntaxNode> firstNodes, IEnumerable<SyntaxNode> secondNodes) where TNode : SyntaxNode {
			var diffNodes = FindDiffNodes(firstNodes, secondNodes).ToList();
			if (diffNodes.All(n => n.First is TNode && n.Second is TNode)) {
				return diffNodes.Select(n => new NodeDiff<TNode>(n.First as TNode, n.Second as TNode));
			} else {
				return Enumerable.Empty<NodeDiff<TNode>>();
			}
        }

		public static IEnumerable<NodeDiff<SyntaxNode>> FindDiffNodes(IEnumerable<SyntaxNode> firstNodes, IEnumerable<SyntaxNode> secondNodes)  {
			var notSameNodes = firstNodes
				.Zip(secondNodes, (a, b) => new NodeDiff<SyntaxNode>(a, b))
				.Where(x => !x.AreEquivalent)
				.ToList();

			foreach (var diffNode in notSameNodes) {
				if (!CanGoDepeer(diffNode)) {
					yield return new NodeDiff<SyntaxNode>(diffNode.First, diffNode.Second);
				}
				else {
					var childDiffNodes = FindDiffNodes(diffNode.First, diffNode.Second).ToList();
					foreach (var childDiffNode in childDiffNodes) {
						yield return childDiffNode;
					}
				}
			}
		}

		private static bool CanGoDepeer<TNode>(NodeDiff<TNode> diffNode) where TNode : SyntaxNode {
			return diffNode.First.RawKind == diffNode.Second.RawKind
				&& diffNode.First.ChildNodes().Any()
				&& diffNode.Second.ChildNodes().Any()
				&& CanBreakInvocation(diffNode.First as InvocationExpressionSyntax, diffNode.Second as InvocationExpressionSyntax);
		}

		private static bool CanBreakInvocation(InvocationExpressionSyntax first, InvocationExpressionSyntax second) {
			if (first == null && second == null) return true;
			// TODO: allow breaking invocation, when method is delegate
			return first.GetMethodName() == second.GetMethodName();
		}

		public static IEnumerable<NodeDiff<SyntaxNode>> FindDiffNodes(SyntaxNode first, SyntaxNode second) {
			if (SyntaxFactory.AreEquivalent(first, second, topLevel: false)) {
				return null;
			}

			if (first.ChildNodes().Count() != second.ChildNodes().Count()) {
				return new[] { new NodeDiff<SyntaxNode>(first, second) };
			}

			return FindDiffNodes(first.ChildNodes(), second.ChildNodes());
		}
	}
}
