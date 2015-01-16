using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Refactorings.InjectFromConstructor {
    internal static class Helpers {
        internal static bool NeedInject(FieldDeclarationSyntax field, ConstructorDeclarationSyntax constructor, CancellationToken token) {
            var variable = field.Declaration.Variables.FirstOrDefault();
            var fieldName = variable.Identifier.Text;
            var assigments = constructor.Body?.DescendantNodes().OfType<AssignmentExpressionSyntax>().ToList();

            foreach (var assigment in assigments) {
                if (token.IsCancellationRequested) {
                    return false;
                }

                if ((assigment.Left as IdentifierNameSyntax)?.Identifier.Text == fieldName) {
                    return false;
                }

                var memberAccess = (assigment.Left as MemberAccessExpressionSyntax);
                if (memberAccess?.Expression is ThisExpressionSyntax && memberAccess?.Name?.Identifier.Text == fieldName) {
                    return false;
                }
            }

            return true;
        }
    }
}
