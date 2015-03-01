using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Transformers;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using Microsoft.CodeAnalysis;

namespace SomeExtensions.Refactorings.ToLinq.Simplifiers {
	internal class IntersectSimplifier : BaseSimplifier {
		public IntersectSimplifier(InvocationExpressionSyntax invocation) : base(invocation) {
		}

		public override bool CanTransform(CompilationUnitSyntax root) {
			return _invocation.GetChildInvocationSequence().Any(IsCollectionIntersect);
		}

		public override TransformationResult<InvocationExpressionSyntax> Transform(CompilationUnitSyntax root, CancellationToken token) {
			var where = _invocation.GetChildInvocationSequence().FirstOrDefault(IsCollectionIntersect);
			var child = where.GetChildExpression();
			var except = WithIntersect(child, IntersectCollectionName(where));

			var newInvocation = ReplaceInInvocation(where, except);

			return root.Transform(_invocation, newInvocation);
		}

		private static InvocationExpressionSyntax WithIntersect(ExpressionSyntax expr, string collectionName) {
			return expr
				.AccessTo("Intersect")
				.ToInvocation(collectionName.ToIdentifierName());
		}

		private bool IsCollectionIntersect(InvocationExpressionSyntax arg) {
			return IntersectCollectionName(arg) != null;
		}

		private string IntersectCollectionName(InvocationExpressionSyntax invocation) {
			var lambda = invocation.GetLinqLambda();

			var linqMethod = lambda?.Body.As<InvocationExpressionSyntax>();
			if (linqMethod.GetMethodName() != "Contains") return null;
			return linqMethod.GetClassName();
		}
	}
}
