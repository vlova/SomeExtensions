using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using SomeExtensions.Transformers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Refactorings.ToLinq.Transformers {
	internal class IfNoElseToWhereTransformer : ITransformer<ForEachStatementSyntax> {
		private readonly ForEachStatementSyntax _foreach;

		public IfNoElseToWhereTransformer(ForEachStatementSyntax @foreach) {
			_foreach = @foreach;
		}

		private BlockSyntax _block => _foreach?.Statement as BlockSyntax;
		private IfStatementSyntax _if => _block?.Statements.FirstOrDefault() as IfStatementSyntax;

		public bool CanTransform(CompilationUnitSyntax root) {
			return IsContinueByConditionStatement();
		}

		private bool IsContinueByConditionStatement() {
			return (_block != null)
				&& (_if != null)
				&& (_if.Else == null || _if.Else.Statement is ContinueStatementSyntax);
		}

		public TransformationResult<ForEachStatementSyntax> Transform(CompilationUnitSyntax root, CancellationToken token) {
			var newForeach = _foreach
				.F(RemoveContinue)
				.F(AddTakeWhile)
				.Nicefy();

			return root.Transform(_foreach, newForeach);
		}

		private ForEachStatementSyntax RemoveContinue(ForEachStatementSyntax @foreach) {
			return @foreach.ReplaceNode(_block, _if.Statement);
		}

		private ForEachStatementSyntax AddTakeWhile(ForEachStatementSyntax @foreach) {
			var lambda = SimpleLambdaExpression(
				Parameter(@foreach.Identifier),
				body: ParenthesizedExpression(_if.Condition));

			var newExpression = @foreach.Expression
				.AccessTo("Where")
				.ToInvocation(lambda);

			return @foreach.WithExpression(newExpression);
		}
	}
}
