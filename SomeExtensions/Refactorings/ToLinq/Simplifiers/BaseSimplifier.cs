using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Transformers;
using static System.Diagnostics.Contracts.Contract;

namespace SomeExtensions.Refactorings.ToLinq.Simplifiers {
	internal abstract class BaseSimplifier : ITransformer<InvocationExpressionSyntax> {
		protected readonly InvocationExpressionSyntax _invocation;

		public BaseSimplifier(InvocationExpressionSyntax invocation) {
			Requires(invocation != null);

			_invocation = invocation;
		}

		protected SyntaxNode ReplaceInInvocation(SyntaxNode expression, SyntaxNode newExpression) {
			if (expression == _invocation) {
				return newExpression;
			}

			return _invocation.ReplaceNode(expression, newExpression);
		}

		public abstract bool CanTransform(CompilationUnitSyntax root);

		public abstract TransformationResult<InvocationExpressionSyntax> Transform(CompilationUnitSyntax root);
	}

}
