using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Transformers {
	delegate ITransformer<T> TransformerFactory<T>(T node) where T : SyntaxNode;

	internal interface ITransformer<T> where T : SyntaxNode {
		bool CanTransform(CompilationUnitSyntax root);

		TransformationResult<T> Transform(CompilationUnitSyntax root, CancellationToken token);
	}
}
