using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Refactorings.ToTernaryOperator {
	internal class ToIfElseRefactoring : IRefactoring {
		private readonly StatementSyntax _statement;
		private readonly ConditionalExpressionSyntax _ternary;

		public ToIfElseRefactoring(ConditionalExpressionSyntax ternary, StatementSyntax statement) {
			_ternary = ternary;
			_statement = statement;
		}

		public string Description => "To if/else";

		public SyntaxNode ComputeRoot(SyntaxNode root, CancellationToken token) {
			var trueStatement = _statement.ReplaceNode(_ternary, _ternary.WhenTrue.WithoutTrailingTrivia());
			var falseStatement = _statement.ReplaceNode(_ternary, _ternary.WhenFalse);

			var @if = IfStatement(
				condition: _ternary.Condition,
				statement: trueStatement,
				@else: ElseClause(falseStatement));

			return root.ReplaceNode(_statement, @if.Nicefy());
		}
	}
}