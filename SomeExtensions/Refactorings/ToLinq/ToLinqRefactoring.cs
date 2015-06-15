using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Refactorings.ToLinq.Transformers;
using SomeExtensions.Transformers;
using static System.Diagnostics.Contracts.Contract;

namespace SomeExtensions.Refactorings.ToLinq {
	internal class ToLinqRefactoring : IRefactoring {
		private readonly ForEachStatementSyntax _foreach;
		private readonly TransformerFactory<ForEachStatementSyntax> _foreachTransformer;
		private readonly TransformerFactory<InvocationExpressionSyntax> _simplifierFactories;

		public ToLinqRefactoring(ForEachStatementSyntax @foreach,
			TransformerFactory<ForEachStatementSyntax> transformerFactories,
			TransformerFactory<InvocationExpressionSyntax> simplifierFactories) {
			Requires(@foreach != null);
			Requires(transformerFactories != null);
			Requires(simplifierFactories != null);

			_foreach = @foreach;
			_foreachTransformer = transformerFactories;
			_simplifierFactories = simplifierFactories;
		}

		public string Description => "To linq";

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root) {
			var transformedForeach = root.Transformed(_foreach)
				.Transform(_foreachTransformer)
				.SelectNode(node => node.Expression as InvocationExpressionSyntax)
				.Transform(_simplifierFactories)
				.SelectNode(node => node.Parent as ForEachStatementSyntax);

			var addTransformer = new CollectionAddTransformer(transformedForeach.Node);
			if (addTransformer.CanTransform(transformedForeach.Root)) {
				return addTransformer.Transform(transformedForeach.Root)
					.Root;
			} else {
				return transformedForeach.Root;
			}
		}

		internal bool CanTransform(CompilationUnitSyntax root) {
			return _foreachTransformer(_foreach).CanTransform(root)
				|| new CollectionAddTransformer(_foreach).CanTransform(root);
		}
	}
}