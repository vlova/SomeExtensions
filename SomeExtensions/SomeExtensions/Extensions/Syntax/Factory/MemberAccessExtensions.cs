using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static System.StringSplitOptions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;


namespace SomeExtensions.Extensions.Syntax {
	public static partial class SyntaxFactoryExtensions {
		public static MemberAccessExpressionSyntax OfThis(this IdentifierNameSyntax identifier) {
			return MemberAccessExpression(
				SimpleMemberAccessExpression, ThisExpression(), identifier);
		}

		public static ExpressionSyntax AccessTo(this ExpressionSyntax to, string what) {
			if (to == null) {
				return what.ToIdentifierName();
			}

			return MemberAccessExpression(
				SimpleMemberAccessExpression,
				to,
				what.ToIdentifierName());
		}

		public static MemberAccessExpressionSyntax AccessTo(this string name, string what) {
			return MemberAccessExpression(
				SimpleMemberAccessExpression,
				name.ToIdentifierName(),
				what.ToIdentifierName());
		}

		public static ExpressionSyntax ToMemberAccess(this string names) {
			var nameArray = names.Split(new char[] { '.' }, RemoveEmptyEntries);
			return nameArray.Aggregate((ExpressionSyntax)null, (to, what) => to.AccessTo(what));
		}
	}
}
