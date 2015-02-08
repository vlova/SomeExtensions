using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Diagnostics.Contracts;

namespace SomeExtensions.Refactorings.ToTernaryOperator {
	internal class ToIfElseRefactoring : IRefactoring {
		private readonly StatementSyntax _statement;
		private readonly ConditionalExpressionSyntax _ternary;

		public ToIfElseRefactoring(ConditionalExpressionSyntax ternary, StatementSyntax statement) {
			Contract.Requires(ternary != null);
			Contract.Requires(statement != null);
			_ternary = ternary;
			_statement = statement;
		}

		public string Description => "To if/else";

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token) {
			var trueStatement = _statement.ReplaceNode(_ternary, _ternary.WhenTrue.WithoutTrailingTrivia());
			var falseStatement = _statement.ReplaceNode(_ternary, _ternary.WhenFalse.WithoutTrailingTrivia());

			var @if = IfStatement(
				condition: _ternary.Condition.WithoutTrailingTrivia(),
				statement: trueStatement,
				@else: ElseClause(falseStatement));

			return root.ReplaceNode(_statement, @if.Nicefy());
		}
	}
}