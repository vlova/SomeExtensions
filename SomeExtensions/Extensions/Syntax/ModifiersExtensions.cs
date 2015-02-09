using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace SomeExtensions.Extensions.Syntax {
	public static class ModifiersExtensions {
		public static ConstructorDeclarationSyntax WithModifiers(
			this ConstructorDeclarationSyntax field,
			params SyntaxKind[] modifiers) {
			return field.WithModifiers(modifiers.ToTokenList());
		}

		public static MethodDeclarationSyntax WithModifiers(
			this MethodDeclarationSyntax field,
			params SyntaxKind[] modifiers) {
			return field.WithModifiers(modifiers.ToTokenList());
		}

		public static ConversionOperatorDeclarationSyntax WithModifiers(
			this ConversionOperatorDeclarationSyntax field,
			params SyntaxKind[] modifiers) {
			return field.WithModifiers(modifiers.ToTokenList());
		}

		public static ClassDeclarationSyntax WithModifiers(
			this ClassDeclarationSyntax field,
			params SyntaxKind[] modifiers) {
			return field.WithModifiers(modifiers.ToTokenList());
		}

		public static bool HasModifier(this BaseMethodDeclarationSyntax field, SyntaxKind modifier) {
			if (field == null) {
				return false;
			}

			return field.Modifiers
				.Select(m => m.CSharpKind())
				.Contains(modifier);
		}

		public static bool HasModifier(this BasePropertyDeclarationSyntax property, SyntaxKind modifier) {
			if (property == null) {
				return false;
			}

			return property.Modifiers
				.Select(m => m.CSharpKind())
				.Contains(modifier);
		}

		public static bool HasModifier(this BaseFieldDeclarationSyntax field, SyntaxKind modifier) {
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
			return field.WithModifiers(modifiers.ToTokenList());
		}

		public static FieldDeclarationSyntax WithModifiers(
			this FieldDeclarationSyntax field,
			IEnumerable<SyntaxKind> modifiers) {
			return field.WithModifiers(modifiers.ToTokenList());
		}

		public static bool IsConstant(this BaseFieldDeclarationSyntax field) {
			return field.HasModifier(ConstKeyword);
		}

		public static bool IsStatic(this BaseFieldDeclarationSyntax field) {
			return field.HasModifier(StaticKeyword);
		}
	}
}
