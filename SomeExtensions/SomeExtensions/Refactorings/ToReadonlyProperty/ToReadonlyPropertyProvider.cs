using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;

namespace SomeExtensions.Refactorings.ToReadonlyProperty {
    [ExportCodeRefactoringProvider(RefactoringId, LanguageNames.CSharp), Shared]
    internal class ToReadonlyPropertyProvider : BaseRefactoringProvider {
        public const string RefactoringId = "ToReadonlyProperty";

        protected override void ComputeRefactorings(CodeRefactoringContext context, SyntaxNode root, SyntaxNode node) {
            var property = node.FindUp<PropertyDeclarationSyntax>();

            if (!property.IsAutomaticProperty()) {
                return;
            }

            context.RegisterRefactoring(new ToReadonlyProperty(property));
        }
    }
}
