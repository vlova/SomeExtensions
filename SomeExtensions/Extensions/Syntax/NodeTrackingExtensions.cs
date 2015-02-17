using Microsoft.CodeAnalysis;

namespace SomeExtensions.Extensions.Syntax {
	public static class NodeTrackingExtensions {
		public static TNode TrackSelf<TNode>(this TNode node) where TNode : SyntaxNode {
			return node.TrackNodes(node);
		}

		public static TRoot ReplaceNodeWithTracking<TRoot>(this TRoot root, SyntaxNode oldNode, SyntaxNode newNode)
			where TRoot : SyntaxNode {
			return root.ReplaceNode(oldNode, newNode.TrackSelf());
		}
	}
}
