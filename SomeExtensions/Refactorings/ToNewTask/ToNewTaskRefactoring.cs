using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace SomeExtensions.Refactorings.ToNewTask {
	internal class ToNewTaskRefactoring : IRefactoring {
		private static Regex taskRegex = new Regex(@"task(\d+)", RegexOptions.Compiled);

		private readonly BlockSyntax _block;
		private readonly ExpressionSyntax _expr;
		private readonly int _index;

		public ToNewTaskRefactoring(BlockSyntax block, ExpressionSyntax expr, int index) {
			_block = block;
			_expr = expr;
			_index = index;
		}

		private StatementSyntax _statement =>
			_expr.GetThisAndParents()
				.OfType<StatementSyntax>()
				.First(n => n.Parent == _block);

		public string Description
			=> _index == 0
				? "To new task"
				: $"To new task #{_index + 1}";

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root) {
			var statementIndex = _block.Statements.IndexOf(_statement);
			var newBlock = _block.ReplaceNode(_statement, GetAwaitedStatement());
			newBlock = newBlock.WithStatements(
				newBlock.Statements.Insert(statementIndex, GetNewTaskVariable()));
			return root.ReplaceNode(_block, newBlock);
		}

		private StatementSyntax GetNewTaskVariable() {
			var varType = "var".ToIdentifierName();

			var taskLambda = _expr.F(ParenthesizedLambdaExpression);
			if (_expr.DescendantNodes<AwaitExpressionSyntax>().Any()) {
				taskLambda = taskLambda.WithAsyncKeyword(AsyncKeyword.ToToken());
			}

			var newTaskExpr = "Task".AccessTo("Run").ToInvocation(taskLambda);

			return GetTaskVariableName()
				.ToVariableDeclaration(varType, newTaskExpr)
				.ToLocalDeclaration();
		}

		private StatementSyntax GetAwaitedStatement() {
			var awaitedExpr = GetTaskVariableName().ToIdentifierName().WithUserRename()
				.F(AwaitExpression)
				.F(ParenthesizedExpression)
				.Nicefy();

			return _statement
				.ReplaceNode(_expr, awaitedExpr)
				.Nicefy();
		}

		private string GetTaskVariableName() {
			var variables = _block.GetThisAndParents()
				.OfType<BlockSyntax>()
				.SelectMany(b => b.Statements)
				.OfType<LocalDeclarationStatementSyntax>()
				.SelectMany(l => l.Declaration.Variables)
				.Select(l => l.Identifier.Text);

			var taskIndexes = variables
				.Select(v => taskRegex.Match(v))
				.WhereNot(v => v == null)
				.Select(v => v.Groups[1].Value)
				.Select(long.Parse);

			var newTaskIndex = taskIndexes
				.OrderByDescending(i => i)
				.Select(i => i + 1)
				.FirstOrDefault();

			return $"task{newTaskIndex}";
		}
	}
}