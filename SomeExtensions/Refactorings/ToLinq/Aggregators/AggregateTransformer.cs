using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Refactorings.ToLinq.Aggregators {
	internal class AggregateTransformer : BaseAggregateTransformer {
		public AggregateTransformer(ForEachStatementSyntax @foreach) : base(@foreach) {
		}

		private AssignmentExpressionSyntax _assignment
			=> _lastStatement.As<ExpressionStatementSyntax>()
				?.Expression.As<AssignmentExpressionSyntax>();

		protected override bool CanTransform(string variableName, CompilationUnitSyntax root) {
			if (_assignment == null) return false;

			var isSameVariable = _assignment.Left.IsEquivalentTo(variableName.ToIdentifierName(), false);
			return isSameVariable;
		}

		protected override ExpressionSyntax ToAggregate(ForEachStatementSyntax @foreach) {
			var accumulatorName = _assignment.Left.As<IdentifierNameSyntax>().Identifier;
			var itemName = @foreach.Identifier;

			var parameters = new[] { accumulatorName.WithUserRename(), itemName.WithUserRename() }
				.Select(Parameter)
				.ToSeparatedList()
				.F(ParameterList);

			var lambda = ParenthesizedLambdaExpression(
				parameters,
				body: _assignment.Right);

			var startValue = _local.GetVariable().Initializer.Value;

            return @foreach.Expression
				.AccessTo("Aggregate").ToInvocation(startValue, lambda);
		}
	}
}
