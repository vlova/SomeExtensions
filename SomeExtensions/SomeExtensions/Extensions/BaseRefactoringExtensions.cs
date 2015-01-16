using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;

using SomeExtensions.Refactorings;

namespace SomeExtensions.Extensions {
    public static class BaseRefactoringExtensions {
        public static CodeAction ToCodeAction(this IRefactoring refactoring) {
            return CodeAction.Create(refactoring.Description, c => refactoring.Compute(c));
        }

        public static void RegisterRefactoring(this CodeRefactoringContext context, IRefactoring refactoring) {
            context.RegisterRefactoring(refactoring.ToCodeAction());
        }
    }
}
