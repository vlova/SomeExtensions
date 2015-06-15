using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using SomeExtensions.Transformers;

namespace SomeExtensions.Refactorings.ToLinq.Simplifiers {
	internal class SelectParenthesizedSimplifier : BaseSimplifier {
		public SelectParenthesizedSimplifier(InvocationExpressionSyntax invocation) : base(invocation) {
		}

		public override bool CanTransform(CompilationUnitSyntax root) {
			return _invocation.GetChildInvocationSequence().Any(IsParenthesizedExpression);
		}

		private bool IsParenthesizedExpression(InvocationExpressionSyntax invocation) {
			var lambda = invocation.GetLinqLambda();
			return lambda?.Body is ParenthesizedExpressionSyntax;
		}

		public override TransformationResult<InvocationExpressionSyntax> Transform(CompilationUnitSyntax root) {
			var lambda = _invocation.GetChildInvocationSequence().First(IsParenthesizedExpression).GetLinqLambda();
			var newLambda = lambda.WithBody(lambda?.Body.As<ParenthesizedExpressionSyntax>().Expression);
			return root.Transform(_invocation, ReplaceInInvocation(lambda, newLambda));
		}
	}
}
