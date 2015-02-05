using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using static SomeExtensions.Extensions.SyntaxDiff;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Refactorings.ToTernaryOperator {
	internal class ToTernaryOperatorRefactoring : IRefactoring {
		private NodeDiff<ExpressionSyntax> diffNode;
		private IfStatementSyntax @if;

		public ToTernaryOperatorRefactoring(IfStatementSyntax @if, NodeDiff<ExpressionSyntax> diffNode) {
			this.@if = @if;
			this.diffNode = diffNode;
		}

		public string Description => "To ternary operator";

		private SyntaxNode Parent => @if.Parent;

		public SyntaxNode ComputeRoot(SyntaxNode root, CancellationToken token) {
			var statements = GetStatementsToReplace()
				.Select(s => s.ReplaceNode(diffNode.First, ComputeTernaryOperator()).Nicefy());

			var newParent = Parent
				.InsertNodesAfter(@if, statements)
				.RemoveNodeAt(Parent.IndexOf(@if));

			return root.ReplaceNode(Parent, newParent);
		}

		private List<StatementSyntax> GetStatementsToReplace() {
			var statements = new List<StatementSyntax>();

			if (@if.Statement is BlockSyntax) {
				statements.AddRange(@if.Statement.As<BlockSyntax>().Statements);
			}
			else {
				statements.Add(@if.Statement);
			}

			return statements;
		}

		private ConditionalExpressionSyntax ComputeTernaryOperator() {
			var condition = @if.Condition;
            if (!(condition is ParenthesizedExpressionSyntax)) {
				condition = ParenthesizedExpression(condition);
			}

			return ConditionalExpression(condition, diffNode.First, diffNode.Second);
		}
	}
}
