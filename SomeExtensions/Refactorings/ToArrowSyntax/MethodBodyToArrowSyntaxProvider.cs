using System.Composition;

using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.ToArrowSyntax {
	[ExportCodeRefactoringProvider(nameof(MethodBodyToArrowSyntaxProvider), CSharp), Shared]
	public class MethodBodyToArrowSyntaxProvider : BaseRefactoringProvider<MethodDeclarationSyntax> {
		protected override int? FindUpLimit => 4;

		protected override void ComputeRefactorings(RefactoringContext context, MethodDeclarationSyntax method) {
			if (method.ExpressionBody != null) return;
			if (method.Body?.Statements.Count != 1) return;

			var statement = method.Body.Statements.First();
			if (statement is ReturnStatementSyntax || statement is ExpressionStatementSyntax) {
				context.Register(new MethodBodyToArrowSyntaxRefactoring(method));
			}
		}
	}
}
