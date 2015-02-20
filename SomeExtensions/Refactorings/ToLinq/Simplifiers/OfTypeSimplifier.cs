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
	internal class OfTypeSimplifier : BaseSimplifier {
		public OfTypeSimplifier(InvocationExpressionSyntax invocation) : base(invocation) {
		}

		public override bool CanTransform(CompilationUnitSyntax root) {
			return GetTypeCandidate() != null;
		}

		private TypeSyntax GetTypeCandidate() {
			return _invocation.GetChildInvocationSequence()
				.GetPatternMatches(IsWhereNotNull, IsAsCast)
				.SelectLast(GetAsCastType)
				.FirstOrDefault();
		}

		private bool IsAsCast(InvocationExpressionSyntax invocation) {
			return GetAsCastType(invocation) != null;
		}

		private TypeSyntax GetAsCastType(InvocationExpressionSyntax invocation) {
			if (invocation.GetMethodName() != "Select") return null;
			if (invocation.ArgumentList.Arguments.Count != 1) return null;

			var lambda = invocation.GetLinqLambda();
			var binaryExpr = lambda?.Body.As<BinaryExpressionSyntax>();

			if (binaryExpr == null) return null;
			if (!binaryExpr.IsKind(SyntaxKind.AsExpression)) return null;

			return binaryExpr.Right as TypeSyntax;
		}

		private bool IsWhereNotNull(InvocationExpressionSyntax invocation) {
			if (invocation.GetMethodName() != "Where") return false;
			if (invocation.ArgumentList.Arguments.Count != 1) return false;

			var lambda = invocation.GetLinqLambda();
			var binaryExpr = lambda?.Body.As<ExpressionSyntax>().FirstNotParenthesized().As<BinaryExpressionSyntax>();

			if (binaryExpr == null) return false;

			if (!binaryExpr.IsKind(SyntaxKind.NotEqualsExpression)) return false;
			if (!binaryExpr.Left.As<IdentifierNameSyntax>()?.Identifier.IsEquivalentTo(lambda.Parameter.Identifier) ?? true) return false;
			if (!binaryExpr.Right.IsEquivalentToNull()) return false;

			return true;
		}

		public override TransformationResult<InvocationExpressionSyntax> Transform(CompilationUnitSyntax root, CancellationToken token) {
			var type = GetTypeCandidate();
			var where = type.GetParents().OfType<InvocationExpressionSyntax>().ElementAt(1);
			var newInvocation = ReplaceInInvocation(where, GetOfType(type));
			return root.Transform(_invocation, newInvocation);
		}

		private static ExpressionSyntax GetExpression(TypeSyntax type) {
			var cast = type.FindUp<InvocationExpressionSyntax>();
			return cast.GetChildExpression();
		}

		private static InvocationExpressionSyntax GetOfType(TypeSyntax type) {
			var expression = GetExpression(type);

            return expression
				.AccessTo("OfType".MakeGeneric(type))
				.ToInvocation();
		}
	}
}
