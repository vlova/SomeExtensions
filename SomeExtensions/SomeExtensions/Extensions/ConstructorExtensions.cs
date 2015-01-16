using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Extensions {
    public static class ConstructorExtensions {
        public static bool HasModifier(this ConstructorDeclarationSyntax field, SyntaxKind modifier) {
            if (field == null) {
                return false;
            }

            return field.Modifiers
                .Select(m => m.CSharpKind())
                .Contains(modifier);
        }

        public static ConstructorDeclarationSyntax WithModifiers(
            this ConstructorDeclarationSyntax field,
            params SyntaxKind[] modifiers) {
            return field.WithModifiers(SyntaxFactory.TokenList(modifiers.Select(SyntaxFactory.Token)));
        }
    }
}
