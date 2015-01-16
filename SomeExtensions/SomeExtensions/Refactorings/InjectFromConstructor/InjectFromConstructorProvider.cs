using System.Composition;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;

namespace SomeExtensions.Refactorings.InjectFromConstructor {
    [ExportCodeRefactoringProvider(RefactoringId, LanguageNames.CSharp), Shared]
    internal class InjectFromConstructorProvider : BaseRefactoringProvider {
        public const string RefactoringId = "InjectFromConstructor";

        protected override void ComputeRefactorings(CodeRefactoringContext context, SyntaxNode root, SyntaxNode node) {
            var field = node.FindUp<FieldDeclarationSyntax>();

            if (!CanHandle(field)) {
                return;
            };

            var type = field.Parent as TypeDeclarationSyntax;
            var constructors = type.FindConstructors();

            if (!constructors.Any()) {
                context.RegisterRefactoring(new CreateConstructor(context.Document, field));
                return;
            }

            if (constructors.Count() == 1) {
                if (Helpers.NeedInject(field, constructors.First(), context.CancellationToken)) {
                    context.RegisterRefactoring(new InjectFromConstructor(
                        context.Document,
                        field,
                        constructors.First()));
                }
            }
            else {
                context.RegisterRefactoring(new InjectFromAllConstructors(context.Document, field));

                var index = 1;
                foreach (var constructor in constructors) {
                    if (Helpers.NeedInject(field, constructor, context.CancellationToken)) {
                        context.RegisterRefactoring(new InjectFromConstructor(
                            context.Document,
                            field,
                            constructor,
                            index));

                        index++;
                    }
                }
            }
        }

        private static bool CanHandle(FieldDeclarationSyntax field) {
            return field != null
                && !field.IsConstant()
                && !field.IsStatic()
                && field.ContainsOneVariable();
        }
    }

}
