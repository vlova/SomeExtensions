using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Extensions {
	static class SyntaxDiff {
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

		public static NodeDiff<TNode>? FindDiffNode<TNode>(IEnumerable<SyntaxNode> firstNodes, IEnumerable<SyntaxNode> secondNodes) where TNode : SyntaxNode {
			var notSameNodes = firstNodes
				.Zip(secondNodes, (a, b) => new NodeDiff<SyntaxNode>(a, b))
				.Where(x => !x.AreEquivalent)
				.ToList();

			if (notSameNodes.Count != 1) {
				return null;
			}

			var diffNode = notSameNodes.First();
			if (diffNode.First is TNode && diffNode.Second is TNode) {
				if (!CanGoDepeer(diffNode)) {
					return new NodeDiff<TNode>(diffNode.First as TNode, diffNode.Second as TNode);
				}
			}

			return FindDiffNode<TNode>(diffNode.First, diffNode.Second);
		}

		private static bool CanGoDepeer<TNode>(NodeDiff<TNode> diffNode) where TNode : SyntaxNode {
			return diffNode.First.ChildNodes().Count() != 0 && diffNode.Second.ChildNodes().Count() != 0;
		}

		public static NodeDiff<TNode>? FindDiffNode<TNode>(SyntaxNode first, SyntaxNode second) where TNode : SyntaxNode {
			if (SyntaxFactory.AreEquivalent(first, second, topLevel: false)) {
				return null;
			}

			return FindDiffNode<TNode>(first.ChildNodes(), second.ChildNodes());
		}
	}
}
