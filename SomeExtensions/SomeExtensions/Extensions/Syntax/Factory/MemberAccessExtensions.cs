using System;
using System.Linq;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Extensions.Syntax {
	public static partial class SyntaxFactoryExtensions {
		public static MemberAccessExpressionSyntax OfThis(this IdentifierNameSyntax identifier) {
			return SyntaxFactory.MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression(), identifier);
		}

		public static ExpressionSyntax AccessTo(this ExpressionSyntax to, string what) {
			if (to == null) {
				return what.ToIdentifierName();
			}

			return SyntaxFactory.MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				to,
				what.ToIdentifierName());
		}

		public static MemberAccessExpressionSyntax AccessTo(this string name, string what) {
			return SyntaxFactory.MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				name.ToIdentifierName(),
				what.ToIdentifierName());
		}

		public static ExpressionSyntax ToMemberAccess(this string names) {
			var nameArray = names.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
			return nameArray.Aggregate((ExpressionSyntax)null, (to, what) => to.AccessTo(what));
		}
	}
}
