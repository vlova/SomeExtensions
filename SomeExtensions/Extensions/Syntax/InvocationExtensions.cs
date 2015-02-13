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
			return invocation
				?.Expression
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
	}
}
