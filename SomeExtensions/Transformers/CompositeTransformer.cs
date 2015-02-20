﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static System.Diagnostics.Contracts.Contract;

namespace SomeExtensions.Transformers {
	internal class CompositeTransformer<T> : ITransformer<T> where T : SyntaxNode {
		private readonly IEnumerable<TransformerFactory<T>> _transformerFactories;
		private readonly T _node;

		public CompositeTransformer(TransformerFactory<T>[] transformerFactories, T node) {
			Requires(transformerFactories != null && transformerFactories.Any());
			Requires(node != null);

			_transformerFactories = transformerFactories;
			_node = node;
		}

		public bool CanTransform(CompilationUnitSyntax root) {
			return FindTransformer(root, _node) != null;
		}

		public TransformationResult<T> Transform(CompilationUnitSyntax root, CancellationToken token) {
			var node = _node;
			while (true) {
				token.ThrowIfCancellationRequested();

				var transformer = FindTransformer(root, node);
				if (transformer == null) {
					break;
				}

				var transformed = transformer.Transform(root, token);
				root = transformed.Root;
				node = transformed.Node;
			}

			return Transformation.Transformed(root, node);
		}

		private ITransformer<T> FindTransformer(CompilationUnitSyntax root, T node) {
			var transformers = _transformerFactories.Select(factory => factory(node));
			var transformer = transformers.FirstOrDefault(t => t.CanTransform(root));
			return transformer;
		}
	}
}