using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using SomeExtensions.Transformers;

namespace SomeExtensions.Refactorings.ToLinq.Simplifiers {
	internal class OfTypeSimplifier : ITransformer<InvocationExpressionSyntax> {
		private readonly InvocationExpressionSyntax _invocation;

		public OfTypeSimplifier(InvocationExpressionSyntax invocation) {
			_invocation = invocation;
		}

		public bool CanTransform(CompilationUnitSyntax root) {
			return GetTypeCandidate() != null;
		}

		private TypeSyntax GetTypeCandidate() {
			return GetInvocationSequence(_invocation)
				.Where(IsWhereNotNull)
				.Select(GetChildInvocation)
				.Select(GetAsCastType)
				.FirstOrDefault(t => t != null);
		}

		private bool IsAsCast(InvocationExpressionSyntax invocation) {
			return GetAsCastType(invocation) != null;
		}

		private TypeSyntax GetAsCastType(InvocationExpressionSyntax invocation) {
			if (invocation.GetMethodName() != "Select") return null;
			if (invocation.ArgumentList.Arguments.Count != 1) return null;

			var lambda = GetLambda(invocation);
			var binaryExpr = lambda?.Body.As<BinaryExpressionSyntax>();

			if (binaryExpr == null) return null;
			if (!binaryExpr.IsKind(SyntaxKind.AsExpression)) return null;

			return binaryExpr.Right as TypeSyntax;
		}

		private bool IsWhereNotNull(InvocationExpressionSyntax invocation) {
			if (invocation.GetMethodName() != "Where") return false;
			if (invocation.ArgumentList.Arguments.Count != 1) return false;

			var lambda = GetLambda(invocation);
			var binaryExpr = lambda?.Body.As<ExpressionSyntax>().FirstNotParenthesized().As<BinaryExpressionSyntax>();

			if (binaryExpr == null) return false;

			if (!binaryExpr.IsKind(SyntaxKind.NotEqualsExpression)) return false;
			if (!binaryExpr.Left.As<IdentifierNameSyntax>()?.Identifier.IsEquivalentTo(lambda.Parameter.Identifier) ?? true) return false;
			if (!binaryExpr.Right.IsEquivalentToNull()) return false;

			return true;
		}

		private static SimpleLambdaExpressionSyntax GetLambda(InvocationExpressionSyntax invocation) {
			return invocation.GetFirstArgument().Expression.As<SimpleLambdaExpressionSyntax>();
		}

		public TransformationResult<InvocationExpressionSyntax> Transform(CompilationUnitSyntax root, CancellationToken token) {
			var type = GetTypeCandidate();
			var expression = GetExpression(type);

			return root.Transform(_invocation, GetOfType(type, expression));
		}

		private static ExpressionSyntax GetExpression(TypeSyntax type) {
			var cast = type.FindUp<InvocationExpressionSyntax>();
			var where = cast.FindUp<InvocationExpressionSyntax>();
			return GetChildExpression(cast);
		}

		private static ExpressionSyntax GetChildExpression(InvocationExpressionSyntax invocation) {
			return invocation
				?.Expression.As<MemberAccessExpressionSyntax>()
				?.Expression;
		}

		private static InvocationExpressionSyntax GetOfType(TypeSyntax type, ExpressionSyntax expression) {
			return expression
				.AccessTo("OfType".MakeGeneric(type))
				.ToInvocation();
		}

		private IEnumerable<InvocationExpressionSyntax> GetInvocationSequence(InvocationExpressionSyntax invocation) {
			while (invocation != null) {
				yield return invocation;
				invocation = GetChildInvocation(invocation);
			}
		}

		private static InvocationExpressionSyntax GetChildInvocation(InvocationExpressionSyntax invocation) {
			return GetChildExpression(invocation).As<InvocationExpressionSyntax>();
		}
	}
}
