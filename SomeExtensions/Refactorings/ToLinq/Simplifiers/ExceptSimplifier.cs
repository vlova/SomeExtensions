using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Transformers;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace SomeExtensions.Refactorings.ToLinq.Simplifiers {
	internal class ExceptSimplifier : BaseSimplifier {
		public ExceptSimplifier(InvocationExpressionSyntax invocation) : base(invocation) {
		}

		public override bool CanTransform(CompilationUnitSyntax root) {
			return _invocation.GetChildInvocationSequence().Any(IsCollectionExcept);
		}

		public override TransformationResult<InvocationExpressionSyntax> Transform(CompilationUnitSyntax root, CancellationToken token) {
			var where = _invocation.GetChildInvocationSequence().FirstOrDefault(IsCollectionExcept);
			var child = where.GetChildExpression();
			var except = WithExcept(child, ExceptCollectionName(where));

			var newInvocation = ReplaceInInvocation(where, except);

			return root.Transform(_invocation, newInvocation);
		}

		private static InvocationExpressionSyntax WithExcept(ExpressionSyntax expr, string collectionName) {
			return expr
				.AccessTo("Except")
				.ToInvocation(collectionName.ToIdentifierName());
		}

		private bool IsCollectionExcept(InvocationExpressionSyntax arg) {
			return ExceptCollectionName(arg) != null;
		}

		private string ExceptCollectionName(InvocationExpressionSyntax invocation) {
			var lambda = invocation.GetLinqLambda();
			var logicalNot = lambda?.Body.As<PrefixUnaryExpressionSyntax>();
			if (logicalNot == null || !logicalNot.IsKind(LogicalNotExpression)) return null;

			var linqMethod = logicalNot.Operand.As<InvocationExpressionSyntax>();
			if (linqMethod.GetMethodName() != "Contains") return null;
			return linqMethod.GetClassName();
		}
	}
}
