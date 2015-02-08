using System.Composition;

using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;
using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.ToArrowSyntax {
	[ExportCodeRefactoringProvider(nameof(PropertyToArrowSyntaxProvider ), CSharp), Shared]
	public class PropertyToArrowSyntaxProvider : BaseRefactoringProvider<PropertyDeclarationSyntax> {
		protected override int? FindUpLimit => 6;

		protected override void ComputeRefactorings(RefactoringContext context, PropertyDeclarationSyntax property) {
			if (property.ExpressionBody != null) return;
			if (property.SetAccessor() != null) return;
			if (property.GetAccessor()?.Body?.Statements.Count != 1) return;

			var statement = property.GetAccessor().Body.Statements.First();
			if (statement is ReturnStatementSyntax) {
				context.Register(new PropertyToArrowSyntaxRefactoring(property));
			}
		}
	}
}
