using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Refactorings.ToLinq.Aggregators {
	internal class ToListTransformer : BaseAggregateTransformer {
		public ToListTransformer(ForEachStatementSyntax @foreach) : base(@foreach) {
		}

		protected InvocationExpressionSyntax _invocation
			=> _lastStatement
					.As<ExpressionStatementSyntax>()?.Expression
					.As<InvocationExpressionSyntax>();

		protected override bool CanTransform(string collectionName, CompilationUnitSyntax root) {
			return _invocation.GetClassName() == collectionName
				&& _invocation.GetMethodName() == "Add"
				&& _invocation.ArgumentList.Arguments.Count == 1;
		}

		protected override ExpressionSyntax ToAggregate(ForEachStatementSyntax @foreach) {
			var lambda = SimpleLambdaExpression(
				Parameter(@foreach.Identifier),
				body: _invocation.ArgumentList.Arguments.Single().Expression);

			return @foreach.Expression
				.AccessTo("Select").ToInvocation(lambda)
				.AccessTo("ToList").ToInvocation();
		}
	}
}
