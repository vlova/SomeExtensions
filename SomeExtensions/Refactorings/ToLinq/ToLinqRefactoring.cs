using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static System.Diagnostics.Contracts.Contract;

namespace SomeExtensions.Refactorings.ToLinq {
	internal class ToLinqRefactoring : IRefactoring {
		ForEachStatementSyntax _foreach;
		IEnumerable<Func<ForEachStatementSyntax, ILinqTransformer>> _transformerFactories;

		public ToLinqRefactoring(
			ForEachStatementSyntax @foreach,
			IEnumerable<Func<ForEachStatementSyntax, ILinqTransformer>> transformerFactories) {
			Requires(_foreach != null);
			Requires(transformerFactories != null);

			_foreach = @foreach;
			_transformerFactories = transformerFactories;
		}

		public string Description => "To linq";

		public bool CanTransform(CompilationUnitSyntax root) {
			return FindTransformer(root, _foreach) != null;
		}

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token) {
			var @foreach = _foreach;
			while (true) {
				token.ThrowIfCancellationRequested();

				var transformer = FindTransformer(root, @foreach);
				if (transformer == null) return root;

				var transformed = transformer.Transform(root, token);
				root = transformed.Item1;
				@foreach = transformed.Item2;
			}
		}

		private ILinqTransformer FindTransformer(CompilationUnitSyntax root, ForEachStatementSyntax @foreach) {
			var transformers = _transformerFactories.Select(factory => factory(@foreach));
			var transformer = transformers.FirstOrDefault(t => t.CanTransform(root));
			return transformer;
		}
	}
}