using System.Composition;

using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions.Syntax;

using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.ToReadonlyProperty {
	[ExportCodeRefactoringProvider(nameof(ToReadonlyPropertyProvider), CSharp), Shared]
    internal class ToReadonlyPropertyProvider : BaseRefactoringProvider<PropertyDeclarationSyntax> {
		protected override int? FindUpLimit => 3;

		protected override void ComputeRefactorings(RefactoringContext context, PropertyDeclarationSyntax property) {
            if (!property.IsAutomaticProperty()) {
                return;
            }

            context.RegisterAsync(new ToReadonlyPropertyRefactoring(property));
        }
    }
}
