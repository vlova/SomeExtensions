using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions.Roslyn;
using SomeExtensions.Extensions.Syntax;

using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.ToReadonlyProperty {
	[ExportCodeRefactoringProvider(nameof(ToReadonlyPropertyProvider), CSharp), Shared]
    internal class ToReadonlyPropertyProvider : BaseRefactoringProvider {
        protected override void ComputeRefactorings(CodeRefactoringContext context, SyntaxNode root, SyntaxNode node) {
            var property = node.FindUp<PropertyDeclarationSyntax>();

            if (!property.IsAutomaticProperty()) {
                return;
            }

            context.RegisterRefactoring(new ToReadonlyPropertyRefactoring(property));
        }
    }
}
