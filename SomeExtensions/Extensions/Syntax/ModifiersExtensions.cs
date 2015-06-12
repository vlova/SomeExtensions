using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using static System.Linq.Enumerable;
using System.Collections.Generic;

namespace SomeExtensions.Extensions.Syntax {
	public static class ModifiersExtensions {
		public static TMember AppendModifiers<TMember>(this TMember member, params SyntaxKind[] modifiers)
			where TMember : MemberDeclarationSyntax {
			var oldModifiers = (SyntaxTokenList)((dynamic)member).Modifiers;
			var newModifiers = Enumerable.Concat(
					oldModifiers.Select(t => t.Kind()),
					modifiers)
				.ToTokenList();
			return ((dynamic)member).WithModifiers(newModifiers);
		}

		public static TMember WithModifiers<TMember>(this TMember member, params SyntaxKind[] modifiers)
			where TMember : MemberDeclarationSyntax {
			return ((dynamic)member).WithModifiers(modifiers.ToTokenList());
		}

		public static TMember WithModifiers<TMember>(this TMember member, IEnumerable<SyntaxKind> modifiers)
			where TMember : MemberDeclarationSyntax {
			return ((dynamic)member).WithModifiers(modifiers.ToTokenList());
		}

		private static bool HasModifierInternal(dynamic anything, SyntaxKind modifier) {
			if (anything == null) {
				return false;
			}

			return ((SyntaxTokenList)anything.Modifiers)
				.Select(m => m.Kind())
				.Contains(modifier);
		}

		public static bool HasModifier(this MemberDeclarationSyntax member, SyntaxKind modifier)
			=> HasModifierInternal(member, modifier);

		public static bool HasModifier(this ParameterSyntax parameter, SyntaxKind modifier)
			=> HasModifierInternal(parameter, modifier);

		public static bool IsConstant(this MemberDeclarationSyntax member)
			=> member.HasModifier(ConstKeyword);

		public static bool IsStatic(this MemberDeclarationSyntax field)
			=> field.HasModifier(StaticKeyword);

		public static bool IsPublic(this MemberDeclarationSyntax field)
			=> field.HasModifier(PublicKeyword);

		public static bool IsProtected(this MemberDeclarationSyntax field)
			=> field.HasModifier(ProtectedKeyword);
	}
}
