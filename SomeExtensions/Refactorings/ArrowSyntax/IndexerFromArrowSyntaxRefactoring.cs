using System.Diagnostics.Contracts;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace SomeExtensions.Refactorings.ArrowSyntax {
	internal class IndexerFromArrowSyntaxRefactoring : IRefactoring {
		private readonly IndexerDeclarationSyntax _indexer;

		public IndexerFromArrowSyntaxRefactoring(IndexerDeclarationSyntax indexer) {
			Contract.Requires(indexer != null);
			_indexer = indexer;
		}

		public string Description => "Use default declaration syntax";

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token) {
			var statement = _indexer.ExpressionBody.Expression.ToReturnStatement();
			var getter = AccessorDeclaration(GetAccessorDeclaration, Block(statement));

			return root.ReplaceNode(
				_indexer,
				_indexer
					.WithExpressionBody(null)
					.WithAccessorList(AccessorList(new[] { getter }.ToSyntaxList()))
					.WithSemicolon(None.ToToken())
					.Nicefy());
		}
	}
}