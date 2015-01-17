using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Extensions {
	public static class InvocationExtensions {
		public static string GetClassName(this InvocationExpressionSyntax invocation) {
			return invocation
				?.Expression
				?.As<MemberAccessExpressionSyntax>()
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
	}
}
