using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Extensions.Syntax {
	public static class InvocationExtensions {
		public static MemberAccessExpressionSyntax GetMemberAccessExpression(this InvocationExpressionSyntax invocation) {
			return invocation
				?.Expression
				?.As<MemberAccessExpressionSyntax>();
		}

		public static string GetClassName(this InvocationExpressionSyntax invocation) {
			return invocation.GetMemberAccessExpression()
				?.Expression
				?.As<SimpleNameSyntax>()
				?.Identifier.Text;
		}

		public static string GetMethodName(this InvocationExpressionSyntax invocation) {
			var expression = invocation?.Expression;
			if (expression is IdentifierNameSyntax) {
				return expression.As<IdentifierNameSyntax>().Identifier.Text;
			}

			return expression
				?.As<MemberAccessExpressionSyntax>()
				?.Name
				?.Identifier.Text;
		}

		public static ArgumentSyntax GetArgument(
			this InvocationExpressionSyntax invocation,
			int position) {
			return invocation?.ArgumentList?.Arguments.At(position);
		}

		public static ArgumentSyntax GetFirstArgument(this InvocationExpressionSyntax invocation) {
			return invocation.GetArgument(0);
		}

		public static SimpleLambdaExpressionSyntax GetLinqLambda(this InvocationExpressionSyntax invocation) {
			return invocation?.GetFirstArgument()?.Expression?.As<SimpleLambdaExpressionSyntax>();
		}

		public static IEnumerable<InvocationExpressionSyntax> GetChildInvocationSequence(this InvocationExpressionSyntax invocation) {
			while (invocation != null) {
				yield return invocation;
				invocation = GetChildInvocation(invocation);
			}
		}

		public static InvocationExpressionSyntax GetChildInvocation(this InvocationExpressionSyntax invocation) {
			return GetChildExpression(invocation).As<InvocationExpressionSyntax>();
		}

		public static ExpressionSyntax GetChildExpression(this InvocationExpressionSyntax invocation) {
			return invocation
				?.Expression.As<MemberAccessExpressionSyntax>()
				?.Expression;
		}
	}
}
