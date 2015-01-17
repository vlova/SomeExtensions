using System.Linq;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Extensions {
    public static class PropertyExtensions {
        public static bool IsAutomaticProperty(this PropertyDeclarationSyntax propertyDeclaration) {
            if (propertyDeclaration == null) {
                return false;
            }

            var accessors = propertyDeclaration
                .AccessorList
                .ChildNodes()
                .OfType<AccessorDeclarationSyntax>()
                .ToList();

            return accessors.Any()
                && accessors.All(a => a.Body == null);
        }

        public static AccessorDeclarationSyntax GetAccessor(this AccessorListSyntax accessorList) {
            return accessorList
                .DescendantNodes()
                .OfType<AccessorDeclarationSyntax>()
                .FirstOrDefault(a => a.CSharpKind() == SyntaxKind.GetAccessorDeclaration);
        }

        public static AccessorDeclarationSyntax SetAccessor(this AccessorListSyntax accessorList) {
            return accessorList
                .DescendantNodes()
                .OfType<AccessorDeclarationSyntax>()
                .FirstOrDefault(a => a.CSharpKind() == SyntaxKind.SetAccessorDeclaration);
        }
    }
}
