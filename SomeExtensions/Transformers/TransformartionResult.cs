using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static System.Diagnostics.Contracts.Contract;

namespace SomeExtensions.Transformers {
	internal struct TransformationResult<T> where T : SyntaxNode {
		public T Node { get; }
		public CompilationUnitSyntax Root { get; }

		public TransformationResult(CompilationUnitSyntax root, T node) {
			Requires(root != null);
			Requires(node != null);

			Root = root;
			Node = node;
		}
	}
}
