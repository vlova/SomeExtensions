using System.Diagnostics.Contracts;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;
using SomeExtensions.Transformers;


namespace SomeExtensions.Refactorings.ToLinq.Simplifiers {
	internal abstract class BaseSimplifier : ITransformer<InvocationExpressionSyntax> {
		protected readonly InvocationExpressionSyntax _invocation;

		public BaseSimplifier(InvocationExpressionSyntax invocation) {
			Contract.Requires(invocation != null);
			_invocation = invocation;
		}

		protected InvocationExpressionSyntax ReplaceInvocation(InvocationExpressionSyntax expression, InvocationExpressionSyntax newExpression) {
			var parentExpression = GetParentInvocation(expression);
			var newInvocation = parentExpression?.ReplaceNode(expression, newExpression) ?? newExpression;
			return newInvocation;
		}

		protected InvocationExpressionSyntax GetParentInvocation(InvocationExpressionSyntax expression) {
			return expression == _invocation
				? null
				: expression.FindUp<InvocationExpressionSyntax>(skipThis: true);
		}

		public abstract bool CanTransform(CompilationUnitSyntax root);

		public abstract TransformationResult<InvocationExpressionSyntax> Transform(CompilationUnitSyntax root, CancellationToken token);
	}

}
