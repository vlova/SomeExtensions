using System.Composition;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.ArrowSyntax {
	[ExportCodeRefactoringProvider(nameof(MethodFromArrowSyntaxProvider), CSharp), Shared]
	internal sealed class MethodFromArrowSyntaxProvider : BaseRefactoringProvider<MethodDeclarationSyntax> {
		protected override int? FindUpLimit => 4;

		protected override void ComputeRefactorings(RefactoringContext context, MethodDeclarationSyntax method) {
			if (method.Body != null) return;
			if (method.ExpressionBody == null) return;

			context.RegisterAsync(new MethodFromArrowSyntaxRefactoring(method));
		}
	}
}
