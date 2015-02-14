using System.Composition;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;
using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.ArrowSyntax {
	[ExportCodeRefactoringProvider(nameof(PropertyFromArrowSyntaxProvider), CSharp), Shared]
	internal sealed class PropertyFromArrowSyntaxProvider : BaseRefactoringProvider<PropertyDeclarationSyntax> {
		protected override int? FindUpLimit => 4;

		protected override void ComputeRefactorings(RefactoringContext context, PropertyDeclarationSyntax property) {
			if (property.GetAccessor()?.Body != null) return;
			if (property.ExpressionBody == null) return;

			context.RegisterAsync(new PropertyFromArrowSyntaxRefactoring(property));
		}
	}
}
