using System.Composition;

using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.ArrowSyntax {
	[ExportCodeRefactoringProvider(nameof(PropertyToArrowSyntaxProvider ), CSharp), Shared]
	public class PropertyToArrowSyntaxProvider : BaseRefactoringProvider<BasePropertyDeclarationSyntax> {
		protected override int? FindUpLimit => 6;

		protected override bool IsGood(BasePropertyDeclarationSyntax property) {
			return property.As<dynamic>().ExpressionBody == null
				&& property.SetAccessor() == null
				&& property.GetAccessor()?.Body?.Statements.Count == 1;
		}

		protected override void ComputeRefactorings(RefactoringContext context, BasePropertyDeclarationSyntax property) {
			var statement = property.GetAccessor().Body.Statements.First();
			if (statement is ReturnStatementSyntax) {
				context.Register(new PropertyToArrowSyntaxRefactoring(property));
			}
		}
	}
}
