using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Extensions {
    public static class FieldExtensions {
        public static bool HasModifier(this FieldDeclarationSyntax field, SyntaxKind modifier) {
            if (field == null) {
                return false;
            }

            return field.Modifiers
                .Select(m => m.CSharpKind())
                .Contains(modifier);
        }

        public static FieldDeclarationSyntax WithModifiers(
            this FieldDeclarationSyntax field,
            params SyntaxKind[] modifiers) {
            return field.WithModifiers(SyntaxFactory.TokenList(modifiers.Select(SyntaxFactory.Token)));
        }

        public static bool IsConstant(this FieldDeclarationSyntax field) {
            return field.HasModifier(SyntaxKind.ConstKeyword);
        }

        public static bool IsStatic(this FieldDeclarationSyntax field) {
            return field.HasModifier(SyntaxKind.StaticKeyword);
        }

        public static bool ContainsOneVariable(this FieldDeclarationSyntax field) {
            return field?.Declaration?.Variables.Count == 1;
        }
    }
}
