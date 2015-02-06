using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Refactorings.ToTernaryOperator {
	internal class ToTernaryOperatorRefactoring : IRefactoring {
		private IEnumerable<NodeDiff<ExpressionSyntax>> diffNodes;
		private IfStatementSyntax @if;

		public ToTernaryOperatorRefactoring(IfStatementSyntax @if, IEnumerable<NodeDiff<ExpressionSyntax>> diffNodes) {
			this.@if = @if;
			this.diffNodes = diffNodes;
		}

		public string Description => "To ternary operator";

		private SyntaxNode Parent => @if.Parent;

		public SyntaxNode ComputeRoot(SyntaxNode root, CancellationToken token) {
			var statements = GetStatementsToReplace()
				.Select(statement => diffNodes.Aggregate(statement, (s, n) => s.ReplaceNode(n.First, ComputeTernaryOperator(n))).Nicefy());

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

		private SyntaxNode ComputeTernaryOperator(NodeDiff<ExpressionSyntax> diffNode) {
			var condition = @if.Condition;
            if (!(condition is ParenthesizedExpressionSyntax)) {
				condition = ParenthesizedExpression(condition);
			}

			return ParenthesizedExpression(ConditionalExpression(condition, diffNode.First, diffNode.Second));
		}
	}
}
