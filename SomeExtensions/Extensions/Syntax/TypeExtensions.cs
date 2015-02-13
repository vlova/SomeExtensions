using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Extensions.Syntax {
	public static class TypeExtensions {
		public static string GetPartialTypeName(this TypeSyntax type) {
			return type.As<IdentifierNameSyntax>()?.Identifier.Text
				?? type.As<GenericNameSyntax>()?.Identifier.Text
				?? type.As<PointerTypeSyntax>()?.ElementType?.GetPartialTypeName()
				?? type.As<ArrayTypeSyntax>()?.ElementType?.GetPartialTypeName()
				?? type.As<NullableTypeSyntax>()?.ElementType?.GetPartialTypeName()
				?? type.As<PredefinedTypeSyntax>()?.GetText()?.ToString()?.Trim();
		}

		public static bool IsGenericTypeParameterOf(this TypeSyntax type, MethodDeclarationSyntax method) {
			return method
				?.TypeParameterList
				?.Parameters
				.Any(t => t.Identifier.Text == type.GetPartialTypeName())
				?? false;
		}

		public static bool ContainsGenericTypeParameterOf(this TypeSyntax type, MethodDeclarationSyntax method) {
			return type.DescendantNodes<TypeSyntax>().Any(t => t.IsGenericTypeParameterOf(method));
		}
	}
}
