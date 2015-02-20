using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;

namespace SomeExtensions.Transformers {
	internal static class Transformation {
		public static TransformerFactory<T> Composite<T>(params TransformerFactory<T>[] transformerFactories)
			where T : SyntaxNode {
			return (node) => new CompositeTransformer<T>(transformerFactories, node);
		}

		public static TransformationResult<T> Transform<T>(this CompilationUnitSyntax root, T oldNode, T newNode) where T : SyntaxNode {
			var newRoot = root.ReplaceNodeWithTracking(oldNode, newNode);
			return new TransformationResult<T>(newRoot, newRoot.GetCurrentNode(newNode));
		}

		public static TransformationResult<T> Transformed<T>(this CompilationUnitSyntax root, T node) where T : SyntaxNode {
			return new TransformationResult<T>(root, node);
		}

		public static TransformationResult<TResult> SelectNode<TArg, TResult>(this TransformationResult<TArg> originalResult, Func<TArg, TResult> selector)
			where TArg : SyntaxNode
			where TResult : SyntaxNode {
			return Transformed(originalResult.Root, selector(originalResult.Node));
		}

		public static TransformationResult<T> Transform<T>(this TransformationResult<T> result, TransformerFactory<T> _transformerFactory, CancellationToken token)
			where T : SyntaxNode {
			if (_transformerFactory == null) {
				return result;
			}

			var transformer = _transformerFactory(result.Node);
			if (transformer?.CanTransform(result.Root) ?? false) {
				result = transformer.Transform(result.Root, token);
			}

			return result;
		}
	}
}
