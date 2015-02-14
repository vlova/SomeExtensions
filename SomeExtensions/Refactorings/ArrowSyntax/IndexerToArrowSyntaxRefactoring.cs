using System.Diagnostics.Contracts;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Refactorings.ArrowSyntax {
	internal class IndexerToArrowSyntaxRefactoring : IRefactoring {
		private readonly IndexerDeclarationSyntax _indexer;

		public IndexerToArrowSyntaxRefactoring(IndexerDeclarationSyntax indexer) {
			Contract.Requires(indexer != null);

			_indexer = indexer;
		}

		public string Description => "Use arrow syntax";

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token) {
			var newProperty = _indexer
				.WithAccessorList(null)
				.WithExpressionBody(ArrowExpressionClause(GetExpression()))
				.WithSemicolon(SemicolonToken.ToToken())
				.Nicefy();

			return root.ReplaceNode(_indexer, newProperty);
		}

		private ExpressionSyntax GetExpression() {
			var statement = _indexer.GetAccessor().Body.Statements.First();
			return statement.As<ReturnStatementSyntax>().Expression;
		}
	}
}