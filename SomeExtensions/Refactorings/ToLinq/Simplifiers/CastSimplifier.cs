using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using SomeExtensions.Transformers;

namespace SomeExtensions.Refactorings.ToLinq.Simplifiers {
	internal class CastSimplifier : BaseSimplifier{
		public CastSimplifier(InvocationExpressionSyntax invocation) : base(invocation) {
		}

		public override bool CanTransform(CompilationUnitSyntax root) {
			return GetTypeCandidate() != null;
		}

		private TypeSyntax GetTypeCandidate() {
			return _invocation.GetChildInvocationSequence()
				.Select(GetCastType)
				.LastOrDefault(t => t != null);
		}

		private TypeSyntax GetCastType(InvocationExpressionSyntax invocation) {
			if (invocation.GetMethodName() != "Select") return null;
			if (invocation.ArgumentList.Arguments.Count != 1) return null;
			var cast = BodyAsCast(invocation.GetLinqLambda());
			return cast?.Type;
		}

		private static CastExpressionSyntax BodyAsCast(SimpleLambdaExpressionSyntax lambda) {
			return lambda?.Body.As<CastExpressionSyntax>();
		}

		public override TransformationResult<InvocationExpressionSyntax> Transform(CompilationUnitSyntax root, CancellationToken token) {
			var type = GetTypeCandidate();
			var expression = GetSelectExpression(type);
			var newExpression = GetCast(type, RemoveCast(expression));
			var newInvocation = ReplaceInvocation(expression, newExpression);

			return root.Transform(_invocation, newInvocation);
		}

		private static InvocationExpressionSyntax GetSelectExpression(TypeSyntax type) {
			return type.FindUp<InvocationExpressionSyntax>(skipThis: true);
		}

		private static ExpressionSyntax RemoveCast(InvocationExpressionSyntax select) {
			var lambda = select.GetLinqLambda();
			var newLambda = lambda.WithBody(BodyAsCast(lambda).Expression);
            return select.ReplaceNodeWithTracking(lambda, newLambda);
		}

		private static InvocationExpressionSyntax GetCast(TypeSyntax type, ExpressionSyntax expression) {
			return expression
				.AccessTo("Cast".MakeGeneric(type))
				.ToInvocation();
		}
	}
}
