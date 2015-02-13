using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static System.StringSplitOptions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;


namespace SomeExtensions.Extensions.Syntax {
	public static partial class SyntaxFactoryExtensions {
		public static MemberAccessExpressionSyntax OfThis(this SimpleNameSyntax identifier) {
			return MemberAccessExpression(
				SimpleMemberAccessExpression, ThisExpression(), identifier);
		}

		public static ExpressionSyntax AccessTo(this ExpressionSyntax to, SimpleNameSyntax what) {
			if (to == null) {
				return what;
			}

			return MemberAccessExpression(
				SimpleMemberAccessExpression,
				to,
				what);
		}

		public static ExpressionSyntax AccessTo(this ExpressionSyntax to, string what) {
			return to.AccessTo(what.ToIdentifierName());
		}

		public static ExpressionSyntax AccessTo(this string name, string what) {
			return name.ToIdentifierName().AccessTo(what);
		}

		public static ExpressionSyntax AccessTo(this string name, SimpleNameSyntax what) {
			return name.ToIdentifierName().AccessTo(what);
		}

		public static ExpressionSyntax ToMemberAccess(this string names) {
			var nameArray = names.Split(new char[] { '.' }, RemoveEmptyEntries);
			return nameArray.Aggregate((ExpressionSyntax)null, (to, what) => to.AccessTo(what));
		}
	}
}
