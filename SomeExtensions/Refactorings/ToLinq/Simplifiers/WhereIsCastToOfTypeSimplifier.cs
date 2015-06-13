using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using SomeExtensions.Transformers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Refactorings.ToLinq.Simplifiers {
	/// <summary>
	/// Transforms nodes.Where(n => n is T).Select(n => n as T)
	/// Into nodes.OfType<T>
	/// </summary>
	internal class WhereIsCastToOfTypeSimplifier : BaseSimplifier {
		public WhereIsCastToOfTypeSimplifier(InvocationExpressionSyntax invocation) : base(invocation) {
		}

		public override bool CanTransform(CompilationUnitSyntax root) {
			return GetTypeCandidate() != null;
		}

		private TypeSyntax GetTypeCandidate() {
			return _invocation.GetChildInvocationSequence()
				.GetPatternMatches(IsSelectAsT, IsInstanceOfType)
				.Where(pair => AreEquivalent(GetInstanceOfType(pair.Last()), GetCast(pair.First()), topLevel: false))
				.SelectFirst(GetInstanceOfType)
				.FirstOrDefault();
		}

		private bool IsInstanceOfType(InvocationExpressionSyntax invocation) {
			return GetInstanceOfType(invocation) != null;
		}

		private TypeSyntax GetInstanceOfType(InvocationExpressionSyntax invocation) {
			if (invocation.GetMethodName() != "Where") return null;
			if (invocation.ArgumentList.Arguments.Count != 1) return null;

			var lambda = invocation.GetLinqLambda();
			var binaryExpr = lambda?.Body.As<BinaryExpressionSyntax>();

			if (binaryExpr == null) return null;
			if (!binaryExpr.IsKind(SyntaxKind.IsExpression)) return null;

			return binaryExpr.Right as TypeSyntax;
		}

		private bool IsSelectAsT(InvocationExpressionSyntax invocation) {
			return GetCast(invocation) != null;
		}

		private static TypeSyntax GetCast(InvocationExpressionSyntax invocation) {
			if (invocation.GetMethodName() != "Select") return null;
			if (invocation.ArgumentList.Arguments.Count != 1) return null;

			var lambda = invocation.GetLinqLambda(); ;

			return GetAsCastType(lambda) ?? GetCastType(lambda);
		}

		private static TypeSyntax GetCastType(SimpleLambdaExpressionSyntax lambda) {
			return lambda?.Body?.As<CastExpressionSyntax>()?.Type;
		}

		private static TypeSyntax GetAsCastType(SimpleLambdaExpressionSyntax lambda) {
			var binaryExpr = lambda?.Body.As<ExpressionSyntax>().FirstNotParenthesized().As<BinaryExpressionSyntax>();
			if (binaryExpr == null) return null;

			if (!binaryExpr.IsKind(SyntaxKind.AsExpression)) return null;
			if (!binaryExpr.Left.As<IdentifierNameSyntax>()?.Identifier.IsEquivalentTo(lambda.Parameter.Identifier) ?? true) return null;

			return binaryExpr.Right as TypeSyntax;
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
