using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using SomeExtensions.Transformers;

namespace SomeExtensions.Refactorings.ToLinq.Simplifiers {
	internal class SelectIdentitySimplifier : BaseSimplifier {
		public SelectIdentitySimplifier(InvocationExpressionSyntax invocation) : base(invocation) {
		}

		public override bool CanTransform(CompilationUnitSyntax root) {
			return _invocation.GetChildInvocationSequence().Any(IsIdentityLambda);
		}

		private bool IsIdentityLambda(InvocationExpressionSyntax invocation) {
			var lambda = invocation.GetLinqLambda();
			if (lambda == null) return false;
			var lambdaResult = lambda?.Body.As<IdentifierNameSyntax>()?.Identifier.ValueText;
			var parameterName = lambda.Parameter.Identifier.ValueText;
			return lambdaResult == parameterName;
		}

		public override TransformationResult<InvocationExpressionSyntax> Transform(CompilationUnitSyntax root, CancellationToken token) {
			var expression = _invocation.GetChildInvocationSequence().First(IsIdentityLambda);
			var newInvocation = ReplaceInInvocation(expression, expression.GetChildExpression());

			return root.Transform(_invocation, newInvocation);
		}
	}
}
