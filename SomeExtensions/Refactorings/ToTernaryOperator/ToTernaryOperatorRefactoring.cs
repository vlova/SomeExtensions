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
		private IEnumerable<NodeDiff<ExpressionSyntax>> _diffNodes;
		private IfStatementSyntax _if;

		public ToTernaryOperatorRefactoring(IfStatementSyntax @if, IEnumerable<NodeDiff<ExpressionSyntax>> diffNodes) {
			_if = @if;
			_diffNodes = diffNodes;
		}

		public string Description => "To ternary operator";

		private SyntaxNode Parent => _if.Parent;

		public SyntaxNode ComputeRoot(SyntaxNode root, CancellationToken token) {
			var statements = GetStatementsToReplace()
				.Select(statement => _diffNodes.Aggregate(statement, (s, n) => s.ReplaceNode(n.First, ComputeTernaryOperator(n))).Nicefy());

			var newParent = Parent
				.InsertNodesAfter(_if, statements)
				.RemoveNodeAt(Parent.IndexOf(_if));

			return root.ReplaceNode(Parent, newParent);
		}

		private List<StatementSyntax> GetStatementsToReplace() {
			var statements = new List<StatementSyntax>();

			if (_if.Statement is BlockSyntax) {
				statements.AddRange(_if.Statement.As<BlockSyntax>().Statements);
			}
			else {
				statements.Add(_if.Statement);
			}

			return statements;
		}

		private SyntaxNode ComputeTernaryOperator(NodeDiff<ExpressionSyntax> diffNode) {
			var condition = _if.Condition;
            if (!(condition is ParenthesizedExpressionSyntax)) {
				condition = ParenthesizedExpression(condition);
			}

			return ParenthesizedExpression(ConditionalExpression(condition, diffNode.First, diffNode.Second));
		}
	}
}
