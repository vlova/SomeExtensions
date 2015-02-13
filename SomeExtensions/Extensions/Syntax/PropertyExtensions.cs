using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace SomeExtensions.Extensions.Syntax {
	public static class PropertyExtensions {
        public static bool IsAutomaticProperty(this BasePropertyDeclarationSyntax propertyDeclaration) {
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

		public static bool IsReadonlyProperty(this BasePropertyDeclarationSyntax property) {
			if (property == null) {
				return false;
			}

			return property.GetAccessor() != null
				&& property.GetAccessor().Body == null
				&& property.SetAccessor() == null;
		}


		public static AccessorDeclarationSyntax GetAccessor(this BasePropertyDeclarationSyntax property) {
			return property?.AccessorList?.GetAccessor();
		}

		public static AccessorDeclarationSyntax GetAccessor(this AccessorListSyntax accessorList) {
            return accessorList
				?.DescendantNodes()
                ?.OfType<AccessorDeclarationSyntax>()
                ?.FirstOrDefault(a => a.CSharpKind() == GetAccessorDeclaration);
        }

        public static AccessorDeclarationSyntax SetAccessor(this BasePropertyDeclarationSyntax property) {
            return property?.AccessorList?.SetAccessor();
		}

		public static AccessorDeclarationSyntax SetAccessor(this AccessorListSyntax accessorList) {
			return accessorList
				?.DescendantNodes()
				?.OfType<AccessorDeclarationSyntax>()
				?.FirstOrDefault(a => a.CSharpKind() == SetAccessorDeclaration);
		}
	}
}
