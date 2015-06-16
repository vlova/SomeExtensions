using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using static System.Diagnostics.Contracts.Contract;

namespace SomeExtensions.Transformers {
	internal class CompositeTransformer<T> : CompositeTransformer<T, T> where T : SyntaxNode {
		public CompositeTransformer(TransformerFactory<T>[] transformerFactories, T node)
			: base(Convert(transformerFactories), node) {
		}

		private static TransformerFactory<T, T>[] Convert(TransformerFactory<T>[] transformerFactories) {
			return transformerFactories.Select(t => new TransformerFactory<T, T>(_ => t(_))).ToArray();
		}
	}

	internal class CompositeTransformer<TSource, TRes> : ITransformer<TRes>
		where TSource : SyntaxNode
		where TRes : SyntaxNode {
		private readonly IEnumerable<TransformerFactory<TSource, TRes>> _transformerFactories;
		private readonly TSource _node;

		public CompositeTransformer(TransformerFactory<TSource, TRes>[] transformerFactories, TSource node) {
			Requires(transformerFactories != null && transformerFactories.Any());
			Requires(node != null);

			_transformerFactories = transformerFactories;
			_node = node;
		}

		public bool CanTransform(CompilationUnitSyntax root) {
			return FindTransformer(root, _node) != null;
		}

		public TransformationResult<TRes> Transform(CompilationUnitSyntax root) {
			Or<TSource, TRes> node = _node;
			while (true) {
				CancellationTokenExtensions.ThrowOnCancellation();

				var transformer = FindTransformer(root, node);
				if (transformer == null) {
					break;
				}

				var transformed = transformer.Transform(root);
				root = transformed.Root;
				node = transformed.Node;

				if (node.IsNull()) {
					break;
				}
			}

			return node.Match(
				source => {
					if (source is TRes) {
						return Transformation.Transformed(root, (TRes)(object)source);
					}
					throw new InvalidOperationException();
                },
				result => Transformation.Transformed(root, result));
		}

		private ITransformer<TRes> FindTransformer(CompilationUnitSyntax root, Or<TSource, TRes> node) {
			return node.Match(
				source => {
					var transformers = _transformerFactories.Select(factory => factory(source));
					var transformer = transformers.FirstOrDefault(t => t.CanTransform(root));
					return transformer;
				},
				another => null);
		}
	}
}
