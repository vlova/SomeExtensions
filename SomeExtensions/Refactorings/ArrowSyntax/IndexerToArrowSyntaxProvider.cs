using System.Composition;

using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;
using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.ArrowSyntax {
	[ExportCodeRefactoringProvider(nameof(IndexerToArrowSyntaxProvider ), CSharp), Shared]
	public class IndexerToArrowSyntaxProvider : BaseRefactoringProvider<IndexerDeclarationSyntax> {
		protected override int? FindUpLimit => 6;

		protected override void ComputeRefactorings(RefactoringContext context, IndexerDeclarationSyntax Indexer) {
			if (Indexer.ExpressionBody != null) return;
			if (Indexer.SetAccessor() != null) return;
			if (Indexer.GetAccessor()?.Body?.Statements.Count != 1) return;

			var statement = Indexer.GetAccessor().Body.Statements.First();
			if (statement is ReturnStatementSyntax) {
				context.Register(new IndexerToArrowSyntaxRefactoring(Indexer));
			}
		}
	}
}
