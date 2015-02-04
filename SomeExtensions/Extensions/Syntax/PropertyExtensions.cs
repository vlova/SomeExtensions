using System.Linq;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Extensions.Syntax {
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

		public static bool IsReadonlyProperty(this PropertyDeclarationSyntax property) {
			if (property == null) {
				return false;
			}

			return property.GetAccessor() != null
				&& property.GetAccessor().Body == null
				&& property.SetAccessor() == null;
		}

		public static AccessorDeclarationSyntax GetAccessor(this PropertyDeclarationSyntax property) {
            return property
				.AccessorList
				.DescendantNodes()
                .OfType<AccessorDeclarationSyntax>()
                .FirstOrDefault(a => a.CSharpKind() == SyntaxKind.GetAccessorDeclaration);
        }

        public static AccessorDeclarationSyntax SetAccessor(this PropertyDeclarationSyntax property) {
            return property?.AccessorList?.SetAccessor();
		}

		public static AccessorDeclarationSyntax SetAccessor(this AccessorListSyntax accessorList) {
			return accessorList
				.DescendantNodes()
				.OfType<AccessorDeclarationSyntax>()
				.FirstOrDefault(a => a.CSharpKind() == SyntaxKind.SetAccessorDeclaration);
		}
	}
}
