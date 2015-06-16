using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;
using SomeExtensions.Refactorings.ToLinq.Transformers;
using SomeExtensions.Transformers;
using static System.Diagnostics.Contracts.Contract;

namespace SomeExtensions.Refactorings.ToLinq {
	internal class ToLinqRefactoring : IRefactoring {
		private readonly ForEachStatementSyntax _foreach;
		private readonly TransformerFactory<ForEachStatementSyntax> _foreachTransformer;
		private readonly TransformerFactory<ForEachStatementSyntax, LocalDeclarationStatementSyntax> _aggregateTransformer;
		private readonly TransformerFactory<InvocationExpressionSyntax> _simplifierFactories;

		public ToLinqRefactoring(ForEachStatementSyntax @foreach,
			TransformerFactory<ForEachStatementSyntax> transformerFactories,
			TransformerFactory<ForEachStatementSyntax, LocalDeclarationStatementSyntax> aggregateFactories,
            TransformerFactory<InvocationExpressionSyntax> simplifierFactories) {
			Requires(@foreach != null);
			Requires(transformerFactories != null);
			Requires(aggregateFactories != null);
			Requires(simplifierFactories != null);

			_foreach = @foreach;
			_foreachTransformer = transformerFactories;
			_aggregateTransformer = aggregateFactories;
			_simplifierFactories = simplifierFactories;
		}

		public string Description => "To linq";

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root) {
			var transformed = root.Transformed(_foreach)
				.Transform(_foreachTransformer)
				.Transform(_aggregateTransformer)
				.Match(
					@foreach => @foreach.SelectNode(_ => _.Expression),
					@local => @local.SelectNode(_ => _.GetVariable().Initializer.Value))
				.SelectNode(node => node as InvocationExpressionSyntax)
				.Transform(_simplifierFactories);

			return transformed.Root;
		}

		internal bool CanTransform(CompilationUnitSyntax root) {
			return _foreachTransformer(_foreach).CanTransform(root)
				|| new ToListTransformer(_foreach).CanTransform(root);
		}
	}
}