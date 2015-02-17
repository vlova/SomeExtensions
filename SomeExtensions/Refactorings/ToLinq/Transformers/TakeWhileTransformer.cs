using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Refactorings.ToLinq.Transformers {
	internal class TakeWhileTransformer : ILinqTransformer {
		private readonly ForEachStatementSyntax _foreach;

		public TakeWhileTransformer(ForEachStatementSyntax @foreach) {
			_foreach = @foreach;
		}

		private BlockSyntax _block => _foreach?.Statement as BlockSyntax;
		private IfStatementSyntax _if => _block?.Statements.FirstOrDefault() as IfStatementSyntax;

		public bool CanTransform(CompilationUnitSyntax root) {
			return IsBreakByConditionStatement();
		}

		private bool IsBreakByConditionStatement() {
			return (_block != null)
				&& (_if != null)
				&& (_if.Else == null)
				&& (_if.Statement is BreakStatementSyntax);
		}

		public Tuple<CompilationUnitSyntax, ForEachStatementSyntax> Transform(CompilationUnitSyntax root, CancellationToken token) {
			var newForeach = _foreach
				.Fluent(f => RemoveBreak(f))
				.Fluent(f => AddTakeWhile(f))
				.Nicefy();

            var newRoot = root.ReplaceNodeWithTracking(_foreach, newForeach);

			return Tuple.Create(newRoot, newRoot.GetCurrentNode(newForeach));
		}

		private ForEachStatementSyntax RemoveBreak(ForEachStatementSyntax @foreach) {
			return @foreach.ReplaceNode(_block, _block.WithStatements(_block.Statements.Skip(1).ToSyntaxList()));
        }

		private ForEachStatementSyntax AddTakeWhile(ForEachStatementSyntax @foreach) {
			var lambda = SimpleLambdaExpression(
				Parameter(@foreach.Identifier),
				body: _if.Condition.ToLogicalNot(simplify: true));

			var newExpression = @foreach.Expression
				.AccessTo("TakeWhile")
				.ToInvocation(lambda);

			return @foreach.WithExpression(newExpression);
		}
	}
}
