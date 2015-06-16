using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Transformers;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Refactorings.ToLinq.Simplifiers {
	internal class SumSimplifier : BaseSimplifier {
		public SumSimplifier(InvocationExpressionSyntax invocation) : base(invocation) {
		}

		public override bool CanTransform(CompilationUnitSyntax root) {
			return _invocation.GetChildInvocationSequence().Any(IsSum);
		}

		public override TransformationResult<InvocationExpressionSyntax> Transform(CompilationUnitSyntax root) {
			var where = _invocation.GetChildInvocationSequence().FirstOrDefault(IsSum);
			var sum = where.GetChildExpression().F(Sum);

			var newInvocation = ReplaceInInvocation(where, sum);

			return root.Transform(_invocation, newInvocation);
		}

		private static InvocationExpressionSyntax Sum(ExpressionSyntax expr) {
			return expr.AccessTo("Sum").ToInvocation();
		}

		private bool IsSum(InvocationExpressionSyntax invocation) {
			if (invocation.GetArgument(0)?.IsEquivalentTo(Argument(0.ToLiteral())) ?? false) return false;
			var lambda = invocation.GetComplexLinqLambda(1);
			if (lambda == null) return false;

			var parameters = lambda.ParameterList.Parameters;
			if (parameters.Count != 2) return false;

			var binaryExpr = lambda.Body.As<BinaryExpressionSyntax>();
			if (binaryExpr == null) return false;

			return binaryExpr.Kind() == AddExpression
				&& IsEquivalent(binaryExpr.Left, parameters.First())
				&& IsEquivalent(binaryExpr.Right, parameters.Second());
		}

		private bool IsEquivalent(ExpressionSyntax expression, ParameterSyntax parameter) {
			var identifier = expression.As<IdentifierNameSyntax>()?.Identifier;
			var paramIdentifier = parameter.Identifier;
			return identifier?.ValueText == paramIdentifier.ValueText;
		}
	}
}
