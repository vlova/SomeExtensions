using System.Composition;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;
using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.ArrowSyntax {
	[ExportCodeRefactoringProvider(nameof(IndexerFromArrowSyntaxProvider), CSharp), Shared]
	internal sealed class IndexerFromArrowSyntaxProvider : BaseRefactoringProvider<IndexerDeclarationSyntax> {
		protected override int? FindUpLimit => 4;

		protected override void ComputeRefactorings(RefactoringContext context, IndexerDeclarationSyntax indexer) {
			if (indexer.GetAccessor()?.Body != null) return;
			if (indexer.ExpressionBody == null) return;

			context.RegisterAsync(new IndexerFromArrowSyntaxRefactoring(indexer));
		}
	}
}
