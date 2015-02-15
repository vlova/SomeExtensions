using System.Composition;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.ArrowSyntax {
	[ExportCodeRefactoringProvider(nameof(PropertyFromArrowSyntaxProvider), CSharp), Shared]
	internal class PropertyFromArrowSyntaxProvider : BaseRefactoringProvider<BasePropertyDeclarationSyntax> {
		protected override int? FindUpLimit => 4;

		protected override bool IsGood(BasePropertyDeclarationSyntax property) {
			return (property.GetAccessor()?.Body == null)
				&& (property.As<dynamic>().ExpressionBody != null);
		}

		protected override void ComputeRefactorings(RefactoringContext context, BasePropertyDeclarationSyntax property) {
			context.RegisterAsync(new PropertyFromArrowSyntaxRefactoring(property));
		}
	}
}
